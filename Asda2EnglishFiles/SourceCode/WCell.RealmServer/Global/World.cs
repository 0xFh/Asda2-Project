using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.Threading;
using WCell.Util.Variables;

namespace WCell.RealmServer.Global
{
    /// <summary>
    /// Manages the areas and maps that make up the game world, and tracks all entities in all maps.
    /// </summary>
    public class World : IWorldSpace
    {
        /// <summary>Only used for implementing interfaces</summary>
        public static readonly WCell.RealmServer.Global.World Instance = new WCell.RealmServer.Global.World();

        private static readonly ReaderWriterLockWrapper worldLock = new ReaderWriterLockWrapper();

        /// <summary>
        /// While pausing, resuming and saving, the World locks against this Lock,
        /// so resuming cannot start before all Contexts have been paused
        /// </summary>
        public static readonly object PauseLock = new object();

        /// <summary>
        /// Global PauseObject that all Contexts wait for when pausing
        /// </summary>
        public static readonly object PauseObject = new object();

        private static readonly Dictionary<string, INamedEntity> s_entitiesByName =
            new Dictionary<string, INamedEntity>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);

        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<uint, INamedEntity> s_namedEntities = new Dictionary<uint, INamedEntity>();
        internal static MapTemplate[] s_MapTemplates = new MapTemplate[727];
        internal static Map[] s_Maps = new Map[727];
        internal static ZoneTemplate[] s_ZoneTemplates = new ZoneTemplate[5023];
        internal static WorldMapOverlayEntry[] s_WorldMapOverlayEntries = new WorldMapOverlayEntry[1642];

        private static readonly SelfRunningTaskQueue _taskQueue =
            new SelfRunningTaskQueue(100, "World Task Queue", false);

        public static Dictionary<ushort, Character> CharactersBySessId = new Dictionary<ushort, Character>();
        public static Dictionary<uint, Character> CharactersByAccId = new Dictionary<uint, Character>();
        private static bool _paused;
        private static int _pauseThreadId;
        private static bool _saving;
        private static int s_characterCount;
        private static int s_hordePlayerCount;
        private static int s_allyPlayerCount;
        private static int s_staffMemberCount;
        private static List<ushort> _charactersSessIdsPool;

        public static int TaskQueueUpdateInterval
        {
            get { return WCell.RealmServer.Global.World._taskQueue.UpdateInterval; }
            set { WCell.RealmServer.Global.World._taskQueue.UpdateInterval = value; }
        }

        /// <summary>Gets the collection of maps.</summary>
        public static MapTemplate[] MapTemplates
        {
            get { return WCell.RealmServer.Global.World.s_MapTemplates; }
        }

        /// <summary>Gets the collection of zones.</summary>
        public static ZoneTemplate[] ZoneTemplates
        {
            get { return WCell.RealmServer.Global.World.s_ZoneTemplates; }
        }

        /// <summary>The number of characters in the game.</summary>
        public static int CharacterCount
        {
            get { return WCell.RealmServer.Global.World.s_characterCount; }
        }

        /// <summary>
        /// The number of online characters that belong to the Horde
        /// </summary>
        public static int HordeCharCount
        {
            get { return WCell.RealmServer.Global.World.s_hordePlayerCount; }
        }

        /// <summary>
        /// The number of online characters that belong to the Alliance
        /// </summary>
        public static int AllianceCharCount
        {
            get { return WCell.RealmServer.Global.World.s_allyPlayerCount; }
        }

        /// <summary>The number of online staff characters</summary>
        public static int StaffMemberCount
        {
            get { return WCell.RealmServer.Global.World.s_staffMemberCount; }
            internal set { WCell.RealmServer.Global.World.s_staffMemberCount = value; }
        }

        /// <summary>Task queue for global tasks and timers</summary>
        public static SelfRunningTaskQueue TaskQueue
        {
            get { return WCell.RealmServer.Global.World._taskQueue; }
        }

        public static int PauseThreadId
        {
            get { return WCell.RealmServer.Global.World._pauseThreadId; }
        }

        public static bool IsInPauseContext
        {
            get { return Thread.CurrentThread.ManagedThreadId == WCell.RealmServer.Global.World._pauseThreadId; }
        }

