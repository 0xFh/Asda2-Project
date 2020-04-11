using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Handlers
{
    internal class Asda2InventoryHandler
    {
        private static readonly byte[] Stab31 = new byte[3];
        private static readonly byte[] stab31 = new byte[3];

        private static readonly byte[] stab16 = new byte[50]
        {
            (byte) 7,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] unk6 = new byte[6]
        {
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue
        };

        [PacketHandler(RealmServerOpCode.ReplaceItem)]
        public static void ReplaceItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            short num = packet.ReadInt16();
            packet.Position += 2;
            byte srcInv = packet.ReadByte();
            int srcQuant = packet.ReadInt32();
            short srcWeight = packet.ReadInt16();
            short destSlot = packet.ReadInt16();
            packet.Position += 2;
            byte destInv = packet.ReadByte();
            int destQuant = packet.ReadInt32();
            short destWeight = packet.ReadInt16();
            if (srcInv == (byte) 0)
                srcInv = (byte) 3;
            Asda2InventoryError status = client.ActiveCharacter.Asda2Inventory.TrySwap((Asda2InventoryType) srcInv, num,
                (Asda2InventoryType) destInv, ref destSlot);
            if (status == Asda2InventoryError.Ok)
                return;
            Asda2InventoryHandler.SendItemReplacedResponse(client, status, num, srcInv, srcQuant, srcWeight,
                (int) destSlot, destInv, destQuant, destWeight, false);
        }

        public static void SendItemReplacedResponse(IRealmClient client,
            Asda2InventoryError status = Asda2InventoryError.NotInfoAboutItem, short srcCell = 0, byte srcInv = 0,
            int srcQuant = 0, short srcWeight = 0, int destCell = 0, byte destInv = 0, int destQuant = 0,
            short destWeight = 0, bool secondItemIsNullNow = false)
        {
            if (destInv == (byte) 0 || destInv == (byte) 3)
            {
                if (client.ActiveCharacter.Asda2Inventory.Equipment[0] != null &&
                    client.ActiveCharacter.Asda2Inventory.Equipment[1] != null &&
                    (client.ActiveCharacter.Asda2Inventory.Equipment[2] != null &&
                     client.ActiveCharacter.Asda2Inventory.Equipment[3] != null) &&
                    (client.ActiveCharacter.Asda2Inventory.Equipment[4] != null &&
                     client.ActiveCharacter.Asda2Inventory.Equipment[0].Template.Quality == Asda2ItemQuality.Green &&
                     (client.ActiveCharacter.Asda2Inventory.Equipment[1].Template.Quality == Asda2ItemQuality.Green &&
                      client.ActiveCharacter.Asda2Inventory.Equipment[2].Template.Quality == Asda2ItemQuality.Green)) &&
                    (client.ActiveCharacter.Asda2Inventory.Equipment[3].Template.Quality == Asda2ItemQuality.Green &&
                     client.ActiveCharacter.Asda2Inventory.Equipment[4].Template.Quality == Asda2ItemQuality.Green))
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Superior238);
                if (client.ActiveCharacter.Asda2Inventory.Equipment[11] != null &&
                    client.ActiveCharacter.Asda2Inventory.Equipment[12] != null &&
                    (client.ActiveCharacter.Asda2Inventory.Equipment[13] != null &&
                     client.ActiveCharacter.Asda2Inventory.Equipment[14] != null) &&
                    (client.ActiveCharacter.Asda2Inventory.Equipment[15] != null &&
                     client.ActiveCharacter.Asda2Inventory.Equipment[19] != null &&
                     client.ActiveCharacter.Asda2Inventory.Equipment[16] != null))
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Stylish239);
            }

            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemReplaced))
            {
                packet.WriteByte((byte) status);
                packet.WriteByte(secondItemIsNullNow ? 0 : 1);
                packet.WriteInt16(srcCell);
                packet.WriteInt16(secondItemIsNullNow ? -1 : 0);
                packet.WriteByte(srcInv);
                packet.WriteInt32(srcQuant);
                packet.WriteInt16(srcWeight);
                packet.WriteInt16(destCell);
                packet.WriteInt16(0);
                packet.WriteByte(destInv);
                packet.WriteInt32(destQuant);
                packet.WriteInt16(destWeight);
                client.Send(packet, true);
            }
        }

        public static void SendCharacterAddEquipmentResponse(Character chr, short slot, int itemId, int upgrade)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterAddEquipment))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt32(-1);
                packet.WriteInt16(slot);
                packet.WriteInt32(itemId);
                packet.WriteByte(upgrade);
                chr.SendPacketToArea(packet, false, false, Locale.Any, new float?());
            }
        }

        public static void SendCharacterRemoveEquipmentResponse(Character chr, short slot, int itemId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterRemoveEquipment))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt32(-1);
                packet.WriteInt16(slot);
                packet.WriteInt32(itemId);
                packet.WriteInt32(0);
                chr.SendPacketToArea(packet, false, true, Locale.Any, new float?());
            }
        }

        [PacketHandler(RealmServerOpCode.SetFastItemSlot)]
        public static void SetFastItemSlotRequest(IRealmClient client, RealmPacketIn packet)
        {
            byte panel = packet.ReadByte();
            if (panel > (byte) 5)
                return;
            Dictionary<byte, Asda2FastItemSlotRecord[]> fastItemSlotRecords =
                client.ActiveCharacter.Asda2Inventory.FastItemSlotRecords;
            for (byte panelSlot = 0; panelSlot < (byte) 12; ++panelSlot)
            {
                byte srcInfo = packet.ReadByte();
                byte num1 = packet.ReadByte();
                short num2 = packet.ReadInt16();
                int amount = packet.ReadInt32();
                short itemOrSkillId = packet.ReadInt16();
                if (fastItemSlotRecords[panel][(int) panelSlot] != null)
                    fastItemSlotRecords[panel][(int) panelSlot].DeleteLater();
                fastItemSlotRecords[panel][(int) panelSlot] =
                    srcInfo != (byte) 0 || num1 != (byte) 0 || (num2 != (short) -1 || amount != 0) ||
                    itemOrSkillId != (short) -1
                        ? Asda2FastItemSlotRecord.CreateRecord(panel, panelSlot, (Asda2InventoryType) num1, (byte) num2,
                            itemOrSkillId, amount, client.ActiveCharacter.EntityId.Low, srcInfo)
                        : (Asda2FastItemSlotRecord) null;
            }
        }

        public static void SendAllFastItemSlotsInfo(Character character)
        {
            IRealmClient client = character.Client;
            if (!client.IsGameServerConnection)
                return;
            foreach (KeyValuePair<byte, Asda2FastItemSlotRecord[]> keyValuePair in character.Asda2Inventory
                .FastItemSlotRecords.Where<KeyValuePair<byte, Asda2FastItemSlotRecord[]>>(
                    (Func<KeyValuePair<byte, Asda2FastItemSlotRecord[]>, bool>) (kvp =>
                        ((IEnumerable<Asda2FastItemSlotRecord>) kvp.Value).Any<Asda2FastItemSlotRecord>(
                            (Func<Asda2FastItemSlotRecord, bool>) (r => r != null)))))
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FastItemSlotsInfo))
                {
                    packet.WriteByte(keyValuePair.Key);
                    foreach (Asda2FastItemSlotRecord fastItemSlotRecord in keyValuePair.Value)
                    {
                        packet.WriteByte(fastItemSlotRecord == null ? 0 : (int) fastItemSlotRecord.SrcInfo);
                        packet.WriteByte(fastItemSlotRecord == null ? 0 : (int) fastItemSlotRecord.InventoryType);
                        packet.WriteInt16(fastItemSlotRecord == null ? -1 : (int) fastItemSlotRecord.InventorySlot);
                        packet.WriteInt32(fastItemSlotRecord == null ? 0 : fastItemSlotRecord.Amount);
                        packet.WriteInt16(fastItemSlotRecord == null ? -1 : fastItemSlotRecord.ItemOrSkillId);
                    }

                    client.Send(packet, true);
                }
            }
        }

        [PacketHandler(RealmServerOpCode.UseItem)]
        public static void UseItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            try
            {
                Character activeCharacter = client.ActiveCharacter;
                packet.Position += 4;
                byte num1 = packet.ReadByte();
                int num2 = packet.ReadInt32();
                if (num1 != (byte) 2 || num2 < 0 || num2 > 69)
                    client.ActiveCharacter.SendInfoMsg("You must update your client to use items!");
                else
                    activeCharacter.Asda2Inventory.UseItem(Asda2InventoryType.Regular, (byte) num2);
            }
            catch (Exception ex)
            {
                client.ActiveCharacter.SendInfoMsg("You must update your client to use items!");
            }
        }

        public static void SendCharUsedItemResponse(UseItemResult status, Character chr, Asda2Item item)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharUsedItem))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt32(chr.Account.AccountId);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, false);
                if (status != UseItemResult.Ok)
                {
                    chr.Client.Send(packet, true);
                }
                else
                {
                    chr.SendPacketToArea(packet, true, true, Locale.Any, new float?());
                    chr.Client.Send(packet, true);
                }
            }
        }

        [PacketHandler(RealmServerOpCode.RemoveItem)]
        public static void RemoveItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 4;
            byte inv = packet.ReadByte();
            short num1 = packet.ReadInt16();
            short num2 = packet.ReadInt16();
            client.ActiveCharacter.Asda2Inventory.RemoveItem((int) num1, inv, (int) num2);
        }

        public static void ItemRemovedFromInventoryResponse(Character chr, Asda2Item item,
            DeleteOrSellItemStatus status, int amountDeleted = 0)
        {
            AchievementProgressRecord progressRecord = chr.Achievements.GetOrCreateProgressRecord(94U);
            switch (++progressRecord.Counter)
            {
                case 500:
                    chr.DiscoverTitle(Asda2TitleId.Trash227);
                    break;
                case 3000:
                    chr.GetTitle(Asda2TitleId.Trash227);
                    break;
            }

            progressRecord.SaveAndFlush();
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemRemovedFromInventory))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(chr.Money);
                packet.WriteInt16(1233);
                packet.WriteInt16(amountDeleted);
                packet.WriteInt16(0);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, false);
                chr.Client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.SellItem)]
        public static void SellItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            ItemStub[] itemStubs = new ItemStub[5];
            for (int index = 0; index < 5; ++index)
            {
                short num1 = packet.ReadInt16();
                packet.Position += 2;
                byte num2 = packet.ReadByte();
                int num3 = packet.ReadInt32();
                int num4 = (int) packet.ReadInt16();
                itemStubs[index] = new ItemStub()
                {
                    Cell = (int) num1,
                    Inv = (Asda2InventoryType) num2,
                    Amount = num3
                };
            }

            client.ActiveCharacter.Asda2Inventory.SellItems(itemStubs);
        }

        public static void SendSellItemResponseResponse(DeleteOrSellItemStatus status, Character chr,
            List<Asda2Item> items)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SellItemResponse))
            {
                packet.WriteByte((byte) status);
                for (int index = 0; index < 5; ++index)
                {
                    Asda2Item asda2Item = items[index];
                    packet.WriteInt32(asda2Item == null ? 0 : (int) asda2Item.Slot);
                    packet.WriteByte(asda2Item == null ? (byte) 0 : (byte) asda2Item.InventoryType);
                    packet.WriteInt32(asda2Item == null ? 0 : asda2Item.Amount);
                    packet.WriteInt16(asda2Item == null ? 0 : (int) asda2Item.Weight);
                }

                packet.WriteInt32(chr.Money);
                packet.WriteInt16(chr.Asda2Inventory.Weight);
                chr.Client.Send(packet, true);
            }
        }

        public static void SendBuyItemResponse(Asda2BuyItemStatus status, Character chr, Asda2Item[] items)
        {
            if (items.Length != 7)
            {
                Asda2Item[] asda2ItemArray = items;
                items = new Asda2Item[7];
                Array.Copy((Array) asda2ItemArray, (Array) items,
                    asda2ItemArray.Length <= 7 ? asda2ItemArray.Length : 7);
            }

            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.BuyItemResponse))
            {
                packet.WriteByte((byte) status);
                for (int index = 0; index < 7; ++index)
                {
                    Asda2Item asda2Item = items[index];
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                }

                packet.WriteInt32(chr.Money);
                packet.WriteInt16(chr.Asda2Inventory.Weight);
                chr.Client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.BuyItem)]
        public static void BuyItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            List<ItemStub> itemStubs = new List<ItemStub>();
            for (int index = 0; index < 7; ++index)
            {
                ushort num1 = packet.ReadUInt16();
                int num2 = (int) packet.ReadInt16();
                short num3 = packet.ReadInt16();
                itemStubs.Add(new ItemStub()
                {
                    Amount = (int) num3,
                    ItemId = (int) num1
                });
            }

            client.ActiveCharacter.Asda2Inventory.BuyItems(itemStubs);
        }

        [PacketHandler(RealmServerOpCode.PickUpItem)]
        public static void PickUpItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = (int) packet.ReadInt16();
            int num2 = (int) packet.ReadInt16();
            short x = packet.ReadInt16();
            short y = packet.ReadInt16();
            client.ActiveCharacter.Asda2Inventory.TryPickUpItem(x, y);
        }

        public static void UpdateItemInventoryInfo(IRealmClient client, Asda2Item item)
        {
            if (item.InventoryType == Asda2InventoryType.Regular)
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.RegularInventoryInfo))
                {
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, false);
                    client.Send(packet, false);
                }
            }
            else
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ShopInventoryInfo))
                {
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, false);
                    client.Send(packet, false);
                }
            }
        }

        public static void SendItemPickupedResponse(Asda2PickUpItemStatus status, Asda2Item item, Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemPickuped))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(-1);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, false);
                packet.WriteInt16(chr.Asda2Inventory.Weight);
                chr.Client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.UpgradeItemRequest)]
        public static void UpgradeItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadInt32();
            int num1 = (int) packet.ReadByte();
            short itemCell = packet.ReadInt16();
            int num2 = (int) packet.ReadByte();
            short stoneCell = packet.ReadInt16();
            int num3 = (int) packet.ReadByte();
            short chanceBoostCell = packet.ReadInt16();
            int num4 = (int) packet.ReadByte();
            short protectScrollCell = packet.ReadInt16();
            client.ActiveCharacter.Asda2Inventory.UpgradeItem(itemCell, stoneCell, chanceBoostCell, protectScrollCell);
        }

        public static void SendUpgradeItemResponse(IRealmClient client, UpgradeItemStatus status,
            Asda2Item upgradedItem, Asda2Item stone, Asda2Item successItem, Asda2Item protectionItem,
            int inventoryWeight, uint money)
        {
            if (upgradedItem != null)
            {
                if (upgradedItem.Enchant == (byte) 5)
                {
                    AchievementProgressRecord progressRecord =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(105U);
                    switch (++progressRecord.Counter)
                    {
                        case 50:
                            client.ActiveCharacter.Map.CallDelayed(500,
                                (Action) (() => client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Upgraded255)));
                            break;
                        case 100:
                            client.ActiveCharacter.Map.CallDelayed(500,
                                (Action) (() => client.ActiveCharacter.GetTitle(Asda2TitleId.Upgraded255)));
                            break;
                    }

                    progressRecord.SaveAndFlush();
                }

                if (upgradedItem.Enchant == (byte) 10)
                {
                    AchievementProgressRecord progressRecord =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(185U);
                    switch (++progressRecord.Counter)
                    {
                        case 25:
                            client.ActiveCharacter.Map.CallDelayed(500,
                                (Action) (() => client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Elite256)));
                            break;
                        case 50:
                            client.ActiveCharacter.Map.CallDelayed(500,
                                (Action) (() => client.ActiveCharacter.GetTitle(Asda2TitleId.Elite256)));
                            break;
                    }

                    progressRecord.SaveAndFlush();
                }

                if (upgradedItem.Enchant == (byte) 20)
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Absolute257);
            }

            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpgradeItemResponse))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                packet.WriteInt32(client.ActiveCharacter.Money);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, upgradedItem, false);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, stone, false);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, successItem, false);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, protectionItem, false);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.ExchangeOption)]
        public static void ExchangeOptionRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadInt32();
            short scrollCell = packet.ReadInt16();
            int num = (int) packet.ReadInt16();
            packet.ReadInt32();
            short itemSlot = packet.ReadInt16();
            client.ActiveCharacter.Asda2Inventory.ExchangeItemOptions(scrollCell, itemSlot);
        }

        public static void SendExchangeItemOptionResultResponse(IRealmClient client, ExchangeOptionResult status,
            Asda2Item item, Asda2Item exchangeToken)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ExchangeItemOptionResult))
            {
                packet.WriteByte((byte) status);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, exchangeToken, false);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, false);
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                packet.WriteInt16(0);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.MoveSoulStoneIn)]
        public static void MoveSoulStoneInRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadInt32();
            int num1 = (int) packet.ReadByte();
            short itemCell = packet.ReadInt16();
            byte sowelSlot = packet.ReadByte();
            int num2 = (int) packet.ReadInt16();
            int num3 = (int) packet.ReadByte();
            short sowelCell = packet.ReadInt16();
            int num4 = (int) packet.ReadInt16();
            int num5 = (int) packet.ReadByte();
            short protectSlot = packet.ReadInt16();
            client.ActiveCharacter.Asda2Inventory.SowelItem(itemCell, sowelCell, sowelSlot, protectSlot, false);
        }

        public static void SendItemSoweledResponse(IRealmClient client, int inventoryWeight, int money,
            SowelingStatus status, Asda2Item item, Asda2Item stone, Asda2Item protect, bool isAvatar = false)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut(isAvatar ? RealmServerOpCode.AvatarSocketed : RealmServerOpCode.ItemSoweled))
            {
                packet.WriteByte((byte) status);
                if (isAvatar)
                    packet.WriteInt16(0);
                packet.WriteInt32(inventoryWeight);
                packet.WriteInt32(money);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, false);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, stone, false);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, protect, false);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.SoketAvatar)]
        public static void SoketAvatarRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadInt32();
            short itemCell = packet.ReadInt16();
            byte sowelSlot = packet.ReadByte();
            int num = (int) packet.ReadInt16();
            short sowelCell = packet.ReadInt16();
            client.ActiveCharacter.Asda2Inventory.SowelItem(itemCell, sowelCell, sowelSlot, (short) -1, true);
        }

        public static void WriteItemInfoToPacket(RealmPacketOut packet, Asda2Item item,
            bool setAmountTo0WhenDeleted = false)
        {
            packet.WriteInt32(item == null ? 0 : item.ItemId);
            packet.WriteByte(item == null ? (byte) 0 : (byte) item.InventoryType);
            packet.WriteInt16(item == null ? -1 : (int) item.Slot);
            packet.WriteInt16(item == null ? -1 : (item.IsDeleted ? -1 : 0));
            packet.WriteItemAmount(item, false);
            packet.WriteByte(item == null ? -1 : (int) item.Durability);
            packet.WriteInt16(item == null ? -1 : (int) item.Weight);
            packet.WriteInt16(item == null ? -1 : item.Soul1Id);
            packet.WriteInt16(item == null ? -1 : item.Soul2Id);
            packet.WriteInt16(item == null ? -1 : item.Soul3Id);
            packet.WriteInt16(item == null ? -1 : item.Soul4Id);
            packet.WriteByte(item == null ? -1 : (int) item.Enchant);
            packet.WriteSkip(Asda2InventoryHandler.Stab31);
            packet.WriteByte(item == null ? -1 : (int) item.SealCount);
            packet.WriteInt16(item == null ? -1 : (int) (short) item.Parametr1Type);
            packet.WriteInt16(item == null ? -1 : (int) item.Parametr1Value);
            packet.WriteInt16(item == null ? -1 : (int) (short) item.Parametr2Type);
            packet.WriteInt16(item == null ? -1 : (int) item.Parametr2Value);
            packet.WriteInt16(item == null ? -1 : (int) (short) item.Parametr3Type);
            packet.WriteInt16(item == null ? -1 : (int) item.Parametr3Value);
            packet.WriteInt16(item == null ? -1 : (int) (short) item.Parametr4Type);
            if (item != null && item.Template.IsAvatar)
            {
                Asda2ItemTemplate template1 = Asda2ItemMgr.GetTemplate(item.Soul2Id);
                Asda2ItemTemplate template2 = Asda2ItemMgr.GetTemplate(item.Soul3Id);
                packet.WriteInt16(template1 == null ? -1 : template1.SowelBonusValue);
                packet.WriteInt16(-1);
                packet.WriteInt16(template2 == null ? -1 : template2.SowelBonusValue);
            }
            else
            {
                packet.WriteInt16(item == null ? -1 : (int) item.Parametr4Value);
                packet.WriteInt16(item == null ? -1 : (int) (short) item.Parametr5Type);
                packet.WriteInt16(item == null ? -1 : (int) item.Parametr5Value);
            }

            packet.WriteByte(0);
            packet.WriteByte(item == null ? -1 : (item.IsSoulbound ? 1 : 0));
            packet.WriteInt32(0);
            packet.WriteInt16(0);
        }

        [PacketHandler(RealmServerOpCode.MoveSoulStoneOut)]
        public static void MoveSoulStoneOutRequest(IRealmClient client, RealmPacketIn packet)
        {
            byte num1 = packet.ReadByte();
            packet.Position += 5;
            short slotInq1 = packet.ReadInt16();
            packet.Position += 4;
            short slotInq2 = packet.ReadInt16();
            Asda2Item shopShopItem1 = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq1);
            if (shopShopItem1 == null || !shopShopItem1.Template.IsEquipment && !shopShopItem1.Template.IsAvatar)
            {
                client.ActiveCharacter.SendInfoMsg("Item not found. Restart game please.");
                Asda2InventoryHandler.SendSowelRemovedResponse(client, SowelRemovedStatus.Fail, (Asda2Item) null,
                    (Asda2Item) null, (Asda2Item) null);
            }
            else
            {
                Asda2Item shopShopItem2 = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq2);
                if (shopShopItem2 == null || shopShopItem2.ItemId != 576 && shopShopItem2.ItemId != 577 &&
                    (shopShopItem2.ItemId != 578 && shopShopItem2.ItemId != 598))
                {
                    client.ActiveCharacter.SendInfoMsg("Item not found. Restart game please.");
                    Asda2InventoryHandler.SendSowelRemovedResponse(client, SowelRemovedStatus.Fail, (Asda2Item) null,
                        (Asda2Item) null, (Asda2Item) null);
                }
                else if (client.ActiveCharacter.Asda2Inventory.FreeRegularSlotsCount < 1)
                {
                    client.ActiveCharacter.SendInfoMsg("Not enought inventory space.");
                    Asda2InventoryHandler.SendSowelRemovedResponse(client, SowelRemovedStatus.Fail, (Asda2Item) null,
                        (Asda2Item) null, (Asda2Item) null);
                }
                else
                {
                    Asda2Item sowel = (Asda2Item) null;
                    LogHelperEntry lgDelete1 = Log
                        .Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                        .AddAttribute("source", 0.0, "remove_sowel_source_item").AddItemAttributes(shopShopItem1, "")
                        .Write();
                    LogHelperEntry lgDelete2 = Log
                        .Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                        .AddAttribute("source", 0.0, "remove_sowel_removal_item").AddItemAttributes(shopShopItem2, "")
                        .Write();
                    switch (num1)
                    {
                        case 2:
                            if (shopShopItem1.Soul2Id == 0)
                            {
                                client.ActiveCharacter.YouAreFuckingCheater("Sowel not found.", 50);
                                Asda2InventoryHandler.SendSowelRemovedResponse(client, SowelRemovedStatus.Fail,
                                    (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
                                return;
                            }

                            int num2 = (int) client.ActiveCharacter.Asda2Inventory.TryAdd(shopShopItem1.Soul2Id, 1,
                                true, ref sowel, new Asda2InventoryType?(), (Asda2Item) null);
                            shopShopItem1.Soul2Id = 0;
                            break;
                        case 3:
                            if (shopShopItem1.Soul3Id == 0)
                            {
                                client.ActiveCharacter.YouAreFuckingCheater("Sowel not found.", 50);
                                Asda2InventoryHandler.SendSowelRemovedResponse(client, SowelRemovedStatus.Fail,
                                    (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
                                return;
                            }

                            int num3 = (int) client.ActiveCharacter.Asda2Inventory.TryAdd(shopShopItem1.Soul3Id, 1,
                                true, ref sowel, new Asda2InventoryType?(), (Asda2Item) null);
                            shopShopItem1.Soul3Id = 0;
                            break;
                        case 4:
                            if (shopShopItem1.Soul4Id == 0)
                            {
                                client.ActiveCharacter.YouAreFuckingCheater("Sowel not found.", 50);
                                Asda2InventoryHandler.SendSowelRemovedResponse(client, SowelRemovedStatus.Fail,
                                    (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
                                return;
                            }

                            int num4 = (int) client.ActiveCharacter.Asda2Inventory.TryAdd(shopShopItem1.Soul4Id, 1,
                                true, ref sowel, new Asda2InventoryType?(), (Asda2Item) null);
                            shopShopItem1.Soul4Id = 0;
                            break;
                        default:
                            client.ActiveCharacter.YouAreFuckingCheater("Sowel not found.", 50);
                            Asda2InventoryHandler.SendSowelRemovedResponse(client, SowelRemovedStatus.Fail,
                                (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
                            return;
                    }

                    --shopShopItem2.Amount;
                    shopShopItem2.Save();
                    shopShopItem1.Save();
                    Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                        .AddAttribute("source", 0.0, "remove_sowel_new_item").AddItemAttributes(sowel, "")
                        .AddReference(lgDelete2).AddReference(lgDelete1).Write();
                    Asda2InventoryHandler.SendSowelRemovedResponse(client, SowelRemovedStatus.Ok, shopShopItem1, sowel,
                        shopShopItem2);
                }
            }
        }

        public static void SendSowelRemovedResponse(IRealmClient client, SowelRemovedStatus status,
            Asda2Item mainItem = null, Asda2Item sowel = null, Asda2Item tool = null)
        {
            Asda2Item[] asda2ItemArray = new Asda2Item[3]
            {
                mainItem,
                sowel,
                tool
            };
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SowelRemoved))
            {
                packet.WriteByte((byte) status);
                for (int index = 0; index < 3; ++index)
                {
                    Asda2Item asda2Item = asda2ItemArray[index];
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                }

                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.OpenBooster)]
        public static void OpenBoosterRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadInt32();
            Asda2InventoryType inv = (Asda2InventoryType) packet.ReadByte();
            short cell = packet.ReadInt16();
            OpenBosterStatus status = client.ActiveCharacter.Asda2Inventory.OpenBooster(inv, cell);
            client.ActiveCharacter.Map.AddMessage((Action) (() =>
            {
                if (status == OpenBosterStatus.Ok)
                    return;
                Asda2InventoryHandler.SendbosterOpenedResponse(client, status, (Asda2Item) null, inv, cell, (short) 0);
            }));
        }

        public static void SendbosterOpenedResponse(IRealmClient client, OpenBosterStatus status, Asda2Item addedItem,
            Asda2InventoryType boosterInv, short boosterCell, short weight)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.bosterOpened))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(11);
                packet.WriteInt16(119);
                packet.WriteByte(0);
                packet.WriteByte((byte) boosterInv);
                packet.WriteInt16(boosterCell);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, addedItem, false);
                packet.WriteInt16(weight);
                if (status == OpenBosterStatus.Ok)
                    client.ActiveCharacter.Send(packet, true);
                else
                    client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.OpenPackage)]
        public static void OpenPackageRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 3;
            Asda2InventoryType packageInv = (Asda2InventoryType) packet.ReadByte();
            short packageSlot = packet.ReadInt16();
            OpenPackageStatus result = client.ActiveCharacter.Asda2Inventory.OpenPackage(packageInv, packageSlot);
            client.ActiveCharacter.Map.AddMessage((Action) (() =>
            {
                if (result == OpenPackageStatus.Ok)
                    return;
                Asda2InventoryHandler.SendOpenPackageResponseResponse(client, result, (List<Asda2Item>) null,
                    packageInv, packageSlot, (short) 0);
            }));
        }

        public static void SendOpenPackageResponseResponse(IRealmClient client, OpenPackageStatus status,
            List<Asda2Item> addedItems, Asda2InventoryType packageInv, short packageSlot, short weight)
        {
            int val = addedItems == null ? 0 : addedItems.Count;
            Asda2Item[] asda2ItemArray = new Asda2Item[5];
            int index = 0;
            if (addedItems != null)
            {
                foreach (Asda2Item addedItem in addedItems)
                {
                    asda2ItemArray[index] = addedItem;
                    ++index;
                }
            }

            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.OpenPackageResponse))
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteByte(val);
                packet.WriteByte((byte) status);
                packet.WriteByte((byte) packageInv);
                packet.WriteInt16(packageSlot);
                foreach (Asda2Item asda2Item in asda2ItemArray)
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                packet.WriteInt32(weight);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.DisasembleEquipment)]
        public static void DisasembleEquipmentRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadInt32();
            Asda2InventoryType invNum = (Asda2InventoryType) packet.ReadByte();
            short slot = packet.ReadInt16();
            DisasembleItemStatus status = client.ActiveCharacter.Asda2Inventory.DisasembleItem(invNum, slot);
            client.ActiveCharacter.Map.AddMessage((Action) (() =>
            {
                if (status == DisasembleItemStatus.Ok)
                    return;
                Asda2InventoryHandler.SendEquipmentDisasembledResponse(client, status, (short) 0, (Asda2Item) null,
                    slot);
            }));
        }

        public static void SendEquipmentDisasembledResponse(IRealmClient client, DisasembleItemStatus status,
            short weight, Asda2Item addedItem, short srcSlot)
        {
            switch (addedItem.ItemId)
            {
                case 20679:
                    AchievementProgressRecord progressRecord1 =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(96U);
                    switch (++progressRecord1.Counter)
                    {
                        case 100:
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Glittering229);
                            break;
                        case 200:
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Glittering229);
                            break;
                    }

                    progressRecord1.SaveAndFlush();
                    break;
                case 20680:
                    AchievementProgressRecord progressRecord2 =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(97U);
                    switch (++progressRecord2.Counter)
                    {
                        case 75:
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Mystic230);
                            break;
                        case 150:
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Mystic230);
                            break;
                    }

                    progressRecord2.SaveAndFlush();
                    break;
                case 20681:
                    AchievementProgressRecord progressRecord3 =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(98U);
                    switch (++progressRecord3.Counter)
                    {
                        case 50:
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Ultimate231);
                            break;
                        case 100:
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Ultimate231);
                            break;
                    }

                    progressRecord3.SaveAndFlush();
                    break;
            }

            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.EquipmentDisasembled))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(weight);
                packet.WriteInt32(0);
                packet.WriteByte((byte) 1);
                packet.WriteInt16(srcSlot);
                packet.WriteSkip(Asda2InventoryHandler.stab16);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, addedItem, false);
                packet.WriteInt16(addedItem == null ? 0 : (int) addedItem.Weight);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.ByuItemFromWarshop)]
        public static void ByuItemFromWarshopRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            short internalWarShopId = packet.ReadInt16();
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
            {
                BuyFromWarShopStatus status =
                    client.ActiveCharacter.Asda2Inventory.BuyItemFromWarshop((int) internalWarShopId);
                client.ActiveCharacter.Map.AddMessage((Action) (() =>
                {
                    if (status == BuyFromWarShopStatus.Ok)
                        return;
                    Asda2InventoryHandler.SendItemFromWarshopBuyedResponse(client, status, (short) 0, 0,
                        (Asda2Item) null, (Asda2Item) null);
                }));
            }));
        }

        public static void SendItemFromWarshopBuyedResponse(IRealmClient client, BuyFromWarShopStatus status,
            short invWeight, int money, Asda2Item moneyItem, Asda2Item buyedItem)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemFromWarshopBuyed))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(invWeight);
                packet.WriteInt32(money);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, moneyItem, false);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, buyedItem, false);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.ShowWarehouse)]
        public static void ShowWarehouseRequest(IRealmClient client, RealmPacketIn packet)
        {
            byte num = packet.ReadByte();
            if (num == (byte) 0 || (int) num - 1 > (int) client.ActiveCharacter.Record.PremiumWarehouseBagsCount)
                client.ActiveCharacter.YouAreFuckingCheater("Is trying to show premium warehouse bags that not owned.",
                    50);
            else
                Asda2InventoryHandler.SendShowWarehouseItemsResponse(client, (byte) ((uint) num - 1U), false);
        }

        [PacketHandler(RealmServerOpCode.ShowAvatarWhItems)]
        public static void ShowAvatarWhItemsRequest(IRealmClient client, RealmPacketIn packet)
        {
            byte num = packet.ReadByte();
            if (num == (byte) 0 || (int) num - 1 > (int) client.ActiveCharacter.Record.PremiumAvatarWarehouseBagsCount)
                client.ActiveCharacter.YouAreFuckingCheater("Is trying to show premium warehouse bags that not owned.",
                    50);
            else
                Asda2InventoryHandler.SendShowWarehouseItemsResponse(client, (byte) ((uint) num - 1U), true);
        }

        [PacketHandler(RealmServerOpCode.PushToWarehouse)]
        public static void PushToWarehouseRequest(IRealmClient client, RealmPacketIn packet)
        {
            IEnumerable<Asda2WhItemStub> itemStubs = Asda2InventoryHandler.ReadItemStubs(packet);
            client.ActiveCharacter.Asda2Inventory.PushItemsToWh(itemStubs);
        }

        private static IEnumerable<Asda2WhItemStub> ReadItemStubs(RealmPacketIn packet)
        {
            Asda2WhItemStub[] asda2WhItemStubArray = new Asda2WhItemStub[5];
            for (int index = 0; index < 5; ++index)
            {
                short num1 = packet.ReadInt16();
                packet.Position += 2;
                byte num2 = packet.ReadByte();
                int num3 = packet.ReadInt32();
                short num4 = packet.ReadInt16();
                asda2WhItemStubArray[index] = new Asda2WhItemStub()
                {
                    Amount = num3 == 0 ? 1 : num3,
                    Invtentory = (Asda2InventoryType) num2,
                    Slot = num1,
                    Weight = num4
                };
            }

            return (IEnumerable<Asda2WhItemStub>) ((IEnumerable<Asda2WhItemStub>) asda2WhItemStubArray)
                .Where<Asda2WhItemStub>((Func<Asda2WhItemStub, bool>) (itemStub => itemStub.Slot != (short) -1))
                .ToArray<Asda2WhItemStub>();
        }

        [PacketHandler(RealmServerOpCode.StoreAvatarItems)]
        public static void StoreAvatarItemsRequest(IRealmClient client, RealmPacketIn packet)
        {
            IEnumerable<Asda2WhItemStub> itemStubs = Asda2InventoryHandler.ReadItemStubs(packet);
            client.ActiveCharacter.Asda2Inventory.PushItemsToAvatarWh(itemStubs);
        }

        public static void SendItemsPushedToWarehouseResponse(IRealmClient client, PushItemToWhStatus status,
            IEnumerable<Asda2WhItemStub> sourceItemStubs = null, IEnumerable<Asda2WhItemStub> destItemStubs = null)
        {
            IEnumerable<Asda2WhItemStub> asda2WhItemStubs = Asda2InventoryHandler
                .CreateArrayOfFiveElementsFromEnumerable(sourceItemStubs)
                .Concat<Asda2WhItemStub>(Asda2InventoryHandler.CreateArrayOfFiveElementsFromEnumerable(destItemStubs));
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemsPushedToWarehouse))
            {
                packet.WriteByte((byte) status);
                if (status == PushItemToWhStatus.Ok)
                {
                    foreach (Asda2WhItemStub asda2WhItemStub in asda2WhItemStubs)
                    {
                        packet.WriteInt32(asda2WhItemStub == null ? -1 : (int) asda2WhItemStub.Slot);
                        packet.WriteByte(asda2WhItemStub == null ? 0 : (int) (byte) asda2WhItemStub.Invtentory);
                        packet.WriteInt32(asda2WhItemStub == null ? -1 : asda2WhItemStub.Amount);
                        packet.WriteInt16(asda2WhItemStub == null ? 0 : (int) asda2WhItemStub.Weight);
                    }

                    packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                }

                client.Send(packet, false);
            }
        }

        public static void SendItemsPushedToAvatarWarehouseResponse(IRealmClient client, PushItemToWhStatus status,
            IEnumerable<Asda2WhItemStub> sourceItemStubs = null, IEnumerable<Asda2WhItemStub> destItemStubs = null)
        {
            IEnumerable<Asda2WhItemStub> asda2WhItemStubs = Asda2InventoryHandler
                .CreateArrayOfFiveElementsFromEnumerable(sourceItemStubs)
                .Concat<Asda2WhItemStub>(Asda2InventoryHandler.CreateArrayOfFiveElementsFromEnumerable(destItemStubs));
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AvatarItemsStored))
            {
                packet.WriteByte((byte) status);
                if (status == PushItemToWhStatus.Ok)
                {
                    foreach (Asda2WhItemStub asda2WhItemStub in asda2WhItemStubs)
                    {
                        packet.WriteInt32(asda2WhItemStub == null ? -1 : (int) asda2WhItemStub.Slot);
                        packet.WriteByte(asda2WhItemStub == null ? 0 : (int) (byte) asda2WhItemStub.Invtentory);
                        packet.WriteInt32(asda2WhItemStub == null ? -1 : asda2WhItemStub.Amount);
                        packet.WriteInt16(asda2WhItemStub == null ? 0 : (int) asda2WhItemStub.Weight);
                    }

                    packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                }

                client.Send(packet, false);
            }
        }

        public static void SendItemsTakedFromWarehouseResponse(IRealmClient client, PushItemToWhStatus status,
            IEnumerable<Asda2WhItemStub> sourceItemStubs = null, IEnumerable<Asda2WhItemStub> destItemStubs = null)
        {
            IEnumerable<Asda2WhItemStub> elementsFromEnumerable =
                Asda2InventoryHandler.CreateArrayOfFiveElementsFromEnumerable(sourceItemStubs);
            IEnumerable<Asda2WhItemStub> asda2WhItemStubs = Asda2InventoryHandler
                .CreateArrayOfFiveElementsFromEnumerable(destItemStubs).Concat<Asda2WhItemStub>(elementsFromEnumerable);
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemFormWarehouseTaked))
            {
                packet.WriteByte((byte) status);
                if (status == PushItemToWhStatus.Ok)
                {
                    foreach (Asda2WhItemStub asda2WhItemStub in asda2WhItemStubs)
                    {
                        packet.WriteInt32(asda2WhItemStub == null ? -1 : (int) asda2WhItemStub.Slot);
                        packet.WriteByte(asda2WhItemStub == null ? 0 : (int) (byte) asda2WhItemStub.Invtentory);
                        packet.WriteInt32(asda2WhItemStub == null ? -1 : asda2WhItemStub.Amount);
                        packet.WriteInt16(asda2WhItemStub == null ? 0 : (int) asda2WhItemStub.Weight);
                    }

                    packet.WriteInt32(client.ActiveCharacter.Money);
                    packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                }

                client.Send(packet, false);
            }
        }

        public static void SendItemsTakedFromAvatarWarehouseResponse(IRealmClient client, PushItemToWhStatus status,
            IEnumerable<Asda2WhItemStub> sourceItemStubs = null, IEnumerable<Asda2WhItemStub> destItemStubs = null)
        {
            IEnumerable<Asda2WhItemStub> elementsFromEnumerable =
                Asda2InventoryHandler.CreateArrayOfFiveElementsFromEnumerable(sourceItemStubs);
            IEnumerable<Asda2WhItemStub> asda2WhItemStubs = Asda2InventoryHandler
                .CreateArrayOfFiveElementsFromEnumerable(destItemStubs).Concat<Asda2WhItemStub>(elementsFromEnumerable);
            using (RealmPacketOut packet =
                new RealmPacketOut(RealmServerOpCode.WiningFactionInfo | RealmServerOpCode.CMSG_QUERY_OBJECT_POSITION))
            {
                packet.WriteByte((byte) status);
                if (status == PushItemToWhStatus.Ok)
                {
                    foreach (Asda2WhItemStub asda2WhItemStub in asda2WhItemStubs)
                    {
                        packet.WriteInt32(asda2WhItemStub == null ? -1 : (int) asda2WhItemStub.Slot);
                        packet.WriteByte(asda2WhItemStub == null ? 0 : (int) (byte) asda2WhItemStub.Invtentory);
                        packet.WriteInt32(asda2WhItemStub == null ? -1 : asda2WhItemStub.Amount);
                        packet.WriteInt16(asda2WhItemStub == null ? 0 : (int) asda2WhItemStub.Weight);
                    }

                    packet.WriteInt32(client.ActiveCharacter.Money);
                    packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                }

                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.TakeItemFromWarehouse)]
        public static void TakeItemFromWarehouseRequest(IRealmClient client, RealmPacketIn packet)
        {
            IEnumerable<Asda2WhItemStub> itemStubs = Asda2InventoryHandler.ReadItemStubs(packet);
            client.ActiveCharacter.Asda2Inventory.TakeItemsFromWh(itemStubs);
        }

        [PacketHandler(RealmServerOpCode.RetriveItemsFromAvatarWh)]
        public static void RetriveItemsFromAvatarWhRequest(IRealmClient client, RealmPacketIn packet)
        {
            IEnumerable<Asda2WhItemStub> itemStubs = Asda2InventoryHandler.ReadItemStubs(packet);
            client.ActiveCharacter.Asda2Inventory.TakeItemsFromAvatarWh(itemStubs);
        }

        private static IEnumerable<Asda2WhItemStub> CreateArrayOfFiveElementsFromEnumerable(
            IEnumerable<Asda2WhItemStub> itemStubs)
        {
            Asda2WhItemStub[] asda2WhItemStubArray = new Asda2WhItemStub[5];
            if (itemStubs == null)
                return (IEnumerable<Asda2WhItemStub>) asda2WhItemStubArray;
            int index = 0;
            foreach (Asda2WhItemStub itemStub in itemStubs)
            {
                asda2WhItemStubArray[index] = itemStub;
                ++index;
            }

            return (IEnumerable<Asda2WhItemStub>) asda2WhItemStubArray;
        }

        public static void SendShowWarehouseItemsResponse(IRealmClient client, byte page, bool isAvatar)
        {
            Asda2PlayerInventory asda2Inventory = client.ActiveCharacter.Asda2Inventory;
            List<List<Asda2Item>> asda2ItemListList = new List<List<Asda2Item>>();
            int count = 0;
            Asda2Item[] asda2ItemArray = isAvatar
                ? ((IEnumerable<Asda2Item>) asda2Inventory.AvatarWarehouseItems)
                .Skip<Asda2Item>(page == byte.MaxValue ? 0 : (int) page * 30)
                .Take<Asda2Item>(page == byte.MaxValue ? 270 : 30)
                .Where<Asda2Item>((Func<Asda2Item, bool>) (it => it != null)).ToArray<Asda2Item>()
                : ((IEnumerable<Asda2Item>) asda2Inventory.WarehouseItems)
                .Skip<Asda2Item>(page == byte.MaxValue ? 0 : (int) page * 30)
                .Take<Asda2Item>(page == byte.MaxValue ? 270 : 30)
                .Where<Asda2Item>((Func<Asda2Item, bool>) (it => it != null)).ToArray<Asda2Item>();
            while (count < asda2ItemArray.Length)
            {
                asda2ItemListList.Add(new List<Asda2Item>(((IEnumerable<Asda2Item>) asda2ItemArray)
                    .Skip<Asda2Item>(count).Take<Asda2Item>(10)));
                count += 10;
            }

            foreach (List<Asda2Item> asda2ItemList in asda2ItemListList)
            {
                using (RealmPacketOut packet = isAvatar
                    ? new RealmPacketOut(RealmServerOpCode.AvatarWhItemsList)
                    : new RealmPacketOut(RealmServerOpCode.ShowWarehouseItems))
                {
                    for (int index = 0; index < asda2ItemList.Count; ++index)
                    {
                        Asda2Item asda2Item = asda2ItemList[index];
                        if (asda2Item != null)
                            Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                    }

                    client.Send(packet, false);
                }
            }

            if (isAvatar)
                Asda2InventoryHandler.SendAvatarWhItemsListEndedResponse(client);
            else
                Asda2InventoryHandler.SendShowWarehouseEndResponse(client);
        }

        public static void SendShowWarehouseEndResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ShowWarehouseEnd))
                client.Send(packet, true);
        }

        public static void SendAvatarWhItemsListEndedResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AvatarWhItemsListEnded))
                client.Send(packet, true);
        }

        [PacketHandler(RealmServerOpCode.DisassembleAvatar)]
        public static void DisassembleAvatarRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 4;
            short slotInq = packet.ReadInt16();
            Asda2Item shopShopItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq);
            if (shopShopItem == null)
            {
                client.ActiveCharacter.SendInfoMsg("Avatar item not founded.");
                Asda2InventoryHandler.SendAvatarDissasembledResponse(client, AvatarDisassembleStatus.Fail, 0, (short) 0,
                    (Asda2Item) null);
            }
            else
            {
                bool flag = shopShopItem.Template.Quality == Asda2ItemQuality.Green ||
                            shopShopItem.Template.Quality == Asda2ItemQuality.Orange ||
                            shopShopItem.Template.Quality == Asda2ItemQuality.Purple;
                AvatarDisasembleRecord disasembleRecord = (AvatarDisasembleRecord) null;
                if (flag)
                {
                    foreach (AvatarDisasembleRecord premiumAvatarRecord in Asda2ItemMgr.PremiumAvatarRecords)
                    {
                        if (premiumAvatarRecord != null)
                        {
                            if (premiumAvatarRecord.Level > client.ActiveCharacter.Level)
                            {
                                if (disasembleRecord == null)
                                {
                                    disasembleRecord = premiumAvatarRecord;
                                    break;
                                }

                                break;
                            }

                            disasembleRecord = premiumAvatarRecord;
                        }
                    }
                }
                else
                {
                    foreach (AvatarDisasembleRecord regularAvatarRecord in Asda2ItemMgr.RegularAvatarRecords)
                    {
                        if (regularAvatarRecord != null)
                        {
                            if (regularAvatarRecord.Level <= client.ActiveCharacter.Level)
                                disasembleRecord = regularAvatarRecord;
                            else
                                break;
                        }
                    }
                }

                if (disasembleRecord == null)
                {
                    client.ActiveCharacter.SendInfoMsg("Avatar template not found.");
                    Asda2InventoryHandler.SendAvatarDissasembledResponse(client, AvatarDisassembleStatus.Fail, 0,
                        (short) 0, (Asda2Item) null);
                }
                else
                {
                    Asda2Item asda2Item = (Asda2Item) null;
                    Asda2InventoryError asda2InventoryError =
                        client.ActiveCharacter.Asda2Inventory.TryAdd((int) disasembleRecord.GetRandomItemId(), 1, true,
                            ref asda2Item, new Asda2InventoryType?(), (Asda2Item) null);
                    if (asda2InventoryError != Asda2InventoryError.Ok)
                    {
                        client.ActiveCharacter.SendInfoMsg("Error " + (object) asda2InventoryError);
                        Asda2InventoryHandler.SendAvatarDissasembledResponse(client, AvatarDisassembleStatus.Fail, 0,
                            (short) 0, (Asda2Item) null);
                    }
                    else
                    {
                        Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                            .AddAttribute("source", 0.0, "disassemble_avatar").AddItemAttributes(shopShopItem, "source")
                            .AddItemAttributes(asda2Item, "result").Write();
                        shopShopItem.Destroy();
                        Asda2InventoryHandler.SendAvatarDissasembledResponse(client, AvatarDisassembleStatus.Ok,
                            shopShopItem.ItemId, shopShopItem.Slot, asda2Item);
                    }
                }
            }
        }

        public static void SendAvatarDissasembledResponse(IRealmClient client, AvatarDisassembleStatus status,
            int avatarId, short avatarSlot, Asda2Item item)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AvatarDissasembled))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(avatarId);
                packet.WriteInt16(avatarSlot);
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, false);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.StartAvatarSynthesis)]
        public static void StartAvatarSynthesisRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 4;
            short slotInq1 = packet.ReadInt16();
            packet.ReadInt32();
            short slotInq2 = packet.ReadInt16();
            int num = packet.ReadInt32();
            short slotInq3 = packet.ReadInt16();
            packet.Position += 4;
            short slotInq4 = packet.ReadInt16();
            Asda2Item shopShopItem1 = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq1);
            Asda2Item shopShopItem2 = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq2);
            Asda2Item suplItem =
                num == 0 ? (Asda2Item) null : client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq3);
            Asda2Item shopShopItem3 = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq4);
            if (shopShopItem1 == null)
            {
                client.ActiveCharacter.SendInfoMsg("Item not found. Please restart client.");
                Asda2InventoryHandler.SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo,
                    (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
            }
            else if (shopShopItem3 == null && shopShopItem2 == null)
            {
                client.ActiveCharacter.SendInfoMsg("Tool or avatar material item not found. Please restart client.");
                Asda2InventoryHandler.SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo,
                    (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
            }
            else if (shopShopItem3 != null && shopShopItem2 != null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Use Tool and avatar material item same time.", 10);
                Asda2InventoryHandler.SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo,
                    (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
            }
            else if (!shopShopItem1.Template.IsAvatar)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to syntes not avatar item.", 50);
                Asda2InventoryHandler.SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo,
                    (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
            }
            else if (suplItem != null && suplItem.Category != Asda2ItemCategory.IncreseAvatarSynethisChanceByPrc)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to syntes with wrong supliment.", 50);
                Asda2InventoryHandler.SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo,
                    (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
            }
            else
            {
                if (shopShopItem3 != null)
                {
                    if (shopShopItem3.Category != Asda2ItemCategory.OpenWarehouseAndRuneSocketTool)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Trying to syntes with wrong tool.", 50);
                        Asda2InventoryHandler.SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo,
                            (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
                        return;
                    }

                    --shopShopItem3.Amount;
                }

                if (shopShopItem2 != null)
                {
                    if (!shopShopItem2.Template.IsAvatar)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Trying to syntes with wrong avatar material item.",
                            50);
                        Asda2InventoryHandler.SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo,
                            (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
                        return;
                    }

                    --shopShopItem2.Amount;
                }

                if (suplItem != null)
                    --suplItem.Amount;
                if (CharacterFormulas.IsAvatarSyntesSuccess(shopShopItem1.Enchant, suplItem != null,
                    shopShopItem2 == null ? Asda2ItemQuality.Orange : shopShopItem2.Template.Quality))
                {
                    ++shopShopItem1.Enchant;
                    Asda2InventoryHandler.SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.Ok,
                        shopShopItem1, shopShopItem3 ?? shopShopItem2, suplItem);
                }
                else
                    Asda2InventoryHandler.SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.Fail,
                        shopShopItem1, shopShopItem3 ?? shopShopItem2, suplItem);
            }
        }

        public static void SendAvatarSynthesisResultResponse(IRealmClient client, AvatarSyntesStatus status,
            Asda2Item avatarItem = null, Asda2Item toolkitItem = null, Asda2Item suplItem = null)
        {
            Asda2Item[] asda2ItemArray = new Asda2Item[3]
            {
                avatarItem,
                toolkitItem,
                suplItem
            };
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AvatarSynthesisResult))
            {
                packet.WriteByte((byte) status);
                packet.WriteSkip(Asda2InventoryHandler.unk6);
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                for (int index = 0; index < 3; ++index)
                {
                    Asda2Item asda2Item = asda2ItemArray[index];
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                }

                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.AdvacedEnchantWeapon)]
        public static void AdvacedEnchantWeaponRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 4;
            short slotInq = packet.ReadInt16();
            Asda2Item shopShopItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq);
            if (shopShopItem == null)
            {
                client.ActiveCharacter.SendInfoMsg("Item not found. Restart game please.");
                Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.Fail,
                    (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
            }
            else if (shopShopItem.Template.Quality != Asda2ItemQuality.Green &&
                     shopShopItem.Template.Quality != Asda2ItemQuality.Purple)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to advanced enchant item with wrong quality.", 50);
                Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.Fail,
                    (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
            }
            else
            {
                ++packet.Position;
                int num1 = packet.ReadInt32();
                packet.Position += 7;
                int num2 = packet.ReadInt32();
                packet.Position += 7;
                int num3 = packet.ReadInt32();
                packet.Position += 7;
                Asda2Item regularItem1 = client.ActiveCharacter.Asda2Inventory.GetRegularItem((short) num1);
                Asda2Item regularItem2 = client.ActiveCharacter.Asda2Inventory.GetRegularItem((short) num2);
                Asda2Item regularItem3 = client.ActiveCharacter.Asda2Inventory.GetRegularItem((short) num3);
                if (regularItem1 == null || regularItem2 == null || regularItem3 == null)
                {
                    client.ActiveCharacter.SendInfoMsg("Resource not found. Restart game please.");
                    Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.Fail,
                        (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
                }
                else
                {
                    if (shopShopItem.Template.Quality == Asda2ItemQuality.Green)
                    {
                        if (regularItem1.ItemId != 33706 || regularItem2.ItemId != 20681 ||
                            regularItem3.ItemId != 33705)
                        {
                            client.ActiveCharacter.YouAreFuckingCheater(
                                "Trying to advanced enchant with wrong resources.", 50);
                            Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.Fail,
                                (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
                            return;
                        }
                    }
                    else if (shopShopItem.Template.Quality == Asda2ItemQuality.Purple &&
                             (regularItem1.ItemId != 20681 || regularItem2.ItemId != 20680 ||
                              regularItem3.ItemId != 33705))
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Trying to advanced enchant with wrong resources.",
                            50);
                        Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.Fail,
                            (Asda2Item) null, (Asda2Item) null, (Asda2Item) null, (Asda2Item) null);
                        return;
                    }

                    switch (shopShopItem.Template.AuctionLevelCriterion)
                    {
                        case AuctionLevelCriterion.Zero:
                            if (regularItem1.Amount < 1 || regularItem2.Amount < 1 || regularItem3.Amount < 3)
                            {
                                client.ActiveCharacter.SendInfoMsg("Not enought resources. Restart game please.");
                                Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client,
                                    AdvancedEnchantStatus.NotEnoughtMaterials, (Asda2Item) null, (Asda2Item) null,
                                    (Asda2Item) null, (Asda2Item) null);
                                return;
                            }

                            if (!client.ActiveCharacter.SubtractMoney(50000U))
                            {
                                client.ActiveCharacter.SendInfoMsg("Not enought money. Restart game please.");
                                Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client,
                                    AdvancedEnchantStatus.NotEnoghtGold, (Asda2Item) null, (Asda2Item) null,
                                    (Asda2Item) null, (Asda2Item) null);
                                return;
                            }

                            --regularItem1.Amount;
                            --regularItem2.Amount;
                            regularItem3.Amount -= 3;
                            break;
                        case AuctionLevelCriterion.One:
                            if (regularItem1.Amount < 1 || regularItem2.Amount < 2 || regularItem3.Amount < 6)
                            {
                                client.ActiveCharacter.SendInfoMsg("Not enought resources. Restart game please.");
                                Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client,
                                    AdvancedEnchantStatus.NotEnoughtMaterials, (Asda2Item) null, (Asda2Item) null,
                                    (Asda2Item) null, (Asda2Item) null);
                                return;
                            }

                            if (!client.ActiveCharacter.SubtractMoney(100000U))
                            {
                                client.ActiveCharacter.SendInfoMsg("Not enought money. Restart game please.");
                                Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client,
                                    AdvancedEnchantStatus.NotEnoghtGold, (Asda2Item) null, (Asda2Item) null,
                                    (Asda2Item) null, (Asda2Item) null);
                                return;
                            }

                            --regularItem1.Amount;
                            regularItem2.Amount -= 2;
                            regularItem3.Amount -= 6;
                            break;
                        case AuctionLevelCriterion.Two:
                            if (regularItem1.Amount < 2 || regularItem2.Amount < 3 || regularItem3.Amount < 9)
                            {
                                client.ActiveCharacter.SendInfoMsg("Not enought resources. Restart game please.");
                                Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client,
                                    AdvancedEnchantStatus.NotEnoughtMaterials, (Asda2Item) null, (Asda2Item) null,
                                    (Asda2Item) null, (Asda2Item) null);
                                return;
                            }

                            if (!client.ActiveCharacter.SubtractMoney(200000U))
                            {
                                client.ActiveCharacter.SendInfoMsg("Not enought money. Restart game please.");
                                Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client,
                                    AdvancedEnchantStatus.NotEnoghtGold, (Asda2Item) null, (Asda2Item) null,
                                    (Asda2Item) null, (Asda2Item) null);
                                return;
                            }

                            regularItem1.Amount -= 2;
                            regularItem2.Amount -= 3;
                            regularItem3.Amount -= 9;
                            break;
                        case AuctionLevelCriterion.Three:
                            if (regularItem1.Amount < 2 || regularItem2.Amount < 4 || regularItem3.Amount < 12)
                            {
                                client.ActiveCharacter.SendInfoMsg("Not enought resources. Restart game please.");
                                Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client,
                                    AdvancedEnchantStatus.NotEnoughtMaterials, (Asda2Item) null, (Asda2Item) null,
                                    (Asda2Item) null, (Asda2Item) null);
                                return;
                            }

                            if (!client.ActiveCharacter.SubtractMoney(400000U))
                            {
                                client.ActiveCharacter.SendInfoMsg("Not enought money. Restart game please.");
                                Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client,
                                    AdvancedEnchantStatus.NotEnoghtGold, (Asda2Item) null, (Asda2Item) null,
                                    (Asda2Item) null, (Asda2Item) null);
                                return;
                            }

                            regularItem1.Amount -= 2;
                            regularItem2.Amount -= 4;
                            regularItem3.Amount -= 12;
                            break;
                        case AuctionLevelCriterion.Four:
                            if (regularItem1.Amount < 3 || regularItem2.Amount < 5 || regularItem3.Amount < 15)
                            {
                                client.ActiveCharacter.SendInfoMsg("Not enought resources. Restart game please.");
                                Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client,
                                    AdvancedEnchantStatus.NotEnoughtMaterials, (Asda2Item) null, (Asda2Item) null,
                                    (Asda2Item) null, (Asda2Item) null);
                                return;
                            }

                            if (!client.ActiveCharacter.SubtractMoney(800000U))
                            {
                                client.ActiveCharacter.SendInfoMsg("Not enought money. Restart game please.");
                                Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client,
                                    AdvancedEnchantStatus.NotEnoghtGold, (Asda2Item) null, (Asda2Item) null,
                                    (Asda2Item) null, (Asda2Item) null);
                                return;
                            }

                            regularItem1.Amount -= 3;
                            regularItem2.Amount -= 5;
                            regularItem3.Amount -= 15;
                            break;
                    }

                    shopShopItem.SetRandomAdvancedEnchant();
                    shopShopItem.Save();
                    if (!regularItem1.IsDeleted)
                        regularItem1.Save();
                    if (!regularItem2.IsDeleted)
                        regularItem2.Save();
                    if (!regularItem3.IsDeleted)
                        regularItem3.Save();
                    Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                        .AddAttribute("source", 0.0, "advanced_enchant").AddItemAttributes(shopShopItem, "main")
                        .AddItemAttributes(regularItem1, "res1").AddItemAttributes(regularItem2, "res2")
                        .AddItemAttributes(regularItem3, "res3").Write();
                    Asda2InventoryHandler.SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.Ok,
                        shopShopItem, regularItem1, regularItem2, regularItem3);
                    client.ActiveCharacter.SendMoneyUpdate();
                }
            }
        }

        public static void SendAdvancedEnchantDoneResponse(IRealmClient client, AdvancedEnchantStatus status,
            Asda2Item enchantedItem = null, Asda2Item firstResource = null, Asda2Item secondResource = null,
            Asda2Item thirdResource = null)
        {
            Asda2Item[] asda2ItemArray = new Asda2Item[4]
            {
                enchantedItem,
                firstResource,
                secondResource,
                thirdResource
            };
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AdvancedEnchantDone))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                packet.WriteInt32(client.ActiveCharacter.Money);
                for (int index = 0; index < 4; ++index)
                {
                    Asda2Item asda2Item = asda2ItemArray[index];
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                }

                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.CombineItem)]
        public static void CombineItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            short comtinationId = packet.ReadInt16();
            client.ActiveCharacter.Asda2Inventory.CombineItems(comtinationId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="resultItem"></param>
        /// <param name="usedItems">max 5 items</param>
        public static void SendItemCombinedResponse(IRealmClient client, Asda2Item resultItem,
            List<Asda2Item> usedItems)
        {
            Asda2Item[] asda2ItemArray = new Asda2Item[10];
            asda2ItemArray[0] = resultItem;
            for (int index = 0; index < usedItems.Count; ++index)
                asda2ItemArray[index + 5] = usedItems[index];
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemCombined))
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt16(client.ActiveCharacter.CharNum);
                packet.WriteByte(1);
                packet.WriteByte(1);
                for (int index = 0; index < 10; ++index)
                {
                    Asda2Item asda2Item = asda2ItemArray[index];
                    if (asda2Item != null)
                    {
                        Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                        if (asda2Item.IsDeleted)
                        {
                            asda2Item.IsDeleted = false;
                            asda2Item.Destroy();
                        }
                    }
                    else
                        Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                }

                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.SummonBoss)]
        public static void SummonBossRequest(IRealmClient client, RealmPacketIn packet)
        {
            List<Asda2Item> items = new List<Asda2Item>();
            for (int index = 0; index < 6; ++index)
            {
                int num1 = packet.ReadInt32();
                int num2 = (int) packet.ReadByte();
                short slotInq = packet.ReadInt16();
                if (num1 != -1)
                {
                    Asda2Item regularItem = client.ActiveCharacter.Asda2Inventory.GetRegularItem(slotInq);
                    if (regularItem == null)
                    {
                        client.ActiveCharacter.SendInfoMsg("Wrong inventory info please restart game.");
                        return;
                    }

                    items.Add(regularItem);
                }
                else
                    break;
            }

            if (items.Count == 0)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to summon boss with wrong info.", 50);
            }
            else
            {
                Asda2BossSummonRecord summonRecord = Asda2ItemMgr.SummonRecords[items[0].ItemId];
                if (summonRecord == null)
                    client.ActiveCharacter.SendInfoMsg(string.Format("Summon record {0} amount {1} not founed.",
                        (object) items[0].ItemId, (object) items.Count));
                else if (items.Count < (int) summonRecord.Amount)
                {
                    client.ActiveCharacter.SendInfoMsg("not enought stones. required " + (object) summonRecord.Amount);
                }
                else
                {
                    for (int index = 1; index < items.Count; ++index)
                    {
                        if (items[index].ItemId != items[0].ItemId)
                        {
                            client.ActiveCharacter.YouAreFuckingCheater("Trying to summon boss with wrong info.", 50);
                            return;
                        }
                    }

                    foreach (Asda2Item asda2Item in items)
                        asda2Item.Destroy();
                    NPCEntry entry = NPCMgr.GetEntry(summonRecord.MobId);
                    if (entry == null)
                    {
                        client.ActiveCharacter.SendInfoMsg(string.Format("Summon record {0} has invalind npc Id {1}.",
                            (object) items[0].ItemId, (object) summonRecord.MobId));
                    }
                    else
                    {
                        WorldLocation worldLocation = new WorldLocation(summonRecord.MapId,
                            new Vector3((float) ((int) summonRecord.X + (int) summonRecord.MapId * 1000),
                                (float) ((int) summonRecord.Y + (int) summonRecord.MapId * 1000)), 1U);
                        entry.SpawnAt((IWorldLocation) worldLocation, false).Brain.EnterDefaultState();
                        Asda2InventoryHandler.SendMonsterSummonedResponse(client, items);
                    }
                }
            }
        }

        public static void SendMonsterSummonedResponse(IRealmClient client, List<Asda2Item> items)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MonsterSummoned))
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);
                for (int index = 0; index < 6; ++index)
                {
                    Asda2Item asda2Item = items.Count <= index ? (Asda2Item) null : items[index];
                    packet.WriteInt32(asda2Item == null ? -1 : asda2Item.ItemId);
                    packet.WriteByte(asda2Item == null ? (byte) 0 : (byte) asda2Item.InventoryType);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Slot);
                }

                packet.WriteByte(1);
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                client.Send(packet, true);
            }
        }

        public static void SendSomeNewItemRecivedResponse(IRealmClient client, int itemId, byte slot)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SomeNewItemRecived))
            {
                packet.WriteByte(slot);
                packet.WriteInt32(itemId);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.TakeItemFromMail)]
        public static void TakeItemFromMailRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 4;
            int key = packet.ReadInt32();
            packet.ReadInt32();
            ++packet.Position;
            int num1 = (int) packet.ReadByte();
            if (!client.ActiveCharacter.Asda2Inventory.DonationItems.ContainsKey(key))
            {
                client.ActiveCharacter.SendInfoMsg("Can't found donated item.");
            }
            else
            {
                int num2 = 1;
                Asda2DonationItem donationItem = client.ActiveCharacter.Asda2Inventory.DonationItems[key];
                if (donationItem.Recived)
                    client.ActiveCharacter.SendInfoMsg("Item already recived.");
                else if (client.ActiveCharacter.Asda2Inventory.FreeRegularSlotsCount < num2 ||
                         client.ActiveCharacter.Asda2Inventory.FreeShopSlotsCount < num2)
                {
                    client.ActiveCharacter.SendInfoMsg("Not enought inventory space.");
                }
                else
                {
                    donationItem.Recived = true;
                    donationItem.Save();
                    client.ActiveCharacter.Asda2Inventory.DonationItems.Remove(donationItem.Guid);
                    Asda2Item asda2Item = (Asda2Item) null;
                    int num3 = (int) client.ActiveCharacter.Asda2Inventory.TryAdd(donationItem.ItemId,
                        donationItem.Amount, true, ref asda2Item, new Asda2InventoryType?(), (Asda2Item) null);
                    Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                        .AddAttribute("source", 0.0, "donation_mail")
                        .AddAttribute("added_amount", (double) donationItem.Amount, "").AddItemAttributes(asda2Item, "")
                        .AddAttribute("donation_id", (double) donationItem.Guid, "")
                        .AddAttribute("creator", 0.0, donationItem.Creator).Write();
                    if (asda2Item != null)
                        asda2Item.IsSoulbound = true;
                    Asda2InventoryHandler.SendItemFromMailTakedResponse(client, TakeItemMallItemsResult.Ok,
                        new List<Asda2Item>()
                        {
                            asda2Item
                        });
                }
            }
        }

        [PacketHandler(RealmServerOpCode.ShowMeItemMallItems)]
        public static void ShowMeItemMallItemsRequest(IRealmClient client, RealmPacketIn packet)
        {
            List<Asda2DonationItem> list = client.ActiveCharacter.Asda2Inventory.DonationItems.Values
                .Take<Asda2DonationItem>(7).ToList<Asda2DonationItem>();
            Asda2InventoryHandler.SendItemMailListResponse(client, list);
        }

        public static void SendItemMailListResponse(IRealmClient client, List<Asda2DonationItem> items)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemMailList))
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);
                for (int val = 0; val < 7; ++val)
                {
                    Asda2DonationItem asda2DonationItem = items.Count <= val ? (Asda2DonationItem) null : items[val];
                    packet.WriteInt32(asda2DonationItem == null ? -1 : asda2DonationItem.Guid);
                    packet.WriteInt32(asda2DonationItem == null ? -1 : asda2DonationItem.ItemId);
                    packet.WriteByte(1);
                    packet.WriteByte(val);
                }

                client.Send(packet, true);
            }
        }

        public static void SendItemFromMailTakedResponse(IRealmClient client, TakeItemMallItemsResult status,
            List<Asda2Item> items)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemFromMailTaked))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                for (int index = 0; index < 7; ++index)
                {
                    Asda2Item asda2Item = items.Count <= index ? (Asda2Item) null : items[index];
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2Item, false);
                }

                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.CloseMailBox)]
        public static void CloseMailBoxRequest(IRealmClient client, RealmPacketIn packet)
        {
            Asda2InventoryHandler.SendMailBoxClosedResponse(client);
        }

        public static void SendMailBoxClosedResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MailBoxClosed))
            {
                packet.WriteByte(1);
                packet.WriteInt32(0);
                client.Send(packet, true);
            }
        }

        public static void SendGoldPickupedResponse(uint amount, Character chr)
        {
            if (chr == null || chr.Asda2Inventory == null)
                return;
            AchievementProgressRecord progressRecord = chr.Achievements.GetOrCreateProgressRecord(101U);
            progressRecord.Counter += amount;
            if (progressRecord.Counter >= 100000U)
                chr.GetTitle(Asda2TitleId.Wealthy234);
            progressRecord.SaveAndFlush();
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemPickuped))
            {
                packet.WriteByte((byte) 1);
                packet.WriteInt16(-1);
                packet.WriteInt32(20551);
                packet.WriteByte(2);
                packet.WriteInt16(0);
                packet.WriteInt16(0);
                packet.WriteInt32(amount);
                packet.WriteByte(0);
                packet.WriteInt16(0);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteByte(-1);
                packet.WriteSkip(Asda2InventoryHandler.Stab31);
                packet.WriteByte(0);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteInt16(-1);
                packet.WriteByte(0);
                packet.WriteByte(0);
                packet.WriteInt32(0);
                packet.WriteInt16(0);
                packet.WriteInt16(chr.Asda2Inventory.Weight);
                chr.Client.Send(packet, true);
            }
        }
    }
}