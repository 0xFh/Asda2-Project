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
            ObjectPoolMgr.CreatePool<HashSet<WorldObject>>(
                (Func<HashSet<WorldObject>>) (() => new HashSet<WorldObject>()));

        public static readonly ObjectPool<List<WorldObject>> WorldObjectListPool =
            ObjectPoolMgr.CreatePool<List<WorldObject>>((Func<List<WorldObject>>) (() => new List<WorldObject>()));

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
        protected UpdatePriority m_UpdatePriority = WorldObject.DefaultObjectUpdatePriority;
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
            get { return this.m_messageQueue; }
        }

        public DateTime LastUpdateTime
        {
            get { return this.m_lastUpdateTime; }
            internal set { this.m_lastUpdateTime = value; }
        }

        public override UpdatePriority UpdatePriority
        {
            get { return this.m_UpdatePriority; }
        }

        public void SetUpdatePriority(UpdatePriority priority)
        {
            this.m_UpdatePriority = priority;
        }

        public OneShotObjectUpdateTimer CallDelayed(int millis, Action<WorldObject> callback)
        {
            OneShotObjectUpdateTimer objectUpdateTimer = new OneShotObjectUpdateTimer(millis, callback);
            this.AddUpdateAction((ObjectUpdateTimer) objectUpdateTimer);
            return objectUpdateTimer;
        }

        /// <summary>
        /// Adds a new Action to the list of Actions to be executed every millis.
        /// </summary>
        /// <param name="callback"></param>
        public ObjectUpdateTimer CallPeriodically(int millis, Action<WorldObject> callback)
        {
            ObjectUpdateTimer timer = new ObjectUpdateTimer(millis, callback);
            this.AddUpdateAction(timer);
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
            this.AddUpdateAction(action);
            this.CallDelayed(callUntilMillis, (Action<WorldObject>) (obj => this.RemoveUpdateAction(action)));
            return action;
        }

        /// <summary>
        /// Adds a new Action to the list of Actions to be executed every action.Delay milliseconds
        /// </summary>
        public void AddUpdateAction(ObjectUpdateTimer timer)
        {
            if (this.m_updateActions == null)
                this.m_updateActions = new List<ObjectUpdateTimer>(3);
            timer.LastCallTime = this.m_lastUpdateTime;
            this.m_updateActions.Add(timer);
        }

        public bool HasUpdateAction(Func<ObjectUpdateTimer, bool> predicate)
        {
            this.EnsureContext();
            if (this.m_updateActions != null)
                return this.m_updateActions.Any<ObjectUpdateTimer>(predicate);
            return false;
        }

        public void RemoveUpdateAction(Action<WorldObject> callback)
        {
            if (this.m_updateActions == null)
                return;
            this.ExecuteInContext((Action) (() =>
            {
                ObjectUpdateTimer timer =
                    this.m_updateActions.FirstOrDefault<ObjectUpdateTimer>(
                        (Func<ObjectUpdateTimer, bool>) (act => act.Callback == callback));
                if (timer == null)
                    return;
                this.RemoveUpdateAction(timer);
            }));
        }

        /// <summary>Removes the given Action</summary>
        /// <param name="timer"></param>
        public bool RemoveUpdateAction(ObjectUpdateTimer timer)
        {
            return this.m_updateActions != null && this.m_updateActions.Remove(timer);
        }

        /// <summary>
        /// Make sure to call this before updating anything else (required for reseting UpdateInfo)
        /// </summary>
        public virtual void Update(int dt)
        {
            IMessage message;
            while (this.m_messageQueue.TryDequeue(out message))
            {
                try
                {
                    message.Execute();
                }
                catch (Exception ex)
                {
                    LogUtil.ErrorException(ex, "Exception raised when processing Message for: {0}", new object[1]
                    {
                        (object) this
                    });
                    this.Delete();
                }
            }

            if (this.m_areaAuras != null)
            {
                int count = this.m_areaAuras.Count;
                for (int index = 0; index < count; ++index)
                {
                    this.m_areaAuras[index].Update(dt);
                    if (this.m_areaAuras.Count != count)
                        break;
                }
            }

            if (this.m_spellCast != null)
                this.m_spellCast.Update(dt);
            if (this.m_updateActions == null)
                return;
            for (int index = this.m_updateActions.Count - 1; index >= 0; --index)
            {
                ObjectUpdateTimer updateAction = this.m_updateActions[index];
                if (updateAction.Delay == 0)
                    updateAction.Execute(this);
                else if ((this.m_lastUpdateTime - updateAction.LastCallTime).ToMilliSecondsInt() >= updateAction.Delay)
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
            this.m_requiresUpdate = true;
        }

        /// <summary>
        /// The current <see cref="T:WCell.Util.Threading.IContextHandler" /> of this Character.
        /// </summary>
        public IContextHandler ContextHandler
        {
            get { return (IContextHandler) this.m_Map; }
        }

        /// <summary>
        /// Whether this object is in the world and within the current
        /// execution context.
        /// </summary>
        public bool IsInContext
        {
            get
            {
                if (this.IsInWorld)
                {
                    IContextHandler contextHandler = this.ContextHandler;
                    if (contextHandler != null && contextHandler.IsInContext)
                        return true;
                }

                return false;
            }
        }

        public void EnsureContext()
        {
            if (!this.IsInWorld)
                return;
            IContextHandler contextHandler = this.ContextHandler;
            if (contextHandler == null)
                return;
            contextHandler.EnsureContext();
        }

        public ushort UniqIdOnMap { get; set; }

        protected bool HasNode
        {
            get { return this.Node != null; }
        }

        protected WorldObject()
        {
            this.CreationTime = Utility.GetSystemTime();
            this.LastUpdateTime = DateTime.Now;
        }

        public virtual ObjectTemplate Template
        {
            get { return (ObjectTemplate) null; }
        }

        /// <summary>Time in seconds since creation</summary>
        public uint Age
        {
            get { return Utility.GetSystemTime() - this.CreationTime; }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual uint Phase
        {
            get { return this.m_Phase; }
            set { this.m_Phase = value; }
        }

        /// <summary>The current position of the object</summary>
        public virtual Vector3 Position
        {
            get { return this.m_position; }
            internal set { this.m_position = value; }
        }

        public float Asda2X
        {
            get
            {
                if (this.Map != null)
                    return this.Position.X - this.Map.Offset;
                return 0.0f;
            }
            set { this.Position = new Vector3(value + this.Map.Offset, this.Position.Y); }
        }

        public float Asda2Y
        {
            get
            {
                if (this.Map != null)
                    return this.Position.Y - this.Map.Offset;
                return 0.0f;
            }
            set { this.Position = new Vector3(this.Position.Y, value + this.Map.Offset); }
        }

        public Vector3 Asda2Position
        {
            get { return new Vector3(this.Asda2X, this.Asda2Y); }
        }

        /// <summary>The current zone of the object</summary>
        public virtual Zone Zone
        {
            get { return this.m_zone; }
            internal set { this.m_zone = value; }
        }

        public virtual void SetZone(Zone zone)
        {
            this.Zone = zone;
        }

        public ZoneTemplate ZoneTemplate
        {
            get
            {
                if (this.m_zone == null)
                    return (ZoneTemplate) null;
                return this.m_zone.Template;
            }
        }

        public ZoneId ZoneId
        {
            get
            {
                if (this.m_zone == null)
                    return ZoneId.None;
                return this.m_zone.Id;
            }
        }

        /// <summary>
        /// The current map of the object.
        /// Map must (and will) never be null.
        /// </summary>
        public virtual Map Map
        {
            get { return this.m_Map; }
            internal set { this.m_Map = value; }
        }

        public MapId MapId
        {
            get
            {
                if (this.m_Map == null)
                    return MapId.End;
                return this.m_Map.Id;
            }
        }

        /// <summary>The heading of the object (direction it is facing)</summary>
        public float Orientation
        {
            get { return this.m_orientation; }
            set { this.m_orientation = value; }
        }

        public override bool IsInWorld
        {
            get { return this.m_Map != null; }
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
                if (this.m_CasterReference == null)
                    this.m_CasterReference = this.CreateCasterInfo();
                this.m_CasterReference.Level = this.CasterLevel;
                return this.m_CasterReference;
            }
        }

        /// <summary>Whether there are active Characters in the Area</summary>
        public bool IsAreaActive
        {
            get { return this.m_areaCharCount > 0; }
        }

        /// <summary>The amount of Characters nearby.</summary>
        public int AreaCharCount
        {
            get { return this.m_areaCharCount; }
            internal set { this.m_areaCharCount = value; }
        }

        protected internal virtual void OnEncounteredBy(Character chr)
        {
            ++this.AreaCharCount;
        }

        public virtual bool IsTrap
        {
            get { return false; }
        }

        public bool IsCorpse
        {
            get
            {
                if (!(this is Corpse))
                    return this.IsNPCCorpse;
                return true;
            }
        }

        public bool IsNPCCorpse
        {
            get
            {
                if (this is NPC)
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
                if (this.m_spellCast == null)
                    return false;
                if (!this.m_spellCast.IsCasting)
                    return this.m_spellCast.IsChanneling;
                return true;
            }
        }

        public void SetSpellCast(SpellCast cast)
        {
            if (this.m_spellCast != null && this.m_spellCast != cast)
                this.m_spellCast.Dispose();
            this.m_spellCast = cast;
        }

        /// <summary>
        /// Set to the SpellCast-object of this Object.
        /// If the Object is not in the world, will return null
        /// </summary>
        public SpellCast SpellCast
        {
            get
            {
                if (this.m_spellCast == null)
                {
                    this.m_spellCast = SpellCast.ObtainPooledCast(this);
                    this.InitSpellCast();
                }

                return this.m_spellCast;
            }
            internal set
            {
                if (value == this.m_spellCast)
                    return;
                this.m_spellCast = value;
            }
        }

        public float GetSpellMaxRange(Spell spell)
        {
            return this.GetSpellMaxRange(spell, spell.Range.MaxDist);
        }

        public float GetSpellMaxRange(Spell spell, float range)
        {
            return this.GetSpellMaxRange(spell, (WorldObject) null, range);
        }

        public float GetSpellMaxRange(Spell spell, WorldObject target)
        {
            return this.GetSpellMaxRange(spell, target, spell.Range.MaxDist);
        }

        public float GetSpellMaxRange(Spell spell, WorldObject target, float range)
        {
            if (target is Unit)
                range += ((Unit) target).CombatReach;
            if (this is Unit)
            {
                range += ((Unit) this).CombatReach + (float) ((Unit) this).IntMods[32];
                range = ((Unit) this).Auras.GetModifiedFloat(SpellModifierType.Range, spell, range);
            }

            return range;
        }

        public float GetSpellMinRange(float range, WorldObject target)
        {
            if (target is Unit)
                range += ((Unit) target).CombatReach;
            if (this is Unit)
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
            if (ticks != 0)
                return ((long) this.Map.TickCount + (long) this.EntityId.Low) % (long) ticks == 0L;
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
            return ((int) this.Phase & (int) phase) != 0;
        }

        public bool IsInPhase(WorldObject obj)
        {
            return ((int) this.Phase & (int) obj.Phase) != 0;
        }

        public void SetOrientationTowards(ref Vector3 pos)
        {
            this.m_orientation = this.GetAngleTowards(ref pos);
        }

        public void SetOrientationTowards(IHasPosition pos)
        {
            this.m_orientation = this.GetAngleTowards(pos);
        }

        public virtual bool SetPosition(Vector3 pt)
        {
            return this.m_Map.MoveObject(this, ref pt);
        }

        public virtual bool SetPosition(Vector3 pt, float orientation)
        {
            if (!this.m_Map.MoveObject(this, ref pt))
                return false;
            this.m_orientation = orientation;
            return true;
        }

        /// <summary>
        /// The Master of this Object (Units are their own Masters if not controlled, Objects might have masters that they belong to)
        /// </summary>
        public Unit Master
        {
            get
            {
                if (this.m_master != null && !this.m_master.IsInWorld)
                    this.m_master = (Unit) null;
                return this.m_master;
            }
            protected internal set
            {
                if (value == this.m_master)
                    return;
                if (value != null)
                {
                    this.Faction = value.Faction;
                    if (value is Character && this is Unit)
                    {
                        ((Unit) this).UnitFlags |= UnitFlags.PlayerControlled;
                        if (this is NPC)
                            ((NPC) this).m_spawnPoint = (NPCSpawnPoint) null;
                    }
                }
                else if (this is Unit)
                {
                    this.Faction = ((Unit) this).DefaultFaction;
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

                this.m_master = value;
            }
        }

        public bool HasMaster
        {
            get
            {
                if (this.Master != null)
                    return this.m_master != this;
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
                if (this is Character)
                    return (Character) this;
                return this.m_master as Character;
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
                if (this is Unit)
                    return (Unit) this;
                return this.m_master;
            }
        }

        /// <summary>The Terrain underneath this object's current location</summary>
        public float TerrainHeight
        {
            get { return this.Map.Terrain.GetGroundHeightUnderneath(this.m_position + this.ScaleX); }
        }

        /// <summary>
        /// Indicates whether the given radius is within the given distance to this object
        /// </summary>
        public bool IsInRadius(ref Vector3 pt, float distance)
        {
            return (double) this.GetDistanceSq(ref pt) <= (double) distance * (double) distance;
        }

        /// <summary>
        /// Indicates whether the given radius is within the given distance to this object
        /// </summary>
        public bool IsInRadius(Vector3 pt, float distance)
        {
            return (double) this.GetDistanceSq(ref pt) <= (double) distance * (double) distance;
        }

        public bool IsInRadius(WorldObject obj, float distance)
        {
            return (double) this.GetDistanceSq(obj) <= (double) distance * (double) distance;
        }

        public bool IsInRadius(ref Vector3 pt, SimpleRange range)
        {
            float distanceSq = this.GetDistanceSq(ref pt);
            if ((double) distanceSq > (double) range.MaxDist * (double) range.MaxDist)
                return false;
            if ((double) range.MinDist >= 1.0)
                return (double) distanceSq >= (double) range.MinDist * (double) range.MinDist;
            return true;
        }

        public bool IsInRadius(WorldObject obj, SimpleRange range)
        {
            float distanceSq = this.GetDistanceSq(obj);
            if ((double) distanceSq > (double) range.MaxDist * (double) range.MaxDist)
                return false;
            if ((double) range.MinDist >= 1.0)
                return (double) distanceSq >= (double) range.MinDist * (double) range.MinDist;
            return true;
        }

        /// <summary>Indicates whether the given obj is in update range</summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool IsInUpdateRange(WorldObject obj)
        {
            return (double) this.GetDistanceSq(obj) <=
                   (double) WorldObject.BroadcastRange * (double) WorldObject.BroadcastRange;
        }

        /// <summary>
        /// Indicates whether the given radius is within the given distance to this object
        /// </summary>
        public bool IsInRadiusSq(ref Vector3 pt, float sqDistance)
        {
            return (double) this.GetDistanceSq(ref pt) <= (double) sqDistance;
        }

        public bool IsInRadiusSq(Vector3 pos, float sqDistance)
        {
            return (double) this.GetDistanceSq(ref pos) <= (double) sqDistance;
        }

        public bool IsInRadiusSq(IHasPosition pos, float sqDistance)
        {
            return (double) this.GetDistanceSq(pos) <= (double) sqDistance;
        }

        public float GetDistance(ref Vector3 pt)
        {
            float num1 = pt.X - this.m_position.X;
            float num2 = pt.Y - this.m_position.Y;
            return (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public float GetDistanceSq(ref Vector3 pt)
        {
            float num1 = pt.X - this.m_position.X;
            float num2 = pt.Y - this.m_position.Y;
            return (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public float GetDistance(Vector3 pt)
        {
            float num1 = pt.X - this.m_position.X;
            float num2 = pt.Y - this.m_position.Y;
            return (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public float GetDistanceSq(Vector3 pt)
        {
            float num1 = pt.X - this.m_position.X;
            float num2 = pt.Y - this.m_position.Y;
            return (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        /// <summary>
        /// Less precise method to get the square distance to a point
        /// </summary>
        public int GetDistanceSqInt(ref Vector3 pt)
        {
            float num1 = pt.X - this.m_position.X;
            float num2 = pt.Y - this.m_position.Y;
            return (int) ((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public float GetDistance(WorldObject obj)
        {
            float num1 = obj.Position.X - this.Position.X;
            float num2 = obj.Position.Y - this.Position.Y;
            return (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public float GetDistanceSq(WorldObject obj)
        {
            float num1 = obj.Position.X - this.Position.X;
            float num2 = obj.Position.Y - this.Position.Y;
            return (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public float GetDistanceSq(IHasPosition pos)
        {
            float num1 = pos.Position.X - this.Position.X;
            float num2 = pos.Position.Y - this.Position.Y;
            return (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public float GetDistanceXY(ref Vector3 pt)
        {
            float num1 = pt.X - this.m_position.X;
            float num2 = pt.Y - this.m_position.Y;
            return (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public float GetDistanceXY(WorldObject obj)
        {
            float num1 = obj.Position.X - this.Position.X;
            float num2 = obj.Position.Y - this.Position.Y;
            return (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public bool IsTeleporting { get; internal set; }

        /// <summary>
        /// Adds the given object ontop of this one.
        /// The object will not be added immediately after the method-call.
        /// </summary>
        /// <remarks>This object must be in the world for this method call.</remarks>
        public void PlaceOnTop(WorldObject obj)
        {
            Vector3 position = this.m_position;
            position.Z += 2f;
            this.m_Map.TransferObjectLater(obj, position);
        }

        public void PlaceInFront(WorldObject obj)
        {
            Vector3 position = this.m_position;
            ++position.Z;
            this.m_Map.TransferObjectLater(obj, position);
            obj.Orientation = obj.GetAngleTowards((IHasPosition) this);
        }

        /// <summary>
        /// Gets the angle between this object and the given position, in relation to the north-south axis
        /// </summary>
        public float GetAngleTowards(Vector3 v)
        {
            float num = (float) Math.Atan2((double) v.Y - (double) this.m_position.Y,
                (double) v.X - (double) this.m_position.X);
            if ((double) num < 0.0)
                num += 6.283185f;
            return num;
        }

        /// <summary>
        /// Gets the angle between this object and the given position, in relation to the north-south axis
        /// </summary>
        public float GetAngleTowards(ref Vector3 v)
        {
            float num = (float) Math.Atan2((double) v.Y - (double) this.m_position.Y,
                (double) v.X - (double) this.m_position.X);
            if ((double) num < 0.0)
                num += 6.283185f;
            return num;
        }

        /// <summary>
        /// Gets the angle between this object and the given position, in relation to the north-south axis
        /// </summary>
        public float GetAngleTowards(IHasPosition obj)
        {
            float num = (float) Math.Atan2((double) obj.Position.Y - (double) this.m_position.Y,
                (double) obj.Position.X - (double) this.m_position.X);
            if ((double) num < 0.0)
                num += 6.283185f;
            return num;
        }

        /// <summary>Returns whether this Object is behind the given obj</summary>
        public bool IsBehind(WorldObject obj)
        {
            if (obj == this)
                return false;
            float num = Math.Abs(obj.m_orientation - obj.GetAngleTowards(this.m_position));
            if ((double) num >= 2.09439516067505)
                return (double) num <= 4.1887903213501;
            return false;
        }

        /// <summary>
        /// Returns whether this Object is in front of the given obj
        /// </summary>
        public bool IsInFrontOf(WorldObject obj)
        {
            if (obj == this)
                return false;
            float num = Math.Abs(obj.m_orientation - obj.GetAngleTowards(this.m_position));
            if ((double) num > 1.04719758033752)
                return (double) num >= 5.23598766326904;
            return true;
        }

        /// <summary>
        /// Returns whether the given pos is in front of this Object
        /// </summary>
        public bool IsInFrontOfThis(Vector3 pos)
        {
            float num = Math.Abs(this.m_orientation - this.GetAngleTowards(pos));
            if ((double) num > 1.04719758033752)
                return (double) num >= 5.23598766326904;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetPointXY(float angle, float dist, out Vector3 pos)
        {
            pos = this.m_position;
            this.m_position.GetPointYX(angle + this.m_orientation, dist, out pos);
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetPointInFront(float dist, out Vector3 pos)
        {
            this.GetPointXY(0.0f, dist, out pos);
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetPointBehind(float dist, out Vector3 pos)
        {
            this.GetPointXY(3.141593f, dist, out pos);
        }

        public virtual ClientLocale Locale
        {
            get { return RealmServerConfiguration.DefaultLocale; }
            set { WorldObject.log.Warn("Tried to illegaly set WorldObject.Locale for: {0}", (object) this); }
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
            this.Say(RealmLocalizer.Instance.Translate(this.Locale, key, args));
        }

        public void Say(string message, params object[] args)
        {
            this.Say(string.Format(message, args));
        }

        public void Say(string[] localizedMsgs)
        {
            this.Say(ChatMgr.ListeningRadius, localizedMsgs);
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
            this.Yell(ChatMgr.YellRadius, message);
        }

        public void Yell(RealmLangKey key, params object[] args)
        {
            this.Yell(RealmLocalizer.Instance.Translate(this.Locale, key, args));
        }

        public void Yell(string message, params object[] args)
        {
            this.Yell(string.Format(message, args));
        }

        public void Yell(string[] localizedMsgs)
        {
            this.Yell(ChatMgr.YellRadius, localizedMsgs);
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
            this.Emote(ChatMgr.ListeningRadius, message);
        }

        public void Emote(RealmLangKey key, params object[] args)
        {
            this.Emote(RealmLocalizer.Instance.Translate(this.Locale, key, args));
        }

        public void Emote(string message, params object[] args)
        {
            this.Emote(string.Format(message, args));
        }

        public void Emote(string[] localizedMsgs)
        {
            this.Emote(ChatMgr.ListeningRadius, localizedMsgs);
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
            WCell.Core.Network.Locale locale = WCell.Core.Network.Locale.Any, float? radius = null)
        {
            if (this.Map == null || Thread.CurrentThread.ManagedThreadId != this.m_Map.CurrentThreadId)
                return;
            float? nullable = radius;
            float num = nullable.HasValue ? nullable.GetValueOrDefault() : WorldObject.BroadcastRange;
            if (!this.IsAreaActive)
                return;
            if (locale == WCell.Core.Network.Locale.Any)
            {
                for (int index = this.m_Map.Characters.Count - 1; index >= 0; --index)
                {
                    Character character = this.m_Map.Characters[index];
                    if ((includeSelf || !this.Equals((object) character)) &&
                        (double) num > (double) this.GetDistance((WorldObject) character))
                        character.Send(packet, addEnd);
                }
            }
            else
            {
                for (int index = this.m_Map.CharactersByLocale[locale].Count - 1; index >= 0; --index)
                {
                    Character character = this.m_Map.CharactersByLocale[locale][index];
                    if ((includeSelf || !this.Equals((object) character)) &&
                        (double) num > (double) this.GetDistance((WorldObject) character))
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
            if (!this.IsAreaActive)
                return;
            uint uint32 = this.GetUInt32(field.RawId);
            using (UpdatePacket fieldUpdatePacket = this.GetFieldUpdatePacket(field, uint32))
                rcv.Send((RealmPacketOut) fieldUpdatePacket, false);
        }

        /// <summary>
        /// Ensures that the given action is always executed in map context of this Character - which
        /// might be right now or after the Character is added to a map or during the next Map update.
        /// </summary>
        /// <returns>Whether the Action has been executed immediately (or enqueued)</returns>
        public bool ExecuteInContext(Action action)
        {
            if (!this.IsInContext)
            {
                this.AddMessage((Action) (() => action()));
                return false;
            }

            action();
            return true;
        }

        public void AddMessage(IMessage msg)
        {
            this.m_messageQueue.Enqueue(msg);
        }

        public void AddMessage(Action action)
        {
            this.m_messageQueue.Enqueue((IMessage) new Message(action));
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
            if (this.HasMaster)
            {
                if (object.ReferenceEquals((object) this.Master, (object) opponent))
                    return true;
                return this.Master.IsFriendlyWith(opponent);
            }

            if (opponent is WorldObject && ((WorldObject) opponent).HasMaster)
                opponent = (IFactionMember) ((WorldObject) opponent).Master;
            if (object.ReferenceEquals((object) opponent, (object) this))
                return true;
            if (opponent is Character)
                return ((WorldObject) opponent).IsFriendlyWith((IFactionMember) this);
            Faction faction1 = this.Faction;
            Faction faction2 = opponent.Faction;
            if (faction1 == opponent.Faction)
                return true;
            if (faction1 != null && faction2 != null)
                return faction1.IsFriendlyTowards(faction2);
            return false;
        }

        /// <summary>
        /// Indicates whether the 2 units are neutral towards each other.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAtLeastNeutralWith(IFactionMember opponent)
        {
            if (this.IsFriendlyWith(opponent))
                return true;
            Faction faction1 = this.Faction;
            Faction faction2 = opponent.Faction;
            return faction1 != null && faction2 != null && faction1.Neutrals.Contains(faction2);
        }

        /// <summary>
        /// Indicates whether the 2 units are hostile towards each other.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsHostileWith(IFactionMember opponent)
        {
            if (this.HasMaster)
            {
                if (object.ReferenceEquals((object) this.Master, (object) opponent))
                    return false;
                return this.Master.IsHostileWith(opponent);
            }

            if (opponent is WorldObject && ((WorldObject) opponent).HasMaster)
                opponent = (IFactionMember) ((WorldObject) opponent).Master;
            if (object.ReferenceEquals((object) opponent, (object) this))
                return false;
            if (opponent is Character)
                return ((WorldObject) opponent).IsHostileWith((IFactionMember) this);
            Faction faction1 = this.Faction;
            Faction faction2 = opponent.Faction;
            return faction1 != faction2 && faction1 != null &&
                   (faction2 != null && faction1.Enemies.Contains(faction2));
        }

        public virtual bool MayAttack(IFactionMember opponent)
        {
            if (this.HasMaster)
            {
                if (object.ReferenceEquals((object) this.Master, (object) opponent))
                    return false;
                return this.Master.MayAttack(opponent);
            }

            if (opponent is WorldObject && ((WorldObject) opponent).HasMaster)
                opponent = (IFactionMember) ((WorldObject) opponent).Master;
            if (opponent == null || object.ReferenceEquals((object) opponent, (object) this))
                return false;
            Unit unit = opponent as Unit;
            if (unit != null && !unit.IsVisible)
                return false;
            Character character = opponent as Character;
            if (character != null && this is NPC && !character.Client.IsConnected)
                return false;
            if (character != null)
                return character.MayAttack((IFactionMember) this);
            Faction faction1 = this.Faction;
            Faction faction2 = opponent.Faction;
            return faction1 != faction2 && faction1 != null &&
                   (faction2 != null && faction1.Enemies.Contains(faction2));
        }

        public virtual bool IsAlliedWith(IFactionMember opponent)
        {
            if (this.HasMaster)
            {
                if (object.ReferenceEquals((object) this.Master, (object) opponent))
                    return true;
                return this.Master.IsAlliedWith(opponent);
            }

            if (opponent is WorldObject && ((WorldObject) opponent).HasMaster)
                opponent = (IFactionMember) ((WorldObject) opponent).Master;
            if (object.ReferenceEquals((object) opponent, (object) this))
                return true;
            if (opponent is Character)
                return ((WorldObject) opponent).IsAlliedWith((IFactionMember) this);
            Faction faction1 = this.Faction;
            Faction faction2 = opponent.Faction;
            if (faction1 == opponent.Faction)
                return true;
            if (faction1 != null && opponent.Faction != null)
                return faction1.Friends.Contains(faction2);
            return false;
        }

        public virtual bool IsInSameDivision(IFactionMember opponent)
        {
            if (opponent is Character)
                return ((WorldObject) opponent).IsInSameDivision((IFactionMember) this);
            return this.IsAlliedWith(opponent);
        }

        /// <summary>
        /// Indicates whether we can currently do any harm and are allowed to attack
        /// the given opponent (hostile or neutral factions, duel partners etc)
        /// </summary>
        public virtual bool CanHarm(WorldObject opponent)
        {
            if (this.CanDoHarm)
                return this.MayAttack((IFactionMember) opponent);
            return false;
        }

        /// <summary>
        /// The set of currently active AreaAuras or null.
        /// Do not modify the list.
        /// </summary>
        public List<AreaAura> AreaAuras
        {
            get { return this.m_areaAuras; }
        }

        public bool HasAreaAuras
        {
            get
            {
                if (this.m_areaAuras != null)
                    return this.m_areaAuras.Count > 0;
                return false;
            }
        }

        /// <summary>Called when AreaAura is created</summary>
        internal void AddAreaAura(AreaAura aura)
        {
            if (this.m_areaAuras == null)
                this.m_areaAuras = new List<AreaAura>(2);
            else if (aura.Spell.AttributesExB.HasFlag((Enum) SpellAttributesExB.ExclusiveAreaAura))
            {
                foreach (AreaAura areaAura in this.m_areaAuras)
                {
                    if (aura.Spell.AttributesExB.HasFlag((Enum) SpellAttributesExB.ExclusiveAreaAura))
                    {
                        areaAura.Remove(true);
                        break;
                    }
                }
            }

            this.m_areaAuras.Add(aura);
        }

        /// <summary>Returns the first AreaAura of the given spell</summary>
        public AreaAura GetAreaAura(Spell spell)
        {
            if (this.m_areaAuras == null)
                return (AreaAura) null;
            return this.m_areaAuras.FirstOrDefault<AreaAura>((Func<AreaAura, bool>) (aura => aura.Spell == spell));
        }

        public bool CancelAreaAura(Spell spell)
        {
            AreaAura areaAura = this.GetAreaAura(spell);
            if (areaAura != null)
                return this.CancelAreaAura(areaAura);
            return false;
        }

        /// <summary>Called by AreaAura.Remove</summary>
        internal bool CancelAreaAura(AreaAura aura)
        {
            if (this.m_areaAuras == null || !this.m_areaAuras.Remove(aura))
                return false;
            if (this is Unit)
                ((Unit) this).Auras.Remove(aura.Spell);
            else if (this.m_areaAuras.Count == 0 && (this.IsTrap || this is DynamicObject))
                this.Delete();
            return true;
        }

        /// <summary>
        /// Indicates whether this Object can see the other object
        /// </summary>
        public virtual bool CanSee(WorldObject obj)
        {
            return this.IsInPhase(obj);
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
                if (this.IsPlayer)
                    return true;
                if (this.m_master != null)
                    return this.m_master.IsPlayer;
                return false;
            }
        }

        /// <summary>Whether this object's master is a player</summary>
        public bool HasPlayerMaster
        {
            get
            {
                if (this.m_master != null)
                    return this.m_master.IsPlayer;
                return false;
            }
        }

        public Character PlayerOwner
        {
            get
            {
                if (!(this is Character))
                    return this.m_master as Character;
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
            float diff = (WorldObject.HighlightScale - 1f) * this.ScaleX;
            this.ScaleX = WorldObject.HighlightScale * this.ScaleX;
            this.CallDelayed(WorldObject.HighlightDelayMillis, (Action<WorldObject>) (obj => obj.ScaleX -= diff));
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
            if (textByEnglishPrefix == null)
                return;
            this.PlayTextAndSound(textByEnglishPrefix);
        }

        /// <summary>Play a text and sound identify by the id</summary>
        /// <param name="id">Id of the text in creature_ai_texts</param>
        public void PlayTextAndSoundById(int id)
        {
            NPCAiText firstTextById = NPCAiTextMgr.GetFirstTextById(id);
            if (firstTextById == null)
                return;
            this.PlayTextAndSound(firstTextById);
        }

        public void PlayTextAndSound(NPCAiText text)
        {
            this.PlaySound((uint) text.Sound);
            this.Yell(text.Texts);
        }

        /// <summary>Deleted objects must never be used again!</summary>
        public bool IsDeleted
        {
            get { return this.m_Deleted; }
        }

        [NotVariable] public int UniqWorldEntityId { get; set; }

        public bool IsAsda2Teleporting { get; set; }

        /// <summary>
        /// Enqueues a message to remove this WorldObject from it's Map
        /// </summary>
        public void RemoveFromMap()
        {
            if (!this.IsInWorld)
                return;
            this.m_Map.RemoveObjectLater(this);
        }

        /// <summary>
        /// Enqueues a message to remove this Object from the World and dispose it.
        /// </summary>
        public virtual void Delete()
        {
            if (this.m_Deleted)
                return;
            this.m_Deleted = true;
            if (this.m_Map != null)
                this.m_Map.AddMessage(new Action(this.DeleteNow));
            else
                this.DeleteNow();
        }

        /// <summary>Removes this Object from the World and disposes it.</summary>
        /// <see cref="M:WCell.RealmServer.Entities.WorldObject.Delete" />
        /// <remarks>Requires map context</remarks>
        protected internal virtual void DeleteNow()
        {
            try
            {
                this.m_Deleted = true;
                this.OnDeleted();
                this.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed to correctly delete object \"{0}\"", (object) this), ex);
            }
        }

        protected void OnDeleted()
        {
            this.m_updateActions = (List<ObjectUpdateTimer>) null;
            if (this.m_Map == null)
                return;
            if (this.m_areaAuras != null)
            {
                foreach (AreaAura areaAura in this.m_areaAuras.ToArray())
                    areaAura.Remove(true);
            }

            this.m_Map.RemoveObjectNow(this);
        }

        public override void Dispose(bool disposing)
        {
            this.SpellCast = (SpellCast) null;
            this.m_Map = (Map) null;
        }

        public override string ToString()
        {
            return this.Name + " (" + (object) this.EntityId + ")";
        }

        /// <summary>ONLY ENGLISH!</summary>
        /// <param name="msg"></param>
        /// <param name="c"></param>
        public void SendMessageToArea(string msg, Color c)
        {
            using (RealmPacketOut globalChatMessage =
                ChatMgr.CreateGlobalChatMessage(this.Name, msg, c, WCell.Core.Network.Locale.Start))
                this.SendPacketToArea(globalChatMessage, true, false, WCell.Core.Network.Locale.Any, new float?());
        }
    }
}