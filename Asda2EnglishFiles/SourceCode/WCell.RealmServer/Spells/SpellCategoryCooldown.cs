using System;

namespace WCell.RealmServer.Spells
{
    public class SpellCategoryCooldown : ISpellCategoryCooldown, ICooldown
    {
        public DateTime Until { get; set; }

        public uint SpellId { get; set; }

        public uint CategoryId { get; set; }

        public uint ItemId { get; set; }

        public IConsistentCooldown AsConsistent()
        {
            return (IConsistentCooldown) new PersistentSpellCategoryCooldown()
            {
                Until = this.Until,
                CategoryId = this.CategoryId,
                ItemId = this.ItemId
            };
        }
    }
}