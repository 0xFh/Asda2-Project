using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    /// <summary>
    /// This handler is notified whenever an Item is equipped/unequipped
    /// </summary>
    public abstract class ItemEquipmentEventAuraHandler : AuraEffectHandler, IItemEquipmentEventHandler
    {
        protected override void Apply()
        {
        }

        protected override void Remove(bool cancelled)
        {
        }

        public abstract void OnEquip(Item item);

        public abstract void OnBeforeUnEquip(Item item);
    }
}