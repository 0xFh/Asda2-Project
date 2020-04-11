using Castle.ActiveRecord;
using System;

namespace WCell.RealmServer.Spells
{
    [Castle.ActiveRecord.ActiveRecord("SpellCategoryCooldown", Access = PropertyAccess.Property)]
    public class PersistentSpellCategoryCooldown : ActiveRecordBase<PersistentSpellCategoryCooldown>,
        ISpellCategoryCooldown, IConsistentCooldown, ICooldown
    {
        [Field("CatId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _catId;

        [Field("ItemId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _itemId;

        [Field("CharId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _charId;

        [Field("SpellId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _spellId;

        public static PersistentSpellCategoryCooldown[] LoadCategoryCooldownsFor(uint lowId)
        {
            return ActiveRecordBase<PersistentSpellCategoryCooldown>.FindAllByProperty("_charId", (object) (int) lowId);
        }

        [PrimaryKey(PrimaryKeyType.Increment)] private long Id { get; set; }

        public uint SpellId
        {
            get { return (uint) this._spellId; }
            set { this._spellId = (int) value; }
        }

        public uint CharId
        {
            get { return (uint) this._charId; }
            set { this._charId = (int) value; }
        }

        [Property] public DateTime Until { get; set; }

        public uint CategoryId
        {
            get { return (uint) this._catId; }
            set { this._catId = (int) value; }
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