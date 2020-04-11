using System;

namespace WCell.RealmServer.Spells
{
    public class SpellIdCooldown : ISpellIdCooldown, ICooldown
    {
        public DateTime Until { get; set; }

        public uint SpellId { get; set; }

        public uint ItemId { get; set; }

        public IConsistentCooldown AsConsistent()
        {
            return (IConsistentCooldown) new PersistentSpellIdCooldown()
            {
                Until = this.Until,
                SpellId = this.SpellId,
                ItemId = this.ItemId
            };
        }
    }
}