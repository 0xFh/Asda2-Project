using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using WCell.Constants.Items;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.RealmServer.Asda2Titles;
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
        public static Asda2AuctionItemComparer ItemsComparer = new Asda2AuctionItemComparer();
        public static Dictionary<int, Asda2ItemRecord> AllAuctionItems = new Dictionary<int, Asda2ItemRecord>();

        public static
            Dictionary<Asda2ItemAuctionCategory, Dictionary<AuctionLevelCriterion, SortedSet<Asda2ItemRecord>>>
            CategorizedItemsById = new Dictionary<Asda2ItemAuctionCategory, Dictionary<AuctionLevelCriterion, SortedSet<Asda2ItemRecord>>>();

        public static Dictionary<uint, List<Asda2ItemRecord>> RegularItemsByOwner =
            new Dictionary<uint, List<Asda2ItemRecord>>();

        public static Dictionary<uint, List<Asda2ItemRecord>> ShopItemsByOwner =
            new Dictionary<uint, List<Asda2ItemRecord>>();
        public static Dictionary<uint, Dictionary<int, Asda2ItemRecord>> ItemsByOwner =
            new Dictionary<uint, Dictionary<int, Asda2ItemRecord>>();
        public static Dictionary<uint, List<AuctionSelledRecord>> SelledRecords = new Dictionary<uint, List<AuctionSelledRecord>>();
        [Initialization(InitializationPass.Tenth, Name = "Auction System")]
        public static void Init()
        {
            foreach (var cat in Enum.GetValues(typeof(Asda2ItemAuctionCategory)).Cast<Asda2ItemAuctionCategory>())
            {
                var byLevelItems = new Dictionary<AuctionLevelCriterion, SortedSet<Asda2ItemRecord>>();
                foreach (
                    var levelCriterion in Enum.GetValues(typeof(AuctionLevelCriterion)).Cast<AuctionLevelCriterion>())
                {
                    byLevelItems.Add(levelCriterion, new SortedSet<Asda2ItemRecord>(ItemsComparer));
                }
                CategorizedItemsById.Add(cat, byLevelItems);
            }
            var items = Asda2ItemRecord.LoadAuctionedItems();
            foreach (var item in items)
            {
                RegisterItem(item);
            }
            var sr = AuctionSelledRecord.FindAll();
            foreach (var rec in sr)
            {
                if (!SelledRecords.ContainsKey(rec.ReciverCharacterId))
                    SelledRecords.Add(rec.ReciverCharacterId, new List<AuctionSelledRecord>());
                SelledRecords[rec.ReciverCharacterId].Add(rec);
            }
        }

        public class Asda2AuctionItemComparer : IComparer<Asda2ItemRecord>
        {
            #region Implementation of IComparer<in Asda2ItemRecord>

            public int Compare(Asda2ItemRecord x, Asda2ItemRecord y)
            {
                if (x.ItemId == y.ItemId)
                {
                    var r = (x.AuctionPrice / x.Amount).CompareTo(y.AuctionPrice / y.Amount);

                    return r == 0 ? x.Guid.CompareTo(y.Guid) : r;
                }
                return x.ItemId.CompareTo(y.ItemId);
            }

            #endregion
        }

        public static void OnShutdown()
        {
            foreach (var item in AllAuctionItems.Values)
            {
                item.Save();
            }
        }
        private static readonly List<Asda2ItemRecord> _enmptyItemList = new List<Asda2ItemRecord>();

        public static List<Asda2ItemRecord> GetCharacterRegularItems(uint charId)
        {
            if (!RegularItemsByOwner.ContainsKey(charId))
                return _enmptyItemList;
            return RegularItemsByOwner[charId];
        }

        public static List<Asda2ItemRecord> GetCharacterShopItems(uint charId)
        {
            if (!ShopItemsByOwner.ContainsKey(charId))
                return _enmptyItemList;
            return ShopItemsByOwner[charId];
        }

        public static void OnLogin(Character chr)
        {
            var chrId = (uint)chr.Record.Guid;
            if (SelledRecords.ContainsKey(chrId))
            {
                var items = SelledRecords[chrId];
                foreach (var rec in items)
                {
                    SendMoneyToSeller(Asda2ItemMgr.GetTemplate(rec.ItemId).Name, rec.GoldAmount, rec.ItemAmount, chr);
                    rec.DeleteLater();
                }
                items.Clear();
                SelledRecords.Remove(chrId);
            }
        }
        public static void RegisterItem(Asda2ItemRecord item)
        {
            item.IsAuctioned = true;
            item.AuctionEndTime = DateTime.Now + TimeSpan.FromDays(7);
            AllAuctionItems.Add((int)item.Guid, item);
            CategorizedItemsById[item.Template.AuctionCategory][item.Template.AuctionLevelCriterion].Add(item);
            CategorizedItemsById[item.Template.AuctionCategory][AuctionLevelCriterion.All].Add(item);
            if (!ItemsByOwner.ContainsKey(item.OwnerId))
                ItemsByOwner.Add(item.OwnerId, new Dictionary<int, Asda2ItemRecord>());
            ItemsByOwner[item.OwnerId].Add(item.AuctionId, item);
            if (item.Template.IsShopInventoryItem)
            {
                if (!ShopItemsByOwner.ContainsKey(item.OwnerId))
                    ShopItemsByOwner.Add(item.OwnerId, new List<Asda2ItemRecord>());
                ShopItemsByOwner[item.OwnerId].Add(item);
            }
            else
            {
                if (!RegularItemsByOwner.ContainsKey(item.OwnerId))
                    RegularItemsByOwner.Add(item.OwnerId, new List<Asda2ItemRecord>());
                RegularItemsByOwner[item.OwnerId].Add(item);
            }
        }
        public static void UnRegisterItem(Asda2ItemRecord item)
        {
            item.IsAuctioned = false;
            item.AuctionEndTime = DateTime.MinValue;
            AllAuctionItems.Remove((int)item.Guid);
            CategorizedItemsById[item.Template.AuctionCategory][item.Template.AuctionLevelCriterion].Remove(item);
            CategorizedItemsById[item.Template.AuctionCategory][AuctionLevelCriterion.All].Remove(item);
            if (!ItemsByOwner.ContainsKey(item.OwnerId))
                ItemsByOwner.Add(item.OwnerId, new Dictionary<int, Asda2ItemRecord>());
            ItemsByOwner[item.OwnerId].Remove(item.AuctionId);
            if (item.Template.IsShopInventoryItem)
            {
                if (!ShopItemsByOwner.ContainsKey(item.OwnerId))
                    ShopItemsByOwner.Add(item.OwnerId, new List<Asda2ItemRecord>());
                ShopItemsByOwner[item.OwnerId].Remove(item);
            }
            else
            {
                if (!RegularItemsByOwner.ContainsKey(item.OwnerId))
                    RegularItemsByOwner.Add(item.OwnerId, new List<Asda2ItemRecord>());
                RegularItemsByOwner[item.OwnerId].Remove(item);
            }
        }
        public static void TryBuy(List<int> aucIds, Character chr)
        {
            if (aucIds.Count == 0)
                return;
            var itemsToBuy = new List<Asda2ItemRecord>();
            var totalPrice = 0u;
            bool? isShopItems = null;
            foreach (var aucId in aucIds)
            {
                if (!AllAuctionItems.ContainsKey(aucId))
                {
                    chr.SendAuctionMsg("Can't found item you want to buy, may be some one already buy it.");
                    return;
                }
                var item = AllAuctionItems[aucId];
                if (isShopItems == null)
                    isShopItems = item.Template.IsShopInventoryItem;
                if (isShopItems != item.Template.IsShopInventoryItem)
                {
                    chr.YouAreFuckingCheater("Trying to buy shop\not shop item in one auction buy request.");
                    chr.SendAuctionMsg("Buying from auction failed cause founded shop\not shop items in one request.");
                }
                itemsToBuy.Add(item);
                totalPrice += (uint)item.AuctionPrice;
            }
            if (chr.Money <= totalPrice)
            {
                chr.SendAuctionMsg("Failed to buy items. Not enoght money.");
                return;
            }
            if (isShopItems != null && isShopItems.Value)
            {
                if (chr.Asda2Inventory.FreeShopSlotsCount < itemsToBuy.Count)
                {
                    chr.SendAuctionMsg("Failed to buy items. Not enoght invntory space.");
                    return;
                }
            }
            else
            {
                if (chr.Asda2Inventory.FreeRegularSlotsCount < itemsToBuy.Count)
                {
                    chr.SendAuctionMsg("Failed to buy items. Not enoght invntory space.");
                    return;
                }
            }
            chr.SubtractMoney(totalPrice);
            var r = new List<Asda2ItemTradeRef>();
            foreach (var itemRec in itemsToBuy)
            {
                SendMoneyToSeller(itemRec);
                UnRegisterItem(itemRec);
                var amount = itemRec.Amount;
                var auctionId = itemRec.AuctionId;
                Asda2Item addedItem = null;
                var item = Asda2Item.CreateItem(itemRec, (Character)null);
                chr.Asda2Inventory.TryAdd(itemRec.ItemId, itemRec.Amount, true, ref addedItem, null, item);
                r.Add(new Asda2ItemTradeRef { Amount = amount, Item = addedItem, Price = auctionId });
                itemRec.DeleteLater();
            }
            Asda2AuctionHandler.SendItemsBuyedFromAukResponse(chr.Client, r);
            chr.SendMoneyUpdate();
        }

        private static void SendMoneyToSeller(Asda2ItemRecord item)
        {
            var chr = World.GetCharacter(item.OwnerId);
            if (chr != null)
            {
                SendMoneyToSeller(item.Template.Name, item.AuctionPrice, item.Amount, chr);
            }
            else
            {
                var newRec = new AuctionSelledRecord(item.OwnerId, item.AuctionPrice, item.Amount, item.ItemId);
                newRec.Create();
            }
        }
        static void SendMoneyToSeller(string itemName, int gold, int itemAmount, Character chr)
        {
            var comission = (int)(gold * CharacterFormulas.AuctionSellComission);
            var goldToOwner = gold - comission;
            chr.AddMoney((uint)goldToOwner);
            chr.SendMoneyUpdate();
            chr.SendAuctionMsg(string.Format("{0} {3} success solded for {1} gold. {2} comission has collected.", itemName, goldToOwner, comission, itemAmount < 2 ? "" : string.Format("[{0}]", itemAmount)));

            Asda2TitleChecker.OnAuctionItemSold(chr);
        }

        public static void TryRemoveItems(Character activeCharacter, List<int> itemIds)
        {
            var chrId = activeCharacter.Record.EntityLowId;
            if (!ItemsByOwner.ContainsKey(chrId))
            {
                activeCharacter.SendAuctionMsg("Failed to remove items from auction. Items not founded.");
                return;
            }
            bool? isShopItems = null;
            var items = new List<Asda2ItemRecord>();
            foreach (var itemId in itemIds)
            {
                if (!ItemsByOwner[chrId].ContainsKey(itemId))
                {
                    items.Clear();
                    activeCharacter.SendAuctionMsg("Failed to remove items from auction. Item not founded.");
                    return;
                }
                var item = ItemsByOwner[chrId][itemId];
                if (isShopItems == null)
                    isShopItems = item.Template.IsShopInventoryItem;
                if (isShopItems != item.Template.IsShopInventoryItem)
                {
                    activeCharacter.YouAreFuckingCheater("Trying to remove shop\not shop item in one auction buy request.");
                    activeCharacter.SendAuctionMsg("Removing from auction failed cause founded shop\not shop items in one request.");
                }
                items.Add(item);
            }
            if ((bool)isShopItems)
            {
                if (activeCharacter.Asda2Inventory.FreeShopSlotsCount < items.Count)
                {
                    activeCharacter.SendAuctionMsg("Failed to delete items. Not enoght invntory space.");
                    return;
                }
            }
            else
            {
                if (activeCharacter.Asda2Inventory.FreeRegularSlotsCount < items.Count)
                {
                    activeCharacter.SendAuctionMsg("Failed to delete items. Not enoght invntory space.");
                    return;
                }
            }
            var r = new List<Asda2ItemTradeRef>();
            foreach (var rec in items)
            {
                UnRegisterItem(rec);
                var amount = rec.Amount;
                var aucId = rec.AuctionId;
                Asda2Item item = null;
                var aucItem = Asda2Item.CreateItem(rec, (Character) null);
                activeCharacter.Asda2Inventory.TryAdd(rec.ItemId, rec.Amount, true, ref item,null,aucItem);
                rec.DeleteLater();
                r.Add(new Asda2ItemTradeRef { Amount = amount, Item = item, Price = aucId });
                Log.Create(Log.Types.ItemOperations, LogSourceType.Character, activeCharacter.EntryId)
                           .AddAttribute("source", 0, "removed_from_auction")
                           .AddItemAttributes(item)
                           .Write();
            }
            Asda2AuctionHandler.SendItemFromAukRemovedResponse(activeCharacter.Client, r);
        }
    }
    [ActiveRecord("AuctionSelledRecord", Access = PropertyAccess.Property)]
    public class AuctionSelledRecord : WCellRecord<AuctionSelledRecord>
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(AuctionSelledRecord), "Guid");

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid
        {
            get;
            set;
        }
        [Property]
        public uint ReciverCharacterId { get; set; }
        [Property]
        public int GoldAmount { get; set; }
        [Property]
        public int ItemAmount { get; set; }
        [Property]
        public int ItemId { get; set; }
        public AuctionSelledRecord() { }
        public AuctionSelledRecord(uint recieverCharId, int goldAmount, int itemAmount, int itemId)
        {
            Guid = _idGenerator.Next();
            ReciverCharacterId = recieverCharId;
            GoldAmount = goldAmount;
            ItemAmount = itemAmount;
            ItemId = itemId;
        }
    }
}


