using System;
using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.RealmServer.Content;
using WCell.RealmServer.Items;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs.Vendors
{
    public class VendorItemEntry : IDataHolder
    {
        [NotPersistent] public static readonly List<VendorItemEntry> EmptyList = new List<VendorItemEntry>(1);
        private int remainingStackAmount;
        public NPCId VendorId;
        public Asda2ItemId ItemId;

        /// <summary>The amount of available stacks</summary>
        public int StockAmount;

        /// <summary>The size of one stack</summary>
        public int BuyStackSize;

        /// <summary>
        /// The time until the vendor restocks, after he/she ran out of this Item
        /// </summary>
        public uint StockRefillDelay;

        public uint ExtendedCostId;
        [NotPersistent] public ItemExtendedCostEntry ExtendedCostEntry;
        [NotPersistent] private DateTime lastUpdate;
        [NotPersistent] public ItemTemplate Template;

        /// <summary>
        /// If this item has a limited supply available, this returns a number smaller than uint.MaxValue
        /// </summary>
        [NotPersistent]
        public int RemainingStockAmount
        {
            get
            {
                if (this.StockAmount <= 0 || this.StockRefillDelay <= 0U)
                    return -1;
                int num = (int) ((long) (DateTime.Now - this.lastUpdate).Milliseconds / (long) this.StockRefillDelay);
                if (this.remainingStackAmount + num > this.StockAmount)
                    this.remainingStackAmount = this.StockAmount;
                else
                    this.remainingStackAmount += num;
                this.lastUpdate = DateTime.Now;
                return this.remainingStackAmount;
            }
            set { this.remainingStackAmount = value; }
        }

        public void FinalizeDataHolder()
        {
            this.Template = ItemMgr.GetTemplate(this.ItemId);
            if (this.Template == null)
            {
                ContentMgr.OnInvalidDBData("{0} has invalid ItemId: {1} ({2})", (object) this, (object) this.ItemId,
                    (object) (int) this.ItemId);
            }
            else
            {
                List<VendorItemEntry> vendorList = NPCMgr.GetOrCreateVendorList(this.VendorId);
                if (this.StockAmount < 0)
                    this.StockAmount = this.Template.StockAmount;
                if (this.StockRefillDelay < 0U)
                    this.StockRefillDelay = this.Template.StockRefillDelay;
                this.remainingStackAmount = this.StockAmount;
                vendorList.Add(this);
            }
        }

        public override string ToString()
        {
            return this.GetType().Name + " " + (object) this.VendorId + " (" + (object) (int) this.VendorId + ")";
        }

        public static IEnumerable<VendorItemEntry> GetAllDataHolders()
        {
            List<VendorItemEntry> vendorItemEntryList = new List<VendorItemEntry>(20000);
            foreach (NPCEntry allEntry in NPCMgr.GetAllEntries())
            {
                if (allEntry != null && allEntry.VendorItems != null)
                    vendorItemEntryList.AddRange((IEnumerable<VendorItemEntry>) allEntry.VendorItems);
            }

            return (IEnumerable<VendorItemEntry>) vendorItemEntryList;
        }
    }
}