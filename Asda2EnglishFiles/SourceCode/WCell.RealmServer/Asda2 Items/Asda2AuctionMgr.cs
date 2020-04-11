using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;

namespace WCell.RealmServer.Asda2_Items
{
    internal class Asda2AuctionMgr
    {
        public static Asda2AuctionMgr.Asda2AuctionItemComparer ItemsComparer =
            new Asda2AuctionMgr.Asda2AuctionItemComparer();

        public static Dictionary<int, Asda2ItemRecord> AllAuctionItems = new Dictionary<int, Asda2ItemRecord>();

        public static
            Dictionary<Asda2ItemAuctionCategory, Dictionary<AuctionLevelCriterion, SortedSet<Asda2ItemRecord>>>
            CategorizedItemsById =
                new Dictionary<Asda2ItemAuctionCategory, Dictionary<AuctionLevelCriterion, SortedSet<Asda2ItemRecord>>
                >();

        public static Dictionary<uint, List<Asda2ItemRecord>> RegularItemsByOwner =
            new Dictionary<uint, List<Asda2ItemRecord>>();

        public static Dictionary<uint, List<Asda2ItemRecord>> ShopItemsByOwner =
            new Dictionary<uint, List<Asda2ItemRecord>>();

        public static Dictionary<uint, Dictionary<int, Asda2ItemRecord>> ItemsByOwner =
            new Dictionary<uint, Dictionary<int, Asda2ItemRecord>>();

        public static Dictionary<uint, List<AuctionSelledRecord>> SelledRecords =
            new Dictionary<uint, List<AuctionSelledRecord>>();

        private static readonly List<Asda2ItemRecord> _enmptyItemList = new List<Asda2ItemRecord>();

        [WCell.Core.Initialization.Initialization(InitializationPass.Tenth, Name = "Auction System")]
        public static void Init()
        {
            foreach (Asda2ItemAuctionCategory key1 in Enum.GetValues(typeof(Asda2ItemAuctionCategory))
                .Cast<Asda2ItemAuctionCategory>())
            {
                Dictionary<AuctionLevelCriterion, SortedSet<Asda2ItemRecord>> dictionary =
                    new Dictionary<AuctionLevelCriterion, SortedSet<Asda2ItemRecord>>();
                foreach (AuctionLevelCriterion key2 in Enum.GetValues(typeof(AuctionLevelCriterion))
                    .Cast<AuctionLevelCriterion>())
                    dictionary.Add(key2,
                        new SortedSet<Asda2ItemRecord>((IComparer<Asda2ItemRecord>) Asda2AuctionMgr.ItemsComparer));
                Asda2AuctionMgr.CategorizedItemsById.Add(key1, dictionary);
            }

            foreach (Asda2ItemRecord loadAuctionedItem in Asda2ItemRecord.LoadAuctionedItems())
                Asda2AuctionMgr.RegisterItem(loadAuctionedItem);
            foreach (AuctionSelledRecord auctionSelledRecord in ActiveRecordBase<AuctionSelledRecord>.FindAll())
            {
                if (!Asda2AuctionMgr.SelledRecords.ContainsKey(auctionSelledRecord.ReciverCharacterId))
                    Asda2AuctionMgr.SelledRecords.Add(auctionSelledRecord.ReciverCharacterId,
                        new List<AuctionSelledRecord>());
                Asda2AuctionMgr.SelledRecords[auctionSelledRecord.ReciverCharacterId].Add(auctionSelledRecord);
            }
        }

        public static void OnShutdown()
        {
            foreach (ActiveRecordBase activeRecordBase in Asda2AuctionMgr.AllAuctionItems.Values)
                activeRecordBase.Save();
        }

        public static List<Asda2ItemRecord> GetCharacterRegularItems(uint charId)
        {
            if (!Asda2AuctionMgr.RegularItemsByOwner.ContainsKey(charId))
                return Asda2AuctionMgr._enmptyItemList;
            return Asda2AuctionMgr.RegularItemsByOwner[charId];
        }

        public static List<Asda2ItemRecord> GetCharacterShopItems(uint charId)
        {
            if (!Asda2AuctionMgr.ShopItemsByOwner.ContainsKey(charId))
                return Asda2AuctionMgr._enmptyItemList;
            return Asda2AuctionMgr.ShopItemsByOwner[charId];
        }

