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
            if (entry == null)
                return (GameObject) null;
            return GameObject.Create(entry, location, spawnEntry, spawnPoint);
        }

        /// <summary>Creates a new GameObject with the given parameters</summary>
        public static GameObject Create(GOEntryId id, Map map, GOSpawnEntry spawnEntry = null,
            GOSpawnPoint spawnPoint = null)
        {
            GOEntry entry = GOMgr.GetEntry(id, true);
            if (entry != null)
                return GameObject.Create(entry, map, spawnEntry, spawnPoint);
            return (GameObject) null;
        }

        public static GameObject Create(GOEntry entry, Map map, GOSpawnEntry spawnEntry = null,
            GOSpawnPoint spawnPoint = null)
        {
            return GameObject.Create(entry, (IWorldLocation) new WorldLocation(map, Vector3.Zero, 1U), spawnEntry,
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
            if (handlerCreator != null)
            {
                gameObject.Handler = handlerCreator();
                gameObject.Phase = where.Phase;
                Vector3 position = where.Position;
                if (spawnPoint == null)
                    position.Z = where.Map.Terrain.GetGroundHeightUnderneath(position);
                where.Map.AddObject((WorldObject) gameObject, ref position);
                gameObject.MarkUpdate((UpdateFieldId) GameObjectFields.DYNAMIC);
                return gameObject;
            }

            GameObject.log.Warn("GOEntry {0} did not have a HandlerCreator set - Type: {1}", (object) entry,
                (object) entry.Type);
            gameObject.Delete();
            return (GameObject) null;
        }

        /// <summary>Initialize the GO</summary>
        /// <param name="entry"></param>
        /// <param name="templ"></param>
        internal virtual void Init(GOEntry entry, GOSpawnEntry spawnEntry, GOSpawnPoint spawnPoint)
        {
            this.EntityId =
                EntityId.GetGameObjectId((uint) Interlocked.Increment(ref GameObject._lastGOUID), entry.GOId);
            this.Type |= ObjectTypes.GameObject;
            this.m_entry = entry;
            this.m_spawnPoint = spawnPoint;
            this.GoId = entry.GOId;
            this.DisplayId = entry.DisplayId;
            this.EntryId = entry.Id;
            this.GOType = entry.Type;
            this.Flags = this.m_entry.Flags;
            this.m_faction = this.m_entry.Faction ?? Faction.NullFaction;
            this.ScaleX = this.m_entry.Scale;
            this.GossipMenu = entry.DefaultGossip;
            if (this.QuestHolderInfo != null && this.GossipMenu == null)
                this.GossipMenu = new GossipMenu();
            spawnEntry = spawnEntry ?? entry.FirstSpawnEntry;
            if (spawnEntry != null)
            {
                this.Phase = spawnEntry.Phase;
                this.State = spawnEntry.State;
                if ((double) spawnEntry.Scale != 1.0)
                    this.ScaleX = spawnEntry.Scale;
                this.Orientation = spawnEntry.Orientation;
                this.AnimationProgress = spawnEntry.AnimProgress;
                this.SetRotationFields(spawnEntry.Rotations);
            }

            this.m_entry.InitGO(this);
        }

        public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
        {
            get { return UpdateFieldHandler.DynamicGOHandlers; }
        }

        public GOEntryId GoId { get; set; }

        public LockEntry Lock
        {
            get { return this.m_entry.Lock; }
        }

        public override void OnFinishedLooting()
        {
            if (!this.m_entry.IsConsumable)
                return;
            this.Delete();
        }

        public override uint GetLootId(Asda2LootEntryType type)
        {
            if (this.m_entry is IGOLootableEntry)
                return ((IGOLootableEntry) this.m_entry).LootId;
            return 0;
        }

        public override bool UseGroupLoot
        {
            get { return this.m_entry.UseGroupLoot; }
        }

        protected internal override void OnEnterMap()
        {
            if (this.m_entry.LinkedTrap != null)
                this.m_linkedTrap = this.m_entry.LinkedTrap.Spawn((IWorldLocation) this, this.m_master);
            if (this.m_spawnPoint != null)
                this.m_spawnPoint.SignalSpawnlingActivated(this);
            this.m_entry.NotifyActivated(this);
        }

        protected internal override void OnLeavingMap()
        {
            if (this.m_master is Character && this.m_master.IsInWorld)
                ((Character) this.m_master).OnOwnedGODestroyed(this);
            this.m_handler.OnRemove();
            this.SendDespawn();
            base.OnLeavingMap();
        }

        public bool IsCloseEnough(Unit unit, float radius = 10f)
        {
            if (unit.IsInRadius((WorldObject) this, radius))
                return true;
            if (unit is Character)
                return ((Character) unit).Role.IsStaff;
            return false;
        }

        public bool CanUseInstantly(Character chr)
        {
            if (!this.IsCloseEnough((Unit) chr, 10f) || this.Lock != null)
                return false;
            return this.CanBeUsedBy(chr);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CanBeUsedBy(Character chr)
        {
            if (!this.IsEnabled)
                return false;
            if (this.Flags.HasFlag((Enum) GameObjectFlags.ConditionalInteraction))
                return chr.QuestLog.IsRequiredForAnyQuest(this);
            return true;
        }

        /// <summary>
        /// Makes the given Unit use this GameObject.
        /// Skill-locked GameObjects cannot be used directly but must be interacted on with spells.
        /// </summary>
        public bool Use(Character chr)
        {
            if (this.Lock != null && !this.Lock.IsUnlocked && this.Lock.Keys.Length <= 0 || !this.Handler.TryUse(chr))
                return false;
            if (this.Entry.PageId != 0U)
                MiscHandler.SendGameObjectTextPage((IPacketReceiver) chr, (IEntity) this);
            if (this.GossipMenu != null)
                chr.StartGossip(this.GossipMenu, (WorldObject) this);
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
            get { return this.m_entry.QuestHolderInfo; }
            internal set { this.m_entry.QuestHolderInfo = value; }
        }

        public bool CanGiveQuestTo(Character chr)
        {
            return this.IsInRadiusSq((IHasPosition) chr, (float) GOMgr.DefaultInteractDistanceSq);
        }

        public void OnQuestGiverStatusQuery(Character chr)
        {
        }

        private void DecayNow(int dt)
        {
            this.Delete();
        }

        protected internal override void DeleteNow()
        {
            if (this.m_spawnPoint != null)
                this.m_spawnPoint.SignalSpawnlingDied(this);
            if (this.m_linkedTrap != null)
                this.m_linkedTrap.DeleteNow();
            base.DeleteNow();
        }

        private void StopDecayTimer()
        {
            if (this.m_decayTimer == null)
                return;
            this.m_decayTimer.Stop();
            this.m_decayTimer = (TimerEntry) null;
        }

        /// <summary>
        /// Can be set to initialize Decay after the given delay in seconds.
        /// Will stop the timer if set to a value less than 0
        /// </summary>
        public int RemainingDecayDelayMillis
        {
            get { return this.m_decayTimer.RemainingInitialDelayMillis; }
            set
            {
                if (value < 0)
                {
                    this.StopDecayTimer();
                }
                else
                {
                    this.m_decayTimer = new TimerEntry(new Action<int>(this.DecayNow));
                    this.m_decayTimer.Start(value, 0);
                }
            }
        }

        public override void Update(int dt)
        {
            base.Update(dt);
            if (this.m_decayTimer == null)
                return;
            this.m_decayTimer.Update(dt);
        }

        protected override UpdateFieldCollection _UpdateFieldInfos
        {
            get { return GameObject.UpdateFieldInfos; }
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
            return this.m_entry is GODuelFlagEntry ? UpdateType.CreateSelf : UpdateType.Create;
        }

        protected override void WriteMovementUpdate(PrimitiveWriter packet, UpdateFieldFlags relation)
        {
            if (this.UpdateFlags.HasAnyFlag(UpdateFlags.StationaryObjectOnTransport))
            {
                EntityId.Zero.WritePacked((BinaryWriter) packet);
                packet.Write(this.Position);
                packet.Write(this.Position);
                packet.Write(this.Orientation);
                packet.Write(0.0f);
            }
            else
            {
                if (!this.UpdateFlags.HasAnyFlag(UpdateFlags.StationaryObject))
                    return;
                packet.Write(this.Position);
                packet.WriteFloat(this.Orientation);
            }
        }

        protected override void WriteTypeSpecificMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation,
            UpdateFlags updateFlags)
        {
            if (updateFlags.HasAnyFlag(UpdateFlags.Transport))
                writer.Write(Utility.GetSystemTime());
            if (!updateFlags.HasAnyFlag(UpdateFlags.HasRotation))
                return;
            writer.Write(this.Rotation);
        }

        public override string ToString()
        {
            return this.m_entry.DefaultName + " (SpawnPoint: " + (object) this.m_spawnPoint + ")";
        }

        public GossipMenu GossipMenu { get; set; }

        public GameObjectHandler Handler
        {
            get { return this.m_handler; }
            set
            {
                this.m_handler = value;
                this.m_handler.Initialize(this);
            }
        }

        public override string Name
        {
            get
            {
                if (this.m_entry == null)
                    return "";
                return this.m_entry.DefaultName;
            }
            set { throw new NotImplementedException("Dynamic renaming of GOs is not implementable."); }
        }

        public GOEntry Entry
        {
            get { return this.m_entry; }
        }

        public override ObjectTemplate Template
        {
            get { return (ObjectTemplate) this.Entry; }
        }

        /// <summary>The Template of this GO (if any was used)</summary>
        public GOSpawnPoint SpawnPoint
        {
            get { return this.m_spawnPoint; }
        }

        /// <summary>Traps get removed when their AreaAura gets removed</summary>
        public override bool IsTrap
        {
            get { return this.m_IsTrap; }
        }

        public EntityId CreatedBy
        {
            get { return this.GetEntityId((UpdateFieldId) GameObjectFields.OBJECT_FIELD_CREATED_BY); }
            set { this.SetEntityId((UpdateFieldId) GameObjectFields.OBJECT_FIELD_CREATED_BY, value); }
        }

        public uint DisplayId
        {
            get { return this.GetUInt32(GameObjectFields.DISPLAYID); }
            set { this.SetUInt32((UpdateFieldId) GameObjectFields.DISPLAYID, value); }
        }

        public GameObjectFlags Flags
        {
            get { return (GameObjectFlags) this.GetUInt32(GameObjectFields.FLAGS); }
            set { this.SetUInt32((UpdateFieldId) GameObjectFields.FLAGS, (uint) value); }
        }

        public bool IsStealthed { get; set; }

        public bool IsEnabled
        {
            get { return this.GetByte((UpdateFieldId) GameObjectFields.BYTES_1, 0) == (byte) 1; }
            set { this.SetByte((UpdateFieldId) GameObjectFields.BYTES_1, 0, value ? (byte) 1 : (byte) 0); }
        }

        public GameObjectState State
        {
            get { return (GameObjectState) this.GetByte((UpdateFieldId) GameObjectFields.BYTES_1, 0); }
            set { this.SetByte((UpdateFieldId) GameObjectFields.BYTES_1, 0, (byte) value); }
        }

        public GameObjectType GOType
        {
            get { return (GameObjectType) this.GetByte((UpdateFieldId) GameObjectFields.BYTES_1, 1); }
            set { this.SetByte((UpdateFieldId) GameObjectFields.BYTES_1, 1, (byte) value); }
        }

        /// <summary>No idea</summary>
        public byte ArtKit
        {
            get { return this.GetByte((UpdateFieldId) GameObjectFields.BYTES_1, 2); }
            set { this.SetByte((UpdateFieldId) GameObjectFields.BYTES_1, 2, value); }
        }

        /// <summary>Seems to be 0 or 100 mostly</summary>
        public byte AnimationProgress
        {
            get { return this.GetByte((UpdateFieldId) GameObjectFields.BYTES_1, 3); }
            set { this.SetByte((UpdateFieldId) GameObjectFields.BYTES_1, 3, value); }
        }

        public byte[] Dynamic
        {
            get { return this.GetByteArray((UpdateFieldId) GameObjectFields.DYNAMIC); }
            set { this.SetByteArray((UpdateFieldId) GameObjectFields.DYNAMIC, value); }
        }

        public override Faction Faction
        {
            get { return this.m_faction; }
            set
            {
                this.m_faction = value;
                this.SetUInt32((UpdateFieldId) GameObjectFields.FACTION, value.Template.Id);
            }
        }

        public override FactionId FactionId
        {
            get { return this.m_faction.Id; }
            set
            {
                Faction faction = FactionMgr.Get(value);
                if (faction != null)
                    this.Faction = faction;
                else
                    this.SetUInt32((UpdateFieldId) GameObjectFields.FACTION, (uint) value);
            }
        }

        public int Level
        {
            get { return this.GetInt32(GameObjectFields.LEVEL); }
            set { this.SetInt32((UpdateFieldId) GameObjectFields.LEVEL, value); }
        }

        public Unit Owner
        {
            get { return this.m_master; }
            set
            {
                this.Master = value;
                if (value != null)
                {
                    this.Faction = value.Faction;
                    this.Level = value.Level;
                }
                else
                    this.Faction = Faction.NullFaction;
            }
        }

        public float ParentRotation1
        {
            get { return this.GetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION); }
            set { this.SetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION, value); }
        }

        public float ParentRotation2
        {
            get { return this.GetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION_2); }
            set { this.SetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION_2, value); }
        }

        public float ParentRotation3
        {
            get { return this.GetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION_3); }
            set { this.SetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION_3, value); }
        }

        public float ParentRotation4
        {
            get { return this.GetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION_4); }
            set { this.SetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION_4, value); }
        }

        public long Rotation { get; set; }

        protected void SetRotationFields(float[] rotations)
        {
            if (rotations.Length != 4)
                return;
            this.SetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION, rotations[0]);
            this.SetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION_2, rotations[1]);
            double num1 = Math.Sin((double) this.Orientation / 2.0);
            double num2 = Math.Cos((double) this.Orientation / 2.0);
            this.Rotation = (long) (num1 / GameObject.RotatationConst * (num2 >= 0.0 ? 1.0 : -1.0)) & 2097151L;
            if ((double) rotations[2] == 0.0 && (double) rotations[3] == 0.0)
            {
                this.SetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION_3, (float) num1);
                this.SetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION_4, (float) num2);
            }
            else
            {
                this.SetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION_3, rotations[2]);
                this.SetFloat((UpdateFieldId) GameObjectFields.PARENTROTATION_4, rotations[3]);
            }
        }

        public override ObjectTypeCustom CustomType
        {
            get { return ObjectTypeCustom.Object | ObjectTypeCustom.GameObject; }
        }

        public void SendCustomAnim(uint anim)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_GAMEOBJECT_CUSTOM_ANIM, 12))
            {
                packet.Write((ulong) this.EntityId);
                packet.Write(anim);
                this.SendPacketToArea(packet, true, true, WCell.Core.Network.Locale.Any, new float?());
            }
        }

        public void SendDespawn()
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_GAMEOBJECT_DESPAWN_ANIM, 8))
            {
                packet.Write((ulong) this.EntityId);
                this.SendPacketToArea(packet, true, true, WCell.Core.Network.Locale.Any, new float?());
            }
        }
    }
}