        /// <summary>
        /// Pauses the World, executes the given Action, unpauses the world again and blocks while doing so
        /// </summary>
        public static void ExecuteWhilePaused(Action onPause)
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessageAndWait(true, (Action) (() =>
            {
                WCell.RealmServer.Global.World.Paused = true;
                onPause();
                WCell.RealmServer.Global.World.Paused = false;
            }));
        }

        [NotVariable]
        internal static bool Paused
        {
            get { return WCell.RealmServer.Global.World._paused; }
            set
            {
                if (WCell.RealmServer.Global.World._paused == value)
                    return;
                lock (WCell.RealmServer.Global.World.PauseLock)
                {
                    if (WCell.RealmServer.Global.World._paused == value)
                        return;
                    lock (WCell.RealmServer.Global.World.PauseObject)
                        WCell.RealmServer.Global.World._paused = value;
                    if (!value)
                    {
                        lock (WCell.RealmServer.Global.World.PauseObject)
                            Monitor.PulseAll(WCell.RealmServer.Global.World.PauseObject);
                    }
                    else
                    {
                        WCell.RealmServer.Global.World._pauseThreadId = Thread.CurrentThread.ManagedThreadId;
                        IEnumerable<Map> source = WCell.RealmServer.Global.World.GetAllMaps().Where<Map>(
                            (Func<Map, bool>) (map =>
                            {
                                if (map != null)
                                    return map.IsRunning;
                                return false;
                            }));
                        int pauseCount = source.Count<Map>();
                        foreach (Map map in source)
                        {
                            if (map.IsInContext)
                            {
                                if (!WCell.RealmServer.Global.World._saving &&
                                    !ServerApp<WCell.RealmServer.RealmServer>.IsShuttingDown)
                                {
                                    lock (WCell.RealmServer.Global.World.PauseObject)
                                        Monitor.PulseAll(WCell.RealmServer.Global.World.PauseObject);
                                    throw new InvalidOperationException(
                                        "Cannot pause World from within a Map's context - Use the Pause() method instead.");
                                }

                                --pauseCount;
                            }
                            else if (map.IsRunning)
                                map.AddMessage((IMessage) new Message((Action) (() =>
                                {
                                    --pauseCount;
                                    lock (WCell.RealmServer.Global.World.PauseObject)
                                        Monitor.Wait(WCell.RealmServer.Global.World.PauseObject);
                                })));
                        }

                        while (pauseCount > 0)
                            Thread.Sleep(50);
                    }

                    Action<bool> worldPaused = WCell.RealmServer.Global.World.WorldPaused;
                    if (worldPaused != null)
                        worldPaused(value);
                    WCell.RealmServer.Global.World._pauseThreadId = 0;
                }
            }
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Third, "Initializing World")]
        public static void InitializeWorld()
        {
            if (WCell.RealmServer.Global.World.s_MapTemplates[726] != null)
                return;
            WCell.RealmServer.Global.World.LoadMapData();
            WCell.RealmServer.Global.World.LoadZoneInfos();
            TerrainMgr.InitTerrain();
            WCell.RealmServer.Global.World._taskQueue.IsRunning = true;
        }

        /// <summary>Indicates whether the world is currently saving.</summary>
        public static bool IsSaving
        {
            get { return WCell.RealmServer.Global.World._saving; }
        }

        public static void Save()
        {
            WCell.RealmServer.Global.World.Save(false);
        }

        /// <summary>
        /// Blocks until all pending changes to dynamic Data have been saved.
        /// </summary>
        /// <param name="beforeShutdown">Whether the server is about to shutdown.</param>
        public static void Save(bool beforeShutdown)
        {
            bool flag = !WCell.RealmServer.Global.World._saving;
            lock (WCell.RealmServer.Global.World.PauseLock)
            {
                if (!flag)
                    return;
                WCell.RealmServer.Global.World._saving = true;
                foreach (Map allMap in WCell.RealmServer.Global.World.GetAllMaps())
                    allMap.Save();
                WCell.RealmServer.Global.World.Broadcast("Saving guilds...");
                GuildMgr.OnShutdown();
                WCell.RealmServer.Global.World.Broadcast("Saving auction...");
                Asda2AuctionMgr.OnShutdown();
                WCell.RealmServer.Global.World.Broadcast("Saving characters...");
                List<Character> chars = WCell.RealmServer.Global.World.GetAllCharacters();
                int saveCount = chars.Count;
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.ExecuteInContext((Action) (() =>
                {
                    for (int index = 0; index < chars.Count; ++index)
                    {
                        Character character = chars[index];
                        WCell.RealmServer.Global.World.Broadcast(string.Format("Saving character {0}. [{1}/{2}]",
                            (object) character.Name, (object) index, (object) saveCount));
                        if (character.IsInWorld)
                        {
                            if (beforeShutdown)
                                character.Record.LastLogout = new DateTime?(DateTime.Now);
                            character.SaveNow();
                        }

                        if (beforeShutdown)
                        {
                            character.Record.CanSave = false;
                            character.Client.Disconnect(false);
                        }

                        --saveCount;
                    }

                    saveCount = 0;
                }));
                while (saveCount > 0)
                    Thread.Sleep(50);
                WCell.RealmServer.Global.World._saving = false;
                if (WCell.RealmServer.Global.World.Saved == null)
                    return;
                WCell.RealmServer.Global.World.Saved();
            }
        }

        private static void LoadMapData()
        {
            WCell.RealmServer.Global.World.Instance.WorldStates = new WorldStateCollection(
                (IWorldSpace) WCell.RealmServer.Global.World.Instance, WCell.Constants.World.WorldStates.GlobalStates);
            WCell.RealmServer.Global.World.SetupCustomMaps();
            WCell.RealmServer.Global.World.SetupBoundaries();
        }

        private static void SetupCustomMaps()
        {
            MapTemplate mapTemplate1 = new MapTemplate()
            {
                Name = "Silaris",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.Silaris,
                Id = MapId.Silaris,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[0] = mapTemplate1;
            MapTemplate mapTemplate2 = new MapTemplate()
            {
                Name = "RainRiver",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.RainRiver,
                Id = MapId.RainRiver,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[1] = mapTemplate2;
            MapTemplate mapTemplate3 = new MapTemplate()
            {
                Name = "ConquestLand",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.ConquestLand,
                Id = MapId.ConquestLand,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[2] = mapTemplate3;
            MapTemplate mapTemplate4 = new MapTemplate()
            {
                Name = "Alpia",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.Alpia,
                Id = MapId.Alpia,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[3] = mapTemplate4;
            MapTemplate mapTemplate5 = new MapTemplate()
            {
                Name = "NightValey",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.NightValey,
                Id = MapId.NightValey,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[4] = mapTemplate5;
            MapTemplate mapTemplate6 = new MapTemplate()
            {
                Name = "Aquaton",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.Aquaton,
                Id = MapId.Aquaton,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[5] = mapTemplate6;
            MapTemplate mapTemplate7 = new MapTemplate()
            {
                Name = "SunnyCoast",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.SunnyCoast,
                Id = MapId.SunnyCoast,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[6] = mapTemplate7;
            MapTemplate mapTemplate8 = new MapTemplate()
            {
                Name = "Flamio",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.Flamio,
                Id = MapId.Flamio,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[7] = mapTemplate8;
            MapTemplate mapTemplate9 = new MapTemplate()
            {
                Name = "QueenPalace",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.QueenPalace,
                Id = MapId.QueenPalace,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[8] = mapTemplate9;
            MapTemplate mapTemplate10 = new MapTemplate()
            {
                Name = "CastleOfChess",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.CastleOfChess,
                Id = MapId.CastleOfChess,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[9] = mapTemplate10;
            MapTemplate mapTemplate11 = new MapTemplate()
            {
                Name = "FlamioPlains",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.FlamioPlains,
                Id = MapId.FlamioPlains,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[10] = mapTemplate11;
            MapTemplate mapTemplate12 = new MapTemplate()
            {
                Name = "NeptillanNode",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.NeptillanNode,
                Id = MapId.NeptillanNode,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[11] = mapTemplate12;
            MapTemplate mapTemplate13 = new MapTemplate()
            {
                Name = "FlamionMoutain",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.FlamionMoutain,
                Id = MapId.FlamionMoutain,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[12] = mapTemplate13;
            MapTemplate mapTemplate14 = new MapTemplate()
            {
                Name = "Flabis",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.Flabis,
                Id = MapId.Flabis,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[13] = mapTemplate14;
            MapTemplate mapTemplate15 = new MapTemplate()
            {
                Name = "StagnantDesert",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.StagnantDesert,
                Id = MapId.StagnantDesert,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[14] = mapTemplate15;
            MapTemplate mapTemplate16 = new MapTemplate()
            {
                Name = "DragonLair",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.DragonLair,
                Id = MapId.DragonLair,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[15] = mapTemplate16;
            MapTemplate mapTemplate17 = new MapTemplate()
            {
                Name = "FirewayForest",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.FirewayForest,
                Id = MapId.FirewayForest,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[16] = mapTemplate17;
            MapTemplate mapTemplate18 = new MapTemplate()
            {
                Name = "CastleOfChaos",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.CastleOfChaos,
                Id = MapId.CastleOfChaos,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[17] = mapTemplate18;
            MapTemplate mapTemplate19 = new MapTemplate()
            {
                Name = "Inferion",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.Inferion,
                Id = MapId.Inferion,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[18] = mapTemplate19;
            MapTemplate mapTemplate20 = new MapTemplate()
            {
                Name = "BatleField",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.BatleField,
                Id = MapId.BatleField,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = true
            };
            WCell.RealmServer.Global.World.s_MapTemplates[19] = mapTemplate20;
            MapTemplate mapTemplate21 = new MapTemplate()
            {
                Name = "DecaronLab",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.DecaronLab,
                Id = MapId.DecaronLab,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[20] = mapTemplate21;
            MapTemplate mapTemplate22 = new MapTemplate()
            {
                Name = "FieldOfHonnor",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.FieldOfHonnor,
                Id = MapId.FieldOfHonnor,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[21] = mapTemplate22;
            MapTemplate mapTemplate23 = new MapTemplate()
            {
                Name = "OX",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.OX,
                Id = MapId.OX,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[22] = mapTemplate23;
            MapTemplate mapTemplate24 = new MapTemplate()
            {
                Name = "IceQuarry",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.IceQuarry,
                Id = MapId.IceQuarry,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[23] = mapTemplate24;
            MapTemplate mapTemplate25 = new MapTemplate()
            {
                Name = "NeverFall",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.NeverFall,
                Id = MapId.NeverFall,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[27] = mapTemplate25;
            MapTemplate mapTemplate26 = new MapTemplate()
            {
                Name = "BurnedoutForest",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.BurnedoutForest,
                Id = MapId.BurnedoutForest,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[24] = mapTemplate26;
            MapTemplate mapTemplate27 = new MapTemplate()
            {
                Name = "FrigidWastes",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.FrigidWastes,
                Id = MapId.FrigidWastes,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[26] = mapTemplate27;
            MapTemplate mapTemplate28 = new MapTemplate()
            {
                Name = "DesolateSwamp",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.DesolatedMarsh,
                Id = MapId.DesolatedMarsh,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[25] = mapTemplate28;
            MapTemplate mapTemplate29 = new MapTemplate()
            {
                Name = "GuildWave",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.Testing,
                Id = MapId.Testing,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[28] = mapTemplate29;
            MapTemplate mapTemplate30 = new MapTemplate()
            {
                Name = "Fantagle",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.Flabis | MapId.FirewayForest,
                Id = MapId.Flabis | MapId.FirewayForest,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[29] = mapTemplate30;
            MapTemplate mapTemplate31 = new MapTemplate()
            {
                Name = "WindCanyon",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.AlteracValley,
                Id = MapId.AlteracValley,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[30] = mapTemplate31;
            MapTemplate mapTemplate32 = new MapTemplate()
            {
                Name = "Astarica",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.DragonLair | MapId.FirewayForest,
                Id = MapId.DragonLair | MapId.FirewayForest,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[31] = mapTemplate32;
            MapTemplate mapTemplate33 = new MapTemplate()
            {
                Name = "Elysion",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = (MapId) 32,
                Id = (MapId) 32,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[32] = mapTemplate33;
            MapTemplate mapTemplate34 = new MapTemplate()
            {
                Name = "Acanpolys",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.ShadowfangKeep,
                Id = MapId.ShadowfangKeep,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[33] = mapTemplate34;
            MapTemplate mapTemplate35 = new MapTemplate()
            {
                Name = "Labyrinthos",
                Type = MapType.Normal,
                MaxPlayerCount = 1000,
                MinLevel = 1,
                RepopMapId = MapId.StormwindStockade,
                Id = MapId.StormwindStockade,
                RepopPosition = new Vector3(0.0f),
                IsAsda2FightingMap = false
            };
            WCell.RealmServer.Global.World.s_MapTemplates[34] = mapTemplate35;
        }

        private static void SetupCustomZones()
        {
            ZoneTemplate val1 = new ZoneTemplate()
            {
                Id = ZoneId.SilarisMain,
                m_MapId = MapId.Silaris,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "SilarisMain",
                Ownership = FactionGroupMask.Alliance
            };
            val1.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val1.Id, val1);
            MapTemplate mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate2 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val1.MapId);
            if (mapTemplate2 != null)
            {
                val1.MapTemplate = mapTemplate2;
                mapTemplate2.ZoneInfos.Add(val1);
            }

            ZoneTemplate val2 = new ZoneTemplate()
            {
                Id = ZoneId.RainRiverMain,
                m_MapId = MapId.RainRiver,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "RainRiverMain",
                Ownership = FactionGroupMask.Alliance
            };
            val2.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val2.Id, val2);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate3 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val2.MapId);
            if (mapTemplate3 != null)
            {
                val2.MapTemplate = mapTemplate3;
                mapTemplate3.ZoneInfos.Add(val2);
            }

            ZoneTemplate val3 = new ZoneTemplate()
            {
                Id = ZoneId.ConquestLandMain,
                m_MapId = MapId.ConquestLand,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "ConquestLandMain",
                Ownership = FactionGroupMask.Alliance
            };
            val3.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val3.Id, val3);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate4 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val3.MapId);
            if (mapTemplate4 != null)
            {
                val3.MapTemplate = mapTemplate4;
                mapTemplate4.ZoneInfos.Add(val3);
            }

            ZoneTemplate val4 = new ZoneTemplate()
            {
                Id = ZoneId.AlpiaMain,
                m_MapId = MapId.Alpia,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "AlpiaMain",
                Ownership = FactionGroupMask.Alliance
            };
            val4.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val4.Id, val4);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate5 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val4.MapId);
            if (mapTemplate5 != null)
            {
                val4.MapTemplate = mapTemplate5;
                mapTemplate5.ZoneInfos.Add(val4);
            }

            ZoneTemplate val5 = new ZoneTemplate()
            {
                Id = ZoneId.NightValeyMain,
                m_MapId = MapId.NightValey,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "NightValeyMain",
                Ownership = FactionGroupMask.Alliance
            };
            val5.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val5.Id, val5);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate6 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val5.MapId);
            if (mapTemplate6 != null)
            {
                val5.MapTemplate = mapTemplate6;
                mapTemplate6.ZoneInfos.Add(val5);
            }

            ZoneTemplate val6 = new ZoneTemplate()
            {
                Id = ZoneId.AquatonMain,
                m_MapId = MapId.Aquaton,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "AquatonMain",
                Ownership = FactionGroupMask.Alliance
            };
            val6.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val6.Id, val6);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate7 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val6.MapId);
            if (mapTemplate7 != null)
            {
                val6.MapTemplate = mapTemplate7;
                mapTemplate7.ZoneInfos.Add(val6);
            }

            ZoneTemplate val7 = new ZoneTemplate()
            {
                Id = ZoneId.SunnyCoastMain,
                m_MapId = MapId.SunnyCoast,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "SunnyCoastMain",
                Ownership = FactionGroupMask.Alliance
            };
            val7.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val7.Id, val7);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate8 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val7.MapId);
            if (mapTemplate8 != null)
            {
                val7.MapTemplate = mapTemplate8;
                mapTemplate8.ZoneInfos.Add(val7);
            }

            ZoneTemplate val8 = new ZoneTemplate()
            {
                Id = ZoneId.FlamioMain,
                m_MapId = MapId.Flamio,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "FlamioMain",
                Ownership = FactionGroupMask.Alliance
            };
            val8.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val8.Id, val8);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate9 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val8.MapId);
            if (mapTemplate9 != null)
            {
                val8.MapTemplate = mapTemplate9;
                mapTemplate9.ZoneInfos.Add(val8);
            }

            ZoneTemplate val9 = new ZoneTemplate()
            {
                Id = ZoneId.QueenPalaceMain,
                m_MapId = MapId.QueenPalace,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "QueenPalaceMain",
                Ownership = FactionGroupMask.Alliance
            };
            val9.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val9.Id, val9);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate10 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val9.MapId);
            if (mapTemplate10 != null)
            {
                val9.MapTemplate = mapTemplate10;
                mapTemplate10.ZoneInfos.Add(val9);
            }

            ZoneTemplate val10 = new ZoneTemplate()
            {
                Id = ZoneId.CastleOfChessMain,
                m_MapId = MapId.CastleOfChess,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "CastleOfChessMain",
                Ownership = FactionGroupMask.Alliance
            };
            val10.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val10.Id, val10);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate11 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val10.MapId);
            if (mapTemplate11 != null)
            {
                val10.MapTemplate = mapTemplate11;
                mapTemplate11.ZoneInfos.Add(val10);
            }

            ZoneTemplate val11 = new ZoneTemplate()
            {
                Id = ZoneId.FlamioPlainsMain,
                m_MapId = MapId.FlamioPlains,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "FlamioPlainsMain",
                Ownership = FactionGroupMask.Alliance
            };
            val11.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val11.Id, val11);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate12 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val11.MapId);
            if (mapTemplate12 != null)
            {
                val11.MapTemplate = mapTemplate12;
                mapTemplate12.ZoneInfos.Add(val11);
            }

            ZoneTemplate val12 = new ZoneTemplate()
            {
                Id = ZoneId.NeptillanNodeMain,
                m_MapId = MapId.NeptillanNode,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "NeptillanNodeMain",
                Ownership = FactionGroupMask.Alliance
            };
            val12.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val12.Id, val12);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate13 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val12.MapId);
            if (mapTemplate13 != null)
            {
                val12.MapTemplate = mapTemplate13;
                mapTemplate13.ZoneInfos.Add(val12);
            }

            ZoneTemplate val13 = new ZoneTemplate()
            {
                Id = ZoneId.FlamionMoutainMain,
                m_MapId = MapId.FlamionMoutain,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "FlamionMoutainMain",
                Ownership = FactionGroupMask.Alliance
            };
            val13.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val13.Id, val13);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate14 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val13.MapId);
            if (mapTemplate14 != null)
            {
                val13.MapTemplate = mapTemplate14;
                mapTemplate14.ZoneInfos.Add(val13);
            }

            ZoneTemplate val14 = new ZoneTemplate()
            {
                Id = ZoneId.FlabisMain,
                m_MapId = MapId.Flabis,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "FlabisMain",
                Ownership = FactionGroupMask.Alliance
            };
            val14.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val14.Id, val14);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate15 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val14.MapId);
            if (mapTemplate15 != null)
            {
                val14.MapTemplate = mapTemplate15;
                mapTemplate15.ZoneInfos.Add(val14);
            }

            ZoneTemplate val15 = new ZoneTemplate()
            {
                Id = ZoneId.StagnantDesertMain,
                m_MapId = MapId.StagnantDesert,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "StagnantDesertMain",
                Ownership = FactionGroupMask.Alliance
            };
            val15.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val15.Id, val15);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate16 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val15.MapId);
            if (mapTemplate16 != null)
            {
                val15.MapTemplate = mapTemplate16;
                mapTemplate16.ZoneInfos.Add(val15);
            }

            ZoneTemplate val16 = new ZoneTemplate()
            {
                Id = ZoneId.DragonLairMain,
                m_MapId = MapId.DragonLair,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "DragonLairMain",
                Ownership = FactionGroupMask.Alliance
            };
            val16.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val16.Id, val16);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate17 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val16.MapId);
            if (mapTemplate17 != null)
            {
                val16.MapTemplate = mapTemplate17;
                mapTemplate17.ZoneInfos.Add(val16);
            }

            ZoneTemplate val17 = new ZoneTemplate()
            {
                Id = ZoneId.FirewayForestMain,
                m_MapId = MapId.FirewayForest,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "FirewayForestMain",
                Ownership = FactionGroupMask.Alliance
            };
            val17.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val17.Id, val17);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate18 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val17.MapId);
            if (mapTemplate18 != null)
            {
                val17.MapTemplate = mapTemplate18;
                mapTemplate18.ZoneInfos.Add(val17);
            }

            ZoneTemplate val18 = new ZoneTemplate()
            {
                Id = ZoneId.CastleOfChaosMain,
                m_MapId = MapId.CastleOfChaos,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "CastleOfChaosMain",
                Ownership = FactionGroupMask.Alliance
            };
            val18.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val18.Id, val18);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate19 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val18.MapId);
            if (mapTemplate19 != null)
            {
                val18.MapTemplate = mapTemplate19;
                mapTemplate19.ZoneInfos.Add(val18);
            }

            ZoneTemplate val19 = new ZoneTemplate()
            {
                Id = ZoneId.InferionMain,
                m_MapId = MapId.Inferion,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "InferionMain",
                Ownership = FactionGroupMask.Alliance
            };
            val19.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val19.Id, val19);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate20 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val19.MapId);
            if (mapTemplate20 != null)
            {
                val19.MapTemplate = mapTemplate20;
                mapTemplate20.ZoneInfos.Add(val19);
            }

            ZoneTemplate val20 = new ZoneTemplate()
            {
                Id = ZoneId.BatleFieldMain,
                m_MapId = MapId.BatleField,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "BatleFieldMain",
                Ownership = FactionGroupMask.Alliance
            };
            val20.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val20.Id, val20);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate21 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val20.MapId);
            if (mapTemplate21 != null)
            {
                val20.MapTemplate = mapTemplate21;
                mapTemplate21.ZoneInfos.Add(val20);
            }

            ZoneTemplate val21 = new ZoneTemplate()
            {
                Id = ZoneId.EventFieldMain,
                m_MapId = MapId.DecaronLab,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "DecaronLabMain",
                Ownership = FactionGroupMask.Alliance
            };
            val21.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val21.Id, val21);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate22 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val21.MapId);
            if (mapTemplate22 != null)
            {
                val21.MapTemplate = mapTemplate22;
                mapTemplate22.ZoneInfos.Add(val21);
            }

            ZoneTemplate val22 = new ZoneTemplate()
            {
                Id = ZoneId.FieldOfHonnorMain,
                m_MapId = MapId.FieldOfHonnor,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "FieldOfHonnorMain",
                Ownership = FactionGroupMask.Alliance
            };
            val22.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val22.Id, val22);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate23 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val22.MapId);
            if (mapTemplate23 != null)
            {
                val22.MapTemplate = mapTemplate23;
                mapTemplate23.ZoneInfos.Add(val22);
            }

            ZoneTemplate val23 = new ZoneTemplate()
            {
                Id = ZoneId.OXMain,
                m_MapId = MapId.OX,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "OXMain",
                Ownership = FactionGroupMask.Alliance
            };
            val23.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val23.Id, val23);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate24 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val23.MapId);
            if (mapTemplate24 != null)
            {
                val23.MapTemplate = mapTemplate24;
                mapTemplate24.ZoneInfos.Add(val23);
            }

            ZoneTemplate val24 = new ZoneTemplate()
            {
                Id = ZoneId.IceQuarryMain,
                m_MapId = MapId.IceQuarry,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "IceQuarryMain",
                Ownership = FactionGroupMask.Alliance
            };
            val24.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val24.Id, val24);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate25 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val24.MapId);
            if (mapTemplate25 != null)
            {
                val24.MapTemplate = mapTemplate25;
                mapTemplate25.ZoneInfos.Add(val24);
            }

            ZoneTemplate val25 = new ZoneTemplate()
            {
                Id = ZoneId.NeverFallMain,
                m_MapId = MapId.NeverFall,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "NeverFallMain",
                Ownership = FactionGroupMask.Alliance
            };
            val25.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val25.Id, val25);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate26 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val25.MapId);
            if (mapTemplate26 != null)
            {
                val25.MapTemplate = mapTemplate26;
                mapTemplate26.ZoneInfos.Add(val25);
            }

            ZoneTemplate val26 = new ZoneTemplate()
            {
                Id = ZoneId.BurnedoutForestMain,
                m_MapId = MapId.BurnedoutForest,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "BurnedoutForestMain",
                Ownership = FactionGroupMask.Alliance
            };
            val26.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val26.Id, val26);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate27 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val26.MapId);
            if (mapTemplate27 != null)
            {
                val26.MapTemplate = mapTemplate27;
                mapTemplate27.ZoneInfos.Add(val26);
            }

            ZoneTemplate val27 = new ZoneTemplate()
            {
                Id = ZoneId.FrigidWastesMain,
                m_MapId = MapId.FrigidWastes,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "FrigidWastesMain",
                Ownership = FactionGroupMask.Alliance
            };
            val27.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val27.Id, val27);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate28 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val27.MapId);
            if (mapTemplate28 != null)
            {
                val27.MapTemplate = mapTemplate28;
                mapTemplate28.ZoneInfos.Add(val27);
            }

            ZoneTemplate val28 = new ZoneTemplate()
            {
                Id = ZoneId.DesolatedMarshMain,
                m_MapId = MapId.DesolatedMarsh,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "DesolateSwampMain",
                Ownership = FactionGroupMask.Alliance
            };
            val28.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val28.Id, val28);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate29 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val28.MapId);
            if (mapTemplate29 != null)
            {
                val28.MapTemplate = mapTemplate29;
                mapTemplate29.ZoneInfos.Add(val28);
            }

            ZoneTemplate val29 = new ZoneTemplate()
            {
                Id = ZoneId.End,
                m_MapId = MapId.Testing,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "GuildWaveMain",
                Ownership = FactionGroupMask.Alliance
            };
            val29.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val29.Id, val29);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate30 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val29.MapId);
            if (mapTemplate30 != null)
            {
                val29.MapTemplate = mapTemplate30;
                mapTemplate30.ZoneInfos.Add(val29);
            }

            ZoneTemplate val30 = new ZoneTemplate()
            {
                Id = ZoneId.DemontsPlace | ZoneId.ClaytönsWoWEditLand,
                m_MapId = MapId.Flabis | MapId.FirewayForest,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "FantagleMain",
                Ownership = FactionGroupMask.Alliance
            };
            val30.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val30.Id, val30);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate31 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val30.MapId);
            if (mapTemplate31 != null)
            {
                val30.MapTemplate = mapTemplate31;
                mapTemplate31.ZoneInfos.Add(val30);
            }

            ZoneTemplate val31 = new ZoneTemplate()
            {
                Id = ZoneId.TheDustPlains | ZoneId.ClaytönsWoWEditLand,
                m_MapId = MapId.AlteracValley,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "WindCanyonMain",
                Ownership = FactionGroupMask.Alliance
            };
            val31.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val31.Id, val31);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate32 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val31.MapId);
            if (mapTemplate32 != null)
            {
                val31.MapTemplate = mapTemplate32;
                mapTemplate32.ZoneInfos.Add(val31);
            }

            ZoneTemplate val32 = new ZoneTemplate()
            {
                Id = ZoneId.StonesplinterValley | ZoneId.ClaytönsWoWEditLand,
                m_MapId = MapId.DragonLair | MapId.FirewayForest,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "AstaricaMain",
                Ownership = FactionGroupMask.Alliance
            };
            val32.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val32.Id, val32);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate33 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val32.MapId);
            if (mapTemplate33 != null)
            {
                val32.MapTemplate = mapTemplate33;
                mapTemplate33.ZoneInfos.Add(val32);
            }

            ZoneTemplate val33 = new ZoneTemplate()
            {
                Id = ZoneId.ValleyOfKings | ZoneId.ClaytönsWoWEditLand,
                m_MapId = (MapId) 32,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "ElysionMain",
                Ownership = FactionGroupMask.Alliance
            };
            val33.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val33.Id, val33);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate34 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val33.MapId);
            if (mapTemplate34 != null)
            {
                val33.MapTemplate = mapTemplate34;
                mapTemplate34.ZoneInfos.Add(val33);
            }

            ZoneTemplate val34 = new ZoneTemplate()
            {
                Id = ZoneId.AlgazStation | ZoneId.ClaytönsWoWEditLand,
                m_MapId = MapId.ShadowfangKeep,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "AcanpolysMain",
                Ownership = FactionGroupMask.Alliance
            };
            val34.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val34.Id, val34);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate35 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val34.MapId);
            if (mapTemplate35 != null)
            {
                val34.MapTemplate = mapTemplate35;
                mapTemplate35.ZoneInfos.Add(val34);
            }

            ZoneTemplate val35 = new ZoneTemplate()
            {
                Id = ZoneId.BucklebreeFarm | ZoneId.ClaytönsWoWEditLand,
                m_MapId = MapId.StormwindStockade,
                m_parentZoneId = ZoneId.None,
                ExplorationBit = 0,
                Flags = ZoneFlags.Duel,
                AreaLevel = 1,
                Name = "LabyrinthosMain",
                Ownership = FactionGroupMask.Alliance
            };
            val35.FinalizeZone();
            ArrayUtil.Set<ZoneTemplate>(ref WCell.RealmServer.Global.World.s_ZoneTemplates, (uint) val35.Id, val35);
            mapTemplate1 = (MapTemplate) null;
            MapTemplate mapTemplate36 =
                WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) val35.MapId);
            if (mapTemplate36 == null)
                return;
            val35.MapTemplate = mapTemplate36;
            mapTemplate36.ZoneInfos.Add(val35);
        }

        private static void SetupBoundaries()
        {
            BoundingBox[] mapBoundaries = MapBoundaries.GetMapBoundaries();
            ZoneTileSet[] zoneTileSets = ZoneBoundaries.GetZoneTileSets();
            for (int index = 0; index < WCell.RealmServer.Global.World.s_MapTemplates.Length; ++index)
            {
                MapTemplate mapTemplate = WCell.RealmServer.Global.World.s_MapTemplates[index];
                if (mapTemplate != null)
                {
                    if (mapBoundaries != null && mapBoundaries.Length > index)
                        mapTemplate.Bounds = mapBoundaries[index];
                    if (zoneTileSets != null && zoneTileSets.Length > index)
                        mapTemplate.ZoneTileSet = zoneTileSets[index];
                }
            }
        }

        private static void LoadZoneInfos()
        {
            WCell.RealmServer.Global.World.SetupCustomZones();
        }

        private static void LoadChatChannelsDBC()
        {
            foreach (ChatChannelEntry chatChannelEntry in new MappedDBCReader<ChatChannelEntry, ChatChannelConverter>(
                RealmServerConfiguration.GetDBCFile("ChatChannels.dbc")).Entries.Values)
                ChatChannelGroup.DefaultChannelFlags.Add(chatChannelEntry.Id, new ChatChannelFlagsEntry()
                {
                    Flags = chatChannelEntry.ChannelFlags,
                    ClientFlags = ChatMgr.Convert(chatChannelEntry.ChannelFlags)
                });
        }

        /// <summary>Does some things to get the World back into sync</summary>
        public static void Resync()
        {
            using (WCell.RealmServer.Global.World.worldLock.EnterWriteLock())
            {
                int num;
                WCell.RealmServer.Global.World.s_allyPlayerCount = num = 0;
                WCell.RealmServer.Global.World.s_hordePlayerCount = num;
                WCell.RealmServer.Global.World.s_staffMemberCount = num;
                WCell.RealmServer.Global.World.s_characterCount = num;
                foreach (INamedEntity namedEntity in WCell.RealmServer.Global.World.s_namedEntities.Values)
                {
                    if (namedEntity is Character)
                    {
                        Character character = (Character) namedEntity;
                        ++WCell.RealmServer.Global.World.s_characterCount;
                        if (character.Role.IsStaff)
                            ++WCell.RealmServer.Global.World.s_staffMemberCount;
                        if (character.Faction.IsHorde)
                            ++WCell.RealmServer.Global.World.s_hordePlayerCount;
                        else
                            ++WCell.RealmServer.Global.World.s_allyPlayerCount;
                    }
                }
            }
        }

        /// <summary>Add a NamedEntity</summary>
        public static void AddNamedEntity(INamedEntity entity)
        {
            if (entity is Character)
            {
                WCell.RealmServer.Global.World.AddCharacter((Character) entity);
            }
            else
            {
                using (WCell.RealmServer.Global.World.worldLock.EnterWriteLock())
                {
                    WCell.RealmServer.Global.World.s_namedEntities.Add(entity.EntityId.Low, entity);
                    WCell.RealmServer.Global.World.s_entitiesByName.Add(entity.Name, entity);
                }
            }
        }

        public static Character GetCharacterBySessionId(ushort sessId)
        {
            if (!WCell.RealmServer.Global.World.CharactersBySessId.ContainsKey(sessId))
                return (Character) null;
            return WCell.RealmServer.Global.World.CharactersBySessId[sessId];
        }

        public static Character GetCharacterByAccId(uint accId)
        {
            if (!WCell.RealmServer.Global.World.CharactersByAccId.ContainsKey(accId))
                return (Character) null;
            return WCell.RealmServer.Global.World.CharactersByAccId[accId];
        }

        private static List<ushort> CharactersSessIdsPool
        {
            get
            {
                if (WCell.RealmServer.Global.World._charactersSessIdsPool == null)
                {
                    WCell.RealmServer.Global.World._charactersSessIdsPool = new List<ushort>((int) ushort.MaxValue);
                    for (ushort index = 1000; index < ushort.MaxValue; ++index)
                        WCell.RealmServer.Global.World._charactersSessIdsPool.Add(index);
                }

                return WCell.RealmServer.Global.World._charactersSessIdsPool;
            }
        }

        private static ushort GetNextFreeSessId()
        {
            ushort num = WCell.RealmServer.Global.World.CharactersSessIdsPool[0];
            WCell.RealmServer.Global.World.CharactersSessIdsPool.RemoveAt(0);
            return num;
        }

        private static void ReturnClearedSessId(ushort sessId)
        {
            WCell.RealmServer.Global.World._charactersSessIdsPool.Add(sessId);
        }

        /// <summary>Add a character to the world manager.</summary>
        /// <param name="chr">the character to add</param>
        public static void AddCharacter(Character chr)
        {
            using (WCell.RealmServer.Global.World.worldLock.EnterWriteLock())
            {
                WCell.RealmServer.Global.World.s_namedEntities.Add(chr.EntityId.Low, (INamedEntity) chr);
                WCell.RealmServer.Global.World.s_entitiesByName.Add(chr.Name, (INamedEntity) chr);
                chr.SessionId = (short) WCell.RealmServer.Global.World.GetNextFreeSessId();
                WCell.RealmServer.Global.World.CharactersBySessId.Add((ushort) chr.SessionId, chr);
                WCell.RealmServer.Global.World.CharactersByAccId.Add(chr.AccId, chr);
                ++WCell.RealmServer.Global.World.s_characterCount;
                if (chr.Role.IsStaff)
                    ++WCell.RealmServer.Global.World.s_staffMemberCount;
                if (chr.Faction.IsHorde)
                    ++WCell.RealmServer.Global.World.s_hordePlayerCount;
                else
                    ++WCell.RealmServer.Global.World.s_allyPlayerCount;
            }
        }

        /// <summary>Removes a character from the world manager.</summary>
        /// <param name="chr">the character to stop tracking</param>
        public static bool RemoveCharacter(Character chr)
        {
            using (WCell.RealmServer.Global.World.worldLock.EnterWriteLock())
            {
                WCell.RealmServer.Global.World.s_entitiesByName.Remove(chr.Name);
                WCell.RealmServer.Global.World.CharactersBySessId.Remove((ushort) chr.SessionId);
                WCell.RealmServer.Global.World.CharactersByAccId.Remove(chr.AccId);
                WCell.RealmServer.Global.World.ReturnClearedSessId((ushort) chr.SessionId);
                if (WCell.RealmServer.Global.World.s_namedEntities.Remove(chr.EntityId.Low))
                {
                    --WCell.RealmServer.Global.World.s_characterCount;
                    if (chr.Role.IsStaff)
                        --WCell.RealmServer.Global.World.s_staffMemberCount;
                    if (chr.Faction.IsHorde)
                        --WCell.RealmServer.Global.World.s_hordePlayerCount;
                    else
                        --WCell.RealmServer.Global.World.s_allyPlayerCount;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a <see cref="T:WCell.RealmServer.Entities.Character" /> by entity ID.
        /// </summary>
        /// <param name="lowEntityId">EntityId.Low of the Character to be looked up</param>
        /// <returns>the <see cref="T:WCell.RealmServer.Entities.Character" /> of the given ID; null otherwise</returns>
        public static Character GetCharacter(uint lowEntityId)
        {
            INamedEntity namedEntity;
            using (WCell.RealmServer.Global.World.worldLock.EnterReadLock())
                WCell.RealmServer.Global.World.s_namedEntities.TryGetValue(lowEntityId, out namedEntity);
            return namedEntity as Character;
        }

        public static INamedEntity GetNamedEntity(uint lowEntityId)
        {
            INamedEntity namedEntity;
            using (WCell.RealmServer.Global.World.worldLock.EnterReadLock())
                WCell.RealmServer.Global.World.s_namedEntities.TryGetValue(lowEntityId, out namedEntity);
            return namedEntity;
        }

        /// <summary>Gets a character by name.</summary>
        /// <param name="name">the name of the character to get</param>
        /// <returns>the <see cref="T:WCell.RealmServer.Entities.Character" /> object representing the character; null if not found</returns>
        public static Character GetCharacter(string name, bool caseSensitive)
        {
            if (name.Length == 0)
                return (Character) null;
            INamedEntity namedEntity;
            using (WCell.RealmServer.Global.World.worldLock.EnterReadLock())
            {
                WCell.RealmServer.Global.World.s_entitiesByName.TryGetValue(name, out namedEntity);
                if (namedEntity == null)
                    WCell.RealmServer.Global.World.s_entitiesByName.TryGetValue(
                        Asda2EncodingHelper.ReverseTranslit(name), out namedEntity);
                if (namedEntity == null)
                    return (Character) null;
                if (caseSensitive)
                {
                    if (namedEntity.Name != name)
                        return (Character) null;
                }
            }

            return namedEntity as Character;
        }

        /// <summary>Gets a character by name.</summary>
        /// <param name="name">the name of the character to get</param>
        /// <returns>the <see cref="T:WCell.RealmServer.Entities.Character" /> object representing the character; null if not found</returns>
        public static INamedEntity GetNamedEntity(string name, bool caseSensitive)
        {
            if (name.Length == 0)
                return (INamedEntity) null;
            INamedEntity namedEntity;
            using (WCell.RealmServer.Global.World.worldLock.EnterReadLock())
            {
                WCell.RealmServer.Global.World.s_entitiesByName.TryGetValue(name, out namedEntity);
                if (caseSensitive)
                {
                    if (namedEntity.Name != name)
                        return (INamedEntity) null;
                }
            }

            return namedEntity;
        }

        /// <summary>Gets all current characters.</summary>
        /// <returns>a list of <see cref="T:WCell.RealmServer.Entities.Character" /> objects</returns>
        public static List<Character> GetAllCharacters()
        {
            List<Character> characterList = new List<Character>(WCell.RealmServer.Global.World.s_namedEntities.Count);
            using (WCell.RealmServer.Global.World.worldLock.EnterReadLock())
            {
                foreach (INamedEntity namedEntity in WCell.RealmServer.Global.World.s_namedEntities.Values)
                {
                    if (namedEntity is Character)
                        characterList.Add((Character) namedEntity);
                }
            }

            return characterList;
        }

        /// <summary>Gets all current characters.</summary>
        /// <returns>a list of <see cref="T:WCell.RealmServer.Entities.Character" /> objects</returns>
        public static ICollection<INamedEntity> GetAllNamedEntities()
        {
            using (WCell.RealmServer.Global.World.worldLock.EnterReadLock())
                return (ICollection<INamedEntity>) WCell.RealmServer.Global.World.s_namedEntities.Values
                    .ToList<INamedEntity>();
        }

        /// <summary>Gets an enumerator of characters based on their race.</summary>
        /// <param name="entRace">the race to search for</param>
        /// <returns>a list of <see cref="T:WCell.RealmServer.Entities.Character" /> objects belonging to the given race</returns>
        public static ICollection<Character> GetCharactersOfRace(RaceId entRace)
        {
            List<Character> characterList = new List<Character>(WCell.RealmServer.Global.World.s_namedEntities.Count);
            using (WCell.RealmServer.Global.World.worldLock.EnterReadLock())
            {
                foreach (INamedEntity namedEntity in WCell.RealmServer.Global.World.s_namedEntities.Values)
                {
                    if (namedEntity is Character && ((Unit) namedEntity).Race == entRace)
                        characterList.Add((Character) namedEntity);
                }
            }

            return (ICollection<Character>) characterList;
        }

        /// <summary>
        /// Gets an enumerator of characters based on their class.
        /// </summary>
        /// <param name="entClass">the class to search for</param>
        /// <returns>a list of <see cref="T:WCell.RealmServer.Entities.Character" /> objects who are of the given class</returns>
        public static ICollection<Character> GetCharactersOfClass(ClassId entClass)
        {
            List<Character> characterList = new List<Character>(WCell.RealmServer.Global.World.s_namedEntities.Count);
            using (WCell.RealmServer.Global.World.worldLock.EnterReadLock())
            {
                foreach (INamedEntity namedEntity in WCell.RealmServer.Global.World.s_namedEntities.Values)
                {
                    if (namedEntity is Character && ((Unit) namedEntity).Class == entClass)
                        characterList.Add((Character) namedEntity);
                }
            }

            return (ICollection<Character>) characterList;
        }

        /// <summary>
        /// Gets an enumerator of characters based on their level.
        /// </summary>
        /// <param name="level">the level to search for</param>
        /// <returns>a list of <see cref="T:WCell.RealmServer.Entities.Character" /> objects who are of the given level</returns>
        public static ICollection<Character> GetCharactersOfLevel(uint level)
        {
            List<Character> characterList = new List<Character>(WCell.RealmServer.Global.World.s_namedEntities.Count);
            using (WCell.RealmServer.Global.World.worldLock.EnterReadLock())
            {
                foreach (INamedEntity namedEntity in WCell.RealmServer.Global.World.s_namedEntities.Values)
                {
                    if (namedEntity is Character && (long) ((Unit) namedEntity).Level == (long) level)
                        characterList.Add((Character) namedEntity);
                }
            }

            return (ICollection<Character>) characterList;
        }

        public static ICollection<Character> GetCharacters(int pageSize, int page, string nameFilter)
        {
            List<Character> characterList = new List<Character>(pageSize);
            int num1 = page * pageSize;
            int num2 = 0;
            using (WCell.RealmServer.Global.World.worldLock.EnterReadLock())
            {
                foreach (INamedEntity namedEntity in WCell.RealmServer.Global.World.s_namedEntities.Values)
                {
                    if (namedEntity is Character)
                    {
                        if (num2 > num1)
                        {
                            characterList.Add((Character) namedEntity);
                            if (characterList.Count == pageSize)
                                break;
                        }

                        ++num2;
                    }
                }
            }

            return (ICollection<Character>) characterList;
        }

        /// <summary>
        /// Gets an enumerator of characters based on what their name starts with.
        /// </summary>
        /// <param name="nameStarts">the starting part of the name to search for</param>
        /// <returns>a list of <see cref="T:WCell.RealmServer.Entities.Character" /> objects whose name starts with the given string</returns>
        public static ICollection<Character> GetCharactersStartingWith(string nameStarts)
        {
            List<Character> characterList = new List<Character>(WCell.RealmServer.Global.World.s_namedEntities.Count);
            using (WCell.RealmServer.Global.World.worldLock.EnterReadLock())
            {
                foreach (INamedEntity namedEntity in WCell.RealmServer.Global.World.s_namedEntities.Values)
                {
                    if (namedEntity is Character &&
                        namedEntity.Name.StartsWith(nameStarts, StringComparison.InvariantCultureIgnoreCase))
                        characterList.Add((Character) namedEntity);
                }
            }

            return (ICollection<Character>) characterList;
        }

        public static Map Kalimdor
        {
            get { return WCell.RealmServer.Global.World.s_Maps[726]; }
        }

        public static Map EasternKingdoms
        {
            get { return WCell.RealmServer.Global.World.s_Maps[725]; }
        }

        public static Map Outland
        {
            get { return WCell.RealmServer.Global.World.s_Maps[530]; }
        }

        public static Map Northrend
        {
            get { return WCell.RealmServer.Global.World.s_Maps[571]; }
        }

        /// <summary>Gets all default (non-instanced) Maps</summary>
        /// <returns>a collection of all current maps</returns>
        public static Map[] Maps
        {
            get { return WCell.RealmServer.Global.World.s_Maps; }
        }

        public static int MapCount { get; private set; }

        /// <summary>All Continents and Instances, including Battlegrounds</summary>
        /// <returns></returns>
        public static IEnumerable<Map> GetAllMaps()
        {
            for (int i = 0; i < WCell.RealmServer.Global.World.s_Maps.Length; ++i)
            {
                Map rgn = WCell.RealmServer.Global.World.s_Maps[i];
                if (rgn != null)
                    yield return rgn;
            }

            foreach (BaseInstance allInstance in InstanceMgr.Instances.GetAllInstances())
                yield return (Map) allInstance;
            foreach (Battleground allInstance in BattlegroundMgr.Instances.GetAllInstances())
                yield return (Map) allInstance;
        }

        /// <summary>Adds the map, associated with its unique Id</summary>
        /// <param name="map">the map to add</param>
        private static void AddMap(Map map)
        {
            ++WCell.RealmServer.Global.World.MapCount;
            if (WCell.RealmServer.Global.World.s_Maps[(uint) map.Id] != null)
                throw new InvalidOperationException("Tried to a second non-instanced map of the same type to World: " +
                                                    (object) map);
            ArrayUtil.Set<Map>(ref WCell.RealmServer.Global.World.s_Maps, (uint) map.Id, map);
        }

        /// <summary>Gets a normal Map by its Id</summary>
        /// <returns>the <see cref="T:WCell.RealmServer.Global.Map" /> object; null if the ID is not valid</returns>
        public static Map GetNonInstancedMap(MapId mapId)
        {
            return WCell.RealmServer.Global.World.s_Maps.Get<Map>((uint) mapId);
        }

        /// <summary>Gets a normal Map by its Id</summary>
        /// <returns>the <see cref="T:WCell.RealmServer.Global.Map" /> object; null if the ID is not valid</returns>
        public static Map GetMap(IMapId mapId)
        {
            if (mapId.InstanceId <= 0U)
                return WCell.RealmServer.Global.World.s_Maps.Get<Map>((uint) mapId.MapId);
            MapTemplate mapTemplate = WCell.RealmServer.Global.World.GetMapTemplate(mapId.MapId);
            if (mapTemplate == null)
                return (Map) null;
            if (mapTemplate.IsBattleground)
                return (Map) BattlegroundMgr.Instances.GetInstance(mapTemplate.BattlegroundTemplate.Id,
                    mapId.InstanceId);
            return (Map) InstanceMgr.Instances.GetInstance(mapId.MapId, mapId.InstanceId);
        }

        /// <summary>Gets map info by ID.</summary>
        /// <param name="mapID">the ID to the map to get</param>
        /// <returns>the <see cref="T:WCell.RealmServer.Global.MapTemplate" /> object for the given map ID</returns>
        public static MapTemplate GetMapTemplate(MapId mapID)
        {
            if (WCell.RealmServer.Global.World.s_ZoneTemplates == null)
                WCell.RealmServer.Global.World.LoadMapData();
            return WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) mapID);
        }

        public static BoundingBox GetMapBoundingBox(MapId mapId)
        {
            MapTemplate mapTemplate = WCell.RealmServer.Global.World.s_MapTemplates.Get<MapTemplate>((uint) mapId);
            if (mapTemplate != null)
                return mapTemplate.Bounds;
            return new BoundingBox();
        }

        public static bool IsInstance(MapId mapId)
        {
            MapTemplate mapTemplate = WCell.RealmServer.Global.World.GetMapTemplate(mapId);
            if (mapTemplate != null)
                return mapTemplate.IsInstance;
            return false;
        }

        /// <summary>Gets zone template by ID.</summary>
        /// <param name="zoneID">the ID to the zone to get</param>
        /// <returns>the <see cref="T:WCell.RealmServer.Global.Zone" /> object for the given zone ID</returns>
        public static ZoneTemplate GetZoneInfo(ZoneId zoneID)
        {
            return WCell.RealmServer.Global.World.s_ZoneTemplates.Get<ZoneTemplate>((uint) zoneID);
        }

        /// <summary>
        /// Gets the first significant location within the Zone with the given Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IWorldLocation GetSite(ZoneId id)
        {
            return WCell.RealmServer.Global.World.GetZoneInfo(id)?.Site;
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Fifth, "Initializing World")]
        public static void LoadDefaultMaps()
        {
            foreach (MapTemplate mapTemplate in WCell.RealmServer.Global.World.s_MapTemplates)
            {
                if (mapTemplate != null && mapTemplate.Type == MapType.Normal)
                {
                    Map map = new Map(mapTemplate);
                    map.XpCalculator = new ExperienceCalculator(XpGenerator.CalcDefaultXp);
                    map.InitMap();
                    WCell.RealmServer.Global.World.AddMap(map);
                }
            }
        }

        /// <summary>
        /// Calls the given Action on all currently logged in Characters
        /// </summary>
        /// <param name="action"></param>
        /// <param name="doneCallback">Called after the action was called on everyone.</param>
        public static void CallOnAllChars(Action<Character> action, Action doneCallback)
        {
            IEnumerable<Map> allMaps = WCell.RealmServer.Global.World.GetAllMaps();
            int rgnCount = allMaps.Count<Map>();
            int i = 0;
            foreach (Map map1 in allMaps)
                map1.AddMessage((IMessage) new Message1<Map>(map1, (Action<Map>) (map =>
                {
                    foreach (Character character in map.Characters)
                        action(character);
                    ++i;
                    if (i != rgnCount)
                        return;
                    doneCallback();
                })));
        }

        /// <summary>
        /// Calls the given Action on all currently logged in Characters
        /// </summary>
        /// <param name="action"></param>
        /// <param name="doneCallback">Called after the action was called on everyone.</param>
        public static void CallOnAllChars(Action<Character> action)
        {
            foreach (Map allMap in WCell.RealmServer.Global.World.GetAllMaps())
            {
                if (allMap.CharacterCount > 0)
                    allMap.AddMessage((IMessage) new Message1<Map>(allMap, (Action<Map>) (map =>
                    {
                        foreach (Character character in map.Characters)
                        {
                            if (character.Client.IsGameServerConnection)
                                action(character);
                        }
                    })));
            }
        }

        /// <summary>
        /// Calls the given Action on all currently existing Maps within each Map's context
        /// </summary>
        /// <param name="action"></param>
        /// <param name="doneCallback">Called after the action was called on all Maps.</param>
        public static void CallOnAllMaps(Action<Map> action, Action doneCallback)
        {
            IEnumerable<Map> allMaps = WCell.RealmServer.Global.World.GetAllMaps();
            int rgnCount = allMaps.Count<Map>();
            int i = 0;
            foreach (Map map in allMaps)
            {
                Map rgn = map;
                rgn.AddMessage((Action) (() =>
                {
                    action(rgn);
                    ++i;
                    if (i != rgnCount)
                        return;
                    doneCallback();
                }));
            }
        }

        /// <summary>
        /// Calls the given Action on all currently existing Maps within each Map's context
        /// </summary>
        /// <param name="action"></param>
        public static void CallOnAllMaps(Action<Map> action)
        {
            foreach (Map allMap in WCell.RealmServer.Global.World.GetAllMaps())
            {
                Map rgn = allMap;
                rgn.AddMessage((Action) (() => action(rgn)));
            }
        }

        public static void Broadcast(RealmLangKey key, params object[] args)
        {
            WCell.RealmServer.Global.World.Broadcast((IChatter) null, RealmLocalizer.Instance.Translate(key, args));
        }

        public static void Broadcast(IChatter broadCaster, RealmLangKey key, params object[] args)
        {
            WCell.RealmServer.Global.World.Broadcast(broadCaster, RealmLocalizer.Instance.Translate(key, args));
        }

        public static void Broadcast(string message, params object[] args)
        {
            WCell.RealmServer.Global.World.Broadcast((IChatter) null, string.Format(message, args));
        }

        public static void Broadcast(string sender, string message, Color c, Locale locale)
        {
            foreach (IPacketReceiver allCharacter in WCell.RealmServer.Global.World.GetAllCharacters())
                ChatMgr.SendMessage(allCharacter, sender, message, c);
        }

        public static void Broadcast(IChatter broadCaster, string message, params object[] args)
        {
            WCell.RealmServer.Global.World.Broadcast(broadCaster, string.Format(message, args));
        }

        public static void Broadcast(IChatter broadCaster, string message)
        {
            if (broadCaster != null)
                message = broadCaster.Name + ": ";
            WCell.RealmServer.Global.World.GetAllCharacters().SendSystemMessage(message);
            WCell.RealmServer.Global.World.s_log.Info("[Broadcast] " + ChatUtility.Strip(message));
            Action<IChatter, string> broadcasted = WCell.RealmServer.Global.World.Broadcasted;
            if (broadcasted == null)
                return;
            broadcasted(broadCaster, message);
        }

        public static void BroadcastMsg(string sender, string msg, Color c)
        {
            for (Locale locale = Locale.Start; locale < Locale.End; ++locale)
            {
                Locale locale1 = locale;
                WCell.RealmServer.Global.World.CallOnAllChars((Action<Character>) (chr =>
                {
                    if (!chr.Client.IsGameServerConnection || chr.Client.Locale != locale1)
                        return;
                    chr.Send(ChatMgr.CreateGlobalChatMessage(sender, msg, c, locale1), false);
                }));
            }
        }

        public static void Broadcast(RealmPacketOut packet, bool addEnd, Locale locale)
        {
            foreach (Character allCharacter in WCell.RealmServer.Global.World.GetAllCharacters())
            {
                if (allCharacter.Client.IsGameServerConnection &&
                    (locale == Locale.Any || allCharacter.Client.Locale == locale))
                    allCharacter.Send(packet, addEnd);
            }
        }

        /// <summary>Checks if the event is currently active</summary>
        /// <param name="eventId">Id of the event to check</param>
        /// <returns></returns>
        public bool IsEventActive(uint eventId)
        {
            return WorldEventMgr.IsEventActive(eventId);
        }

        public IWorldSpace ParentSpace
        {
            get { return (IWorldSpace) null; }
        }

        public WorldStateCollection WorldStates { get; private set; }

        public void CallOnAllCharacters(Action<Character> action)
        {
            WCell.RealmServer.Global.World.CallOnAllChars(action);
        }

        /// <summary>
        /// Is called after the World paused or unpaused completely and before the PauseLock is released.
        /// </summary>
        public static event Action<bool> WorldPaused;

        /// <summary>
        /// Is called after the World has been saved and before the PauseLock is released (eg. during shutdown)
        /// </summary>
        public static event Action Saved;

        /// <summary>
        /// Is called when the given Chatter (can be null) broadcasts something
        /// </summary>
        public static event Action<IChatter, string> Broadcasted;
    }
}