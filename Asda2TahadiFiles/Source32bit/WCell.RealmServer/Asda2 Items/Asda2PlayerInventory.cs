using NHibernate;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Asda2PetSystem;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Looting;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.NLog;

namespace WCell.RealmServer.Asda2_Items
{
  public class Asda2PlayerInventory
  {
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<Asda2ItemCategory, DateTime> _cooldowns =
      new Dictionary<Asda2ItemCategory, DateTime>();

    public Asda2Item[] AvatarWarehouseItems = new Asda2Item[270];
    public Dictionary<int, Asda2DonationItem> DonationItems = new Dictionary<int, Asda2DonationItem>();
    public Asda2Item[] Equipment = new Asda2Item[21];
    public Asda2Item[] RegularItems = new Asda2Item[60];
    public Asda2Item[] ShopItems = new Asda2Item[61];
    public Asda2Item[] WarehouseItems = new Asda2Item[270];
    private Dictionary<byte, Asda2FastItemSlotRecord[]> _fastItemSlotRecords;
    private Character m_owner;

    public Asda2PlayerInventory(Character character)
    {
      Owner = character;
    }

    public Character Owner
    {
      get { return m_owner; }
      private set { m_owner = value; }
    }

    public short Weight { get; set; }

    public Dictionary<byte, Asda2FastItemSlotRecord[]> FastItemSlotRecords
    {
      get
      {
        if(_fastItemSlotRecords == null)
        {
          _fastItemSlotRecords = new Dictionary<byte, Asda2FastItemSlotRecord[]>();
          for(byte key = 0; key <= (byte) 5; ++key)
            _fastItemSlotRecords.Add(key, new Asda2FastItemSlotRecord[13]);
        }

        return _fastItemSlotRecords;
      }
    }

    public int FreeRegularSlotsCount
    {
      get
      {
        return RegularItems.Count(
          i => i == null);
      }
    }

    public int FreeShopSlotsCount
    {
      get
      {
        int num = 0;
        for(int index = 0; index < (Owner.InventoryExpanded ? 60 : 30); ++index)
        {
          if(ShopItems[index] == null)
            ++num;
        }

        return num;
      }
    }

    public int FreeWarehouseSlotsCount
    {
      get
      {
        int num = 0;
        for(int index = 0; index < (int) Owner.Record.PremiumWarehouseBagsCount * 30 + 30; ++index)
        {
          if(WarehouseItems[index] == null)
            ++num;
        }

        return num;
      }
    }

    public int FreeAvatarWarehouseSlotsCount
    {
      get
      {
        int num = 0;
        for(int index = 0; index < (int) Owner.Record.PremiumAvatarWarehouseBagsCount * 30 + 30; ++index)
        {
          if(AvatarWarehouseItems[index] == null)
            ++num;
        }

        return num;
      }
    }

    public void SaveAll()
    {
      foreach(Asda2Item asda2Item in Equipment)
      {
        if(asda2Item != null)
          asda2Item.Save();
      }

      foreach(Asda2Item regularItem in RegularItems)
      {
        if(regularItem != null)
          regularItem.Save();
      }

      foreach(Asda2Item shopItem in ShopItems)
      {
        if(shopItem != null)
          shopItem.Save();
      }

      foreach(Asda2Item warehouseItem in WarehouseItems)
      {
        if(warehouseItem != null)
          warehouseItem.Save();
      }

      foreach(Asda2Item avatarWarehouseItem in AvatarWarehouseItems)
      {
        if(avatarWarehouseItem != null)
          avatarWarehouseItem.Save();
      }

      foreach(KeyValuePair<byte, Asda2FastItemSlotRecord[]> fastItemSlotRecord1 in FastItemSlotRecords)
      {
        foreach(Asda2FastItemSlotRecord fastItemSlotRecord2 in fastItemSlotRecord1.Value)
        {
          if(fastItemSlotRecord2 != null)
          {
            try
            {
              fastItemSlotRecord2.Save();
            }
            catch(StaleStateException ex)
            {
            }
          }
        }
      }
    }

    private void SetEquipment(Asda2Item item, Asda2EquipmentSlots slot)
    {
      Asda2Item asda2Item = Equipment[(int) slot];
      if(item != null)
      {
        if(item.IsDeleted)
        {
          LogUtil.WarnException("{0} trying to equip item {1} witch is deleted.", (object) Owner.Name,
            (object) item.ItemId);
          return;
        }

        if(item.Record == null)
        {
          LogUtil.WarnException("{0} trying to equip item {1} witch record is null.", (object) Owner.Name,
            (object) item.ItemId);
          return;
        }

        item.Slot = (short) slot;
        item.InventoryType = Asda2InventoryType.Equipment;
        item.OwningCharacter = Owner;
      }

      Equipment[(int) slot] = item;
      if(item != null)
        item.Save();
      if(item == null && slot == Asda2EquipmentSlots.Weapon)
        Owner.MainWeapon = null;
      else if(item != null && slot == Asda2EquipmentSlots.Weapon && item.IsWeapon)
        Owner.MainWeapon = item;
      if(asda2Item != null)
      {
        Asda2InventoryHandler.SendCharacterRemoveEquipmentResponse(Owner, (short) slot, asda2Item.ItemId);
        asda2Item.OnUnEquip();
      }

      if(item == null)
        return;
      Asda2InventoryHandler.SendCharacterAddEquipmentResponse(Owner, (short) slot, item.ItemId,
        item.Enchant);
      item.OnEquip();
    }

    private void SetRegularInventoty(Asda2Item item, short slot, bool silent)
    {
      if(item != null)
      {
        if(item.IsDeleted)
          throw new InvalidOperationException(string.Format(
            "{0} trying to set regular item {1} witch is deleted.", Owner.Name,
            item.ItemId));
        if(item.Record == null)
        {
          LogUtil.WarnException("{0} trying to regular item {1} witch record is null.", (object) Owner.Name,
            (object) item.ItemId);
          return;
        }

        item.InventoryType = Asda2InventoryType.Regular;
        item.Slot = slot;
        item.OwningCharacter = Owner;
      }

      RegularItems[slot] = item;
      if(silent)
        return;
      Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, Owner, new Asda2Item[1]
      {
        item
      });
    }

