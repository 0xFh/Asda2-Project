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
            this.Owner = character;
        }

        public Character Owner
        {
            get { return this.m_owner; }
            private set { this.m_owner = value; }
        }

        public short Weight { get; set; }

        public Dictionary<byte, Asda2FastItemSlotRecord[]> FastItemSlotRecords
        {
            get
            {
                if (this._fastItemSlotRecords == null)
                {
                    this._fastItemSlotRecords = new Dictionary<byte, Asda2FastItemSlotRecord[]>();
                    for (byte key = 0; key <= (byte) 5; ++key)
                        this._fastItemSlotRecords.Add(key, new Asda2FastItemSlotRecord[13]);
                }

                return this._fastItemSlotRecords;
            }
        }

        public int FreeRegularSlotsCount
        {
            get
            {
                return ((IEnumerable<Asda2Item>) this.RegularItems).Count<Asda2Item>(
                    (Func<Asda2Item, bool>) (i => i == null));
            }
        }

        public int FreeShopSlotsCount
        {
            get
            {
                int num = 0;
                for (int index = 0; index < (this.Owner.InventoryExpanded ? 60 : 30); ++index)
                {
                    if (this.ShopItems[index] == null)
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
                for (int index = 0; index < (int) this.Owner.Record.PremiumWarehouseBagsCount * 30 + 30; ++index)
                {
                    if (this.WarehouseItems[index] == null)
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
                for (int index = 0; index < (int) this.Owner.Record.PremiumAvatarWarehouseBagsCount * 30 + 30; ++index)
                {
                    if (this.AvatarWarehouseItems[index] == null)
                        ++num;
                }

                return num;
            }
        }

        public void SaveAll()
        {
            foreach (Asda2Item asda2Item in this.Equipment)
            {
                if (asda2Item != null)
                    asda2Item.Save();
            }

            foreach (Asda2Item regularItem in this.RegularItems)
            {
                if (regularItem != null)
                    regularItem.Save();
            }

            foreach (Asda2Item shopItem in this.ShopItems)
            {
                if (shopItem != null)
                    shopItem.Save();
            }

            foreach (Asda2Item warehouseItem in this.WarehouseItems)
            {
                if (warehouseItem != null)
                    warehouseItem.Save();
            }

            foreach (Asda2Item avatarWarehouseItem in this.AvatarWarehouseItems)
            {
                if (avatarWarehouseItem != null)
                    avatarWarehouseItem.Save();
            }

            foreach (KeyValuePair<byte, Asda2FastItemSlotRecord[]> fastItemSlotRecord1 in this.FastItemSlotRecords)
            {
                foreach (Asda2FastItemSlotRecord fastItemSlotRecord2 in fastItemSlotRecord1.Value)
                {
                    if (fastItemSlotRecord2 != null)
                    {
                        try
                        {
                            fastItemSlotRecord2.Save();
                        }
                        catch (StaleStateException ex)
                        {
                        }
                    }
                }
            }
        }

        private void SetEquipment(Asda2Item item, Asda2EquipmentSlots slot)
        {
            Asda2Item asda2Item = this.Equipment[(int) slot];
            if (item != null)
            {
                if (item.IsDeleted)
                {
                    LogUtil.WarnException("{0} trying to equip item {1} witch is deleted.", new object[2]
                    {
                        (object) this.Owner.Name,
                        (object) item.ItemId
                    });
                    return;
                }

                if (item.Record == null)
                {
                    LogUtil.WarnException("{0} trying to equip item {1} witch record is null.", new object[2]
                    {
                        (object) this.Owner.Name,
                        (object) item.ItemId
                    });
                    return;
                }

                item.Slot = (short) slot;
                item.InventoryType = Asda2InventoryType.Equipment;
                item.OwningCharacter = this.Owner;
            }

            this.Equipment[(int) slot] = item;
            if (item != null)
                item.Save();
            if (item == null && slot == Asda2EquipmentSlots.Weapon)
                this.Owner.MainWeapon = (IAsda2Weapon) null;
            else if (item != null && slot == Asda2EquipmentSlots.Weapon && item.IsWeapon)
                this.Owner.MainWeapon = (IAsda2Weapon) item;
            if (asda2Item != null)
            {
                Asda2InventoryHandler.SendCharacterRemoveEquipmentResponse(this.Owner, (short) slot, asda2Item.ItemId);
                asda2Item.OnUnEquip();
            }

            if (item == null)
                return;
            Asda2InventoryHandler.SendCharacterAddEquipmentResponse(this.Owner, (short) slot, item.ItemId,
                (int) item.Enchant);
            item.OnEquip();
        }

        private void SetRegularInventoty(Asda2Item item, short slot, bool silent)
        {
            if (item != null)
            {
                if (item.IsDeleted)
                    throw new InvalidOperationException(string.Format(
                        "{0} trying to set regular item {1} witch is deleted.", (object) this.Owner.Name,
                        (object) item.ItemId));
                if (item.Record == null)
                {
                    LogUtil.WarnException("{0} trying to regular item {1} witch record is null.", new object[2]
                    {
                        (object) this.Owner.Name,
                        (object) item.ItemId
                    });
                    return;
                }

                item.InventoryType = Asda2InventoryType.Regular;
                item.Slot = slot;
                item.OwningCharacter = this.Owner;
            }

            this.RegularItems[(int) slot] = item;
            if (silent)
                return;
            Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, this.Owner, new Asda2Item[1]
            {
                item
            });
        }

        private void SetWarehouseInventoty(Asda2Item item, short slot, bool silent)
        {
            if (item != null)
            {
                if (item.IsDeleted)
                    throw new InvalidOperationException(string.Format("{0} trying to set wh item {1} witch is deleted.",
                        (object) this.Owner.Name, (object) item.ItemId));
                if (item.Record == null)
                {
                    LogUtil.WarnException("{0} trying to wh item {1} witch record is null.", new object[2]
                    {
                        (object) this.Owner.Name,
                        (object) item.ItemId
                    });
                    return;
                }

                item.InventoryType = Asda2InventoryType.Warehouse;
                item.Slot = slot;
                item.OwningCharacter = this.Owner;
            }

            this.WarehouseItems[(int) slot] = item;
            if (item == null || silent)
                return;
            Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, this.Owner, new Asda2Item[1]
            {
                item
            });
        }

        private void SetAvatarWarehouseInventoty(Asda2Item item, short slot, bool silent)
        {
            if (item != null)
            {
                if (item.IsDeleted)
                    throw new InvalidOperationException(string.Format(
                        "{0} trying to set awh item {1} witch is deleted.", (object) this.Owner.Name,
                        (object) item.ItemId));
                if (item.Record == null)
                {
                    LogUtil.WarnException("{0} trying to avatar wh item {1} witch record is null.", new object[2]
                    {
                        (object) this.Owner.Name,
                        (object) item.ItemId
                    });
                    return;
                }

                item.InventoryType = Asda2InventoryType.AvatarWarehouse;
                item.Slot = slot;
                item.OwningCharacter = this.Owner;
            }

            this.AvatarWarehouseItems[(int) slot] = item;
            if (item == null || silent)
                return;
            Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, this.Owner, new Asda2Item[1]
            {
                item
            });
        }

        private void SetShopInventoty(Asda2Item item, short slot, bool silent)
        {
            if (item != null)
            {
                if (item.IsDeleted)
                    throw new InvalidOperationException(string.Format(
                        "{0} trying to set shop item {1} which is deleted.", (object) this.Owner.Name,
                        (object) item.ItemId));
                if (item.Record == null)
                {
                    LogUtil.WarnException("{0} trying to set shop item {1} which record is null.", new object[2]
                    {
                        (object) this.Owner.Name,
                        (object) item.ItemId
                    });
                    return;
                }

                item.InventoryType = Asda2InventoryType.Shop;
                item.Slot = slot;
                item.OwningCharacter = this.Owner;
            }

            this.ShopItems[(int) slot] = item;
            if (item == null || silent)
                return;
            Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, this.Owner, new Asda2Item[1]
            {
                item
            });
        }

        private void SetItem(Asda2Item item, short slot, Asda2InventoryType inventoryType, bool silent = true)
        {
            switch (inventoryType)
            {
                case Asda2InventoryType.Shop:
                    this.SetShopInventoty(item, slot, silent);
                    break;
                case Asda2InventoryType.Regular:
                    this.SetRegularInventoty(item, slot, silent);
                    break;
                case Asda2InventoryType.Warehouse:
                    this.SetWarehouseInventoty(item, slot, silent);
                    break;
                case Asda2InventoryType.AvatarWarehouse:
                    this.SetAvatarWarehouseInventoty(item, slot, silent);
                    break;
                default:
                    this.Owner.SendErrorMsg(string.Format("failed to set item. wrong inventory type {0}",
                        (object) inventoryType));
                    break;
            }
        }

        private short FindFreeShopItemsSlot()
        {
            for (short index = 0; (int) index < (this.Owner.InventoryExpanded ? this.ShopItems.Length : 30); ++index)
            {
                if (this.ShopItems[(int) index] == null)
                    return index;
            }

            return -1;
        }

        private short FindFreeRegularItemsSlot()
        {
            for (short index = 1; (int) index < this.RegularItems.Length; ++index)
            {
                if (this.RegularItems[(int) index] == null)
                    return index;
            }

            return -1;
        }

        private short FindFreeWarehouseItemsSlot()
        {
            for (short index = 0; (int) index < this.WarehouseItems.Length; ++index)
            {
                if (this.WarehouseItems[(int) index] == null)
                    return index;
            }

            return -1;
        }

        private short FindFreeAvatarWarehouseItemsSlot()
        {
            for (short index = 0; (int) index < this.WarehouseItems.Length; ++index)
            {
                if (this.AvatarWarehouseItems[(int) index] == null)
                    return index;
            }

            return -1;
        }

        internal void AddOwnedItems()
        {
            foreach (Asda2DonationItem asda2DonationItem in Asda2DonationItem.LoadAll(this.Owner))
            {
                if (!this.DonationItems.ContainsKey(asda2DonationItem.Guid))
                    this.DonationItems.Add(asda2DonationItem.Guid, asda2DonationItem);
            }

            foreach (Asda2FastItemSlotRecord loadFastItemSlot in (IEnumerable<Asda2FastItemSlotRecord>) this.m_owner
                .Record.GetOrLoadFastItemSlots())
            {
                if (loadFastItemSlot.PanelNum > (byte) 5 || loadFastItemSlot.PanelSlot > (byte) 11)
                    Asda2PlayerInventory.Log.Warn("Bad fastitemslot record {0}", (object) loadFastItemSlot);
                else
                    this.FastItemSlotRecords[loadFastItemSlot.PanelNum][(int) loadFastItemSlot.PanelSlot] =
                        loadFastItemSlot;
            }

            ICollection<Asda2ItemRecord> orLoadItems = this.m_owner.Record.GetOrLoadItems();
            if (orLoadItems == null)
                return;
            List<Asda2Item> asda2ItemList = new List<Asda2Item>(orLoadItems.Count);
            foreach (Asda2ItemRecord record in (IEnumerable<Asda2ItemRecord>) orLoadItems)
            {
                if (!record.IsAuctioned)
                {
                    Asda2ItemTemplate template = Asda2ItemMgr.Templates.Get<Asda2ItemTemplate>(record.ItemId);
                    if (template == null)
                    {
                        Asda2PlayerInventory.Log.Warn(
                            "Item #{0} on {1} could not be loaded because it had an invalid ItemId: {2} ({3})",
                            (object) record.Guid, (object) this, (object) record.ItemId, (object) record.ItemId);
                    }
                    else
                    {
                        Asda2Item asda2Item = Asda2Item.CreateItem(record, this.m_owner, template);
                        asda2ItemList.Add(asda2Item);
                    }
                }
            }

            foreach (Asda2Item asda2Item in asda2ItemList)
            {
                switch (asda2Item.InventoryType)
                {
                    case Asda2InventoryType.Shop:
                        if (asda2Item.Slot >= (short) 0 && (int) asda2Item.Slot < this.ShopItems.Length)
                        {
                            this.ShopItems[(int) asda2Item.Slot] = asda2Item;
                            continue;
                        }

                        continue;
                    case Asda2InventoryType.Regular:
                        if (asda2Item.Slot >= (short) 0 && (int) asda2Item.Slot < this.RegularItems.Length)
                        {
                            this.RegularItems[(int) asda2Item.Slot] = asda2Item;
                            continue;
                        }

                        continue;
                    case Asda2InventoryType.Equipment:
                        this.SetEquipment(asda2Item, (Asda2EquipmentSlots) asda2Item.Slot);
                        continue;
                    case Asda2InventoryType.Warehouse:
                        if (asda2Item.Slot >= (short) 0 && (int) asda2Item.Slot < this.WarehouseItems.Length)
                        {
                            this.WarehouseItems[(int) asda2Item.Slot] = asda2Item;
                            continue;
                        }

                        continue;
                    case Asda2InventoryType.AvatarWarehouse:
                        if (asda2Item.Slot >= (short) 0 && (int) asda2Item.Slot < this.AvatarWarehouseItems.Length)
                        {
                            this.AvatarWarehouseItems[(int) asda2Item.Slot] = asda2Item;
                            continue;
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
            Asda2Item asda2Item1 = (Asda2Item) null;
            Asda2Item asda2Item2 = (Asda2Item) null;
            if (srcInv == Asda2InventoryType.Equipment)
            {
                destSlot = this.FindFreeShopItemsSlot();
                if (destSlot == (short) -1)
                    status = Asda2InventoryError.NoSpace;
            }

            if (srcInv == Asda2InventoryType.Regular && srcSlot == (short) 0 ||
                destInv == Asda2InventoryType.Regular && destSlot == (short) 0)
                return Asda2InventoryError.Fail;
            if (srcInv != Asda2InventoryType.Shop && srcInv != Asda2InventoryType.Equipment &&
                (srcInv != Asda2InventoryType.Warehouse && srcInv != Asda2InventoryType.AvatarWarehouse) &&
                srcInv != Asda2InventoryType.Regular)
            {
                status = Asda2InventoryError.NotInfoAboutItem;
                this.Owner.YouAreFuckingCheater("Moving items from wrong inventory.", 50);
            }
            else if (srcInv == Asda2InventoryType.Regular && destInv != Asda2InventoryType.Regular &&
                     (destInv == Asda2InventoryType.Shop && destSlot != (short) 10))
            {
                this.Owner.YouAreFuckingCheater("Moving items from regular to not regular inventory.", 50);
                status = Asda2InventoryError.NotInfoAboutItem;
            }
            else if (srcInv == Asda2InventoryType.Shop && destInv != Asda2InventoryType.Shop &&
                     destInv != Asda2InventoryType.Equipment)
            {
                this.Owner.YouAreFuckingCheater("Moving items from shop to not shop/equipment inventory.", 50);
                status = Asda2InventoryError.NotInfoAboutItem;
            }
            else if (srcInv == Asda2InventoryType.Warehouse && destInv != Asda2InventoryType.Warehouse)
            {
                this.Owner.YouAreFuckingCheater("Moving items from wh to not wh inventory.", 50);
                status = Asda2InventoryError.NotInfoAboutItem;
            }
            else if (srcInv == Asda2InventoryType.AvatarWarehouse && destInv != Asda2InventoryType.AvatarWarehouse)
            {
                this.Owner.YouAreFuckingCheater("Moving items from awh to not awh inventory.", 50);
                status = Asda2InventoryError.NotInfoAboutItem;
            }
            else if (srcInv == Asda2InventoryType.Equipment && destInv != Asda2InventoryType.Shop &&
                     destInv != Asda2InventoryType.Regular)
            {
                this.Owner.YouAreFuckingCheater("Moving items from equipment to not shop inventory.", 50);
                status = Asda2InventoryError.NotInfoAboutItem;
            }
            else if (
                srcInv == Asda2InventoryType.Shop && (srcSlot < (short) 0 || (int) srcSlot >= this.ShopItems.Length) ||
                srcInv == Asda2InventoryType.Regular &&
                (srcSlot < (short) 0 || (int) srcSlot >= this.RegularItems.Length) ||
                (srcInv == Asda2InventoryType.Equipment &&
                 (srcSlot < (short) 0 || (int) srcSlot >= this.Equipment.Length) ||
                 srcInv == Asda2InventoryType.Warehouse &&
                 (srcSlot < (short) 0 || (int) srcSlot >= this.WarehouseItems.Length)) ||
                srcInv == Asda2InventoryType.AvatarWarehouse &&
                (srcSlot < (short) 0 || (int) srcSlot >= this.AvatarWarehouseItems.Length))
            {
                this.Owner.YouAreFuckingCheater("Moving items from wrong slot.", 50);
                status = Asda2InventoryError.NotInfoAboutItem;
            }
            else if (
                destInv == Asda2InventoryType.Shop && (destSlot < (short) 0 ||
                                                       (int) destSlot >= (this.Owner.InventoryExpanded
                                                           ? this.ShopItems.Length
                                                           : 30)) ||
                (destInv == Asda2InventoryType.Regular &&
                 (destSlot < (short) 0 || (int) destSlot >= this.RegularItems.Length) ||
                 destInv == Asda2InventoryType.Equipment &&
                 (destSlot < (short) 0 || (int) destSlot >= this.Equipment.Length) ||
                 (destInv == Asda2InventoryType.Warehouse &&
                  (destSlot < (short) 0 || (int) destSlot >= this.WarehouseItems.Length) ||
                  destInv == Asda2InventoryType.AvatarWarehouse &&
                  (destSlot < (short) 0 || (int) destSlot >= this.AvatarWarehouseItems.Length))))
                status = Asda2InventoryError.NotInfoAboutItem;
            else if (destInv == Asda2InventoryType.Regular && destSlot == (short) 0)
                status = Asda2InventoryError.NotInfoAboutItem;
            else if (srcInv == Asda2InventoryType.Regular && srcSlot == (short) 0)
                status = Asda2InventoryError.NotInfoAboutItem;

            if (status != Asda2InventoryError.Ok && status != Asda2InventoryError.Ok)
            {
                Asda2InventoryHandler.SendItemReplacedResponse(this.Owner.Client, status, (short) 0, (byte) 0, 0,
                    (short) 0, 0, (byte) 0, 0, (short) 0, false);
                return status;
            }

            switch (srcInv)
            {
                case Asda2InventoryType.Shop:
                    asda2Item1 = this.ShopItems[(int) srcSlot];
                    break;
                case Asda2InventoryType.Regular:
                    asda2Item1 = this.RegularItems[(int) srcSlot];
                    break;
                case Asda2InventoryType.Equipment:
                    asda2Item1 = this.Equipment[(int) srcSlot];
                    break;
                case Asda2InventoryType.Warehouse:
                    asda2Item1 = this.WarehouseItems[(int) srcSlot];
                    break;
                case Asda2InventoryType.AvatarWarehouse:
                    asda2Item1 = this.AvatarWarehouseItems[(int) srcSlot];
                    break;
            }

            if (asda2Item1 == null)
                status = Asda2InventoryError.NotInfoAboutItem;
            else if (!this.m_owner.CanInteract)
            {
                status = Asda2InventoryError.NotInfoAboutItem;
            }
            else
            {
                switch (destInv)
                {
                    case Asda2InventoryType.Shop:
                        asda2Item2 = this.ShopItems[(int) destSlot];
                        break;
                    case Asda2InventoryType.Regular:
                        asda2Item2 = this.RegularItems[(int) destSlot];
                        break;
                    case Asda2InventoryType.Equipment:
                        asda2Item2 = this.Equipment[(int) destSlot];
                        break;
                    case Asda2InventoryType.Warehouse:
                        asda2Item2 = this.WarehouseItems[(int) destSlot];
                        break;
                    case Asda2InventoryType.AvatarWarehouse:
                        asda2Item2 = this.AvatarWarehouseItems[(int) destSlot];
                        break;
                }
            }

            if (status != Asda2InventoryError.Ok)
            {
                Asda2InventoryHandler.SendItemReplacedResponse(this.Owner.Client, status, (short) 0, (byte) 0, 0,
                    (short) 0, 0, (byte) 0, 0, (short) 0, false);
                return status;
            }

            if (destInv == Asda2InventoryType.Equipment && destSlot == (short) 9 &&
                (asda2Item1 != null && asda2Item1.Template.Category != Asda2ItemCategory.OneHandedSword) &&
                this.Equipment[8] != null)
            {
                this.Owner.SendInfoMsg("You cant use this item with shield.");
                return Asda2InventoryError.ItemIsNotForEquiping;
            }

            if (destInv == Asda2InventoryType.Equipment && asda2Item1 != null)
            {
                if ((asda2Item1.Template.EquipmentSlot != Asda2EquipmentSlots.LeftRing || destSlot != (short) 6) &&
                    asda2Item1.Template.EquipmentSlot != (Asda2EquipmentSlots) destSlot)
                {
                    this.Owner.SendInfoMsg("This item is not for equiping.");
                    return Asda2InventoryError.ItemIsNotForEquiping;
                }

                if ((int) asda2Item1.RequiredLevel > this.Owner.Level)
                {
                    this.Owner.SendInfoMsg("Your's level is not enogth.");
                    return Asda2InventoryError.Fail;
                }
            }

            if (asda2Item2 != null && srcInv == Asda2InventoryType.Equipment)
            {
                switch (destInv)
                {
                    case Asda2InventoryType.Shop:
                        asda2Item2 = (Asda2Item) null;
                        short freeShopItemsSlot = this.FindFreeShopItemsSlot();
                        if (freeShopItemsSlot == (short) -1)
                        {
                            status = Asda2InventoryError.NoSpace;
                            break;
                        }

                        destSlot = freeShopItemsSlot;
                        break;
                    case Asda2InventoryType.Regular:
                        asda2Item2 = (Asda2Item) null;
                        short regularItemsSlot = this.FindFreeRegularItemsSlot();
                        if (regularItemsSlot == (short) -1)
                        {
                            status = Asda2InventoryError.NoSpace;
                            break;
                        }

                        destSlot = regularItemsSlot;
                        break;
                }
            }

            if (status != Asda2InventoryError.Ok)
            {
                Asda2InventoryHandler.SendItemReplacedResponse(this.Owner.Client, status, (short) 0, (byte) 0, 0,
                    (short) 0, 0, (byte) 0, 0, (short) 0, false);
            }
            else
            {
                this.SwapUnchecked(srcInv, srcSlot, destInv, destSlot);
                WCell.RealmServer.Logs.Log
                    .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                        this.Owner.EntryId).AddAttribute("source", 0.0, "swap")
                    .AddAttribute(nameof(srcInv), (double) srcInv, srcInv.ToString())
                    .AddAttribute(nameof(destInv), (double) destInv, destInv.ToString())
                    .AddAttribute(nameof(srcSlot), (double) srcSlot, "")
                    .AddAttribute(nameof(destSlot), (double) destSlot, "").AddItemAttributes(asda2Item1, "srcItem")
                    .AddItemAttributes(asda2Item2, "destItem").Write();
                Asda2InventoryHandler.SendItemReplacedResponse(this.Owner.Client, status, srcSlot, (byte) srcInv,
                    asda2Item1 == null ? -1 : asda2Item1.Amount,
                    asda2Item1 == null ? (short) 0 : (short) asda2Item1.Weight, (int) destSlot, (byte) destInv,
                    asda2Item2 == null ? -1 : asda2Item2.Amount,
                    asda2Item2 == null ? (short) 0 : (short) asda2Item1.Weight, asda2Item2 == null);
            }

            return status;
        }

        private void SwapUnchecked(Asda2InventoryType srcInv, short srcSlot, Asda2InventoryType destInv, short destSlot)
        {
            Asda2Item asda2Item1 = (Asda2Item) null;
            Asda2Item asda2Item2 = (Asda2Item) null;
            switch (srcInv)
            {
                case Asda2InventoryType.Shop:
                    asda2Item1 = this.ShopItems[(int) srcSlot];
                    break;
                case Asda2InventoryType.Regular:
                    asda2Item1 = this.RegularItems[(int) srcSlot];
                    break;
                case Asda2InventoryType.Equipment:
                    asda2Item1 = this.Equipment[(int) srcSlot];
                    break;
                case Asda2InventoryType.Warehouse:
                    asda2Item1 = this.WarehouseItems[(int) srcSlot];
                    break;
                case Asda2InventoryType.AvatarWarehouse:
                    asda2Item1 = this.AvatarWarehouseItems[(int) srcSlot];
                    break;
            }

            switch (destInv)
            {
                case Asda2InventoryType.Shop:
                    asda2Item2 = this.ShopItems[(int) destSlot];
                    break;
                case Asda2InventoryType.Regular:
                    asda2Item2 = this.RegularItems[(int) destSlot];
                    break;
                case Asda2InventoryType.Equipment:
                    asda2Item2 = this.Equipment[(int) destSlot];
                    break;
                case Asda2InventoryType.Warehouse:
                    asda2Item2 = this.WarehouseItems[(int) destSlot];
                    break;
                case Asda2InventoryType.AvatarWarehouse:
                    asda2Item2 = this.AvatarWarehouseItems[(int) destSlot];
                    break;
            }

            switch (srcInv)
            {
                case Asda2InventoryType.Shop:
                    this.SetShopInventoty(asda2Item2, srcSlot, true);
                    break;
                case Asda2InventoryType.Regular:
                    this.SetRegularInventoty(asda2Item2, srcSlot, true);
                    break;
                case Asda2InventoryType.Equipment:
                    this.SetEquipment(asda2Item2, (Asda2EquipmentSlots) srcSlot);
                    break;
                case Asda2InventoryType.Warehouse:
                    this.SetWarehouseInventoty(asda2Item2, srcSlot, true);
                    break;
                case Asda2InventoryType.AvatarWarehouse:
                    this.SetAvatarWarehouseInventoty(asda2Item2, srcSlot, true);
                    break;
            }

            switch (destInv)
            {
                case Asda2InventoryType.Shop:
                    this.SetShopInventoty(asda2Item1, destSlot, true);
                    break;
                case Asda2InventoryType.Regular:
                    this.SetRegularInventoty(asda2Item1, destSlot, true);
                    break;
                case Asda2InventoryType.Equipment:
                    this.SetEquipment(asda2Item1, (Asda2EquipmentSlots) destSlot);
                    break;
                case Asda2InventoryType.Warehouse:
                    this.SetWarehouseInventoty(asda2Item1, destSlot, true);
                    break;
                case Asda2InventoryType.AvatarWarehouse:
                    this.SetAvatarWarehouseInventoty(asda2Item1, destSlot, true);
                    break;
            }
        }

        public void RemoveItemFromInventory(Asda2Item asda2Item)
        {
            if (asda2Item.IsDeleted)
                this.Owner.SendErrorMsg(string.Format(
                    "Cant remove deleted item from inventory. inv {0}.slot {1}. itemId {2}",
                    (object) asda2Item.InventoryType, (object) asda2Item.Slot, (object) asda2Item.ItemId));
            else if (asda2Item.Slot < (short) 0)
            {
                this.Owner.SendErrorMsg(string.Format(
                    "Cant remove item from inventory with slot < 0. inv {0}.slot {1}. itemId {2}",
                    (object) asda2Item.InventoryType, (object) asda2Item.Slot, (object) asda2Item.ItemId));
            }
            else
            {
                switch (asda2Item.InventoryType)
                {
                    case Asda2InventoryType.Shop:
                        this.SetShopInventoty((Asda2Item) null, asda2Item.Slot, true);
                        break;
                    case Asda2InventoryType.Regular:
                        this.SetRegularInventoty((Asda2Item) null, asda2Item.Slot, true);
                        break;
                    case Asda2InventoryType.Equipment:
                        this.SetEquipment((Asda2Item) null, (Asda2EquipmentSlots) asda2Item.Slot);
                        break;
                    case Asda2InventoryType.Warehouse:
                        this.SetWarehouseInventoty((Asda2Item) null, asda2Item.Slot, true);
                        break;
                    case Asda2InventoryType.AvatarWarehouse:
                        this.SetAvatarWarehouseInventoty((Asda2Item) null, asda2Item.Slot, true);
                        break;
                }
            }
        }

        public Asda2InventoryError TryAdd(int itemId, int amount, bool silent, ref Asda2Item item,
            Asda2InventoryType? requiredInventoryType = null, Asda2Item itemToCopyStats = null)
        {
            Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(itemId);
            if (template == null)
            {
                this.Owner.SendErrorMsg(string.Format("Failed to create and add item {0}. template not founed",
                    (object) itemId));
                return Asda2InventoryError.Fail;
            }

            Asda2InventoryType add = Asda2PlayerInventory.CalcIntentoryTypeToAdd(requiredInventoryType, template);
            short freeSlot = this.FindFreeSlot(add);
            if (freeSlot < (short) 0)
            {
                this.Owner.SendErrorMsg(string.Format("Failed to create and add item {0}. not enough space",
                    (object) itemId));
                return Asda2InventoryError.NoSpace;
            }

            if (template.IsStackable)
            {
                item = this.FindItem(template, new Asda2InventoryType?(add));
                if (item != null)
                {
                    item.Amount += amount;
                    return Asda2InventoryError.Ok;
                }
            }

            item = Asda2Item.CreateItem(template, this.Owner, amount);
            if (itemToCopyStats != null && itemToCopyStats.Record != null)
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

            this.SetItem(item, freeSlot, add, true);
            return Asda2InventoryError.Ok;
        }

        private short FindFreeSlot(Asda2InventoryType inventoryType)
        {
            short num;
            switch (inventoryType)
            {
                case Asda2InventoryType.Shop:
                    num = this.FindFreeShopItemsSlot();
                    break;
                case Asda2InventoryType.Regular:
                    num = this.FindFreeRegularItemsSlot();
                    break;
                case Asda2InventoryType.Warehouse:
                    num = this.FindFreeWarehouseItemsSlot();
                    break;
                case Asda2InventoryType.AvatarWarehouse:
                    num = this.FindFreeAvatarWarehouseItemsSlot();
                    break;
                default:
                    this.Owner.SendErrorMsg(string.Format("wrong inventory type {0}", (object) inventoryType));
                    num = (short) -1;
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
            switch (inv)
            {
                case Asda2InventoryType.Shop:
                    if ((int) slot >= this.ShopItems.Length)
                    {
                        Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.Fail, this.Owner,
                            (Asda2Item) null);
                        return;
                    }

                    item = this.ShopItems[(int) slot];
                    break;
                case Asda2InventoryType.Regular:
                    if ((int) slot >= this.RegularItems.Length)
                    {
                        Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.Fail, this.Owner,
                            (Asda2Item) null);
                        return;
                    }

                    item = this.RegularItems[(int) slot];
                    break;
                default:
                    Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.Fail, this.Owner, (Asda2Item) null);
                    return;
            }

            if (item == null)
                Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.Fail, this.Owner, (Asda2Item) null);
            else if ((int) item.RequiredLevel > this.Owner.Level)
                Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.CantUseBacauseOfItemLevel, this.Owner,
                    (Asda2Item) null);
            else if (!this.CheckCooldown(item))
                Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.ItemOnCooldown, this.Owner,
                    (Asda2Item) null);
            else if (this.Owner.IsDead)
                Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.Fail, this.Owner, (Asda2Item) null);
            else if (this.Owner.IsTrading)
                Asda2InventoryHandler.SendCharUsedItemResponse(UseItemResult.Fail, this.Owner, (Asda2Item) null);
            else
                this.Owner.AddMessage((Action) (() =>
                    Asda2InventoryHandler.SendCharUsedItemResponse(this.UseItemUnchecked(item), this.Owner, item)));
        }

        private UseItemResult UseItemUnchecked(Asda2Item item)
        {
            switch (item.Category)
            {
                case Asda2ItemCategory.SoulStone:
                    this.Owner.SendSystemMessage("Using {0} is not implemented yet.", new object[1]
                    {
                        (object) item.Category
                    });
                    break;
                case Asda2ItemCategory.PetResurect:
                    return UseItemResult.Fail;
                case Asda2ItemCategory.Incubator:
                    this.Owner.SendSystemMessage("Using {0} is not implemented yet.", new object[1]
                    {
                        (object) item.Category
                    });
                    break;
                case Asda2ItemCategory.PetExp:
                    if (this.Owner.Asda2Pet == null)
                        return UseItemResult.ThereIsNoActivePet;
                    if (!this.Owner.Asda2Pet.GainXp(item.Template.ValueOnUse / 2))
                        return UseItemResult.PetIsMature;
                    break;
                case Asda2ItemCategory.ItemPackage:
                    this.Owner.SendSystemMessage("Using {0} is not implemented yet.", new object[1]
                    {
                        (object) item.Category
                    });
                    break;
                case Asda2ItemCategory.PartialItem:
                    this.Owner.SendSystemMessage("Using {0} is not implemented yet.", new object[1]
                    {
                        (object) item.Category
                    });
                    break;
                case Asda2ItemCategory.Fish:
                    this.Owner.Power += item.Template.ValueOnUse;
                    break;
                case Asda2ItemCategory.SoulShard:
                    this.Owner.SendSystemMessage("Using {0} is not implemented yet.", new object[1]
                    {
                        (object) item.Category
                    });
                    break;
                case Asda2ItemCategory.FishingBook:
                    this.Owner.SendSystemMessage("Using {0} is not implemented yet.", new object[1]
                    {
                        (object) item.Category
                    });
                    break;
                case Asda2ItemCategory.HealthPotion:
                    PereodicAction pereodicAction1 = (PereodicAction) null;
                    if (this.Owner.PereodicActions.ContainsKey(Asda2PereodicActionType.HpRegen))
                        pereodicAction1 = this.Owner.PereodicActions[Asda2PereodicActionType.HpRegen];
                    if (pereodicAction1 != null && pereodicAction1.CallsNum >= 10 &&
                        pereodicAction1.Value >= item.Template.ValueOnUse)
                        return UseItemResult.ItemOnCooldown;
                    if (this.Owner.PereodicActions.ContainsKey(Asda2PereodicActionType.HpRegen))
                        this.Owner.PereodicActions.Remove(Asda2PereodicActionType.HpRegen);
                    this.Owner.PereodicActions.Add(Asda2PereodicActionType.HpRegen,
                        new PereodicAction(this.Owner,
                            (int) ((double) item.Template.ValueOnUse *
                                   (double) CharacterFormulas.CalcHpPotionBoost(this.Owner.Asda2Stamina)), 10, 3000,
                            Asda2PereodicActionType.HpRegen));
                    AchievementProgressRecord progressRecord1 = this.Owner.Achievements.GetOrCreateProgressRecord(81U);
                    switch (++progressRecord1.Counter)
                    {
                        case 50:
                            this.Owner.DiscoverTitle(Asda2TitleId.Weakling215);
                            break;
                        case 100:
                            this.Owner.GetTitle(Asda2TitleId.Weakling215);
                            break;
                    }

                    progressRecord1.SaveAndFlush();
                    break;
                case Asda2ItemCategory.ManaPotion:
                    PereodicAction pereodicAction2 = (PereodicAction) null;
                    if (this.Owner.PereodicActions.ContainsKey(Asda2PereodicActionType.MpRegen))
                        pereodicAction2 = this.Owner.PereodicActions[Asda2PereodicActionType.MpRegen];
                    if (pereodicAction2 != null && pereodicAction2.CallsNum >= 10 &&
                        pereodicAction2.Value >= item.Template.ValueOnUse)
                        return UseItemResult.ItemOnCooldown;
                    if (this.Owner.PereodicActions.ContainsKey(Asda2PereodicActionType.MpRegen))
                        this.Owner.PereodicActions.Remove(Asda2PereodicActionType.MpRegen);
                    this.Owner.PereodicActions.Add(Asda2PereodicActionType.MpRegen,
                        new PereodicAction(this.Owner, item.Template.ValueOnUse, 10, 3000,
                            Asda2PereodicActionType.MpRegen));
                    break;
                case Asda2ItemCategory.Recipe:
                    this.Owner.YouAreFuckingCheater("Trying to use recipe in wrong way.", 50);
                    return UseItemResult.Fail;
                case Asda2ItemCategory.HealthElixir:
                    this.Owner.Health += (int) ((double) item.Template.ValueOnUse *
                                                (double) CharacterFormulas.CalcHpPotionBoost(this.Owner.Asda2Stamina));
                    break;
                case Asda2ItemCategory.ManaElixir:
                    this.Owner.Power += item.Template.ValueOnUse;
                    break;
                case Asda2ItemCategory.ResurectScroll:
                    if (!(this.Owner.Target is Character))
                    {
                        this.Owner.SendSystemMessage("Select character to resurect.", new object[1]
                        {
                            (object) item.Category
                        });
                        return UseItemResult.Fail;
                    }

                    Character target = (Character) this.Owner.Target;
                    if (target.IsAlive)
                    {
                        this.Owner.SendSystemMessage("Select character is alive and can't be resurected.", new object[1]
                        {
                            (object) item.Category
                        });
                        return UseItemResult.Fail;
                    }

                    AchievementProgressRecord progressRecord2 = this.Owner.Achievements.GetOrCreateProgressRecord(83U);
                    switch (++progressRecord2.Counter)
                    {
                        case 500:
                            this.Owner.DiscoverTitle(Asda2TitleId.Savior217);
                            break;
                        case 1000:
                            this.Owner.GetTitle(Asda2TitleId.Savior217);
                            break;
                    }

                    progressRecord2.SaveAndFlush();
                    target.Resurrect();
                    break;
                case Asda2ItemCategory.ReturnScroll:
                    if (this.Owner.IsInCombat || this.Owner.IsAsda2BattlegroundInProgress)
                        return UseItemResult.Fail;
                    this.Owner.TeleportToBindLocation();
                    AchievementProgressRecord progressRecord3 = this.Owner.Achievements.GetOrCreateProgressRecord(82U);
                    switch (++progressRecord3.Counter)
                    {
                        case 500:
                            this.Owner.DiscoverTitle(Asda2TitleId.Returning216);
                            break;
                        case 1000:
                            this.Owner.GetTitle(Asda2TitleId.Returning216);
                            break;
                    }

                    progressRecord3.SaveAndFlush();
                    break;
                default:
                    this.Owner.SendSystemMessage(string.Format("Item {0} from category {1} can't be used.",
                        (object) item.Template.Name, (object) item.Category));
                    return UseItemResult.Fail;
            }

            WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, this.Owner.EntryId)
                .AddAttribute("source", 0.0, "use").AddItemAttributes(item, "").Write();
            --item.Amount;
            return UseItemResult.Ok;
        }

        private bool CheckCooldown(Asda2Item item)
        {
            if (!this._cooldowns.ContainsKey(item.Template.Category))
            {
                this._cooldowns.Add(item.Template.Category, DateTime.Now.AddSeconds(30.0));
            }
            else
            {
                if (this._cooldowns[item.Template.Category] > DateTime.Now)
                    return false;
                this._cooldowns[item.Template.Category] = DateTime.Now.AddSeconds(30.0);
            }

            return true;
        }

        public void RemoveItem(int slot, byte inv, int count)
        {
            if (count == 0)
                count = 1;
            if (inv != (byte) 1 && inv != (byte) 2 || (slot < 0 || slot >= 70))
            {
                this.Owner.SendInfoMsg(string.Format("Failed to removeItem from inventory. slot is {0}. inv is {1}",
                    (object) slot, (object) inv));
                Asda2InventoryHandler.ItemRemovedFromInventoryResponse(this.Owner, (Asda2Item) null,
                    DeleteOrSellItemStatus.Fail, 0);
            }
            else
            {
                Asda2Item asda2Item;
                switch (inv)
                {
                    case 1:
                        if (slot >= this.ShopItems.Length)
                        {
                            Asda2InventoryHandler.ItemRemovedFromInventoryResponse(this.Owner, (Asda2Item) null,
                                DeleteOrSellItemStatus.Fail, 0);
                            this.Owner.SendInfoMsg(string.Format(
                                "Failed to removeItem from inventory. slot is {0}. inv is {1}", (object) slot,
                                (object) inv));
                            return;
                        }

                        asda2Item = this.ShopItems[slot];
                        break;
                    case 2:
                        if (slot >= this.RegularItems.Length)
                        {
                            Asda2InventoryHandler.ItemRemovedFromInventoryResponse(this.Owner, (Asda2Item) null,
                                DeleteOrSellItemStatus.Fail, 0);
                            this.Owner.SendInfoMsg(string.Format(
                                "Failed to removeItem from inventory. slot is {0}. inv is {1}", (object) slot,
                                (object) inv));
                            return;
                        }

                        asda2Item = this.RegularItems[slot];
                        break;
                    default:
                        this.Owner.SendInfoMsg(string.Format(
                            "Failed to removeItem from inventory. slot is {0}. inv is {1}", (object) slot,
                            (object) inv));
                        Asda2InventoryHandler.ItemRemovedFromInventoryResponse(this.Owner, (Asda2Item) null,
                            DeleteOrSellItemStatus.Fail, 0);
                        return;
                }

                if (asda2Item == null || asda2Item.ItemId == 20551)
                {
                    Asda2InventoryHandler.ItemRemovedFromInventoryResponse(this.Owner, (Asda2Item) null,
                        DeleteOrSellItemStatus.Fail, 0);
                    this.Owner.SendInfoMsg(string.Format(
                        "Failed to removeItem from inventory item not found or money. slot is {0}. inv is {1}",
                        (object) slot, (object) inv));
                }
                else
                {
                    WCell.RealmServer.Logs.Log
                        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                            this.Owner.EntryId).AddAttribute("source", 0.0, "remove_from_inventory")
                        .AddItemAttributes(asda2Item, "").AddAttribute("amount", (double) count, "").Write();
                    if (count <= 0)
                        asda2Item.Destroy();
                    else
                        asda2Item.Amount -= asda2Item.Amount < count ? asda2Item.Amount : count;
                    Asda2InventoryHandler.ItemRemovedFromInventoryResponse(this.Owner, asda2Item,
                        DeleteOrSellItemStatus.Ok, count);
                }
            }
        }

        public void SellItems(ItemStub[] itemStubs)
        {
            List<Asda2Item> items = new List<Asda2Item>(5);
            foreach (ItemStub itemStub in itemStubs)
            {
                switch (itemStub.Inv)
                {
                    case Asda2InventoryType.Shop:
                        if (itemStub.Cell >= this.ShopItems.Length || itemStub.Cell < 0)
                        {
                            items.Add((Asda2Item) null);
                            break;
                        }

                        items.Add(this.ShopItems[itemStub.Cell]);
                        break;
                    case Asda2InventoryType.Regular:
                        if (itemStub.Cell >= this.RegularItems.Length || itemStub.Cell < 0)
                        {
                            items.Add((Asda2Item) null);
                            break;
                        }

                        Asda2Item regularItem = this.RegularItems[itemStub.Cell];
                        if (regularItem != null)
                            regularItem.CountForNextSell = itemStub.Amount;
                        items.Add(regularItem);
                        break;
                    default:
                        items.Add((Asda2Item) null);
                        break;
                }
            }

            long num1 = 0;
            foreach (Asda2Item asda2Item in items)
            {
                if (asda2Item != null)
                {
                    int num2 = !asda2Item.Template.IsStackable
                        ? 1
                        : (asda2Item.CountForNextSell > 0
                            ? (asda2Item.Amount < asda2Item.CountForNextSell
                                ? asda2Item.Amount
                                : asda2Item.CountForNextSell)
                            : asda2Item.Amount);
                    float num3 = 1f;
                    if (asda2Item.Template.MaxAmount > 1)
                        num3 /= (float) asda2Item.Template.MaxAmount;
                    int num4 = (int) ((double) ((long) asda2Item.Template.SellPrice * (long) num2) *
                                      (1.0 + (double) this.Owner.FloatMods[34]) * (double) num3);
                    num1 += (long) num4;
                    WCell.RealmServer.Logs.Log
                        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                            this.Owner.EntryId).AddAttribute("source", 0.0, "selling_to_regular_shop")
                        .AddItemAttributes(asda2Item, "").AddAttribute("amount_to_sell", (double) num2, "")
                        .AddAttribute("gold_earned", (double) num4, "").Write();
                    asda2Item.Amount -= num2;
                }
            }

            if (num1 > (long) int.MaxValue || num1 < 0L)
            {
                this.Owner.YouAreFuckingCheater("Wrong total gold amount while selling items.", 20);
                Asda2InventoryHandler.SendSellItemResponseResponse(DeleteOrSellItemStatus.Fail, this.Owner, items);
            }
            else
            {
                this.Owner.AddMoney((uint) num1);
                Asda2InventoryHandler.SendSellItemResponseResponse(DeleteOrSellItemStatus.Ok, this.Owner, items);
                this.Owner.SendMoneyUpdate();
            }
        }

        public void BuyItems(List<ItemStub> itemStubs)
        {
            Asda2Item[] items = new Asda2Item[7];
            List<Asda2ItemTemplate> asda2ItemTemplateList = new List<Asda2ItemTemplate>(7);
            foreach (ItemStub itemStub in itemStubs)
            {
                if (itemStub.ItemId == 0)
                {
                    asda2ItemTemplateList.Add((Asda2ItemTemplate) null);
                }
                else
                {
                    Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(itemStub.ItemId);
                    if (template == null || !template.CanBuyInRegularShop)
                    {
                        Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.BadItemId, this.Owner, items);
                        this.Owner.YouAreFuckingCheater(
                            string.Format("Trying to buy bad item with id {0}.", (object) itemStub.ItemId), 20);
                        return;
                    }

                    if (template.IsStackable && itemStub.Amount <= 0 || !template.IsStackable && itemStub.Amount != 1)
                        itemStub.Amount = 1;
                    asda2ItemTemplateList.Add(template);
                }
            }

            if (!this.CheckFreeRegularItemsSlots(asda2ItemTemplateList.Count<Asda2ItemTemplate>(
                    (Func<Asda2ItemTemplate, bool>) (t =>
                    {
                        if (t != null)
                            return t.InventoryType == (byte) 2;
                        return false;
                    }))) || !this.CheckShopItemsSlots(asda2ItemTemplateList.Count<Asda2ItemTemplate>(
                    (Func<Asda2ItemTemplate, bool>) (t =>
                    {
                        if (t != null)
                            return t.InventoryType == (byte) 1;
                        return false;
                    }))))
            {
                Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughSpace, this.Owner, items);
            }
            else
            {
                long price = this.CalculatePrice(asda2ItemTemplateList, itemStubs);
                if (price < 0L || price >= (long) int.MaxValue)
                {
                    this.Owner.YouAreFuckingCheater("Wrong price while buying items", 20);
                    Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughGold, this.Owner, items);
                }
                else if (price >= (long) this.Owner.Money)
                {
                    Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughGold, this.Owner, items);
                }
                else
                {
                    for (int index1 = 0; index1 < 7; ++index1)
                    {
                        if (asda2ItemTemplateList[index1] != null)
                        {
                            int amount = asda2ItemTemplateList[index1].MaxAmount == 0
                                ? itemStubs[index1].Amount
                                : asda2ItemTemplateList[index1].MaxAmount;
                            Asda2Item asda2Item = (Asda2Item) null;
                            if (asda2ItemTemplateList[index1].IsStackable)
                            {
                                asda2Item = this.FindItem(asda2ItemTemplateList[index1], new Asda2InventoryType?());
                                if (asda2Item != null)
                                {
                                    asda2Item.Amount += amount;
                                    if (asda2Item.Category == Asda2ItemCategory.HealthPotion)
                                    {
                                        AchievementProgressRecord progressRecord =
                                            this.Owner.Achievements.GetOrCreateProgressRecord(93U);
                                        progressRecord.Counter += (uint) amount;
                                        if (progressRecord.Counter >= 1000U)
                                            this.Owner.GetTitle(Asda2TitleId.Stocked226);
                                        progressRecord.SaveAndFlush();
                                    }
                                }
                                else
                                {
                                    asda2Item = Asda2Item.CreateItem(asda2ItemTemplateList[index1], this.Owner, amount);
                                    if (asda2Item.Category == Asda2ItemCategory.HealthPotion)
                                    {
                                        AchievementProgressRecord progressRecord =
                                            this.Owner.Achievements.GetOrCreateProgressRecord(93U);
                                        progressRecord.Counter += (uint) amount;
                                        if (progressRecord.Counter >= 1000U)
                                            this.Owner.GetTitle(Asda2TitleId.Stocked226);
                                        progressRecord.SaveAndFlush();
                                    }

                                    WCell.RealmServer.Logs.Log
                                        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations,
                                            LogSourceType.Character, this.Owner.EntryId)
                                        .AddAttribute("source", 0.0, "buying_from_regular_shop")
                                        .AddItemAttributes(asda2Item, "").Write();
                                    if (asda2Item.Template.IsShopInventoryItem)
                                    {
                                        short freeShopItemsSlot = this.FindFreeShopItemsSlot();
                                        if (freeShopItemsSlot == (short) -1)
                                        {
                                            Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughSpace,
                                                this.Owner, items);
                                            return;
                                        }

                                        this.SetShopInventoty(asda2Item, freeShopItemsSlot, true);
                                    }
                                    else
                                    {
                                        short regularItemsSlot = this.FindFreeRegularItemsSlot();
                                        if (regularItemsSlot == (short) -1)
                                        {
                                            Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughSpace,
                                                this.Owner, items);
                                            return;
                                        }

                                        this.SetRegularInventoty(asda2Item, regularItemsSlot, true);
                                    }
                                }
                            }
                            else
                            {
                                for (int index2 = 0; index2 < amount; ++index2)
                                {
                                    asda2Item = Asda2Item.CreateItem(asda2ItemTemplateList[index1], this.Owner, 1);
                                    WCell.RealmServer.Logs.Log
                                        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations,
                                            LogSourceType.Character, this.Owner.EntryId)
                                        .AddAttribute("source", 0.0, "buying_from_regular_shop")
                                        .AddItemAttributes(asda2Item, "").Write();
                                    if (asda2Item.Template.IsShopInventoryItem)
                                    {
                                        short freeShopItemsSlot = this.FindFreeShopItemsSlot();
                                        if (freeShopItemsSlot == (short) -1)
                                        {
                                            Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughSpace,
                                                this.Owner, items);
                                            return;
                                        }

                                        this.SetShopInventoty(asda2Item, freeShopItemsSlot, true);
                                    }
                                    else
                                    {
                                        short regularItemsSlot = this.FindFreeRegularItemsSlot();
                                        if (regularItemsSlot == (short) -1)
                                        {
                                            Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.NotEnoughSpace,
                                                this.Owner, items);
                                            return;
                                        }

                                        this.SetRegularInventoty(asda2Item, regularItemsSlot, true);
                                    }
                                }
                            }

                            items[index1] = asda2Item;
                        }
                    }

                    this.Owner.SubtractMoney((uint) price);
                    Asda2InventoryHandler.SendBuyItemResponse(Asda2BuyItemStatus.Ok, this.Owner, items);
                    this.Owner.SendMoneyUpdate();
                }
            }
        }

        private Asda2Item FindItem(int itemId, Asda2InventoryType? requiredInventoryType = null)
        {
            Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(itemId);
            if (template != null)
                return this.FindItem(template, requiredInventoryType);
            this.Owner.SendErrorMsg(string.Format("failed ti find item .wrong item id {0}", (object) itemId));
            return (Asda2Item) null;
        }

        private Asda2Item FindItem(Asda2ItemTemplate asda2ItemTemplate,
            Asda2InventoryType? requiredInventoryType = null)
        {
            Asda2InventoryType add =
                Asda2PlayerInventory.CalcIntentoryTypeToAdd(requiredInventoryType, asda2ItemTemplate);
            switch (add)
            {
                case Asda2InventoryType.Shop:
                    return ((IEnumerable<Asda2Item>) this.ShopItems).FirstOrDefault<Asda2Item>(
                        (Func<Asda2Item, bool>) (i =>
                        {
                            if (i != null)
                                return (Asda2ItemId) i.ItemId == asda2ItemTemplate.ItemId;
                            return false;
                        }));
                case Asda2InventoryType.Regular:
                    return ((IEnumerable<Asda2Item>) this.RegularItems).FirstOrDefault<Asda2Item>(
                        (Func<Asda2Item, bool>) (i =>
                        {
                            if (i != null)
                                return (Asda2ItemId) i.ItemId == asda2ItemTemplate.ItemId;
                            return false;
                        }));
                case Asda2InventoryType.Warehouse:
                    return ((IEnumerable<Asda2Item>) this.WarehouseItems).FirstOrDefault<Asda2Item>(
                        (Func<Asda2Item, bool>) (i =>
                        {
                            if (i != null)
                                return (Asda2ItemId) i.ItemId == asda2ItemTemplate.ItemId;
                            return false;
                        }));
                case Asda2InventoryType.AvatarWarehouse:
                    return ((IEnumerable<Asda2Item>) this.AvatarWarehouseItems).FirstOrDefault<Asda2Item>(
                        (Func<Asda2Item, bool>) (i =>
                        {
                            if (i != null)
                                return (Asda2ItemId) i.ItemId == asda2ItemTemplate.ItemId;
                            return false;
                        }));
                default:
                    this.Owner.SendErrorMsg(
                        string.Format("failed ti find item .wrong inventory type {0}", (object) add));
                    return (Asda2Item) null;
            }
        }

        private long CalculatePrice(List<Asda2ItemTemplate> templates, List<ItemStub> itemStubs)
        {
            long num = 0;
            for (int index = 0; index < 7; ++index)
            {
                Asda2ItemTemplate template = templates[index];
                if (template != null)
                    num += (long) template.BuyPrice * (long) itemStubs[index].Amount;
            }

            return num;
        }

        private bool CheckShopItemsSlots(int count)
        {
            int num = ((IEnumerable<Asda2Item>) this.ShopItems).Count<Asda2Item>(
                (Func<Asda2Item, bool>) (i => i == null));
            if (!this.Owner.InventoryExpanded)
                num -= 30;
            return num >= count;
        }

        private bool CheckFreeRegularItemsSlots(int count)
        {
            return ((IEnumerable<Asda2Item>) this.RegularItems).Count<Asda2Item>(
                       (Func<Asda2Item, bool>) (i => i == null)) >= count;
        }

        public void TryPickUpItem(short x, short y)
        {
            Asda2LootItem lootItem = this.Owner.Map.TryPickUpItem(x, y);
            if (lootItem == null)
            {
                Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.Fail, (Asda2Item) null,
                    this.Owner);
                this.Owner.Map.AddMessage(
                    (Action) (() => GlobalHandler.SendRemoveItemResponse(this.Owner.Client, x, y)));
            }
            else if (lootItem.Loot.Looters != null && lootItem.Loot.Looters.Count > 0 &&
                     (lootItem.Loot.SpawnTime.AddSeconds((double) CharacterFormulas.ForeignLootPickupTimeout) >
                      DateTime.Now &&
                      lootItem.Loot.Looters.FirstOrDefault<Asda2LooterEntry>(
                          (Func<Asda2LooterEntry, bool>) (l => l.Owner == this.Owner)) == null))
            {
                AchievementProgressRecord progressRecord = this.Owner.Achievements.GetOrCreateProgressRecord(99U);
                switch (++progressRecord.Counter)
                {
                    case 500:
                        this.Owner.DiscoverTitle(Asda2TitleId.Bandit232);
                        break;
                    case 1000:
                        this.Owner.GetTitle(Asda2TitleId.Bandit232);
                        break;
                }

                progressRecord.SaveAndFlush();
                this.Owner.SendInfoMsg("Stealer!!! It's not your loot!");
                Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.Fail, (Asda2Item) null,
                    this.Owner);
            }
            else
            {
                this.Owner.Map.ClearLootSlot(x, y);
                Asda2Item asda2Item = (Asda2Item) null;
                Asda2InventoryError asda2InventoryError = this.TryAdd((int) lootItem.Template.ItemId, lootItem.Amount,
                    true, ref asda2Item, new Asda2InventoryType?(), (Asda2Item) null);
                if (asda2Item != null)
                    WCell.RealmServer.Logs.Log
                        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                            this.Owner.EntryId).AddAttribute("source", 0.0, "from_loot")
                        .AddAttribute("mob_id",
                            lootItem.Loot.MonstrId.HasValue ? (double) lootItem.Loot.MonstrId.Value : 0.0, "")
                        .AddItemAttributes(asda2Item, "").Write();
                if (asda2InventoryError != Asda2InventoryError.Ok)
                {
                    Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.NoSpace, (Asda2Item) null,
                        this.Owner);
                }
                else
                {
                    if (asda2Item != null && asda2Item.Template.Quality >= Asda2ItemQuality.Green)
                        ChatMgr.SendGlobalMessageResponse(this.Owner.Name, ChatMgr.Asda2GlobalMessageType.HasObinedItem,
                            asda2Item.ItemId, (short) 0, (short) 0);
                    switch (++this.Owner.Achievements.GetOrCreateProgressRecord(100U).Counter)
                    {
                        case 500:
                            this.Owner.DiscoverTitle(Asda2TitleId.Gatherer233);
                            break;
                        case 1000:
                            this.Owner.GetTitle(Asda2TitleId.Gatherer233);
                            break;
                    }

                    Asda2InventoryHandler.SendItemPickupedResponse(Asda2PickUpItemStatus.Ok, asda2Item, this.Owner);
                    this.Owner.Map.AddMessage((Action) (() => GlobalHandler.SendRemoveItemResponse(lootItem)));
                    if (!lootItem.Loot.IsAllItemsTaken)
                        return;
                    lootItem.Loot.Dispose();
                }
            }
        }

        public void SowelItem(short itemCell, short sowelCell, byte sowelSlot, short protectSlot, bool isAvatar = false)
        {
            Asda2Item shopItem = this.ShopItems[(int) itemCell];
            if (shopItem == null)
            {
                Asda2InventoryHandler.SendItemSoweledResponse(this.Owner.Client, (int) this.Weight,
                    (int) this.Owner.Money, SowelingStatus.EquipmentError, (Asda2Item) null, (Asda2Item) null,
                    (Asda2Item) null, isAvatar);
            }
            else
            {
                if (isAvatar)
                {
                    if ((int) shopItem.Enchant < (int) sowelSlot)
                    {
                        Asda2InventoryHandler.SendItemSoweledResponse(this.Owner.Client, (int) this.Weight,
                            (int) this.Owner.Money, SowelingStatus.MaxSocketSlotError, (Asda2Item) null,
                            (Asda2Item) null, (Asda2Item) null, true);
                        return;
                    }
                }
                else if ((int) shopItem.SowelSlots - 1 < (int) sowelSlot)
                {
                    Asda2InventoryHandler.SendItemSoweledResponse(this.Owner.Client, (int) this.Weight,
                        (int) this.Owner.Money, SowelingStatus.MaxSocketSlotError, (Asda2Item) null, (Asda2Item) null,
                        (Asda2Item) null, false);
                    return;
                }

                Asda2Item regularItem = this.RegularItems[(int) sowelCell];
                if (regularItem == null || regularItem.Category != Asda2ItemCategory.Sowel)
                    Asda2InventoryHandler.SendItemSoweledResponse(this.Owner.Client, (int) this.Weight,
                        (int) this.Owner.Money, SowelingStatus.SowelError, (Asda2Item) null, (Asda2Item) null,
                        (Asda2Item) null, isAvatar);
                else if ((int) regularItem.RequiredLevel > this.Owner.Level)
                {
                    Asda2InventoryHandler.SendItemSoweledResponse(this.Owner.Client, (int) this.Weight,
                        (int) this.Owner.Money, SowelingStatus.LowLevel, (Asda2Item) null, (Asda2Item) null,
                        (Asda2Item) null, isAvatar);
                }
                else
                {
                    Asda2Item protect = protectSlot < (short) 0 ? (Asda2Item) null : this.ShopItems[(int) protectSlot];
                    bool flag1 = protect != null && protect.Category == Asda2ItemCategory.SowelProtectionScroll;
                    bool flag2 = this.SowelItemUnchecked(shopItem, regularItem.ItemId, sowelSlot);
                    if (!flag2 && !flag1 || flag2)
                    {
                        regularItem.Destroy();
                        this.RegularItems[(int) sowelCell] = (Asda2Item) null;
                    }

                    LogHelperEntry lgDelete1 = (LogHelperEntry) null;
                    LogHelperEntry lgDelete2 = WCell.RealmServer.Logs.Log
                        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                            this.Owner.EntryId).AddAttribute("operation", 1.0, "sowel_item_sowel_delete")
                        .AddItemAttributes(protect, "").Write();
                    if (protect != null && flag1)
                    {
                        lgDelete1 = WCell.RealmServer.Logs.Log
                            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                                this.Owner.EntryId).AddAttribute("operation", 1.0, "sowel_item_protect_delete")
                            .AddItemAttributes(protect, "").Write();
                        --protect.Amount;
                    }

                    WCell.RealmServer.Logs.Log
                        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                            this.Owner.EntryId).AddAttribute("operation", 0.0, "sowel_item")
                        .AddItemAttributes(shopItem, "")
                        .AddAttribute("success", flag2 ? 1.0 : 0.0, flag2 ? "yes" : "no").AddReference(lgDelete1)
                        .AddReference(lgDelete2).Write();
                    if (!flag2)
                    {
                        AchievementProgressRecord progressRecord =
                            this.Owner.Achievements.GetOrCreateProgressRecord(103U);
                        switch (++progressRecord.Counter)
                        {
                            case 50:
                                this.Owner.DiscoverTitle(Asda2TitleId.Misfortune236);
                                break;
                            case 100:
                                this.Owner.GetTitle(Asda2TitleId.Misfortune236);
                                break;
                        }

                        progressRecord.SaveAndFlush();
                    }

                    Asda2InventoryHandler.SendItemSoweledResponse(this.Owner.Client, (int) this.Weight,
                        (int) this.Owner.Money, flag2 ? SowelingStatus.Ok : SowelingStatus.Fail, shopItem, regularItem,
                        protect, isAvatar);
                }
            }
        }

        private bool SowelItemUnchecked(Asda2Item item, int sowelId, byte sowelSlot)
        {
            if (70 <= Utility.Random(0, 100))
                return false;
            switch (sowelSlot)
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
            if (scrollCell < (short) 0 || (int) scrollCell >= this.ShopItems.Length)
                this.Owner.SendInfoMsg("Wrong scroll cell " + (object) scrollCell);
            else if (itemSlot < (short) 0 || (int) itemSlot >= this.ShopItems.Length)
            {
                this.Owner.SendInfoMsg("Item scroll cell " + (object) scrollCell);
            }
            else
            {
                Asda2Item shopItem1 = this.ShopItems[(int) scrollCell];
                if (shopItem1 == null)
                {
                    Asda2InventoryHandler.SendExchangeItemOptionResultResponse(this.Owner.Client,
                        ExchangeOptionResult.ScrollInvalid, (Asda2Item) null, (Asda2Item) null);
                }
                else
                {
                    Asda2Item shopItem2 = this.ShopItems[(int) itemSlot];
                    if (shopItem2 == null || !shopItem2.Template.IsEquipment)
                    {
                        Asda2InventoryHandler.SendExchangeItemOptionResultResponse(this.Owner.Client,
                            ExchangeOptionResult.ItemInvalid, (Asda2Item) null, (Asda2Item) null);
                    }
                    else
                    {
                        --shopItem1.Amount;
                        LogHelperEntry lgDelete = WCell.RealmServer.Logs.Log
                            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                                this.Owner.EntryId).AddAttribute("operation", 1.0, "exchange_options_scroll_delete")
                            .AddItemAttributes(shopItem1, "").Write();
                        shopItem2.GenerateNewOptions();
                        WCell.RealmServer.Logs.Log
                            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                                this.Owner.EntryId).AddAttribute("operation", 0.0, "exchange_options")
                            .AddItemAttributes(shopItem2, "").AddReference(lgDelete).Write();
                        Asda2InventoryHandler.SendExchangeItemOptionResultResponse(this.Owner.Client,
                            ExchangeOptionResult.Ok, shopItem2, shopItem1);
                    }
                }
            }
        }

        public void UpgradeItem(short itemCell, short stoneCell, short chanceBoostCell, short protectScrollCell)
        {
            Asda2Item shopItem = this.ShopItems[(int) itemCell];
            Asda2Item regularItem = this.RegularItems[(int) stoneCell];
            if (shopItem == null || regularItem == null)
                Asda2InventoryHandler.SendUpgradeItemResponse(this.Owner.Client, UpgradeItemStatus.Fail,
                    (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, (int) this.Weight,
                    this.Owner.Money);
            else if (!this.CalcCanUseThisStone(shopItem, regularItem))
            {
                Asda2InventoryHandler.SendUpgradeItemResponse(this.Owner.Client, UpgradeItemStatus.Fail,
                    (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, (int) this.Weight,
                    this.Owner.Money);
            }
            else
            {
                uint enchantPrice = (uint) Asda2ItemMgr.GetEnchantPrice(shopItem.Enchant, (int) shopItem.RequiredLevel,
                    shopItem.Template.Quality);
                if (!this.Owner.SubtractMoney(enchantPrice))
                {
                    Asda2InventoryHandler.SendUpgradeItemResponse(this.Owner.Client, UpgradeItemStatus.Fail,
                        (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, (int) this.Weight,
                        this.Owner.Money);
                    this.Owner.SendInfoMsg("Not enought money to enchant.");
                }
                else
                {
                    Asda2Item successItem = protectScrollCell == (short) -1
                        ? (Asda2Item) null
                        : this.ShopItems[(int) protectScrollCell];
                    Asda2Item protectionItem = chanceBoostCell == (short) -1
                        ? (Asda2Item) null
                        : this.ShopItems[(int) chanceBoostCell];
                    int useChanceBoost =
                        successItem == null || successItem.Category != Asda2ItemCategory.IncreaceUpgredeChance
                            ? 0
                            : successItem.Template.ValueOnUse;
                    bool useProtect = false;
                    bool noEnchantLose = false;
                    if (protectionItem != null)
                    {
                        if (shopItem.Enchant >= (byte) 10)
                        {
                            switch (protectionItem.Template.ValueOnUse)
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
                        else if (protectionItem.Template.ValueOnUse == 0)
                            useProtect = true;
                    }

                    ItemUpgradeResult itemUpgradeResult = CharacterFormulas.CalculateItemUpgradeResult(
                        regularItem.Template.Quality, shopItem.Template.Quality, shopItem.Enchant,
                        shopItem.RequiredLevel, this.Owner.Asda2Luck, 0, 0, useProtect, useChanceBoost, noEnchantLose);
                    this.Owner.SendSystemMessage(string.Format("{0} with chance {1}(S:{2},P:{3},NC:{4} {5}.",
                        (object) itemUpgradeResult.Status, (object) itemUpgradeResult.Chance,
                        (object) itemUpgradeResult.BoostFromOwnerLuck, (object) itemUpgradeResult.BoostFormGroupLuck,
                        (object) itemUpgradeResult.BoostFromNearbyCharactersLuck,
                        useProtect ? (object) "with protection." : (object) "without protection."));
                    LogHelperEntry lgDelete1 = WCell.RealmServer.Logs.Log
                        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                            this.Owner.EntryId).AddAttribute("operation", 1.0, "enchant_remove_money")
                        .AddAttribute("difference_money", (double) enchantPrice, "")
                        .AddAttribute("total_money", (double) this.Owner.Money, "").Write();
                    LogHelperEntry lgDelete2 = WCell.RealmServer.Logs.Log
                        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                            this.Owner.EntryId).AddAttribute("operation", 1.0, "enchant_remove_stone")
                        .AddItemAttributes(regularItem, "").Write();
                    --regularItem.Amount;
                    LogHelperEntry lgDelete3 = (LogHelperEntry) null;
                    LogHelperEntry lgDelete4 = (LogHelperEntry) null;
                    if (protectionItem != null)
                    {
                        lgDelete3 = WCell.RealmServer.Logs.Log
                            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                                this.Owner.EntryId).AddAttribute("operation", 1.0, "enchant_remove_protect")
                            .AddItemAttributes(protectionItem, "").Write();
                        --protectionItem.Amount;
                    }

                    if (successItem != null)
                    {
                        lgDelete4 = WCell.RealmServer.Logs.Log
                            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                                this.Owner.EntryId).AddAttribute("operation", 1.0, "enchant_remove_chance_boost")
                            .AddItemAttributes(successItem, "").Write();
                        --successItem.Amount;
                    }

                    switch (itemUpgradeResult.Status)
                    {
                        case ItemUpgradeResultStatus.Success:
                            ++shopItem.Enchant;
                            Asda2InventoryHandler.SendUpgradeItemResponse(this.Owner.Client, UpgradeItemStatus.Ok,
                                shopItem, regularItem, successItem, protectionItem, (int) this.Weight,
                                this.Owner.Money);
                            if (shopItem.Enchant >= (byte) 10)
                                ChatMgr.SendGlobalMessageResponse(this.Owner.Name,
                                    ChatMgr.Asda2GlobalMessageType.HasUpgradeItem, shopItem.ItemId,
                                    (short) shopItem.Enchant, (short) 0);
                            WCell.RealmServer.Logs.Log
                                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                                    this.Owner.EntryId).AddAttribute("operation", 0.0, "enchant_success")
                                .AddAttribute("chance", itemUpgradeResult.Chance, "").AddReference(lgDelete3)
                                .AddReference(lgDelete2).AddReference(lgDelete4).AddReference(lgDelete1)
                                .AddItemAttributes(shopItem, "").Write();
                            break;
                        case ItemUpgradeResultStatus.Fail:
                            Asda2InventoryHandler.SendUpgradeItemResponse(this.Owner.Client, UpgradeItemStatus.Fail,
                                shopItem, regularItem, successItem, protectionItem, (int) this.Weight,
                                this.Owner.Money);
                            WCell.RealmServer.Logs.Log
                                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                                    this.Owner.EntryId).AddAttribute("operation", 0.0, "enchant_fail")
                                .AddAttribute("chance", itemUpgradeResult.Chance, "").AddItemAttributes(shopItem, "")
                                .AddReference(lgDelete3).AddReference(lgDelete2).AddReference(lgDelete4)
                                .AddReference(lgDelete1).Write();
                            break;
                        case ItemUpgradeResultStatus.ReduceLevelToZero:
                            shopItem.Enchant = (byte) 0;
                            Asda2InventoryHandler.SendUpgradeItemResponse(this.Owner.Client, UpgradeItemStatus.Fail,
                                shopItem, regularItem, successItem, protectionItem, (int) this.Weight,
                                this.Owner.Money);
                            WCell.RealmServer.Logs.Log
                                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                                    this.Owner.EntryId).AddAttribute("operation", 0.0, "enchant_reduce_to_zero")
                                .AddAttribute("chance", itemUpgradeResult.Chance, "").AddReference(lgDelete3)
                                .AddReference(lgDelete2).AddReference(lgDelete4).AddReference(lgDelete1)
                                .AddItemAttributes(shopItem, "").Write();
                            break;
                        case ItemUpgradeResultStatus.ReduceOneLevel:
                            --shopItem.Enchant;
                            Asda2InventoryHandler.SendUpgradeItemResponse(this.Owner.Client, UpgradeItemStatus.Fail,
                                shopItem, regularItem, successItem, protectionItem, (int) this.Weight,
                                this.Owner.Money);
                            WCell.RealmServer.Logs.Log
                                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                                    this.Owner.EntryId).AddAttribute("operation", 0.0, "enchant_reduce_one_level")
                                .AddAttribute("chance", itemUpgradeResult.Chance, "").AddReference(lgDelete3)
                                .AddReference(lgDelete2).AddReference(lgDelete4).AddReference(lgDelete1)
                                .AddItemAttributes(shopItem, "").Write();
                            AchievementProgressRecord progressRecord1 =
                                this.Owner.Achievements.GetOrCreateProgressRecord(106U);
                            switch (++progressRecord1.Counter)
                            {
                                case 50:
                                    this.Owner.DiscoverTitle(Asda2TitleId.Cursed258);
                                    break;
                                case 100:
                                    this.Owner.GetTitle(Asda2TitleId.Cursed258);
                                    break;
                            }

                            progressRecord1.SaveAndFlush();
                            break;
                        case ItemUpgradeResultStatus.BreakItem:
                            WCell.RealmServer.Logs.Log
                                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                                    this.Owner.EntryId).AddAttribute("operation", 0.0, "enchant_break_item")
                                .AddAttribute("chance", itemUpgradeResult.Chance, "").AddReference(lgDelete3)
                                .AddReference(lgDelete2).AddReference(lgDelete4).AddReference(lgDelete1)
                                .AddItemAttributes(shopItem, "").Write();
                            AchievementProgressRecord progressRecord2 =
                                this.Owner.Achievements.GetOrCreateProgressRecord(107U);
                            switch (++progressRecord2.Counter)
                            {
                                case 25:
                                    this.Owner.DiscoverTitle(Asda2TitleId.Broken259);
                                    break;
                                case 50:
                                    this.Owner.GetTitle(Asda2TitleId.Broken259);
                                    break;
                            }

                            progressRecord2.SaveAndFlush();
                            shopItem.IsDeleted = true;
                            Asda2InventoryHandler.SendUpgradeItemResponse(this.Owner.Client, UpgradeItemStatus.Fail,
                                shopItem, regularItem, successItem, protectionItem, (int) this.Weight,
                                this.Owner.Money);
                            shopItem.IsDeleted = false;
                            if (shopItem.Enchant >= (byte) 10)
                                ChatMgr.SendGlobalMessageResponse(this.Owner.Name,
                                    ChatMgr.Asda2GlobalMessageType.HasUpgradeFail, shopItem.ItemId,
                                    (short) shopItem.Enchant, (short) 0);
                            shopItem.Destroy();
                            break;
                    }

                    this.Owner.SendMoneyUpdate();
                }
            }
        }

        private bool CalcCanUseThisStone(Asda2Item item, Asda2Item stone)
        {
            switch (stone.Category)
            {
                case Asda2ItemCategory.EnchantWeaponStoneD:
                    if (item.IsWeapon)
                        return item.RequiredLevel <= (byte) 20;
                    return false;
                case Asda2ItemCategory.EnchantWeaponStoneC:
                    if (item.IsWeapon)
                        return item.RequiredLevel <= (byte) 40;
                    return false;
                case Asda2ItemCategory.EnchantWeaponStoneB:
                    if (item.IsWeapon)
                        return item.RequiredLevel <= (byte) 60;
                    return false;
                case Asda2ItemCategory.EnchantWeaponStoneA:
                    if (item.IsWeapon)
                        return item.RequiredLevel <= (byte) 80;
                    return false;
                case Asda2ItemCategory.EnchantWeaponStoneS:
                    return item.IsWeapon;
                case Asda2ItemCategory.EnchantArmorStoneD:
                    if (item.IsArmor)
                        return item.RequiredLevel <= (byte) 20;
                    return false;
                case Asda2ItemCategory.EnchantArmorStoneC:
                    if (item.IsArmor)
                        return item.RequiredLevel <= (byte) 40;
                    return false;
                case Asda2ItemCategory.EnchantArmorStoneB:
                    if (item.IsArmor)
                        return item.RequiredLevel <= (byte) 60;
                    return false;
                case Asda2ItemCategory.EnchantArmorStoneA:
                    if (item.IsArmor)
                        return item.RequiredLevel <= (byte) 80;
                    return false;
                case Asda2ItemCategory.EnchantArmorStoneS:
                    return item.IsArmor;
                case Asda2ItemCategory.EnchantArmorStoneE:
                    return item.IsArmor;
                case Asda2ItemCategory.EnchantUniversalStoneE:
                    if (!item.IsArmor)
                        return item.IsWeapon;
                    return true;
                case Asda2ItemCategory.EnchantUniversalStoneD:
                    if (item.IsArmor || item.IsWeapon)
                        return item.RequiredLevel <= (byte) 20;
                    return false;
                case Asda2ItemCategory.EnchantUniversalStoneC:
                    if (item.IsArmor || item.IsWeapon)
                        return item.RequiredLevel <= (byte) 40;
                    return false;
                case Asda2ItemCategory.EnchantUniversalStoneB:
                    if (item.IsArmor || item.IsWeapon)
                        return item.RequiredLevel <= (byte) 60;
                    return false;
                case Asda2ItemCategory.EnchantUniversalStoneA:
                    if (item.IsArmor || item.IsWeapon)
                        return item.RequiredLevel <= (byte) 80;
                    return false;
                case Asda2ItemCategory.EnchantUniversalStoneS:
                    if (!item.IsArmor)
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
            if (inv != Asda2InventoryType.Regular && inv != Asda2InventoryType.Shop)
                return OpenBosterStatus.Fail;
            Asda2Item asda2Item1 = this.GetItem(inv, cell);
            if (asda2Item1 == null || asda2Item1.Category != Asda2ItemCategory.Booster)
                return OpenBosterStatus.ItIsNotABooster;
            List<BoosterDrop> boosterDrop1 = Asda2ItemMgr.BoosterDrops[asda2Item1.BoosterId];
            if (boosterDrop1 == null)
                return OpenBosterStatus.BoosterError;
            if (!this.CheckFreeRegularItemsSlots(1) || !this.CheckShopItemsSlots(1))
                return OpenBosterStatus.NoSpace;
            Asda2Item addedItem = new Asda2Item();
            BoosterDrop boosterDrop2 = boosterDrop1.Last<BoosterDrop>();
            float num1 = Utility.Random(0.0f, 100f);
            float num2 = 0.0f;
            LogHelperEntry lgDelete = WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, this.Owner.EntryId)
                .AddAttribute("source", 0.0, "open_booster_delete").AddItemAttributes(asda2Item1, "").Write();
            foreach (BoosterDrop boosterDrop3 in boosterDrop1)
            {
                num2 += boosterDrop3.Chance;
                if (boosterDrop2 == boosterDrop3 || (double) num1 <= (double) num2)
                {
                    Asda2Item asda2Item2 = (Asda2Item) null;
                    int num3 = (int) this.TryAdd(boosterDrop3.ItemId, 1, true, ref asda2Item2,
                        new Asda2InventoryType?(), (Asda2Item) null);
                    if (asda2Item2 == null)
                        return OpenBosterStatus.NoSpace;
                    LogHelperEntry logHelperEntry = WCell.RealmServer.Logs.Log
                        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                            this.Owner.EntryId).AddAttribute("source", 0.0, "open_booster_create")
                        .AddAttribute("booster_item_id", (double) asda2Item1.ItemId, "")
                        .AddItemAttributes(asda2Item2, "");
                    logHelperEntry.AddReference(lgDelete);
                    logHelperEntry.Write();
                    addedItem = asda2Item2;
                    if (asda2Item2.Template.Quality >= Asda2ItemQuality.Green)
                    {
                        ChatMgr.SendGlobalMessageResponse(this.Owner.Name, ChatMgr.Asda2GlobalMessageType.HasObinedItem,
                            boosterDrop3.ItemId, (short) 0, (short) 0);
                        break;
                    }

                    break;
                }
            }

            asda2Item1.Destroy();
            if (addedItem.Category == Asda2ItemCategory.Egg)
            {
                AchievementProgressRecord progressRecord = this.Owner.Achievements.GetOrCreateProgressRecord(164U);
                switch (++progressRecord.Counter)
                {
                    case 3:
                        this.Owner.DiscoverTitle(Asda2TitleId.Adopted355);
                        break;
                    case 5:
                        this.Owner.GetTitle(Asda2TitleId.Adopted355);
                        break;
                }

                progressRecord.SaveAndFlush();
            }

            Asda2InventoryHandler.SendbosterOpenedResponse(this.Owner.Client, OpenBosterStatus.Ok, addedItem, inv, cell,
                this.Weight);
            return OpenBosterStatus.Ok;
        }

        private Asda2Item GetItem(Asda2InventoryType inv, short cell)
        {
            switch (inv)
            {
                case Asda2InventoryType.Shop:
                    if (cell < (short) 0 || (int) cell >= this.ShopItems.Length)
                        return (Asda2Item) null;
                    return this.ShopItems[(int) cell];
                case Asda2InventoryType.Regular:
                    if (cell < (short) 0 || (int) cell >= this.RegularItems.Length)
                        return (Asda2Item) null;
                    return this.RegularItems[(int) cell];
                case Asda2InventoryType.Equipment:
                    if (cell < (short) 0 || (int) cell >= this.Equipment.Length)
                        return (Asda2Item) null;
                    return this.Equipment[(int) cell];
                case Asda2InventoryType.Warehouse:
                    if (cell < (short) 0 || (int) cell >= this.WarehouseItems.Length)
                        return (Asda2Item) null;
                    return this.WarehouseItems[(int) cell];
                case Asda2InventoryType.AvatarWarehouse:
                    if (cell < (short) 0 || (int) cell >= this.AvatarWarehouseItems.Length)
                        return (Asda2Item) null;
                    return this.AvatarWarehouseItems[(int) cell];
                default:
                    return (Asda2Item) null;
            }
        }

        public OpenPackageStatus OpenPackage(Asda2InventoryType packageInv, short packageSlot)
        {
            if (packageInv != Asda2InventoryType.Regular && packageInv != Asda2InventoryType.Shop)
                return OpenPackageStatus.PackageItemError;
            Asda2Item asda2Item1 = this.GetItem(packageInv, packageSlot);
            if (asda2Item1 == null || asda2Item1.Category != Asda2ItemCategory.ItemPackage)
                return OpenPackageStatus.PackageItemError;
            List<PackageDrop> packageDrop1 = Asda2ItemMgr.PackageDrops[asda2Item1.PackageId];
            if (packageDrop1 == null)
                return OpenPackageStatus.PackageItemError;
            if (!this.CheckFreeRegularItemsSlots(packageDrop1.Count) || !this.CheckShopItemsSlots(packageDrop1.Count))
                return OpenPackageStatus.InfoErrorInEmptyInventry;
            List<Asda2Item> addedItems = new List<Asda2Item>();
            LogHelperEntry lgDelete = WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, this.Owner.EntryId)
                .AddAttribute("source", 0.0, "open_package_delete").AddItemAttributes(asda2Item1, "").Write();
            foreach (PackageDrop packageDrop2 in packageDrop1)
            {
                Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(packageDrop2.ItemId);
                if (template != null)
                {
                    Asda2Item asda2Item2 = (Asda2Item) null;
                    int num = (int) this.TryAdd(packageDrop2.ItemId,
                        template.IsStackable
                            ? (template.MaxAmount == 0 ? packageDrop2.Amount : template.MaxAmount * packageDrop2.Amount)
                            : 1, true, ref asda2Item2, new Asda2InventoryType?(), (Asda2Item) null);
                    if (asda2Item2 == null)
                    {
                        LogUtil.WarnException("Open package get null item by Try add. Unexpected! {0} {1}",
                            new object[2]
                            {
                                (object) this.Owner.Account.Name,
                                (object) this.Owner.Name
                            });
                        return OpenPackageStatus.InfoErrorInEmptyInventry;
                    }

                    asda2Item2.IsSoulbound = true;
                    LogHelperEntry logHelperEntry = WCell.RealmServer.Logs.Log
                        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                            this.Owner.EntryId).AddAttribute("source", 0.0, "open_package_create")
                        .AddAttribute("package_item_id", (double) asda2Item1.ItemId, "")
                        .AddItemAttributes(asda2Item2, "");
                    logHelperEntry.AddReference(lgDelete);
                    logHelperEntry.Write();
                    addedItems.Add(asda2Item2);
                }
            }

            asda2Item1.Destroy();
            Asda2InventoryHandler.SendOpenPackageResponseResponse(this.Owner.Client, OpenPackageStatus.Ok, addedItems,
                packageInv, packageSlot, this.Weight);
            return OpenPackageStatus.Ok;
        }

        public DisasembleItemStatus DisasembleItem(Asda2InventoryType invNum, short slot)
        {
            if (invNum != Asda2InventoryType.Shop)
                return DisasembleItemStatus.LackOfMaterialForCraft;
            Asda2Item asda2Item1 = this.GetItem(invNum, slot);
            if (asda2Item1 == null)
                return DisasembleItemStatus.LackOfMaterialForCraft;
            if (!Asda2ItemMgr.DecompositionDrops.ContainsKey(asda2Item1.ItemId))
            {
                this.Owner.SendSystemMessage(string.Format(
                    "Item id {0} can't dissassembled cause need to update dissassemble table. Please report to admin.",
                    (object) asda2Item1.ItemId));
                return DisasembleItemStatus.LackOfMaterialForCraft;
            }

            List<DecompositionDrop> decompositionDrop1 = Asda2ItemMgr.DecompositionDrops[asda2Item1.ItemId];
            if (decompositionDrop1 == null)
                return DisasembleItemStatus.LackOfMaterialForCraft;
            if (!this.CheckFreeRegularItemsSlots(1) || !this.CheckShopItemsSlots(1))
                return DisasembleItemStatus.NoEmptySlotInThePlate;
            Asda2Item addedItem = new Asda2Item();
            DecompositionDrop decompositionDrop2 = decompositionDrop1.Last<DecompositionDrop>();
            foreach (DecompositionDrop decompositionDrop3 in decompositionDrop1)
            {
                if (decompositionDrop2 == decompositionDrop3 ||
                    (double) Utility.Random(0.0f, 100f) <= (double) decompositionDrop3.Chance)
                {
                    if (Asda2ItemMgr.GetTemplate(decompositionDrop3.ItemId) == null)
                        return DisasembleItemStatus.LackOfMaterialForCraft;
                    Asda2Item asda2Item2 = (Asda2Item) null;
                    int num = (int) this.TryAdd(decompositionDrop3.ItemId, 1, true, ref asda2Item2,
                        new Asda2InventoryType?(), (Asda2Item) null);
                    if (asda2Item2 == null)
                    {
                        LogUtil.ErrorException("Dissassemble item get null item by Try add. Unexpected! {0} {1}",
                            new object[2]
                            {
                                (object) this.Owner.Account.Name,
                                (object) this.Owner.Name
                            });
                        return DisasembleItemStatus.CraftingInfoIsInaccurate;
                    }

                    addedItem = asda2Item2;
                    LogHelperEntry logHelperEntry = WCell.RealmServer.Logs.Log
                        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                            this.Owner.EntryId).AddAttribute("source", 0.0, "disassemble_create")
                        .AddAttribute("disassemble_item_id", (double) asda2Item1.ItemId, "")
                        .AddItemAttributes(asda2Item2, "");
                    LogHelperEntry lgDelete = WCell.RealmServer.Logs.Log
                        .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                            this.Owner.EntryId).AddAttribute("source", 0.0, "disassemble_delete")
                        .AddItemAttributes(asda2Item1, "").Write();
                    logHelperEntry.AddReference(lgDelete);
                    logHelperEntry.Write();
                    break;
                }
            }

            if (asda2Item1.IsSoulbound)
            {
                AchievementProgressRecord progressRecord = this.Owner.Achievements.GetOrCreateProgressRecord(95U);
                switch (++progressRecord.Counter)
                {
                    case 30:
                        this.Owner.DiscoverTitle(Asda2TitleId.Destructive228);
                        break;
                    case 60:
                        this.Owner.GetTitle(Asda2TitleId.Destructive228);
                        break;
                }

                progressRecord.SaveAndFlush();
            }

            asda2Item1.Destroy();
            Asda2InventoryHandler.SendEquipmentDisasembledResponse(this.Owner.Client, DisasembleItemStatus.Ok,
                this.Weight, addedItem, slot);
            return DisasembleItemStatus.Ok;
        }

        public BuyFromWarShopStatus BuyItemFromWarshop(int internalWarShopId)
        {
            if (!this.CheckFreeRegularItemsSlots(1) || !this.CheckShopItemsSlots(1))
                return BuyFromWarShopStatus.InventoryIsFull;
            WarShopDataRecord warshopDataRecord = Asda2ItemMgr.GetWarshopDataRecord(internalWarShopId);
            if (warshopDataRecord == null)
                return BuyFromWarShopStatus.CantFoundItem;
            Asda2Item moneyItem = this.FindItem(Asda2ItemMgr.GetTemplate(warshopDataRecord.Money1Type),
                new Asda2InventoryType?());
            if (moneyItem == null)
                return BuyFromWarShopStatus.NotEnoghtExchangeItems;
            LogHelperEntry lgDelete;
            if (moneyItem.ItemId == 20551)
            {
                if (!this.Owner.SubtractMoney((uint) warshopDataRecord.Cost1))
                    return BuyFromWarShopStatus.NonEnoghtGold;
                lgDelete = WCell.RealmServer.Logs.Log
                    .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                        this.Owner.EntryId).AddAttribute("source", 0.0, "buyed_from_war_shop_remove_money")
                    .AddAttribute("cost", (double) warshopDataRecord.Cost1, "")
                    .AddAttribute("total_money", (double) this.Owner.Money, "").Write();
            }
            else
            {
                if (moneyItem.Amount < warshopDataRecord.Cost1)
                    return BuyFromWarShopStatus.NotEnoghtExchangeItems;
                lgDelete = WCell.RealmServer.Logs.Log
                    .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                        this.Owner.EntryId).AddAttribute("source", 0.0, "buyed_from_war_shop_remove_money_item")
                    .AddItemAttributes(moneyItem, "").AddAttribute("cost", (double) warshopDataRecord.Cost1, "")
                    .Write();
                moneyItem.Amount -= warshopDataRecord.Cost1;
            }

            Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(warshopDataRecord.ItemId);
            if (template == null)
                return BuyFromWarShopStatus.CantFoundItem;
            Asda2Item buyedItem = (Asda2Item) null;
            if (this.TryAdd(warshopDataRecord.ItemId, warshopDataRecord.Amount == 0 ? 1 : warshopDataRecord.Amount,
                    true, ref buyedItem, new Asda2InventoryType?(), (Asda2Item) null) != Asda2InventoryError.Ok)
                return BuyFromWarShopStatus.UnableToPurshace;
            WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, this.Owner.EntryId)
                .AddAttribute("source", 0.0, "buyed_from_war_shop").AddReference(lgDelete)
                .AddItemAttributes(buyedItem, "").Write();
            Asda2InventoryHandler.SendItemFromWarshopBuyedResponse(this.Owner.Client, BuyFromWarShopStatus.Ok,
                this.Weight, (int) this.Owner.Money, moneyItem, buyedItem);
            World.BroadcastMsg("Donation shop",
                string.Format("Thanks to {0} for buying {1}[{2}] and helping server!", (object) this.Owner.Name,
                    (object) template.Name, (object) template.Id), Color.PaleGreen);
            this.Owner.SendMoneyUpdate();
            return BuyFromWarShopStatus.Ok;
        }

        public bool UseGlobalChatItem()
        {
            Asda2Item globalChatItem = ((IEnumerable<Asda2Item>) this.ShopItems).FirstOrDefault<Asda2Item>(
                (Func<Asda2Item, bool>) (i =>
                {
                    if (i != null)
                        return i.Category == Asda2ItemCategory.GlobalChat;
                    return false;
                }));
            if (globalChatItem == null)
            {
                this.Owner.SendSystemMessage("You must have global chat item to use this chat.");
                return false;
            }

            WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, this.Owner.EntryId)
                .AddAttribute("source", 0.0, "use_global_chat_item").AddItemAttributes(globalChatItem, "").Write();
            --globalChatItem.Amount;
            ChatMgr.SendGlobalChatRemoveItemResponse(this.Owner.Client, true, globalChatItem);
            return true;
        }

        public bool UseTeleportScroll(bool somming = false)
        {
            if (!somming)
            {
                Asda2Item asda2Item = ((IEnumerable<Asda2Item>) this.ShopItems).FirstOrDefault<Asda2Item>(
                    (Func<Asda2Item, bool>) (i =>
                    {
                        if (i != null)
                            return i.Category == Asda2ItemCategory.TeleportToCharacter;
                        return false;
                    }));
                if (asda2Item == null)
                    return false;
                WCell.RealmServer.Logs.Log
                    .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                        this.Owner.EntryId).AddAttribute("source", 0.0, "use_teleport_scroll_item")
                    .AddItemAttributes(asda2Item, "").Write();
                --asda2Item.Amount;
                Asda2InventoryHandler.UpdateItemInventoryInfo(this.Owner.Client, asda2Item);
            }

            if (somming)
            {
                Asda2Item asda2Item = ((IEnumerable<Asda2Item>) this.ShopItems).FirstOrDefault<Asda2Item>(
                    (Func<Asda2Item, bool>) (i =>
                    {
                        if (i != null)
                            return i.Category == Asda2ItemCategory.SummonCharacterToYou;
                        return false;
                    }));
                if (asda2Item == null)
                    return false;
                WCell.RealmServer.Logs.Log
                    .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                        this.Owner.EntryId).AddAttribute("source", 0.0, "use_summon_scroll_item")
                    .AddItemAttributes(asda2Item, "").Write();
                --asda2Item.Amount;
                Asda2InventoryHandler.UpdateItemInventoryInfo(this.Owner.Client, asda2Item);
            }

            return true;
        }

        public void AuctionItem(Asda2ItemTradeRef itemRef)
        {
            if (!itemRef.Item.Template.IsStackable)
                itemRef.Amount = itemRef.Item.Amount;
            Asda2Item asda2Item = itemRef.Item;
            itemRef.Item = Asda2Item.CreateItem(itemRef.Item.ItemId, itemRef.Item.OwningCharacter, itemRef.Amount);
            asda2Item.Amount -= itemRef.Amount;
            itemRef.Item.Slot = asda2Item.Slot;
            itemRef.Item.InventoryType = asda2Item.InventoryType;
            LogHelperEntry removeLog = WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, this.Owner.EntryId)
                .AddAttribute("source", 0.0, "auctioning_item_left").AddAttribute("amount", (double) itemRef.Amount, "")
                .AddItemAttributes(asda2Item, "").Write();
            this.AddToAuction(itemRef, removeLog);
        }

        private void AddToAuction(Asda2ItemTradeRef itemRef, LogHelperEntry removeLog)
        {
            uint amount = (uint) ((double) CharacterFormulas.AuctionPushComission * (double) itemRef.Price);
            if (!this.Owner.SubtractMoney(amount))
            {
                this.Owner.YouAreFuckingCheater("Auctioning item without money", 100);
                throw new InvalidOperationException("unexpected behavior");
            }

            itemRef.Item.AuctionPrice = itemRef.Price;
            Asda2AuctionMgr.RegisterItem(itemRef.Item.Record);
            WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, this.Owner.EntryId)
                .AddAttribute("source", 0.0, "auctioning_item").AddAttribute("commission", (double) amount, "")
                .AddAttribute("price", (double) itemRef.Price, "").AddItemAttributes(itemRef.Item, "")
                .AddAttribute("tolal_money", (double) this.Owner.Money, "").AddReference(removeLog).Write();
            itemRef.Item.Save();
            this.Owner.SendAuctionMsg(string.Format("[Reg] {0} for {1} gold. [Cms] {2} gold.",
                (object) itemRef.Item.Template.Name, (object) itemRef.Price, (object) amount));
        }

        public void LearnRecipe(short slot)
        {
            if (slot < (short) 1 || (int) slot >= this.RegularItems.Length)
            {
                Asda2CraftingHandler.SendRecipeLeadnedResponse(this.Owner.Client, false, (short) 0, (Asda2Item) null);
                this.Owner.YouAreFuckingCheater("Trying to learn not existing recipe.Bad SLOT.", 50);
            }
            else
            {
                Asda2Item regularItem = this.RegularItems[(int) slot];
                if (regularItem == null)
                {
                    Asda2CraftingHandler.SendRecipeLeadnedResponse(this.Owner.Client, false, (short) 0,
                        (Asda2Item) null);
                    this.Owner.YouAreFuckingCheater("Trying to learn not existing recipe.", 1);
                }
                else if (regularItem.Category != Asda2ItemCategory.Recipe)
                {
                    Asda2CraftingHandler.SendRecipeLeadnedResponse(this.Owner.Client, false, (short) 0,
                        (Asda2Item) null);
                    this.Owner.YouAreFuckingCheater("Trying to learn not recipe item.", 50);
                }
                else
                {
                    int valueOnUse = regularItem.Template.ValueOnUse;
                    Asda2RecipeTemplate recipeTemplate = Asda2CraftMgr.GetRecipeTemplate(valueOnUse);
                    Asda2ItemTemplate template = Asda2ItemMgr.GetTemplate(recipeTemplate.ResultItemIds[0]);
                    if (recipeTemplate == null)
                    {
                        Asda2CraftingHandler.SendRecipeLeadnedResponse(this.Owner.Client, false, (short) 0,
                            (Asda2Item) null);
                        this.Owner.SendCraftingMsg("Can't find recipe info. Recipe id is " + (object) valueOnUse);
                    }
                    else if ((int) this.Owner.Record.CraftingLevel < recipeTemplate.CraftingLevel)
                    {
                        Asda2CraftingHandler.SendRecipeLeadnedResponse(this.Owner.Client, false, (short) 0,
                            (Asda2Item) null);
                        this.Owner.SendCraftingMsg("Trying to learn recipe with level higher than you have.");
                    }
                    else
                    {
                        try
                        {
                            if (this.Owner.LearnedRecipes.GetBit(valueOnUse))
                            {
                                Asda2CraftingHandler.SendRecipeLeadnedResponse(this.Owner.Client, false, (short) 0,
                                    (Asda2Item) null);
                                this.Owner.SendCraftingMsg("Recipe already learned.");
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Asda2CraftingHandler.SendRecipeLeadnedResponse(this.Owner.Client, false, (short) 0,
                                (Asda2Item) null);
                            this.Owner.SendCraftingMsg("Wrond recipe id " + (object) valueOnUse);
                            return;
                        }

                        this.Owner.LearnedRecipes.SetBit(valueOnUse);
                        ++this.Owner.LearnedRecipesCount;
                        if (template.IsArmor || template.IsWeapon)
                        {
                            AchievementProgressRecord progressRecord =
                                this.Owner.Achievements.GetOrCreateProgressRecord(109U);
                            switch (++progressRecord.Counter)
                            {
                                case 50:
                                    this.Owner.DiscoverTitle(Asda2TitleId.Blacksmith268);
                                    break;
                                case 100:
                                    this.Owner.GetTitle(Asda2TitleId.Blacksmith268);
                                    break;
                            }

                            progressRecord.SaveAndFlush();
                        }

                        if (template.Category == Asda2ItemCategory.HealthPotion ||
                            template.Category == Asda2ItemCategory.ManaPotion ||
                            (template.Category == Asda2ItemCategory.HealthElixir ||
                             template.Category == Asda2ItemCategory.ManaElixir))
                        {
                            AchievementProgressRecord progressRecord =
                                this.Owner.Achievements.GetOrCreateProgressRecord(110U);
                            switch (++progressRecord.Counter)
                            {
                                case 3:
                                    this.Owner.DiscoverTitle(Asda2TitleId.Alchemist269);
                                    break;
                                case 5:
                                    this.Owner.GetTitle(Asda2TitleId.Alchemist269);
                                    break;
                            }

                            progressRecord.SaveAndFlush();
                        }

                        WCell.RealmServer.Logs.Log
                            .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                                this.Owner.EntryId).AddAttribute("source", 0.0, "learn_recipe")
                            .AddItemAttributes(regularItem, "").Write();
                        --regularItem.Amount;
                        Asda2CraftingHandler.SendRecipeLeadnedResponse(this.Owner.Client, true, (short) valueOnUse,
                            regularItem);
                    }
                }
            }
        }

        public Asda2Item FindRegularItem(int requredItemId)
        {
            return ((IEnumerable<Asda2Item>) this.RegularItems).FirstOrDefault<Asda2Item>((Func<Asda2Item, bool>) (i =>
            {
                if (i != null)
                    return i.ItemId == requredItemId;
                return false;
            }));
        }

        public Asda2Item GetRegularItem(short slotInq)
        {
            if (slotInq < (short) 0 || (int) slotInq >= this.RegularItems.Length)
                return (Asda2Item) null;
            return this.RegularItems[(int) slotInq];
        }

        public Asda2Item GetShopShopItem(short slotInq)
        {
            if (slotInq < (short) 0 || (int) slotInq >= this.ShopItems.Length)
                return (Asda2Item) null;
            return this.ShopItems[(int) slotInq];
        }

        public Asda2Item GetWarehouseItem(short slotInq)
        {
            if (slotInq < (short) 0 || (int) slotInq >= this.WarehouseItems.Length)
                return (Asda2Item) null;
            return this.WarehouseItems[(int) slotInq];
        }

        public Asda2Item GetAvatarWarehouseItem(short slotInq)
        {
            if (slotInq < (short) 0 || (int) slotInq >= this.AvatarWarehouseItems.Length)
                return (Asda2Item) null;
            return this.AvatarWarehouseItems[(int) slotInq];
        }

        public HatchEggStatus HatchEgg(short slotInq, short slotEgg, short slotSupl)
        {
            if (slotInq < (short) 0 || slotEgg < (short) 0 ||
                ((int) slotInq > this.RegularItems.Length || (int) slotEgg > this.RegularItems.Length) ||
                (int) slotSupl > this.ShopItems.Length)
            {
                this.Owner.YouAreFuckingCheater("Sending wrong inventory info when hatching egg.", 50);
                return HatchEggStatus.Fail;
            }

            Asda2Item regularItem1 = this.RegularItems[(int) slotInq];
            Asda2Item regularItem2 = this.RegularItems[(int) slotEgg];
            Asda2Item asda2Item = slotSupl < (short) 0 ? (Asda2Item) null : this.ShopItems[(int) slotSupl];
            if (regularItem1 == null || regularItem2 == null)
            {
                this.Owner.YouAreFuckingCheater("Egg or iqubator not exist when hatching egg.", 1);
                return HatchEggStatus.Fail;
            }

            if (regularItem1.Category != Asda2ItemCategory.Incubator)
            {
                this.Owner.YouAreFuckingCheater(
                    string.Format("Trying to use {0} as incubator :)", (object) regularItem1.Name), 50);
                return HatchEggStatus.Fail;
            }

            if (regularItem2.Category != Asda2ItemCategory.Egg)
            {
                this.Owner.YouAreFuckingCheater(
                    string.Format("Trying to use {0} as egg :)", (object) regularItem2.Name), 50);
                return HatchEggStatus.Fail;
            }

            if ((int) regularItem2.RequiredLevel > this.Owner.Level)
            {
                this.Owner.YouAreFuckingCheater(
                    string.Format("Trying to hatch egg with required level {0} that higher than his level {1} :)",
                        (object) regularItem2.RequiredLevel, (object) this.Owner.Level), 50);
                return HatchEggStatus.Fail;
            }

            if (this.Owner.OwnedPets.Count >= 6 + 6 * (int) this.Owner.Record.PetBoxEnchants)
            {
                this.Owner.SendInfoMsg("You already have max pet count.");
                return HatchEggStatus.Fail;
            }

            bool flag = regularItem1.Template.ValueOnUse + (asda2Item == null ? 0 : 30000) >= Utility.Random(0, 100000);
            WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, this.Owner.EntryId)
                .AddAttribute("source", 0.0, "learn_recipe").AddItemAttributes(regularItem1, "inqubator")
                .AddItemAttributes(regularItem2, "egg").AddItemAttributes(asda2Item, "supl")
                .AddAttribute("success", flag ? 1.0 : 0.0, flag ? "yes" : "no").Write();
            regularItem1.Destroy();
            regularItem2.Destroy();
            if (asda2Item != null)
                asda2Item.ModAmount(-1);
            if (!flag)
                return HatchEggStatus.PetHatchingFailed;
            PetTemplate petTemplate = Asda2PetMgr.PetTemplates.Get<PetTemplate>(regularItem2.Template.ValueOnUse);
            if (petTemplate == null)
            {
                this.Owner.YouAreFuckingCheater(
                    string.Format("Error on hatching egg {0} cant find template {1}.", (object) regularItem2,
                        (object) regularItem2.Template.ValueOnUse), 0);
                return HatchEggStatus.NoEgg;
            }

            AchievementProgressRecord progressRecord1 = this.Owner.Achievements.GetOrCreateProgressRecord(165U);
            switch (++progressRecord1.Counter)
            {
                case 1:
                    this.Owner.Client.ActiveCharacter.Map.CallDelayed(500,
                        (Action) (() => this.Owner.GetTitle(Asda2TitleId.Pet356)));
                    break;
                case 5:
                    this.Owner.Client.ActiveCharacter.Map.CallDelayed(500,
                        (Action) (() => this.Owner.DiscoverTitle(Asda2TitleId.Farm357)));
                    break;
                case 10:
                    this.Owner.Client.ActiveCharacter.Map.CallDelayed(500,
                        (Action) (() => this.Owner.GetTitle(Asda2TitleId.Farm357)));
                    break;
                case 17:
                    this.Owner.Client.ActiveCharacter.Map.CallDelayed(500,
                        (Action) (() => this.Owner.DiscoverTitle(Asda2TitleId.Zoo358)));
                    break;
                case 25:
                    this.Owner.Client.ActiveCharacter.Map.CallDelayed(500,
                        (Action) (() => this.Owner.GetTitle(Asda2TitleId.Zoo358)));
                    break;
            }

            progressRecord1.SaveAndFlush();
            if (petTemplate.Rarity == 2)
            {
                AchievementProgressRecord progressRecord2 = this.Owner.Achievements.GetOrCreateProgressRecord(166U);
                switch (++progressRecord2.Counter)
                {
                    case 10:
                        this.Owner.Client.ActiveCharacter.Map.CallDelayed(500,
                            (Action) (() => this.Owner.GetTitle(Asda2TitleId.Exotic362)));
                        break;
                    case 20:
                        this.Owner.Client.ActiveCharacter.Map.CallDelayed(500,
                            (Action) (() => this.Owner.DiscoverTitle(Asda2TitleId.Exotic362)));
                        break;
                }

                progressRecord2.SaveAndFlush();
            }

            if (petTemplate.Id == 1 || petTemplate.Id == 3 || (petTemplate.Id == 7 || petTemplate.Id == 11) ||
                (petTemplate.Id == 13 || petTemplate.Id == 17 || (petTemplate.Id == 21 || petTemplate.Id == 23)) ||
                petTemplate.Id == 27)
                this.Owner.Client.ActiveCharacter.Map.CallDelayed(500,
                    (Action) (() => this.Owner.GetTitle(Asda2TitleId.Beast359)));
            if (petTemplate.Id == 2 || petTemplate.Id == 5 || (petTemplate.Id == 6 || petTemplate.Id == 12) ||
                (petTemplate.Id == 15 || petTemplate.Id == 16 || (petTemplate.Id == 22 || petTemplate.Id == 25)) ||
                petTemplate.Id == 26)
                this.Owner.Client.ActiveCharacter.Map.CallDelayed(500,
                    (Action) (() => this.Owner.GetTitle(Asda2TitleId.Vegetable360)));
            if (petTemplate.Id == 4 || petTemplate.Id == 8 || (petTemplate.Id == 32 || petTemplate.Id == 14) ||
                (petTemplate.Id == 18 || petTemplate.Id == 34 || (petTemplate.Id == 24 || petTemplate.Id == 28)) ||
                petTemplate.Id == 36)
                this.Owner.Client.ActiveCharacter.Map.CallDelayed(500,
                    (Action) (() => this.Owner.GetTitle(Asda2TitleId.Machine361)));
            this.Owner.AddAsda2Pet(petTemplate, false);
            return HatchEggStatus.Ok;
        }

        public void OnDeath()
        {
            foreach (Asda2Item asda2Item in this.Equipment)
            {
                if (asda2Item != null)
                    asda2Item.DecreaseDurability((byte) ((uint) asda2Item.MaxDurability / 10U), false);
            }
        }

        public Asda2DonationItem AddDonateItem(Asda2ItemTemplate templ, int amount, string initializer,
            bool isSoulBound = false)
        {
            Asda2DonationItem asda2DonationItem = new Asda2DonationItem(this.Owner.EntityId.Low, (int) templ.Id, amount,
                initializer, isSoulBound);
            asda2DonationItem.Create();
            this.Owner.Asda2Inventory.DonationItems.Add(asda2DonationItem.Guid, asda2DonationItem);
            Asda2InventoryHandler.SendSomeNewItemRecivedResponse(this.Owner.Client, asda2DonationItem.ItemId,
                (byte) 102);
            return asda2DonationItem;
        }

        public void DropItems(List<Asda2Item> itemsToDrop)
        {
            Asda2NPCLoot loot = new Asda2NPCLoot();
            loot.Items = itemsToDrop.Select<Asda2Item, Asda2LootItem>((Func<Asda2Item, Asda2LootItem>) (asda2Item =>
                new Asda2LootItem(asda2Item.Template, 1, 0U)
                {
                    Loot = (Asda2Loot) loot
                })).ToArray<Asda2LootItem>();
            loot.Lootable = (IAsda2Lootable) this.Owner;
            loot.MonstrId = new short?((short) 22222);
            this.Owner.Map.SpawnLoot((Asda2Loot) loot);
            foreach (Asda2Item asda2Item in itemsToDrop)
            {
                switch (asda2Item.InventoryType)
                {
                    case Asda2InventoryType.Shop:
                        if (asda2Item.IsWeapon || asda2Item.IsArmor ||
                            asda2Item.Category == Asda2ItemCategory.ItemPackage)
                        {
                            Asda2InventoryHandler.SendItemReplacedResponse(this.Owner.Client, Asda2InventoryError.Ok,
                                (short) 60, (byte) 1, -1, (short) 0, (int) asda2Item.Slot,
                                (byte) asda2Item.InventoryType, asda2Item.Amount, (short) 0, false);
                            this.SwapUnchecked(asda2Item.InventoryType, asda2Item.Slot, Asda2InventoryType.Shop,
                                (short) 60);
                            this.RemoveItem(60, (byte) 1, asda2Item.Amount);
                            continue;
                        }

                        continue;
                    case Asda2InventoryType.Regular:
                        Asda2InventoryHandler.SendItemReplacedResponse(this.Owner.Client, Asda2InventoryError.Ok,
                            (short) 60, (byte) 1, -1, (short) 0, (int) asda2Item.Slot, (byte) asda2Item.InventoryType,
                            asda2Item.Amount, (short) 0, false);
                        this.SwapUnchecked(asda2Item.InventoryType, asda2Item.Slot, Asda2InventoryType.Shop,
                            (short) 60);
                        this.RemoveItem(60, (byte) 1, asda2Item.Amount);
                        continue;
                    case Asda2InventoryType.Equipment:
                        Asda2InventoryHandler.SendItemReplacedResponse(this.Owner.Client, Asda2InventoryError.Ok,
                            (short) 60, (byte) 1, -1, (short) 0, (int) asda2Item.Slot, (byte) asda2Item.InventoryType,
                            asda2Item.Amount, (short) 0, false);
                        this.SwapUnchecked(asda2Item.InventoryType, asda2Item.Slot, Asda2InventoryType.Shop,
                            (short) 60);
                        this.RemoveItem(60, (byte) 1, asda2Item.Amount);
                        continue;
                    default:
                        continue;
                }
            }
        }

        public void FillOnCharacterCreate()
        {
            this.SetEquipment(Asda2Item.CreateItem(21498, this.Owner, 1), Asda2EquipmentSlots.Weapon);
            this.SetRegularInventoty(Asda2Item.CreateItem(20551, this.Owner, 1), (short) 0, true);
            this.SetRegularInventoty(Asda2Item.CreateItem(20572, this.Owner, 30), (short) 1, true);
            this.SetRegularInventoty(Asda2Item.CreateItem(20583, this.Owner, 10), (short) 2, true);
            this.SetRegularInventoty(Asda2Item.CreateItem(31820, this.Owner, 1), (short) 3, true);
            this.SetRegularInventoty(Asda2Item.CreateItem(32314, this.Owner, 20), (short) 4, true);
            this.SetShopInventoty(Asda2Item.CreateItem(21499, this.Owner, 1), (short) 0, true);
            this.SetShopInventoty(Asda2Item.CreateItem(20615, this.Owner, 1), (short) 1, true);
            this.SetShopInventoty(Asda2Item.CreateItem(33527, this.Owner, 1), (short) 2, true);
            this.SetShopInventoty(Asda2Item.CreateItem(26, this.Owner, 5), (short) 4, true);
        }

        public void CombineItems(short comtinationId)
        {
            ItemCombineDataRecord itemCombineRecord = Asda2ItemMgr.ItemCombineRecords[(int) comtinationId];
            if (itemCombineRecord == null)
                this.Owner.SendInfoMsg(string.Format("Can't combine items cause record №{0} not founded.",
                    (object) comtinationId));
            else if (this.FreeRegularSlotsCount < 1 || this.FreeShopSlotsCount < 1)
            {
                this.Owner.SendInfoMsg("Not enought space.");
            }
            else
            {
                List<Asda2Item> usedItems = new List<Asda2Item>();
                for (int index = 0; index < 5; ++index)
                {
                    int requiredItem = itemCombineRecord.RequiredItems[index];
                    if (requiredItem != -1)
                    {
                        Asda2Item asda2Item = this.FindItem(requiredItem,
                            new Asda2InventoryType?(Asda2InventoryType.Regular));
                        int amount = itemCombineRecord.Amounts[index];
                        if (asda2Item == null || asda2Item.Amount < amount)
                        {
                            this.Owner.SendInfoMsg(string.Format(
                                "Can't combine items cause not enought resources. Item Id {0} amount {1}.",
                                (object) requiredItem, (object) amount));
                            return;
                        }

                        usedItems.Add(asda2Item);
                    }
                    else
                        break;
                }

                for (int index = 0; index < usedItems.Count; ++index)
                {
                    Asda2Item asda2Item = usedItems[index];
                    if (asda2Item.Amount - itemCombineRecord.Amounts[index] <= 0)
                        asda2Item.IsDeleted = true;
                    else
                        asda2Item.Amount -= itemCombineRecord.Amounts[index];
                }

                Asda2Item resultItem = (Asda2Item) null;
                int num1 = (int) this.TryAdd(itemCombineRecord.ResultItem, 1, true, ref resultItem,
                    new Asda2InventoryType?(), (Asda2Item) null);
                LogHelperEntry logHelperEntry = WCell.RealmServer.Logs.Log
                    .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character,
                        this.Owner.EntryId).AddAttribute("source", 0.0, "combine_items")
                    .AddItemAttributes(resultItem, "result").Write();
                int num2 = 0;
                foreach (Asda2Item asda2Item in usedItems)
                    logHelperEntry.AddItemAttributes(asda2Item, "resource_item_" + (object) num2++);
                if (resultItem.ItemId >= 31547 && 31606 <= resultItem.ItemId)
                {
                    AchievementProgressRecord progressRecord = this.Owner.Achievements.GetOrCreateProgressRecord(102U);
                    switch (++progressRecord.Counter)
                    {
                        case 7:
                            this.Owner.DiscoverTitle(Asda2TitleId.Zodiac235);
                            break;
                        case 15:
                            this.Owner.GetTitle(Asda2TitleId.Zodiac235);
                            break;
                    }

                    progressRecord.SaveAndFlush();
                }

                Asda2InventoryHandler.SendItemCombinedResponse(this.Owner.Client, resultItem, usedItems);
            }
        }

        public Asda2Item TryCraftItem(short recId, out List<Asda2Item> materials)
        {
            materials = new List<Asda2Item>();
            if (!this.Owner.LearnedRecipes.GetBit((int) recId))
            {
                this.Owner.SendErrorMsg("Trying craft not learned recipe. " + (object) recId);
                return (Asda2Item) null;
            }

            Asda2RecipeTemplate recipeTemplate = Asda2CraftMgr.GetRecipeTemplate((int) recId);
            if (recipeTemplate == null)
            {
                this.Owner.SendErrorMsg("Can't find recipe template. " + (object) recId);
                return (Asda2Item) null;
            }

            if (this.FreeRegularSlotsCount < 1 || this.FreeRegularSlotsCount < 1)
            {
                this.Owner.SendCraftingMsg("Not enought space.");
                return (Asda2Item) null;
            }

            int index = 0;
            foreach (int requredItemId in recipeTemplate.RequredItemIds)
            {
                if (requredItemId != -1)
                {
                    Asda2Item asda2Item =
                        this.FindItem(requredItemId, new Asda2InventoryType?(Asda2InventoryType.Regular));
                    if (asda2Item == null || asda2Item.Amount < (int) recipeTemplate.ReqiredItemAmounts[index])
                    {
                        this.Owner.SendErrorMsg("Not enought materials to craft.");
                        return (Asda2Item) null;
                    }

                    materials.Add(asda2Item);
                    asda2Item.Amount -= (int) recipeTemplate.ReqiredItemAmounts[index];
                    ++index;
                }
                else
                    break;
            }

            byte num1 = CharacterFormulas.GetCraftedRarity();
            if (num1 == (byte) 0)
            {
                while (num1 == (byte) 0)
                    num1 = CharacterFormulas.GetCraftedRarity();
            }

            if ((int) num1 > (int) recipeTemplate.MaximumPosibleRarity)
                num1 = recipeTemplate.MaximumPosibleRarity;
            int resultItemId = recipeTemplate.ResultItemIds[(int) num1 - 1];
            short resultItemAmount = recipeTemplate.ResultItemAmounts[(int) num1 - 1];
            if (resultItemAmount <= (short) 0)
            {
                this.Owner.SendErrorMsg("Crafted amount error.");
                return (Asda2Item) null;
            }

            Asda2Item asda2Item1 = (Asda2Item) null;
            if (this.TryAdd(resultItemId, (int) resultItemAmount, false, ref asda2Item1, new Asda2InventoryType?(),
                    (Asda2Item) null) != Asda2InventoryError.Ok)
            {
                this.Owner.SendErrorMsg("Cant add crafted item.");
                return (Asda2Item) null;
            }

            WCell.RealmServer.Logs.Log
                .Create(WCell.RealmServer.Logs.Log.Types.ItemOperations, LogSourceType.Character, this.Owner.EntryId)
                .AddAttribute("source", 0.0, "craft_create").AddItemAttributes(asda2Item1, "")
                .AddAttribute("recipe_id", (double) recipeTemplate.Id, "").Write();
            int diffLvl = (int) this.Owner.Record.CraftingLevel - recipeTemplate.CraftingLevel;
            float num2 = CharacterFormulas.CalcCraftingExp(diffLvl, this.Owner.Record.CraftingLevel);
            if (diffLvl > 0)
                this.Owner.GuildPoints += CharacterFormulas.CraftingGuildPointsPerLevel * diffLvl;
            this.Owner.Record.CraftingExp += num2;
            if ((double) this.Owner.Record.CraftingExp >= 100.0)
            {
                ++this.Owner.Record.CraftingLevel;
                this.Owner.Record.CraftingExp = 0.0f;
                if (this.Owner.Record.CraftingLevel == (byte) 2)
                    this.Owner.GetTitle(Asda2TitleId.Apprentice265);
                if (this.Owner.Record.CraftingLevel == (byte) 5)
                    this.Owner.GetTitle(Asda2TitleId.Master266);
            }

            this.Owner.GainXp(
                CharacterFormulas.CalcExpForCrafting(diffLvl, this.Owner.Record.CraftingLevel, (byte) this.Owner.Level),
                "craft", false);
            asda2Item1.Record.IsCrafted = true;
            asda2Item1.GenerateOptionsByCraft();
            return asda2Item1;
        }

        public void PushItemsToWh(IEnumerable<Asda2WhItemStub> itemStubs)
        {
            if (!this.IsItemsExists(itemStubs) || !this.IsInventorySpaceEnough(itemStubs, false, false))
            {
                Asda2InventoryHandler.SendItemsPushedToWarehouseResponse(this.Owner.Client,
                    PushItemToWhStatus.ItemNotFounded, (IEnumerable<Asda2WhItemStub>) null,
                    (IEnumerable<Asda2WhItemStub>) null);
            }
            else
            {
                List<Asda2WhItemStub> asda2WhItemStubList1 = new List<Asda2WhItemStub>();
                List<Asda2WhItemStub> asda2WhItemStubList2 = new List<Asda2WhItemStub>();
                foreach (Asda2WhItemStub itemStub in itemStubs)
                {
                    Asda2Item itemToCopyStats = this.GetItem(itemStub.Invtentory, itemStub.Slot);
                    Asda2Item asda2Item = (Asda2Item) null;
                    int num = (int) this.TryAdd(itemToCopyStats.ItemId, itemStub.Amount, true, ref asda2Item,
                        new Asda2InventoryType?(Asda2InventoryType.Warehouse), itemToCopyStats);
                    itemToCopyStats.Amount -= itemStub.Amount;
                    asda2WhItemStubList1.Add(new Asda2WhItemStub()
                    {
                        Amount = itemToCopyStats.Amount,
                        Invtentory = itemToCopyStats.InventoryType,
                        Slot = itemToCopyStats.Slot
                    });
                    asda2WhItemStubList2.Add(new Asda2WhItemStub()
                    {
                        Amount = asda2Item.Amount,
                        Invtentory = asda2Item.InventoryType,
                        Slot = asda2Item.Slot
                    });
                }

                Asda2InventoryHandler.SendItemsPushedToWarehouseResponse(this.Owner.Client, PushItemToWhStatus.Ok,
                    (IEnumerable<Asda2WhItemStub>) asda2WhItemStubList1,
                    (IEnumerable<Asda2WhItemStub>) asda2WhItemStubList2);
            }
        }

        public void PushItemsToAvatarWh(IEnumerable<Asda2WhItemStub> itemStubs)
        {
            if (!this.IsItemsExists(itemStubs) || !this.IsInventorySpaceEnough(itemStubs, false, true))
            {
                Asda2InventoryHandler.SendItemsPushedToAvatarWarehouseResponse(this.Owner.Client,
                    PushItemToWhStatus.ItemNotFounded, (IEnumerable<Asda2WhItemStub>) null,
                    (IEnumerable<Asda2WhItemStub>) null);
            }
            else
            {
                List<Asda2WhItemStub> asda2WhItemStubList1 = new List<Asda2WhItemStub>();
                List<Asda2WhItemStub> asda2WhItemStubList2 = new List<Asda2WhItemStub>();
                foreach (Asda2WhItemStub itemStub in itemStubs)
                {
                    Asda2Item itemToCopyStats = this.GetItem(itemStub.Invtentory, itemStub.Slot);
                    Asda2Item asda2Item = (Asda2Item) null;
                    int num = (int) this.TryAdd(itemToCopyStats.ItemId, itemStub.Amount, true, ref asda2Item,
                        new Asda2InventoryType?(Asda2InventoryType.AvatarWarehouse), itemToCopyStats);
                    asda2WhItemStubList1.Add(new Asda2WhItemStub()
                    {
                        Amount = itemToCopyStats.Amount,
                        Invtentory = itemToCopyStats.InventoryType,
                        Slot = itemToCopyStats.Slot
                    });
                    asda2WhItemStubList2.Add(new Asda2WhItemStub()
                    {
                        Amount = asda2Item.Amount,
                        Invtentory = asda2Item.InventoryType,
                        Slot = asda2Item.Slot
                    });
                    itemToCopyStats.Amount -= itemStub.Amount;
                }

                Asda2InventoryHandler.SendItemsPushedToAvatarWarehouseResponse(this.Owner.Client, PushItemToWhStatus.Ok,
                    (IEnumerable<Asda2WhItemStub>) asda2WhItemStubList1,
                    (IEnumerable<Asda2WhItemStub>) asda2WhItemStubList2);
            }
        }

        public void TakeItemsFromWh(IEnumerable<Asda2WhItemStub> itemStubs)
        {
            if (this.IsWarehouseLocked() || !this.IsItemsExists(itemStubs) ||
                (!this.IsInventorySpaceEnough(itemStubs, true, false) ||
                 !this.GetCommissionForTake(itemStubs.Count<Asda2WhItemStub>())))
            {
                Asda2InventoryHandler.SendItemsTakedFromWarehouseResponse(this.Owner.Client,
                    PushItemToWhStatus.ItemNotFounded, (IEnumerable<Asda2WhItemStub>) null,
                    (IEnumerable<Asda2WhItemStub>) null);
            }
            else
            {
                List<Asda2WhItemStub> asda2WhItemStubList1 = new List<Asda2WhItemStub>();
                List<Asda2WhItemStub> asda2WhItemStubList2 = new List<Asda2WhItemStub>();
                foreach (Asda2WhItemStub itemStub in itemStubs)
                {
                    Asda2Item itemToCopyStats = this.GetItem(itemStub.Invtentory, itemStub.Slot);
                    Asda2Item asda2Item = (Asda2Item) null;
                    int num = (int) this.TryAdd(itemToCopyStats.ItemId, itemStub.Amount, true, ref asda2Item,
                        new Asda2InventoryType?(), itemToCopyStats);
                    itemToCopyStats.Amount -= itemStub.Amount;
                    asda2WhItemStubList1.Add(new Asda2WhItemStub()
                    {
                        Amount = itemToCopyStats.Amount,
                        Invtentory = itemToCopyStats.InventoryType,
                        Slot = itemToCopyStats.Slot
                    });
                    asda2WhItemStubList2.Add(new Asda2WhItemStub()
                    {
                        Amount = asda2Item.Amount,
                        Invtentory = asda2Item.InventoryType,
                        Slot = asda2Item.Slot
                    });
                }

                Asda2InventoryHandler.SendItemsTakedFromWarehouseResponse(this.Owner.Client, PushItemToWhStatus.Ok,
                    (IEnumerable<Asda2WhItemStub>) asda2WhItemStubList1,
                    (IEnumerable<Asda2WhItemStub>) asda2WhItemStubList2);
            }
        }

        public void TakeItemsFromAvatarWh(IEnumerable<Asda2WhItemStub> itemStubs)
        {
            if (this.IsWarehouseLocked() || !this.IsItemsExists(itemStubs) ||
                (!this.IsInventorySpaceEnough(itemStubs, true, true) ||
                 !this.GetCommissionForTake(itemStubs.Count<Asda2WhItemStub>())))
            {
                Asda2InventoryHandler.SendItemsTakedFromAvatarWarehouseResponse(this.Owner.Client,
                    PushItemToWhStatus.ItemNotFounded, (IEnumerable<Asda2WhItemStub>) null,
                    (IEnumerable<Asda2WhItemStub>) null);
            }
            else
            {
                List<Asda2WhItemStub> asda2WhItemStubList1 = new List<Asda2WhItemStub>();
                List<Asda2WhItemStub> asda2WhItemStubList2 = new List<Asda2WhItemStub>();
                foreach (Asda2WhItemStub itemStub in itemStubs)
                {
                    Asda2Item itemToCopyStats = this.GetItem(itemStub.Invtentory, itemStub.Slot);
                    Asda2Item asda2Item = (Asda2Item) null;
                    int num = (int) this.TryAdd(itemToCopyStats.ItemId, itemStub.Amount, true, ref asda2Item,
                        new Asda2InventoryType?(), itemToCopyStats);
                    itemToCopyStats.Amount -= itemStub.Amount;
                    asda2WhItemStubList1.Add(new Asda2WhItemStub()
                    {
                        Amount = itemToCopyStats.Amount,
                        Invtentory = itemToCopyStats.InventoryType,
                        Slot = itemToCopyStats.Slot
                    });
                    asda2WhItemStubList2.Add(new Asda2WhItemStub()
                    {
                        Amount = asda2Item.Amount,
                        Invtentory = asda2Item.InventoryType,
                        Slot = asda2Item.Slot
                    });
                }

                Asda2InventoryHandler.SendItemsTakedFromAvatarWarehouseResponse(this.Owner.Client,
                    PushItemToWhStatus.Ok, (IEnumerable<Asda2WhItemStub>) asda2WhItemStubList1,
                    (IEnumerable<Asda2WhItemStub>) asda2WhItemStubList2);
            }
        }

        private bool GetCommissionForTake(int count)
        {
            return this.Owner.SubtractMoney((uint) (count * 30));
        }

        private bool IsInventorySpaceEnough(IEnumerable<Asda2WhItemStub> itemStubs, bool pop, bool isAvatar)
        {
            if (pop)
            {
                if (this.FreeShopSlotsCount < itemStubs.Count<Asda2WhItemStub>())
                {
                    this.Owner.SendInfoMsg("Not enought space in shop inventory.");
                    return false;
                }

                if (this.FreeRegularSlotsCount < itemStubs.Count<Asda2WhItemStub>())
                {
                    this.Owner.SendInfoMsg("Not enought space in regular inventory.");
                    return false;
                }
            }
            else if (isAvatar)
            {
                if (this.FreeAvatarWarehouseSlotsCount < itemStubs.Count<Asda2WhItemStub>())
                {
                    this.Owner.SendInfoMsg("Not enought space in avatar warehouse.");
                    return false;
                }
            }
            else if (this.FreeWarehouseSlotsCount < itemStubs.Count<Asda2WhItemStub>())
            {
                this.Owner.SendInfoMsg("Not enought space in warehouse.");
                return false;
            }

            return true;
        }

        private bool IsItemsExists(IEnumerable<Asda2WhItemStub> itemStubs)
        {
            foreach (Asda2WhItemStub itemStub in itemStubs)
            {
                Asda2Item asda2Item = this.GetItem(itemStub.Invtentory, itemStub.Slot);
                if (asda2Item != null)
                {
                    if (asda2Item.IsDeleted)
                    {
                        this.Owner.SendErrorMsg(string.Format("Item is deleted. inv {0}, slot {1}.",
                            (object) itemStub.Invtentory, (object) itemStub.Slot));
                        return false;
                    }

                    if (asda2Item.Amount < itemStub.Amount || itemStub.Amount == 0)
                    {
                        this.Owner.SendErrorMsg(string.Format("Item amount is {0} but required {1}. inv {2}, slot {3}.",
                            (object) asda2Item.Amount, (object) itemStub.Amount, (object) itemStub.Invtentory,
                            (object) itemStub.Slot));
                        return false;
                    }

                    if (asda2Item.ItemId == 20551)
                    {
                        this.Owner.SendErrorMsg(string.Format("You cant put gold to warehouse. inv {0}, slot {1}.",
                            (object) itemStub.Invtentory, (object) itemStub.Slot));
                        return false;
                    }
                }
                else
                {
                    this.Owner.SendErrorMsg(string.Format("Item not found. inv {0}, slot {1}.",
                        (object) itemStub.Invtentory, (object) itemStub.Slot));
                    return false;
                }
            }

            return true;
        }

        private bool IsWarehouseLocked()
        {
            if (!this.Owner.IsWarehouseLocked)
                return false;
            this.Owner.SendInfoMsg(
                "Your warehouse is locked. Use <#Warehouse unlock [pass]> command to unlock it. Or use char manager.");
            return true;
        }
    }
}