using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.Core.Paths;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.UpdateFields;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.Graphics;
using WCell.Util.NLog;
using WCell.Util.ObjectPools;
using WCell.Util.Threading;
using WCell.Util.Variables;

namespace WCell.RealmServer.Entities
{
  [Serializable]
  public abstract class WorldObject : ObjectBase, IFactionMember, IWorldLocation, IHasPosition, INamedEntity, IEntity,
    INamed, IContextHandler
  {
    private static Logger log = LogManager.GetCurrentClassLogger();

    public static readonly ObjectPool<HashSet<WorldObject>> WorldObjectSetPool =
      ObjectPoolMgr.CreatePool(
        () => new HashSet<WorldObject>());

    public static readonly ObjectPool<List<WorldObject>> WorldObjectListPool =
      ObjectPoolMgr.CreatePool(() => new List<WorldObject>());

    /// <summary>
    /// Default vision range. Characters will only receive packets of what happens within this range (unit: Yards)
    /// </summary>
    public static float BroadcastRange = 50f;

    public static float BroadcastRangeNpc = 35f;
    public static readonly WorldObject[] EmptyArray = new WorldObject[0];
    public static readonly List<WorldObject> EmptyList = new List<WorldObject>();
    public static UpdatePriority DefaultObjectUpdatePriority = UpdatePriority.LowPriority;
    public static float HighlightScale = 5f;
    public static int HighlightDelayMillis = 1500;
    protected UpdatePriority m_UpdatePriority = DefaultObjectUpdatePriority;
    protected uint m_Phase = 1;

    /// <summary>
    /// Messages to be processed during the next Update (which ensures that it will be within the Object's context)
    /// </summary>
    internal readonly LockfreeQueue<IMessage> m_messageQueue = new LockfreeQueue<IMessage>();

    /// <summary>
    /// Default phase (the one that existed long before phasing was added)
    /// </summary>
    public const uint DefaultPhase = 1;

    /// <summary>All phases</summary>
    public const uint AllPhases = 4294967295;

    public const float InFrontAngleMin = 5.235988f;
    public const float InFrontAngleMax = 1.047198f;
    public const float BehindAngleMin = 2.094395f;
    public const float BehindAngleMax = 4.18879f;
    protected DateTime m_lastUpdateTime;
    protected List<ObjectUpdateTimer> m_updateActions;
    protected Vector3 m_position;

    /// <summary>never null</summary>
    protected Map m_Map;

    internal ZoneSpacePartitionNode Node;
    protected Zone m_zone;
    protected float m_orientation;
    protected SpellCast m_spellCast;
    protected List<AreaAura> m_areaAuras;
    protected ObjectReference m_CasterReference;
    protected Unit m_master;
    protected int m_areaCharCount;
    public readonly uint CreationTime;
    protected bool m_Deleted;

    protected internal virtual void OnEnterMap()
    {
    }

    protected internal virtual void OnLeavingMap()
    {
    }

    /// <summary>
    /// The queue of messages. Messages are executed on every map tick.
    /// </summary>
    public LockfreeQueue<IMessage> MessageQueue
    {
      get { return m_messageQueue; }
    }

    public DateTime LastUpdateTime
    {
      get { return m_lastUpdateTime; }
      internal set { m_lastUpdateTime = value; }
    }

    public override UpdatePriority UpdatePriority
    {
      get { return m_UpdatePriority; }
    }

    public void SetUpdatePriority(UpdatePriority priority)
    {
      m_UpdatePriority = priority;
    }

    public OneShotObjectUpdateTimer CallDelayed(int millis, Action<WorldObject> callback)
    {
      OneShotObjectUpdateTimer objectUpdateTimer = new OneShotObjectUpdateTimer(millis, callback);
      AddUpdateAction(objectUpdateTimer);
      return objectUpdateTimer;
    }

    /// <summary>
    /// Adds a new Action to the list of Actions to be executed every millis.
    /// </summary>
    /// <param name="callback"></param>
    public ObjectUpdateTimer CallPeriodically(int millis, Action<WorldObject> callback)
    {
      ObjectUpdateTimer timer = new ObjectUpdateTimer(millis, callback);
      AddUpdateAction(timer);
      return timer;
    }

    /// <summary>
    /// Adds a new Action to the list of Actions to be executed every millis.
    /// </summary>
    /// <param name="callback"></param>
    public ObjectUpdateTimer CallPeriodicallyUntil(int callIntervalMillis, int callUntilMillis,
      Action<WorldObject> callback)
    {
      ObjectUpdateTimer action = new ObjectUpdateTimer(callIntervalMillis, callback);
      AddUpdateAction(action);
      CallDelayed(callUntilMillis, obj => RemoveUpdateAction(action));
      return action;
    }

    /// <summary>
    /// Adds a new Action to the list of Actions to be executed every action.Delay milliseconds
    /// </summary>
    public void AddUpdateAction(ObjectUpdateTimer timer)
    {
      if(m_updateActions == null)
        m_updateActions = new List<ObjectUpdateTimer>(3);
      timer.LastCallTime = m_lastUpdateTime;
      m_updateActions.Add(timer);
    }

    public bool HasUpdateAction(Func<ObjectUpdateTimer, bool> predicate)
    {
      EnsureContext();
      if(m_updateActions != null)
        return m_updateActions.Any(predicate);
      return false;
    }

    public void RemoveUpdateAction(Action<WorldObject> callback)
    {
      if(m_updateActions == null)
        return;
      ExecuteInContext(() =>
      {
        ObjectUpdateTimer timer =
          m_updateActions.FirstOrDefault(
            act => act.Callback == callback);
        if(timer == null)
          return;
        RemoveUpdateAction(timer);
      });
    }

    /// <summary>Removes the given Action</summary>
    /// <param name="timer"></param>
    public bool RemoveUpdateAction(ObjectUpdateTimer timer)
    {
      return m_updateActions != null && m_updateActions.Remove(timer);
    }

    /// <summary>
    /// Make sure to call this before updating anything else (required for reseting UpdateInfo)
    /// </summary>
    public virtual void Update(int dt)
    {
      IMessage message;
      while(m_messageQueue.TryDequeue(out message))
      {
        try
        {
          message.Execute();
        }
        catch(Exception ex)
        {
          LogUtil.ErrorException(ex, "Exception raised when processing Message for: {0}", (object) this);
          Delete();
        }
      }

      if(m_areaAuras != null)
      {
        int count = m_areaAuras.Count;
        for(int index = 0; index < count; ++index)
        {
          m_areaAuras[index].Update(dt);
          if(m_areaAuras.Count != count)
            break;
        }
      }

      if(m_spellCast != null)
        m_spellCast.Update(dt);
      if(m_updateActions == null)
        return;
      for(int index = m_updateActions.Count - 1; index >= 0; --index)
      {
        ObjectUpdateTimer updateAction = m_updateActions[index];
        if(updateAction.Delay == 0)
          updateAction.Execute(this);
        else if((m_lastUpdateTime - updateAction.LastCallTime).ToMilliSecondsInt() >= updateAction.Delay)
          updateAction.Execute(this);
      }
    }

    protected override UpdateType GetCreationUpdateType(UpdateFieldFlags relation)
    {
      return relation.HasAnyFlag(UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly)
        ? UpdateType.CreateSelf
        : UpdateType.Create;
    }

    public override void RequestUpdate()
    {
      m_requiresUpdate = true;
    }

    /// <summary>
    /// The current <see cref="T:WCell.Util.Threading.IContextHandler" /> of this Character.
    /// </summary>
    public IContextHandler ContextHandler
    {
      get { return m_Map; }
    }

    /// <summary>
    /// Whether this object is in the world and within the current
    /// execution context.
    /// </summary>
    public bool IsInContext
    {
      get
      {
        if(IsInWorld)
        {
          IContextHandler contextHandler = ContextHandler;
          if(contextHandler != null && contextHandler.IsInContext)
            return true;
        }

        return false;
      }
    }

    public void EnsureContext()
    {
      if(!IsInWorld)
        return;
      IContextHandler contextHandler = ContextHandler;
      if(contextHandler == null)
        return;
      contextHandler.EnsureContext();
    }

    public ushort UniqIdOnMap { get; set; }

    protected bool HasNode
    {
      get { return Node != null; }
    }

    protected WorldObject()
    {
      CreationTime = Utility.GetSystemTime();
      LastUpdateTime = DateTime.Now;
    }

    public virtual ObjectTemplate Template
    {
      get { return null; }
    }

    /// <summary>Time in seconds since creation</summary>
    public uint Age
    {
      get { return Utility.GetSystemTime() - CreationTime; }
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual uint Phase
    {
      get { return m_Phase; }
      set { m_Phase = value; }
    }

    /// <summary>The current position of the object</summary>
    public virtual Vector3 Position
    {
      get { return m_position; }
      internal set { m_position = value; }
    }

    public float Asda2X
    {
      get
      {
        if(Map != null)
          return Position.X - Map.Offset;
        return 0.0f;
      }
      set { Position = new Vector3(value + Map.Offset, Position.Y); }
    }

    public float Asda2Y
    {
      get
      {
        if(Map != null)
          return Position.Y - Map.Offset;
        return 0.0f;
      }
      set { Position = new Vector3(Position.Y, value + Map.Offset); }
    }

    public Vector3 Asda2Position
    {
      get { return new Vector3(Asda2X, Asda2Y); }
    }

    /// <summary>The current zone of the object</summary>
    public virtual Zone Zone
    {
      get { return m_zone; }
      internal set { m_zone = value; }
    }

    public virtual void SetZone(Zone zone)
    {
      Zone = zone;
    }

    public ZoneTemplate ZoneTemplate
    {
      get
      {
        if(m_zone == null)
          return null;
        return m_zone.Template;
      }
    }

    public ZoneId ZoneId
    {
      get
      {
        if(m_zone == null)
          return ZoneId.None;
        return m_zone.Id;
      }
    }

    /// <summary>
    /// The current map of the object.
    /// Map must (and will) never be null.
    /// </summary>
    public virtual Map Map
    {
      get { return m_Map; }
      internal set { m_Map = value; }
    }

    public MapId MapId
    {
      get
      {
        if(m_Map == null)
          return MapId.End;
        return m_Map.Id;
      }
    }

    /// <summary>The heading of the object (direction it is facing)</summary>
    public float Orientation
    {
      get { return m_orientation; }
      set { m_orientation = value; }
    }

    public override bool IsInWorld
    {
      get { return m_Map != null; }
    }

    public abstract string Name { get; set; }

    /// <summary>TODO: Find correct caster-level for non-units</summary>
    public virtual int CasterLevel
    {
      get { return 0; }
    }

    public ObjectReference SharedReference
    {
      get
      {
        if(m_CasterReference == null)
          m_CasterReference = CreateCasterInfo();
        m_CasterReference.Level = CasterLevel;
        return m_CasterReference;
      }
    }

    /// <summary>Whether there are active Characters in the Area</summary>
    public bool IsAreaActive
    {
      get { return m_areaCharCount > 0; }
    }

    /// <summary>The amount of Characters nearby.</summary>
    public int AreaCharCount
    {
      get { return m_areaCharCount; }
      internal set { m_areaCharCount = value; }
    }

    protected internal virtual void OnEncounteredBy(Character chr)
    {
      ++AreaCharCount;
    }

    public virtual bool IsTrap
    {
      get { return false; }
    }

    public bool IsCorpse
    {
      get
      {
        if(!(this is Corpse))
          return IsNPCCorpse;
        return true;
      }
    }

    public bool IsNPCCorpse
    {
      get
      {
        if(this is NPC)
          return !((Unit) this).IsAlive;
        return false;
      }
    }

    /// <summary>
    /// whether this Object is currently casting or channeling a Spell
    /// </summary>
    public bool IsUsingSpell
    {
      get
      {
        if(m_spellCast == null)
          return false;
        if(!m_spellCast.IsCasting)
          return m_spellCast.IsChanneling;
        return true;
      }
    }

    public void SetSpellCast(SpellCast cast)
    {
      if(m_spellCast != null && m_spellCast != cast)
        m_spellCast.Dispose();
      m_spellCast = cast;
    }

    /// <summary>
    /// Set to the SpellCast-object of this Object.
    /// If the Object is not in the world, will return null
    /// </summary>
    public SpellCast SpellCast
    {
      get
      {
        if(m_spellCast == null)
        {
          m_spellCast = SpellCast.ObtainPooledCast(this);
          InitSpellCast();
        }

        return m_spellCast;
      }
      internal set
      {
        if(value == m_spellCast)
          return;
        m_spellCast = value;
      }
    }

    public float GetSpellMaxRange(Spell spell)
    {
      return GetSpellMaxRange(spell, spell.Range.MaxDist);
    }

    public float GetSpellMaxRange(Spell spell, float range)
    {
      return GetSpellMaxRange(spell, null, range);
    }

    public float GetSpellMaxRange(Spell spell, WorldObject target)
    {
      return GetSpellMaxRange(spell, target, spell.Range.MaxDist);
    }

    public float GetSpellMaxRange(Spell spell, WorldObject target, float range)
    {
      if(target is Unit)
        range += ((Unit) target).CombatReach;
      if(this is Unit)
      {
        range += ((Unit) this).CombatReach + ((Unit) this).IntMods[32];
        range = ((Unit) this).Auras.GetModifiedFloat(SpellModifierType.Range, spell, range);
      }

      return range;
    }

    public float GetSpellMinRange(float range, WorldObject target)
    {
      if(target is Unit)
        range += ((Unit) target).CombatReach;
      if(this is Unit)
        range += ((Unit) this).CombatReach;
      return range;
    }

    /// <summary>
    /// Can be used to slow down execution of methods that:
    /// 	1. Should not be executed too often
    /// 	2. Don't need to be timed precisely
    /// For example: AI updates
    /// </summary>
    public bool CheckTicks(int ticks)
    {
      if(ticks != 0)
        return (Map.TickCount + EntityId.Low) % ticks == 0L;
      return true;
    }

    /// <summary>
    /// Creates a new CasterInfo object to represent this WorldObject
    /// </summary>
    protected ObjectReference CreateCasterInfo()
    {
      return new ObjectReference(this);
    }

    protected virtual void InitSpellCast()
    {
    }

    public bool IsInPhase(uint phase)
    {
      return ((int) Phase & (int) phase) != 0;
    }

    public bool IsInPhase(WorldObject obj)
    {
      return ((int) Phase & (int) obj.Phase) != 0;
    }

    public void SetOrientationTowards(ref Vector3 pos)
    {
      m_orientation = GetAngleTowards(ref pos);
    }

    public void SetOrientationTowards(IHasPosition pos)
    {
      m_orientation = GetAngleTowards(pos);
    }

    public virtual bool SetPosition(Vector3 pt)
    {
      return m_Map.MoveObject(this, ref pt);
    }

    public virtual bool SetPosition(Vector3 pt, float orientation)
    {
      if(!m_Map.MoveObject(this, ref pt))
        return false;
      m_orientation = orientation;
      return true;
    }

    /// <summary>
    /// The Master of this Object (Units are their own Masters if not controlled, Objects might have masters that they belong to)
    /// </summary>
    public Unit Master
    {
      get
      {
        if(m_master != null && !m_master.IsInWorld)
          m_master = null;
        return m_master;
      }
      protected internal set
      {
        if(value == m_master)
          return;
        if(value != null)
        {
          Faction = value.Faction;
          if(value is Character && this is Unit)
          {
            ((Unit) this).UnitFlags |= UnitFlags.PlayerControlled;
            if(this is NPC)
              ((NPC) this).m_spawnPoint = null;
          }
        }
        else if(this is Unit)
        {
          Faction = ((Unit) this).DefaultFaction;
          ((Unit) this).UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 |
                                     UnitFlags.SelectableNotAttackable | UnitFlags.Influenced |
                                     UnitFlags.Flag_0x10 | UnitFlags.Preparation | UnitFlags.PlusMob |
                                     UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable |
                                     UnitFlags.Passive | UnitFlags.Looting | UnitFlags.PetInCombat |
                                     UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced |
                                     UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 |
                                     UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat |
                                     UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused |
                                     UnitFlags.Feared | UnitFlags.NotSelectable | UnitFlags.Skinnable |
                                     UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 |
                                     UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 |
                                     UnitFlags.Flag_31_0x80000000;
        }

        m_master = value;
      }
    }

    public bool HasMaster
    {
      get
      {
        if(Master != null)
          return m_master != this;
        return false;
      }
    }

    /// <summary>
    /// Either this or the master as Character.
    /// Returns null if neither is Character.
    /// </summary>
    public Character CharacterMaster
    {
      get
      {
        if(this is Character)
          return (Character) this;
        return m_master as Character;
      }
    }

    /// <summary>
    /// Either this or the master as Unit.
    /// Returns null if neither is Unit.
    /// </summary>
    public Unit UnitMaster
    {
      get
      {
        if(this is Unit)
          return (Unit) this;
        return m_master;
      }
    }

    /// <summary>The Terrain underneath this object's current location</summary>
    public float TerrainHeight
    {
      get { return Map.Terrain.GetGroundHeightUnderneath(m_position + ScaleX); }
    }

    /// <summary>
    /// Indicates whether the given radius is within the given distance to this object
    /// </summary>
    public bool IsInRadius(ref Vector3 pt, float distance)
    {
      return GetDistanceSq(ref pt) <= distance * (double) distance;
    }

    /// <summary>
    /// Indicates whether the given radius is within the given distance to this object
    /// </summary>
    public bool IsInRadius(Vector3 pt, float distance)
    {
      return GetDistanceSq(ref pt) <= distance * (double) distance;
    }

    public bool IsInRadius(WorldObject obj, float distance)
    {
      return GetDistanceSq(obj) <= distance * (double) distance;
    }

    public bool IsInRadius(ref Vector3 pt, SimpleRange range)
    {
      float distanceSq = GetDistanceSq(ref pt);
      if(distanceSq > range.MaxDist * (double) range.MaxDist)
        return false;
      if(range.MinDist >= 1.0)
        return distanceSq >= range.MinDist * (double) range.MinDist;
      return true;
    }

    public bool IsInRadius(WorldObject obj, SimpleRange range)
    {
      float distanceSq = GetDistanceSq(obj);
      if(distanceSq > range.MaxDist * (double) range.MaxDist)
        return false;
      if(range.MinDist >= 1.0)
        return distanceSq >= range.MinDist * (double) range.MinDist;
      return true;
    }

    /// <summary>Indicates whether the given obj is in update range</summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool IsInUpdateRange(WorldObject obj)
    {
      return GetDistanceSq(obj) <=
             BroadcastRange * (double) BroadcastRange;
    }

    /// <summary>
    /// Indicates whether the given radius is within the given distance to this object
    /// </summary>
    public bool IsInRadiusSq(ref Vector3 pt, float sqDistance)
    {
      return GetDistanceSq(ref pt) <= (double) sqDistance;
    }

    public bool IsInRadiusSq(Vector3 pos, float sqDistance)
    {
      return GetDistanceSq(ref pos) <= (double) sqDistance;
    }

    public bool IsInRadiusSq(IHasPosition pos, float sqDistance)
    {
      return GetDistanceSq(pos) <= (double) sqDistance;
    }

    public float GetDistance(ref Vector3 pt)
    {
      float num1 = pt.X - m_position.X;
      float num2 = pt.Y - m_position.Y;
      return (float) Math.Sqrt(num1 * (double) num1 + num2 * (double) num2);
    }

    public float GetDistanceSq(ref Vector3 pt)
    {
      float num1 = pt.X - m_position.X;
      float num2 = pt.Y - m_position.Y;
      return (float) (num1 * (double) num1 + num2 * (double) num2);
    }

    public float GetDistance(Vector3 pt)
    {
      float num1 = pt.X - m_position.X;
      float num2 = pt.Y - m_position.Y;
      return (float) Math.Sqrt(num1 * (double) num1 + num2 * (double) num2);
    }

    public float GetDistanceSq(Vector3 pt)
    {
      float num1 = pt.X - m_position.X;
      float num2 = pt.Y - m_position.Y;
      return (float) (num1 * (double) num1 + num2 * (double) num2);
    }

    /// <summary>
    /// Less precise method to get the square distance to a point
    /// </summary>
    public int GetDistanceSqInt(ref Vector3 pt)
    {
      float num1 = pt.X - m_position.X;
      float num2 = pt.Y - m_position.Y;
      return (int) (num1 * (double) num1 + num2 * (double) num2);
    }

    public float GetDistance(WorldObject obj)
    {
      float num1 = obj.Position.X - Position.X;
      float num2 = obj.Position.Y - Position.Y;
      return (float) Math.Sqrt(num1 * (double) num1 + num2 * (double) num2);
    }

    public float GetDistanceSq(WorldObject obj)
    {
      float num1 = obj.Position.X - Position.X;
      float num2 = obj.Position.Y - Position.Y;
      return (float) (num1 * (double) num1 + num2 * (double) num2);
    }

    public float GetDistanceSq(IHasPosition pos)
    {
      float num1 = pos.Position.X - Position.X;
      float num2 = pos.Position.Y - Position.Y;
      return (float) (num1 * (double) num1 + num2 * (double) num2);
    }

    public float GetDistanceXY(ref Vector3 pt)
    {
      float num1 = pt.X - m_position.X;
      float num2 = pt.Y - m_position.Y;
      return (float) Math.Sqrt(num1 * (double) num1 + num2 * (double) num2);
    }

    public float GetDistanceXY(WorldObject obj)
    {
      float num1 = obj.Position.X - Position.X;
      float num2 = obj.Position.Y - Position.Y;
      return (float) Math.Sqrt(num1 * (double) num1 + num2 * (double) num2);
    }

    public bool IsTeleporting { get; internal set; }

    /// <summary>
    /// Adds the given object ontop of this one.
    /// The object will not be added immediately after the method-call.
    /// </summary>
    /// <remarks>This object must be in the world for this method call.</remarks>
    public void PlaceOnTop(WorldObject obj)
    {
      Vector3 position = m_position;
      position.Z += 2f;
      m_Map.TransferObjectLater(obj, position);
    }

    public void PlaceInFront(WorldObject obj)
    {
      Vector3 position = m_position;
      ++position.Z;
      m_Map.TransferObjectLater(obj, position);
      obj.Orientation = obj.GetAngleTowards(this);
    }

    /// <summary>
    /// Gets the angle between this object and the given position, in relation to the north-south axis
    /// </summary>
    public float GetAngleTowards(Vector3 v)
    {
      float num = (float) Math.Atan2(v.Y - (double) m_position.Y,
        v.X - (double) m_position.X);
      if(num < 0.0)
        num += 6.283185f;
      return num;
    }

    /// <summary>
    /// Gets the angle between this object and the given position, in relation to the north-south axis
    /// </summary>
    public float GetAngleTowards(ref Vector3 v)
    {
      float num = (float) Math.Atan2(v.Y - (double) m_position.Y,
        v.X - (double) m_position.X);
      if(num < 0.0)
        num += 6.283185f;
      return num;
    }

    /// <summary>
    /// Gets the angle between this object and the given position, in relation to the north-south axis
    /// </summary>
    public float GetAngleTowards(IHasPosition obj)
    {
      float num = (float) Math.Atan2(obj.Position.Y - (double) m_position.Y,
        obj.Position.X - (double) m_position.X);
      if(num < 0.0)
        num += 6.283185f;
      return num;
    }

    /// <summary>Returns whether this Object is behind the given obj</summary>
    public bool IsBehind(WorldObject obj)
    {
      if(obj == this)
        return false;
      float num = Math.Abs(obj.m_orientation - obj.GetAngleTowards(m_position));
      if(num >= 2.09439516067505)
        return num <= 4.1887903213501;
      return false;
    }

    /// <summary>
    /// Returns whether this Object is in front of the given obj
    /// </summary>
    public bool IsInFrontOf(WorldObject obj)
    {
      if(obj == this)
        return false;
      float num = Math.Abs(obj.m_orientation - obj.GetAngleTowards(m_position));
      if(num > 1.04719758033752)
        return num >= 5.23598766326904;
      return true;
    }

    /// <summary>
    /// Returns whether the given pos is in front of this Object
    /// </summary>
    public bool IsInFrontOfThis(Vector3 pos)
    {
      float num = Math.Abs(m_orientation - GetAngleTowards(pos));
      if(num > 1.04719758033752)
        return num >= 5.23598766326904;
      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void GetPointXY(float angle, float dist, out Vector3 pos)
    {
      pos = m_position;
      m_position.GetPointYX(angle + m_orientation, dist, out pos);
    }

    /// <summary>
    /// 
    /// </summary>
    public void GetPointInFront(float dist, out Vector3 pos)
    {
      GetPointXY(0.0f, dist, out pos);
    }

    /// <summary>
    /// 
    /// </summary>
    public void GetPointBehind(float dist, out Vector3 pos)
    {
      GetPointXY(3.141593f, dist, out pos);
    }

    public virtual ClientLocale Locale
    {
      get { return RealmServerConfiguration.DefaultLocale; }
      set { log.Warn("Tried to illegaly set WorldObject.Locale for: {0}", this); }
    }

    public void Say(string message)
    {
      throw new NotImplementedException();
    }

    public virtual void Say(float radius, string message)
    {
      ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterSay, ChatLanguage.Universal, message, radius);
    }

    public void Say(RealmLangKey key, params object[] args)
    {
      Say(RealmLocalizer.Instance.Translate(Locale, key, args));
    }

    public void Say(string message, params object[] args)
    {
      Say(string.Format(message, args));
    }

    public void Say(string[] localizedMsgs)
    {
      Say(ChatMgr.ListeningRadius, localizedMsgs);
    }

    public virtual void Say(float radius, string[] localizedMsgs)
    {
      ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterSay, ChatLanguage.Universal, localizedMsgs, radius);
    }

    public virtual void Yell(float radius, string message)
    {
      ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterYell, ChatLanguage.Universal, message, radius);
    }

