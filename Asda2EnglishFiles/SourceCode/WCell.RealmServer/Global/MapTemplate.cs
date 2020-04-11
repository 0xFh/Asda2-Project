using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.World;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Misc;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
    /// <summary>The template of a Map</summary>
    [Serializable]
    public class MapTemplate : IDataHolder, IComparable
    {
        [NotPersistent] public Vector3[] EntrancePositions = new Vector3[0];
        [NotPersistent] public readonly IList<ZoneTemplate> ZoneInfos = (IList<ZoneTemplate>) new List<ZoneTemplate>();
        public MapId Id;
        public string Name;
        public bool HasTwoSides;
        public uint LoadScreen;
        public MapId ParentMapId;
        public MapType Type;
        public int MinLevel;
        public int MaxLevel;

        /// <summary>
        /// Maximum amount of players allowed in the map.
        /// See Difficulties for more information.
        /// </summary>
        [NotPersistent] public int MaxPlayerCount;

        public Vector3 RepopPosition;
        public MapId RepopMapId;
        public uint AreaTableId;
        public string HordeText;
        public string AllianceText;
        public int HeroicLevelDiff;
        public uint RequiredQuestId;
        public uint RequiredItemId;
        public ClientId RequiredClientId;
        public int DefaultResetTime;

        /// <summary>
        /// The default InstanceTemplate, associated with this MapInfo
        /// </summary>
        [NotPersistent] public InstanceTemplate InstanceTemplate;

        /// <summary>The BoundingBox around the entire Map</summary>
        [NotPersistent] public BoundingBox Bounds;

        [NotPersistent] public MapDifficultyEntry[] Difficulties;
        private IdQueue Ids;
        [NotPersistent] public ZoneTileSet ZoneTileSet;

        /// <summary>
        /// Called when a new Map of this MapInfo has been created.
        /// </summary>
        public event Action<Map> Created;

        /// <summary>Called after some Map has started</summary>
        public event Action<Map> Started;

        /// <summary>
        /// Called when this Map is about to add default spawn points.
        /// The returned value determines whether spawning should commence or be skipped.
        /// </summary>
        public event Func<Map, bool> Spawning;

        /// <summary>
        /// Called when this Map added all default spawns and objects.
        /// </summary>
        public event Action<Map> Spawned;

        /// <summary>
        /// Called when this Map is about to stop.
        /// The returned value determines whether the Map should stop.
        /// </summary>
        public event Func<Map, bool> Stopping;

        /// <summary>Called when the given Character enters this Map.</summary>
        public event Action<Map, Character> PlayerEntered;

        /// <summary>
        /// Called when the given Character left this Map (Character might already be logging off).
        /// </summary>
        public event Action<Map, Character> PlayerLeft;

        /// <summary>
        /// Is called before a Character dies.
        /// If false is returned, the Character won't die.
        /// </summary>
        public static event Func<Character, bool> PlayerBeforeDeath;

        /// <summary>Is called when the Target of the given action died</summary>
        public static event Action<IDamageAction> PlayerDied;

        /// <summary>
        /// Is called when the given Character has been resurrected
        /// </summary>
        public static event Action<Character> PlayerResurrected;

        /// <summary>
        /// The default BattlegroundTemplate, associated with this MapTemplate
        /// </summary>
        public BattlegroundTemplate BattlegroundTemplate { get; internal set; }

        public MapDifficultyEntry GetDifficulty(uint index)
        {
            return this.Difficulties.Get<MapDifficultyEntry>(index) ?? this.Difficulties[0];
        }

        public uint GetId()
        {
            return (uint) this.Id;
        }

        public bool IsRaid
        {
            get { return this.Type == MapType.Raid; }
        }

        /// <summary>All zone ids within the Map</summary>
        public bool IsInstance
        {
            get
            {
                if (this.Type != MapType.Dungeon)
                    return this.Type == MapType.Raid;
                return true;
            }
        }

        /// <summary>Battleground or Arena</summary>
        public bool IsBattleground
        {
            get { return this.BattlegroundTemplate != null; }
        }

        public Map RepopMap
        {
            get
            {
                if (this.RepopMapId != MapId.End)
                    return WCell.RealmServer.Global.World.GetNonInstancedMap(this.RepopMapId);
                return (Map) null;
            }
        }

        [NotPersistent]
        public Vector3 FirstEntrance
        {
            get
            {
                if (this.EntrancePositions.Length <= 0)
                    return Vector3.Right;
                return this.EntrancePositions[0];
            }
        }

        [NotPersistent] public bool IsAsda2FightingMap { get; set; }

        public bool IsDisabled { get; set; }

        public uint NextId()
        {
            if (this.Ids == null)
                this.Ids = new IdQueue();
            return this.Ids.NextId();
        }

        public void RecycleId(uint id)
        {
            this.Ids.RecycleId(id);
        }

        public ZoneTemplate GetZoneInfo(float x, float y)
        {
            if (this.ZoneTileSet != null)
                return WCell.RealmServer.Global.World.GetZoneInfo(this.ZoneTileSet.GetZoneId(x, y));
            return (ZoneTemplate) null;
        }

        /// <summary>Does all the default checks</summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public bool MayEnter(Character chr)
        {
            if (this.Type == MapType.Normal)
                return true;
            if (this.RequiredQuestId == 0U || chr.QuestLog.FinishedQuests.Contains(this.RequiredQuestId))
                return this.RequiredClientId <= chr.Account.ClientId;
            return false;
        }

        public void FinalizeDataHolder()
        {
            if (this.RepopPosition == new Vector3())
                this.RepopMapId = MapId.End;
            if (!this.IsInstance)
                return;
            this.InstanceTemplate = new InstanceTemplate(this);
            InstanceMgr.InstanceInfos.Add(this);
        }

        internal void NotifyCreated(Map map)
        {
            Action<Map> created = this.Created;
            if (created == null)
                return;
            created(map);
        }

        public void NotifyStarted(Map map)
        {
            Action<Map> started = this.Started;
            if (started == null)
                return;
            started(map);
        }

        public bool NotifySpawning(Map map)
        {
            Func<Map, bool> spawning = this.Spawning;
            if (spawning != null)
                return spawning(map);
            return true;
        }

        public void NotifySpawned(Map map)
        {
            Action<Map> spawned = this.Spawned;
            if (spawned == null)
                return;
            spawned(map);
        }

        public bool NotifyStopping(Map map)
        {
            Func<Map, bool> stopping = this.Stopping;
            if (stopping != null)
                return stopping(map);
            return true;
        }

        public void NotifyStopped(Map map)
        {
            Action<Map> started = this.Started;
            if (started == null)
                return;
            started(map);
        }

        public void NotifyPlayerEntered(Map map, Character chr)
        {
            Action<Map, Character> playerEntered = this.PlayerEntered;
            if (playerEntered == null)
                return;
            playerEntered(map, chr);
        }

        public void NotifyPlayerLeft(Map map, Character chr)
        {
            Action<Map, Character> playerLeft = this.PlayerLeft;
            if (playerLeft == null)
                return;
            playerLeft(map, chr);
        }

        public bool NotifyPlayerBeforeDeath(Character chr)
        {
            Func<Character, bool> playerBeforeDeath = MapTemplate.PlayerBeforeDeath;
            if (playerBeforeDeath != null)
                return playerBeforeDeath(chr);
            return true;
        }

        public void NotifyPlayerDied(IDamageAction action)
        {
            Action<IDamageAction> playerDied = MapTemplate.PlayerDied;
            if (playerDied != null)
                playerDied(action);
            action.Victim.Map.OnPlayerDeath(action);
        }

        public void NotifyPlayerResurrected(Character chr)
        {
            Action<Character> playerResurrected = MapTemplate.PlayerResurrected;
            if (playerResurrected == null)
                return;
            playerResurrected(chr);
        }

        public int CompareTo(object obj)
        {
            MapTemplate mapTemplate = obj as MapTemplate;
            if (mapTemplate != null)
                return this.Id.CompareTo((object) mapTemplate.Id);
            return -1;
        }

        public override string ToString()
        {
            return ((int) this.Type).ToString() + " " + this.Name + " (" + (object) this.Id + " #" +
                   (object) (uint) this.Id + ")";
        }
    }
}