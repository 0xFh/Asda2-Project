using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Network;
using WCell.Core.Terrain;
using WCell.Core.Timers;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.AI;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Battlegrounds.Arenas;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Events.Asda2;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.Res;
using WCell.RealmServer.Spawns;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.Graphics;
using WCell.Util.NLog;
using WCell.Util.Threading;
using WCell.Util.Threading.TaskParallel;
using WCell.Util.Variables;

namespace WCell.RealmServer.Global
{
    /// <summary>
    /// Represents a continent or Instance (including Battleground instances), that may or may not contain Zones.
    /// X-Axis: South -&gt; North
    /// Y-Axis: East -&gt; West
    /// Z-Axis: Down -&gt; Up
    /// Orientation: North = 0, 2*Pi, counter-clockwise
    /// </summary>
    public class Map : IGenericChatTarget, IMapId, IContextHandler, IWorldSpace
    {
        protected List<Character> m_characters = new List<Character>(50);
        protected List<NPC> m_spiritHealers = new List<NPC>();
        protected LockfreeQueue<IMessage> m_messageQueue = new LockfreeQueue<IMessage>();
        protected List<IUpdatable> m_updatables = new List<IUpdatable>();
        protected internal Dictionary<uint, GOSpawnPool> m_goSpawnPools = new Dictionary<uint, GOSpawnPool>();
        protected internal Dictionary<uint, NPCSpawnPool> m_npcSpawnPools = new Dictionary<uint, NPCSpawnPool>();

        /// <summary>
        /// List of all Main zones (that have no parent-zone but only children)
        /// </summary>
        public readonly List<Zone> MainZones = new List<Zone>(5);

        /// <summary>All the Zone-instances within this map.</summary>
        public readonly IDictionary<ZoneId, Zone> Zones = (IDictionary<ZoneId, Zone>) new Dictionary<ZoneId, Zone>();

        private bool m_SpawnPointsEnabled = true;
        private Dictionary<ushort, WorldObject> _asda2Npcs = new Dictionary<ushort, WorldObject>();
        private readonly Asda2LootItem[,] _lootPositions = new Asda2LootItem[500, 500];
        public readonly List<Asda2Loot> Loots = new List<Asda2Loot>();

        private readonly Dictionary<Locale, List<Character>> _charactersByLocale =
            new Dictionary<Locale, List<Character>>();

        /// <summary>
        /// Whether to spawn NPCs and GOs immediately, whenever a new Map gets activated the first time.
        /// For developing you might want to toggle this off and use the "Map Spawn" command ingame to spawn the map, if necessary.
        /// </summary>
        [Variable("AutoSpawnMaps")] public static bool AutoSpawnMaps = true;

        /// <summary>Default update delay in milliseconds</summary>
        public static int DefaultUpdateDelay = 120;

        /// <summary>
        /// Every how many ticks to send UpdateField-changes to Characters
        /// </summary>
        public static int CharacterUpdateEnvironmentTicks = 1000 / Map.DefaultUpdateDelay;

        [Variable("UpdateInactiveAreas")] public static bool UpdateInactiveAreasDefault = false;

        /// <summary>
        /// Whether to have NPCs in inactive areas scan for enemies
        /// </summary>
        [Variable("ScanInactiveAreas")] public static bool ScanInactiveAreasDefault = false;

        /// <summary>
        /// Whether NPCs can evade and run back to their spawn point when pulled too far away
        /// </summary>
        [Variable("NPCsCanEvade")] public static bool CanNPCsEvadeDefault = true;

        private static int[] updatePriorityMillis = new int[6];
        protected static Logger s_log = LogManager.GetCurrentClassLogger();
        public static int UniqObjetIdOnMapInterator = 200000;
        private static float _avgUpdateTime;
        protected internal MapTemplate m_MapTemplate;
        protected Dictionary<EntityId, WorldObject> m_objects;
        protected ZoneSpacePartitionNode m_root;
        protected ZoneTileSet m_zoneTileSet;
        protected ITerrain m_Terrain;
        protected volatile bool m_running;
        protected DateTime m_lastUpdateTime;
        protected int m_tickCount;
        protected int m_updateDelay;
        protected bool m_updateInactiveAreas;
        protected bool m_ScanInactiveAreas;
        protected bool m_canNPCsEvade;
        private int m_allyCount;
        private int m_hordeCount;
        private int m_currentThreadId;
        private bool m_npcsSpawned;
        private bool m_gosSpawned;
        private bool m_isUpdating;
        private int m_lastNPCPoolId;
        protected bool m_CanFly;
        private Zone m_defaultZone;

        /// <summary>
        /// The first TaxiPath-node of this Map (or null).
        /// This is used to send individual Taxi Maps to Players.
        /// </summary>
        public PathNode FirstTaxiNode;

        private bool m_IsAIFrozen;
        private int maxObjUpdateTime;
        private int totalUpdates;
        private List<ushort> _avalibleUniqNpcIds;
        private bool m_IsDisposed;

        [NotVariable]
        public static int[] UpdatePriorityMillis
        {
            get { return Map.updatePriorityMillis; }
            set
            {
                Map.updatePriorityMillis = value;
                Map.SetUpdatePriorityTicks();
            }
        }

        private static void SetDefaultUpdatePriorityTick(UpdatePriority priority, int ticks)
        {
            if (Map.UpdatePriorityMillis[(int) priority] != 0)
                return;
            Map.UpdatePriorityMillis[(int) priority] = ticks;
        }

        private static void SetUpdatePriorityTicks()
        {
            if (Map.UpdatePriorityMillis == null)
                Map.UpdatePriorityMillis = new int[6];
            else if (Map.UpdatePriorityMillis.Length != 6)
                Array.Resize<int>(ref Map.updatePriorityMillis, 6);
            Map.SetDefaultUpdatePriorityTick(UpdatePriority.Inactive, 10000);
            Map.SetDefaultUpdatePriorityTick(UpdatePriority.Background, 3000);
            Map.SetDefaultUpdatePriorityTick(UpdatePriority.VeryLowPriority, 1000);
            Map.SetDefaultUpdatePriorityTick(UpdatePriority.LowPriority, 600);
            Map.SetDefaultUpdatePriorityTick(UpdatePriority.Active, 300);
            Map.SetDefaultUpdatePriorityTick(UpdatePriority.HighPriority, 0);
        }

        public static int GetTickCount(UpdatePriority priority)
        {
            return Map.UpdatePriorityMillis[(int) priority];
        }

        public static void SetTickCount(UpdatePriority priority, int count)
        {
            Map.UpdatePriorityMillis[(int) priority] = count;
        }

        static Map()
        {
            Map.SetUpdatePriorityTicks();
        }

        public static string LoadAvgStr
        {
            get
            {
                return string.Format("{0:0.00}/{1} ({2} %)", (object) Map._avgUpdateTime,
                    (object) Map.DefaultUpdateDelay,
                    (object) (float) ((double) Map._avgUpdateTime / (double) Map.DefaultUpdateDelay * 100.0));
            }
        }

        public int CurrentThreadId
        {
            get { return this.m_currentThreadId; }
        }

        internal uint GenerateNewNPCPoolId()
        {
            return (uint) Interlocked.Increment(ref this.m_lastNPCPoolId);
        }

        public WorldStateCollection WorldStates { get; private set; }

        protected Map()
        {
            this.m_objects = new Dictionary<EntityId, WorldObject>();
            this.m_updateDelay = Map.DefaultUpdateDelay;
            this.m_updateInactiveAreas = Map.UpdateInactiveAreasDefault;
            this.m_ScanInactiveAreas = Map.ScanInactiveAreasDefault;
            this.m_canNPCsEvade = Map.CanNPCsEvadeDefault;
            for (Locale key = Locale.Start; key < Locale.End; ++key)
                this.CharactersByLocale.Add(key, new List<Character>());
            this.CallDelayed(600000, new Action(this.ClearLoot));
        }

        public void ClearLoot()
        {
            foreach (Asda2Loot asda2Loot in this.Loots.Where<Asda2Loot>((Func<Asda2Loot, bool>) (asda2Loot =>
                    (DateTime.Now - asda2Loot.SpawnTime).TotalMinutes > (double) CharacterFormulas.DropLiveMinutes))
                .ToList<Asda2Loot>())
            {
                foreach (Asda2LootItem asda2LootItem in asda2Loot.Items)
                {
                    GlobalHandler.SendRemoveItemResponse(asda2LootItem);
                    this.ClearLootSlot(asda2LootItem.Position.X, asda2LootItem.Position.Y);
                }

                asda2Loot.Dispose();
            }

            this.CallDelayed(600000, new Action(this.ClearLoot));
        }

        public void ClearLootNow()
        {
            for (int index = 0; index < this.Loots.Count; index = index - 1 + 1)
            {
                Asda2Loot loot = this.Loots[index];
                foreach (Asda2LootItem asda2LootItem in loot.Items)
                {
                    GlobalHandler.SendRemoveItemResponse(asda2LootItem);
                    this.ClearLootSlot(asda2LootItem.Position.X, asda2LootItem.Position.Y);
                }

                loot.Dispose();
            }
        }

        /// <summary>Creates a map from the given map info.</summary>
        /// <param name="rgnTemplate">the info for this map to use</param>
        public Map(MapTemplate rgnTemplate)
            : this()
        {
            this.m_MapTemplate = rgnTemplate;
            this.Offset = (float) rgnTemplate.Id * 1000f;
            this.m_CanFly = rgnTemplate.Id == MapId.Outland || rgnTemplate.Id == MapId.Northrend;
        }

        protected internal void InitMap(MapTemplate template)
        {
            this.m_MapTemplate = template;
            this.InitMap();
        }

        /// <summary>Method is called after Creation of Map</summary>
        protected internal virtual void InitMap()
        {
            this.m_Terrain = TerrainMgr.GetTerrain(this.m_MapTemplate.Id);
            this.m_zoneTileSet = this.m_MapTemplate.ZoneTileSet;
            this.m_root = new ZoneSpacePartitionNode(this.m_MapTemplate.Bounds);
            this.PartitionSpace();
            this.WorldStates = new WorldStateCollection((IWorldSpace) this,
                WCell.Constants.World.WorldStates.GetStates(this.m_MapTemplate.Id) ?? WorldState.EmptyArray);
            this.CreateZones();
            this.m_MapTemplate.NotifyCreated(this);
        }

