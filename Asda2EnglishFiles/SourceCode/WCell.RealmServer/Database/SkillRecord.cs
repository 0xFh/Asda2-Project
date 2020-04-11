using Castle.ActiveRecord;
using WCell.Constants.Skills;

namespace WCell.RealmServer.Database
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class SkillRecord : ActiveRecordBase<SkillRecord>
    {
        [Field("SkillId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _skillId;

        [Field("CurrentValue", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private short _value;

        [Field("MaxVal", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private short _max;

        [PrimaryKey(PrimaryKeyType.Increment, "EntityLowId")]
        public long Guid { get; set; }

        [Property(NotNull = true)] public long OwnerId { get; set; }

        public SkillId SkillId
        {
            get { return (SkillId) this._skillId; }
            set { this._skillId = (int) value; }
        }

        public ushort CurrentValue
        {
            get { return (ushort) this._value; }
            set { this._value = (short) value; }
        }

        public ushort MaxValue
        {
            get { return (ushort) this._max; }
            set { this._max = (short) value; }
        }

        public static SkillRecord[] GetAllSkillsFor(long charRecordId)
        {
            return ActiveRecordBase<SkillRecord>.FindAllByProperty("OwnerId", (object) charRecordId);
        }
    }
}