        public static void OnLogin(Character chr)
        {
            uint guid = (uint) chr.Record.Guid;
            if (!Asda2AuctionMgr.SelledRecords.ContainsKey(guid))
                return;
            List<AuctionSelledRecord> selledRecord = Asda2AuctionMgr.SelledRecords[guid];
            foreach (AuctionSelledRecord record in selledRecord)
            {
                Asda2AuctionMgr.SendMoneyToSeller(Asda2ItemMgr.GetTemplate(record.ItemId).Name, record.GoldAmount,
                    record.ItemAmount, chr);
                record.DeleteLater();
            }

            selledRecord.Clear();
            Asda2AuctionMgr.SelledRecords.Remove(guid);
        }

        public static void RegisterItem(Asda2ItemRecord item)
        {
            item.IsAuctioned = true;
            item.AuctionEndTime = DateTime.Now + TimeSpan.FromDays(7.0);
            Asda2AuctionMgr.AllAuctionItems.Add((int) item.Guid, item);
            Asda2AuctionMgr.CategorizedItemsById[item.Template.AuctionCategory][item.Template.AuctionLevelCriterion]
                .Add(item);
            Asda2AuctionMgr.CategorizedItemsById[item.Template.AuctionCategory][AuctionLevelCriterion.All].Add(item);
            if (!Asda2AuctionMgr.ItemsByOwner.ContainsKey(item.OwnerId))
                Asda2AuctionMgr.ItemsByOwner.Add(item.OwnerId, new Dictionary<int, Asda2ItemRecord>());
            Asda2AuctionMgr.ItemsByOwner[item.OwnerId].Add(item.AuctionId, item);
            if (item.Template.IsShopInventoryItem)
            {
                if (!Asda2AuctionMgr.ShopItemsByOwner.ContainsKey(item.OwnerId))
                    Asda2AuctionMgr.ShopItemsByOwner.Add(item.OwnerId, new List<Asda2ItemRecord>());
                Asda2AuctionMgr.ShopItemsByOwner[item.OwnerId].Add(item);
            }
            else
            {
                if (!Asda2AuctionMgr.RegularItemsByOwner.ContainsKey(item.OwnerId))
                    Asda2AuctionMgr.RegularItemsByOwner.Add(item.OwnerId, new List<Asda2ItemRecord>());
                Asda2AuctionMgr.RegularItemsByOwner[item.OwnerId].Add(item);
            }
        }

        public static void UnRegisterItem(Asda2ItemRecord item)
        {
            item.IsAuctioned = false;
            item.AuctionEndTime = DateTime.MinValue;
            Asda2AuctionMgr.AllAuctionItems.Remove((int) item.Guid);
            Asda2AuctionMgr.CategorizedItemsById[item.Template.AuctionCategory][item.Template.AuctionLevelCriterion]
                .Remove(item);
            Asda2AuctionMgr.CategorizedItemsById[item.Template.AuctionCategory][AuctionLevelCriterion.All].Remove(item);
            if (!Asda2AuctionMgr.ItemsByOwner.ContainsKey(item.OwnerId))
                Asda2AuctionMgr.ItemsByOwner.Add(item.OwnerId, new Dictionary<int, Asda2ItemRecord>());
            Asda2AuctionMgr.ItemsByOwner[item.OwnerId].Remove(item.AuctionId);
            if (item.Template.IsShopInventoryItem)
            {
                if (!Asda2AuctionMgr.ShopItemsByOwner.ContainsKey(item.OwnerId))
                    Asda2AuctionMgr.ShopItemsByOwner.Add(item.OwnerId, new List<Asda2ItemRecord>());
                Asda2AuctionMgr.ShopItemsByOwner[item.OwnerId].Remove(item);
            }
            else
            {
                if (!Asda2AuctionMgr.RegularItemsByOwner.ContainsKey(item.OwnerId))
                    Asda2AuctionMgr.RegularItemsByOwner.Add(item.OwnerId, new List<Asda2ItemRecord>());
                Asda2AuctionMgr.RegularItemsByOwner[item.OwnerId].Remove(item);
            }
        }