    private void SetWarehouseInventoty(Asda2Item item, short slot, bool silent)
    {
      if(item != null)
      {
        if(item.IsDeleted)
          throw new InvalidOperationException(string.Format("{0} trying to set wh item {1} witch is deleted.",
            Owner.Name, item.ItemId));
        if(item.Record == null)
        {
          LogUtil.WarnException("{0} trying to wh item {1} witch record is null.", (object) Owner.Name,
            (object) item.ItemId);
          return;
        }

        item.InventoryType = Asda2InventoryType.Warehouse;
        item.Slot = slot;
        item.OwningCharacter = Owner;
      }

      WarehouseItems[slot] = item;
      if(item == null || silent)
        return;
      Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, Owner, new Asda2Item[1]
      {
        item
      });
    }

    private void SetAvatarWarehouseInventoty(Asda2Item item, short slot, bool silent)
    {
      if(item != null)
      {
        if(item.IsDeleted)
          throw new InvalidOperationException(string.Format(
            "{0} trying to set awh item {1} witch is deleted.", Owner.Name,
            item.ItemId));
        if(item.Record == null)
        {
          LogUtil.WarnException("{0} trying to avatar wh item {1} witch record is null.", (object) Owner.Name,
            (object) item.ItemId);
          return;
        }

        item.InventoryType = Asda2InventoryType.AvatarWarehouse;
        item.Slot = slot;
        item.OwningCharacter = Owner;
      }

      AvatarWarehouseItems[slot] = item;
      if(item == null || silent)
        return;
      Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, Owner, new Asda2Item[1]
      {
        item
      });
    }

    private void SetShopInventoty(Asda2Item item, short slot, bool silent)
    {
      if(item != null)
      {
        if(item.IsDeleted)
          throw new InvalidOperationException(string.Format(
            "{0} trying to set shop item {1} which is deleted.", Owner.Name,
            item.ItemId));
        if(item.Record == null)
        {
          LogUtil.WarnException("{0} trying to set shop item {1} which record is null.", (object) Owner.Name,
            (object) item.ItemId);
          return;
        }

        item.InventoryType = Asda2InventoryType.Shop;
        item.Slot = slot;
        item.OwningCharacter = Owner;
      }

      ShopItems[slot] = item;
      if(item == null || silent)
        return;
      Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, Owner, new Asda2Item[1]
      {
        item
      });
    }

    private void SetItem(Asda2Item item, short slot, Asda2InventoryType inventoryType, bool silent = true)
    {
      switch(inventoryType)
      {
        case Asda2InventoryType.Shop:
          SetShopInventoty(item, slot, silent);
          break;
        case Asda2InventoryType.Regular:
          SetRegularInventoty(item, slot, silent);
          break;
        case Asda2InventoryType.Warehouse:
          SetWarehouseInventoty(item, slot, silent);
          break;
        case Asda2InventoryType.AvatarWarehouse:
          SetAvatarWarehouseInventoty(item, slot, silent);
          break;
        default:
          Owner.SendErrorMsg(string.Format("failed to set item. wrong inventory type {0}",
            inventoryType));
          break;
      }
    }

    private short FindFreeShopItemsSlot()
    {
      for(short index = 0; (int) index < (Owner.InventoryExpanded ? ShopItems.Length : 30); ++index)
      {
        if(ShopItems[index] == null)
          return index;
      }

      return -1;
    }

    private short FindFreeRegularItemsSlot()
    {
      for(short index = 1; (int) index < RegularItems.Length; ++index)
      {
        if(RegularItems[index] == null)
          return index;
      }

      return -1;
    }

    private short FindFreeWarehouseItemsSlot()
    {
      for(short index = 0; (int) index < WarehouseItems.Length; ++index)
      {
        if(WarehouseItems[index] == null)
          return index;
      }

      return -1;
    }

    private short FindFreeAvatarWarehouseItemsSlot()
    {
      for(short index = 0; (int) index < WarehouseItems.Length; ++index)
      {
        if(AvatarWarehouseItems[index] == null)
          return index;
      }

      return -1;
    }

    internal void AddOwnedItems()
    {
      foreach(Asda2DonationItem asda2DonationItem in Asda2DonationItem.LoadAll(Owner))
      {
        if(!DonationItems.ContainsKey(asda2DonationItem.Guid))
          DonationItems.Add(asda2DonationItem.Guid, asda2DonationItem);
      }

      foreach(Asda2FastItemSlotRecord loadFastItemSlot in m_owner
        .Record.GetOrLoadFastItemSlots())
      {
        if(loadFastItemSlot.PanelNum > 5 || loadFastItemSlot.PanelSlot > 11)
          Log.Warn("Bad fastitemslot record {0}", loadFastItemSlot);
        else
          FastItemSlotRecords[loadFastItemSlot.PanelNum][loadFastItemSlot.PanelSlot] =
            loadFastItemSlot;
      }

      ICollection<Asda2ItemRecord> orLoadItems = m_owner.Record.GetOrLoadItems();
      if(orLoadItems == null)
        return;
      List<Asda2Item> asda2ItemList = new List<Asda2Item>(orLoadItems.Count);
      foreach(Asda2ItemRecord record in orLoadItems)
      {
        if(!record.IsAuctioned)
        {
          Asda2ItemTemplate template = Asda2ItemMgr.Templates.Get(record.ItemId);
          if(template == null)
          {
            Log.Warn(
              "Item #{0} on {1} could not be loaded because it had an invalid ItemId: {2} ({3})",
              (object) record.Guid, (object) this, (object) record.ItemId, (object) record.ItemId);
          }
          else
          {
            Asda2Item asda2Item = Asda2Item.CreateItem(record, m_owner, template);
            asda2ItemList.Add(asda2Item);
          }
        }
      }

      foreach(Asda2Item asda2Item in asda2ItemList)
      {
        switch(asda2Item.InventoryType)
        {
          case Asda2InventoryType.Shop:
            if(asda2Item.Slot >= 0 && asda2Item.Slot < ShopItems.Length)
            {
              ShopItems[asda2Item.Slot] = asda2Item;
            }

            continue;
          case Asda2InventoryType.Regular:
            if(asda2Item.Slot >= 0 && asda2Item.Slot < RegularItems.Length)
            {
              RegularItems[asda2Item.Slot] = asda2Item;
            }

            continue;
          case Asda2InventoryType.Equipment:
            SetEquipment(asda2Item, (Asda2EquipmentSlots) asda2Item.Slot);
            continue;
          case Asda2InventoryType.Warehouse:
            if(asda2Item.Slot >= 0 && asda2Item.Slot < WarehouseItems.Length)
            {
              WarehouseItems[asda2Item.Slot] = asda2Item;
            }

            continue;
          case Asda2InventoryType.AvatarWarehouse:
            if(asda2Item.Slot >= 0 && asda2Item.Slot < AvatarWarehouseItems.Length)
            {
              AvatarWarehouseItems[asda2Item.Slot] = asda2Item;
            }

            continue;
          default:
            continue;
        }
      }
    }

    public Asda2InventoryError TrySwap(Asda2InventoryType srcInv, short srcSlot, Asda2InventoryType destInv,
      ref short destSlot)
    {
      Asda2InventoryError status = Asda2InventoryError.Ok;
      Asda2Item asda2Item1 = null;
      Asda2Item asda2Item2 = null;
      if(srcInv == Asda2InventoryType.Equipment)
      {
        destSlot = FindFreeShopItemsSlot();
        if(destSlot == -1)
          status = Asda2InventoryError.NoSpace;
      }

      if(srcInv == Asda2InventoryType.Regular && srcSlot == 0 ||
         destInv == Asda2InventoryType.Regular && destSlot == 0)
        return Asda2InventoryError.Fail;
      if(srcInv != Asda2InventoryType.Shop && srcInv != Asda2InventoryType.Equipment &&
         (srcInv != Asda2InventoryType.Warehouse && srcInv != Asda2InventoryType.AvatarWarehouse) &&
         srcInv != Asda2InventoryType.Regular)
      {
        status = Asda2InventoryError.NotInfoAboutItem;
        Owner.YouAreFuckingCheater("Moving items from wrong inventory.", 50);
      }
      else if(srcInv == Asda2InventoryType.Regular && destInv != Asda2InventoryType.Regular &&
              (destInv == Asda2InventoryType.Shop && destSlot != 10))
      {
        Owner.YouAreFuckingCheater("Moving items from regular to not regular inventory.", 50);
        status = Asda2InventoryError.NotInfoAboutItem;
      }
      else if(srcInv == Asda2InventoryType.Shop && destInv != Asda2InventoryType.Shop &&
              destInv != Asda2InventoryType.Equipment)
      {
        Owner.YouAreFuckingCheater("Moving items from shop to not shop/equipment inventory.", 50);
        status = Asda2InventoryError.NotInfoAboutItem;
      }
      else if(srcInv == Asda2InventoryType.Warehouse && destInv != Asda2InventoryType.Warehouse)
      {
        Owner.YouAreFuckingCheater("Moving items from wh to not wh inventory.", 50);
        status = Asda2InventoryError.NotInfoAboutItem;
      }
      else if(srcInv == Asda2InventoryType.AvatarWarehouse && destInv != Asda2InventoryType.AvatarWarehouse)
      {
        Owner.YouAreFuckingCheater("Moving items from awh to not awh inventory.", 50);
        status = Asda2InventoryError.NotInfoAboutItem;
      }
      else if(srcInv == Asda2InventoryType.Equipment && destInv != Asda2InventoryType.Shop &&
              destInv != Asda2InventoryType.Regular)
      {
        Owner.YouAreFuckingCheater("Moving items from equipment to not shop inventory.", 50);
        status = Asda2InventoryError.NotInfoAboutItem;
      }
      else if(
        srcInv == Asda2InventoryType.Shop && (srcSlot < 0 || srcSlot >= ShopItems.Length) ||
        srcInv == Asda2InventoryType.Regular &&
        (srcSlot < 0 || srcSlot >= RegularItems.Length) ||
        (srcInv == Asda2InventoryType.Equipment &&
         (srcSlot < 0 || srcSlot >= Equipment.Length) ||
         srcInv == Asda2InventoryType.Warehouse &&
         (srcSlot < 0 || srcSlot >= WarehouseItems.Length)) ||
        srcInv == Asda2InventoryType.AvatarWarehouse &&
        (srcSlot < 0 || srcSlot >= AvatarWarehouseItems.Length))
      {
        Owner.YouAreFuckingCheater("Moving items from wrong slot.", 50);
        status = Asda2InventoryError.NotInfoAboutItem;
      }
      else if(
        destInv == Asda2InventoryType.Shop && (destSlot < 0 ||
                                               destSlot >= (Owner.InventoryExpanded
                                                 ? ShopItems.Length
                                                 : 30)) ||
        (destInv == Asda2InventoryType.Regular &&
         (destSlot < 0 || destSlot >= RegularItems.Length) ||
         destInv == Asda2InventoryType.Equipment &&
         (destSlot < 0 || destSlot >= Equipment.Length) ||
         (destInv == Asda2InventoryType.Warehouse &&
          (destSlot < 0 || destSlot >= WarehouseItems.Length) ||
          destInv == Asda2InventoryType.AvatarWarehouse &&
          (destSlot < 0 || destSlot >= AvatarWarehouseItems.Length))))
        status = Asda2InventoryError.NotInfoAboutItem;
      else if(destInv == Asda2InventoryType.Regular && destSlot == 0)
        status = Asda2InventoryError.NotInfoAboutItem;
      else if(srcInv == Asda2InventoryType.Regular && srcSlot == 0)
        status = Asda2InventoryError.NotInfoAboutItem;

      if(status != Asda2InventoryError.Ok && status != Asda2InventoryError.Ok)
      {
        Asda2InventoryHandler.SendItemReplacedResponse(Owner.Client, status, 0, 0, 0,
          0, 0, 0, 0, 0, false);
        return status;
      }

      switch(srcInv)
      {
        case Asda2InventoryType.Shop:
          asda2Item1 = ShopItems[srcSlot];
          break;
        case Asda2InventoryType.Regular:
          asda2Item1 = RegularItems[srcSlot];
          break;
        case Asda2InventoryType.Equipment:
          asda2Item1 = Equipment[srcSlot];
          break;
        case Asda2InventoryType.Warehouse:
          asda2Item1 = WarehouseItems[srcSlot];
          break;
        case Asda2InventoryType.AvatarWarehouse:
          asda2Item1 = AvatarWarehouseItems[srcSlot];
          break;
      }

      if(asda2Item1 == null)
        status = Asda2InventoryError.NotInfoAboutItem;
      else if(!m_owner.CanInteract)
      {
        status = Asda2InventoryError.NotInfoAboutItem;
      }
      else
      {
        switch(destInv)
        {
          case Asda2InventoryType.Shop:
            asda2Item2 = ShopItems[destSlot];
            break;
          case Asda2InventoryType.Regular:
            asda2Item2 = RegularItems[destSlot];
            break;
          case Asda2InventoryType.Equipment:
            asda2Item2 = Equipment[destSlot];
            break;
          case Asda2InventoryType.Warehouse:
            asda2Item2 = WarehouseItems[destSlot];
            break;
          case Asda2InventoryType.AvatarWarehouse:
            asda2Item2 = AvatarWarehouseItems[destSlot];
            break;
        }
      }

      if(status != Asda2InventoryError.Ok)
      {
        Asda2InventoryHandler.SendItemReplacedResponse(Owner.Client, status, 0, 0, 0,
          0, 0, 0, 0, 0, false);
        return status;
      }

      if(destInv == Asda2InventoryType.Equipment && destSlot == 9 &&
         (asda2Item1 != null && asda2Item1.Template.Category != Asda2ItemCategory.OneHandedSword) &&
         Equipment[8] != null)
      {
        Owner.SendInfoMsg("You cant use this item with shield.");
        return Asda2InventoryError.ItemIsNotForEquiping;
      }

      if(destInv == Asda2InventoryType.Equipment && asda2Item1 != null)
      {
        if((asda2Item1.Template.EquipmentSlot != Asda2EquipmentSlots.LeftRing || destSlot != 6) &&
           asda2Item1.Template.EquipmentSlot != (Asda2EquipmentSlots) destSlot)
        {
          Owner.SendInfoMsg("This item is not for equiping.");
          return Asda2InventoryError.ItemIsNotForEquiping;
        }

        if(asda2Item1.RequiredLevel > Owner.Level)
        {
          Owner.SendInfoMsg("Your's level is not enogth.");
          return Asda2InventoryError.Fail;
        }
      }

      if(asda2Item2 != null && srcInv == Asda2InventoryType.Equipment)
      {
        switch(destInv)
        {
          case Asda2InventoryType.Shop:
            asda2Item2 = null;
            short freeShopItemsSlot = FindFreeShopItemsSlot();
            if(freeShopItemsSlot == -1)
            {
              status = Asda2InventoryError.NoSpace;
              break;
            }

            destSlot = freeShopItemsSlot;
            break;
          case Asda2InventoryType.Regular:
            asda2Item2 = null;
            short regularItemsSlot = FindFreeRegularItemsSlot();
            if(regularItemsSlot == -1)
            {
              status = Asda2InventoryError.NoSpace;
              break;
            }

            destSlot = regularItemsSlot;
            break;
        }
      }

      if(status != Asda2InventoryError.Ok)
      {
        Asda2InventoryHandler.SendItemReplacedResponse(Owner.Client, status, 0, 0, 0,
          0, 0, 0, 0, 0, false);
      }
      else
      {
        SwapUnchecked(srcInv, srcSlot, destInv, destSlot);
        WCell.RealmServer.Logs.Log
          .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
            Owner.EntryId).AddAttribute("source", 0.0, "swap")
          .AddAttribute(nameof(srcInv), (double) srcInv, srcInv.ToString())
          .AddAttribute(nameof(destInv), (double) destInv, destInv.ToString())
          .AddAttribute(nameof(srcSlot), srcSlot, "")
          .AddAttribute(nameof(destSlot), destSlot, "").AddItemAttributes(asda2Item1, "srcItem")
          .AddItemAttributes(asda2Item2, "destItem").Write();
        Asda2InventoryHandler.SendItemReplacedResponse(Owner.Client, status, srcSlot, (byte) srcInv,
          asda2Item1 == null ? -1 : asda2Item1.Amount,
          asda2Item1 == null ? (short) 0 : (short) asda2Item1.Weight, destSlot, (byte) destInv,
          asda2Item2 == null ? -1 : asda2Item2.Amount,
          asda2Item2 == null ? (short) 0 : (short) asda2Item1.Weight, asda2Item2 == null);
      }

      return status;
    }

    private void SwapUnchecked(Asda2InventoryType srcInv, short srcSlot, Asda2InventoryType destInv, short destSlot)
    {
      Asda2Item asda2Item1 = null;
      Asda2Item asda2Item2 = null;
      switch(srcInv)
      {
        case Asda2InventoryType.Shop:
          asda2Item1 = ShopItems[srcSlot];
          break;
        case Asda2InventoryType.Regular:
          asda2Item1 = RegularItems[srcSlot];
          break;
        case Asda2InventoryType.Equipment:
          asda2Item1 = Equipment[srcSlot];
          break;
        case Asda2InventoryType.Warehouse:
          asda2Item1 = WarehouseItems[srcSlot];
          break;
        case Asda2InventoryType.AvatarWarehouse:
          asda2Item1 = AvatarWarehouseItems[srcSlot];
          break;
      }

      switch(destInv)
      {
        case Asda2InventoryType.Shop:
          asda2Item2 = ShopItems[destSlot];
          break;
        case Asda2InventoryType.Regular:
          asda2Item2 = RegularItems[destSlot];
          break;
        case Asda2InventoryType.Equipment:
          asda2Item2 = Equipment[destSlot];
          break;
        case Asda2InventoryType.Warehouse:
          asda2Item2 = WarehouseItems[destSlot];
          break;
        case Asda2InventoryType.AvatarWarehouse:
          asda2Item2 = AvatarWarehouseItems[destSlot];
          break;
      }

      switch(srcInv)
      {
        case Asda2InventoryType.Shop:
          SetShopInventoty(asda2Item2, srcSlot, true);
          break;
        case Asda2InventoryType.Regular:
          SetRegularInventoty(asda2Item2, srcSlot, true);
          break;
        case Asda2InventoryType.Equipment:
          SetEquipment(asda2Item2, (Asda2EquipmentSlots) srcSlot);
          break;
        case Asda2InventoryType.Warehouse:
          SetWarehouseInventoty(asda2Item2, srcSlot, true);
          break;
        case Asda2InventoryType.AvatarWarehouse:
          SetAvatarWarehouseInventoty(asda2Item2, srcSlot, true);
          break;
      }

      switch(destInv)
      {
        case Asda2InventoryType.Shop:
          SetShopInventoty(asda2Item1, destSlot, true);
          break;
        case Asda2InventoryType.Regular:
          SetRegularInventoty(asda2Item1, destSlot, true);
          break;
        case Asda2InventoryType.Equipment:
          SetEquipment(asda2Item1, (Asda2EquipmentSlots) destSlot);
          break;
        case Asda2InventoryType.Warehouse:
          SetWarehouseInventoty(asda2Item1, destSlot, true);
          break;
        case Asda2InventoryType.AvatarWarehouse:
          SetAvatarWarehouseInventoty(asda2Item1, destSlot, true);
          break;
      }
    }

    public void RemoveItemFromInventory(Asda2Item asda2Item)
    {
      if(asda2Item.IsDeleted)
        Owner.SendErrorMsg(string.Format(
          "Cant remove deleted item from inventory. inv {0}.slot {1}. itemId {2}",
          asda2Item.InventoryType, asda2Item.Slot, asda2Item.ItemId));
      else if(asda2Item.Slot < 0)
      {
        Owner.SendErrorMsg(string.Format(
          "Cant remove item from inventory with slot < 0. inv {0}.slot {1}. itemId {2}",
          asda2Item.InventoryType, asda2Item.Slot, asda2Item.ItemId));
      }
      else
      {
        switch(asda2Item.InventoryType)
        {
          case Asda2InventoryType.Shop:
            SetShopInventoty(null, asda2Item.Slot, true);
            break;
          case Asda2InventoryType.Regular:
            SetRegularInventoty(null, asda2Item.Slot, true);
            break;
          case Asda2InventoryType.Equipment:
            SetEquipment(null, (Asda2EquipmentSlots) asda2Item.Slot);
            break;
          case Asda2InventoryType.Warehouse:
            SetWarehouseInventoty(null, asda2Item.Slot, true);
            break;
          case Asda2InventoryType.AvatarWarehouse:
            SetAvatarWarehouseInventoty(null, asda2Item.Slot, true);
            break;
        }
      }
    }

    public Asda2InventoryError TryAdd(int itemId, int amount, bool silent, ref Asda2Item item,
      Asda2InventoryType? requiredInventoryType = null, Asda2Item itemToCopyStats = null)
    {
      Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(itemId);
      if(template == null)
      {
        Owner.SendErrorMsg(string.Format("Failed to create and add item {0}. template not founed",
          itemId));
        return Asda2InventoryError.Fail;
      }

      Asda2InventoryType add = CalcIntentoryTypeToAdd(requiredInventoryType, template);
      short freeSlot = FindFreeSlot(add);
      if(freeSlot < 0)
      {
        Owner.SendErrorMsg(string.Format("Failed to create and add item {0}. not enough space",
          itemId));
        return Asda2InventoryError.NoSpace;
      }

      if(template.IsStackable)
      {
        item = FindItem(template, add);
        if(item != null)
        {
          item.Amount += amount;
          return Asda2InventoryError.Ok;
        }
      }

      item = Asda2Item.CreateItem(template, Owner, amount);
      if(itemToCopyStats != null && itemToCopyStats.Record != null)
      {
        item.Enchant = itemToCopyStats.Enchant;
        item.IsSoulbound = itemToCopyStats.IsSoulbound;
        item.Soul1Id = itemToCopyStats.Soul1Id;
        item.Soul2Id = itemToCopyStats.Soul2Id;
        item.Soul3Id = itemToCopyStats.Soul3Id;
        item.Soul4Id = itemToCopyStats.Soul4Id;
        item.Parametr1Type = itemToCopyStats.Parametr1Type;
        item.Parametr1Value = itemToCopyStats.Parametr1Value;
        item.Parametr2Type = itemToCopyStats.Parametr2Type;
        item.Parametr2Value = itemToCopyStats.Parametr2Value;
        item.Parametr3Type = itemToCopyStats.Parametr3Type;
        item.Parametr3Value = itemToCopyStats.Parametr3Value;
        item.Parametr4Type = itemToCopyStats.Parametr4Type;
        item.Parametr4Value = itemToCopyStats.Parametr4Value;
        item.Parametr5Type = itemToCopyStats.Parametr5Type;
        item.Parametr5Value = itemToCopyStats.Parametr5Value;
        item.Durability = itemToCopyStats.Durability;
      }

      SetItem(item, freeSlot, add, true);
      return Asda2InventoryError.Ok;
    }

    private short FindFreeSlot(Asda2InventoryType inventoryType)
    {
      short num;
      switch(inventoryType)
      {
        case Asda2InventoryType.Shop:
          num = FindFreeShopItemsSlot();
          break;
        case Asda2InventoryType.Regular:
          num = FindFreeRegularItemsSlot();
          break;
        case Asda2InventoryType.Warehouse:
          num = FindFreeWarehouseItemsSlot();
          break;
        case Asda2InventoryType.AvatarWarehouse:
          num = FindFreeAvatarWarehouseItemsSlot();
          break;
        default:
          Owner.SendErrorMsg(string.Format("wrong inventory type {0}", inventoryType));
          num = -1;
          break;
      }

      return num;
    }

    private static Asda2InventoryType CalcIntentoryTypeToAdd(Asda2InventoryType? requiredInventoryType,
      Asda2ItemTemplate itemTemplate)
    {
      return requiredInventoryType.HasValue
        ? requiredInventoryType.Value
        : (itemTemplate.IsShopInventoryItem ? Asda2InventoryType.Shop : Asda2InventoryType.Regular);
    }

    public void UseItem(Asda2InventoryType inv, byte slot)
    {
      Asda2Item item;
      switch(inv)
      {
        case Asda2InventoryType.Shop:
          if(slot >= ShopItems.Length)
          {
            Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.Fail, Owner,
              null);
            return;
          }

          item = ShopItems[slot];
          break;
        case Asda2InventoryType.Regular:
          if(slot >= RegularItems.Length)
          {
            Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.Fail, Owner,
              null);
            return;
          }

          item = RegularItems[slot];
          break;
        default:
          Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.Fail, Owner, null);
          return;
      }

      if(item == null)
        Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.Fail, Owner, null);
      else if(item.RequiredLevel > Owner.Level)
        Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.CantUseBacauseOfItemLevel, Owner,
          null);
      else if(!CheckCooldown(item))
        Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.ItemOnCooldown, Owner,
          null);
      else if(Owner.IsDead)
        Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.Fail, Owner, null);
      else if(Owner.IsTrading)
        Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.Fail, Owner, null);
      else
        Owner.AddMessage(() =>
          Asda2InventoryHandler.SendCharUsedItemResponse(UseItemUnchecked(item), Owner, item));
    }

    private UseItemResult UseItemUnchecked(Asda2Item item)
    {
      switch(item.Category)
      {
        case Asda2ItemCategory.SoulStone:
          Owner.SendSystemMessage("Using {0} is not implemented yet.", (object) item.Category);
          break;
        case Asda2ItemCategory.PetResurect:
          return UseItemResult.Fail;
        case Asda2ItemCategory.Incubator:
          Owner.SendSystemMessage("Using {0} is not implemented yet.", (object) item.Category);
          break;
        case Asda2ItemCategory.PetExp:
          if(Owner.Asda2Pet == null)
            return UseItemResult.ThereIsNoActivePet;
          if(!Owner.Asda2Pet.GainXp(item.Template.ValueOnUse / 2))
            return UseItemResult.PetIsMature;
          break;
        case Asda2ItemCategory.ItemPackage:
          Owner.SendSystemMessage("Using {0} is not implemented yet.", (object) item.Category);
          break;
        case Asda2ItemCategory.PartialItem:
          Owner.SendSystemMessage("Using {0} is not implemented yet.", (object) item.Category);
          break;
        case Asda2ItemCategory.Fish:
          Owner.Power += item.Template.ValueOnUse;
          break;
        case Asda2ItemCategory.SoulShard:
          Owner.SendSystemMessage("Using {0} is not implemented yet.", (object) item.Category);
          break;
        case Asda2ItemCategory.FishingBook:
          Owner.SendSystemMessage("Using {0} is not implemented yet.", (object) item.Category);
          break;
        case Asda2ItemCategory.HealthPotion:
          PereodicAction pereodicAction1 = null;
          if(Owner.PereodicActions.ContainsKey(Asda2PereodicActionType.HpRegen))
            pereodicAction1 = Owner.PereodicActions[Asda2PereodicActionType.HpRegen];
          if(pereodicAction1 != null && pereodicAction1.CallsNum >= 10 &&
             pereodicAction1.Value >= item.Template.ValueOnUse)
            return UseItemResult.ItemOnCooldown;
          if(Owner.PereodicActions.ContainsKey(Asda2PereodicActionType.HpRegen))
            Owner.PereodicActions.Remove(Asda2PereodicActionType.HpRegen);
          Owner.PereodicActions.Add(Asda2PereodicActionType.HpRegen,
            new PereodicAction(Owner,
              (int) (item.Template.ValueOnUse *
                     (double) CharacterFormulas.CalcHpPotionBoost(Owner.Asda2Stamina)), 10, 3000,
              Asda2PereodicActionType.HpRegen));
          AchievementProgressRecord progressRecord1 = Owner.Achievements.GetOrCreateProgressRecord(81U);
          switch(++progressRecord1.Counter)
          {
            case 50:
              Owner.DiscoverTitle(Asda2TitleId.Weakling215);
              break;
            case 100:
              Owner.GetTitle(Asda2TitleId.Weakling215);
              break;
          }

          progressRecord1.SaveAndFlush();
          break;
        case Asda2ItemCategory.ManaPotion:
          PereodicAction pereodicAction2 = null;
          if(Owner.PereodicActions.ContainsKey(Asda2PereodicActionType.MpRegen))
            pereodicAction2 = Owner.PereodicActions[Asda2PereodicActionType.MpRegen];
          if(pereodicAction2 != null && pereodicAction2.CallsNum >= 10 &&
             pereodicAction2.Value >= item.Template.ValueOnUse)
            return UseItemResult.ItemOnCooldown;
          if(Owner.PereodicActions.ContainsKey(Asda2PereodicActionType.MpRegen))
            Owner.PereodicActions.Remove(Asda2PereodicActionType.MpRegen);
          Owner.PereodicActions.Add(Asda2PereodicActionType.MpRegen,
            new PereodicAction(Owner, item.Template.ValueOnUse, 10, 3000,
              Asda2PereodicActionType.MpRegen));
          break;
        case Asda2ItemCategory.Recipe:
          Owner.YouAreFuckingCheater("Trying to use recipe in wrong way.", 50);
          return UseItemResult.Fail;
        case Asda2ItemCategory.HealthElixir:
          Owner.Health += (int) (item.Template.ValueOnUse *
                                 (double) CharacterFormulas.CalcHpPotionBoost(Owner.Asda2Stamina));
          break;
        case Asda2ItemCategory.ManaElixir:
          Owner.Power += item.Template.ValueOnUse;
          break;
        case Asda2ItemCategory.ResurectScroll:
          if(!(Owner.Target is Character))
          {
            Owner.SendSystemMessage("Select character to resurect.", (object) item.Category);
            return UseItemResult.Fail;
          }

          Character target = (Character) Owner.Target;
          if(target.IsAlive)
          {
            Owner.SendSystemMessage("Select character is alive and can't be resurected.", (object) item.Category);
            return UseItemResult.Fail;
          }

          AchievementProgressRecord progressRecord2 = Owner.Achievements.GetOrCreateProgressRecord(83U);
          switch(++progressRecord2.Counter)
          {
            case 500:
              Owner.DiscoverTitle(Asda2TitleId.Savior217);
              break;
            case 1000:
              Owner.GetTitle(Asda2TitleId.Savior217);
              break;
          }

          progressRecord2.SaveAndFlush();
          target.Resurrect();
          break;
        case Asda2ItemCategory.ReturnScroll:
          if(Owner.IsInCombat || Owner.IsAsda2BattlegroundInProgress)
            return UseItemResult.Fail;
          Owner.TeleportToBindLocation();
          AchievementProgressRecord progressRecord3 = Owner.Achievements.GetOrCreateProgressRecord(82U);
          switch(++progressRecord3.Counter)
          {
            case 500:
              Owner.DiscoverTitle(Asda2TitleId.Returning216);
              break;
            case 1000:
              Owner.GetTitle(Asda2TitleId.Returning216);
              break;
          }

          progressRecord3.SaveAndFlush();
          break;
        default:
          Owner.SendSystemMessage(string.Format("Item {0} from category {1} can't be used.",
            item.Template.Name, item.Category));
          return UseItemResult.Fail;
      }

      WCell.RealmServer.Logs.Log
        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, Owner.EntryId)
        .AddAttribute("source", 0.0, "use").AddItemAttributes(item, "").Write();
      --item.Amount;
      return UseItemResult.Ok;
    }

    private bool CheckCooldown(Asda2Item item)
    {
      if(!_cooldowns.ContainsKey(item.Template.Category))
      {
        _cooldowns.Add(item.Template.Category, DateTime.Now.AddSeconds(30.0));
      }
      else
      {
        if(_cooldowns[item.Template.Category] > DateTime.Now)
          return false;
        _cooldowns[item.Template.Category] = DateTime.Now.AddSeconds(30.0);
      }

      return true;
    }

    public void RemoveItem(int slot, byte inv, int count)
    {
      if(count == 0)
        count = 1;
      if(inv != 1 && inv != 2 || (slot < 0 || slot >= 70))
      {
        Owner.SendInfoMsg(string.Format("Failed to removeItem from inventory. slot is {0}. inv is {1}",
          slot, inv));
        Asda2InventoryHandler.ItemRemovedFromInventoryResponse(Owner, null,
          DeleteOrSellItemStatus.Fail, 0);
      }
      else
      {
        Asda2Item asda2Item;
        switch(inv)
        {
          case 1:
            if(slot >= ShopItems.Length)
            {
              Asda2InventoryHandler.ItemRemovedFromInventoryResponse(Owner, null,
                DeleteOrSellItemStatus.Fail, 0);
              Owner.SendInfoMsg(string.Format(
                "Failed to removeItem from inventory. slot is {0}. inv is {1}", slot,
                inv));
              return;
            }

            asda2Item = ShopItems[slot];
            break;
          case 2:
            if(slot >= RegularItems.Length)
            {
              Asda2InventoryHandler.ItemRemovedFromInventoryResponse(Owner, null,
                DeleteOrSellItemStatus.Fail, 0);
              Owner.SendInfoMsg(string.Format(
                "Failed to removeItem from inventory. slot is {0}. inv is {1}", slot,
                inv));
              return;
            }

            asda2Item = RegularItems[slot];
            break;
          default:
            Owner.SendInfoMsg(string.Format(
              "Failed to removeItem from inventory. slot is {0}. inv is {1}", slot,
              inv));
            Asda2InventoryHandler.ItemRemovedFromInventoryResponse(Owner, null,
              DeleteOrSellItemStatus.Fail, 0);
            return;
        }

        if(asda2Item == null || asda2Item.ItemId == 20551)
        {
          Asda2InventoryHandler.ItemRemovedFromInventoryResponse(Owner, null,
            DeleteOrSellItemStatus.Fail, 0);
          Owner.SendInfoMsg(string.Format(
            "Failed to removeItem from inventory item not found or money. slot is {0}. inv is {1}",
            slot, inv));
        }
        else
        {
          WCell.RealmServer.Logs.Log
            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
              Owner.EntryId).AddAttribute("source", 0.0, "remove_from_inventory")
            .AddItemAttributes(asda2Item, "").AddAttribute("amount", count, "").Write();
          if(count <= 0)
            asda2Item.Destroy();
          else
            asda2Item.Amount -= asda2Item.Amount < count ? asda2Item.Amount : count;
          Asda2InventoryHandler.ItemRemovedFromInventoryResponse(Owner, asda2Item,
            DeleteOrSellItemStatus.Ok, count);
        }
      }
    }

    public void SellItems(ItemStub[] itemStubs)
    {
      List<Asda2Item> items = new List<Asda2Item>(5);
      foreach(ItemStub itemStub in itemStubs)
      {
        switch(itemStub.Inv)
        {
          case Asda2InventoryType.Shop:
            if(itemStub.Cell >= ShopItems.Length || itemStub.Cell < 0)
            {
              items.Add(null);
              break;
            }

            items.Add(ShopItems[itemStub.Cell]);
            break;
          case Asda2InventoryType.Regular:
            if(itemStub.Cell >= RegularItems.Length || itemStub.Cell < 0)
            {
              items.Add(null);
              break;
            }

            Asda2Item regularItem = RegularItems[itemStub.Cell];
            if(regularItem != null)
              regularItem.CountForNextSell = itemStub.Amount;
            items.Add(regularItem);
            break;
          default:
            items.Add(null);
            break;
        }
      }

      long num1 = 0;
      foreach(Asda2Item asda2Item in items)
      {
        if(asda2Item != null)
        {
          int num2 = !asda2Item.Template.IsStackable
            ? 1
            : (asda2Item.CountForNextSell > 0
              ? (asda2Item.Amount < asda2Item.CountForNextSell
                ? asda2Item.Amount
                : asda2Item.CountForNextSell)
              : asda2Item.Amount);
          float num3 = 1f;
          if(asda2Item.Template.MaxAmount > 1)
            num3 /= asda2Item.Template.MaxAmount;
          int num4 = (int) (asda2Item.Template.SellPrice * num2 *
                            (1.0 + Owner.FloatMods[34]) * num3);
          num1 += num4;
          WCell.RealmServer.Logs.Log
            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
              Owner.EntryId).AddAttribute("source", 0.0, "selling_to_regular_shop")
            .AddItemAttributes(asda2Item, "").AddAttribute("amount_to_sell", num2, "")
            .AddAttribute("gold_earned", num4, "").Write();
          asda2Item.Amount -= num2;
        }
      }

      if(num1 > int.MaxValue || num1 < 0L)
      {
        Owner.YouAreFuckingCheater("Wrong total gold amount while selling items.", 20);
        Asda2InventoryHandler.SendSellItemResponseResponse(DeleteOrSellItemStatus.Fail, Owner, items);
      }
      else
      {
        Owner.AddMoney((uint) num1);
        Asda2InventoryHandler.SendSellItemResponseResponse(DeleteOrSellItemStatus.Ok, Owner, items);
        Owner.SendMoneyUpdate();
      }
    }

    public void BuyItems(List<ItemStub> itemStubs)
    {
      Asda2Item[] items = new Asda2Item[7];
      List<Asda2ItemTemplate> asda2ItemTemplateList = new List<Asda2ItemTemplate>(7);
      foreach(ItemStub itemStub in itemStubs)
      {
        if(itemStub.ItemId == 0)
        {
          asda2ItemTemplateList.Add(null);
        }
        else
        {
          Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(itemStub.ItemId);
          if(template == null || !template.CanBuyInRegularShop)
          {
            Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.BadItemId, Owner, items);
            Owner.YouAreFuckingCheater(
              string.Format("Trying to buy bad item with id {0}.", itemStub.ItemId), 20);
            return;
          }

          if(template.IsStackable && itemStub.Amount <= 0 || !template.IsStackable && itemStub.Amount != 1)
            itemStub.Amount = 1;
          asda2ItemTemplateList.Add(template);
        }
      }

      if(!CheckFreeRegularItemsSlots(asda2ItemTemplateList.Count(
           t =>
           {
             if(t != null)
               return t.InventoryType == (byte) 2;
             return false;
           })) || !CheckShopItemsSlots(asda2ItemTemplateList.Count(
           t =>
           {
             if(t != null)
               return t.InventoryType == (byte) 1;
             return false;
           })))
      {
        Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughSpace, Owner, items);
      }
      else
      {
        long price = CalculatePrice(asda2ItemTemplateList, itemStubs);
        if(price < 0L || price >= int.MaxValue)
        {
          Owner.YouAreFuckingCheater("Wrong price while buying items", 20);
          Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughGold, Owner, items);
        }
        else if(price >= Owner.Money)
        {
          Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughGold, Owner, items);
        }
        else
        {
          for(int index1 = 0; index1 < 7; ++index1)
          {
            if(asda2ItemTemplateList[index1] != null)
            {
              int amount = asda2ItemTemplateList[index1].MaxAmount == 0
                ? itemStubs[index1].Amount
                : asda2ItemTemplateList[index1].MaxAmount;
              Asda2Item asda2Item = null;
              if(asda2ItemTemplateList[index1].IsStackable)
              {
                asda2Item = FindItem(asda2ItemTemplateList[index1], new Asda2InventoryType?());
                if(asda2Item != null)
                {
                  asda2Item.Amount += amount;
                  if(asda2Item.Category == Asda2ItemCategory.HealthPotion)
                  {
                    AchievementProgressRecord progressRecord =
                      Owner.Achievements.GetOrCreateProgressRecord(93U);
                    progressRecord.Counter += (uint) amount;
                    if(progressRecord.Counter >= 1000U)
                      Owner.GetTitle(Asda2TitleId.Stocked226);
                    progressRecord.SaveAndFlush();
                  }
                }
                else
                {
                  asda2Item = Asda2Item.CreateItem(asda2ItemTemplateList[index1], Owner, amount);
                  if(asda2Item.Category == Asda2ItemCategory.HealthPotion)
                  {
                    AchievementProgressRecord progressRecord =
                      Owner.Achievements.GetOrCreateProgressRecord(93U);
                    progressRecord.Counter += (uint) amount;
                    if(progressRecord.Counter >= 1000U)
                      Owner.GetTitle(Asda2TitleId.Stocked226);
                    progressRecord.SaveAndFlush();
                  }

                  WCell.RealmServer.Logs.Log
                    .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations,
                      LogSourceType.Character, Owner.EntryId)
                    .AddAttribute("source", 0.0, "buying_from_regular_shop")
                    .AddItemAttributes(asda2Item, "").Write();
                  if(asda2Item.Template.IsShopInventoryItem)
                  {
                    short freeShopItemsSlot = FindFreeShopItemsSlot();
                    if(freeShopItemsSlot == -1)
                    {
                      Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughSpace,
                        Owner, items);
                      return;
                    }

                    SetShopInventoty(asda2Item, freeShopItemsSlot, true);
                  }
                  else
                  {
                    short regularItemsSlot = FindFreeRegularItemsSlot();
                    if(regularItemsSlot == -1)
                    {
                      Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughSpace,
                        Owner, items);
                      return;
                    }

                    SetRegularInventoty(asda2Item, regularItemsSlot, true);
                  }
                }
              }
              else
              {
                for(int index2 = 0; index2 < amount; ++index2)
                {
                  asda2Item = Asda2Item.CreateItem(asda2ItemTemplateList[index1], Owner, 1);
                  WCell.RealmServer.Logs.Log
                    .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations,
                      LogSourceType.Character, Owner.EntryId)
                    .AddAttribute("source", 0.0, "buying_from_regular_shop")
                    .AddItemAttributes(asda2Item, "").Write();
                  if(asda2Item.Template.IsShopInventoryItem)
                  {
                    short freeShopItemsSlot = FindFreeShopItemsSlot();
                    if(freeShopItemsSlot == -1)
                    {
                      Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughSpace,
                        Owner, items);
                      return;
                    }

                    SetShopInventoty(asda2Item, freeShopItemsSlot, true);
                  }
                  else
                  {
                    short regularItemsSlot = FindFreeRegularItemsSlot();
                    if(regularItemsSlot == -1)
                    {
                      Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughSpace,
                        Owner, items);
                      return;
                    }

                    SetRegularInventoty(asda2Item, regularItemsSlot, true);
                  }
                }
              }

              items[index1] = asda2Item;
            }
          }

          Owner.SubtractMoney((uint) price);
          Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, Owner, items);
          Owner.SendMoneyUpdate();
        }
      }
    }

    private Asda2Item FindItem(int itemId, Asda2InventoryType? requiredInventoryType = null)
    {
      Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(itemId);
      if(template != null)
        return FindItem(template, requiredInventoryType);
      Owner.SendErrorMsg(string.Format("failed ti find item .wrong item id {0}", itemId));
      return null;
    }

    private Asda2Item FindItem(Asda2ItemTemplate asda2ItemTemplate,
      Asda2InventoryType? requiredInventoryType = null)
    {
      Asda2InventoryType add =
        CalcIntentoryTypeToAdd(requiredInventoryType, asda2ItemTemplate);
      switch(add)
      {
        case Asda2InventoryType.Shop:
          return ShopItems.FirstOrDefault(
            i =>
            {
              if(i != null)
                return (Asda2ItemId) i.ItemId == asda2ItemTemplate.ItemId;
              return false;
            });
        case Asda2InventoryType.Regular:
          return RegularItems.FirstOrDefault(
            i =>
            {
              if(i != null)
                return (Asda2ItemId) i.ItemId == asda2ItemTemplate.ItemId;
              return false;
            });
        case Asda2InventoryType.Warehouse:
          return WarehouseItems.FirstOrDefault(
            i =>
            {
              if(i != null)
                return (Asda2ItemId) i.ItemId == asda2ItemTemplate.ItemId;
              return false;
            });
        case Asda2InventoryType.AvatarWarehouse:
          return AvatarWarehouseItems.FirstOrDefault(
            i =>
            {
              if(i != null)
                return (Asda2ItemId) i.ItemId == asda2ItemTemplate.ItemId;
              return false;
            });
        default:
          Owner.SendErrorMsg(
            string.Format("failed ti find item .wrong inventory type {0}", add));
          return null;
      }
    }

    private long CalculatePrice(List<Asda2ItemTemplate> templates, List<ItemStub> itemStubs)
    {
      long num = 0;
      for(int index = 0; index < 7; ++index)
      {
        Asda2ItemTemplate template = templates[index];
        if(template != null)
          num += template.BuyPrice * itemStubs[index].Amount;
      }

      return num;
    }

    private bool CheckShopItemsSlots(int count)
    {
      int num = ShopItems.Count(
        i => i == null);
      if(!Owner.InventoryExpanded)
        num -= 30;
      return num >= count;
    }

    private bool CheckFreeRegularItemsSlots(int count)
    {
      return RegularItems.Count(
               i => i == null) >= count;
    }

    public void TryPickUpItem(short x, short y)
    {
      Asda2LootItem lootItem = Owner.Map.TryPickUpItem(x, y);
      if(lootItem == null)
      {
        Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.Fail, null,
          Owner);
        Owner.Map.AddMessage(
          () => GlobalHandler.SendRemoveItemResponse(Owner.Client, x, y));
      }
      else if(lootItem.Loot.Looters != null && lootItem.Loot.Looters.Count > 0 &&
              (lootItem.Loot.SpawnTime.AddSeconds(CharacterFormulas.ForeignLootPickupTimeout) >
               DateTime.Now &&
               lootItem.Loot.Looters.FirstOrDefault(
                 l => l.Owner == Owner) == null))
      {
        AchievementProgressRecord progressRecord = Owner.Achievements.GetOrCreateProgressRecord(99U);
        switch(++progressRecord.Counter)
        {
          case 500:
            Owner.DiscoverTitle(Asda2TitleId.Bandit232);
            break;
          case 1000:
            Owner.GetTitle(Asda2TitleId.Bandit232);
            break;
        }

        progressRecord.SaveAndFlush();
        Owner.SendInfoMsg("Stealer!!! It's not your loot!");
        Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.Fail, null,
          Owner);
      }
      else
      {
        Owner.Map.ClearLootSlot(x, y);
        Asda2Item asda2Item = null;
        Asda2InventoryError asda2InventoryError = TryAdd((int) lootItem.Template.ItemId, lootItem.Amount,
          true, ref asda2Item, new Asda2InventoryType?(), null);
        if(asda2Item != null)
          WCell.RealmServer.Logs.Log
            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
              Owner.EntryId).AddAttribute("source", 0.0, "from_loot")
            .AddAttribute("mob_id",
              lootItem.Loot.MonstrId.HasValue ? lootItem.Loot.MonstrId.Value : 0.0, "")
            .AddItemAttributes(asda2Item, "").Write();
        if(asda2InventoryError != Asda2InventoryError.Ok)
        {
          Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.NoSpace, null,
            Owner);
        }
        else
        {
          if(asda2Item != null && asda2Item.Template.Quality >= Asda2ItemQuality.Green)
            ChatMgr.SendGlobalMessageResponse(Owner.Name, ChatMgr.Asda2GlobalMessageType.HasObinedItem,
              asda2Item.ItemId, 0, 0);
          switch(++Owner.Achievements.GetOrCreateProgressRecord(100U).Counter)
          {
            case 500:
              Owner.DiscoverTitle(Asda2TitleId.Gatherer233);
              break;
            case 1000:
              Owner.GetTitle(Asda2TitleId.Gatherer233);
              break;
          }

          Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.Ok, asda2Item, Owner);
          Owner.Map.AddMessage(() => GlobalHandler.SendRemoveItemResponse(lootItem));
          if(!lootItem.Loot.IsAllItemsTaken)
            return;
          lootItem.Loot.Dispose();
        }
      }
    }

    public void SowelItem(short itemCell, short sowelCell, byte sowelSlot, short protectSlot, bool isAvatar = false)
    {
      Asda2Item shopItem = ShopItems[itemCell];
      if(shopItem == null)
      {
        Asda2InventoryHandler.SendItemSoweledResponse(Owner.Client, Weight,
          (int) Owner.Money, SowelingStatus.EquipmentError, null, null,
          null, isAvatar);
      }
      else
      {
        if(isAvatar)
        {
          if(shopItem.Enchant < sowelSlot)
          {
            Asda2InventoryHandler.SendItemSoweledResponse(Owner.Client, Weight,
              (int) Owner.Money, SowelingStatus.MaxSocketSlotError, null,
              null, null, true);
            return;
          }
        }
        else if(shopItem.SowelSlots - 1 < sowelSlot)
        {
          Asda2InventoryHandler.SendItemSoweledResponse(Owner.Client, Weight,
            (int) Owner.Money, SowelingStatus.MaxSocketSlotError, null, null,
            null, false);
          return;
        }

        Asda2Item regularItem = RegularItems[sowelCell];
        if(regularItem == null || regularItem.Category != Asda2ItemCategory.Sowel)
          Asda2InventoryHandler.SendItemSoweledResponse(Owner.Client, Weight,
            (int) Owner.Money, SowelingStatus.SowelError, null, null,
            null, isAvatar);
        else if(regularItem.RequiredLevel > Owner.Level)
        {
          Asda2InventoryHandler.SendItemSoweledResponse(Owner.Client, Weight,
            (int) Owner.Money, SowelingStatus.LowLevel, null, null,
            null, isAvatar);
        }
        else
        {
          Asda2Item protect = protectSlot < (short) 0 ? null : ShopItems[protectSlot];
          bool flag1 = protect != null && protect.Category == Asda2ItemCategory.SowelProtectionScroll;
          bool flag2 = SowelItemUnchecked(shopItem, regularItem.ItemId, sowelSlot);
          if(!flag2 && !flag1 || flag2)
          {
            regularItem.Destroy();
            RegularItems[sowelCell] = null;
          }

          LogHelperEntry lgDelete1 = null;
          LogHelperEntry lgDelete2 = WCell.RealmServer.Logs.Log
            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
              Owner.EntryId).AddAttribute("operation", 1.0, "sowel_item_sowel_delete")
            .AddItemAttributes(protect, "").Write();
          if(protect != null && flag1)
          {
            lgDelete1 = WCell.RealmServer.Logs.Log
              .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                Owner.EntryId).AddAttribute("operation", 1.0, "sowel_item_protect_delete")
              .AddItemAttributes(protect, "").Write();
            --protect.Amount;
          }

          WCell.RealmServer.Logs.Log
            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
              Owner.EntryId).AddAttribute("operation", 0.0, "sowel_item")
            .AddItemAttributes(shopItem, "")
            .AddAttribute("success", flag2 ? 1.0 : 0.0, flag2 ? "yes" : "no").AddReference(lgDelete1)
            .AddReference(lgDelete2).Write();
          if(!flag2)
          {
            AchievementProgressRecord progressRecord =
              Owner.Achievements.GetOrCreateProgressRecord(103U);
            switch(++progressRecord.Counter)
            {
              case 50:
                Owner.DiscoverTitle(Asda2TitleId.Misfortune236);
                break;
              case 100:
                Owner.GetTitle(Asda2TitleId.Misfortune236);
                break;
            }

            progressRecord.SaveAndFlush();
          }

          Asda2InventoryHandler.SendItemSoweledResponse(Owner.Client, Weight,
            (int) Owner.Money, flag2 ? SowelingStatus.Ok : SowelingStatus.Fail, shopItem, regularItem,
            protect, isAvatar);
        }
      }
    }

    private bool SowelItemUnchecked(Asda2Item item, int sowelId, byte sowelSlot)
    {
      if(70 <= Utility.Random(0, 100))
        return false;
      switch(sowelSlot)
      {
        case 0:
          item.Soul1Id = sowelId;
          break;
        case 1:
          item.Soul2Id = sowelId;
          break;
        case 2:
          item.Soul3Id = sowelId;
          break;
        case 3:
          item.Soul4Id = sowelId;
          break;
      }

      return true;
    }

    public void ExchangeItemOptions(short scrollCell, short itemSlot)
    {
      if(scrollCell < 0 || scrollCell >= ShopItems.Length)
        Owner.SendInfoMsg("Wrong scroll cell " + scrollCell);
      else if(itemSlot < 0 || itemSlot >= ShopItems.Length)
      {
        Owner.SendInfoMsg("Item scroll cell " + scrollCell);
      }
      else
      {
        Asda2Item shopItem1 = ShopItems[scrollCell];
        if(shopItem1 == null)
        {
          Asda2InventoryHandler.SendExchangeItemOptionResultResponse(Owner.Client,
            ExchangeOptionResult.ScrollInvalid, null, null);
        }
        else
        {
          Asda2Item shopItem2 = ShopItems[itemSlot];
          if(shopItem2 == null || !shopItem2.Template.IsEquipment)
          {
            Asda2InventoryHandler.SendExchangeItemOptionResultResponse(Owner.Client,
              ExchangeOptionResult.ItemInvalid, null, null);
          }
          else
          {
            --shopItem1.Amount;
            LogHelperEntry lgDelete = WCell.RealmServer.Logs.Log
              .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                Owner.EntryId).AddAttribute("operation", 1.0, "exchange_options_scroll_delete")
              .AddItemAttributes(shopItem1, "").Write();
            shopItem2.GenerateNewOptions();
            WCell.RealmServer.Logs.Log
              .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                Owner.EntryId).AddAttribute("operation", 0.0, "exchange_options")
              .AddItemAttributes(shopItem2, "").AddReference(lgDelete).Write();
            Asda2InventoryHandler.SendExchangeItemOptionResultResponse(Owner.Client,
              ExchangeOptionResult.Ok, shopItem2, shopItem1);
          }
        }
      }
    }

    public void UpgradeItem(short itemCell, short stoneCell, short chanceBoostCell, short protectScrollCell)
    {
      Asda2Item shopItem = ShopItems[itemCell];
      Asda2Item regularItem = RegularItems[stoneCell];
      if(shopItem == null || regularItem == null)
        Asda2InventoryHandler.SendUpgradeItemResponse(Owner.Client, UpgradeItemStatus.Fail,
          null, null, null, null, Weight,
          Owner.Money);
      else if(!CalcCanUseThisStone(shopItem, regularItem))
      {
        Asda2InventoryHandler.SendUpgradeItemResponse(Owner.Client, UpgradeItemStatus.Fail,
          null, null, null, null, Weight,
          Owner.Money);
      }
      else
      {
        uint enchantPrice = (uint) Asda2ItemMgr.GetEnchantPrice(shopItem.Enchant, shopItem.RequiredLevel,
          shopItem.Template.Quality);
        if(!Owner.SubtractMoney(enchantPrice))
        {
          Asda2InventoryHandler.SendUpgradeItemResponse(Owner.Client, UpgradeItemStatus.Fail,
            null, null, null, null, Weight,
            Owner.Money);
          Owner.SendInfoMsg("Not enought money to enchant.");
        }
        else
        {
          Asda2Item successItem = protectScrollCell == (short) -1
            ? null
            : ShopItems[protectScrollCell];
          Asda2Item protectionItem = chanceBoostCell == (short) -1
            ? null
            : ShopItems[chanceBoostCell];
          int useChanceBoost =
            successItem == null || successItem.Category != Asda2ItemCategory.IncreaceUpgredeChance
              ? 0
              : successItem.Template.ValueOnUse;
          bool useProtect = false;
          bool noEnchantLose = false;
          if(protectionItem != null)
          {
            if(shopItem.Enchant >= 10)
            {
              switch(protectionItem.Template.ValueOnUse)
              {
                case 1:
                  useProtect = true;
                  break;
                case 2:
                  useProtect = true;
                  noEnchantLose = true;
                  break;
              }
            }
            else if(protectionItem.Template.ValueOnUse == 0)
              useProtect = true;
          }

          ItemUpgradeResult itemUpgradeResult = CharacterFormulas.CalculateItemUpgradeResult(
            regularItem.Template.Quality, shopItem.Template.Quality, shopItem.Enchant,
            shopItem.RequiredLevel, Owner.Asda2Luck, 0, 0, useProtect, useChanceBoost, noEnchantLose);
          Owner.SendSystemMessage(string.Format("{0} with chance {1}(S:{2},P:{3},NC:{4} {5}.",
            (object) itemUpgradeResult.Status, (object) itemUpgradeResult.Chance,
            (object) itemUpgradeResult.BoostFromOwnerLuck, (object) itemUpgradeResult.BoostFormGroupLuck,
            (object) itemUpgradeResult.BoostFromNearbyCharactersLuck,
            useProtect ? (object) "with protection." : (object) "without protection."));
          LogHelperEntry lgDelete1 = WCell.RealmServer.Logs.Log
            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
              Owner.EntryId).AddAttribute("operation", 1.0, "enchant_remove_money")
            .AddAttribute("difference_money", enchantPrice, "")
            .AddAttribute("total_money", Owner.Money, "").Write();
          LogHelperEntry lgDelete2 = WCell.RealmServer.Logs.Log
            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
              Owner.EntryId).AddAttribute("operation", 1.0, "enchant_remove_stone")
            .AddItemAttributes(regularItem, "").Write();
          --regularItem.Amount;
          LogHelperEntry lgDelete3 = null;
          LogHelperEntry lgDelete4 = null;
          if(protectionItem != null)
          {
            lgDelete3 = WCell.RealmServer.Logs.Log
              .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                Owner.EntryId).AddAttribute("operation", 1.0, "enchant_remove_protect")
              .AddItemAttributes(protectionItem, "").Write();
            --protectionItem.Amount;
          }

          if(successItem != null)
          {
            lgDelete4 = WCell.RealmServer.Logs.Log
              .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                Owner.EntryId).AddAttribute("operation", 1.0, "enchant_remove_chance_boost")
              .AddItemAttributes(successItem, "").Write();
            --successItem.Amount;
          }

          switch(itemUpgradeResult.Status)
          {
            case ItemUpgradeResultStatus.Success:
              ++shopItem.Enchant;
              Asda2InventoryHandler.SendUpgradeItemResponse(Owner.Client, UpgradeItemStatus.Ok,
                shopItem, regularItem, successItem, protectionItem, Weight,
                Owner.Money);
              if(shopItem.Enchant >= 10)
                ChatMgr.SendGlobalMessageResponse(Owner.Name,
                  ChatMgr.Asda2GlobalMessageType.HasUpgradeItem, shopItem.ItemId,
                  shopItem.Enchant, 0);
              WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                  Owner.EntryId).AddAttribute("operation", 0.0, "enchant_success")
                .AddAttribute("chance", itemUpgradeResult.Chance, "").AddReference(lgDelete3)
                .AddReference(lgDelete2).AddReference(lgDelete4).AddReference(lgDelete1)
                .AddItemAttributes(shopItem, "").Write();
              break;
            case ItemUpgradeResultStatus.Fail:
              Asda2InventoryHandler.SendUpgradeItemResponse(Owner.Client, UpgradeItemStatus.Fail,
                shopItem, regularItem, successItem, protectionItem, Weight,
                Owner.Money);
              WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                  Owner.EntryId).AddAttribute("operation", 0.0, "enchant_fail")
                .AddAttribute("chance", itemUpgradeResult.Chance, "").AddItemAttributes(shopItem, "")
                .AddReference(lgDelete3).AddReference(lgDelete2).AddReference(lgDelete4)
                .AddReference(lgDelete1).Write();
              break;
            case ItemUpgradeResultStatus.ReduceLevelToZero:
              shopItem.Enchant = 0;
              Asda2InventoryHandler.SendUpgradeItemResponse(Owner.Client, UpgradeItemStatus.Fail,
                shopItem, regularItem, successItem, protectionItem, Weight,
                Owner.Money);
              WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                  Owner.EntryId).AddAttribute("operation", 0.0, "enchant_reduce_to_zero")
                .AddAttribute("chance", itemUpgradeResult.Chance, "").AddReference(lgDelete3)
                .AddReference(lgDelete2).AddReference(lgDelete4).AddReference(lgDelete1)
                .AddItemAttributes(shopItem, "").Write();
              break;
            case ItemUpgradeResultStatus.ReduceOneLevel:
              --shopItem.Enchant;
              Asda2InventoryHandler.SendUpgradeItemResponse(Owner.Client, UpgradeItemStatus.Fail,
                shopItem, regularItem, successItem, protectionItem, Weight,
                Owner.Money);
              WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                  Owner.EntryId).AddAttribute("operation", 0.0, "enchant_reduce_one_level")
                .AddAttribute("chance", itemUpgradeResult.Chance, "").AddReference(lgDelete3)
                .AddReference(lgDelete2).AddReference(lgDelete4).AddReference(lgDelete1)
                .AddItemAttributes(shopItem, "").Write();
              AchievementProgressRecord progressRecord1 =
                Owner.Achievements.GetOrCreateProgressRecord(106U);
              switch(++progressRecord1.Counter)
              {
                case 50:
                  Owner.DiscoverTitle(Asda2TitleId.Cursed258);
                  break;
                case 100:
                  Owner.GetTitle(Asda2TitleId.Cursed258);
                  break;
              }

              progressRecord1.SaveAndFlush();
              break;
            case ItemUpgradeResultStatus.BreakItem:
              WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                  Owner.EntryId).AddAttribute("operation", 0.0, "enchant_break_item")
                .AddAttribute("chance", itemUpgradeResult.Chance, "").AddReference(lgDelete3)
                .AddReference(lgDelete2).AddReference(lgDelete4).AddReference(lgDelete1)
                .AddItemAttributes(shopItem, "").Write();
              AchievementProgressRecord progressRecord2 =
                Owner.Achievements.GetOrCreateProgressRecord(107U);
              switch(++progressRecord2.Counter)
              {
                case 25:
                  Owner.DiscoverTitle(Asda2TitleId.Broken259);
                  break;
                case 50:
                  Owner.GetTitle(Asda2TitleId.Broken259);
                  break;
              }

              progressRecord2.SaveAndFlush();
              shopItem.IsDeleted = true;
              Asda2InventoryHandler.SendUpgradeItemResponse(Owner.Client, UpgradeItemStatus.Fail,
                shopItem, regularItem, successItem, protectionItem, Weight,
                Owner.Money);
              shopItem.IsDeleted = false;
              if(shopItem.Enchant >= 10)
                ChatMgr.SendGlobalMessageResponse(Owner.Name,
                  ChatMgr.Asda2GlobalMessageType.HasUpgradeFail, shopItem.ItemId,
                  shopItem.Enchant, 0);
              shopItem.Destroy();
              break;
          }

          Owner.SendMoneyUpdate();
        }
      }
    }

    private bool CalcCanUseThisStone(Asda2Item item, Asda2Item stone)
    {
      switch(stone.Category)
      {
        case Asda2ItemCategory.EnchantWeaponStoneD:
          if(item.IsWeapon)
            return item.RequiredLevel <= 20;
          return false;
        case Asda2ItemCategory.EnchantWeaponStoneC:
          if(item.IsWeapon)
            return item.RequiredLevel <= 40;
          return false;
        case Asda2ItemCategory.EnchantWeaponStoneB:
          if(item.IsWeapon)
            return item.RequiredLevel <= 60;
          return false;
        case Asda2ItemCategory.EnchantWeaponStoneA:
          if(item.IsWeapon)
            return item.RequiredLevel <= 80;
          return false;
        case Asda2ItemCategory.EnchantWeaponStoneS:
          return item.IsWeapon;
        case Asda2ItemCategory.EnchantArmorStoneD:
          if(item.IsArmor)
            return item.RequiredLevel <= 20;
          return false;
        case Asda2ItemCategory.EnchantArmorStoneC:
          if(item.IsArmor)
            return item.RequiredLevel <= 40;
          return false;
        case Asda2ItemCategory.EnchantArmorStoneB:
          if(item.IsArmor)
            return item.RequiredLevel <= 60;
          return false;
        case Asda2ItemCategory.EnchantArmorStoneA:
          if(item.IsArmor)
            return item.RequiredLevel <= 80;
          return false;
        case Asda2ItemCategory.EnchantArmorStoneS:
          return item.IsArmor;
        case Asda2ItemCategory.EnchantArmorStoneE:
          return item.IsArmor;
        case Asda2ItemCategory.EnchantUniversalStoneE:
          if(!item.IsArmor)
            return item.IsWeapon;
          return true;
        case Asda2ItemCategory.EnchantUniversalStoneD:
          if(item.IsArmor || item.IsWeapon)
            return item.RequiredLevel <= 20;
          return false;
        case Asda2ItemCategory.EnchantUniversalStoneC:
          if(item.IsArmor || item.IsWeapon)
            return item.RequiredLevel <= 40;
          return false;
        case Asda2ItemCategory.EnchantUniversalStoneB:
          if(item.IsArmor || item.IsWeapon)
            return item.RequiredLevel <= 60;
          return false;
        case Asda2ItemCategory.EnchantUniversalStoneA:
          if(item.IsArmor || item.IsWeapon)
            return item.RequiredLevel <= 80;
          return false;
        case Asda2ItemCategory.EnchantUniversalStoneS:
          if(!item.IsArmor)
            return item.IsWeapon;
          return true;
        case Asda2ItemCategory.Enchant100Stone:
          return true;
        default:
          return false;
      }
    }

    public OpenBosterStatus OpenBooster(Asda2InventoryType inv, short cell)
    {
      if(inv != Asda2InventoryType.Regular && inv != Asda2InventoryType.Shop)
        return OpenBosterStatus.Fail;
      Asda2Item asda2Item1 = GetItem(inv, cell);
      if(asda2Item1 == null || asda2Item1.Category != Asda2ItemCategory.Booster)
        return OpenBosterStatus.ItIsNotABooster;
      List<BoosterDrop> boosterDrop1 = Asda2ItemMgr.BoosterDrops[asda2Item1.BoosterId];
      if(boosterDrop1 == null)
        return OpenBosterStatus.BoosterError;
      if(!CheckFreeRegularItemsSlots(1) || !CheckShopItemsSlots(1))
        return OpenBosterStatus.NoSpace;
      Asda2Item addedItem = new Asda2Item();
      BoosterDrop boosterDrop2 = boosterDrop1.Last();
      float num1 = Utility.Random(0.0f, 100f);
      float num2 = 0.0f;
      LogHelperEntry lgDelete = WCell.RealmServer.Logs.Log
        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, Owner.EntryId)
        .AddAttribute("source", 0.0, "open_booster_delete").AddItemAttributes(asda2Item1, "").Write();
      foreach(BoosterDrop boosterDrop3 in boosterDrop1)
      {
        num2 += boosterDrop3.Chance;
        if(boosterDrop2 == boosterDrop3 || num1 <= (double) num2)
        {
          Asda2Item asda2Item2 = null;
          int num3 = (int) TryAdd(boosterDrop3.ItemId, 1, true, ref asda2Item2,
            new Asda2InventoryType?(), null);
          if(asda2Item2 == null)
            return OpenBosterStatus.NoSpace;
          LogHelperEntry logHelperEntry = WCell.RealmServer.Logs.Log
            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
              Owner.EntryId).AddAttribute("source", 0.0, "open_booster_create")
            .AddAttribute("booster_item_id", asda2Item1.ItemId, "")
            .AddItemAttributes(asda2Item2, "");
          logHelperEntry.AddReference(lgDelete);
          logHelperEntry.Write();
          addedItem = asda2Item2;
          if(asda2Item2.Template.Quality >= Asda2ItemQuality.Green)
          {
            ChatMgr.SendGlobalMessageResponse(Owner.Name, ChatMgr.Asda2GlobalMessageType.HasObinedItem,
              boosterDrop3.ItemId, 0, 0);
          }

          break;
        }
      }

      asda2Item1.Destroy();
      if(addedItem.Category == Asda2ItemCategory.Egg)
      {
        AchievementProgressRecord progressRecord = Owner.Achievements.GetOrCreateProgressRecord(164U);
        switch(++progressRecord.Counter)
        {
          case 3:
            Owner.DiscoverTitle(Asda2TitleId.Adopted355);
            break;
          case 5:
            Owner.GetTitle(Asda2TitleId.Adopted355);
            break;
        }

        progressRecord.SaveAndFlush();
      }

      Asda2InventoryHandler.SendbosterOpenedResponse(Owner.Client, OpenBosterStatus.Ok, addedItem, inv, cell,
        Weight);
      return OpenBosterStatus.Ok;
    }

    private Asda2Item GetItem(Asda2InventoryType inv, short cell)
    {
      switch(inv)
      {
        case Asda2InventoryType.Shop:
          if(cell < 0 || cell >= ShopItems.Length)
            return null;
          return ShopItems[cell];
        case Asda2InventoryType.Regular:
          if(cell < 0 || cell >= RegularItems.Length)
            return null;
          return RegularItems[cell];
        case Asda2InventoryType.Equipment:
          if(cell < 0 || cell >= Equipment.Length)
            return null;
          return Equipment[cell];
        case Asda2InventoryType.Warehouse:
          if(cell < 0 || cell >= WarehouseItems.Length)
            return null;
          return WarehouseItems[cell];
        case Asda2InventoryType.AvatarWarehouse:
          if(cell < 0 || cell >= AvatarWarehouseItems.Length)
            return null;
          return AvatarWarehouseItems[cell];
        default:
          return null;
      }
    }

    public OpenPackageStatus OpenPackage(Asda2InventoryType packageInv, short packageSlot)
    {
      if(packageInv != Asda2InventoryType.Regular && packageInv != Asda2InventoryType.Shop)
        return OpenPackageStatus.PackageItemError;
      Asda2Item asda2Item1 = GetItem(packageInv, packageSlot);
      if(asda2Item1 == null || asda2Item1.Category != Asda2ItemCategory.ItemPackage)
        return OpenPackageStatus.PackageItemError;
      List<PackageDrop> packageDrop1 = Asda2ItemMgr.PackageDrops[asda2Item1.PackageId];
      if(packageDrop1 == null)
        return OpenPackageStatus.PackageItemError;
      if(!CheckFreeRegularItemsSlots(packageDrop1.Count) || !CheckShopItemsSlots(packageDrop1.Count))
        return OpenPackageStatus.InfoErrorInEmptyInventry;
      List<Asda2Item> addedItems = new List<Asda2Item>();
      LogHelperEntry lgDelete = WCell.RealmServer.Logs.Log
        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, Owner.EntryId)
        .AddAttribute("source", 0.0, "open_package_delete").AddItemAttributes(asda2Item1, "").Write();
      foreach(PackageDrop packageDrop2 in packageDrop1)
      {
        Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(packageDrop2.ItemId);
        if(template != null)
        {
          Asda2Item asda2Item2 = null;
          int num = (int) TryAdd(packageDrop2.ItemId,
            template.IsStackable
              ? (template.MaxAmount == 0 ? packageDrop2.Amount : template.MaxAmount * packageDrop2.Amount)
              : 1, true, ref asda2Item2, new Asda2InventoryType?(), null);
          if(asda2Item2 == null)
          {
            LogUtil.WarnException("Open package get null item by Try add. Unexpected! {0} {1}",
              (object) Owner.Account.Name, (object) Owner.Name);
            return OpenPackageStatus.InfoErrorInEmptyInventry;
          }

          asda2Item2.IsSoulbound = true;
          LogHelperEntry logHelperEntry = WCell.RealmServer.Logs.Log
            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
              Owner.EntryId).AddAttribute("source", 0.0, "open_package_create")
            .AddAttribute("package_item_id", asda2Item1.ItemId, "")
            .AddItemAttributes(asda2Item2, "");
          logHelperEntry.AddReference(lgDelete);
          logHelperEntry.Write();
          addedItems.Add(asda2Item2);
        }
      }

      asda2Item1.Destroy();
      Asda2InventoryHandler.SendOpenPackageResponseResponse(Owner.Client, OpenPackageStatus.Ok, addedItems,
        packageInv, packageSlot, Weight);
      return OpenPackageStatus.Ok;
    }

    public DisasembleItemStatus DisasembleItem(Asda2InventoryType invNum, short slot)
    {
      if(invNum != Asda2InventoryType.Shop)
        return DisasembleItemStatus.LackOfMaterialForCraft;
      Asda2Item asda2Item1 = GetItem(invNum, slot);
      if(asda2Item1 == null)
        return DisasembleItemStatus.LackOfMaterialForCraft;
      if(!Asda2ItemMgr.DecompositionDrops.ContainsKey(asda2Item1.ItemId))
      {
        Owner.SendSystemMessage(string.Format(
          "Item id {0} can't dissassembled cause need to update dissassemble table. Please report to admin.",
          asda2Item1.ItemId));
        return DisasembleItemStatus.LackOfMaterialForCraft;
      }

      List<DecompositionDrop> decompositionDrop1 = Asda2ItemMgr.DecompositionDrops[asda2Item1.ItemId];
      if(decompositionDrop1 == null)
        return DisasembleItemStatus.LackOfMaterialForCraft;
      if(!CheckFreeRegularItemsSlots(1) || !CheckShopItemsSlots(1))
        return DisasembleItemStatus.NoEmptySlotInThePlate;
      Asda2Item addedItem = new Asda2Item();
      DecompositionDrop decompositionDrop2 = decompositionDrop1.Last();
      foreach(DecompositionDrop decompositionDrop3 in decompositionDrop1)
      {
        if(decompositionDrop2 == decompositionDrop3 ||
           Utility.Random(0.0f, 100f) <= (double) decompositionDrop3.Chance)
        {
          if(Asda2ItemMgr.GetTemplate(decompositionDrop3.ItemId) == null)
            return DisasembleItemStatus.LackOfMaterialForCraft;
          Asda2Item asda2Item2 = null;
          int num = (int) TryAdd(decompositionDrop3.ItemId, 1, true, ref asda2Item2,
            new Asda2InventoryType?(), null);
          if(asda2Item2 == null)
          {
            LogUtil.ErrorException("Dissassemble item get null item by Try add. Unexpected! {0} {1}",
              (object) Owner.Account.Name, (object) Owner.Name);
            return DisasembleItemStatus.CraftingInfoIsInaccurate;
          }

          addedItem = asda2Item2;
          LogHelperEntry logHelperEntry = WCell.RealmServer.Logs.Log
            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
              Owner.EntryId).AddAttribute("source", 0.0, "disassemble_create")
            .AddAttribute("disassemble_item_id", asda2Item1.ItemId, "")
            .AddItemAttributes(asda2Item2, "");
          LogHelperEntry lgDelete = WCell.RealmServer.Logs.Log
            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
              Owner.EntryId).AddAttribute("source", 0.0, "disassemble_delete")
            .AddItemAttributes(asda2Item1, "").Write();
          logHelperEntry.AddReference(lgDelete);
          logHelperEntry.Write();
          break;
        }
      }

      if(asda2Item1.IsSoulbound)
      {
        AchievementProgressRecord progressRecord = Owner.Achievements.GetOrCreateProgressRecord(95U);
        switch(++progressRecord.Counter)
        {
          case 30:
            Owner.DiscoverTitle(Asda2TitleId.Destructive228);
            break;
          case 60:
            Owner.GetTitle(Asda2TitleId.Destructive228);
            break;
        }

        progressRecord.SaveAndFlush();
      }

      asda2Item1.Destroy();
      Asda2InventoryHandler.SendEquipmentDisasembledResponse(Owner.Client, DisasembleItemStatus.Ok,
        Weight, addedItem, slot);
      return DisasembleItemStatus.Ok;
    }

    public BuyFromWarShopStatus BuyItemFromWarshop(int internalWarShopId)
    {
      if(!CheckFreeRegularItemsSlots(1) || !CheckShopItemsSlots(1))
        return BuyFromWarShopStatus.InventoryIsFull;
      WarShopDataRecord warshopDataRecord = Asda2ItemMgr.GetWarshopDataRecord(internalWarShopId);
      if(warshopDataRecord == null)
        return BuyFromWarShopStatus.CantFoundItem;
      Asda2Item moneyItem = FindItem(Asda2ItemMgr.GetTemplate(warshopDataRecord.Money1Type),
        new Asda2InventoryType?());
      if(moneyItem == null)
        return BuyFromWarShopStatus.NotEnoghtExchangeItems;
      LogHelperEntry lgDelete;
      if(moneyItem.ItemId == 20551)
      {
        if(!Owner.SubtractMoney((uint) warshopDataRecord.Cost1))
          return BuyFromWarShopStatus.NonEnoghtGold;
        lgDelete = WCell.RealmServer.Logs.Log
          .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
            Owner.EntryId).AddAttribute("source", 0.0, "buyed_from_war_shop_remove_money")
          .AddAttribute("cost", warshopDataRecord.Cost1, "")
          .AddAttribute("total_money", Owner.Money, "").Write();
      }
      else
      {
        if(moneyItem.Amount < warshopDataRecord.Cost1)
          return BuyFromWarShopStatus.NotEnoghtExchangeItems;
        lgDelete = WCell.RealmServer.Logs.Log
          .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
            Owner.EntryId).AddAttribute("source", 0.0, "buyed_from_war_shop_remove_money_item")
          .AddItemAttributes(moneyItem, "").AddAttribute("cost", warshopDataRecord.Cost1, "")
          .Write();
        moneyItem.Amount -= warshopDataRecord.Cost1;
      }

      Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(warshopDataRecord.ItemId);
      if(template == null)
        return BuyFromWarShopStatus.CantFoundItem;
      Asda2Item buyedItem = null;
      if(TryAdd(warshopDataRecord.ItemId, warshopDataRecord.Amount == 0 ? 1 : warshopDataRecord.Amount,
           true, ref buyedItem, new Asda2InventoryType?(), null) != Asda2InventoryError.Ok)
        return BuyFromWarShopStatus.UnableToPurshace;
      WCell.RealmServer.Logs.Log
        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, Owner.EntryId)
        .AddAttribute("source", 0.0, "buyed_from_war_shop").AddReference(lgDelete)
        .AddItemAttributes(buyedItem, "").Write();
      Asda2InventoryHandler.SendItemFromWarshopBuyedResponse(Owner.Client, BuyFromWarShopStatus.Ok,
        Weight, (int) Owner.Money, moneyItem, buyedItem);
      World.BroadcastMsg("Donation shop",
        string.Format("Thanks to {0} for buying {1}[{2}] and helping server!", Owner.Name,
          template.Name, template.Id), Color.PaleGreen);
      Owner.SendMoneyUpdate();
      return BuyFromWarShopStatus.Ok;
    }

    public bool UseGlobalChatItem()
    {
      Asda2Item globalChatItem = ShopItems.FirstOrDefault(
        i =>
        {
          if(i != null)
            return i.Category == Asda2ItemCategory.GlobalChat;
          return false;
        });
      if(globalChatItem == null)
      {
        Owner.SendSystemMessage("You must have global chat item to use this chat.");
        return false;
      }

      WCell.RealmServer.Logs.Log
        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, Owner.EntryId)
        .AddAttribute("source", 0.0, "use_global_chat_item").AddItemAttributes(globalChatItem, "").Write();
      --globalChatItem.Amount;
      ChatMgr.SendGlobalChatRemoveItemResponse(Owner.Client, true, globalChatItem);
      return true;
    }

    public bool UseTeleportScroll(bool somming = false)
    {
      if(!somming)
      {
        Asda2Item asda2Item = ShopItems.FirstOrDefault(
          i =>
          {
            if(i != null)
              return i.Category == Asda2ItemCategory.TeleportToCharacter;
            return false;
          });
        if(asda2Item == null)
          return false;
        WCell.RealmServer.Logs.Log
          .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
            Owner.EntryId).AddAttribute("source", 0.0, "use_teleport_scroll_item")
          .AddItemAttributes(asda2Item, "").Write();
        --asda2Item.Amount;
        Asda2InventoryHandler.UpdateItemInventoryInfo(Owner.Client, asda2Item);
      }

      if(somming)
      {
        Asda2Item asda2Item = ShopItems.FirstOrDefault(
          i =>
          {
            if(i != null)
              return i.Category == Asda2ItemCategory.SummonCharacterToYou;
            return false;
          });
        if(asda2Item == null)
          return false;
        WCell.RealmServer.Logs.Log
          .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
            Owner.EntryId).AddAttribute("source", 0.0, "use_summon_scroll_item")
          .AddItemAttributes(asda2Item, "").Write();
        --asda2Item.Amount;
        Asda2InventoryHandler.UpdateItemInventoryInfo(Owner.Client, asda2Item);
      }

      return true;
    }

    public void AuctionItem(Asda2ItemTradeRef itemRef)
    {
      if(!itemRef.Item.Template.IsStackable)
        itemRef.Amount = itemRef.Item.Amount;
      Asda2Item asda2Item = itemRef.Item;
      itemRef.Item = Asda2Item.CreateItem(itemRef.Item.ItemId, itemRef.Item.OwningCharacter, itemRef.Amount);
      asda2Item.Amount -= itemRef.Amount;
      itemRef.Item.Slot = asda2Item.Slot;
      itemRef.Item.InventoryType = asda2Item.InventoryType;
      LogHelperEntry removeLog = WCell.RealmServer.Logs.Log
        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, Owner.EntryId)
        .AddAttribute("source", 0.0, "auctioning_item_left").AddAttribute("amount", itemRef.Amount, "")
        .AddItemAttributes(asda2Item, "").Write();
      AddToAuction(itemRef, removeLog);
    }

    private void AddToAuction(Asda2ItemTradeRef itemRef, LogHelperEntry removeLog)
    {
      uint amount = (uint) (CharacterFormulas.AuctionPushComission * (double) itemRef.Price);
      if(!Owner.SubtractMoney(amount))
      {
        Owner.YouAreFuckingCheater("Auctioning item without money", 100);
        throw new InvalidOperationException("unexpected behavior");
      }

      itemRef.Item.AuctionPrice = itemRef.Price;
      Asda2AuctionMgr.RegisterItem(itemRef.Item.Record);
      WCell.RealmServer.Logs.Log
        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, Owner.EntryId)
        .AddAttribute("source", 0.0, "auctioning_item").AddAttribute("commission", amount, "")
        .AddAttribute("price", itemRef.Price, "").AddItemAttributes(itemRef.Item, "")
        .AddAttribute("tolal_money", Owner.Money, "").AddReference(removeLog).Write();
      itemRef.Item.Save();
      Owner.SendAuctionMsg(string.Format("[Reg] {0} for {1} gold. [Cms] {2} gold.",
        itemRef.Item.Template.Name, itemRef.Price, amount));
    }

    public void LearnRecipe(short slot)
    {
      if(slot < 1 || slot >= RegularItems.Length)
      {
        Asda2CraftingHandler.SendRecipeLeadnedResponse(Owner.Client, false, 0, null);
        Owner.YouAreFuckingCheater("Trying to learn not existing recipe.Bad SLOT.", 50);
      }
      else
      {
        Asda2Item regularItem = RegularItems[slot];
        if(regularItem == null)
        {
          Asda2CraftingHandler.SendRecipeLeadnedResponse(Owner.Client, false, 0,
            null);
          Owner.YouAreFuckingCheater("Trying to learn not existing recipe.", 1);
        }
        else if(regularItem.Category != Asda2ItemCategory.Recipe)
        {
          Asda2CraftingHandler.SendRecipeLeadnedResponse(Owner.Client, false, 0,
            null);
          Owner.YouAreFuckingCheater("Trying to learn not recipe item.", 50);
        }
        else
        {
          int valueOnUse = regularItem.Template.ValueOnUse;
          Asda2RecipeTemplate recipeTemplate = Asda2CraftMgr.GetRecipeTemplate(valueOnUse);
          Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(recipeTemplate.ResultItemIds[0]);
          if(recipeTemplate == null)
          {
            Asda2CraftingHandler.SendRecipeLeadnedResponse(Owner.Client, false, 0,
              null);
            Owner.SendCraftingMsg("Can't find recipe info. Recipe id is " + valueOnUse);
          }
          else if(Owner.Record.CraftingLevel < recipeTemplate.CraftingLevel)
          {
            Asda2CraftingHandler.SendRecipeLeadnedResponse(Owner.Client, false, 0,
              null);
            Owner.SendCraftingMsg("Trying to learn recipe with level higher than you have.");
          }
          else
          {
            try
            {
              if(Owner.LearnedRecipes.GetBit(valueOnUse))
              {
                Asda2CraftingHandler.SendRecipeLeadnedResponse(Owner.Client, false, 0,
                  null);
                Owner.SendCraftingMsg("Recipe already learned.");
                return;
              }
            }
            catch(Exception ex)
            {
              Asda2CraftingHandler.SendRecipeLeadnedResponse(Owner.Client, false, 0,
                null);
              Owner.SendCraftingMsg("Wrond recipe id " + valueOnUse);
              return;
            }

            Owner.LearnedRecipes.SetBit(valueOnUse);
            ++Owner.LearnedRecipesCount;
            if(template.IsArmor || template.IsWeapon)
            {
              AchievementProgressRecord progressRecord =
                Owner.Achievements.GetOrCreateProgressRecord(109U);
              switch(++progressRecord.Counter)
              {
                case 50:
                  Owner.DiscoverTitle(Asda2TitleId.Blacksmith268);
                  break;
                case 100:
                  Owner.GetTitle(Asda2TitleId.Blacksmith268);
                  break;
              }

              progressRecord.SaveAndFlush();
            }

            if(template.Category == Asda2ItemCategory.HealthPotion ||
               template.Category == Asda2ItemCategory.ManaPotion ||
               (template.Category == Asda2ItemCategory.HealthElixir ||
                template.Category == Asda2ItemCategory.ManaElixir))
            {
              AchievementProgressRecord progressRecord =
                Owner.Achievements.GetOrCreateProgressRecord(110U);
              switch(++progressRecord.Counter)
              {
                case 3:
                  Owner.DiscoverTitle(Asda2TitleId.Alchemist269);
                  break;
                case 5:
                  Owner.GetTitle(Asda2TitleId.Alchemist269);
                  break;
              }

              progressRecord.SaveAndFlush();
            }

            WCell.RealmServer.Logs.Log
              .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                Owner.EntryId).AddAttribute("source", 0.0, "learn_recipe")
              .AddItemAttributes(regularItem, "").Write();
            --regularItem.Amount;
            Asda2CraftingHandler.SendRecipeLeadnedResponse(Owner.Client, true, (short) valueOnUse,
              regularItem);
          }
        }
      }
    }

    public Asda2Item FindRegularItem(int requredItemId)
    {
      return RegularItems.FirstOrDefault(i =>
      {
        if(i != null)
          return i.ItemId == requredItemId;
        return false;
      });
    }

    public Asda2Item GetRegularItem(short slotInq)
    {
      if(slotInq < 0 || slotInq >= RegularItems.Length)
        return null;
      return RegularItems[slotInq];
    }

    public Asda2Item GetShopShopItem(short slotInq)
    {
      if(slotInq < 0 || slotInq >= ShopItems.Length)
        return null;
      return ShopItems[slotInq];
    }

    public Asda2Item GetWarehouseItem(short slotInq)
    {
      if(slotInq < 0 || slotInq >= WarehouseItems.Length)
        return null;
      return WarehouseItems[slotInq];
    }

    public Asda2Item GetAvatarWarehouseItem(short slotInq)
    {
      if(slotInq < 0 || slotInq >= AvatarWarehouseItems.Length)
        return null;
      return AvatarWarehouseItems[slotInq];
    }

    public HatchEggStatus HatchEgg(short slotInq, short slotEgg, short slotSupl)
    {
      if(slotInq < 0 || slotEgg < 0 ||
         (slotInq > RegularItems.Length || slotEgg > RegularItems.Length) ||
         slotSupl > ShopItems.Length)
      {
        Owner.YouAreFuckingCheater("Sending wrong inventory info when hatching egg.", 50);
        return HatchEggStatus.Fail;
      }

      Asda2Item regularItem1 = RegularItems[slotInq];
      Asda2Item regularItem2 = RegularItems[slotEgg];
      Asda2Item asda2Item = slotSupl < (short) 0 ? null : ShopItems[slotSupl];
      if(regularItem1 == null || regularItem2 == null)
      {
        Owner.YouAreFuckingCheater("Egg or iqubator not exist when hatching egg.", 1);
        return HatchEggStatus.Fail;
      }

      if(regularItem1.Category != Asda2ItemCategory.Incubator)
      {
        Owner.YouAreFuckingCheater(
          string.Format("Trying to use {0} as incubator :)", regularItem1.Name), 50);
        return HatchEggStatus.Fail;
      }

      if(regularItem2.Category != Asda2ItemCategory.Egg)
      {
        Owner.YouAreFuckingCheater(
          string.Format("Trying to use {0} as egg :)", regularItem2.Name), 50);
        return HatchEggStatus.Fail;
      }

      if(regularItem2.RequiredLevel > Owner.Level)
      {
        Owner.YouAreFuckingCheater(
          string.Format("Trying to hatch egg with required level {0} that higher than his level {1} :)",
            regularItem2.RequiredLevel, Owner.Level), 50);
        return HatchEggStatus.Fail;
      }

      if(Owner.OwnedPets.Count >= 6 + 6 * Owner.Record.PetBoxEnchants)
      {
        Owner.SendInfoMsg("You already have max pet count.");
        return HatchEggStatus.Fail;
      }

      bool flag = regularItem1.Template.ValueOnUse + (asda2Item == null ? 0 : 30000) >= Utility.Random(0, 100000);
      WCell.RealmServer.Logs.Log
        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, Owner.EntryId)
        .AddAttribute("source", 0.0, "learn_recipe").AddItemAttributes(regularItem1, "inqubator")
        .AddItemAttributes(regularItem2, "egg").AddItemAttributes(asda2Item, "supl")
        .AddAttribute("success", flag ? 1.0 : 0.0, flag ? "yes" : "no").Write();
      regularItem1.Destroy();
      regularItem2.Destroy();
      if(asda2Item != null)
        asda2Item.ModAmount(-1);
      if(!flag)
        return HatchEggStatus.PetHatchingFailed;
      PetTemplate petTemplate = Asda2PetMgr.PetTemplates.Get(regularItem2.Template.ValueOnUse);
      if(petTemplate == null)
      {
        Owner.YouAreFuckingCheater(
          string.Format("Error on hatching egg {0} cant find template {1}.", regularItem2,
            regularItem2.Template.ValueOnUse), 0);
        return HatchEggStatus.NoEgg;
      }

      AchievementProgressRecord progressRecord1 = Owner.Achievements.GetOrCreateProgressRecord(165U);
      switch(++progressRecord1.Counter)
      {
        case 1:
          Owner.Client.ActiveCharacter.Map.CallDelayed(500,
            () => Owner.GetTitle(Asda2TitleId.Pet356));
          break;
        case 5:
          Owner.Client.ActiveCharacter.Map.CallDelayed(500,
            () => Owner.DiscoverTitle(Asda2TitleId.Farm357));
          break;
        case 10:
          Owner.Client.ActiveCharacter.Map.CallDelayed(500,
            () => Owner.GetTitle(Asda2TitleId.Farm357));
          break;
        case 17:
          Owner.Client.ActiveCharacter.Map.CallDelayed(500,
            () => Owner.DiscoverTitle(Asda2TitleId.Zoo358));
          break;
        case 25:
          Owner.Client.ActiveCharacter.Map.CallDelayed(500,
            () => Owner.GetTitle(Asda2TitleId.Zoo358));
          break;
      }

      progressRecord1.SaveAndFlush();
      if(petTemplate.Rarity == 2)
      {
        AchievementProgressRecord progressRecord2 = Owner.Achievements.GetOrCreateProgressRecord(166U);
        switch(++progressRecord2.Counter)
        {
          case 10:
            Owner.Client.ActiveCharacter.Map.CallDelayed(500,
              () => Owner.GetTitle(Asda2TitleId.Exotic362));
            break;
          case 20:
            Owner.Client.ActiveCharacter.Map.CallDelayed(500,
              () => Owner.DiscoverTitle(Asda2TitleId.Exotic362));
            break;
        }

        progressRecord2.SaveAndFlush();
      }

      if(petTemplate.Id == 1 || petTemplate.Id == 3 || (petTemplate.Id == 7 || petTemplate.Id == 11) ||
         (petTemplate.Id == 13 || petTemplate.Id == 17 || (petTemplate.Id == 21 || petTemplate.Id == 23)) ||
         petTemplate.Id == 27)
        Owner.Client.ActiveCharacter.Map.CallDelayed(500,
          () => Owner.GetTitle(Asda2TitleId.Beast359));
      if(petTemplate.Id == 2 || petTemplate.Id == 5 || (petTemplate.Id == 6 || petTemplate.Id == 12) ||
         (petTemplate.Id == 15 || petTemplate.Id == 16 || (petTemplate.Id == 22 || petTemplate.Id == 25)) ||
         petTemplate.Id == 26)
        Owner.Client.ActiveCharacter.Map.CallDelayed(500,
          () => Owner.GetTitle(Asda2TitleId.Vegetable360));
      if(petTemplate.Id == 4 || petTemplate.Id == 8 || (petTemplate.Id == 32 || petTemplate.Id == 14) ||
         (petTemplate.Id == 18 || petTemplate.Id == 34 || (petTemplate.Id == 24 || petTemplate.Id == 28)) ||
         petTemplate.Id == 36)
        Owner.Client.ActiveCharacter.Map.CallDelayed(500,
          () => Owner.GetTitle(Asda2TitleId.Machine361));
      Owner.AddAsda2Pet(petTemplate, false);
      return HatchEggStatus.Ok;
    }

    public void OnDeath()
    {
      foreach(Asda2Item asda2Item in Equipment)
      {
        if(asda2Item != null)
          asda2Item.DecreaseDurability((byte) (asda2Item.MaxDurability / 10U), false);
      }
    }

    public Asda2DonationItem AddDonateItem(Asda2ItemTemplate templ, int amount, string initializer,
      bool isSoulBound = false)
    {
      Asda2DonationItem asda2DonationItem = new Asda2DonationItem(Owner.EntityId.Low, (int) templ.Id, amount,
        initializer, isSoulBound);
      asda2DonationItem.Create();
      Owner.Asda2Inventory.DonationItems.Add(asda2DonationItem.Guid, asda2DonationItem);
      Asda2InventoryHandler.SendSomeNewItemRecivedResponse(Owner.Client, asda2DonationItem.ItemId,
        102);
      return asda2DonationItem;
    }

    public void DropItems(List<Asda2Item> itemsToDrop)
    {
      Asda2NPCLoot loot = new Asda2NPCLoot();
      loot.Items = itemsToDrop.Select(asda2Item =>
        new Asda2LootItem(asda2Item.Template, 1, 0U)
        {
          Loot = (Asda2Loot) loot
        }).ToArray();
      loot.Lootable = Owner;
      loot.MonstrId = 22222;
      Owner.Map.SpawnLoot(loot);
      foreach(Asda2Item asda2Item in itemsToDrop)
      {
        switch(asda2Item.InventoryType)
        {
          case Asda2InventoryType.Shop:
            if(asda2Item.IsWeapon || asda2Item.IsArmor ||
               asda2Item.Category == Asda2ItemCategory.ItemPackage)
            {
              Asda2InventoryHandler.SendItemReplacedResponse(Owner.Client, Asda2InventoryError.Ok,
                60, 1, -1, 0, asda2Item.Slot,
                (byte) asda2Item.InventoryType, asda2Item.Amount, 0, false);
              SwapUnchecked(asda2Item.InventoryType, asda2Item.Slot, Asda2InventoryType.Shop,
                60);
              RemoveItem(60, 1, asda2Item.Amount);
            }

            continue;
          case Asda2InventoryType.Regular:
            Asda2InventoryHandler.SendItemReplacedResponse(Owner.Client, Asda2InventoryError.Ok,
              60, 1, -1, 0, asda2Item.Slot, (byte) asda2Item.InventoryType,
              asda2Item.Amount, 0, false);
            SwapUnchecked(asda2Item.InventoryType, asda2Item.Slot, Asda2InventoryType.Shop,
              60);
            RemoveItem(60, 1, asda2Item.Amount);
            continue;
          case Asda2InventoryType.Equipment:
            Asda2InventoryHandler.SendItemReplacedResponse(Owner.Client, Asda2InventoryError.Ok,
              60, 1, -1, 0, asda2Item.Slot, (byte) asda2Item.InventoryType,
              asda2Item.Amount, 0, false);
            SwapUnchecked(asda2Item.InventoryType, asda2Item.Slot, Asda2InventoryType.Shop,
              60);
            RemoveItem(60, 1, asda2Item.Amount);
            continue;
          default:
            continue;
        }
      }
    }

    public void FillOnCharacterCreate()
    {
      SetEquipment(Asda2Item.CreateItem(21498, Owner, 1), Asda2EquipmentSlots.Weapon);
      SetRegularInventoty(Asda2Item.CreateItem(20551, Owner, 1), 0, true);
      SetRegularInventoty(Asda2Item.CreateItem(20572, Owner, 30), 1, true);
      SetRegularInventoty(Asda2Item.CreateItem(20583, Owner, 10), 2, true);
      SetRegularInventoty(Asda2Item.CreateItem(31820, Owner, 1), 3, true);
      SetRegularInventoty(Asda2Item.CreateItem(32314, Owner, 20), 4, true);
      SetShopInventoty(Asda2Item.CreateItem(21499, Owner, 1), 0, true);
      SetShopInventoty(Asda2Item.CreateItem(20615, Owner, 1), 1, true);
      SetShopInventoty(Asda2Item.CreateItem(33527, Owner, 1), 2, true);
      SetShopInventoty(Asda2Item.CreateItem(26, Owner, 5), 4, true);
    }

    public void CombineItems(short comtinationId)
    {
      ItemCombineDataRecord itemCombineRecord = Asda2ItemMgr.ItemCombineRecords[comtinationId];
      if(itemCombineRecord == null)
        Owner.SendInfoMsg(string.Format("Can't combine items cause record №{0} not founded.",
          comtinationId));
      else if(FreeRegularSlotsCount < 1 || FreeShopSlotsCount < 1)
      {
        Owner.SendInfoMsg("Not enought space.");
      }
      else
      {
        List<Asda2Item> usedItems = new List<Asda2Item>();
        for(int index = 0; index < 5; ++index)
        {
          int requiredItem = itemCombineRecord.RequiredItems[index];
          if(requiredItem != -1)
          {
            Asda2Item asda2Item = FindItem(requiredItem,
              Asda2InventoryType.Regular);
            int amount = itemCombineRecord.Amounts[index];
            if(asda2Item == null || asda2Item.Amount < amount)
            {
              Owner.SendInfoMsg(string.Format(
                "Can't combine items cause not enought resources. Item Id {0} amount {1}.",
                requiredItem, amount));
              return;
            }

            usedItems.Add(asda2Item);
          }
          else
            break;
        }

        for(int index = 0; index < usedItems.Count; ++index)
        {
          Asda2Item asda2Item = usedItems[index];
          if(asda2Item.Amount - itemCombineRecord.Amounts[index] <= 0)
            asda2Item.IsDeleted = true;
          else
            asda2Item.Amount -= itemCombineRecord.Amounts[index];
        }

        Asda2Item resultItem = null;
        int num1 = (int) TryAdd(itemCombineRecord.ResultItem, 1, true, ref resultItem,
          new Asda2InventoryType?(), null);
        LogHelperEntry logHelperEntry = WCell.RealmServer.Logs.Log
          .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
            Owner.EntryId).AddAttribute("source", 0.0, "combine_items")
          .AddItemAttributes(resultItem, "result").Write();
        int num2 = 0;
        foreach(Asda2Item asda2Item in usedItems)
          logHelperEntry.AddItemAttributes(asda2Item, "resource_item_" + num2++);
        if(resultItem.ItemId >= 31547 && 31606 <= resultItem.ItemId)
        {
          AchievementProgressRecord progressRecord = Owner.Achievements.GetOrCreateProgressRecord(102U);
          switch(++progressRecord.Counter)
          {
            case 7:
              Owner.DiscoverTitle(Asda2TitleId.Zodiac235);
              break;
            case 15:
              Owner.GetTitle(Asda2TitleId.Zodiac235);
              break;
          }

          progressRecord.SaveAndFlush();
        }

        Asda2InventoryHandler.SendItemCombinedResponse(Owner.Client, resultItem, usedItems);
      }
    }

    public Asda2Item TryCraftItem(short recId, out List<Asda2Item> materials)
    {
      materials = new List<Asda2Item>();
      if(!Owner.LearnedRecipes.GetBit(recId))
      {
        Owner.SendErrorMsg("Trying craft not learned recipe. " + recId);
        return null;
      }

      Asda2RecipeTemplate recipeTemplate = Asda2CraftMgr.GetRecipeTemplate(recId);
      if(recipeTemplate == null)
      {
        Owner.SendErrorMsg("Can't find recipe template. " + recId);
        return null;
      }

      if(FreeRegularSlotsCount < 1 || FreeRegularSlotsCount < 1)
      {
        Owner.SendCraftingMsg("Not enought space.");
        return null;
      }

      int index = 0;
      foreach(int requredItemId in recipeTemplate.RequredItemIds)
      {
        if(requredItemId != -1)
        {
          Asda2Item asda2Item =
            FindItem(requredItemId, Asda2InventoryType.Regular);
          if(asda2Item == null || asda2Item.Amount < recipeTemplate.ReqiredItemAmounts[index])
          {
            Owner.SendErrorMsg("Not enought materials to craft.");
            return null;
          }

          materials.Add(asda2Item);
          asda2Item.Amount -= recipeTemplate.ReqiredItemAmounts[index];
          ++index;
        }
        else
          break;
      }

      byte num1 = CharacterFormulas.GetCraftedRarity();
      if(num1 == 0)
      {
        while(num1 == 0)
          num1 = CharacterFormulas.GetCraftedRarity();
      }

      if(num1 > recipeTemplate.MaximumPosibleRarity)
        num1 = recipeTemplate.MaximumPosibleRarity;
      int resultItemId = recipeTemplate.ResultItemIds[num1 - 1];
      short resultItemAmount = recipeTemplate.ResultItemAmounts[num1 - 1];
      if(resultItemAmount <= 0)
      {
        Owner.SendErrorMsg("Crafted amount error.");
        return null;
      }

      Asda2Item asda2Item1 = null;
      if(TryAdd(resultItemId, resultItemAmount, false, ref asda2Item1, new Asda2InventoryType?(),
           null) != Asda2InventoryError.Ok)
      {
        Owner.SendErrorMsg("Cant add crafted item.");
        return null;
      }

      WCell.RealmServer.Logs.Log
        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, Owner.EntryId)
        .AddAttribute("source", 0.0, "craft_create").AddItemAttributes(asda2Item1, "")
        .AddAttribute("recipe_id", recipeTemplate.Id, "").Write();
      int diffLvl = Owner.Record.CraftingLevel - recipeTemplate.CraftingLevel;
      float num2 = CharacterFormulas.CalcCraftingExp(diffLvl, Owner.Record.CraftingLevel);
      if(diffLvl > 0)
        Owner.GuildPoints += CharacterFormulas.CraftingGuildPointsPerLevel * diffLvl;
      Owner.Record.CraftingExp += num2;
      if(Owner.Record.CraftingExp >= 100.0)
      {
        ++Owner.Record.CraftingLevel;
        Owner.Record.CraftingExp = 0.0f;
        if(Owner.Record.CraftingLevel == 2)
          Owner.GetTitle(Asda2TitleId.Apprentice265);
        if(Owner.Record.CraftingLevel == 5)
          Owner.GetTitle(Asda2TitleId.Master266);
      }

      Owner.GainXp(
        CharacterFormulas.CalcExpForCrafting(diffLvl, Owner.Record.CraftingLevel, (byte) Owner.Level),
        "craft", false);
      asda2Item1.Record.IsCrafted = true;
      asda2Item1.GenerateOptionsByCraft();
      return asda2Item1;
    }

    public void PushItemsToWh(IEnumerable<Asda2WhItemStub> itemStubs)
    {
      if(!IsItemsExists(itemStubs) || !IsInventorySpaceEnough(itemStubs, false, false))
      {
        Asda2InventoryHandler.SendItemsPushedToWarehouseResponse(Owner.Client,
          PushItemToWhStatus.ItemNotFounded, null,
          null);
      }
      else
      {
        List<Asda2WhItemStub> asda2WhItemStubList1 = new List<Asda2WhItemStub>();
        List<Asda2WhItemStub> asda2WhItemStubList2 = new List<Asda2WhItemStub>();
        foreach(Asda2WhItemStub itemStub in itemStubs)
        {
          Asda2Item itemToCopyStats = GetItem(itemStub.Invtentory, itemStub.Slot);
          Asda2Item asda2Item = null;
          int num = (int) TryAdd(itemToCopyStats.ItemId, itemStub.Amount, true, ref asda2Item,
            Asda2InventoryType.Warehouse, itemToCopyStats);
          itemToCopyStats.Amount -= itemStub.Amount;
          asda2WhItemStubList1.Add(new Asda2WhItemStub
          {
            Amount = itemToCopyStats.Amount,
            Invtentory = itemToCopyStats.InventoryType,
            Slot = itemToCopyStats.Slot
          });
          asda2WhItemStubList2.Add(new Asda2WhItemStub
          {
            Amount = asda2Item.Amount,
            Invtentory = asda2Item.InventoryType,
            Slot = asda2Item.Slot
          });
        }

        Asda2InventoryHandler.SendItemsPushedToWarehouseResponse(Owner.Client, PushItemToWhStatus.Ok,
          asda2WhItemStubList1,
          asda2WhItemStubList2);
      }
    }

    public void PushItemsToAvatarWh(IEnumerable<Asda2WhItemStub> itemStubs)
    {
      if(!IsItemsExists(itemStubs) || !IsInventorySpaceEnough(itemStubs, false, true))
      {
        Asda2InventoryHandler.SendItemsPushedToAvatarWarehouseResponse(Owner.Client,
          PushItemToWhStatus.ItemNotFounded, null,
          null);
      }
      else
      {
        List<Asda2WhItemStub> asda2WhItemStubList1 = new List<Asda2WhItemStub>();
        List<Asda2WhItemStub> asda2WhItemStubList2 = new List<Asda2WhItemStub>();
        foreach(Asda2WhItemStub itemStub in itemStubs)
        {
          Asda2Item itemToCopyStats = GetItem(itemStub.Invtentory, itemStub.Slot);
          Asda2Item asda2Item = null;
          int num = (int) TryAdd(itemToCopyStats.ItemId, itemStub.Amount, true, ref asda2Item,
            Asda2InventoryType.AvatarWarehouse, itemToCopyStats);
          asda2WhItemStubList1.Add(new Asda2WhItemStub
          {
            Amount = itemToCopyStats.Amount,
            Invtentory = itemToCopyStats.InventoryType,
            Slot = itemToCopyStats.Slot
          });
          asda2WhItemStubList2.Add(new Asda2WhItemStub
          {
            Amount = asda2Item.Amount,
            Invtentory = asda2Item.InventoryType,
            Slot = asda2Item.Slot
          });
          itemToCopyStats.Amount -= itemStub.Amount;
        }

        Asda2InventoryHandler.SendItemsPushedToAvatarWarehouseResponse(Owner.Client, PushItemToWhStatus.Ok,
          asda2WhItemStubList1,
          asda2WhItemStubList2);
      }
    }

    public void TakeItemsFromWh(IEnumerable<Asda2WhItemStub> itemStubs)
    {
      if(IsWarehouseLocked() || !IsItemsExists(itemStubs) ||
         (!IsInventorySpaceEnough(itemStubs, true, false) ||
          !GetCommissionForTake(itemStubs.Count())))
      {
        Asda2InventoryHandler.SendItemsTakedFromWarehouseResponse(Owner.Client,
          PushItemToWhStatus.ItemNotFounded, null,
          null);
      }
      else
      {
        List<Asda2WhItemStub> asda2WhItemStubList1 = new List<Asda2WhItemStub>();
        List<Asda2WhItemStub> asda2WhItemStubList2 = new List<Asda2WhItemStub>();
        foreach(Asda2WhItemStub itemStub in itemStubs)
        {
          Asda2Item itemToCopyStats = GetItem(itemStub.Invtentory, itemStub.Slot);
          Asda2Item asda2Item = null;
          int num = (int) TryAdd(itemToCopyStats.ItemId, itemStub.Amount, true, ref asda2Item,
            new Asda2InventoryType?(), itemToCopyStats);
          itemToCopyStats.Amount -= itemStub.Amount;
          asda2WhItemStubList1.Add(new Asda2WhItemStub
          {
            Amount = itemToCopyStats.Amount,
            Invtentory = itemToCopyStats.InventoryType,
            Slot = itemToCopyStats.Slot
          });
          asda2WhItemStubList2.Add(new Asda2WhItemStub
          {
            Amount = asda2Item.Amount,
            Invtentory = asda2Item.InventoryType,
            Slot = asda2Item.Slot
          });
        }

        Asda2InventoryHandler.SendItemsTakedFromWarehouseResponse(Owner.Client, PushItemToWhStatus.Ok,
          asda2WhItemStubList1,
          asda2WhItemStubList2);
      }
    }

    public void TakeItemsFromAvatarWh(IEnumerable<Asda2WhItemStub> itemStubs)
    {
      if(IsWarehouseLocked() || !IsItemsExists(itemStubs) ||
         (!IsInventorySpaceEnough(itemStubs, true, true) ||
          !GetCommissionForTake(itemStubs.Count())))
      {
        Asda2InventoryHandler.SendItemsTakedFromAvatarWarehouseResponse(Owner.Client,
          PushItemToWhStatus.ItemNotFounded, null,
          null);
      }
      else
      {
        List<Asda2WhItemStub> asda2WhItemStubList1 = new List<Asda2WhItemStub>();
        List<Asda2WhItemStub> asda2WhItemStubList2 = new List<Asda2WhItemStub>();
        foreach(Asda2WhItemStub itemStub in itemStubs)
        {
          Asda2Item itemToCopyStats = GetItem(itemStub.Invtentory, itemStub.Slot);
          Asda2Item asda2Item = null;
          int num = (int) TryAdd(itemToCopyStats.ItemId, itemStub.Amount, true, ref asda2Item,
            new Asda2InventoryType?(), itemToCopyStats);
          itemToCopyStats.Amount -= itemStub.Amount;
          asda2WhItemStubList1.Add(new Asda2WhItemStub
          {
            Amount = itemToCopyStats.Amount,
            Invtentory = itemToCopyStats.InventoryType,
            Slot = itemToCopyStats.Slot
          });
          asda2WhItemStubList2.Add(new Asda2WhItemStub
          {
            Amount = asda2Item.Amount,
            Invtentory = asda2Item.InventoryType,
            Slot = asda2Item.Slot
          });
        }

        Asda2InventoryHandler.SendItemsTakedFromAvatarWarehouseResponse(Owner.Client,
          PushItemToWhStatus.Ok, asda2WhItemStubList1,
          asda2WhItemStubList2);
      }
    }

    private bool GetCommissionForTake(int count)
    {
      return Owner.SubtractMoney((uint) (count * 30));
    }

    private bool IsInventorySpaceEnough(IEnumerable<Asda2WhItemStub> itemStubs, bool pop, bool isAvatar)
    {
      if(pop)
      {
        if(FreeShopSlotsCount < itemStubs.Count())
        {
          Owner.SendInfoMsg("Not enought space in shop inventory.");
          return false;
        }

        if(FreeRegularSlotsCount < itemStubs.Count())
        {
          Owner.SendInfoMsg("Not enought space in regular inventory.");
          return false;
        }
      }
      else if(isAvatar)
      {
        if(FreeAvatarWarehouseSlotsCount < itemStubs.Count())
        {
          Owner.SendInfoMsg("Not enought space in avatar warehouse.");
          return false;
        }
      }
      else if(FreeWarehouseSlotsCount < itemStubs.Count())
      {
        Owner.SendInfoMsg("Not enought space in warehouse.");
        return false;
      }

      return true;
    }

    private bool IsItemsExists(IEnumerable<Asda2WhItemStub> itemStubs)
    {
      foreach(Asda2WhItemStub itemStub in itemStubs)
      {
        Asda2Item asda2Item = GetItem(itemStub.Invtentory, itemStub.Slot);
        if(asda2Item != null)
        {
          if(asda2Item.IsDeleted)
          {
            Owner.SendErrorMsg(string.Format("Item is deleted. inv {0}, slot {1}.",
              itemStub.Invtentory, itemStub.Slot));
            return false;
          }

          if(asda2Item.Amount < itemStub.Amount || itemStub.Amount == 0)
          {
            Owner.SendErrorMsg(string.Format("Item amount is {0} but required {1}. inv {2}, slot {3}.",
              (object) asda2Item.Amount, (object) itemStub.Amount, (object) itemStub.Invtentory,
              (object) itemStub.Slot));
            return false;
          }

          if(asda2Item.ItemId == 20551)
          {
            Owner.SendErrorMsg(string.Format("You cant put gold to warehouse. inv {0}, slot {1}.",
              itemStub.Invtentory, itemStub.Slot));
            return false;
          }
        }
        else
        {
          Owner.SendErrorMsg(string.Format("Item not found. inv {0}, slot {1}.",
            itemStub.Invtentory, itemStub.Slot));
          return false;
        }
      }

      return true;
    }

    private bool IsWarehouseLocked()
    {
      if(!Owner.IsWarehouseLocked)
        return false;
      Owner.SendInfoMsg(
        "Your warehouse is locked. Use <#Warehouse unlock [pass]> command to unlock it. Or use char manager.");
      return true;
    }
  }
}