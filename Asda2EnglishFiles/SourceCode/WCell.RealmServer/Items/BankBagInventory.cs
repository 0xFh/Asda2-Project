using WCell.Constants.Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Items
{
    /// <summary>Represents all bank bags</summary>
    public class BankBagInventory : EquippedContainerInventory
    {
        public BankBagInventory(PlayerInventory baseInventory)
            : base(baseInventory)
        {
        }

        public override int Offset
        {
            get { return 67; }
        }

        public override int End
        {
            get { return 73; }
        }

        /// <summary>
        /// Tries to increase the amount of slots for bank bags by one.
        /// </summary>
        /// <param name="takeMoney">whether to also check (and subtract) the amount of money for the slot</param>
        /// <returns>whether there was still space left to add (and -if takeMoney is true- also checks whether the funds were sufficient)</returns>
        public BuyBankBagResponse IncBankBagSlotCount(bool takeMoney)
        {
            return BuyBankBagResponse.Ok;
        }

        /// <summary>
        /// Tries to reduce the amount of slots for bank bags by one.
        /// Returns the cont, or null if the amount is already at 0 or if the last cont still contains items.
        /// </summary>
        /// <returns></returns>
        public Container DecBankBagSlotCount()
        {
            Character owner = this.m_inventory.Owner;
            if (owner == null)
                return (Container) null;
            byte bankBagSlots = owner.BankBagSlots;
            if (bankBagSlots == (byte) 0)
                return (Container) null;
            Container container = this.m_inventory.Items[67 + (int) bankBagSlots] as Container;
            if (container == null)
                return (Container) null;
            if (!container.BaseInventory.IsEmpty)
                return (Container) null;
            owner.BankBagSlots = (byte) ((uint) bankBagSlots - 1U);
            return container;
        }

        public override void CheckAdd(int slot, int amount, IMountableItem item, ref InventoryError err)
        {
            if (!this.m_inventory.IsBankOpen)
            {
                err = InventoryError.TOO_FAR_AWAY_FROM_BANK;
            }
            else
            {
                if (slot >= (int) this.m_inventory.Owner.BankBagSlots)
                    return;
                err = InventoryError.MUST_PURCHASE_THAT_BAG_SLOT;
            }
        }
    }
}