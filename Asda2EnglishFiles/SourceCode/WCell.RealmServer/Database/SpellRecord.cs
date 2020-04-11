using Castle.ActiveRecord;
using WCell.Constants.Spells;
using WCell.Core.Database;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Database
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class SpellRecord : WCellRecord<SpellRecord>
    {
        private static readonly NHIdGenerator _idGenerator =
            new NHIdGenerator(typeof(SpellRecord), nameof(RecordId), 1L);

        public const int NoSpecIndex = -1;

        [Field("SpellId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int m_spellId;

        [Field("OwnerId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int m_ownerId;

        /// <summary>Returns the next unique Id for a new SpellRecord</summary>
        public static long NextId()
        {
            return SpellRecord._idGenerator.Next();
        }

        public static SpellRecord[] LoadAllRecordsFor(uint lowId)
        {
            return ActiveRecordBase<SpellRecord>.FindAllByProperty("m_ownerId", (object) (int) lowId);
        }

        private SpellRecord()
        {
        }

        public SpellRecord(SpellId id, uint ownerId, int specIndex)
        {
            this.SpellId = id;
            this.OwnerId = ownerId;
            this.SpecIndex = specIndex;
            this.RecordId = SpellRecord.NextId();
            this.State = RecordState.New;
        }

        [PrimaryKey(PrimaryKeyType.Assigned, "SpellRecordId")]
        public long RecordId { get; set; }

        public uint OwnerId
        {
            get { return (uint) this.m_ownerId; }
            set { this.m_ownerId = (int) value; }
        }

        public SpellId SpellId
        {
            get { return (SpellId) this.m_spellId; }
            set { this.m_spellId = (int) value; }
        }

        public Spell Spell
        {
            get { return SpellHandler.Get(this.SpellId); }
        }

        [Property] public int SpecIndex { get; set; }

        public bool MatchesSpec(int index)
        {
            if (this.SpecIndex != index)
                return index == -1;
            return true;
        }

        public override string ToString()
        {
            return this.m_spellId.ToString() + " (" + (object) this.SpellId + ")";
        }
    }
}