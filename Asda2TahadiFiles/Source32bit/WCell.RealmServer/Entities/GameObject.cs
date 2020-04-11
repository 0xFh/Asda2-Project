using NLog;
using System;
using System.IO;
using System.Threading;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Looting;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.Core.Paths;
using WCell.Core.Timers;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Quests;
using WCell.RealmServer.UpdateFields;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
  /// <summary>TODO: Respawning</summary>
  public class GameObject : WorldObject, IOwned, ILockable, ILootable, IQuestHolder, IEntity
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.GameObject);
    private static readonly double RotatationConst = Math.Atan(Math.Pow(2.0, -20.0));
    internal static int _lastGOUID;
    protected GOEntry m_entry;
    protected Faction m_faction;
    protected GOSpawnPoint m_spawnPoint;
    protected GameObjectHandler m_handler;
    protected bool m_respawns;
    protected TimerEntry m_decayTimer;
    protected GameObject m_linkedTrap;
    protected internal bool m_IsTrap;

    /// <summary>
    /// Creates the given kind of GameObject with the default Template
    /// </summary>
    public static GameObject Create(GOEntryId id, IWorldLocation location, GOSpawnEntry spawnEntry = null,
      GOSpawnPoint spawnPoint = null)
    {
      GOEntry entry = GOMgr.GetEntry(id, true);
      if(entry == null)
        return null;
      return Create(entry, location, spawnEntry, spawnPoint);
    }

    /// <summary>Creates a new GameObject with the given parameters</summary>
    public static GameObject Create(GOEntryId id, Map map, GOSpawnEntry spawnEntry = null,
      GOSpawnPoint spawnPoint = null)
    {
      GOEntry entry = GOMgr.GetEntry(id, true);
      if(entry != null)
        return Create(entry, map, spawnEntry, spawnPoint);
      return null;
    }

    public static GameObject Create(GOEntry entry, Map map, GOSpawnEntry spawnEntry = null,
      GOSpawnPoint spawnPoint = null)
    {
      return Create(entry, new WorldLocation(map, Vector3.Zero, 1U), spawnEntry,
        spawnPoint);
    }

    /// <summary>Creates a new GameObject with the given parameters</summary>
    public static GameObject Create(GOEntry entry, IWorldLocation where, GOSpawnEntry spawnEntry = null,
      GOSpawnPoint spawnPoint = null)
    {
      GameObject gameObject = entry.GOCreator();
      gameObject.GoId = entry.GOId;
      Func<GameObjectHandler> handlerCreator = entry.HandlerCreator;
      gameObject.Init(entry, spawnEntry, spawnPoint);
      if(handlerCreator != null)
      {
        gameObject.Handler = handlerCreator();
        gameObject.Phase = where.Phase;
        Vector3 position = where.Position;
        if(spawnPoint == null)
          position.Z = where.Map.Terrain.GetGroundHeightUnderneath(position);
        where.Map.AddObject(gameObject, ref position);
        gameObject.MarkUpdate(GameObjectFields.DYNAMIC);
        return gameObject;
      }

      log.Warn("GOEntry {0} did not have a HandlerCreator set - Type: {1}", entry,
        entry.Type);
      gameObject.Delete();
      return null;
    }

    /// <summary>Initialize the GO</summary>
    /// <param name="entry"></param>
    /// <param name="templ"></param>
    internal virtual void Init(GOEntry entry, GOSpawnEntry spawnEntry, GOSpawnPoint spawnPoint)
    {
      EntityId =
        EntityId.GetGameObjectId((uint) Interlocked.Increment(ref _lastGOUID), entry.GOId);
      Type |= ObjectTypes.GameObject;
      m_entry = entry;
      m_spawnPoint = spawnPoint;
      GoId = entry.GOId;
      DisplayId = entry.DisplayId;
      EntryId = entry.Id;
      GOType = entry.Type;
      Flags = m_entry.Flags;
      m_faction = m_entry.Faction ?? Faction.NullFaction;
      ScaleX = m_entry.Scale;
      GossipMenu = entry.DefaultGossip;
      if(QuestHolderInfo != null && GossipMenu == null)
        GossipMenu = new GossipMenu();
      spawnEntry = spawnEntry ?? entry.FirstSpawnEntry;
      if(spawnEntry != null)
      {
        Phase = spawnEntry.Phase;
        State = spawnEntry.State;
        if(spawnEntry.Scale != 1.0)
          ScaleX = spawnEntry.Scale;
        Orientation = spawnEntry.Orientation;
        AnimationProgress = spawnEntry.AnimProgress;
        SetRotationFields(spawnEntry.Rotations);
      }

      m_entry.InitGO(this);
    }

    public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
    {
      get { return UpdateFieldHandler.DynamicGOHandlers; }
    }

    public GOEntryId GoId { get; set; }

    public LockEntry Lock
    {
      get { return m_entry.Lock; }
    }

    public override void OnFinishedLooting()
    {
      if(!m_entry.IsConsumable)
        return;
      Delete();
    }

    public override uint GetLootId(Asda2LootEntryType type)
    {
      if(m_entry is IGOLootableEntry)
        return ((IGOLootableEntry) m_entry).LootId;
      return 0;
    }

    public override bool UseGroupLoot
    {
      get { return m_entry.UseGroupLoot; }
    }

    protected internal override void OnEnterMap()
    {
      if(m_entry.LinkedTrap != null)
        m_linkedTrap = m_entry.LinkedTrap.Spawn(this, m_master);
      if(m_spawnPoint != null)
        m_spawnPoint.SignalSpawnlingActivated(this);
      m_entry.NotifyActivated(this);
    }

    protected internal override void OnLeavingMap()
    {
      if(m_master is Character && m_master.IsInWorld)
        ((Character) m_master).OnOwnedGODestroyed(this);
      m_handler.OnRemove();
      SendDespawn();
      base.OnLeavingMap();
    }

    public bool IsCloseEnough(Unit unit, float radius = 10f)
    {
      if(unit.IsInRadius(this, radius))
        return true;
      if(unit is Character)
        return ((Character) unit).Role.IsStaff;
      return false;
    }

    public bool CanUseInstantly(Character chr)
    {
      if(!IsCloseEnough(chr, 10f) || Lock != null)
        return false;
      return CanBeUsedBy(chr);
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CanBeUsedBy(Character chr)
    {
      if(!IsEnabled)
        return false;
      if(Flags.HasFlag(GameObjectFlags.ConditionalInteraction))
        return chr.QuestLog.IsRequiredForAnyQuest(this);
      return true;
    }

    /// <summary>
    /// Makes the given Unit use this GameObject.
    /// Skill-locked GameObjects cannot be used directly but must be interacted on with spells.
    /// </summary>
    public bool Use(Character chr)
    {
      if(Lock != null && !Lock.IsUnlocked && Lock.Keys.Length <= 0 || !Handler.TryUse(chr))
        return false;
      if(Entry.PageId != 0U)
        MiscHandler.SendGameObjectTextPage(chr, this);
      if(GossipMenu != null)
        chr.StartGossip(GossipMenu, this);
      chr.QuestLog.OnUse(this);
      return true;
    }

    /// <summary>
    /// Lets the given user try to loot this object.
    /// Called on Chests automatically when using Chest-GOs.
    /// </summary>
    public bool TryLoot(Character chr)
    {
      return LootMgr.TryLoot(this, chr);
    }

    /// <summary>
    /// All available Quest information, in case that this is a QuestGiver
    /// </summary>
    public QuestHolderInfo QuestHolderInfo
    {
      get { return m_entry.QuestHolderInfo; }
      internal set { m_entry.QuestHolderInfo = value; }
    }

    public bool CanGiveQuestTo(Character chr)
    {
      return IsInRadiusSq(chr, GOMgr.DefaultInteractDistanceSq);
    }

    public void OnQuestGiverStatusQuery(Character chr)
    {
    }

    private void DecayNow(int dt)
    {
      Delete();
    }

    protected internal override void DeleteNow()
    {
      if(m_spawnPoint != null)
        m_spawnPoint.SignalSpawnlingDied(this);
      if(m_linkedTrap != null)
        m_linkedTrap.DeleteNow();
      base.DeleteNow();
    }

    private void StopDecayTimer()
    {
      if(m_decayTimer == null)
        return;
      m_decayTimer.Stop();
      m_decayTimer = null;
    }

    /// <summary>
    /// Can be set to initialize Decay after the given delay in seconds.
    /// Will stop the timer if set to a value less than 0
    /// </summary>
    public int RemainingDecayDelayMillis
    {
      get { return m_decayTimer.RemainingInitialDelayMillis; }
      set
      {
        if(value < 0)
        {
          StopDecayTimer();
        }
        else
        {
          m_decayTimer = new TimerEntry(DecayNow);
          m_decayTimer.Start(value, 0);
        }
      }
    }

    public override void Update(int dt)
    {
      base.Update(dt);
      if(m_decayTimer == null)
        return;
      m_decayTimer.Update(dt);
    }

    protected override UpdateFieldCollection _UpdateFieldInfos
    {
      get { return UpdateFieldInfos; }
    }

    public override ObjectTypeId ObjectTypeId
    {
      get { return ObjectTypeId.GameObject; }
    }

    public override UpdateFlags UpdateFlags
    {
      get
      {
        return UpdateFlags.Flag_0x10 | UpdateFlags.StationaryObject | UpdateFlags.StationaryObjectOnTransport |
               UpdateFlags.HasRotation;
      }
    }

    protected override UpdateType GetCreationUpdateType(UpdateFieldFlags relation)
    {
      return m_entry is GODuelFlagEntry ? UpdateType.CreateSelf : UpdateType.Create;
    }

    protected override void WriteMovementUpdate(PrimitiveWriter packet, UpdateFieldFlags relation)
    {
      if(UpdateFlags.HasAnyFlag(UpdateFlags.StationaryObjectOnTransport))
      {
        EntityId.Zero.WritePacked(packet);
        packet.Write(Position);
        packet.Write(Position);
        packet.Write(Orientation);
        packet.Write(0.0f);
      }
      else
      {
        if(!UpdateFlags.HasAnyFlag(UpdateFlags.StationaryObject))
          return;
        packet.Write(Position);
        packet.WriteFloat(Orientation);
      }
    }

    protected override void WriteTypeSpecificMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation,
      UpdateFlags updateFlags)
    {
      if(updateFlags.HasAnyFlag(UpdateFlags.Transport))
        writer.Write(Utility.GetSystemTime());
      if(!updateFlags.HasAnyFlag(UpdateFlags.HasRotation))
        return;
      writer.Write(Rotation);
    }

    public override string ToString()
    {
      return m_entry.DefaultName + " (SpawnPoint: " + m_spawnPoint + ")";
    }

    public GossipMenu GossipMenu { get; set; }

    public GameObjectHandler Handler
    {
      get { return m_handler; }
      set
      {
        m_handler = value;
        m_handler.Initialize(this);
      }
    }

    public override string Name
    {
      get
      {
        if(m_entry == null)
          return "";
        return m_entry.DefaultName;
      }
      set { throw new NotImplementedException("Dynamic renaming of GOs is not implementable."); }
    }

    public GOEntry Entry
    {
      get { return m_entry; }
    }

    public override ObjectTemplate Template
    {
      get { return Entry; }
    }

    /// <summary>The Template of this GO (if any was used)</summary>
    public GOSpawnPoint SpawnPoint
    {
      get { return m_spawnPoint; }
    }

    /// <summary>Traps get removed when their AreaAura gets removed</summary>
    public override bool IsTrap
    {
      get { return m_IsTrap; }
    }

    public EntityId CreatedBy
    {
      get { return GetEntityId(GameObjectFields.OBJECT_FIELD_CREATED_BY); }
      set { SetEntityId(GameObjectFields.OBJECT_FIELD_CREATED_BY, value); }
    }

    public uint DisplayId
    {
      get { return GetUInt32(GameObjectFields.DISPLAYID); }
      set { SetUInt32(GameObjectFields.DISPLAYID, value); }
    }

    public GameObjectFlags Flags
    {
      get { return (GameObjectFlags) GetUInt32(GameObjectFields.FLAGS); }
      set { SetUInt32(GameObjectFields.FLAGS, (uint) value); }
    }

    public bool IsStealthed { get; set; }

    public bool IsEnabled
    {
      get { return GetByte(GameObjectFields.BYTES_1, 0) == 1; }
      set { SetByte(GameObjectFields.BYTES_1, 0, value ? (byte) 1 : (byte) 0); }
    }

    public GameObjectState State
    {
      get { return (GameObjectState) GetByte(GameObjectFields.BYTES_1, 0); }
      set { SetByte(GameObjectFields.BYTES_1, 0, (byte) value); }
    }

    public GameObjectType GOType
    {
      get { return (GameObjectType) GetByte(GameObjectFields.BYTES_1, 1); }
      set { SetByte(GameObjectFields.BYTES_1, 1, (byte) value); }
    }

    /// <summary>No idea</summary>
    public byte ArtKit
    {
      get { return GetByte(GameObjectFields.BYTES_1, 2); }
      set { SetByte(GameObjectFields.BYTES_1, 2, value); }
    }

    /// <summary>Seems to be 0 or 100 mostly</summary>
    public byte AnimationProgress
    {
      get { return GetByte(GameObjectFields.BYTES_1, 3); }
      set { SetByte(GameObjectFields.BYTES_1, 3, value); }
    }

    public byte[] Dynamic
    {
      get { return GetByteArray(GameObjectFields.DYNAMIC); }
      set { SetByteArray(GameObjectFields.DYNAMIC, value); }
    }

    public override Faction Faction
    {
      get { return m_faction; }
      set
      {
        m_faction = value;
        SetUInt32(GameObjectFields.FACTION, value.Template.Id);
      }
    }

    public override FactionId FactionId
    {
      get { return m_faction.Id; }
      set
      {
        Faction faction = FactionMgr.Get(value);
        if(faction != null)
          Faction = faction;
        else
          SetUInt32(GameObjectFields.FACTION, (uint) value);
      }
    }

    public int Level
    {
      get { return GetInt32(GameObjectFields.LEVEL); }
      set { SetInt32(GameObjectFields.LEVEL, value); }
    }

    public Unit Owner
    {
      get { return m_master; }
      set
      {
        Master = value;
        if(value != null)
        {
          Faction = value.Faction;
          Level = value.Level;
        }
        else
          Faction = Faction.NullFaction;
      }
    }

    public float ParentRotation1
    {
      get { return GetFloat(GameObjectFields.PARENTROTATION); }
      set { SetFloat(GameObjectFields.PARENTROTATION, value); }
    }

    public float ParentRotation2
    {
      get { return GetFloat(GameObjectFields.PARENTROTATION_2); }
      set { SetFloat(GameObjectFields.PARENTROTATION_2, value); }
    }

    public float ParentRotation3
    {
      get { return GetFloat(GameObjectFields.PARENTROTATION_3); }
      set { SetFloat(GameObjectFields.PARENTROTATION_3, value); }
    }

    public float ParentRotation4
    {
      get { return GetFloat(GameObjectFields.PARENTROTATION_4); }
      set { SetFloat(GameObjectFields.PARENTROTATION_4, value); }
    }

    public long Rotation { get; set; }

    protected void SetRotationFields(float[] rotations)
    {
      if(rotations.Length != 4)
        return;
      SetFloat(GameObjectFields.PARENTROTATION, rotations[0]);
      SetFloat(GameObjectFields.PARENTROTATION_2, rotations[1]);
      double num1 = Math.Sin(Orientation / 2.0);
      double num2 = Math.Cos(Orientation / 2.0);
      Rotation = (long) (num1 / RotatationConst * (num2 >= 0.0 ? 1.0 : -1.0)) & 2097151L;
      if(rotations[2] == 0.0 && rotations[3] == 0.0)
      {
        SetFloat(GameObjectFields.PARENTROTATION_3, (float) num1);
        SetFloat(GameObjectFields.PARENTROTATION_4, (float) num2);
      }
      else
      {
        SetFloat(GameObjectFields.PARENTROTATION_3, rotations[2]);
        SetFloat(GameObjectFields.PARENTROTATION_4, rotations[3]);
      }
    }

    public override ObjectTypeCustom CustomType
    {
      get { return ObjectTypeCustom.Object | ObjectTypeCustom.GameObject; }
    }

    public void SendCustomAnim(uint anim)
    {
      using(RealmPacketOut packet =
        new RealmPacketOut(RealmServerOpCode.SMSG_GAMEOBJECT_CUSTOM_ANIM, 12))
      {
        packet.Write(EntityId);
        packet.Write(anim);
        SendPacketToArea(packet, true, true, WCell.Core.Network.Locale.Any, new float?());
      }
    }

    public void SendDespawn()
    {
      using(RealmPacketOut packet =
        new RealmPacketOut(RealmServerOpCode.SMSG_GAMEOBJECT_DESPAWN_ANIM, 8))
      {
        packet.Write(EntityId);
        SendPacketToArea(packet, true, true, WCell.Core.Network.Locale.Any, new float?());
      }
    }
  }
}