    public void Yell(string message)
    {
      Yell(ChatMgr.YellRadius, message);
    }

    public void Yell(RealmLangKey key, params object[] args)
    {
      Yell(RealmLocalizer.Instance.Translate(Locale, key, args));
    }

    public void Yell(string message, params object[] args)
    {
      Yell(string.Format(message, args));
    }

    public void Yell(string[] localizedMsgs)
    {
      Yell(ChatMgr.YellRadius, localizedMsgs);
    }

    public virtual void Yell(float radius, string[] localizedMsgs)
    {
      ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterYell, ChatLanguage.Universal, localizedMsgs, radius);
    }

    public virtual void Emote(float radius, string message)
    {
      ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterEmote, ChatLanguage.Universal, message, radius);
    }

    public void Emote(string message)
    {
      Emote(ChatMgr.ListeningRadius, message);
    }

    public void Emote(RealmLangKey key, params object[] args)
    {
      Emote(RealmLocalizer.Instance.Translate(Locale, key, args));
    }

    public void Emote(string message, params object[] args)
    {
      Emote(string.Format(message, args));
    }

    public void Emote(string[] localizedMsgs)
    {
      Emote(ChatMgr.ListeningRadius, localizedMsgs);
    }

    public virtual void Emote(float radius, string[] localizedMsgs)
    {
      ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterEmote, ChatLanguage.Universal, localizedMsgs, radius);
    }

    /// <summary>Sends a packet to all nearby characters.</summary>
    /// <param name="packet">the packet to send</param>
    /// <param name="includeSelf">whether or not to send the packet to ourselves (if we're a character)</param>
    /// <param name="addEnd"> </param>
    /// <param name="isRus"> </param>
    public virtual void SendPacketToArea(RealmPacketOut packet, bool includeSelf = true, bool addEnd = true,
      Locale locale = WCell.Core.Network.Locale.Any, float? radius = null)
    {
      if(Map == null || Thread.CurrentThread.ManagedThreadId != m_Map.CurrentThreadId)
        return;
      float? nullable = radius;
      float num = nullable.HasValue ? nullable.GetValueOrDefault() : BroadcastRange;
      if(!IsAreaActive)
        return;
      if(locale == WCell.Core.Network.Locale.Any)
      {
        for(int index = m_Map.Characters.Count - 1; index >= 0; --index)
        {
          Character character = m_Map.Characters[index];
          if((includeSelf || !Equals(character)) &&
             num > (double) GetDistance(character))
            character.Send(packet, addEnd);
        }
      }
      else
      {
        for(int index = m_Map.CharactersByLocale[locale].Count - 1; index >= 0; --index)
        {
          Character character = m_Map.CharactersByLocale[locale][index];
          if((includeSelf || !Equals(character)) &&
             num > (double) GetDistance(character))
            character.Send(packet, addEnd);
        }
      }
    }

    /// <summary>
    /// Sends a manual update field refresh to all nearby characters.
    /// </summary>
    /// <param name="field">the field to refresh</param>
    public void SendFieldUpdateTo(IPacketReceiver rcv, UpdateFieldId field)
    {
      if(!IsAreaActive)
        return;
      uint uint32 = GetUInt32(field.RawId);
      using(UpdatePacket fieldUpdatePacket = GetFieldUpdatePacket(field, uint32))
        rcv.Send(fieldUpdatePacket, false);
    }

    /// <summary>
    /// Ensures that the given action is always executed in map context of this Character - which
    /// might be right now or after the Character is added to a map or during the next Map update.
    /// </summary>
    /// <returns>Whether the Action has been executed immediately (or enqueued)</returns>
    public bool ExecuteInContext(Action action)
    {
      if(!IsInContext)
      {
        AddMessage(() => action());
        return false;
      }

      action();
      return true;
    }

    public void AddMessage(IMessage msg)
    {
      m_messageQueue.Enqueue(msg);
    }

    public void AddMessage(Action action)
    {
      m_messageQueue.Enqueue(new Message(action));
    }

    public abstract Faction Faction { get; set; }

    public abstract FactionId FactionId { get; set; }

    /// <summary>
    /// Checks whether this Unit can currently do any harm (must be alive and not in a sanctuary)
    /// </summary>
    public virtual bool CanDoHarm
    {
      get { return !(this is Unit) || ((Unit) this).IsAlive; }
    }

    /// <summary>
    /// Checks whether this Object can currently be harmed (must be alive and not in sanctuary)
    /// </summary>
    public bool CanBeHarmed
    {
      get { return true; }
    }

    /// <summary>whether this Unit is in a no-combat zone</summary>
    public bool IsInSanctuary
    {
      get { return false; }
    }

    /// <summary>
    /// Indicates whether the 2 units are friendly towards each other.
    /// </summary>
    /// <returns></returns>
    public virtual bool IsFriendlyWith(IFactionMember opponent)
    {
      if(HasMaster)
      {
        if(ReferenceEquals(Master, opponent))
          return true;
        return Master.IsFriendlyWith(opponent);
      }

      if(opponent is WorldObject && ((WorldObject) opponent).HasMaster)
        opponent = ((WorldObject) opponent).Master;
      if(ReferenceEquals(opponent, this))
        return true;
      if(opponent is Character)
        return ((WorldObject) opponent).IsFriendlyWith(this);
      Faction faction1 = Faction;
      Faction faction2 = opponent.Faction;
      if(faction1 == opponent.Faction)
        return true;
      if(faction1 != null && faction2 != null)
        return faction1.IsFriendlyTowards(faction2);
      return false;
    }

    /// <summary>
    /// Indicates whether the 2 units are neutral towards each other.
    /// </summary>
    /// <returns></returns>
    public virtual bool IsAtLeastNeutralWith(IFactionMember opponent)
    {
      if(IsFriendlyWith(opponent))
        return true;
      Faction faction1 = Faction;
      Faction faction2 = opponent.Faction;
      return faction1 != null && faction2 != null && faction1.Neutrals.Contains(faction2);
    }

    /// <summary>
    /// Indicates whether the 2 units are hostile towards each other.
    /// </summary>
    /// <returns></returns>
    public virtual bool IsHostileWith(IFactionMember opponent)
    {
      if(HasMaster)
      {
        if(ReferenceEquals(Master, opponent))
          return false;
        return Master.IsHostileWith(opponent);
      }

      if(opponent is WorldObject && ((WorldObject) opponent).HasMaster)
        opponent = ((WorldObject) opponent).Master;
      if(ReferenceEquals(opponent, this))
        return false;
      if(opponent is Character)
        return ((WorldObject) opponent).IsHostileWith(this);
      Faction faction1 = Faction;
      Faction faction2 = opponent.Faction;
      return faction1 != faction2 && faction1 != null &&
             (faction2 != null && faction1.Enemies.Contains(faction2));
    }

    public virtual bool MayAttack(IFactionMember opponent)
    {
      if(HasMaster)
      {
        if(ReferenceEquals(Master, opponent))
          return false;
        return Master.MayAttack(opponent);
      }

      if(opponent is WorldObject && ((WorldObject) opponent).HasMaster)
        opponent = ((WorldObject) opponent).Master;
      if(opponent == null || ReferenceEquals(opponent, this))
        return false;
      Unit unit = opponent as Unit;
      if(unit != null && !unit.IsVisible)
        return false;
      Character character = opponent as Character;
      if(character != null && this is NPC && !character.Client.IsConnected)
        return false;
      if(character != null)
        return character.MayAttack(this);
      Faction faction1 = Faction;
      Faction faction2 = opponent.Faction;
      return faction1 != faction2 && faction1 != null &&
             (faction2 != null && faction1.Enemies.Contains(faction2));
    }

    public virtual bool IsAlliedWith(IFactionMember opponent)
    {
      if(HasMaster)
      {
        if(ReferenceEquals(Master, opponent))
          return true;
        return Master.IsAlliedWith(opponent);
      }

      if(opponent is WorldObject && ((WorldObject) opponent).HasMaster)
        opponent = ((WorldObject) opponent).Master;
      if(ReferenceEquals(opponent, this))
        return true;
      if(opponent is Character)
        return ((WorldObject) opponent).IsAlliedWith(this);
      Faction faction1 = Faction;
      Faction faction2 = opponent.Faction;
      if(faction1 == opponent.Faction)
        return true;
      if(faction1 != null && opponent.Faction != null)
        return faction1.Friends.Contains(faction2);
      return false;
    }

    public virtual bool IsInSameDivision(IFactionMember opponent)
    {
      if(opponent is Character)
        return ((WorldObject) opponent).IsInSameDivision(this);
      return IsAlliedWith(opponent);
    }

    /// <summary>
    /// Indicates whether we can currently do any harm and are allowed to attack
    /// the given opponent (hostile or neutral factions, duel partners etc)
    /// </summary>
    public virtual bool CanHarm(WorldObject opponent)
    {
      if(CanDoHarm)
        return MayAttack(opponent);
      return false;
    }

    /// <summary>
    /// The set of currently active AreaAuras or null.
    /// Do not modify the list.
    /// </summary>
    public List<AreaAura> AreaAuras
    {
      get { return m_areaAuras; }
    }

    public bool HasAreaAuras
    {
      get
      {
        if(m_areaAuras != null)
          return m_areaAuras.Count > 0;
        return false;
      }
    }

    /// <summary>Called when AreaAura is created</summary>
    internal void AddAreaAura(AreaAura aura)
    {
      if(m_areaAuras == null)
        m_areaAuras = new List<AreaAura>(2);
      else if(aura.Spell.AttributesExB.HasFlag(SpellAttributesExB.ExclusiveAreaAura))
      {
        foreach(AreaAura areaAura in m_areaAuras)
        {
          if(aura.Spell.AttributesExB.HasFlag(SpellAttributesExB.ExclusiveAreaAura))
          {
            areaAura.Remove(true);
            break;
          }
        }
      }

      m_areaAuras.Add(aura);
    }

    /// <summary>Returns the first AreaAura of the given spell</summary>
    public AreaAura GetAreaAura(Spell spell)
    {
      if(m_areaAuras == null)
        return null;
      return m_areaAuras.FirstOrDefault(aura => aura.Spell == spell);
    }

    public bool CancelAreaAura(Spell spell)
    {
      AreaAura areaAura = GetAreaAura(spell);
      if(areaAura != null)
        return CancelAreaAura(areaAura);
      return false;
    }

    /// <summary>Called by AreaAura.Remove</summary>
    internal bool CancelAreaAura(AreaAura aura)
    {
      if(m_areaAuras == null || !m_areaAuras.Remove(aura))
        return false;
      if(this is Unit)
        ((Unit) this).Auras.Remove(aura.Spell);
      else if(m_areaAuras.Count == 0 && (IsTrap || this is DynamicObject))
        Delete();
      return true;
    }

    /// <summary>
    /// Indicates whether this Object can see the other object
    /// </summary>
    public virtual bool CanSee(WorldObject obj)
    {
      return IsInPhase(obj);
    }

    /// <summary>
    /// Visibility of this object in the eyes of the given observer.
    /// Can be used to override default visibility checks
    /// </summary>
    public virtual VisibilityStatus DetermineVisibilityFor(Unit observer)
    {
      return VisibilityStatus.Default;
    }

    public virtual bool IsPlayer
    {
      get { return false; }
    }

    /// <summary>Whether this or it's master is a player</summary>
    public bool IsPlayerOwned
    {
      get
      {
        if(IsPlayer)
          return true;
        if(m_master != null)
          return m_master.IsPlayer;
        return false;
      }
    }

    /// <summary>Whether this object's master is a player</summary>
    public bool HasPlayerMaster
    {
      get
      {
        if(m_master != null)
          return m_master.IsPlayer;
        return false;
      }
    }

    public Character PlayerOwner
    {
      get
      {
        if(!(this is Character))
          return m_master as Character;
        return (Character) this;
      }
    }

    /// <summary>
    /// Whether this is actively controlled by a player.
    /// Not to be confused with IsOwnedByPlayer.
    /// </summary>
    public virtual bool IsPlayerControlled
    {
      get { return false; }
    }

    /// <summary>Grow a bit and then become small again</summary>
    public void Highlight()
    {
      float diff = (HighlightScale - 1f) * ScaleX;
      ScaleX = HighlightScale * ScaleX;
      CallDelayed(HighlightDelayMillis, obj => obj.ScaleX -= diff);
    }

    public void PlaySound(uint sound)
    {
      MiscHandler.SendPlayObjectSound(this, sound);
    }

    /// <summary>
    /// TODO: Find a better way to identify texts (i.e. by entry/object-type ids and sequence number)
    /// </summary>
    public void PlayTextAndSoundByEnglishPrefix(string englishPrefix)
    {
      NPCAiText textByEnglishPrefix = NPCAiTextMgr.GetFirstTextByEnglishPrefix(englishPrefix, true);
      if(textByEnglishPrefix == null)
        return;
      PlayTextAndSound(textByEnglishPrefix);
    }

    /// <summary>Play a text and sound identify by the id</summary>
    /// <param name="id">Id of the text in creature_ai_texts</param>
    public void PlayTextAndSoundById(int id)
    {
      NPCAiText firstTextById = NPCAiTextMgr.GetFirstTextById(id);
      if(firstTextById == null)
        return;
      PlayTextAndSound(firstTextById);
    }

    public void PlayTextAndSound(NPCAiText text)
    {
      PlaySound((uint) text.Sound);
      Yell(text.Texts);
    }

    /// <summary>Deleted objects must never be used again!</summary>
    public bool IsDeleted
    {
      get { return m_Deleted; }
    }

    [NotVariable]
    public int UniqWorldEntityId { get; set; }

    public bool IsAsda2Teleporting { get; set; }

    /// <summary>
    /// Enqueues a message to remove this WorldObject from it's Map
    /// </summary>
    public void RemoveFromMap()
    {
      if(!IsInWorld)
        return;
      m_Map.RemoveObjectLater(this);
    }

    /// <summary>
    /// Enqueues a message to remove this Object from the World and dispose it.
    /// </summary>
    public virtual void Delete()
    {
      if(m_Deleted)
        return;
      m_Deleted = true;
      if(m_Map != null)
        m_Map.AddMessage(DeleteNow);
      else
        DeleteNow();
    }

    /// <summary>Removes this Object from the World and disposes it.</summary>
    /// <see cref="M:WCell.RealmServer.Entities.WorldObject.Delete" />
    /// <remarks>Requires map context</remarks>
    protected internal virtual void DeleteNow()
    {
      try
      {
        m_Deleted = true;
        OnDeleted();
        Dispose();
      }
      catch(Exception ex)
      {
        throw new Exception(string.Format("Failed to correctly delete object \"{0}\"", this), ex);
      }
    }

    protected void OnDeleted()
    {
      m_updateActions = null;
      if(m_Map == null)
        return;
      if(m_areaAuras != null)
      {
        foreach(AreaAura areaAura in m_areaAuras.ToArray())
          areaAura.Remove(true);
      }

      m_Map.RemoveObjectNow(this);
    }

    public override void Dispose(bool disposing)
    {
      SpellCast = null;
      m_Map = null;
    }

    public override string ToString()
    {
      return Name + " (" + EntityId + ")";
    }

    /// <summary>ONLY ENGLISH!</summary>
    /// <param name="msg"></param>
    /// <param name="c"></param>
    public void SendMessageToArea(string msg, Color c)
    {
      using(RealmPacketOut globalChatMessage =
        ChatMgr.CreateGlobalChatMessage(Name, msg, c, WCell.Core.Network.Locale.Start))
        SendPacketToArea(globalChatMessage, true, false, WCell.Core.Network.Locale.Any, new float?());
    }
  }
}