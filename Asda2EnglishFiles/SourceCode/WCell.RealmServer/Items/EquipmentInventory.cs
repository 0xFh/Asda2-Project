using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
    /// <summary>Represents the Equipment</summary>
    public class EquipmentInventory : PartialInventory, IItemSlotHandler
    {
        public EquipmentInventory(PlayerInventory baseInventory)
            : base(baseInventory)
        {
        }

        public override int Offset
        {
            get { return 0; }
        }

        public override int End
        {
            get { return 18; }
        }

        public Item this[EquipmentSlot slot]
        {
            get { return this.m_inventory.Items[(int) slot]; }
        }

        /// <summary>
        /// Is called before adding to check whether the item may be added to the corresponding slot
        /// (given the case that the corresponding slot is valid and unoccupied)
        /// </summary>
        public void CheckAdd(int slot, int amount, IMountableItem item, ref InventoryError err)
        {
            ItemTemplate template = item.Template;
            err = template.CheckEquip(this.m_inventory.Owner);
            if (err != InventoryError.OK)
                return;
            if (template.EquipmentSlots == null)
                err = InventoryError.ITEM_CANT_BE_EQUIPPED;
            else if (!((IEnumerable<EquipmentSlot>) template.EquipmentSlots).Contains<EquipmentSlot>(
                (EquipmentSlot) slot))
                err = InventoryError.ITEM_DOESNT_GO_TO_SLOT;
            else if (slot == 16)
            {
                Item obj = this.m_inventory[InventorySlot.AvLeftHead];
                if (obj != null && obj.Template.IsTwoHandWeapon)
                {
                    err = InventoryError.CANT_EQUIP_WITH_TWOHANDED;
                }
                else
                {
                    if (!template.IsWeapon || this.m_inventory.Owner.Skills.Contains(SkillId.DualWield))
                        return;
                    err = InventoryError.CANT_DUAL_WIELD;
                }
            }
            else if (template.IsTwoHandWeapon && this.m_inventory[EquipmentSlot.OffHand] != null)
            {
                err = InventoryError.CANT_EQUIP_WITH_TWOHANDED;
            }
            else
            {
                if (item.IsEquipped)
                    return;
                err = this.m_inventory.CheckEquipCount(item);
            }
        }

        /// <summary>
        /// Is called before removing the given item to check whether it may actually be removed
        /// </summary>
        public void CheckRemove(int slot, IMountableItem templ, ref InventoryError err)
        {
            if (!(templ is IWeapon) || !templ.Template.IsWeapon ||
                this.Owner.MayCarry(templ.Template.InventorySlotMask))
                return;
            err = InventoryError.CANT_DO_WHILE_DISARMED;
        }

        /// <summary>
        /// Is called after the given item is added to the given slot
        /// </summary>
        public void Added(Item item)
        {
            item.OnEquipDecision();
        }

        /// <summary>
        /// Is called after the given item is removed from the given slot
        /// </summary>
        public void Removed(int slot, Item item)
        {
            item.OnUnequipDecision((InventorySlot) slot);
        }
    }
}