using System;
using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.Util;

namespace WCell.RealmServer.NPCs.Vendors
{
    public class VendorEntry
    {
        /// <summary>
        /// A list of VendorItemEtnries that can be bought at this Vendor.
        /// </summary>
        public readonly List<VendorItemEntry> ItemsForSale = new List<VendorItemEntry>(10);

        public const int UnlimitedSupply = -1;
        public readonly NPC NPC;

        public VendorEntry(NPC npc, List<VendorItemEntry> items)
        {
            this.NPC = npc;
            if (items == null)
                return;
            foreach (VendorItemEntry vendorItemEntry in items)
                this.ItemsForSale.Add(new VendorItemEntry()
                {
                    BuyStackSize = vendorItemEntry.BuyStackSize,
                    RemainingStockAmount = vendorItemEntry.RemainingStockAmount,
                    ExtendedCostEntry = NPCMgr.ItemExtendedCostEntries[(int) vendorItemEntry.ExtendedCostId],
                    StockAmount = vendorItemEntry.StockAmount,
                    StockRefillDelay = vendorItemEntry.StockRefillDelay,
                    Template = vendorItemEntry.Template
                });
        }

        /// <summary>
        /// Returns the VendorItemEntry with the given entryId if contained in the Vendor's ItemsForSale, else null.
        /// </summary>
        public VendorItemEntry GetVendorItem(uint entryId)
        {
            return this.ItemsForSale.Find(
                (Predicate<VendorItemEntry>) (item => (int) item.Template.Id == (int) entryId));
        }

        /// <summary>Character starts a trade-session with this Vendor</summary>
        /// <param name="chr"></param>
        public void UseVendor(Character chr)
        {
            if (!this.CheckVendorInteraction(chr))
                return;
            chr.OnInteract((WorldObject) this.NPC);
            WCell.RealmServer.Handlers.NPCHandler.SendVendorInventoryList(chr, this.NPC, this.ItemsForSale);
        }

        /// <summary>Tries to sell the given Item of the given Character</summary>
        /// <param name="chr">The seller.</param>
        /// <param name="item">May be null (will result into error message for chr)</param>
        /// <param name="amount">The amount of Item to sell.</param>
        public void SellItem(Character chr, Item item, int amount)
        {
        }

        public void BuyBackItem(Character chr, Item item)
        {
        }

        public void BuyItem(Character chr, uint itemEntryId, BaseInventory bag, int amount, int slot)
        {
        }

        private bool CheckVendorInteraction(Character chr)
        {
            if (!this.NPC.CheckVendorInteraction(chr))
                return false;
            chr.Auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);
            return true;
        }

        /// <summary>
        /// Checks whether the given Item may be sold by the given
        /// Character and sends an Error reply if not
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool CanPlayerSellItem(Character chr, Item item, ref SellItemError error)
        {
            if (chr != item.OwningCharacter)
            {
                error = SellItemError.PlayerDoesntOwnItem;
                return false;
            }

            ItemTemplate template = item.Template;
            if (template.IsBag && !((Container) item).BaseInventory.IsEmpty)
            {
                error = SellItemError.OnlyEmptyBag;
                return false;
            }

            if (item.CanBeTraded && template.SellPrice != 0U)
                return true;
            error = SellItemError.CantSellItem;
            return false;
        }

        public List<VendorItemEntry> ConstructVendorItemList(Character curChar)
        {
            return this.ItemsForSale;
        }

        public override string ToString()
        {
            return Utility.GetStringRepresentation((object) this.ItemsForSale);
        }
    }
}