using Castle.ActiveRecord;
using System;

namespace WCell.RealmServer.Spells
{
    [Castle.ActiveRecord.ActiveRecord("SpellIdCooldown", Access = PropertyAccess.Property)]
    public class PersistentSpellIdCooldown : ActiveRecordBase<PersistentSpellIdCooldown>, ISpellIdCooldown,
        IConsistentCooldown, ICooldown
    {
        [Field("SpellId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _spellId;

        [Field("ItemId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _itemId;

        [Field("CharId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _charId;

        public static PersistentSpellIdCooldown[] LoadIdCooldownsFor(uint lowId)
        {
            return ActiveRecordBase<PersistentSpellIdCooldown>.FindAllByProperty("_charId", (object) (int) lowId);
        }

        [PrimaryKey(PrimaryKeyType.Increment)] private long Id { get; set; }

        public uint CharId
        {
            get { return (uint) this._charId; }
            set { this._charId = (int) value; }
        }

        [Property] public DateTime Until { get; set; }

        public uint SpellId
        {
            get { return (uint) this._spellId; }
            set { this._spellId = (int) value; }
        }

        public uint ItemId
        {
            get { return (uint) this._itemId; }
            set { this._itemId = (int) value; }
        }

        public IConsistentCooldown AsConsistent()
        {
            return (IConsistentCooldown) this;
        }
    }
}