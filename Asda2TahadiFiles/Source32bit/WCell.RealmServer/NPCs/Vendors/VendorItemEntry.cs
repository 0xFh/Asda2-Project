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
    [NotPersistent]public static readonly List<VendorItemEntry> EmptyList = new List<VendorItemEntry>(1);
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
    [NotPersistent]public ItemExtendedCostEntry ExtendedCostEntry;
    [NotPersistent]private DateTime lastUpdate;
    [NotPersistent]public ItemTemplate Template;

    /// <summary>
    /// If this item has a limited supply available, this returns a number smaller than uint.MaxValue
    /// </summary>
    [NotPersistent]
    public int RemainingStockAmount
    {
      get
      {
        if(StockAmount <= 0 || StockRefillDelay <= 0U)
          return -1;
        int num = (int) ((DateTime.Now - lastUpdate).Milliseconds / StockRefillDelay);
        if(remainingStackAmount + num > StockAmount)
          remainingStackAmount = StockAmount;
        else
          remainingStackAmount += num;
        lastUpdate = DateTime.Now;
        return remainingStackAmount;
      }
      set { remainingStackAmount = value; }
    }

    public void FinalizeDataHolder()
    {
      Template = ItemMgr.GetTemplate(ItemId);
      if(Template == null)
      {
        ContentMgr.OnInvalidDBData("{0} has invalid ItemId: {1} ({2})", (object) this, (object) ItemId,
          (object) (int) ItemId);
      }
      else
      {
        List<VendorItemEntry> vendorList = NPCMgr.GetOrCreateVendorList(VendorId);
        if(StockAmount < 0)
          StockAmount = Template.StockAmount;
        if(StockRefillDelay < 0U)
          StockRefillDelay = Template.StockRefillDelay;
        remainingStackAmount = StockAmount;
        vendorList.Add(this);
      }
    }

    public override string ToString()
    {
      return GetType().Name + " " + VendorId + " (" + (int) VendorId + ")";
    }

    public static IEnumerable<VendorItemEntry> GetAllDataHolders()
    {
      List<VendorItemEntry> vendorItemEntryList = new List<VendorItemEntry>(20000);
      foreach(NPCEntry allEntry in NPCMgr.GetAllEntries())
      {
        if(allEntry != null && allEntry.VendorItems != null)
          vendorItemEntryList.AddRange(allEntry.VendorItems);
      }

      return vendorItemEntryList;
    }
  }
}