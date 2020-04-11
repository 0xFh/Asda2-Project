using Castle.ActiveRecord;
using System;
using WCell.Constants.World;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Database
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class RaidRecord : ActiveRecordBase<RaidRecord>, IMapId
    {
        [Field("CharLowId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _characterLow;

        [Field("InstanceId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _instanceId;

        [Field("MapId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int m_MapId;

        [PrimaryKey(PrimaryKeyType.Native, "InstanceRelationId")]
        public long RecordId { get; set; }

        public uint CharacterLow
        {
            get { return (uint) this._characterLow; }
            set { this._characterLow = (int) value; }
        }

        public uint InstanceId
        {
            get { return (uint) this._instanceId; }
            set { this._instanceId = (int) value; }
        }

        public MapId MapId
        {
            get { return (MapId) this.m_MapId; }
            set { this.m_MapId = (int) value; }
        }

        [Property(NotNull = true)] public DateTime Until { get; set; }
    }
}