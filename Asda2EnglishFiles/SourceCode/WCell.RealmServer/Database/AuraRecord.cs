using Castle.ActiveRecord;
using NLog;
using System;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.ObjectPools;

namespace WCell.RealmServer.Database
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class AuraRecord : WCellRecord<AuraRecord>
    {
        internal static readonly ObjectPool<AuraRecord> AuraRecordPool =
            new ObjectPool<AuraRecord>((Func<AuraRecord>) (() => new AuraRecord()));

        private static readonly NHIdGenerator
            _idGenerator = new NHIdGenerator(typeof(AuraRecord), nameof(RecordId), 1L);

        [Field("OwnerId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int m_OwnerId;

        private Spell m_spell;

        /// <summary>Returns the next unique Id for a new SpellRecord</summary>
        public static long NextId()
        {
            return AuraRecord._idGenerator.Next();
        }

        public static AuraRecord ObtainAuraRecord(Aura aura)
        {
            AuraRecord auraRecord = AuraRecord.AuraRecordPool.Obtain();
            auraRecord.State = RecordState.New;
            auraRecord.RecordId = AuraRecord.NextId();
            auraRecord.SyncData(aura);
            return auraRecord;
        }

        public static AuraRecord[] LoadAuraRecords(uint lowId)
        {
            return ActiveRecordBase<AuraRecord>.FindAllByProperty("m_OwnerId", (object) (int) lowId);
        }

        public AuraRecord(Aura aura)
        {
            this.State = RecordState.New;
            this.RecordId = AuraRecord.NextId();
            this.SyncData(aura);
        }

        public AuraRecord()
        {
        }

        public void SyncData(Aura aura)
        {
            this.OwnerId = aura.Auras.Owner.EntityId.Low;
            this.CasterId = (long) aura.CasterReference.EntityId.Full;
            this.Level = (int) aura.Level;
            this.m_spell = aura.Spell;
            this.MillisLeft = !aura.HasTimeout ? -1 : aura.TimeLeft;
            this.StackCount = aura.StackCount;
            this.IsBeneficial = aura.IsBeneficial;
        }

        [PrimaryKey(PrimaryKeyType.Assigned)] public long RecordId { get; set; }

        public uint OwnerId
        {
            get { return (uint) this.m_OwnerId; }
            set { this.m_OwnerId = (int) value; }
        }

        [Property] public long CasterId { get; set; }

        [Property] public int Level { get; set; }

        [Property]
        public int SpellId
        {
            get { return (int) this.m_spell.Id; }
            set
            {
                this.m_spell = SpellHandler.Get((uint) value);
                if (this.m_spell != null)
                    return;
                LogManager.GetCurrentClassLogger().Warn("Aura record {0} has invalid SpellId {1}",
                    (object) this.RecordId, (object) value);
            }
        }

        public Spell Spell
        {
            get { return this.m_spell; }
        }

        [Property] public int MillisLeft { get; set; }

        [Property] public int StackCount { get; set; }

        [Property] public bool IsBeneficial { get; set; }

        public ObjectReference GetCasterInfo(Map map)
        {
            EntityId entityId = new EntityId((ulong) this.CasterId);
            WorldObject worldObject = map.GetObject(entityId);
            if (worldObject != null)
                return worldObject.SharedReference;
            return new ObjectReference(entityId, this.Level);
        }

        public override void Delete()
        {
            base.Delete();
            AuraRecord.AuraRecordPool.Recycle(this);
        }

        internal void Recycle()
        {
            AuraRecord.AuraRecordPool.Recycle(this);
        }
    }
}