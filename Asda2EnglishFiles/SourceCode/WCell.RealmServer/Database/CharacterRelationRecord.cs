using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.RealmServer.Interaction;

namespace WCell.RealmServer.Database
{
    /// <summary>
    /// Represents a character relationship entry in the database
    /// </summary>
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class CharacterRelationRecord : WCellRecord<CharacterRelationRecord>
    {
        private static readonly NHIdGenerator _idGenerator =
            new NHIdGenerator(typeof(CharacterRelationRecord), nameof(CharacterRelationGuid), 1L);

        [Field("CharacterId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private long _characterId;

        [Field("RelatedCharacterId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private long _relatedCharacterId;

        /// <summary>Returns the next unique Id for a new SpellRecord</summary>
        public static long NextId()
        {
            return CharacterRelationRecord._idGenerator.Next();
        }

        public CharacterRelationRecord()
        {
        }

        public CharacterRelationRecord(uint charId, uint relatedCharId, CharacterRelationType type)
        {
            this.State = RecordState.New;
            this.CharacterId = charId;
            this.RelatedCharacterId = relatedCharId;
            this.RelationType = type;
            this.CharacterRelationGuid = CharacterRelationRecord.NextId();
        }

        [PrimaryKey(PrimaryKeyType.Assigned)] public long CharacterRelationGuid { get; set; }

        public uint CharacterId
        {
            get { return (uint) this._characterId; }
            set { this._characterId = (long) value; }
        }

        public uint RelatedCharacterId
        {
            get { return (uint) this._relatedCharacterId; }
            set { this._relatedCharacterId = (long) value; }
        }

        [Property(NotNull = true)] public CharacterRelationType RelationType { get; set; }

        [Property] public string Note { get; set; }
    }
}