        public static void TryBuy(List<int> aucIds, Character chr)
        {
            if (aucIds.Count == 0)
                return;
            List<Asda2ItemRecord> asda2ItemRecordList = new List<Asda2ItemRecord>();
            uint amount1 = 0;
            bool? nullable1 = new bool?();
            foreach (int aucId in aucIds)
            {
                if (!Asda2AuctionMgr.AllAuctionItems.ContainsKey(aucId))
                {
                    chr.SendAuctionMsg("Can't found item you want to buy, may be some one already buy it.");
                    return;
                }

                Asda2ItemRecord allAuctionItem = Asda2AuctionMgr.AllAuctionItems[aucId];
                if (!nullable1.HasValue)
                    nullable1 = new bool?(allAuctionItem.Template.IsShopInventoryItem);
                bool? nullable2 = nullable1;
                bool shopInventoryItem = allAuctionItem.Template.IsShopInventoryItem;
                if ((nullable2.GetValueOrDefault() != shopInventoryItem ? 1 : (!nullable2.HasValue ? 1 : 0)) != 0)
                {
                    chr.YouAreFuckingCheater("Trying to buy shop\not shop item in one auction buy request.", 1);
                    chr.SendAuctionMsg("Buying from auction failed cause founded shop\not shop items in one request.");
                }

                asda2ItemRecordList.Add(allAuctionItem);
                amount1 += (uint) allAuctionItem.AuctionPrice;
            }

            if (chr.Money <= amount1)
            {
                chr.SendAuctionMsg("Failed to buy items. Not enoght money.");
            }
            else
            {
                if (nullable1.HasValue && nullable1.Value)
                {
                    if (chr.Asda2Inventory.FreeShopSlotsCount < asda2ItemRecordList.Count)
                    {
                        chr.SendAuctionMsg("Failed to buy items. Not enoght invntory space.");
                        return;
                    }
                }
                else if (chr.Asda2Inventory.FreeRegularSlotsCount < asda2ItemRecordList.Count)
                {
                    chr.SendAuctionMsg("Failed to buy items. Not enoght invntory space.");
                    return;
                }

                chr.SubtractMoney(amount1);
                List<Asda2ItemTradeRef> items = new List<Asda2ItemTradeRef>();
                foreach (Asda2ItemRecord record in asda2ItemRecordList)
                {
                    Asda2AuctionMgr.SendMoneyToSeller(record);
                    Asda2AuctionMgr.UnRegisterItem(record);
                    int amount2 = record.Amount;
                    int auctionId = record.AuctionId;
                    Asda2Item asda2Item = (Asda2Item) null;
                    Asda2Item itemToCopyStats = Asda2Item.CreateItem(record, (Character) null);
                    int num = (int) chr.Asda2Inventory.TryAdd(record.ItemId, record.Amount, true, ref asda2Item,
                        new Asda2InventoryType?(), itemToCopyStats);
                    items.Add(new Asda2ItemTradeRef()
                    {
                        Amount = amount2,
                        Item = asda2Item,
                        Price = auctionId
                    });
                    record.DeleteLater();
                }

                Asda2AuctionHandler.SendItemsBuyedFromAukResponse(chr.Client, items);
                chr.SendMoneyUpdate();
            }
        }

        private static void SendMoneyToSeller(Asda2ItemRecord item)
        {
            Character character = World.GetCharacter(item.OwnerId);
            if (character != null)
                Asda2AuctionMgr.SendMoneyToSeller(item.Template.Name, item.AuctionPrice, item.Amount, character);
            else
                new AuctionSelledRecord(item.OwnerId, item.AuctionPrice, item.Amount, item.ItemId).Create();
        }

        private static void SendMoneyToSeller(string itemName, int gold, int itemAmount, Character chr)
        {
            int num1 = (int) ((double) gold * (double) CharacterFormulas.AuctionSellComission);
            int num2 = gold - num1;
            chr.AddMoney((uint) num2);
            chr.SendMoneyUpdate();
            chr.SendAuctionMsg(string.Format("{0} {3} success solded for {1} gold. {2} comission has collected.",
                (object) itemName, (object) num2, (object) num1,
                itemAmount < 2 ? (object) "" : (object) string.Format("[{0}]", (object) itemAmount)));
        }