        private void CreateZones()
        {
            for (int index = 0; index < this.m_MapTemplate.ZoneInfos.Count; ++index)
            {
                ZoneTemplate zoneInfo = this.m_MapTemplate.ZoneInfos[index];
                Zone zone = zoneInfo.Creator(this, zoneInfo);
                if (zone.ParentZone == null)
                    this.MainZones.Add(zone);
                this.Zones.Add(zoneInfo.Id, zone);
            }

            this.m_defaultZone = this.MainZones.FirstOrDefault<Zone>();
        }

        public MapId MapId
        {
            get { return this.Id; }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual uint InstanceId
        {
            get { return 0; }
        }

        public MapTemplate MapTemplate
        {
            get { return this.m_MapTemplate; }
        }

        public ITerrain Terrain
        {
            get { return this.m_Terrain; }
        }

        public IWorldSpace ParentSpace
        {
            get { return (IWorldSpace) WCell.RealmServer.Global.World.Instance; }
        }

        public int MainZoneCount
        {
            get { return this.MainZones.Count; }
        }

        /// <summary>
        /// The first MainZone (for reference and Maps that only contain one Zone)
        /// </summary>
        public Zone DefaultZone
        {
            get { return this.m_defaultZone; }
        }

        /// <summary>The bounds of this map.</summary>
        public BoundingBox Bounds
        {
            get { return this.m_root.Bounds; }
        }

        public ICollection<NPC> SpiritHealers
        {
            get { return (ICollection<NPC>) this.m_spiritHealers; }
        }

        /// <summary>The display name of this map.</summary>
        public string Name
        {
            get { return this.m_MapTemplate.Name; }
        }

        /// <summary>The map ID of this map.</summary>
        public MapId Id
        {
            get { return this.m_MapTemplate.Id; }
        }

        /// <summary>The minimum required ClientId</summary>
        public ClientId RequiredClient
        {
            get { return this.m_MapTemplate.RequiredClientId; }
        }

        /// <summary>Whether or not the map is instanced</summary>
        public bool IsInstance
        {
            get { return this is InstancedMap; }
        }

        public bool IsArena
        {
            get { return this is Arena; }
        }

        public bool IsBattleground
        {
            get { return this is Battleground; }
        }

        /// <summary>
        /// The type of the map (normal, battlegrounds, instance, etc)
        /// </summary>
        public MapType Type
        {
            get { return this.m_MapTemplate.Type; }
        }

        public bool IsHeroic
        {
            get
            {
                MapDifficultyEntry difficulty = this.Difficulty;
                if (difficulty != null)
                    return difficulty.IsHeroic;
                return false;
            }
        }

        public uint DifficultyIndex
        {
            get
            {
                MapDifficultyEntry difficulty = this.Difficulty;
                if (difficulty == null)
                    return 0;
                return difficulty.Index;
            }
        }

        /// <summary>Difficulty of the instance</summary>
        public virtual MapDifficultyEntry Difficulty
        {
            get { return (MapDifficultyEntry) null; }
        }

        /// <summary>
        /// The minimum level a player has to be to enter the map (instance)
        /// </summary>
        public virtual int MinLevel
        {
            get { return this.m_MapTemplate.MinLevel; }
        }

        /// <summary>
        /// The maximum level a player can be to enter the map (instance)
        /// </summary>
        public virtual int MaxLevel
        {
            get { return this.m_MapTemplate.MaxLevel; }
        }

        /// <summary>
        /// Maximum number of players allowed in the map (instance)
        /// </summary>
        public int MaxPlayerCount
        {
            get { return this.m_MapTemplate.MaxPlayerCount; }
        }

        /// <summary>
        /// Whether or not the map is currently processing object updates
        /// </summary>
        public bool IsRunning
        {
            get { return this.m_running; }
            set
            {
                if (this.m_running == value)
                    return;
                if (value)
                    this.Start();
                else
                    this.Stop();
            }
        }

        /// <summary>
        /// Indicates whether the current Thread is the Map's update-thread.
        /// </summary>
        public bool IsInContext
        {
            get { return Thread.CurrentThread.ManagedThreadId == this.m_currentThreadId; }
        }

        public bool IsUpdating
        {
            get { return this.m_isUpdating; }
        }

        public virtual DateTime CreationTime
        {
            get { return ServerApp<WCell.RealmServer.RealmServer>.StartTime; }
        }

        /// <summary>
        /// The amount of all Players in this Map (excludes Staff members)
        /// </summary>
        public int PlayerCount
        {
            get { return this.m_allyCount + this.m_hordeCount; }
        }

        /// <summary>
        /// The amount of all Characters in this Map (includes Staff members)
        /// </summary>
        public int CharacterCount
        {
            get { return this.m_characters.Count; }
        }

        /// <summary>
        /// The number of Alliance Players currently in the map (not counting Staff)
        /// </summary>
        public int AllianceCount
        {
            get { return this.m_allyCount; }
        }

        /// <summary>
        /// The number of Alliance Players currently in the map (not counting Staff)
        /// </summary>
        public int HordeCount
        {
            get { return this.m_hordeCount; }
        }

        public int NPCSpawnPoolCount
        {
            get { return this.m_npcSpawnPools.Count; }
        }

        /// <summary>Amount of passed ticks in this Map</summary>
        public int TickCount
        {
            get { return this.m_tickCount; }
        }

        /// <summary>Don't modify the List.</summary>
        public List<Character> Characters
        {
            get
            {
                this.EnsureContext();
                return this.m_characters;
            }
        }

        /// <summary>
        /// The calculator used to compute Xp given for killing NPCs
        /// </summary>
        public ExperienceCalculator XpCalculator { get; set; }

        /// <summary>
        /// Called whenever an object is removed from the map to determine whether it may stop now.
        /// </summary>
        public virtual bool ShouldStop
        {
            get { return this.m_objects.Count == 0; }
        }

        /// <summary>
        /// Whether to also update Nodes in areas without Players.
        /// Default: <see cref="F:WCell.RealmServer.Global.Map.UpdateInactiveAreasDefault" />
        /// </summary>
        public bool UpdateInactiveAreas
        {
            get { return this.m_updateInactiveAreas; }
            set { this.m_updateInactiveAreas = value; }
        }

        /// <summary>
        /// Whether to let NPCs scan inactive Nodes for hostility.
        /// Default: <see cref="F:WCell.RealmServer.Global.Map.ScanInactiveAreasDefault" />
        /// </summary>
        public bool ScanInactiveAreas
        {
            get { return this.m_ScanInactiveAreas; }
            set { this.m_ScanInactiveAreas = value; }
        }

        /// <summary>
        /// Time in milliseconds between the beginning of
        /// one Map-Update and the next.
        /// </summary>
        public int UpdateDelay
        {
            get { return this.m_updateDelay; }
            set { Interlocked.Exchange(ref this.m_updateDelay, value); }
        }

        public int CharacterUpdateEnvironmentDelay
        {
            get { return Map.CharacterUpdateEnvironmentTicks * this.m_updateDelay; }
        }

        /// <summary>Total amount of objects within this Map</summary>
        public int ObjectCount
        {
            get { return this.m_objects.Count; }
        }

        /// <summary>
        /// Whether NPCs in this Map will try to evade after Combat
        /// </summary>
        public bool CanNPCsEvade
        {
            get { return this.m_canNPCsEvade; }
            set { this.m_canNPCsEvade = value; }
        }

        public bool CanFly
        {
            get { return this.m_CanFly; }
            set { this.m_CanFly = value; }
        }

        /// <summary>Toggles all NPCs to be invul and idle</summary>
        public bool IsAIFrozen
        {
            get { return this.m_IsAIFrozen; }
            set
            {
                this.EnsureContext();
                if (this.m_IsAIFrozen == value)
                    return;
                this.m_IsAIFrozen = value;
                foreach (WorldObject worldObject in this.m_objects.Values)
                {
                    if (worldObject is NPC)
                    {
                        NPC npc = (NPC) worldObject;
                        if (value)
                        {
                            npc.Brain.State = BrainState.Idle;
                            ++npc.Invulnerable;
                        }
                        else
                        {
                            npc.Brain.EnterDefaultState();
                            --npc.Invulnerable;
                        }
                    }
                }
            }
        }

        public virtual FactionGroup OwningFaction
        {
            get { return FactionGroup.Invalid; }
        }

        /// <summary>Starts the Map's update-, message- and timer- loop</summary>
        public void Start()
        {
            if (this.m_running)
                return;
            lock (this.m_objects)
            {
                if (this.m_running)
                    return;
                lock (WCell.RealmServer.Global.World.PauseLock)
                {
                    if (WCell.RealmServer.Global.World.Paused)
                        throw new InvalidOperationException("Tried to start Map while World is paused.");
                    this.m_running = true;
                }

                Map.s_log.Debug(WCell_RealmServer.MapStarted, (object) this.m_MapTemplate.Id);
                Task.Factory.StartNewDelayed(this.m_updateDelay, new Action<object>(this.MapUpdateCallback),
                    (object) this);
                if (Map.AutoSpawnMaps)
                    this.SpawnMapLater();
                this.m_lastUpdateTime = DateTime.Now;
                this.m_MapTemplate.NotifyStarted(this);
            }
        }

        /// <summary>
        /// Stops map updating and stops the update delta measuring
        /// </summary>
        public void Stop()
        {
            if (!this.m_running || !this.m_MapTemplate.NotifyStopping(this))
                return;
            lock (this.m_objects)
            {
                if (!this.m_running)
                    return;
                this.m_running = false;
                Map.s_log.Debug(WCell_RealmServer.MapStopped, (object) this.m_MapTemplate.Id);
                this.m_MapTemplate.NotifyStopped(this);
            }
        }

        public bool ExecuteInContext(Action action)
        {
            if (!this.IsInContext)
            {
                this.AddMessage((IMessage) new Message(action));
                return false;
            }

            action();
            return true;
        }

        /// <summary>Ensures execution within the map.</summary>
        /// <exception cref="T:System.InvalidOperationException">thrown if the calling thread isn't the map thread</exception>
        public void EnsureContext()
        {
            if (Thread.CurrentThread.ManagedThreadId != this.m_currentThreadId && this.IsRunning)
            {
                this.Stop();
                throw new InvalidOperationException(string.Format(WCell_RealmServer.MapContextNeeded, (object) this));
            }
        }

        /// <summary>Ensures execution outside the Map-context.</summary>
        /// <exception cref="T:System.InvalidOperationException">thrown if the calling thread is the map thread</exception>
        public void EnsureNoContext()
        {
            if (Thread.CurrentThread.ManagedThreadId == this.m_currentThreadId)
            {
                this.Stop();
                throw new InvalidOperationException(
                    string.Format(WCell_RealmServer.MapContextProhibited, (object) this));
            }
        }

        /// <summary>Ensures that Map is not updating.</summary>
        /// <exception cref="T:System.InvalidOperationException">thrown if the Map is currently updating</exception>
        public void EnsureNotUpdating()
        {
            if (this.m_isUpdating)
            {
                this.Stop();
                throw new InvalidOperationException(string.Format(WCell_RealmServer.MapUpdating, (object) this));
            }
        }

        /// <summary>
        /// Whether this Map's NPCs and GOs have been fully spawned
        /// </summary>
        public bool IsSpawned
        {
            get
            {
                if (this.m_npcsSpawned)
                    return this.m_gosSpawned;
                return false;
            }
        }

        public bool IsSpawning { get; private set; }

        public bool NPCsSpawned
        {
            get { return this.m_npcsSpawned; }
        }

        public bool GOsSpawned
        {
            get { return this.m_gosSpawned; }
        }

        public void RemoveNPCSpawnPool(NPCSpawnPoolTemplate templ)
        {
            this.AddMessage((Action) (() => this.RemoveNPCSpawnPoolNow(templ)));
        }

        public void RemoveNPCSpawnPoolNow(NPCSpawnPoolTemplate templ)
        {
            NPCSpawnPool npcSpawnPool;
            if (!this.m_npcSpawnPools.TryGetValue(templ.PoolId, out npcSpawnPool))
                return;
            npcSpawnPool.RemovePoolNow();
        }

        public void RemoveGOSpawnPool(GOSpawnPoolTemplate templ)
        {
            this.AddMessage((Action) (() => this.RemoveGOSpawnPoolNow(templ)));
        }

        public void RemoveGOSpawnPoolNow(GOSpawnPoolTemplate templ)
        {
            GOSpawnPool goSpawnPool;
            if (!this.m_goSpawnPools.TryGetValue(templ.PoolId, out goSpawnPool))
                return;
            goSpawnPool.RemovePoolNow();
        }

        public void AddNPCSpawnPool(NPCSpawnPoolTemplate templ)
        {
            this.AddMessage((Action) (() => this.AddNPCSpawnPoolNow(templ)));
        }

        public NPCSpawnPool AddNPCSpawnPoolNow(NPCSpawnPoolTemplate templ)
        {
            NPCSpawnPool pool = new NPCSpawnPool(this, templ);
            this.AddNPCSpawnPoolNow(pool);
            return pool;
        }

        public void AddNPCSpawnPoolNow(NPCSpawnPool pool)
        {
            NPCSpawnPool npcSpawnPool;
            if (!this.m_npcSpawnPools.TryGetValue(pool.Template.PoolId, out npcSpawnPool))
            {
                this.m_npcSpawnPools.Add(pool.Template.PoolId, pool);
                Map.OnPoolAdded<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>(pool);
            }
            else
                pool = npcSpawnPool;

            if (!this.SpawnPointsEnabled)
                return;
            pool.IsActive = true;
        }

        public void AddGOSpawnPoolLater(GOSpawnPoolTemplate templ)
        {
            this.AddMessage((Action) (() => this.AddGOSpawnPoolNow(templ)));
        }

        public GOSpawnPool AddGOSpawnPoolNow(GOSpawnPoolTemplate templ)
        {
            GOSpawnPool pool = new GOSpawnPool(this, templ);
            this.AddGOSpawnPoolNow(pool);
            return pool;
        }

        public void AddGOSpawnPoolNow(GOSpawnPool pool)
        {
            GOSpawnPool goSpawnPool;
            if (!this.m_goSpawnPools.TryGetValue(pool.Template.PoolId, out goSpawnPool))
            {
                this.m_goSpawnPools.Add(pool.Template.PoolId, pool);
                Map.OnPoolAdded<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>(pool);
            }
            else
                pool = goSpawnPool;

            if (!this.SpawnPointsEnabled)
                return;
            pool.IsActive = true;
        }

        private static void OnPoolAdded<T, E, O, POINT, POOL>(POOL pool)
            where T : SpawnPoolTemplate<T, E, O, POINT, POOL>
            where E : SpawnEntry<T, E, O, POINT, POOL>
            where O : WorldObject
            where POINT : SpawnPoint<T, E, O, POINT, POOL>, new()
            where POOL : SpawnPool<T, E, O, POINT, POOL>
        {
            foreach (POINT spawnPoint in pool.SpawnPoints)
                spawnPoint.SpawnEntry.SpawnPoints.Add(spawnPoint);
        }

        /// <summary>
        /// Called by SpawnPool.
        /// Use SpawnPool.Remove* methods to remove pools.
        /// </summary>
        internal void RemoveSpawnPool<T, E, O, POINT, POOL>(POOL pool) where T : SpawnPoolTemplate<T, E, O, POINT, POOL>
            where E : SpawnEntry<T, E, O, POINT, POOL>
            where O : WorldObject
            where POINT : SpawnPoint<T, E, O, POINT, POOL>, new()
            where POOL : SpawnPool<T, E, O, POINT, POOL>
        {
            if (typeof(O) == typeof(NPC))
            {
                if (this.m_npcSpawnPools.Remove(pool.Template.PoolId))
                    pool.IsActive = false;
            }
            else
            {
                if (!(typeof(O) == typeof(GameObject)))
                    throw new ArgumentException("Invalid Pool type: " + (object) pool);
                if (this.m_goSpawnPools.Remove(pool.Template.PoolId))
                    pool.IsActive = false;
            }

            foreach (POINT spawnPoint in pool.SpawnPoints)
                spawnPoint.SpawnEntry.SpawnPoints.Remove(spawnPoint);
        }

        /// <summary>Adds a message to the Map to clear it</summary>
        public void ClearLater()
        {
            this.AddMessage(new Action(this.RemoveAll));
        }

        /// <summary>Removes all Objects, NPCs and Spawns from this Map.</summary>
        /// <remarks>Requires map context</remarks>
        public virtual void RemoveAll()
        {
            this.RemoveObjects();
        }

        /// <summary>Removes all Objects and NPCs from the Map</summary>
        public void RemoveObjects()
        {
            if (this.IsInContext && !this.IsUpdating)
                this.RemoveObjectsNow();
            else
                this.AddMessage((Action) (() => this.RemoveObjectsNow()));
        }

        private void RemoveObjectsNow()
        {
            foreach (SpawnPool<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool> spawnPool in this
                .m_npcSpawnPools.Values.ToArray<NPCSpawnPool>())
                spawnPool.RemovePoolNow();
            foreach (SpawnPool<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool> spawnPool in
                this.m_goSpawnPools.Values.ToArray<GOSpawnPool>())
                spawnPool.RemovePoolNow();
            foreach (WorldObject copyObject in this.CopyObjects())
            {
                if (!(copyObject is Character) && !copyObject.IsPlayerOwned && !copyObject.IsDeleted)
                    copyObject.DeleteNow();
            }

            this.m_gosSpawned = false;
            this.m_npcsSpawned = false;
        }

        /// <summary>
        /// Clears all Objects and NPCs and spawns the default ones again
        /// </summary>
        public virtual void Reset()
        {
            this.RemoveAll();
            this.SpawnMap();
            foreach (SpawnPool<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool> spawnPool in this
                .m_npcSpawnPools.Values)
                spawnPool.RespawnFull();
        }

        /// <summary>
        /// If not added already, this method adds all default GameObjects and NPC spawnpoints to this map.
        /// </summary>
        public void SpawnMapLater()
        {
            this.AddMessage(new Action(this.SpawnMap));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <see cref="M:WCell.RealmServer.Global.Map.SpawnMapLater" />
        /// <remarks>Requires map context</remarks>
        public virtual void SpawnMap()
        {
            this.EnsureContext();
            if (this.IsSpawned)
                return;
            this.IsSpawning = true;
            if (!this.m_MapTemplate.NotifySpawning(this))
            {
                this.IsSpawning = false;
            }
            else
            {
                this.SpawnPortals();
                if (!this.m_gosSpawned && GOMgr.Loaded)
                {
                    int objectCount = this.ObjectCount;
                    this.SpawnGOs();
                    if (objectCount > 0)
                        Map.s_log.Debug("Added {0} Objects to Map: {1}", (object) (this.ObjectCount - objectCount),
                            (object) this);
                    this.m_gosSpawned = true;
                }

                if (!this.m_npcsSpawned && NPCMgr.Loaded)
                {
                    int count = this.ObjectCount;
                    this.SpawnNPCs();
                    this.AddMessage((Action) (() =>
                    {
                        if (count <= 0)
                            return;
                        Map.s_log.Debug("Added {0} NPC Spawnpoints to Map: {1}", (object) (this.ObjectCount - count),
                            (object) this);
                    }));
                    this.m_npcsSpawned = true;
                }

                this.IsSpawning = false;
                if (!this.IsSpawned)
                    return;
                this.m_MapTemplate.NotifySpawned(this);
            }
        }

        private void SpawnPortals()
        {
            foreach (GOSpawnPoolTemplate spawnPoolTemplatesBy in Asda2PortalMgr.GetSpawnPoolTemplatesByMap(this.MapId))
            {
                if (spawnPoolTemplatesBy.AutoSpawns && this.IsEventActive(spawnPoolTemplatesBy.EventId))
                    this.AddGOSpawnPoolNow(spawnPoolTemplatesBy);
            }
        }

        protected virtual void SpawnGOs()
        {
            List<GOSpawnPoolTemplate> poolTemplatesByMap = GOMgr.GetSpawnPoolTemplatesByMap(this.MapId);
            if (poolTemplatesByMap == null)
                return;
            foreach (GOSpawnPoolTemplate templ in poolTemplatesByMap)
            {
                if (templ.AutoSpawns && this.IsEventActive(templ.EventId))
                    this.AddGOSpawnPoolNow(templ);
            }
        }

        protected virtual void SpawnNPCs()
        {
            List<NPCSpawnPoolTemplate> poolTemplatesByMap = NPCMgr.GetSpawnPoolTemplatesByMap(this.Id);
            if (poolTemplatesByMap == null)
                return;
            foreach (NPCSpawnPoolTemplate templ in poolTemplatesByMap)
            {
                if (templ.AutoSpawns && this.IsEventActive(templ.EventId))
                    this.AddNPCSpawnPoolNow(templ);
            }
        }

        public bool SpawnPointsEnabled
        {
            get { return this.m_SpawnPointsEnabled; }
            set
            {
                if (this.m_SpawnPointsEnabled == value)
                    return;
                this.m_SpawnPointsEnabled = value;
                if (value)
                    this.ForeachSpawnPool((Action<NPCSpawnPool>) (pool => pool.IsActive = true));
                else
                    this.ForeachSpawnPool((Action<NPCSpawnPool>) (pool => pool.Disable()));
            }
        }

        public void ForeachSpawnPool(Action<NPCSpawnPool> func)
        {
            this.ForeachSpawnPool(Vector3.Zero, 0.0f, func);
        }

        public void ForeachSpawnPool(Vector3 pos, float radius, Action<NPCSpawnPool> func)
        {
            float radiusSq = radius * radius;
            foreach (NPCSpawnPool npcSpawnPool in this.m_npcSpawnPools.Values)
            {
                if ((double) radius <= 0.0 || npcSpawnPool.Template.Entries.Any<NPCSpawnEntry>(
                        (Func<NPCSpawnEntry, bool>) (spawn => (double) spawn.GetDistSq(pos) < (double) radiusSq)))
                    func(npcSpawnPool);
            }
        }

        public void RespawnInRadius(Vector3 pos, float radius)
        {
            this.ForeachSpawnPool(pos, radius, (Action<NPCSpawnPool>) (pool => pool.RespawnFull()));
        }

        public void SpawnZone(ZoneId id)
        {
        }

        /// <summary>
        /// The time in seconds when the last
        /// Update started.
        /// </summary>
        public DateTime LastUpdateTime
        {
            get { return this.m_lastUpdateTime; }
        }

        /// <summary>
        /// Adds a new Updatable right away.
        /// Requires Map context.
        /// <see cref="M:WCell.RealmServer.Global.Map.RegisterUpdatableLater(WCell.Core.Timers.IUpdatable)" />
        /// </summary>
        /// <param name="updatable"></param>
        public void RegisterUpdatable(IUpdatable updatable)
        {
            this.EnsureContext();
            this.m_updatables.Add(updatable);
        }

        /// <summary>
        /// Unregisters an Updatable right away.
        /// In map context.
        /// <see cref="M:WCell.RealmServer.Global.Map.UnregisterUpdatableLater(WCell.Core.Timers.IUpdatable)" />
        /// </summary>
        public void UnregisterUpdatable(IUpdatable updatable)
        {
            this.EnsureContext();
            this.m_updatables.Remove(updatable);
        }

        /// <summary>
        /// Registers the given Updatable during the next Map Tick
        /// </summary>
        public void RegisterUpdatableLater(IUpdatable updatable)
        {
            this.m_messageQueue.Enqueue((IMessage) new Message((Action) (() => this.RegisterUpdatable(updatable))));
        }

        /// <summary>
        /// Unregisters the given Updatable during the next Map Update
        /// </summary>
        public void UnregisterUpdatableLater(IUpdatable updatable)
        {
            this.m_messageQueue.Enqueue((IMessage) new Message((Action) (() => this.UnregisterUpdatable(updatable))));
        }

        /// <summary>
        /// Executes the given action after the given delay within this Map's context.
        /// </summary>
        /// <remarks>Make sure that once the timeout is hit, the given action is executed in the correct Map's context.</remarks>
        public TimerEntry CallDelayed(int millis, Action action)
        {
            TimerEntry timer = new TimerEntry();
            timer.Action = (Action<int>) (delay =>
            {
                action();
                this.UnregisterUpdatableLater((IUpdatable) timer);
            });
            timer.Start(millis, 0);
            this.RegisterUpdatableLater((IUpdatable) timer);
            return timer;
        }

        /// <summary>
        /// Executes the given action after the given delay within this Map's context.
        /// </summary>
        /// <remarks>Make sure that once the timeout is hit, the given action is executed in the correct Map's context.</remarks>
        public TimerEntry CallPeriodically(int seconds, Action action)
        {
            TimerEntry timerEntry = new TimerEntry()
            {
                Action = (Action<int>) (delay => action())
            };
            timerEntry.Start(seconds, seconds);
            this.RegisterUpdatableLater((IUpdatable) timerEntry);
            return timerEntry;
        }

        /// <summary>
        /// Adds a message to the message queue for this map.
        /// TODO: Consider extra-message for Character that checks whether Char is still in Map?
        /// </summary>
        /// <param name="action">the action to be enqueued</param>
        public void AddMessage(Action action)
        {
            this.AddMessage((IMessage) (Message) action);
        }

        /// <summary>Adds a message to the message queue for this map.</summary>
        /// <param name="msg">the message</param>
        public void AddMessage(IMessage msg)
        {
            this.Start();
            this.m_messageQueue.Enqueue(msg);
        }

        /// <summary>
        /// Callback for executing updates of the map, which includes updates for all inhabiting objects.
        /// </summary>
        /// <param name="state">the <see cref="T:WCell.RealmServer.Global.Map" /> to update</param>
        private void MapUpdateCallback(object state)
        {
            ++this.totalUpdates;
            if (Interlocked.CompareExchange(ref this.m_currentThreadId, Thread.CurrentThread.ManagedThreadId, 0) != 0)
                return;
            DateTime now = DateTime.Now;
            int milliSecondsInt1 = (now - this.m_lastUpdateTime).ToMilliSecondsInt();
            IMessage message;
            while (this.m_messageQueue.TryDequeue(out message))
            {
                try
                {
                    message.Execute();
                }
                catch (Exception ex)
                {
                    LogUtil.ErrorException(ex, "Exception raised when processing Message.", new object[0]);
                }
            }

            this.m_isUpdating = true;
            foreach (IUpdatable updatable in this.m_updatables)
            {
                try
                {
                    updatable.Update(milliSecondsInt1);
                }
                catch (Exception ex)
                {
                    LogUtil.ErrorException(ex, "Exception raised when updating Updatable: " + (object) updatable,
                        new object[0]);
                    this.UnregisterUpdatableLater(updatable);
                }
            }

            int num1 = 0;
            foreach (WorldObject worldObject in this.m_objects.Values)
            {
                if (!worldObject.IsTeleporting)
                {
                    UpdatePriority updatePriority;
                    if (!worldObject.IsAreaActive && this.DefenceTownEvent == null)
                    {
                        if (this.m_updateInactiveAreas)
                            updatePriority = UpdatePriority.Inactive;
                        else
                            continue;
                    }
                    else
                        updatePriority = worldObject.UpdatePriority;

                    try
                    {
                        int updatePriorityMilli = Map.UpdatePriorityMillis[(int) updatePriority];
                        int milliSecondsInt2 = (now - worldObject.LastUpdateTime).ToMilliSecondsInt();
                        if (milliSecondsInt2 >= updatePriorityMilli)
                        {
                            worldObject.LastUpdateTime = now;
                            int tickCount = Environment.TickCount;
                            worldObject.Update(milliSecondsInt2);
                            ++num1;
                            int num2 = Environment.TickCount - tickCount;
                            if (this.maxObjUpdateTime < num2)
                            {
                                this.maxObjUpdateTime = num2;
                                if (this.totalUpdates > 500)
                                {
                                    if (this.maxObjUpdateTime > 1000)
                                    {
                                        this.totalUpdates = 0;
                                        this.maxObjUpdateTime = 100;
                                        LogUtil.WarnException("Object {0} updates {1} !!!", new object[2]
                                        {
                                            (object) worldObject,
                                            (object) num2
                                        });
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtil.ErrorException(ex, "Exception raised when updating Object: " + (object) worldObject,
                            new object[0]);
                        if (worldObject is Unit)
                        {
                            Unit unit = (Unit) worldObject;
                            if (unit.Brain != null)
                                unit.Brain.IsRunning = false;
                        }

                        Character character = worldObject as Character;
                        if (character != null)
                            character.Logout(true);
                        else
                            worldObject.Delete();
                    }
                }
            }

            if (this.m_tickCount % Map.CharacterUpdateEnvironmentTicks == 0)
                this.UpdateCharacters();
            this.UpdateMap();
            this.m_lastUpdateTime = now;
            ++this.m_tickCount;
            this.m_isUpdating = false;
            TimeSpan time = DateTime.Now - now;
            Map._avgUpdateTime = (float) (((double) Map._avgUpdateTime * 9.0 + time.TotalMilliseconds) / 10.0);
            if ((double) Map._avgUpdateTime > 1000.0)
                LogUtil.WarnException(
                    "---=---end delta :{0}, avg :{1}, objs :{2}, objUpd :{3}, maxUpdTime: {4}, map: {5}", (object) time,
                    (object) Map._avgUpdateTime, (object) this.ObjectCount, (object) num1,
                    (object) this.maxObjUpdateTime, (object) this.Name);
            Interlocked.Exchange(ref this.m_currentThreadId, 0);
            if (this.m_running)
            {
                int millisecondsDelay = this.m_updateDelay - time.ToMilliSecondsInt();
                if (millisecondsDelay < 0)
                    millisecondsDelay = 0;
                Task.Factory.StartNewDelayed(millisecondsDelay, new Action<object>(this.MapUpdateCallback),
                    (object) this);
            }
            else
            {
                if (!this.IsDisposed)
                    return;
                this.Dispose();
            }
        }

        /// <summary>
        /// Can be overridden to add any kind of to be executed every Map-tick
        /// </summary>
        protected virtual void UpdateMap()
        {
        }

        internal void UpdateCharacters()
        {
            HashSet<WorldObject> updatedObjects = WorldObject.WorldObjectSetPool.Obtain();
            int count = this.m_characters.Count;
            for (int index = 0; index < count; ++index)
            {
                Character character = this.m_characters[index];
                try
                {
                    character.UpdateEnvironment(updatedObjects);
                }
                catch (Exception ex)
                {
                    LogUtil.ErrorException(ex, "Exception raised when updating Character {0}", new object[1]
                    {
                        (object) character
                    });
                }
            }

            foreach (ObjectBase objectBase in updatedObjects)
                objectBase.ResetUpdateInfo();
            updatedObjects.Clear();
            WorldObject.WorldObjectSetPool.Recycle(updatedObjects);
        }

        /// <summary>
        /// Instantly updates all active Characters' environment: Collect environment info and send update deltas
        /// </summary>
        public void ForceUpdateCharacters()
        {
            if (this.IsInContext && !this.IsUpdating)
                this.UpdateCharacters();
            else
                this.AddMessageAndWait(new Action(this.UpdateCharacters));
        }

        public void CallOnAllCharacters(Action<Character> action)
        {
            this.ExecuteInContext((Action) (() =>
            {
                foreach (Character character in this.m_characters)
                    action(character);
            }));
        }

        public void CallOnAllNPCs(Action<NPC> action)
        {
            this.ExecuteInContext((Action) (() =>
            {
                foreach (WorldObject worldObject in this.m_objects.Values)
                {
                    if (worldObject is NPC)
                        action((NPC) worldObject);
                }
            }));
        }

        internal void UpdateWorldStates(uint index, int value)
        {
            foreach (Zone mainZone in this.MainZones)
                mainZone.WorldStates.UpdateWorldState(index, value);
        }

        /// <summary>Partitions the space of the zone.</summary>
        private void PartitionSpace()
        {
            this.m_root.PartitionSpace((ZoneSpacePartitionNode) null, ZoneSpacePartitionNode.DefaultPartitionThreshold,
                0);
        }

        /// <summary>Gets the partitioned node in which the point lies.</summary>
        /// <param name="pt">the point to search for</param>
        /// <returns>a <see cref="T:WCell.RealmServer.Global.ZoneSpacePartitionNode" /> if found; null otherwise</returns>
        internal ZoneSpacePartitionNode GetNodeFromPoint(ref Vector3 pt)
        {
            if (this.m_root == null)
                return (ZoneSpacePartitionNode) null;
            return this.m_root.GetLeafFromPoint(ref pt);
        }

        /// <summary>
        /// Checks to see if the supplied location is within this zone's bounds.
        /// </summary>
        /// <param name="point">the point to check for containment</param>
        /// <returns>true if the location is within the bounds, false otherwise</returns>
        public bool IsPointInMap(ref Vector3 point)
        {
            if (this.m_root != null)
                return this.m_root.Bounds.Contains(ref point);
            return false;
        }

        /// <summary>Gets a zone by its area ID.</summary>
        /// <param name="id">the ID of the zone to search for</param>
        /// <returns>the Zone object representing the specified ID; null if the zone was not found</returns>
        public Zone GetZone(ZoneId id)
        {
            Zone zone;
            this.Zones.TryGetValue(id, out zone);
            return zone;
        }

        public Zone GetZone(float x, float y)
        {
            if (this.m_MapTemplate.ZoneTileSet != null)
                return this.GetZone(this.m_MapTemplate.ZoneTileSet.GetZoneId(x, y));
            return (Zone) null;
        }

        /// <summary>
        /// Gets all entities in a radius from the origin, according to the search filter.
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        /// <param name="origin">the point to search from</param>
        /// <param name="radius">the area to check in</param>
        /// <param name="filter">the entities to return</param>
        /// <param name="limit">Max amount of objects to search for</param>
        /// <returns>a linked list of the entities which were found in the search area</returns>
        public IList<WorldObject> GetObjectsInRadius(Vector3 origin, float radius, ObjectTypes filter, uint phase,
            int limit)
        {
            this.EnsureContext();
            List<WorldObject> entities = new List<WorldObject>();
            if (this.m_root != null && (double) radius >= 1.0)
            {
                BoundingSphere sphere = new BoundingSphere(origin, radius);
                this.m_root.GetEntitiesInArea(ref sphere, entities, filter, phase, ref limit);
            }

            return (IList<WorldObject>) entities;
        }

        /// <summary>
        /// Gets all entities in a radius from the origin, according to the search filter.
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        /// <param name="origin">the point to search from</param>
        /// <param name="radius">the area to check in</param>
        /// <param name="limit">Max amount of objects to search for</param>
        /// <returns>a linked list of the entities which were found in the search area</returns>
        public ICollection<T> GetObjectsInRadius<T>(Vector3 origin, float radius, uint phase, int limit = 2147483647)
            where T : WorldObject
        {
            this.EnsureContext();
            List<T> entities = new List<T>();
            if (this.m_root != null && (double) radius >= 1.0)
            {
                BoundingSphere sphere = new BoundingSphere(origin, radius);
                this.m_root.GetEntitiesInArea<T>(ref sphere, entities, phase, ref limit);
            }

            return (ICollection<T>) entities;
        }

        /// <summary>
        /// Gets all entities in a radius from the origin, according to the search filter.
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        /// <param name="origin">the point to search from</param>
        /// <param name="radius">the area to check in</param>
        /// <param name="filter">the entities to return</param>
        /// <param name="limit">Max amount of objects to search for</param>
        /// <returns>a linked list of the entities which were found in the search area</returns>
        public ICollection<T> GetObjectsInRadius<T>(Vector3 origin, float radius, Func<T, bool> filter, uint phase,
            int limit = 2147483647) where T : WorldObject
        {
            this.EnsureContext();
            List<T> entities = new List<T>();
            if (this.m_root != null && (double) radius >= 1.0)
            {
                BoundingSphere sphere = new BoundingSphere(origin, radius);
                this.m_root.GetEntitiesInArea<T>(ref sphere, entities, filter, phase, ref limit);
            }

            return (ICollection<T>) entities;
        }

        /// <summary>
        /// Gets all objects in a radius around the origin, regarding the search filter.
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        /// <param name="origin">the point to search from</param>
        /// <param name="radius">the area to check in</param>
        /// <param name="filter">a delegate to filter search results</param>
        /// <returns>a linked list of the entities which were found in the search area</returns>
        public IList<WorldObject> GetObjectsInRadius(Vector3 origin, float radius, Func<WorldObject, bool> filter,
            uint phase, int limit)
        {
            this.EnsureContext();
            List<WorldObject> entities = new List<WorldObject>();
            if (this.m_root != null && (double) radius >= 1.0)
            {
                BoundingSphere sphere = new BoundingSphere(origin, radius);
                this.m_root.GetEntitiesInArea(ref sphere, entities, filter, phase, ref limit);
            }

            return (IList<WorldObject>) entities;
        }

        /// <summary>
        /// Gets all entities in a radius from the origin, according to the search filter.
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        /// <param name="origin">the point to search from</param>
        /// <param name="box">the BoundingBox object that matches the area we want to search in</param>
        /// <param name="filter">the entities to return</param>
        /// <param name="limit">max amount of objects to search for</param>
        /// <returns>a linked list of the entities which were found in the search area</returns>
        public ICollection<WorldObject> GetObjectsInBox(ref Vector3 origin, BoundingBox box, ObjectTypes filter,
            int limit, uint phase)
        {
            this.EnsureContext();
            List<WorldObject> entities = new List<WorldObject>();
            if (this.m_root != null && !(box.Min == box.Max))
                this.m_root.GetEntitiesInArea(ref box, entities, filter, phase, ref limit);
            return (ICollection<WorldObject>) entities;
        }

        /// <summary>
        /// Gets all entities in a radius from the origin, according to the search filter.
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        /// <param name="origin">the point to search from</param>
        /// <param name="box">the BoundingBox object that matches the area we want to search in</param>
        /// <param name="limit">Max amount of objects to search for</param>
        /// <returns>a linked list of the entities which were found in the search area</returns>
        public ICollection<T> GetObjectsInBox<T>(ref Vector3 origin, BoundingBox box, uint phase, int limit)
            where T : WorldObject
        {
            this.EnsureContext();
            List<T> entities = new List<T>();
            if (this.m_root != null && !(box.Min == box.Max))
                this.m_root.GetEntitiesInArea<T>(ref box, entities, phase, ref limit);
            return (ICollection<T>) entities;
        }

        /// <summary>
        /// Gets all entities in a radius from the origin, according to the search filter.
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        /// <param name="origin">the point to search from</param>
        /// <param name="box">the BoundingBox object that matches the area we want to search in</param>
        /// <param name="filter">the entities to return</param>
        /// <param name="limit">Max amount of objects to search for</param>
        /// <returns>a linked list of the entities which were found in the search area</returns>
        public ICollection<T> GetObjectsInBox<T>(ref Vector3 origin, BoundingBox box, Func<T, bool> filter, uint phase,
            int limit) where T : WorldObject
        {
            this.EnsureContext();
            List<T> entities = new List<T>();
            if (this.m_root != null && !(box.Min == box.Max))
                this.m_root.GetEntitiesInArea<T>(ref box, entities, filter, phase, ref limit);
            return (ICollection<T>) entities;
        }

        /// <summary>
        /// Gets all objects in a radius around the origin, regarding the search filter.
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        /// <param name="origin">the point to search from</param>
        /// <param name="box">the BoundingBox object that matches the area we want to search in</param>
        /// <param name="filter">a delegate to filter search results</param>
        /// <returns>a linked list of the entities which were found in the search area</returns>
        public ICollection<WorldObject> GetObjectsInBox(ref Vector3 origin, BoundingBox box,
            Func<WorldObject, bool> filter, uint phase, int limit)
        {
            this.EnsureContext();
            List<WorldObject> entities = new List<WorldObject>();
            if (this.m_root != null && !(box.Min == box.Max))
                this.m_root.GetEntitiesInArea(ref box, entities, filter, phase, ref limit);
            return (ICollection<WorldObject>) entities;
        }

        /// <summary>
        /// Iterates over all objects in the given radius around the given origin.
        /// If the predicate returns false, iteration is cancelled and false is returned.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="radius"></param>
        /// <param name="predicate">Returns whether to continue iteration.</param>
        /// <returns>Whether Iteration was not cancelled (usually indicating that we did not find what we were looking for).</returns>
        public bool IterateObjects(Vector3 origin, float radius, uint phase, Func<WorldObject, bool> predicate)
        {
            this.EnsureContext();
            BoundingSphere sphere = new BoundingSphere(origin, radius);
            return this.m_root.Iterate(ref sphere, predicate, phase);
        }

        /// <summary>Sends a packet to all nearby characters.</summary>
        /// <param name="packet">the packet to send</param>
        /// <param name="includeSelf">whether or not to send the packet to ourselves (if we're a character)</param>
        public void SendPacketToArea(RealmPacketOut packet, ref Vector3 center, uint phase)
        {
            this.IterateObjects(center, WorldObject.BroadcastRange, phase, (Func<WorldObject, bool>) (obj =>
            {
                if (obj is NPC && ((Unit) obj).Charmer != null && ((Unit) obj).Charmer is Character)
                    ((Character) ((Unit) obj).Charmer).Send(packet.GetFinalizedPacket());
                if (obj is Character)
                    ((Character) obj).Send(packet.GetFinalizedPacket());
                return true;
            }));
        }

        /// <summary>Sends a packet to all characters in the map</summary>
        /// <param name="packet">the packet to send</param>
        public void SendPacketToMap(RealmPacketOut packet)
        {
            this.CallOnAllCharacters((Action<Character>) (chr => chr.Send(packet.GetFinalizedPacket())));
        }

        /// <summary>Removes an entity from the zone.</summary>
        /// <param name="obj">the entity to remove</param>
        /// <returns>true if the entity was removed; false otherwise</returns>
        public bool RemoveObjectLater(WorldObject obj)
        {
            if (!this.m_objects.ContainsKey(obj.EntityId))
                return false;
            this.m_messageQueue.Enqueue(Map.GetRemoveObjectTask(obj, this));
            return true;
        }

        public void AddObject(WorldObject obj)
        {
            if (this.m_isUpdating || !this.IsInContext)
                this.AddObjectLater(obj);
            else
                this.AddObjectNow(obj);
        }

        public void AddObject(WorldObject obj, Vector3 pos)
        {
            if (this.m_isUpdating || !this.IsInContext)
                this.TransferObjectLater(obj, pos);
            else
                this.AddObjectNow(obj, ref pos);
        }

        public void AddObject(WorldObject obj, ref Vector3 pos)
        {
            if (this.m_isUpdating || !this.IsInContext)
                this.TransferObjectLater(obj, pos);
            else
                this.AddObjectNow(obj, ref pos);
        }

        /// <summary>Adds the given Object to this Map.</summary>
        /// <remarks>Requires map context.</remarks>
        public void AddObjectNow(WorldObject obj, Vector3 pos)
        {
            obj.Position = pos;
            this.AddObjectNow(obj);
        }

        /// <summary>Adds the given Object to this Map.</summary>
        /// <remarks>Requires map context.</remarks>
        public void AddObjectNow(WorldObject obj, ref Vector3 pos)
        {
            obj.Position = pos;
            this.AddObjectNow(obj);
        }

        public Dictionary<Locale, List<Character>> CharactersByLocale
        {
            get
            {
                this.EnsureContext();
                return this._charactersByLocale;
            }
        }

        /// <summary>Adds the given Object to this Map.</summary>
        /// <remarks>Requires map context.</remarks>
        public void AddObjectNow(WorldObject obj)
        {
            try
            {
                this.EnsureNotUpdating();
                this.EnsureContext();
                if (this.IsDisposed)
                {
                    if (!(obj is Character))
                        obj.Delete();
                    else
                        ((Character) obj).TeleportToBindLocation();
                }
                else if (obj.IsDeleted)
                    Map.s_log.Warn("Tried to add deleted object \"{0}\" to Map: " + (object) this);
                else if (!this.m_root.AddObject(obj))
                {
                    Map.s_log.Error("Could not add Object to Map {0} at {1} (Map Bounds: {2})", (object) this,
                        (object) obj.Position, (object) this.Bounds);
                    if (!(obj is Character))
                        obj.Delete();
                    else
                        ((Character) obj).TeleportToBindLocation();
                }
                else
                {
                    obj.Map = this;
                    this.m_objects.Add(obj.EntityId, obj);
                    Interlocked.Increment(ref Map.UniqObjetIdOnMapInterator);
                    obj.UniqWorldEntityId = Map.UniqObjetIdOnMapInterator;
                    if (obj is Unit)
                    {
                        if (obj is Character)
                        {
                            Character chr = (Character) obj;
                            this.m_characters.Add(chr);
                            this.CharactersByLocale[chr.Client.Locale].Add(chr);
                            if (chr.Role.Status == RoleStatus.Player)
                                this.IncreasePlayerCount(chr);
                            this.OnEnter(chr);
                        }
                        else if (obj is NPC)
                        {
                            NPC npc = (NPC) obj;
                            if (npc.IsSpiritHealer)
                                this.m_spiritHealers.Add(npc);
                            npc.UniqIdOnMap = this.AvalibleUniqNpcIds[0];
                            this._asda2Npcs.Add(npc.UniqIdOnMap, (WorldObject) npc);
                            this.AvalibleUniqNpcIds.RemoveAt(0);
                        }
                    }
                    else if (obj is GameObject)
                    {
                        obj.UniqIdOnMap = this.AvalibleUniqNpcIds[0];
                        this._asda2Npcs.Add(obj.UniqIdOnMap, obj);
                        this.AvalibleUniqNpcIds.RemoveAt(0);
                    }

                    if (this.MainZoneCount == 1)
                    {
                        obj.SetZone(this.m_defaultZone);
                    }
                    else
                    {
                        int mainZoneCount = this.MainZoneCount;
                    }

                    obj.OnEnterMap();
                    obj.RequestUpdate();
                    if (!(obj is Character))
                        return;
                    this.m_MapTemplate.NotifyPlayerEntered(this, (Character) obj);
                }
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex, "Unable to add Object \"{0}\" to Map: {1}", new object[2]
                {
                    (object) obj,
                    (object) this
                });
                if (obj is Character)
                {
                    Character character = obj as Character;
                    if (character.Client != null)
                        character.Client.Disconnect(false);
                    character.Logout(true);
                }
                else
                    obj.DeleteNow();
            }
        }

        private List<ushort> AvalibleUniqNpcIds
        {
            get
            {
                if (this._avalibleUniqNpcIds == null)
                {
                    this._avalibleUniqNpcIds = new List<ushort>((int) short.MaxValue);
                    for (ushort index = 0; index < ushort.MaxValue; ++index)
                        this._avalibleUniqNpcIds.Add(index);
                }

                return this._avalibleUniqNpcIds;
            }
        }

        internal void IncreasePlayerCount(Character chr)
        {
            if (chr.Faction.IsHorde)
                ++this.m_hordeCount;
            else
                ++this.m_allyCount;
        }

        internal void DecreasePlayerCount(Character chr)
        {
            if (chr.Faction.IsHorde)
                --this.m_hordeCount;
            else
                --this.m_allyCount;
        }

        /// <summary>
        /// Gets a WorldObject by its ID.
        /// Requires map context.
        /// </summary>
        /// <param name="id">the ID of the WorldObject</param>
        /// <returns>The corresponding <see cref="T:WCell.RealmServer.Entities.WorldObject" /> if found; null otherwise</returns>
        public WorldObject GetObject(EntityId id)
        {
            WorldObject worldObject;
            this.m_objects.TryGetValue(id, out worldObject);
            return worldObject;
        }

        /// <summary>Called when a Character object is added to the Map</summary>
        protected virtual void OnEnter(Character chr)
        {
        }

        /// <summary>
        /// Called when a Character object is removed from the Map
        /// </summary>
        protected virtual void OnLeave(Character chr)
        {
        }

        public virtual void OnSpawnedCorpse(Character chr)
        {
            chr.TeleportToNearestGraveyard();
        }

        public void RemoveObject(WorldObject obj)
        {
            if (this.m_isUpdating || !this.IsInContext)
                this.RemoveObjectLater(obj);
            else
                this.RemoveObjectNow(obj);
        }

        public NPC GetNpcByUniqMapId(ushort id)
        {
            return (this._asda2Npcs.ContainsKey(id) ? this._asda2Npcs[id] : (WorldObject) null) as NPC;
        }

        /// <summary>
        /// Removes an entity from this Map, instantly.
        /// Also make sure that the given Object is actually within this Map.
        /// Requires map context.
        /// </summary>
        /// <param name="obj">the entity to remove</param>
        /// <returns>true if the entity was removed; false otherwise</returns>
        public void RemoveObjectNow(WorldObject obj)
        {
            this.EnsureContext();
            this.EnsureNotUpdating();
            obj.EnsureContext();
            if (obj is Character)
            {
                Character chr = (Character) obj;
                if (this.m_characters.Remove(chr))
                {
                    if (chr.Role.Status == RoleStatus.Player)
                        this.DecreasePlayerCount(chr);
                    this.OnLeave(chr);
                }

                this.CharactersByLocale[chr.Client.Locale].Remove(chr);
                this.m_MapTemplate.NotifyPlayerLeft(this, (Character) obj);
            }

            NPC npc = obj as NPC;
            if (npc != null)
            {
                this.AvalibleUniqNpcIds.Add(npc.UniqIdOnMap);
                this._asda2Npcs.Remove(npc.UniqIdOnMap);
            }
            else
            {
                Asda2Loot asda2Loot = obj as Asda2Loot;
                if (asda2Loot != null)
                {
                    foreach (Asda2LootItem asda2LootItem in asda2Loot.Items)
                        GlobalHandler.SendRemoveItemResponse(asda2LootItem);
                }
            }

            obj.OnLeavingMap();
            if (obj.Node == null || !obj.Node.RemoveObject(obj))
                return;
            this.m_objects.Remove(obj.EntityId);
            if (obj is Character)
            {
                this.m_characters.Remove((Character) obj);
            }
            else
            {
                if (!(obj is NPC) || !((Unit) obj).IsSpiritHealer)
                    return;
                this.m_spiritHealers.Remove((NPC) obj);
            }
        }

        /// <summary>Returns all objects within the Map.</summary>
        /// <remarks>Requires map context.</remarks>
        /// <returns>an array of all objects in the map</returns>
        public WorldObject[] CopyObjects()
        {
            this.EnsureContext();
            return this.m_objects.Values.ToArray<WorldObject>();
        }

        public virtual bool CanEnter(Character chr)
        {
            if (chr.Level < this.MinLevel || this.MaxLevel != 0 && chr.Level > this.MaxLevel ||
                !this.m_MapTemplate.MayEnter(chr))
                return false;
            if (this.MaxPlayerCount != 0)
                return this.PlayerCount < this.MaxPlayerCount;
            return true;
        }

        public virtual void TeleportOutside(Character chr)
        {
            chr.TeleportToBindLocation();
        }

        /// <summary>
        /// Returns the specified GameObject that is closest to the given point.
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        /// <returns>the closest GameObject to the given point, or null if none found.</returns>
        public GameObject GetNearestGameObject(Vector3 pos, GOEntryId goId, uint phase = 1)
        {
            this.EnsureContext();
            GameObject closest = (GameObject) null;
            float distanceSq = (float) int.MaxValue;
            this.IterateObjects(pos, WorldObject.BroadcastRange, phase, (Func<WorldObject, bool>) (obj =>
            {
                if (obj is GameObject && ((GameObject) obj).Entry.GOId == goId)
                {
                    float distanceSq1 = obj.GetDistanceSq(ref pos);
                    if ((double) distanceSq1 < (double) distanceSq)
                    {
                        distanceSq = distanceSq1;
                        closest = (GameObject) obj;
                    }
                }

                return true;
            }));
            return closest;
        }

        /// <summary>
        /// Returns the specified NPC that is closest to the given point.
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        /// <returns>the closest NPC to the given point, or null if none found.</returns>
        public NPC GetNearestNPC(ref Vector3 pos, NPCId id)
        {
            this.EnsureContext();
            NPC npc = (NPC) null;
            float num = (float) int.MaxValue;
            foreach (WorldObject worldObject in this.m_objects.Values)
            {
                if (!(worldObject is NPC) || ((NPC) worldObject).Entry.NPCId != id)
                {
                    float distanceSq = worldObject.GetDistanceSq(ref pos);
                    if ((double) distanceSq < (double) num)
                    {
                        num = distanceSq;
                        npc = (NPC) worldObject;
                    }
                }
            }

            return npc;
        }

        /// <summary>
        /// Returns the spirit healer that is closest to the given point.
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        /// <returns>the closest spirit healer, or null if none found.</returns>
        public virtual NPC GetNearestSpiritHealer(ref Vector3 pos)
        {
            this.EnsureContext();
            NPC npc = (NPC) null;
            int num = int.MaxValue;
            foreach (NPC spiritHealer in this.m_spiritHealers)
            {
                int distanceSqInt = spiritHealer.GetDistanceSqInt(ref pos);
                if (distanceSqInt < num)
                {
                    num = distanceSqInt;
                    npc = spiritHealer;
                }
            }

            return npc;
        }

        /// <summary>
        /// Gets the first GO with the given SpellFocus within the given radius around the given position.
        /// </summary>
        public GameObject GetGOWithSpellFocus(Vector3 pos, SpellFocus focus, float radius, uint phase)
        {
            foreach (GameObject objectsInRadiu in (IEnumerable<WorldObject>) this.GetObjectsInRadius(pos, radius,
                ObjectTypes.GameObject, phase, 0))
            {
                if (objectsInRadiu.Entry is GOSpellFocusEntry &&
                    ((GOSpellFocusEntry) objectsInRadiu.Entry).SpellFocus == focus)
                    return objectsInRadiu;
            }

            return (GameObject) null;
        }

        /// <summary>
        /// Enqueues an object to be moved into this Map during the next Map-update
        /// </summary>
        /// <param name="obj">the entity to add</param>
        /// <returns>true if the entity was added, false otherwise</returns>
        protected internal bool AddObjectLater(WorldObject obj)
        {
            this.Start();
            this.m_messageQueue.Enqueue((IMessage) new Message((Action) (() => this.AddObjectNow(obj))));
            return true;
        }

        /// <summary>
        /// Enqueues an object to be removed from its old map (if already added to one) and
        /// moved into this one during the next Map-updates of the maps involved.
        /// </summary>
        /// <param name="obj">the object to add</param>
        /// <returns>true if the entity was added, false otherwise</returns>
        internal bool TransferObjectLater(WorldObject obj, Vector3 newPos)
        {
            if (obj.IsTeleporting)
                return true;
            if (obj.Map == this)
            {
                this.MoveObject(obj, ref newPos);
            }
            else
            {
                obj.IsTeleporting = true;
                Map oldMap = obj.Map;
                if (oldMap != null)
                {
                    Message message1 = new Message((Action) (() =>
                    {
                        oldMap.RemoveObjectNow(obj);
                        obj.Map = this;
                        Message message = new Message((Action) (() => obj.IsTeleporting = false));
                        obj.Position = newPos;
                        Character character = obj as Character;
                        if (character != null)
                        {
                            character.IsAsda2Teleporting = true;
                            GlobalHandler.SendTeleportedByCristalResponse(character.Client, this.Id,
                                (short) ((double) newPos.X - (double) this.Offset),
                                (short) ((double) newPos.Y - (double) this.Offset), TeleportByCristalStaus.Ok);
                        }

                        this.AddMessage((IMessage) message);
                    }));
                    oldMap.AddMessage((IMessage) message1);
                }
                else
                    this.AddMessage((IMessage) new Message((Action) (() =>
                    {
                        obj.IsTeleporting = false;
                        obj.Position = newPos;
                        if (!(obj is NPC))
                            return;
                        this.AddObjectNow(obj);
                    })));
            }

            return true;
        }

        public bool MoveObject(WorldObject obj, Vector3 newPos)
        {
            return this.MoveObject(obj, ref newPos);
        }

        /// <summary>
        /// Moves the given object to the given position (does not animate movement)
        /// </summary>
        /// <param name="newPos">the position to move the entity to</param>
        /// <param name="obj">the entity to move</param>
        /// <returns>true if the entity was moved, false otherwise</returns>
        public bool MoveObject(WorldObject obj, ref Vector3 newPos)
        {
            if (this.m_root == null || !this.m_root.Bounds.Contains(ref newPos))
                return false;
            ZoneSpacePartitionNode leafFromPoint = this.m_root.GetLeafFromPoint(ref newPos);
            if (leafFromPoint == null)
                return false;
            ZoneSpacePartitionNode node = obj.Node;
            if (node == null)
                return false;
            Map.MoveObject(obj, ref newPos, leafFromPoint, node);
            if (obj is Unit)
                ((Unit) obj).OnMove();
            return true;
        }

        private static void MoveObject(WorldObject obj, ref Vector3 newPos, ZoneSpacePartitionNode newNode,
            ZoneSpacePartitionNode curNode)
        {
            if (newNode != curNode)
            {
                curNode.RemoveObject(obj);
                if (!newNode.AddObject(obj))
                    return;
                obj.Position = newPos;
            }
            else
                obj.Position = newPos;
        }

        /// <summary>Checks if the event is currently active</summary>
        /// <param name="eventId">Id of the event to check</param>
        /// <returns></returns>
        public bool IsEventActive(uint eventId)
        {
            return WorldEventMgr.IsEventActive(eventId);
        }

        /// <summary>Enumerates all objects within the map.</summary>
        /// <remarks>Requires map context.</remarks>
        public IEnumerator<WorldObject> GetEnumerator()
        {
            this.EnsureContext();
            return (IEnumerator<WorldObject>) this.m_objects.Values.GetEnumerator();
        }

        /// <summary>
        /// Adds the given message to the map's message queue and does not return
        /// until the message is processed.
        /// </summary>
        /// <remarks>Make sure that the map is running before calling this method.</remarks>
        /// <remarks>Must not be called from the map context.</remarks>
        public void AddMessageAndWait(Action action)
        {
            this.AddMessageAndWait((IMessage) new Message(action));
        }

        /// <summary>
        /// Adds the given message to the map's message queue and does not return
        /// until the message is processed.
        /// </summary>
        /// <remarks>Make sure that the map is running before calling this method.</remarks>
        /// <remarks>Must not be called from the map context.</remarks>
        public void AddMessageAndWait(IMessage msg)
        {
            this.EnsureNoContext();
            this.Start();
            SimpleUpdatable updatable = new SimpleUpdatable();
            updatable.Callback = (Action) (() => this.AddMessage((IMessage) new Message((Action) (() =>
            {
                msg.Execute();
                lock (msg)
                    Monitor.PulseAll((object) msg);
                this.UnregisterUpdatable((IUpdatable) updatable);
            }))));
            lock (msg)
            {
                this.RegisterUpdatableLater((IUpdatable) updatable);
                Monitor.Wait((object) msg);
            }
        }

        /// <summary>Waits for one map tick before returning.</summary>
        /// <remarks>Must not be called from the map context.</remarks>
        public void WaitOneTick()
        {
            this.EnsureNoContext();
            this.AddMessageAndWait((IMessage) new Message((Action) (() => { })));
        }

        /// <summary>
        /// Waits for the given amount of ticks.
        /// One tick might take 0 until Map.UpdateSpeed milliseconds.
        /// </summary>
        /// <remarks>Make sure that the map is running before calling this method.</remarks>
        /// <remarks>Must not be called from the map context.</remarks>
        public void WaitTicks(int ticks)
        {
            this.EnsureNoContext();
            for (int index = 0; index < ticks; ++index)
                this.WaitOneTick();
        }

        public override string ToString()
        {
            return string.Format("{0} (Id: {1}{2}, Players: {3}{4} (Alliance: {5}, Horde: {6}))", (object) this.Name,
                (object) this.Id, this.InstanceId != 0U ? (object) (", #" + (object) this.InstanceId) : (object) "",
                (object) this.PlayerCount,
                this.MaxPlayerCount > 0 ? (object) (" / " + (object) this.MaxPlayerCount) : (object) "",
                (object) this.m_allyCount, (object) this.m_hordeCount);
        }

        /// <summary>
        /// Indicates whether this Map is disposed.
        /// Disposed Maps may not be used any longer.
        /// </summary>
        public bool IsDisposed
        {
            get { return this.m_IsDisposed; }
            protected set
            {
                this.m_IsDisposed = value;
                this.IsRunning = false;
            }
        }

        public bool IsAsda2FightingMap
        {
            get { return this.MapTemplate.IsAsda2FightingMap; }
        }

        public DeffenceTownEvent DefenceTownEvent { get; set; }

        protected virtual void Dispose()
        {
            this.m_running = false;
            this.m_root = (ZoneSpacePartitionNode) null;
            this.m_objects = (Dictionary<EntityId, WorldObject>) null;
            this.m_characters = (List<Character>) null;
            this.m_spiritHealers = (List<NPC>) null;
            this.m_updatables = (List<IUpdatable>) null;
        }

        /// <summary>Sends the given message to everyone</summary>
        public void SendMessage(string message)
        {
            this.ExecuteInContext((Action) (() =>
            {
                this.m_characters.SendSystemMessage(message);
                ChatMgr.ChatNotify((IChatter) null, message, ChatLanguage.Universal, ChatMsgType.System,
                    (IGenericChatTarget) this);
            }));
        }

        /// <summary>Sends the given message to everyone</summary>
        public void SendMessage(string message, params object[] args)
        {
            this.SendMessage(string.Format(message, args));
        }

        /// <summary>Is called whenever a Character dies.</summary>
        /// <param name="action"></param>
        protected internal virtual void OnPlayerDeath(IDamageAction action)
        {
            if (action.Attacker != null && action.Attacker.IsPvPing && action.Victim.YieldsXpOrHonor)
            {
                ((Character) action.Attacker).OnHonorableKill(action);
                this.OnHonorableKill(action);
            }

            if (!(action.Victim is Character))
                return;
            Character victim = action.Victim as Character;
            victim.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.DeathAtMap, (uint) this.MapId,
                1U, (Unit) null);
            if (action.Attacker == null)
                return;
            if (action.Attacker is Character)
                victim.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KilledByPlayer,
                    (uint) action.Attacker.FactionGroup, 1U, (Unit) null);
            else
                victim.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KilledByCreature,
                    action.Attacker.EntryId, 1U, (Unit) null);
        }

        /// <summary>
        /// Is called whenever an honorable character was killed by another character
        /// </summary>
        /// <param name="action"></param>
        protected internal virtual void OnHonorableKill(IDamageAction action)
        {
        }

        /// <summary>Is called whenever an NPC dies</summary>
        protected internal virtual void OnNPCDied(NPC npc)
        {
            if (npc == null || !npc.YieldsXpOrHonor)
                return;
            Character character = npc.FirstAttacker != null ? npc.FirstAttacker.PlayerOwner : (Character) null;
            if (character == null || this.XpCalculator == null)
                return;
            Character killer = character;
            int num1 = this.XpCalculator(character.Level, npc);
            float num2 = 0.0f;
            if (killer.IsInGroup)
            {
                foreach (GroupMember groupMember in killer.Group)
                {
                    if (groupMember.Character != null)
                        num2 += groupMember.Character.Asda2ExpAmountBoost - 1f;
                }
            }
            else
                num2 += killer.Asda2ExpAmountBoost - 1f;

            int num3 = (int) ((double) num1 * (1.0 + (double) num2));
            if (num3 < 0)
                num3 = 1;
            XpGenerator.CombatXpDistributer(killer, (INamed) npc, num3);
            if (killer.Group == null)
                return;
            killer.Group.OnKill(killer, npc);
        }

        public virtual void Save()
        {
        }

        public void SpawnLoot(Asda2Loot loot)
        {
            if (!(loot.Lootable is Unit) || loot == null || loot.Lootable == null)
                return;
            loot.GiveMoney();
            if (loot.Items == null || loot.Items.Length == 0)
            {
                loot.Dispose();
            }
            else
            {
                bool flag = false;
                if (loot.Looters != null)
                {
                    foreach (Asda2LooterEntry looter in (IEnumerable<Asda2LooterEntry>) loot.Looters)
                    {
                        if (looter != null && looter.Owner != null && looter.Owner.Level >=
                            ((Unit) loot.Lootable).Level + CharacterFormulas.MaxLvlMobCharDiff)
                        {
                            foreach (Asda2LootItem asda2LootItem in loot.Items)
                            {
                                if (asda2LootItem != null)
                                    asda2LootItem.Taken = true;
                            }

                            loot.Dispose();
                            return;
                        }
                    }
                }

                if (loot.AutoLoot)
                    flag = loot.GiveItems();
                if (flag)
                    return;
                this.DropLoot(loot);
            }
        }

        private void DropLoot(Asda2Loot loot)
        {
            if (this.Loots == null)
                throw new Exception("Loots is null!");
            Unit lootable = (Unit) loot.Lootable;
            loot.Position = lootable.Position;
            loot.Map = lootable.Map;
            int length = loot.Items.Length;
            this.Loots.Add(loot);
            loot.LootPositions = this.FindFreeLootSlots(length, loot.Asda2Position.XY);
            if (loot.LootPositions == null)
            {
                LogUtil.WarnException("LootPosition is null {0}, {1}", new object[2]
                {
                    (object) loot.Asda2Position.XY,
                    (object) this.Name
                });
                loot.Dispose();
            }
            else
            {
                for (int index = 0; index < loot.Items.Length; ++index)
                {
                    Vector2Short lootPosition = loot.LootPositions[index];
                    loot.Items[index].Position = lootPosition;
                    this._lootPositions[(int) lootPosition.X, (int) lootPosition.Y] = loot.Items[index];
                }

                this.AddObjectLater((WorldObject) loot);
            }
        }

        private Vector2Short[] FindFreeLootSlots(int itemsCount, Vector2 xy)
        {
            Vector2Short[] vector2ShortArray = new Vector2Short[itemsCount];
            short num = 0;
            Vector2Short init = new Vector2Short((short) xy.X, (short) xy.Y);
            if (this._lootPositions[(int) init.X, (int) init.Y] == null)
            {
                vector2ShortArray[(int) num] = init;
                ++num;
                if ((int) num >= itemsCount)
                    return vector2ShortArray;
            }

            short offset = 1;
            while (true)
            {
                foreach (Vector2Short vector2Short in this.FindFreePositionsWithOffset(init, offset,
                    (short) (vector2ShortArray.Length - (int) num)))
                    vector2ShortArray[(int) num++] = vector2Short;
                if ((int) num < vector2ShortArray.Length && offset <= (short) 10)
                    ++offset;
                else
                    break;
            }

            return vector2ShortArray;
        }

        private IEnumerable<Vector2Short> FindFreePositionsWithOffset(Vector2Short init, short offset, short count)
        {
            try
            {
                List<Vector2Short> source = new List<Vector2Short>(8);
                int count1 = 0;
                if (this._lootPositions[(int) init.X + (int) offset, (int) init.Y] == null)
                {
                    source.Add(new Vector2Short((short) ((int) init.X + (int) offset), init.Y));
                    ++count1;
                    if (count1 == (int) count)
                        return (IEnumerable<Vector2Short>) source;
                }

                if (this._lootPositions[(int) init.X - (int) offset, (int) init.Y] == null)
                {
                    source.Add(new Vector2Short((short) ((int) init.X - (int) offset), init.Y));
                    ++count1;
                    if (count1 == (int) count)
                        return (IEnumerable<Vector2Short>) source;
                }

                if (this._lootPositions[(int) init.X + (int) offset, (int) init.Y + (int) offset] == null)
                {
                    source.Add(new Vector2Short((short) ((int) init.X + (int) offset),
                        (short) ((int) init.Y + (int) offset)));
                    ++count1;
                    if (count1 == (int) count)
                        return (IEnumerable<Vector2Short>) source;
                }

                if (this._lootPositions[(int) init.X - (int) offset, (int) init.Y + (int) offset] == null)
                {
                    source.Add(new Vector2Short((short) ((int) init.X - (int) offset),
                        (short) ((int) init.Y + (int) offset)));
                    ++count1;
                    if (count1 == (int) count)
                        return (IEnumerable<Vector2Short>) source;
                }

                if (this._lootPositions[(int) init.X + (int) offset, (int) init.Y - (int) offset] == null)
                {
                    source.Add(new Vector2Short((short) ((int) init.X + (int) offset),
                        (short) ((int) init.Y - (int) offset)));
                    ++count1;
                    if (count1 == (int) count)
                        return (IEnumerable<Vector2Short>) source;
                }

                if (this._lootPositions[(int) init.X - (int) offset, (int) init.Y - (int) offset] == null)
                {
                    source.Add(new Vector2Short((short) ((int) init.X - (int) offset),
                        (short) ((int) init.Y - (int) offset)));
                    ++count1;
                    if (count1 == (int) count)
                        return (IEnumerable<Vector2Short>) source;
                }

                if (this._lootPositions[(int) init.X, (int) init.Y + (int) offset] == null)
                {
                    source.Add(new Vector2Short(init.X, (short) ((int) init.Y + (int) offset)));
                    ++count1;
                    if (count1 == (int) count)
                        return (IEnumerable<Vector2Short>) source;
                }

                if (this._lootPositions[(int) init.X - (int) offset, (int) init.Y + (int) offset] == null)
                {
                    source.Add(new Vector2Short(init.X, (short) ((int) init.Y - (int) offset)));
                    ++count1;
                    if (count1 == (int) count)
                        return (IEnumerable<Vector2Short>) source;
                }

                return (IEnumerable<Vector2Short>) source.Take<Vector2Short>(count1).ToArray<Vector2Short>();
            }
            catch (IndexOutOfRangeException ex)
            {
                List<Vector2Short> vector2ShortList = new List<Vector2Short>((int) count);
                for (int index = 0; index < (int) count; ++index)
                    vector2ShortList.Add(init);
                return (IEnumerable<Vector2Short>) vector2ShortList;
            }
        }

        public Asda2LootItem TryPickUpItem(short x, short y)
        {
            if ((int) x > this._lootPositions.Length || (int) y > this._lootPositions.Length)
                return (Asda2LootItem) null;
            return this._lootPositions[(int) x, (int) y] ?? (Asda2LootItem) null;
        }

        public void ClearLootSlot(short x, short y)
        {
            Asda2LootItem lootPosition = this._lootPositions[(int) x, (int) y];
            if (lootPosition == null)
                return;
            lootPosition.Taken = true;
            this._lootPositions[(int) x, (int) y] = (Asda2LootItem) null;
        }

        public static IMessage GetInitializeCharacterTask(Character chr, Map rgn)
        {
            return (IMessage) new Message2<Character, Map>()
            {
                Parameter1 = chr,
                Parameter2 = rgn,
                Callback = (Action<Character, Map>) ((initChr, initRgn) => initRgn.AddObjectNow((WorldObject) chr))
            };
        }

        public static IMessage GetRemoveObjectTask(WorldObject obj, Map rgn)
        {
            return (IMessage) new Message2<WorldObject, Map>()
            {
                Parameter1 = obj,
                Parameter2 = rgn,
                Callback = (Action<WorldObject, Map>) ((worldObj, objRgn) => objRgn.RemoveObjectNow(worldObj))
            };
        }

        public float Offset { get; set; }
    }
}