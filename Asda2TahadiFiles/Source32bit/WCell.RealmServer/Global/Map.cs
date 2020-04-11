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
    public readonly IDictionary<ZoneId, Zone> Zones = new Dictionary<ZoneId, Zone>();

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
    [Variable("AutoSpawnMaps")]public static bool AutoSpawnMaps = true;

    /// <summary>Default update delay in milliseconds</summary>
    public static int DefaultUpdateDelay = 120;

    /// <summary>
    /// Every how many ticks to send UpdateField-changes to Characters
    /// </summary>
    public static int CharacterUpdateEnvironmentTicks = 1000 / DefaultUpdateDelay;

    [Variable("UpdateInactiveAreas")]public static bool UpdateInactiveAreasDefault = false;

    /// <summary>
    /// Whether to have NPCs in inactive areas scan for enemies
    /// </summary>
    [Variable("ScanInactiveAreas")]public static bool ScanInactiveAreasDefault = false;

    /// <summary>
    /// Whether NPCs can evade and run back to their spawn point when pulled too far away
    /// </summary>
    [Variable("NPCsCanEvade")]public static bool CanNPCsEvadeDefault = true;

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
      get { return updatePriorityMillis; }
      set
      {
        updatePriorityMillis = value;
        SetUpdatePriorityTicks();
      }
    }

    private static void SetDefaultUpdatePriorityTick(UpdatePriority priority, int ticks)
    {
      if(UpdatePriorityMillis[(int) priority] != 0)
        return;
      UpdatePriorityMillis[(int) priority] = ticks;
    }

    private static void SetUpdatePriorityTicks()
    {
      if(UpdatePriorityMillis == null)
        UpdatePriorityMillis = new int[6];
      else if(UpdatePriorityMillis.Length != 6)
        Array.Resize(ref updatePriorityMillis, 6);
      SetDefaultUpdatePriorityTick(UpdatePriority.Inactive, 10000);
      SetDefaultUpdatePriorityTick(UpdatePriority.Background, 3000);
      SetDefaultUpdatePriorityTick(UpdatePriority.VeryLowPriority, 1000);
      SetDefaultUpdatePriorityTick(UpdatePriority.LowPriority, 600);
      SetDefaultUpdatePriorityTick(UpdatePriority.Active, 300);
      SetDefaultUpdatePriorityTick(UpdatePriority.HighPriority, 0);
    }

    public static int GetTickCount(UpdatePriority priority)
    {
      return UpdatePriorityMillis[(int) priority];
    }

    public static void SetTickCount(UpdatePriority priority, int count)
    {
      UpdatePriorityMillis[(int) priority] = count;
    }

    static Map()
    {
      SetUpdatePriorityTicks();
    }

    public static string LoadAvgStr
    {
      get
      {
        return string.Format("{0:0.00}/{1} ({2} %)", _avgUpdateTime,
          DefaultUpdateDelay,
          (float) (_avgUpdateTime / (double) DefaultUpdateDelay * 100.0));
      }
    }

    public int CurrentThreadId
    {
      get { return m_currentThreadId; }
    }

    internal uint GenerateNewNPCPoolId()
    {
      return (uint) Interlocked.Increment(ref m_lastNPCPoolId);
    }

    public WorldStateCollection WorldStates { get; private set; }

    protected Map()
    {
      m_objects = new Dictionary<EntityId, WorldObject>();
      m_updateDelay = DefaultUpdateDelay;
      m_updateInactiveAreas = UpdateInactiveAreasDefault;
      m_ScanInactiveAreas = ScanInactiveAreasDefault;
      m_canNPCsEvade = CanNPCsEvadeDefault;
      for(Locale key = Locale.Start; key < Locale.End; ++key)
        CharactersByLocale.Add(key, new List<Character>());
      CallDelayed(600000, ClearLoot);
    }

    public void ClearLoot()
    {
      foreach(Asda2Loot asda2Loot in Loots.Where(asda2Loot =>
          (DateTime.Now - asda2Loot.SpawnTime).TotalMinutes > (double) CharacterFormulas.DropLiveMinutes)
        .ToList())
      {
        foreach(Asda2LootItem asda2LootItem in asda2Loot.Items)
        {
          GlobalHandler.SendRemoveItemResponse(asda2LootItem);
          ClearLootSlot(asda2LootItem.Position.X, asda2LootItem.Position.Y);
        }

        asda2Loot.Dispose();
      }

      CallDelayed(600000, ClearLoot);
    }

    public void ClearLootNow()
    {
      for(int index = 0; index < Loots.Count; index = index - 1 + 1)
      {
        Asda2Loot loot = Loots[index];
        foreach(Asda2LootItem asda2LootItem in loot.Items)
        {
          GlobalHandler.SendRemoveItemResponse(asda2LootItem);
          ClearLootSlot(asda2LootItem.Position.X, asda2LootItem.Position.Y);
        }

        loot.Dispose();
      }
    }

    /// <summary>Creates a map from the given map info.</summary>
    /// <param name="rgnTemplate">the info for this map to use</param>
    public Map(MapTemplate rgnTemplate)
      : this()
    {
      m_MapTemplate = rgnTemplate;
      Offset = (float) rgnTemplate.Id * 1000f;
      m_CanFly = rgnTemplate.Id == MapId.Outland || rgnTemplate.Id == MapId.Northrend;
    }

    protected internal void InitMap(MapTemplate template)
    {
      m_MapTemplate = template;
      InitMap();
    }

    /// <summary>Method is called after Creation of Map</summary>
    protected internal virtual void InitMap()
    {
      m_Terrain = TerrainMgr.GetTerrain(m_MapTemplate.Id);
      m_zoneTileSet = m_MapTemplate.ZoneTileSet;
      m_root = new ZoneSpacePartitionNode(m_MapTemplate.Bounds);
      PartitionSpace();
      WorldStates = new WorldStateCollection(this,
        WCell.Constants.World.WorldStates.GetStates(m_MapTemplate.Id) ?? WorldState.EmptyArray);
      CreateZones();
      m_MapTemplate.NotifyCreated(this);
    }

    private void CreateZones()
    {
      for(int index = 0; index < m_MapTemplate.ZoneInfos.Count; ++index)
      {
        ZoneTemplate zoneInfo = m_MapTemplate.ZoneInfos[index];
        Zone zone = zoneInfo.Creator(this, zoneInfo);
        if(zone.ParentZone == null)
          MainZones.Add(zone);
        Zones.Add(zoneInfo.Id, zone);
      }

      m_defaultZone = MainZones.FirstOrDefault();
    }

    public MapId MapId
    {
      get { return Id; }
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
      get { return m_MapTemplate; }
    }

    public ITerrain Terrain
    {
      get { return m_Terrain; }
    }

    public IWorldSpace ParentSpace
    {
      get { return World.Instance; }
    }

    public int MainZoneCount
    {
      get { return MainZones.Count; }
    }

    /// <summary>
    /// The first MainZone (for reference and Maps that only contain one Zone)
    /// </summary>
    public Zone DefaultZone
    {
      get { return m_defaultZone; }
    }

    /// <summary>The bounds of this map.</summary>
    public BoundingBox Bounds
    {
      get { return m_root.Bounds; }
    }

    public ICollection<NPC> SpiritHealers
    {
      get { return m_spiritHealers; }
    }

    /// <summary>The display name of this map.</summary>
    public string Name
    {
      get { return m_MapTemplate.Name; }
    }

    /// <summary>The map ID of this map.</summary>
    public MapId Id
    {
      get { return m_MapTemplate.Id; }
    }

    /// <summary>The minimum required ClientId</summary>
    public ClientId RequiredClient
    {
      get { return m_MapTemplate.RequiredClientId; }
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
      get { return m_MapTemplate.Type; }
    }

    public bool IsHeroic
    {
      get
      {
        MapDifficultyEntry difficulty = Difficulty;
        if(difficulty != null)
          return difficulty.IsHeroic;
        return false;
      }
    }

    public uint DifficultyIndex
    {
      get
      {
        MapDifficultyEntry difficulty = Difficulty;
        if(difficulty == null)
          return 0;
        return difficulty.Index;
      }
    }

    /// <summary>Difficulty of the instance</summary>
    public virtual MapDifficultyEntry Difficulty
    {
      get { return null; }
    }

    /// <summary>
    /// The minimum level a player has to be to enter the map (instance)
    /// </summary>
    public virtual int MinLevel
    {
      get { return m_MapTemplate.MinLevel; }
    }

    /// <summary>
    /// The maximum level a player can be to enter the map (instance)
    /// </summary>
    public virtual int MaxLevel
    {
      get { return m_MapTemplate.MaxLevel; }
    }

    /// <summary>
    /// Maximum number of players allowed in the map (instance)
    /// </summary>
    public int MaxPlayerCount
    {
      get { return m_MapTemplate.MaxPlayerCount; }
    }

    /// <summary>
    /// Whether or not the map is currently processing object updates
    /// </summary>
    public bool IsRunning
    {
      get { return m_running; }
      set
      {
        if(m_running == value)
          return;
        if(value)
          Start();
        else
          Stop();
      }
    }

    /// <summary>
    /// Indicates whether the current Thread is the Map's update-thread.
    /// </summary>
    public bool IsInContext
    {
      get { return Thread.CurrentThread.ManagedThreadId == m_currentThreadId; }
    }

    public bool IsUpdating
    {
      get { return m_isUpdating; }
    }

    public virtual DateTime CreationTime
    {
      get { return ServerApp<RealmServer>.StartTime; }
    }

    /// <summary>
    /// The amount of all Players in this Map (excludes Staff members)
    /// </summary>
    public int PlayerCount
    {
      get { return m_allyCount + m_hordeCount; }
    }

    /// <summary>
    /// The amount of all Characters in this Map (includes Staff members)
    /// </summary>
    public int CharacterCount
    {
      get { return m_characters.Count; }
    }

    /// <summary>
    /// The number of Alliance Players currently in the map (not counting Staff)
    /// </summary>
    public int AllianceCount
    {
      get { return m_allyCount; }
    }

    /// <summary>
    /// The number of Alliance Players currently in the map (not counting Staff)
    /// </summary>
    public int HordeCount
    {
      get { return m_hordeCount; }
    }

    public int NPCSpawnPoolCount
    {
      get { return m_npcSpawnPools.Count; }
    }

    /// <summary>Amount of passed ticks in this Map</summary>
    public int TickCount
    {
      get { return m_tickCount; }
    }

    /// <summary>Don't modify the List.</summary>
    public List<Character> Characters
    {
      get
      {
        EnsureContext();
        return m_characters;
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
      get { return m_objects.Count == 0; }
    }

    /// <summary>
    /// Whether to also update Nodes in areas without Players.
    /// Default: <see cref="F:WCell.RealmServer.Global.Map.UpdateInactiveAreasDefault" />
    /// </summary>
    public bool UpdateInactiveAreas
    {
      get { return m_updateInactiveAreas; }
      set { m_updateInactiveAreas = value; }
    }

    /// <summary>
    /// Whether to let NPCs scan inactive Nodes for hostility.
    /// Default: <see cref="F:WCell.RealmServer.Global.Map.ScanInactiveAreasDefault" />
    /// </summary>
    public bool ScanInactiveAreas
    {
      get { return m_ScanInactiveAreas; }
      set { m_ScanInactiveAreas = value; }
    }

    /// <summary>
    /// Time in milliseconds between the beginning of
    /// one Map-Update and the next.
    /// </summary>
    public int UpdateDelay
    {
      get { return m_updateDelay; }
      set { Interlocked.Exchange(ref m_updateDelay, value); }
    }

    public int CharacterUpdateEnvironmentDelay
    {
      get { return CharacterUpdateEnvironmentTicks * m_updateDelay; }
    }

    /// <summary>Total amount of objects within this Map</summary>
    public int ObjectCount
    {
      get { return m_objects.Count; }
    }

    /// <summary>
    /// Whether NPCs in this Map will try to evade after Combat
    /// </summary>
    public bool CanNPCsEvade
    {
      get { return m_canNPCsEvade; }
      set { m_canNPCsEvade = value; }
    }

    public bool CanFly
    {
      get { return m_CanFly; }
      set { m_CanFly = value; }
    }

    /// <summary>Toggles all NPCs to be invul and idle</summary>
    public bool IsAIFrozen
    {
      get { return m_IsAIFrozen; }
      set
      {
        EnsureContext();
        if(m_IsAIFrozen == value)
          return;
        m_IsAIFrozen = value;
        foreach(WorldObject worldObject in m_objects.Values)
        {
          if(worldObject is NPC)
          {
            NPC npc = (NPC) worldObject;
            if(value)
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
      if(m_running)
        return;
      lock(m_objects)
      {
        if(m_running)
          return;
        lock(World.PauseLock)
        {
          if(World.Paused)
            throw new InvalidOperationException("Tried to start Map while World is paused.");
          m_running = true;
        }

        s_log.Debug(WCell_RealmServer.MapStarted, m_MapTemplate.Id);
        Task.Factory.StartNewDelayed(m_updateDelay, MapUpdateCallback,
          this);
        if(AutoSpawnMaps)
          SpawnMapLater();
        m_lastUpdateTime = DateTime.Now;
        m_MapTemplate.NotifyStarted(this);
      }
    }

    /// <summary>
    /// Stops map updating and stops the update delta measuring
    /// </summary>
    public void Stop()
    {
      if(!m_running || !m_MapTemplate.NotifyStopping(this))
        return;
      lock(m_objects)
      {
        if(!m_running)
          return;
        m_running = false;
        s_log.Debug(WCell_RealmServer.MapStopped, m_MapTemplate.Id);
        m_MapTemplate.NotifyStopped(this);
      }
    }

    public bool ExecuteInContext(Action action)
    {
      if(!IsInContext)
      {
        AddMessage(new Message(action));
        return false;
      }

      action();
      return true;
    }

    /// <summary>Ensures execution within the map.</summary>
    /// <exception cref="T:System.InvalidOperationException">thrown if the calling thread isn't the map thread</exception>
    public void EnsureContext()
    {
      if(Thread.CurrentThread.ManagedThreadId != m_currentThreadId && IsRunning)
      {
        Stop();
        throw new InvalidOperationException(string.Format(WCell_RealmServer.MapContextNeeded, this));
      }
    }

    /// <summary>Ensures execution outside the Map-context.</summary>
    /// <exception cref="T:System.InvalidOperationException">thrown if the calling thread is the map thread</exception>
    public void EnsureNoContext()
    {
      if(Thread.CurrentThread.ManagedThreadId == m_currentThreadId)
      {
        Stop();
        throw new InvalidOperationException(
          string.Format(WCell_RealmServer.MapContextProhibited, this));
      }
    }

    /// <summary>Ensures that Map is not updating.</summary>
    /// <exception cref="T:System.InvalidOperationException">thrown if the Map is currently updating</exception>
    public void EnsureNotUpdating()
    {
      if(m_isUpdating)
      {
        Stop();
        throw new InvalidOperationException(string.Format(WCell_RealmServer.MapUpdating, this));
      }
    }

    /// <summary>
    /// Whether this Map's NPCs and GOs have been fully spawned
    /// </summary>
    public bool IsSpawned
    {
      get
      {
        if(m_npcsSpawned)
          return m_gosSpawned;
        return false;
      }
    }

    public bool IsSpawning { get; private set; }

    public bool NPCsSpawned
    {
      get { return m_npcsSpawned; }
    }

    public bool GOsSpawned
    {
      get { return m_gosSpawned; }
    }

    public void RemoveNPCSpawnPool(NPCSpawnPoolTemplate templ)
    {
      AddMessage(() => RemoveNPCSpawnPoolNow(templ));
    }

    public void RemoveNPCSpawnPoolNow(NPCSpawnPoolTemplate templ)
    {
      NPCSpawnPool npcSpawnPool;
      if(!m_npcSpawnPools.TryGetValue(templ.PoolId, out npcSpawnPool))
        return;
      npcSpawnPool.RemovePoolNow();
    }

    public void RemoveGOSpawnPool(GOSpawnPoolTemplate templ)
    {
      AddMessage(() => RemoveGOSpawnPoolNow(templ));
    }

    public void RemoveGOSpawnPoolNow(GOSpawnPoolTemplate templ)
    {
      GOSpawnPool goSpawnPool;
      if(!m_goSpawnPools.TryGetValue(templ.PoolId, out goSpawnPool))
        return;
      goSpawnPool.RemovePoolNow();
    }

    public void AddNPCSpawnPool(NPCSpawnPoolTemplate templ)
    {
      AddMessage(() => AddNPCSpawnPoolNow(templ));
    }

    public NPCSpawnPool AddNPCSpawnPoolNow(NPCSpawnPoolTemplate templ)
    {
      NPCSpawnPool pool = new NPCSpawnPool(this, templ);
      AddNPCSpawnPoolNow(pool);
      return pool;
    }

    public void AddNPCSpawnPoolNow(NPCSpawnPool pool)
    {
      NPCSpawnPool npcSpawnPool;
      if(!m_npcSpawnPools.TryGetValue(pool.Template.PoolId, out npcSpawnPool))
      {
        m_npcSpawnPools.Add(pool.Template.PoolId, pool);
        OnPoolAdded<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>(pool);
      }
      else
        pool = npcSpawnPool;

      if(!SpawnPointsEnabled)
        return;
      pool.IsActive = true;
    }

    public void AddGOSpawnPoolLater(GOSpawnPoolTemplate templ)
    {
      AddMessage(() => AddGOSpawnPoolNow(templ));
    }

    public GOSpawnPool AddGOSpawnPoolNow(GOSpawnPoolTemplate templ)
    {
      GOSpawnPool pool = new GOSpawnPool(this, templ);
      AddGOSpawnPoolNow(pool);
      return pool;
    }

    public void AddGOSpawnPoolNow(GOSpawnPool pool)
    {
      GOSpawnPool goSpawnPool;
      if(!m_goSpawnPools.TryGetValue(pool.Template.PoolId, out goSpawnPool))
      {
        m_goSpawnPools.Add(pool.Template.PoolId, pool);
        OnPoolAdded<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>(pool);
      }
      else
        pool = goSpawnPool;

      if(!SpawnPointsEnabled)
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
      foreach(POINT spawnPoint in pool.SpawnPoints)
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
      if(typeof(O) == typeof(NPC))
      {
        if(m_npcSpawnPools.Remove(pool.Template.PoolId))
          pool.IsActive = false;
      }
      else
      {
        if(!(typeof(O) == typeof(GameObject)))
          throw new ArgumentException("Invalid Pool type: " + pool);
        if(m_goSpawnPools.Remove(pool.Template.PoolId))
          pool.IsActive = false;
      }

      foreach(POINT spawnPoint in pool.SpawnPoints)
        spawnPoint.SpawnEntry.SpawnPoints.Remove(spawnPoint);
    }

    /// <summary>Adds a message to the Map to clear it</summary>
    public void ClearLater()
    {
      AddMessage(RemoveAll);
    }

    /// <summary>Removes all Objects, NPCs and Spawns from this Map.</summary>
    /// <remarks>Requires map context</remarks>
    public virtual void RemoveAll()
    {
      RemoveObjects();
    }

    /// <summary>Removes all Objects and NPCs from the Map</summary>
    public void RemoveObjects()
    {
      if(IsInContext && !IsUpdating)
        RemoveObjectsNow();
      else
        AddMessage(() => RemoveObjectsNow());
    }

    private void RemoveObjectsNow()
    {
      foreach(SpawnPool<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool> spawnPool in
        m_npcSpawnPools.Values.ToArray())
        spawnPool.RemovePoolNow();
      foreach(SpawnPool<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool> spawnPool in
        m_goSpawnPools.Values.ToArray())
        spawnPool.RemovePoolNow();
      foreach(WorldObject copyObject in CopyObjects())
      {
        if(!(copyObject is Character) && !copyObject.IsPlayerOwned && !copyObject.IsDeleted)
          copyObject.DeleteNow();
      }

      m_gosSpawned = false;
      m_npcsSpawned = false;
    }

    /// <summary>
    /// Clears all Objects and NPCs and spawns the default ones again
    /// </summary>
    public virtual void Reset()
    {
      RemoveAll();
      SpawnMap();
      foreach(SpawnPool<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool> spawnPool in
        m_npcSpawnPools.Values)
        spawnPool.RespawnFull();
    }

    /// <summary>
    /// If not added already, this method adds all default GameObjects and NPC spawnpoints to this map.
    /// </summary>
    public void SpawnMapLater()
    {
      AddMessage(SpawnMap);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <see cref="M:WCell.RealmServer.Global.Map.SpawnMapLater" />
    /// <remarks>Requires map context</remarks>
    public virtual void SpawnMap()
    {
      EnsureContext();
      if(IsSpawned)
        return;
      IsSpawning = true;
      if(!m_MapTemplate.NotifySpawning(this))
      {
        IsSpawning = false;
      }
      else
      {
        SpawnPortals();
        if(!m_gosSpawned && GOMgr.Loaded)
        {
          int objectCount = ObjectCount;
          SpawnGOs();
          if(objectCount > 0)
            s_log.Debug("Added {0} Objects to Map: {1}", ObjectCount - objectCount,
              this);
          m_gosSpawned = true;
        }

        if(!m_npcsSpawned && NPCMgr.Loaded)
        {
          int count = ObjectCount;
          SpawnNPCs();
          AddMessage(() =>
          {
            if(count <= 0)
              return;
            s_log.Debug("Added {0} NPC Spawnpoints to Map: {1}", ObjectCount - count,
              this);
          });
          m_npcsSpawned = true;
        }

        IsSpawning = false;
        if(!IsSpawned)
          return;
        m_MapTemplate.NotifySpawned(this);
      }
    }

    private void SpawnPortals()
    {
      foreach(GOSpawnPoolTemplate spawnPoolTemplatesBy in Asda2PortalMgr.GetSpawnPoolTemplatesByMap(MapId))
      {
        if(spawnPoolTemplatesBy.AutoSpawns && IsEventActive(spawnPoolTemplatesBy.EventId))
          AddGOSpawnPoolNow(spawnPoolTemplatesBy);
      }
    }

    protected virtual void SpawnGOs()
    {
      List<GOSpawnPoolTemplate> poolTemplatesByMap = GOMgr.GetSpawnPoolTemplatesByMap(MapId);
      if(poolTemplatesByMap == null)
        return;
      foreach(GOSpawnPoolTemplate templ in poolTemplatesByMap)
      {
        if(templ.AutoSpawns && IsEventActive(templ.EventId))
          AddGOSpawnPoolNow(templ);
      }
    }

    protected virtual void SpawnNPCs()
    {
      List<NPCSpawnPoolTemplate> poolTemplatesByMap = NPCMgr.GetSpawnPoolTemplatesByMap(Id);
      if(poolTemplatesByMap == null)
        return;
      foreach(NPCSpawnPoolTemplate templ in poolTemplatesByMap)
      {
        if(templ.AutoSpawns && IsEventActive(templ.EventId))
          AddNPCSpawnPoolNow(templ);
      }
    }

    public bool SpawnPointsEnabled
    {
      get { return m_SpawnPointsEnabled; }
      set
      {
        if(m_SpawnPointsEnabled == value)
          return;
        m_SpawnPointsEnabled = value;
        if(value)
          ForeachSpawnPool(pool => pool.IsActive = true);
        else
          ForeachSpawnPool(pool => pool.Disable());
      }
    }

    public void ForeachSpawnPool(Action<NPCSpawnPool> func)
    {
      ForeachSpawnPool(Vector3.Zero, 0.0f, func);
    }

    public void ForeachSpawnPool(Vector3 pos, float radius, Action<NPCSpawnPool> func)
    {
      float radiusSq = radius * radius;
      foreach(NPCSpawnPool npcSpawnPool in m_npcSpawnPools.Values)
      {
        if(radius <= 0.0 || npcSpawnPool.Template.Entries.Any(
             spawn => (double) spawn.GetDistSq(pos) < (double) radiusSq))
          func(npcSpawnPool);
      }
    }

    public void RespawnInRadius(Vector3 pos, float radius)
    {
      ForeachSpawnPool(pos, radius, pool => pool.RespawnFull());
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
      get { return m_lastUpdateTime; }
    }

    /// <summary>
    /// Adds a new Updatable right away.
    /// Requires Map context.
    /// <see cref="M:WCell.RealmServer.Global.Map.RegisterUpdatableLater(WCell.Core.Timers.IUpdatable)" />
    /// </summary>
    /// <param name="updatable"></param>
    public void RegisterUpdatable(IUpdatable updatable)
    {
      EnsureContext();
      m_updatables.Add(updatable);
    }

    /// <summary>
    /// Unregisters an Updatable right away.
    /// In map context.
    /// <see cref="M:WCell.RealmServer.Global.Map.UnregisterUpdatableLater(WCell.Core.Timers.IUpdatable)" />
    /// </summary>
    public void UnregisterUpdatable(IUpdatable updatable)
    {
      EnsureContext();
      m_updatables.Remove(updatable);
    }

    /// <summary>
    /// Registers the given Updatable during the next Map Tick
    /// </summary>
    public void RegisterUpdatableLater(IUpdatable updatable)
    {
      m_messageQueue.Enqueue(new Message(() => RegisterUpdatable(updatable)));
    }

    /// <summary>
    /// Unregisters the given Updatable during the next Map Update
    /// </summary>
    public void UnregisterUpdatableLater(IUpdatable updatable)
    {
      m_messageQueue.Enqueue(new Message(() => UnregisterUpdatable(updatable)));
    }

    /// <summary>
    /// Executes the given action after the given delay within this Map's context.
    /// </summary>
    /// <remarks>Make sure that once the timeout is hit, the given action is executed in the correct Map's context.</remarks>
    public TimerEntry CallDelayed(int millis, Action action)
    {
      TimerEntry timer = new TimerEntry();
      timer.Action = delay =>
      {
        action();
        UnregisterUpdatableLater(timer);
      };
      timer.Start(millis, 0);
      RegisterUpdatableLater(timer);
      return timer;
    }

    /// <summary>
    /// Executes the given action after the given delay within this Map's context.
    /// </summary>
    /// <remarks>Make sure that once the timeout is hit, the given action is executed in the correct Map's context.</remarks>
    public TimerEntry CallPeriodically(int seconds, Action action)
    {
      TimerEntry timerEntry = new TimerEntry
      {
        Action = delay => action()
      };
      timerEntry.Start(seconds, seconds);
      RegisterUpdatableLater(timerEntry);
      return timerEntry;
    }

    /// <summary>
    /// Adds a message to the message queue for this map.
    /// TODO: Consider extra-message for Character that checks whether Char is still in Map?
    /// </summary>
    /// <param name="action">the action to be enqueued</param>
    public void AddMessage(Action action)
    {
      AddMessage((Message) action);
    }

    /// <summary>Adds a message to the message queue for this map.</summary>
    /// <param name="msg">the message</param>
    public void AddMessage(IMessage msg)
    {
      Start();
      m_messageQueue.Enqueue(msg);
    }

    /// <summary>
    /// Callback for executing updates of the map, which includes updates for all inhabiting objects.
    /// </summary>
    /// <param name="state">the <see cref="T:WCell.RealmServer.Global.Map" /> to update</param>
    private void MapUpdateCallback(object state)
    {
      ++totalUpdates;
      if(Interlocked.CompareExchange(ref m_currentThreadId, Thread.CurrentThread.ManagedThreadId, 0) != 0)
        return;
      DateTime now = DateTime.Now;
      int milliSecondsInt1 = (now - m_lastUpdateTime).ToMilliSecondsInt();
      IMessage message;
      while(m_messageQueue.TryDequeue(out message))
      {
        try
        {
          message.Execute();
        }
        catch(Exception ex)
        {
          LogUtil.ErrorException(ex, "Exception raised when processing Message.");
        }
      }

      m_isUpdating = true;
      foreach(IUpdatable updatable in m_updatables)
      {
        try
        {
          updatable.Update(milliSecondsInt1);
        }
        catch(Exception ex)
        {
          LogUtil.ErrorException(ex, "Exception raised when updating Updatable: " + updatable);
          UnregisterUpdatableLater(updatable);
        }
      }

      int num1 = 0;
      foreach(WorldObject worldObject in m_objects.Values)
      {
        if(!worldObject.IsTeleporting)
        {
          UpdatePriority updatePriority;
          if(!worldObject.IsAreaActive && DefenceTownEvent == null)
          {
            if(m_updateInactiveAreas)
              updatePriority = UpdatePriority.Inactive;
            else
              continue;
          }
          else
            updatePriority = worldObject.UpdatePriority;

          try
          {
            int updatePriorityMilli = UpdatePriorityMillis[(int) updatePriority];
            int milliSecondsInt2 = (now - worldObject.LastUpdateTime).ToMilliSecondsInt();
            if(milliSecondsInt2 >= updatePriorityMilli)
            {
              worldObject.LastUpdateTime = now;
              int tickCount = Environment.TickCount;
              worldObject.Update(milliSecondsInt2);
              ++num1;
              int num2 = Environment.TickCount - tickCount;
              if(maxObjUpdateTime < num2)
              {
                maxObjUpdateTime = num2;
                if(totalUpdates > 500)
                {
                  if(maxObjUpdateTime > 1000)
                  {
                    totalUpdates = 0;
                    maxObjUpdateTime = 100;
                    LogUtil.WarnException("Object {0} updates {1} !!!", (object) worldObject, (object) num2);
                  }
                }
              }
            }
          }
          catch(Exception ex)
          {
            LogUtil.ErrorException(ex, "Exception raised when updating Object: " + worldObject);
            if(worldObject is Unit)
            {
              Unit unit = (Unit) worldObject;
              if(unit.Brain != null)
                unit.Brain.IsRunning = false;
            }

            Character character = worldObject as Character;
            if(character != null)
              character.Logout(true);
            else
              worldObject.Delete();
          }
        }
      }

      if(m_tickCount % CharacterUpdateEnvironmentTicks == 0)
        UpdateCharacters();
      UpdateMap();
      m_lastUpdateTime = now;
      ++m_tickCount;
      m_isUpdating = false;
      TimeSpan time = DateTime.Now - now;
      _avgUpdateTime = (float) ((_avgUpdateTime * 9.0 + time.TotalMilliseconds) / 10.0);
      if(_avgUpdateTime > 1000.0)
        LogUtil.WarnException(
          "---=---end delta :{0}, avg :{1}, objs :{2}, objUpd :{3}, maxUpdTime: {4}, map: {5}", (object) time,
          (object) _avgUpdateTime, (object) ObjectCount, (object) num1,
          (object) maxObjUpdateTime, (object) Name);
      Interlocked.Exchange(ref m_currentThreadId, 0);
      if(m_running)
      {
        int millisecondsDelay = m_updateDelay - time.ToMilliSecondsInt();
        if(millisecondsDelay < 0)
          millisecondsDelay = 0;
        Task.Factory.StartNewDelayed(millisecondsDelay, MapUpdateCallback,
          this);
      }
      else
      {
        if(!IsDisposed)
          return;
        Dispose();
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
      int count = m_characters.Count;
      for(int index = 0; index < count; ++index)
      {
        Character character = m_characters[index];
        try
        {
          character.UpdateEnvironment(updatedObjects);
        }
        catch(Exception ex)
        {
          LogUtil.ErrorException(ex, "Exception raised when updating Character {0}", (object) character);
        }
      }

      foreach(ObjectBase objectBase in updatedObjects)
        objectBase.ResetUpdateInfo();
      updatedObjects.Clear();
      WorldObject.WorldObjectSetPool.Recycle(updatedObjects);
    }

    /// <summary>
    /// Instantly updates all active Characters' environment: Collect environment info and send update deltas
    /// </summary>
    public void ForceUpdateCharacters()
    {
      if(IsInContext && !IsUpdating)
        UpdateCharacters();
      else
        AddMessageAndWait(UpdateCharacters);
    }

    public void CallOnAllCharacters(Action<Character> action)
    {
      ExecuteInContext(() =>
      {
        foreach(Character character in m_characters)
          action(character);
      });
    }

    public void CallOnAllNPCs(Action<NPC> action)
    {
      ExecuteInContext(() =>
      {
        foreach(WorldObject worldObject in m_objects.Values)
        {
          if(worldObject is NPC)
            action((NPC) worldObject);
        }
      });
    }

    internal void UpdateWorldStates(uint index, int value)
    {
      foreach(Zone mainZone in MainZones)
        mainZone.WorldStates.UpdateWorldState(index, value);
    }

    /// <summary>Partitions the space of the zone.</summary>
    private void PartitionSpace()
    {
      m_root.PartitionSpace(null, ZoneSpacePartitionNode.DefaultPartitionThreshold,
        0);
    }

    /// <summary>Gets the partitioned node in which the point lies.</summary>
    /// <param name="pt">the point to search for</param>
    /// <returns>a <see cref="T:WCell.RealmServer.Global.ZoneSpacePartitionNode" /> if found; null otherwise</returns>
    internal ZoneSpacePartitionNode GetNodeFromPoint(ref Vector3 pt)
    {
      if(m_root == null)
        return null;
      return m_root.GetLeafFromPoint(ref pt);
    }

    /// <summary>
    /// Checks to see if the supplied location is within this zone's bounds.
    /// </summary>
    /// <param name="point">the point to check for containment</param>
    /// <returns>true if the location is within the bounds, false otherwise</returns>
    public bool IsPointInMap(ref Vector3 point)
    {
      if(m_root != null)
        return m_root.Bounds.Contains(ref point);
      return false;
    }

    /// <summary>Gets a zone by its area ID.</summary>
    /// <param name="id">the ID of the zone to search for</param>
    /// <returns>the Zone object representing the specified ID; null if the zone was not found</returns>
    public Zone GetZone(ZoneId id)
    {
      Zone zone;
      Zones.TryGetValue(id, out zone);
      return zone;
    }

    public Zone GetZone(float x, float y)
    {
      if(m_MapTemplate.ZoneTileSet != null)
        return GetZone(m_MapTemplate.ZoneTileSet.GetZoneId(x, y));
      return null;
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
      EnsureContext();
      List<WorldObject> entities = new List<WorldObject>();
      if(m_root != null && radius >= 1.0)
      {
        BoundingSphere sphere = new BoundingSphere(origin, radius);
        m_root.GetEntitiesInArea(ref sphere, entities, filter, phase, ref limit);
      }

      return entities;
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
      EnsureContext();
      List<T> entities = new List<T>();
      if(m_root != null && radius >= 1.0)
      {
        BoundingSphere sphere = new BoundingSphere(origin, radius);
        m_root.GetEntitiesInArea(ref sphere, entities, phase, ref limit);
      }

      return entities;
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
      EnsureContext();
      List<T> entities = new List<T>();
      if(m_root != null && radius >= 1.0)
      {
        BoundingSphere sphere = new BoundingSphere(origin, radius);
        m_root.GetEntitiesInArea(ref sphere, entities, filter, phase, ref limit);
      }

      return entities;
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
      EnsureContext();
      List<WorldObject> entities = new List<WorldObject>();
      if(m_root != null && radius >= 1.0)
      {
        BoundingSphere sphere = new BoundingSphere(origin, radius);
        m_root.GetEntitiesInArea(ref sphere, entities, filter, phase, ref limit);
      }

      return entities;
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
      EnsureContext();
      List<WorldObject> entities = new List<WorldObject>();
      if(m_root != null && !(box.Min == box.Max))
        m_root.GetEntitiesInArea(ref box, entities, filter, phase, ref limit);
      return entities;
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
      EnsureContext();
      List<T> entities = new List<T>();
      if(m_root != null && !(box.Min == box.Max))
        m_root.GetEntitiesInArea(ref box, entities, phase, ref limit);
      return entities;
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
      EnsureContext();
      List<T> entities = new List<T>();
      if(m_root != null && !(box.Min == box.Max))
        m_root.GetEntitiesInArea(ref box, entities, filter, phase, ref limit);
      return entities;
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
      EnsureContext();
      List<WorldObject> entities = new List<WorldObject>();
      if(m_root != null && !(box.Min == box.Max))
        m_root.GetEntitiesInArea(ref box, entities, filter, phase, ref limit);
      return entities;
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
      EnsureContext();
      BoundingSphere sphere = new BoundingSphere(origin, radius);
      return m_root.Iterate(ref sphere, predicate, phase);
    }

    /// <summary>Sends a packet to all nearby characters.</summary>
    /// <param name="packet">the packet to send</param>
    /// <param name="includeSelf">whether or not to send the packet to ourselves (if we're a character)</param>
    public void SendPacketToArea(RealmPacketOut packet, ref Vector3 center, uint phase)
    {
      IterateObjects(center, WorldObject.BroadcastRange, phase, obj =>
      {
        if(obj is NPC && ((Unit) obj).Charmer != null && ((Unit) obj).Charmer is Character)
          ((Character) ((Unit) obj).Charmer).Send(packet.GetFinalizedPacket());
        if(obj is Character)
          ((Character) obj).Send(packet.GetFinalizedPacket());
        return true;
      });
    }

    /// <summary>Sends a packet to all characters in the map</summary>
    /// <param name="packet">the packet to send</param>
    public void SendPacketToMap(RealmPacketOut packet)
    {
      CallOnAllCharacters(chr => chr.Send(packet.GetFinalizedPacket()));
    }

    /// <summary>Removes an entity from the zone.</summary>
    /// <param name="obj">the entity to remove</param>
    /// <returns>true if the entity was removed; false otherwise</returns>
    public bool RemoveObjectLater(WorldObject obj)
    {
      if(!m_objects.ContainsKey(obj.EntityId))
        return false;
      m_messageQueue.Enqueue(GetRemoveObjectTask(obj, this));
      return true;
    }

    public void AddObject(WorldObject obj)
    {
      if(m_isUpdating || !IsInContext)
        AddObjectLater(obj);
      else
        AddObjectNow(obj);
    }

    public void AddObject(WorldObject obj, Vector3 pos)
    {
      if(m_isUpdating || !IsInContext)
        TransferObjectLater(obj, pos);
      else
        AddObjectNow(obj, ref pos);
    }

    public void AddObject(WorldObject obj, ref Vector3 pos)
    {
      if(m_isUpdating || !IsInContext)
        TransferObjectLater(obj, pos);
      else
        AddObjectNow(obj, ref pos);
    }

    /// <summary>Adds the given Object to this Map.</summary>
    /// <remarks>Requires map context.</remarks>
    public void AddObjectNow(WorldObject obj, Vector3 pos)
    {
      obj.Position = pos;
      AddObjectNow(obj);
    }

    /// <summary>Adds the given Object to this Map.</summary>
    /// <remarks>Requires map context.</remarks>
    public void AddObjectNow(WorldObject obj, ref Vector3 pos)
    {
      obj.Position = pos;
      AddObjectNow(obj);
    }

    public Dictionary<Locale, List<Character>> CharactersByLocale
    {
      get
      {
        EnsureContext();
        return _charactersByLocale;
      }
    }

    /// <summary>Adds the given Object to this Map.</summary>
    /// <remarks>Requires map context.</remarks>
    public void AddObjectNow(WorldObject obj)
    {
      try
      {
        EnsureNotUpdating();
        EnsureContext();
        if(IsDisposed)
        {
          if(!(obj is Character))
            obj.Delete();
          else
            ((Character) obj).TeleportToBindLocation();
        }
        else if(obj.IsDeleted)
          s_log.Warn("Tried to add deleted object \"{0}\" to Map: " + this);
        else if(!m_root.AddObject(obj))
        {
          s_log.Error("Could not add Object to Map {0} at {1} (Map Bounds: {2})", this,
            obj.Position, Bounds);
          if(!(obj is Character))
            obj.Delete();
          else
            ((Character) obj).TeleportToBindLocation();
        }
        else
        {
          obj.Map = this;
          m_objects.Add(obj.EntityId, obj);
          Interlocked.Increment(ref UniqObjetIdOnMapInterator);
          obj.UniqWorldEntityId = UniqObjetIdOnMapInterator;
          if(obj is Unit)
          {
            if(obj is Character)
            {
              Character chr = (Character) obj;
              m_characters.Add(chr);
              CharactersByLocale[chr.Client.Locale].Add(chr);
              if(chr.Role.Status == RoleStatus.Player)
                IncreasePlayerCount(chr);
              OnEnter(chr);
            }
            else if(obj is NPC)
            {
              NPC npc = (NPC) obj;
              if(npc.IsSpiritHealer)
                m_spiritHealers.Add(npc);
              npc.UniqIdOnMap = AvalibleUniqNpcIds[0];
              _asda2Npcs.Add(npc.UniqIdOnMap, npc);
              AvalibleUniqNpcIds.RemoveAt(0);
            }
          }
          else if(obj is GameObject)
          {
            obj.UniqIdOnMap = AvalibleUniqNpcIds[0];
            _asda2Npcs.Add(obj.UniqIdOnMap, obj);
            AvalibleUniqNpcIds.RemoveAt(0);
          }

          if(MainZoneCount == 1)
          {
            obj.SetZone(m_defaultZone);
          }
          else
          {
            int mainZoneCount = MainZoneCount;
          }

          obj.OnEnterMap();
          obj.RequestUpdate();
          if(!(obj is Character))
            return;
          m_MapTemplate.NotifyPlayerEntered(this, (Character) obj);
        }
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException(ex, "Unable to add Object \"{0}\" to Map: {1}", (object) obj, (object) this);
        if(obj is Character)
        {
          Character character = obj as Character;
          if(character.Client != null)
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
        if(_avalibleUniqNpcIds == null)
        {
          _avalibleUniqNpcIds = new List<ushort>(short.MaxValue);
          for(ushort index = 0; index < ushort.MaxValue; ++index)
            _avalibleUniqNpcIds.Add(index);
        }

        return _avalibleUniqNpcIds;
      }
    }

    internal void IncreasePlayerCount(Character chr)
    {
      if(chr.Faction.IsHorde)
        ++m_hordeCount;
      else
        ++m_allyCount;
    }

    internal void DecreasePlayerCount(Character chr)
    {
      if(chr.Faction.IsHorde)
        --m_hordeCount;
      else
        --m_allyCount;
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
      m_objects.TryGetValue(id, out worldObject);
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
      if(m_isUpdating || !IsInContext)
        RemoveObjectLater(obj);
      else
        RemoveObjectNow(obj);
    }

    public NPC GetNpcByUniqMapId(ushort id)
    {
      return (_asda2Npcs.ContainsKey(id) ? _asda2Npcs[id] : (WorldObject) null) as NPC;
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
      EnsureContext();
      EnsureNotUpdating();
      obj.EnsureContext();
      if(obj is Character)
      {
        Character chr = (Character) obj;
        if(m_characters.Remove(chr))
        {
          if(chr.Role.Status == RoleStatus.Player)
            DecreasePlayerCount(chr);
          OnLeave(chr);
        }

        CharactersByLocale[chr.Client.Locale].Remove(chr);
        m_MapTemplate.NotifyPlayerLeft(this, (Character) obj);
      }

      NPC npc = obj as NPC;
      if(npc != null)
      {
        AvalibleUniqNpcIds.Add(npc.UniqIdOnMap);
        _asda2Npcs.Remove(npc.UniqIdOnMap);
      }
      else
      {
        Asda2Loot asda2Loot = obj as Asda2Loot;
        if(asda2Loot != null)
        {
          foreach(Asda2LootItem asda2LootItem in asda2Loot.Items)
            GlobalHandler.SendRemoveItemResponse(asda2LootItem);
        }
      }

      obj.OnLeavingMap();
      if(obj.Node == null || !obj.Node.RemoveObject(obj))
        return;
      m_objects.Remove(obj.EntityId);
      if(obj is Character)
      {
        m_characters.Remove((Character) obj);
      }
      else
      {
        if(!(obj is NPC) || !((Unit) obj).IsSpiritHealer)
          return;
        m_spiritHealers.Remove((NPC) obj);
      }
    }

    /// <summary>Returns all objects within the Map.</summary>
    /// <remarks>Requires map context.</remarks>
    /// <returns>an array of all objects in the map</returns>
    public WorldObject[] CopyObjects()
    {
      EnsureContext();
      return m_objects.Values.ToArray();
    }

    public virtual bool CanEnter(Character chr)
    {
      if(chr.Level < MinLevel || MaxLevel != 0 && chr.Level > MaxLevel ||
         !m_MapTemplate.MayEnter(chr))
        return false;
      if(MaxPlayerCount != 0)
        return PlayerCount < MaxPlayerCount;
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
      EnsureContext();
      GameObject closest = null;
      float distanceSq = int.MaxValue;
      IterateObjects(pos, WorldObject.BroadcastRange, phase, obj =>
      {
        if(obj is GameObject && ((GameObject) obj).Entry.GOId == goId)
        {
          float distanceSq1 = obj.GetDistanceSq(ref pos);
          if(distanceSq1 < (double) distanceSq)
          {
            distanceSq = distanceSq1;
            closest = (GameObject) obj;
          }
        }

        return true;
      });
      return closest;
    }

    /// <summary>
    /// Returns the specified NPC that is closest to the given point.
    /// </summary>
    /// <remarks>Requires map context.</remarks>
    /// <returns>the closest NPC to the given point, or null if none found.</returns>
    public NPC GetNearestNPC(ref Vector3 pos, NPCId id)
    {
      EnsureContext();
      NPC npc = null;
      float num = int.MaxValue;
      foreach(WorldObject worldObject in m_objects.Values)
      {
        if(!(worldObject is NPC) || ((NPC) worldObject).Entry.NPCId != id)
        {
          float distanceSq = worldObject.GetDistanceSq(ref pos);
          if(distanceSq < (double) num)
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
      EnsureContext();
      NPC npc = null;
      int num = int.MaxValue;
      foreach(NPC spiritHealer in m_spiritHealers)
      {
        int distanceSqInt = spiritHealer.GetDistanceSqInt(ref pos);
        if(distanceSqInt < num)
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
      foreach(GameObject objectsInRadiu in GetObjectsInRadius(pos, radius,
        ObjectTypes.GameObject, phase, 0))
      {
        if(objectsInRadiu.Entry is GOSpellFocusEntry &&
           ((GOSpellFocusEntry) objectsInRadiu.Entry).SpellFocus == focus)
          return objectsInRadiu;
      }

      return null;
    }

    /// <summary>
    /// Enqueues an object to be moved into this Map during the next Map-update
    /// </summary>
    /// <param name="obj">the entity to add</param>
    /// <returns>true if the entity was added, false otherwise</returns>
    protected internal bool AddObjectLater(WorldObject obj)
    {
      Start();
      m_messageQueue.Enqueue(new Message(() => AddObjectNow(obj)));
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
      if(obj.IsTeleporting)
        return true;
      if(obj.Map == this)
      {
        MoveObject(obj, ref newPos);
      }
      else
      {
        obj.IsTeleporting = true;
        Map oldMap = obj.Map;
        if(oldMap != null)
        {
          Message message1 = new Message(() =>
          {
            oldMap.RemoveObjectNow(obj);
            obj.Map = this;
            Message message = new Message(() => obj.IsTeleporting = false);
            obj.Position = newPos;
            Character character = obj as Character;
            if(character != null)
            {
              character.IsAsda2Teleporting = true;
              GlobalHandler.SendTeleportedByCristalResponse(character.Client, Id,
                (short) (newPos.X - (double) Offset),
                (short) (newPos.Y - (double) Offset), TeleportByCristalStaus.Ok);
            }

            AddMessage(message);
          });
          oldMap.AddMessage(message1);
        }
        else
          AddMessage(new Message(() =>
          {
            obj.IsTeleporting = false;
            obj.Position = newPos;
            if(!(obj is NPC))
              return;
            AddObjectNow(obj);
          }));
      }

      return true;
    }

    public bool MoveObject(WorldObject obj, Vector3 newPos)
    {
      return MoveObject(obj, ref newPos);
    }

    /// <summary>
    /// Moves the given object to the given position (does not animate movement)
    /// </summary>
    /// <param name="newPos">the position to move the entity to</param>
    /// <param name="obj">the entity to move</param>
    /// <returns>true if the entity was moved, false otherwise</returns>
    public bool MoveObject(WorldObject obj, ref Vector3 newPos)
    {
      if(m_root == null || !m_root.Bounds.Contains(ref newPos))
        return false;
      ZoneSpacePartitionNode leafFromPoint = m_root.GetLeafFromPoint(ref newPos);
      if(leafFromPoint == null)
        return false;
      ZoneSpacePartitionNode node = obj.Node;
      if(node == null)
        return false;
      MoveObject(obj, ref newPos, leafFromPoint, node);
      if(obj is Unit)
        ((Unit) obj).OnMove();
      return true;
    }

    private static void MoveObject(WorldObject obj, ref Vector3 newPos, ZoneSpacePartitionNode newNode,
      ZoneSpacePartitionNode curNode)
    {
      if(newNode != curNode)
      {
        curNode.RemoveObject(obj);
        if(!newNode.AddObject(obj))
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
      EnsureContext();
      return m_objects.Values.GetEnumerator();
    }

    /// <summary>
    /// Adds the given message to the map's message queue and does not return
    /// until the message is processed.
    /// </summary>
    /// <remarks>Make sure that the map is running before calling this method.</remarks>
    /// <remarks>Must not be called from the map context.</remarks>
    public void AddMessageAndWait(Action action)
    {
      AddMessageAndWait(new Message(action));
    }

    /// <summary>
    /// Adds the given message to the map's message queue and does not return
    /// until the message is processed.
    /// </summary>
    /// <remarks>Make sure that the map is running before calling this method.</remarks>
    /// <remarks>Must not be called from the map context.</remarks>
    public void AddMessageAndWait(IMessage msg)
    {
      EnsureNoContext();
      Start();
      SimpleUpdatable updatable = new SimpleUpdatable();
      updatable.Callback = () => AddMessage(new Message(() =>
      {
        msg.Execute();
        lock(msg)
          Monitor.PulseAll(msg);
        UnregisterUpdatable(updatable);
      }));
      lock(msg)
      {
        RegisterUpdatableLater(updatable);
        Monitor.Wait(msg);
      }
    }

    /// <summary>Waits for one map tick before returning.</summary>
    /// <remarks>Must not be called from the map context.</remarks>
    public void WaitOneTick()
    {
      EnsureNoContext();
      AddMessageAndWait(new Message(() => { }));
    }

    /// <summary>
    /// Waits for the given amount of ticks.
    /// One tick might take 0 until Map.UpdateSpeed milliseconds.
    /// </summary>
    /// <remarks>Make sure that the map is running before calling this method.</remarks>
    /// <remarks>Must not be called from the map context.</remarks>
    public void WaitTicks(int ticks)
    {
      EnsureNoContext();
      for(int index = 0; index < ticks; ++index)
        WaitOneTick();
    }

    public override string ToString()
    {
      return string.Format("{0} (Id: {1}{2}, Players: {3}{4} (Alliance: {5}, Horde: {6}))", (object) Name,
        (object) Id, InstanceId != 0U ? (object) (", #" + (object) InstanceId) : (object) "",
        (object) PlayerCount,
        MaxPlayerCount > 0 ? (object) (" / " + (object) MaxPlayerCount) : (object) "",
        (object) m_allyCount, (object) m_hordeCount);
    }

    /// <summary>
    /// Indicates whether this Map is disposed.
    /// Disposed Maps may not be used any longer.
    /// </summary>
    public bool IsDisposed
    {
      get { return m_IsDisposed; }
      protected set
      {
        m_IsDisposed = value;
        IsRunning = false;
      }
    }

    public bool IsAsda2FightingMap
    {
      get { return MapTemplate.IsAsda2FightingMap; }
    }

    public DeffenceTownEvent DefenceTownEvent { get; set; }

    protected virtual void Dispose()
    {
      m_running = false;
      m_root = null;
      m_objects = null;
      m_characters = null;
      m_spiritHealers = null;
      m_updatables = null;
    }

    /// <summary>Sends the given message to everyone</summary>
    public void SendMessage(string message)
    {
      ExecuteInContext(() =>
      {
        m_characters.SendSystemMessage(message);
        ChatMgr.ChatNotify(null, message, ChatLanguage.Universal, ChatMsgType.System,
          this);
      });
    }

    /// <summary>Sends the given message to everyone</summary>
    public void SendMessage(string message, params object[] args)
    {
      SendMessage(string.Format(message, args));
    }

    /// <summary>Is called whenever a Character dies.</summary>
    /// <param name="action"></param>
    protected internal virtual void OnPlayerDeath(IDamageAction action)
    {
      if(action.Attacker != null && action.Attacker.IsPvPing && action.Victim.YieldsXpOrHonor)
      {
        ((Character) action.Attacker).OnHonorableKill(action);
        OnHonorableKill(action);
      }

      if(!(action.Victim is Character))
        return;
      Character victim = action.Victim as Character;
      victim.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.DeathAtMap, (uint) MapId,
        1U, null);
      if(action.Attacker == null)
        return;
      if(action.Attacker is Character)
        victim.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KilledByPlayer,
          (uint) action.Attacker.FactionGroup, 1U, null);
      else
        victim.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KilledByCreature,
          action.Attacker.EntryId, 1U, null);
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
      if(npc == null || !npc.YieldsXpOrHonor)
        return;
      Character character = npc.FirstAttacker != null ? npc.FirstAttacker.PlayerOwner : null;
      if(character == null || XpCalculator == null)
        return;
      Character killer = character;
      int num1 = XpCalculator(character.Level, npc);
      float num2 = 0.0f;
      if(killer.IsInGroup)
      {
        foreach(GroupMember groupMember in killer.Group)
        {
          if(groupMember.Character != null)
            num2 += groupMember.Character.Asda2ExpAmountBoost - 1f;
        }
      }
      else
        num2 += killer.Asda2ExpAmountBoost - 1f;

      int num3 = (int) (num1 * (1.0 + num2));
      if(num3 < 0)
        num3 = 1;
      XpGenerator.CombatXpDistributer(killer, npc, num3);
      if(killer.Group == null)
        return;
      killer.Group.OnKill(killer, npc);
    }

    public virtual void Save()
    {
    }

    public void SpawnLoot(Asda2Loot loot)
    {
      if(!(loot.Lootable is Unit) || loot == null || loot.Lootable == null)
        return;
      loot.GiveMoney();
      if(loot.Items == null || loot.Items.Length == 0)
      {
        loot.Dispose();
      }
      else
      {
        bool flag = false;
        if(loot.Looters != null)
        {
          foreach(Asda2LooterEntry looter in loot.Looters)
          {
            if(looter != null && looter.Owner != null && looter.Owner.Level >=
               ((Unit) loot.Lootable).Level + CharacterFormulas.MaxLvlMobCharDiff)
            {
              foreach(Asda2LootItem asda2LootItem in loot.Items)
              {
                if(asda2LootItem != null)
                  asda2LootItem.Taken = true;
              }

              loot.Dispose();
              return;
            }
          }
        }

        if(loot.AutoLoot)
          flag = loot.GiveItems();
        if(flag)
          return;
        DropLoot(loot);
      }
    }

    private void DropLoot(Asda2Loot loot)
    {
      if(Loots == null)
        throw new Exception("Loots is null!");
      Unit lootable = (Unit) loot.Lootable;
      loot.Position = lootable.Position;
      loot.Map = lootable.Map;
      int length = loot.Items.Length;
      Loots.Add(loot);
      loot.LootPositions = FindFreeLootSlots(length, loot.Asda2Position.XY);
      if(loot.LootPositions == null)
      {
        LogUtil.WarnException("LootPosition is null {0}, {1}", (object) loot.Asda2Position.XY, (object) Name);
        loot.Dispose();
      }
      else
      {
        for(int index = 0; index < loot.Items.Length; ++index)
        {
          Vector2Short lootPosition = loot.LootPositions[index];
          loot.Items[index].Position = lootPosition;
          _lootPositions[lootPosition.X, lootPosition.Y] = loot.Items[index];
        }

        AddObjectLater(loot);
      }
    }

    private Vector2Short[] FindFreeLootSlots(int itemsCount, Vector2 xy)
    {
      Vector2Short[] vector2ShortArray = new Vector2Short[itemsCount];
      short num = 0;
      Vector2Short init = new Vector2Short((short) xy.X, (short) xy.Y);
      if(_lootPositions[init.X, init.Y] == null)
      {
        vector2ShortArray[num] = init;
        ++num;
        if(num >= itemsCount)
          return vector2ShortArray;
      }

      short offset = 1;
      while(true)
      {
        foreach(Vector2Short vector2Short in FindFreePositionsWithOffset(init, offset,
          (short) (vector2ShortArray.Length - num)))
          vector2ShortArray[num++] = vector2Short;
        if(num < vector2ShortArray.Length && offset <= 10)
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
        if(_lootPositions[init.X + offset, init.Y] == null)
        {
          source.Add(new Vector2Short((short) (init.X + offset), init.Y));
          ++count1;
          if(count1 == count)
            return source;
        }

        if(_lootPositions[init.X - offset, init.Y] == null)
        {
          source.Add(new Vector2Short((short) (init.X - offset), init.Y));
          ++count1;
          if(count1 == count)
            return source;
        }

        if(_lootPositions[init.X + offset, init.Y + offset] == null)
        {
          source.Add(new Vector2Short((short) (init.X + offset),
            (short) (init.Y + offset)));
          ++count1;
          if(count1 == count)
            return source;
        }

        if(_lootPositions[init.X - offset, init.Y + offset] == null)
        {
          source.Add(new Vector2Short((short) (init.X - offset),
            (short) (init.Y + offset)));
          ++count1;
          if(count1 == count)
            return source;
        }

        if(_lootPositions[init.X + offset, init.Y - offset] == null)
        {
          source.Add(new Vector2Short((short) (init.X + offset),
            (short) (init.Y - offset)));
          ++count1;
          if(count1 == count)
            return source;
        }

        if(_lootPositions[init.X - offset, init.Y - offset] == null)
        {
          source.Add(new Vector2Short((short) (init.X - offset),
            (short) (init.Y - offset)));
          ++count1;
          if(count1 == count)
            return source;
        }

        if(_lootPositions[init.X, init.Y + offset] == null)
        {
          source.Add(new Vector2Short(init.X, (short) (init.Y + offset)));
          ++count1;
          if(count1 == count)
            return source;
        }

        if(_lootPositions[init.X - offset, init.Y + offset] == null)
        {
          source.Add(new Vector2Short(init.X, (short) (init.Y - offset)));
          ++count1;
          if(count1 == count)
            return source;
        }

        return source.Take(count1).ToArray();
      }
      catch(IndexOutOfRangeException ex)
      {
        List<Vector2Short> vector2ShortList = new List<Vector2Short>(count);
        for(int index = 0; index < (int) count; ++index)
          vector2ShortList.Add(init);
        return vector2ShortList;
      }
    }

    public Asda2LootItem TryPickUpItem(short x, short y)
    {
      if(x > _lootPositions.Length || y > _lootPositions.Length)
        return null;
      return _lootPositions[x, y] ?? null;
    }

    public void ClearLootSlot(short x, short y)
    {
      Asda2LootItem lootPosition = _lootPositions[x, y];
      if(lootPosition == null)
        return;
      lootPosition.Taken = true;
      _lootPositions[x, y] = null;
    }

    public static IMessage GetInitializeCharacterTask(Character chr, Map rgn)
    {
      return new Message2<Character, Map>
      {
        Parameter1 = chr,
        Parameter2 = rgn,
        Callback = (initChr, initRgn) => initRgn.AddObjectNow(chr)
      };
    }

    public static IMessage GetRemoveObjectTask(WorldObject obj, Map rgn)
    {
      return new Message2<WorldObject, Map>
      {
        Parameter1 = obj,
        Parameter2 = rgn,
        Callback = (worldObj, objRgn) => objRgn.RemoveObjectNow(worldObj)
      };
    }

    public float Offset { get; set; }
  }
}