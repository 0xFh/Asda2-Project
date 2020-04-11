using Castle.ActiveRecord;
using System;
using WCell.Core.Database;

namespace WCell.RealmServer.Database
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class QuestRecord : WCellRecord<QuestRecord>
    {
        private static readonly NHIdGenerator _idGenerator =
            new NHIdGenerator(typeof(QuestRecord), nameof(QuestRecordId), 1L);

        [Field("QuestTemplateId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private long _questTemplateId;

        [Field("OwnerId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private long _ownerId;

        /// <summary>Returns the next unique Id for a new Record</summary>
        public static long NextId()
        {
            return QuestRecord._idGenerator.Next();
        }

        public QuestRecord(uint qId, uint ownerId)
        {
            this.QuestTemplateId = qId;
            this._ownerId = (long) ownerId;
            this.QuestRecordId = QuestRecord.NextId();
        }

        public QuestRecord()
        {
        }

        [PrimaryKey(PrimaryKeyType.Assigned)] public long QuestRecordId { get; set; }

        public uint OwnerId
        {
            get { return (uint) this._ownerId; }
            set { this._ownerId = (long) value; }
        }

        public uint QuestTemplateId
        {
            get { return (uint) this._questTemplateId; }
            set { this._questTemplateId = (long) value; }
        }

        [Property(NotNull = true)] public int Slot { get; set; }

        [Property(NotNull = false)] public DateTime? TimeUntil { get; set; }

        [Property(NotNull = false)] public uint[] Interactions { get; set; }

        [Property(NotNull = false)] public bool[] VisitedATs { get; set; }

        public static QuestRecord[] GetQuestRecordForCharacter(uint chrId)
        {
            return ActiveRecordBase<QuestRecord>.FindAllByProperty("_ownerId", (object) (long) chrId);
        }
    }
}