        public static void TryRemoveItems(Character activeCharacter, List<int> itemIds)
        {
            uint entityLowId = activeCharacter.Record.EntityLowId;
            if (!Asda2AuctionMgr.ItemsByOwner.ContainsKey(entityLowId))
            {
                activeCharacter.SendAuctionMsg("Failed to remove items from auction. Items not founded.");
            }
            else
            {
                bool? nullable1 = new bool?();
                List<Asda2ItemRecord> asda2ItemRecordList = new List<Asda2ItemRecord>();
                foreach (int itemId in itemIds)
                {
                    if (!Asda2AuctionMgr.ItemsByOwner[entityLowId].ContainsKey(itemId))
                    {
                        asda2ItemRecordList.Clear();
                        activeCharacter.SendAuctionMsg("Failed to remove items from auction. Item not founded.");
                        return;
                    }

                    Asda2ItemRecord asda2ItemRecord = Asda2AuctionMgr.ItemsByOwner[entityLowId][itemId];
                    if (!nullable1.HasValue)
                        nullable1 = new bool?(asda2ItemRecord.Template.IsShopInventoryItem);
                    bool? nullable2 = nullable1;
                    bool shopInventoryItem = asda2ItemRecord.Template.IsShopInventoryItem;
                    if ((nullable2.GetValueOrDefault() != shopInventoryItem ? 1 : (!nullable2.HasValue ? 1 : 0)) != 0)
                    {
                        activeCharacter.YouAreFuckingCheater(
                            "Trying to remove shop\not shop item in one auction buy request.", 1);
                        activeCharacter.SendAuctionMsg(
                            "Removing from auction failed cause founded shop\not shop items in one request.");
                    }

                    asda2ItemRecordList.Add(asda2ItemRecord);
                }

                if (nullable1.Value)
                {
                    if (activeCharacter.Asda2Inventory.FreeShopSlotsCount < asda2ItemRecordList.Count)
                    {
                        activeCharacter.SendAuctionMsg("Failed to delete items. Not enoght invntory space.");
                        return;
                    }
                }
                else if (activeCharacter.Asda2Inventory.FreeRegularSlotsCount < asda2ItemRecordList.Count)
                {
                    activeCharacter.SendAuctionMsg("Failed to delete items. Not enoght invntory space.");
                    return;
                }

                List<Asda2ItemTradeRef> asda2Items = new List<Asda2ItemTradeRef>();
                foreach (Asda2ItemRecord record in asda2ItemRecordList)
                {
                    Asda2AuctionMgr.UnRegisterItem(record);
                    int amount = record.Amount;
                    int auctionId = record.AuctionId;
                    Asda2Item asda2Item = (Asda2Item) null;
                    Asda2Item itemToCopyStats = Asda2Item.CreateItem(record, (Character) null);
                    int num = (int) activeCharacter.Asda2Inventory.TryAdd(record.ItemId, record.Amount, true,
                        ref asda2Item, new Asda2InventoryType?(), itemToCopyStats);
                    record.DeleteLater();
                    asda2Items.Add(new Asda2ItemTradeRef()
                    {
                        Amount = amount,
                        Item = asda2Item,
                        Price = auctionId
                    });
                    Log.Create(Log.Types.ItemOperations, LogSourceType.Character, activeCharacter.EntryId)
                        .AddAttribute("source", 0.0, "removed_from_auction").AddItemAttributes(asda2Item, "").Write();
                }

                Asda2AuctionHandler.SendItemFromAukRemovedResponse(activeCharacter.Client, asda2Items);
            }
        }

        public class Asda2AuctionItemComparer : IComparer<Asda2ItemRecord>
        {
            public int Compare(Asda2ItemRecord x, Asda2ItemRecord y)
            {
                if (x.ItemId != y.ItemId)
                    return x.ItemId.CompareTo(y.ItemId);
                int num = (x.AuctionPrice / x.Amount).CompareTo(y.AuctionPrice / y.Amount);
                if (num != 0)
                    return num;
                return x.Guid.CompareTo(y.Guid);
            }
        }
    }
}