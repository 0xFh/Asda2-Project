using Castle.ActiveRecord;
using System;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.NPCs.Pets
{
    [Castle.ActiveRecord.ActiveRecord("PetTalentSpellRecords", Access = PropertyAccess.Property)]
    public class PetTalentSpellRecord : ActiveRecordBase<PetTalentSpellRecord>
    {
        [Field("SpellId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private long _spellId;

        [PrimaryKey(PrimaryKeyType.Native, "TalentRecordId")]
        public long RecordId { get; set; }

        [Property("CooldownUntil", NotNull = true)]
        public DateTime? CooldownUntil { get; set; }

        public uint SpellId
        {
            get { return (uint) this._spellId; }
            set { this._spellId = (long) value; }
        }

        public Spell Spell
        {
            get { return SpellHandler.ById[this._spellId]; }
            set { this._spellId = (long) value.SpellId; }
        }
    }
}