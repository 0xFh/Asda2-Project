using System;
using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using NLog;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Network;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Handlers
{
    public static class PacketOutExt
    {
        private const int goldItemId = 20551;
        public static void WriteItemAmount(this RealmPacketOut packet, Asda2Item item, bool setAmountTo0WhenDeleted = false)
        {
            int value;
            if (item == null)
                value = -1;
            else
            {
                if (item.ItemId == goldItemId)
                {
                    if (item.OwningCharacter == null)
                        value = item.Amount;
                    else
                    {
                        value = (int)item.OwningCharacter.Money;
                    }
                }
                else
                {
                    if (item.IsDeleted)
                    {
                        if (setAmountTo0WhenDeleted)
                            value = 0;
                        else
                        {
                            value = -1;
                        }
                    }
                    else
                    {
                        value = item.Template.IsStackable ? item.Amount : 0;
                    }
                }
            }
            packet.WriteInt32(value);
        }
    }
    internal class Asda2InventoryHandler
    {
        [PacketHandler(RealmServerOpCode.ReplaceItem)] //5000
        public static void ReplaceItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            var srcCell = packet.ReadInt16(); //default : 6Len : 2
            packet.Position += 2; //tab32 default : stab32Len : 2
            var invSrc = packet.ReadByte(); //default : 2Len : 1
            var quantity = packet.ReadInt32(); //default : 300Len : 4
            var weightSrc = packet.ReadInt16(); //default : 23Len : 2
            var cellDst = packet.ReadInt16(); //default : 10Len : 4
            packet.Position += 2; //tab32 default : stab32Len : 2
            var invDst = packet.ReadByte(); //default : 3Len : 1
            var destQuantity = packet.ReadInt32(); //default : 0Len : 4
            var destWeight = packet.ReadInt16(); //default : 0Len : 2

            if (invSrc == 0)
                invSrc = 3;
            var inv = client.ActiveCharacter.Asda2Inventory;

            var err = inv.TrySwap((Asda2InventoryType)invSrc, srcCell, (Asda2InventoryType)invDst,
                                  ref cellDst);
            if (err != Asda2InventoryError.Ok)
                SendItemReplacedResponse(client, err, srcCell, invSrc, quantity, weightSrc, cellDst, invDst,
                                         destQuantity, destWeight);
        }

        public static void SendItemReplacedResponse(IRealmClient client,
                                                    Asda2InventoryError status = Asda2InventoryError.NotInfoAboutItem,
                                                    short srcCell = 0, byte srcInv = 0, int srcQuant = 0,
                                                    short srcWeight = 0, int destCell = 0, byte destInv = 0,
                                                    int destQuant = 0, short destWeight = 0, bool secondItemIsNullNow = false)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemReplaced)) //5001
            {
                packet.WriteByte((byte)status);
                //{status}default value : 1 Len : 1 0 - нет места . 1 - ok, 2 - недостаточно инфы о предмете - 3 - предмен не предназначен для ношения
                packet.WriteByte(secondItemIsNullNow ? 0 : 1); //value name : stab7 default value : stab7Len : 1
                packet.WriteInt16(srcCell); //{srcCell}default value : 6 Len : 2
                packet.WriteInt16(secondItemIsNullNow ? -1 : 0); //value name : stab10 default value : stab10Len : 2
                packet.WriteByte(srcInv); //{srcInv}default value : 2 Len : 1
                packet.WriteInt32(srcQuant); //{srcQuant}default value : 500 Len : 4
                packet.WriteInt16(srcWeight); //{srcWeight}default value : 34 Len : 2
                packet.WriteInt16(destCell); //{destCell}default value : 10 Len : 2
                packet.WriteInt16(0); //value name : stab21 default value : stab21Len : 2
                packet.WriteByte(destInv); //{destInv}default value : 3 Len : 1
                packet.WriteInt32(destQuant); //{destQuant}default value : 300 Len : 4
                packet.WriteInt16(destWeight); //{destWeight}default value : 23 Len : 2
                client.Send(packet, addEnd: true);
            }
        }


        public static void SendCharacterAddEquipmentResponse(Character chr, short slot, int itemId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterAddEquipment)) //4021
            {
                packet.WriteInt16(chr.SessionId); //{sessId}default value : 38 Len : 2
                packet.WriteInt32(-1);
                packet.WriteInt16(slot); //{slot}default value : 9 Len : 2
                packet.WriteInt32(itemId); //{ItemId}default value : 21499 Len : 4
                chr.SendPacketToArea(packet, false, false);
            }
        }

        public static void SendCharacterRemoveEquipmentResponse(Character chr, short slot, int itemId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterRemoveEquipment)) //4023
            {
                packet.WriteInt16(chr.SessionId); //{sessId}default value : 38 Len : 2
                packet.WriteInt32(-1);
                packet.WriteInt16(slot); //{slot}default value : 9 Len : 2
                packet.WriteInt32(itemId); //{ItemId}default value : 21499 Len : 4
                packet.WriteInt32(0);
                chr.SendPacketToArea(packet, false, true);
            }
        }

        #region FastItemSlots

        [PacketHandler(RealmServerOpCode.SetFastItemSlot)] //5067
        public static void SetFastItemSlotRequest(IRealmClient client, RealmPacketIn packet)
        {
            var panelNum = packet.ReadByte(); //default : 0Len : 1
            if (panelNum > 5)
                return;
            var fastItemSlotsCollection = client.ActiveCharacter.Asda2Inventory.FastItemSlotRecords;
            for (byte i = 0; i < 12; i += 1)
            {
                var srcInfo = packet.ReadByte(); //default : 0Len : 1
                var invType = packet.ReadByte(); //default : 2Len : 1
                var slot = packet.ReadInt16(); //default : 3Len : 2
                var quantity = packet.ReadInt32(); //default : 13Len : 4
                var itemOrSkillId = packet.ReadInt16(); //default : 20576Len : 2
                /*if (panelSlot == 0 || panelSlot > 12 || panelSlot < 1)
                {
                    fastItemSlotsCollection[panelNum][i] = null;
                    continue;
                }*/
                if (fastItemSlotsCollection[panelNum][i] != null)
                {
                    fastItemSlotsCollection[panelNum][i].DeleteLater();
                }
                if (srcInfo == 0 && invType == 0 && slot == -1 && quantity == 0 && itemOrSkillId == -1)
                {
                    fastItemSlotsCollection[panelNum][i] = null;
                    continue;
                }
                fastItemSlotsCollection[panelNum][i] =
                    Asda2FastItemSlotRecord.CreateRecord(panelNum, i, (Asda2InventoryType)invType,
                                                         (byte)slot,
                                                         itemOrSkillId,
                                                         quantity, client.ActiveCharacter.EntityId.Low, srcInfo);
            }
        }

        public static void SendAllFastItemSlotsInfo(Character character)
        {
            var client = character.Client;
            if (!client.IsGameServerConnection)
                return;
            var fastItemSLotsCollection = character.Asda2Inventory.FastItemSlotRecords;
            foreach (var kvp in fastItemSLotsCollection.Where(kvp => kvp.Value.Any(r => r != null)))
            {
                using (var packet = new RealmPacketOut(RealmServerOpCode.FastItemSlotsInfo)) //5059
                {
                    packet.WriteByte(kvp.Key); //{panelNum}default value : 0 Len : 1
                    foreach (var rec in kvp.Value)
                    {
                        //packet.WriteByte(kvp.Key); //{panelNum}default value : 0 Len : 1
                        packet.WriteByte(rec == null ? 0 : rec.SrcInfo); //{panelSlot}default value : 0 Len : 1
                        packet.WriteByte(rec == null ? 0 : rec.InventoryType); //{invType}default value : 2 Len : 1
                        packet.WriteInt16(rec == null ? -1 : rec.InventorySlot); //{slot}default value : 3 Len : 2
                        packet.WriteInt32(rec == null ? 0 : rec.Amount); //{quantity}default value : 13 Len : 4
                        packet.WriteInt16(rec == null ? -1 : rec.ItemOrSkillId);
                        //{itemOrSkillId}default value : 20576 Len : 2
                    }
                    client.Send(packet, addEnd: true);
                }
            }
        }

        #endregion

        [PacketHandler(RealmServerOpCode.UseItem)] //5028
        public static void UseItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            byte randomValue = 255;
            try
            {
                var chr = client.ActiveCharacter;
                packet.Position += 4;
                var inv = packet.ReadByte(); //default : 2Len : 1
                var slot = packet.ReadInt32(); //default : 39Len : 4
                if (inv != 2 || slot < 0 || slot > 69)
                {
                    if (slot >= 65000000)
                    {
                        chr.Asda2Inventory.UseItem(Asda2InventoryType.Regular, randomValue); // 24 - random slot number, bad fix, dude, sorry.
                        return;
                    }

                    //chr.YouAreFuckingCheater("Trying to use item from wrong slot.", 20);
                    client.ActiveCharacter.SendInfoMsg("You must update your client to use items!");
                    return;
                }
                chr.Asda2Inventory.UseItem(Asda2InventoryType.Regular, (byte)slot);
            }
            catch (Exception)
            {
                client.ActiveCharacter.SendInfoMsg("You must update your client to use items!");
            }
        }

        /*old ver
         [PacketHandler(RealmServerOpCode.UseItem)] //5028
        public static void UseItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            var inv = packet.ReadByte(); //default : 2Len : 1
            var slot = packet.ReadInt32(); //default : 39Len : 4
            if (inv != 2 || slot < 0 || slot > 69)
            {
                chr.YouAreFuckingCheater("Trying to use item from wrong slot.", 20);
                return;
            }
            chr.Asda2Inventory.UseItem(Asda2InventoryType.Regular, (byte)slot);


        }*/
        public static void SendCharUsedItemResponse(UseItemResult status, Character chr, Asda2Item item)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharUsedItem)) //5029
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                packet.WriteInt16(chr.SessionId); //{sessId}default value : 36 Len : 2
                packet.WriteInt32((int)chr.Account.AccountId); //{accId}default value : 340701 Len : 4
                WriteItemInfoToPacket(packet, item);
                if (status != UseItemResult.Ok)
                    chr.Client.Send(packet, addEnd: true);
                else
                    chr.SendPacketToArea(packet, true, true);
            }
        }
        /*old ver
         public static void SendCharUsedItemResponse(UseItemResult status, Character chr, Asda2Item item)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharUsedItem)) //5029
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                packet.WriteInt16(chr.SessionId); //{sessId}default value : 36 Len : 2
                packet.WriteInt32((int)chr.Account.AccountId); //{accId}default value : 340701 Len : 4
                packet.WriteInt32(item == null ? 0 : item.ItemId); //{itemId}default value : 20583 Len : 4
                packet.WriteInt32(item == null ? -1 : item.Slot); //{slot}default value : 4 Len : 4
                packet.WriteByte(item == null ? -1 : (byte)item.InventoryType); //{inv}default value : 2 Len : 1
                packet.WriteInt32(item == null ? 0 : item.Amount); //{amount}default value : 3 Len : 4
                packet.WriteInt16(item == null ? 0 : item.Amount * item.Weight);
                //value name : cellWeight default value : 30Len : 2
                packet.WriteInt16(item == null ? 0 : chr.Asda2Inventory.Weight); //{weight}default value : 5463 Len : 2
                if (status != UseItemResult.Ok)
                    chr.Client.Send(packet, true);
                else
                    chr.SendPacketToArea(packet, true, true);
            }
        }*/

        #region removeItem

        [PacketHandler(RealmServerOpCode.RemoveItem)] //5006
        public static void RemoveItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 4;
            var inv = packet.ReadByte(); //default : 2Len : 1
            var cell = packet.ReadInt16(); //default : 20Len : 4
            var count = packet.ReadInt16(); //default : 1Len : 4
            client.ActiveCharacter.Asda2Inventory.RemoveItem(cell, inv, count);
        }
        /*old ver
         [PacketHandler(RealmServerOpCode.RemoveItem)] //5006
        public static void RemoveItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            var cell = packet.ReadInt16(); //default : 20Len : 4
            packet.Position += 2;
            var inv = packet.ReadByte(); //default : 2Len : 1
            var count = packet.ReadInt32(); //default : 1Len : 4
            var weight = packet.ReadInt16(); //default : 12Len : 2
            client.ActiveCharacter.Asda2Inventory.RemoveItem(cell, inv, count);
        }*/
        public static void ItemRemovedFromInventoryResponse(Character chr, Asda2Item item, DeleteOrSellItemStatus status, int amountDeleted = 0)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemRemovedFromInventory)) //5007
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt32(chr.Money);//{gold}default value : 4 Len : 4
                packet.WriteInt16(1233);//{accId}default value : 0 Len : 4
                packet.WriteInt16(amountDeleted);//{accId}default value : 0 Len : 4
                packet.WriteInt16(0);//value name : unk default value : 0Len : 2
                WriteItemInfoToPacket(packet, item);
                chr.Client.Send(packet, addEnd: true);
            }
        }
        /*old ver
         public static void ItemRemovedFromInventoryResponse(Character chr, Asda2Item item, DeleteOrSellItemStatus status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemRemovedFromInventory)) //5007
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                packet.WriteInt32(item == null ? 0 : item.Slot); //{slot}default value : 4 Len : 4
                packet.WriteByte((byte)(item == null ? 0 : item.InventoryType)); //{inv}default value : 2 Len : 1
                packet.WriteInt32(item == null ? 0 : item.Amount); //{amount}default value : 5 Len : 4
                packet.WriteInt16(item == null ? 0 : item.Weight); //{weight}default value : 50 Len : 2
                packet.WriteInt32(chr.Money); //{gold}default value : 4445 Len : 4
                packet.WriteInt16(chr.Asda2Inventory.Weight); //{curWeight}default value : 7069 Len : 2
                chr.Client.Send(packet, true);
            }
        }*/
        #endregion

        #region sellItem

        [PacketHandler(RealmServerOpCode.SellItem)] //5018
        public static void SellItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            var items = new ItemStub[5];
            for (int i = 0; i < 5; i += 1)
            {
                var cell = packet.ReadInt16(); //default : 0Len : 4
                packet.Position += 2;
                var inventory = packet.ReadByte(); //default : 1Len : 1
                var quantity = packet.ReadInt32(); //default : 0Len : 4
                var weight = packet.ReadInt16(); //default : 0Len : 2
                items[i] = new ItemStub() { Cell = cell, Inv = (Asda2InventoryType)inventory, Amount = quantity };
            }
            client.ActiveCharacter.Asda2Inventory.SellItems(items);
        }

        public static void SendSellItemResponseResponse(DeleteOrSellItemStatus status, Character chr,
                                                        List<Asda2Item> items)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SellItemResponse)) //5019
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                for (int i = 0; i < 5; i += 1)
                {
                    var item = items[i];
                    packet.WriteInt32(item == null ? 0 : item.Slot); //{cell}default value : 0 Len : 4
                    packet.WriteByte((byte)(item == null ? 0 : item.InventoryType)); //{inv}default value : 1 Len : 1
                    packet.WriteInt32(item == null ? 0 : item.Amount); //{amount}default value : -1 Len : 4
                    packet.WriteInt16(item == null ? 0 : item.Weight); //{weight}default value : 0 Len : 2
                }
                packet.WriteInt32(chr.Money); //{gold}default value : 0 Len : 4
                packet.WriteInt16(chr.Asda2Inventory.Weight); //{curWeight}default value : 0 Len : 2
                chr.Client.Send(packet, addEnd: true);
            }
        }

        #endregion

        #region buyItem

        public static void SendBuyItemResponse(Asda2BuyItemStatus status, Character chr, Asda2Item[] items)
        {
            if (items.Length != 7)
            {
                var t = items;
                items = new Asda2Item[7];
                Array.Copy(t, items, t.Length <= 7 ? t.Length : 7);
            }
            using (var packet = new RealmPacketOut(RealmServerOpCode.BuyItemResponse)) //5021
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                for (int i = 0; i < 7; i += 1)
                {
                    var item = items[i];
                    WriteItemInfoToPacket(packet, item, false);
                    /*packet.WriteInt32(item == null ? 0 : item.ItemId); //{itemID}default value : 28516 Len : 4
                    packet.WriteByte((byte) (item == null ? 0 : item.InventoryType));                        //{bagNum}default value : 1 Len : 1
                    packet.WriteInt32(item == null ? -1 : item.Slot); //{cellNum}default value : 0 Len : 4
                    packet.WriteInt32(item == null ? -1 : item.Amount); //{quantity}default value : 0 Len : 4
                    packet.WriteByte(item == null ? -1 : item.Durability); //{durability}default value : 100 Len : 1
                    packet.WriteInt16(item == null ? -1 : item.Weight); //{weight}default value : 677 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Soul1Id); //{soul1Id}default value : 7576 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Soul2Id); //{soul2Id}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Soul3Id); //{soul3Id}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Soul4Id); //{soul4Id}default value : -1 Len : 2
                    packet.WriteByte(item == null ? -1 : item.Enchant); //{enchant}default value : 0 Len : 1
                    packet.WriteSkip(Stab31); //value name : stab31 default value : stab31Len : 3
                    packet.WriteByte(item == null ? -1 : item.SealCount); //{sealCount}default value : 0 Len : 1
                    packet.WriteInt16(item == null ? -1 : (short) item.Parametr1Type);
                    packet.WriteInt16(item == null ? -1 : item.Parametr1Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item == null ? -1 : (short) item.Parametr2Type);
                    packet.WriteInt16(item == null ? -1 : item.Parametr2Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item == null ? -1 : (short) item.Parametr3Type);//{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Parametr3Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item == null ? -1 : (short) item.Parametr4Type);//{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Parametr4Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item == null ? -1 : (short) item.Parametr5Type);//{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Parametr5Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteByte(0); //value name : unk15 default value : 0Len : 1
                    packet.WriteByte(item == null ? -1 : item.IsSoulbound ? 1 : 0); //{equiped}default value : 0 Len : 1
                    packet.WriteInt32(0); //value name : unk17 default value : 0Len : 4
                    packet.WriteInt16(0); //value name : unk18 default value : 0Len : 2*/

                }
                packet.WriteInt32(chr.Money); //{gold}default value : 7098 Len : 4
                packet.WriteInt16(chr.Asda2Inventory.Weight); //{weight}default value : 5431 Len : 2
                chr.Client.Send(packet, addEnd: false);
            }
        }

        [PacketHandler(RealmServerOpCode.BuyItem)] //5020
        public static void BuyItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            var items = new List<ItemStub>();
            for (int i = 0; i < 7; i += 1)
            {
                var itemId = packet.ReadUInt16(); //default : 1043Len : 2
                var isEquipment = packet.ReadInt16(); //default : 1Len : 2
                var quantity = packet.ReadInt16(); //default : -1Len : 2
                items.Add(new ItemStub() { Amount = quantity, ItemId = itemId });
            }
            client.ActiveCharacter.Asda2Inventory.BuyItems(items);
        }

        #endregion

        #region PickUp

        [PacketHandler(RealmServerOpCode.PickUpItem)] //5012
        public static void PickUpItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            var x = packet.ReadInt16(); //default : 0Len : 2
            var y = packet.ReadInt16(); //default : 0Len : 2
            var x1 = packet.ReadInt16(); //default : 0Len : 2
            var y1 = packet.ReadInt16(); //default : 0Len : 2
            var chr = client.ActiveCharacter;
            chr.Asda2Inventory.TryPickUpItem(x1, y1);
        }
        public static void UpdateItemInventoryInfo(IRealmClient client, Asda2Item item)
        {
            if (item.InventoryType == Asda2InventoryType.Regular)
                using (var packet = new RealmPacketOut(RealmServerOpCode.RegularInventoryInfo)) //4048
                {
                    WriteItemInfoToPacket(packet, item, false);
                    client.Send(packet, addEnd: false);
                }
            else
                using (var packet = new RealmPacketOut(RealmServerOpCode.ShopInventoryInfo)) //4049
                {
                    WriteItemInfoToPacket(packet, item, false);
                    client.Send(packet, addEnd: false);
                }
        }

        public static void SendItemPickupedResponse(Asda2PickUpItemStatus status, Asda2Item item, Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemPickuped)) //5013
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                packet.WriteInt16(-1); //value name : unk5 default value : -1Len : 2
                WriteItemInfoToPacket(packet, item, false);
                packet.WriteInt16(chr.Asda2Inventory.Weight); //{weight}default value : 5571 Len : 2
                chr.Client.Send(packet, addEnd: true);
            }

        }

        private static readonly byte[] Stab31 = new byte[] { 0x00, 0x00, 0x00 };

        #endregion

        #region upgrade

        [PacketHandler(RealmServerOpCode.UpgradeItemRequest)] //6504
        public static void UpgradeItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            var itemId = packet.ReadInt32(); //default : 28566Len : 4
            var itemInv = packet.ReadByte(); //default : 1Len : 1
            var itemCell = packet.ReadInt16(); //default : 0Len : 2
            var stoneInv = packet.ReadByte(); //default : 2Len : 1
            var stoneCell = packet.ReadInt16(); //default : 3Len : 2
            var chanceBoostInv = packet.ReadByte(); //default : 1Len : 1
            var chanceBoostCell = packet.ReadInt16(); //default : 28Len : 2
            var protectScrollInv = packet.ReadByte(); //default : 1Len : 1
            var protectScrollCell = packet.ReadInt16(); //default : 25Len : 2
            client.ActiveCharacter.Asda2Inventory.UpgradeItem(itemCell, stoneCell, chanceBoostCell,
                                                              protectScrollCell);
        }



        public static void SendUpgradeItemResponse(IRealmClient client, UpgradeItemStatus status, Asda2Item upgradedItem,
                                                   Asda2Item stone,
                                                   Asda2Item successItem, Asda2Item protectionItem, int inventoryWeight,
                                                   uint money)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.UpgradeItemResponse)) //6505
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                //value name : unk4 default value : 0Len : 4
                packet.WriteInt32(client.ActiveCharacter.Money); //value name : unk4 default value : 0Len : 4
                WriteItemInfoToPacket(packet, upgradedItem, false);
                WriteItemInfoToPacket(packet, stone, false);
                WriteItemInfoToPacket(packet, successItem, false);
                WriteItemInfoToPacket(packet, protectionItem, false);
                client.Send(packet, addEnd: false);
            }
        }


        #endregion

        #region option exchange

        [PacketHandler(RealmServerOpCode.ExchangeOption)] //6553
        public static void ExchangeOptionRequest(IRealmClient client, RealmPacketIn packet)
        {
            var exchangiItemId = packet.ReadInt32(); //default : 115Len : 4
            var scrollCell = packet.ReadInt16(); //default : 26Len : 2
            var scrollInv = packet.ReadInt16(); //default : 1Len : 2
            var itemId = packet.ReadInt32(); //default : 28144Len : 4
            var itemSlot = packet.ReadInt16(); //default : 3Len : 2
            client.ActiveCharacter.Asda2Inventory.ExchangeItemOptions(scrollCell, itemSlot);
        }

        public static void SendExchangeItemOptionResultResponse(IRealmClient client, ExchangeOptionResult status,
                                                                Asda2Item item, Asda2Item exchangeToken)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ExchangeItemOptionResult)) //6554
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                WriteItemInfoToPacket(packet, exchangeToken, false);
                WriteItemInfoToPacket(packet, item, false);
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                packet.WriteInt16(0);
                client.Send(packet);
            }
        }

        #endregion

        #region soweling

        [PacketHandler(RealmServerOpCode.MoveSoulStoneIn)] //5024
        public static void MoveSoulStoneInRequest(IRealmClient client, RealmPacketIn packet)
        {
            var itemId = packet.ReadInt32(); //default : 23541Len : 4
            var itemInv = packet.ReadByte(); //default : 1Len : 1
            var itemSlot = packet.ReadInt16(); //default : 4Len : 2
            var sowelSlot = packet.ReadByte(); //default : 1Len : 1
            var sowelId = packet.ReadInt16(); //default : 14673Len : 2
            var sowelInv = packet.ReadByte(); //default : 2Len : 1
            var sowelCell = packet.ReadInt16(); //default : 0Len : 2
            var protectItemId = packet.ReadInt16();
            var protectInv = packet.ReadByte(); //default : 0Len : 1
            var protectSlot = packet.ReadInt16(); //default : 0Len : 2
            client.ActiveCharacter.Asda2Inventory.SowelItem(itemSlot, sowelCell, sowelSlot, protectSlot);
        }

        public static void SendItemSoweledResponse(IRealmClient client, int inventoryWeight, int money,
                                                   SowelingStatus status, Asda2Item item, Asda2Item stone,
                                                   Asda2Item protect, bool isAvatar = false)
        {
            using (var packet = new RealmPacketOut(isAvatar ? RealmServerOpCode.AvatarSocketed : RealmServerOpCode.ItemSoweled)) //5025
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                if (isAvatar) packet.WriteInt16(0);
                packet.WriteInt32(inventoryWeight); //{weight}default value : 11048 Len : 4
                packet.WriteInt32(money); //{money}default value : 23740702 Len : 4
                WriteItemInfoToPacket(packet, item, false);
                WriteItemInfoToPacket(packet, stone, false);
                WriteItemInfoToPacket(packet, protect, false);
                client.Send(packet, addEnd: false);
            }
        }

        [PacketHandler(RealmServerOpCode.SoketAvatar)]//6635
        public static void SoketAvatarRequest(IRealmClient client, RealmPacketIn packet)
        {
            var itemId = packet.ReadInt32();//default : 23541Len : 4
            var itemSlot = packet.ReadInt16();//default : 4Len : 2
            var sowelSlot = packet.ReadByte();//default : 1Len : 1
            var sowelId = packet.ReadInt16();//default : 14673Len : 2
            var sovelCell = packet.ReadInt16();//default : 1Len : 1
            client.ActiveCharacter.Asda2Inventory.SowelItem(itemSlot, sovelCell, sowelSlot, -1, true);
        }



        public static void WriteItemInfoToPacket(RealmPacketOut packet, Asda2Item item, bool setAmountTo0WhenDeleted = false)
        {
            packet.WriteInt32(item == null ? 0 : item.ItemId); //{itemID}default value : 28516 Len : 4
            packet.WriteByte((byte)(item == null ? 0 : item.InventoryType)); //{bagNum}default value : 1 Len : 1
            packet.WriteInt16(item == null ? -1 : item.Slot); //{cellNum}default value : 0 Len : 4
            packet.WriteInt16(item == null ? -1 : item.IsDeleted ? -1 : 0);
            packet.WriteItemAmount(item); //{quantity}default value : 0 Len : 4
            packet.WriteByte(item == null ? -1 : item.Durability); //{durability}default value : 100 Len : 1
            packet.WriteInt16(item == null ? -1 : item.Weight); //{weight}default value : 677 Len : 2
            packet.WriteInt16(item == null ? -1 : item.Soul1Id); //{soul1Id}default value : 7576 Len : 2
            packet.WriteInt16(item == null ? -1 : item.Soul2Id); //{soul2Id}default value : -1 Len : 2
            packet.WriteInt16(item == null ? -1 : item.Soul3Id); //{soul3Id}default value : -1 Len : 2
            packet.WriteInt16(item == null ? -1 : item.Soul4Id); //{soul4Id}default value : -1 Len : 2
            packet.WriteByte(item == null ? -1 : item.Enchant); //{enchant}default value : 0 Len : 1
            packet.WriteSkip(Stab31); //value name : stab31 default value : stab31Len : 3
            packet.WriteByte(item == null ? -1 : item.SealCount); //{sealCount}default value : 0 Len : 1
            packet.WriteInt16(item == null ? -1 : (short)item.Parametr1Type);
            packet.WriteInt16(item == null ? -1 : item.Parametr1Value); //{stat1Value}default value : 9 Len : 2
            packet.WriteInt16(item == null ? -1 : (short)item.Parametr2Type);
            packet.WriteInt16(item == null ? -1 : item.Parametr2Value); //{stat1Value}default value : 9 Len : 2
            packet.WriteInt16(item == null ? -1 : (short)item.Parametr3Type); //{stat1Type}default value : 1 Len : 2
            packet.WriteInt16(item == null ? -1 : item.Parametr3Value); //{stat1Value}default value : 9 Len : 2
            packet.WriteInt16(item == null ? -1 : (short)item.Parametr4Type); //{stat1Type}default value : 1 Len : 2
            if (item != null && item.Template.IsAvatar)
            {
                var t1 = Asda2ItemMgr.GetTemplate(item.Soul2Id);
                var t2 = Asda2ItemMgr.GetTemplate(item.Soul3Id);
                packet.WriteInt16(t1 == null ? -1 : t1.SowelBonusValue); //{stat1Value}default value : 9 Len : 2
                packet.WriteInt16(-1); //{stat1Type}default value : 1 Len : 2
                packet.WriteInt16(t2 == null ? -1 : t2.SowelBonusValue);
            }
            else
            {
                packet.WriteInt16(item == null ? -1 : item.Parametr4Value); //{stat1Value}default value : 9 Len : 2
                packet.WriteInt16(item == null ? -1 : (short)item.Parametr5Type); //{stat1Type}default value : 1 Len : 2
                packet.WriteInt16(item == null ? -1 : item.Parametr5Value); //{stat1Value}default value : 9 Len : 2
            }
            packet.WriteByte(0); //value name : unk15 default value : 0Len : 1
            packet.WriteByte(item == null ? -1 : item.IsSoulbound ? 1 : 0); //{equiped}default value : 0 Len : 1
            packet.WriteInt32(0); //value name : unk17 default value : 0Len : 4
            packet.WriteInt16(0); //value name : unk18 default value : 0Len : 2
        }

        [PacketHandler(RealmServerOpCode.MoveSoulStoneOut)]//5022
        public static void MoveSoulStoneOutRequest(IRealmClient client, RealmPacketIn packet)
        {
            var sowelNum = packet.ReadByte();//default : 1Len : 1
            packet.Position += 5;
            var itemSlot = packet.ReadInt16();//default : 25Len : 2
            packet.Position += 4;//nk12 default : 1Len : 1
            var removalToolSlot = packet.ReadInt16();//default : 17Len : 2

            var item = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(itemSlot);
            if (item == null || (!item.Template.IsEquipment && !item.Template.IsAvatar))
            {
                client.ActiveCharacter.SendInfoMsg("Item not found. Restart game please.");
                SendSowelRemovedResponse(client, SowelRemovedStatus.Fail);
                return;
            }
            var removalItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(removalToolSlot);
            if (removalItem == null ||
                (removalItem.ItemId != 576 && removalItem.ItemId != 577 && removalItem.ItemId != 578 &&
                 removalItem.ItemId != 598))
            {
                client.ActiveCharacter.SendInfoMsg("Item not found. Restart game please.");
                SendSowelRemovedResponse(client, SowelRemovedStatus.Fail);
                return;
            }
            if (client.ActiveCharacter.Asda2Inventory.FreeRegularSlotsCount < 1)
            {
                client.ActiveCharacter.SendInfoMsg("Not enought inventory space.");
                SendSowelRemovedResponse(client, SowelRemovedStatus.Fail);
                return;
            }
            Asda2Item newSowel = null;

            var srcLog = Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                                                 .AddAttribute("source", 0, "remove_sowel_source_item")
                                                 .AddItemAttributes(item)
                                                 .Write();
            var remLog = Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                                                 .AddAttribute("source", 0, "remove_sowel_removal_item")
                                                 .AddItemAttributes(removalItem)
                                                 .Write();
            switch (sowelNum)
            {
                case 2:
                    if (item.Soul2Id == 0)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Sowel not found.", 50);
                        SendSowelRemovedResponse(client, SowelRemovedStatus.Fail);
                        return;
                    }
                    client.ActiveCharacter.Asda2Inventory.TryAdd(item.Soul2Id, 1, true,
                                                                 ref newSowel);
                    item.Soul2Id = 0;
                    break;
                case 3:
                    if (item.Soul3Id == 0)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Sowel not found.", 50);
                        SendSowelRemovedResponse(client, SowelRemovedStatus.Fail);
                        return;
                    }
                    client.ActiveCharacter.Asda2Inventory.TryAdd(item.Soul3Id, 1, true,
                                                                 ref newSowel);
                    item.Soul3Id = 0;
                    break;
                case 4:
                    if (item.Soul4Id == 0)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater("Sowel not found.", 50);
                        SendSowelRemovedResponse(client, SowelRemovedStatus.Fail);
                        return;
                    }
                    client.ActiveCharacter.Asda2Inventory.TryAdd(item.Soul4Id, 1, true,
                                                                 ref newSowel);
                    item.Soul4Id = 0;
                    break;
                default:
                    client.ActiveCharacter.YouAreFuckingCheater("Sowel not found.", 50);
                    SendSowelRemovedResponse(client, SowelRemovedStatus.Fail);
                    return;
            }
            removalItem.Amount--;
            removalItem.Save();
            item.Save();
            Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                                                 .AddAttribute("source", 0, "remove_sowel_new_item")
                                                 .AddItemAttributes(newSowel)
                                                 .AddReference(remLog)
                                                 .AddReference(srcLog)
                                                 .Write();
            SendSowelRemovedResponse(client, SowelRemovedStatus.Ok, item, newSowel, removalItem);


        }
        public static void SendSowelRemovedResponse(IRealmClient client, SowelRemovedStatus status, Asda2Item mainItem = null, Asda2Item sowel = null, Asda2Item tool = null)
        {
            var items = new Asda2Item[3];
            items[0] = mainItem;
            items[1] = sowel;
            items[2] = tool;
            using (var packet = new RealmPacketOut(RealmServerOpCode.SowelRemoved))//5023
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                for (int i = 0; i < 3; i += 1)
                {
                    var item = items[i];
                    WriteItemInfoToPacket(packet, item, false);

                }
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);//{weight}default value : 0 Len : 4
                client.Send(packet, addEnd: true);
            }
        }
        static readonly byte[] stab31 = new byte[] { 0x00, 0x00, 0x00 };


        #endregion

        #region Booster

        [PacketHandler(RealmServerOpCode.OpenBooster)] //5468
        public static void OpenBoosterRequest(IRealmClient client, RealmPacketIn packet)
        {
            var boosterId = packet.ReadInt32(); //default : 70299Len : 4
            var inv = (Asda2InventoryType)packet.ReadByte(); //default : 2Len : 1
            var cell = packet.ReadInt16(); //default : 13Len : 2

            var status = client.ActiveCharacter.Asda2Inventory.OpenBooster(inv, cell);
            client.ActiveCharacter.Map.AddMessage(() =>
            {
                if (status != OpenBosterStatus.Ok)
                    SendbosterOpenedResponse(client, status, null, inv, cell, 0);
            });
        }

        public static void SendbosterOpenedResponse(IRealmClient client, OpenBosterStatus status, Asda2Item addedItem, Asda2InventoryType boosterInv, short boosterCell, Int16 weight)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.bosterOpened)) //5469
            {
                packet.WriteByte((byte)status); //{status}default value : 0 Len : 1
                packet.WriteInt16(11); //value name : unk4 default value : 11Len : 2
                packet.WriteInt16(119); //value name : unk6 default value : 119Len : 2
                packet.WriteByte(0); //value name : unk7 default value : 0Len : 1
                packet.WriteByte((byte)boosterInv); //{boosterInv}default value : 2 Len : 1
                packet.WriteInt16(boosterCell); //{boosterCell}default value : 3 Len : 2
                WriteItemInfoToPacket(packet, addedItem, false);
                packet.WriteInt16(weight); //{weight}default value : 2910 Len : 2
                if (status == OpenBosterStatus.Ok)
                    client.ActiveCharacter.Send(packet, addEnd: true);
                else
                    client.Send(packet, addEnd: false);
            }
        }


        #endregion

        #region package

        [PacketHandler(RealmServerOpCode.OpenPackage)] //6096
        public static void OpenPackageRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 3; //tab36 default : stab36Len : 1
            var packageInv = (Asda2InventoryType)packet.ReadByte(); //default : 1Len : 1
            var packageSlot = packet.ReadInt16(); //default : 6Len : 2

            var result = client.ActiveCharacter.Asda2Inventory.OpenPackage(packageInv, packageSlot);
            client.ActiveCharacter.Map.AddMessage(() =>
            {
                if (result != OpenPackageStatus.Ok)
                    SendOpenPackageResponseResponse(client, result, null, packageInv, packageSlot, 0);
            });
        }

        public static void SendOpenPackageResponseResponse(IRealmClient client, OpenPackageStatus status, List<Asda2Item> addedItems, Asda2InventoryType packageInv, short packageSlot, short weight)
        {
            var itemsCount = addedItems == null ? 0 : addedItems.Count;
            var items = new Asda2Item[5];
            var i = 0;
            if (addedItems != null)
                foreach (var asda2Item in addedItems)
                {
                    items[i] = asda2Item;
                    i++;
                }
            using (var packet = new RealmPacketOut(RealmServerOpCode.OpenPackageResponse))//6097
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{sessd}default value : 35 Len : 2
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 340701 Len : 4
                packet.WriteByte(itemsCount);//{itemsCount}default value : 1 Len : 1
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteByte((byte)packageInv);//{packageInv}default value : 1 Len : 1
                packet.WriteInt16(packageSlot);//{packageSlot}default value : 8 Len : 2
                foreach (var asda2Item in items)
                {
                    WriteItemInfoToPacket(packet, asda2Item, false);
                }
                packet.WriteInt32(weight);//{weight}default value : 2911 Len : 4
                client.Send(packet, addEnd: false);
            }
        }
        #endregion

        #region disasemble
        [PacketHandler(RealmServerOpCode.DisasembleEquipment)]//6502
        public static void DisasembleEquipmentRequest(IRealmClient client, RealmPacketIn packet)
        {
            var itemId = packet.ReadInt32(); //default : 23451Len : 4
            var invNum = (Asda2InventoryType)packet.ReadByte(); //default : 1Len : 1
            var slot = packet.ReadInt16(); //default : 7Len : 2

            var status = client.ActiveCharacter.Asda2Inventory.DisasembleItem(invNum, slot);
            client.ActiveCharacter.Map.AddMessage(() =>
            {
                if (status != DisasembleItemStatus.Ok)
                    SendEquipmentDisasembledResponse(client, status, 0, null, slot);
            });
        }

        public static void SendEquipmentDisasembledResponse(IRealmClient client, DisasembleItemStatus status, Int16 weight, Asda2Item addedItem, short srcSlot)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.EquipmentDisasembled))//6503
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt16(weight);//{weight}default value : 2093 Len : 2
                packet.WriteInt32(0);//value name : unk6 default value : 0Len : 4
                packet.WriteByte((byte)Asda2InventoryType.Shop);//{srcInvNum}default value : 1 Len : 1
                packet.WriteInt16(srcSlot);//{srcSlot}default value : 7 Len : 2
                packet.WriteSkip(stab16);//value name : stab16 default value : stab16Len : 50
                WriteItemInfoToPacket(packet, addedItem, false);
                /*packet.WriteInt32(itemId);//{itemId}default value : 20642 Len : 4
                packet.WriteByte(invNum);//{invNum}default value : 2 Len : 1
                packet.WriteInt16(slot);//{slot}default value : 5 Len : 2
                packet.WriteInt16(-1);//value name : unk10 default value : -1Len : 2
                packet.WriteInt32(quantity);//{quantity}default value : 1 Len : 4
                packet.WriteByte(durability);//{durability}default value : 0 Len : 1
                packet.WriteInt16(weight0);//{weight0}default value : 21 Len : 2
                packet.WriteInt16(soul1Id);//{soul1Id}default value : -1 Len : 2
                packet.WriteInt16(soul2Id);//{soul2Id}default value : -1 Len : 2
                packet.WriteInt16(soul3Id);//{soul3Id}default value : -1 Len : 2
                packet.WriteInt16(soul4Id);//{soul4Id}default value : -1 Len : 2
                packet.WriteInt16(enchant);//{enchant}default value : 0 Len : 2
                packet.WriteInt16(0);//value name : unk21 default value : 0Len : 2
                packet.WriteByte(0);//value name : unk22 default value : 0Len : 1
                packet.WriteInt16(parametr1Type);//{parametr1Type}default value : -1 Len : 2
                packet.WriteInt16(paramtetr1Value);//{paramtetr1Value}default value : -1 Len : 2
                packet.WriteInt16(parametr2Type);//{parametr2Type}default value : -1 Len : 2
                packet.WriteInt16(paramtetr2Value);//{paramtetr2Value}default value : -1 Len : 2
                packet.WriteInt16(parametr3Type);//{parametr3Type}default value : -1 Len : 2
                packet.WriteInt16(paramtetr3Value);//{paramtetr3Value}default value : -1 Len : 2
                packet.WriteInt16(parametr4Type);//{parametr4Type}default value : -1 Len : 2
                packet.WriteInt16(paramtetr4Value);//{paramtetr4Value}default value : -1 Len : 2
                packet.WriteInt16(parametr5Type);//{parametr5Type}default value : -1 Len : 2
                packet.WriteInt16(paramtetr5Value);//{paramtetr5Value}default value : -1 Len : 2
                packet.WriteByte(0);//value name : unk33 default value : 0Len : 1
                packet.WriteByte(isDressed);//{isDressed}default value : 0 Len : 1
                packet.WriteInt32(0);//value name : unk35 default value : 0Len : 4
                packet.WriteInt16(0);//value name : unk36 default value : 0Len : 2*/
                packet.WriteInt16(addedItem == null ? 0 : addedItem.Weight);//{cellWeight}default value : 1 Len : 2
                client.Send(packet, addEnd: false);
            }
        }

        static readonly byte[] stab16 = new byte[] { 0x07, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        #endregion

        #region WarShop
        [PacketHandler(RealmServerOpCode.ByuItemFromWarshop)]//6162
        public static void ByuItemFromWarshopRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            var internalWarShopId = packet.ReadInt16();//default : 3Len : 2
            RealmServer.IOQueue.AddMessage(() =>
            {
                BuyFromWarShopStatus status =
                    client.ActiveCharacter.Asda2Inventory.BuyItemFromWarshop(internalWarShopId);
                client.ActiveCharacter.Map.AddMessage(() =>
                {
                    if (status != BuyFromWarShopStatus.Ok)
                        SendItemFromWarshopBuyedResponse(client, status, 0, 0, null, null);
                });
            });
        }
        public static void SendItemFromWarshopBuyedResponse(IRealmClient client, BuyFromWarShopStatus status, short invWeight, int money, Asda2Item moneyItem, Asda2Item buyedItem)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemFromWarshopBuyed)) //6163
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                packet.WriteInt16(invWeight); //{invWeight}default value : 12170 Len : 2
                packet.WriteInt32(money); //{money}default value : 23348552 Len : 4
                WriteItemInfoToPacket(packet, moneyItem, false);
                WriteItemInfoToPacket(packet, buyedItem, false);
                client.Send(packet, addEnd: false);
            }
        }


        #endregion

        #region warhouse
        [PacketHandler(RealmServerOpCode.ShowWarehouse)]//4050
        public static void ShowWarehouseRequest(IRealmClient client, RealmPacketIn packet)
        {
            var bagNum = packet.ReadByte();//default : 0Len : 1

            if (bagNum == 0 || bagNum - 1 > client.ActiveCharacter.Record.PremiumWarehouseBagsCount)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Is trying to show premium warehouse bags that not owned.", 50);
                return;
            }
            SendShowWarehouseItemsResponse(client, (byte)(bagNum - 1), false);
        }
        [PacketHandler(RealmServerOpCode.ShowAvatarWhItems)]//4210
        public static void ShowAvatarWhItemsRequest(IRealmClient client, RealmPacketIn packet)
        {
            var bagNum = packet.ReadByte();//default : 1Len : 1
            if (bagNum == 0 || bagNum - 1 > client.ActiveCharacter.Record.PremiumAvatarWarehouseBagsCount)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Is trying to show premium warehouse bags that not owned.", 50);
                return;
            }
            SendShowWarehouseItemsResponse(client, (byte)(bagNum - 1), true);
        }
        [PacketHandler(RealmServerOpCode.PushToWarehouse)]//5008
        public static void PushToWarehouseRequest(IRealmClient client, RealmPacketIn packet)
        {
            var itemStubs = ReadItemStubs(packet);
            client.ActiveCharacter.Asda2Inventory.PushItemsToWh(itemStubs);
        }

        private static IEnumerable<Asda2WhItemStub> ReadItemStubs(RealmPacketIn packet)
        {
            var itemStubs = new Asda2WhItemStub[5];
            for (int i = 0; i < 5; i++)
            {
                var cell = packet.ReadInt16();//default : -1Len : 2
                packet.Position += 2;//nk2 default : -1Len : 2
                var inv = packet.ReadByte();//default : 0Len : 1
                var amount = packet.ReadInt32();//default : -1Len : 4
                var weight = packet.ReadInt16();//default : -1Len : 2
                itemStubs[i] = new Asda2WhItemStub
                {
                    Amount = amount == 0 ? 1 : amount,
                    Invtentory = (Asda2InventoryType)inv,
                    Slot = cell,
                    Weight = weight
                };
            }
            return itemStubs.Where(itemStub => itemStub.Slot != -1).ToArray();
        }

        [PacketHandler(RealmServerOpCode.StoreAvatarItems)]//6258
        public static void StoreAvatarItemsRequest(IRealmClient client, RealmPacketIn packet)
        {
            var itemStubs = ReadItemStubs(packet);
            client.ActiveCharacter.Asda2Inventory.PushItemsToAvatarWh(itemStubs);
        }


        public static void SendItemsPushedToWarehouseResponse(IRealmClient client, PushItemToWhStatus status, IEnumerable<Asda2WhItemStub> sourceItemStubs = null, IEnumerable<Asda2WhItemStub> destItemStubs = null)
        {
            var sourceItems = CreateArrayOfFiveElementsFromEnumerable(sourceItemStubs);
            var destItems = CreateArrayOfFiveElementsFromEnumerable(destItemStubs);
            var fullArray = sourceItems.Concat(destItems);
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemsPushedToWarehouse))
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                if (status == PushItemToWhStatus.Ok)
                {
                    foreach (var item in fullArray)
                    {
                        packet.WriteInt32(item == null ? -1 : item.Slot); //{dstInvSlot}default value : 9 Len : 4
                        packet.WriteByte(item == null ? 0 : (byte)item.Invtentory);//{invType}default value : 2 Len : 1
                        packet.WriteInt32(item == null ? -1 : item.Amount); //{amount}default value : 70 Len : 4
                        packet.WriteInt16(item == null ? 0 : item.Weight); //{weight}default value : 490 Len : 2
                    }
                    packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                }
                client.Send(packet);
            }
        }

        public static void SendItemsPushedToAvatarWarehouseResponse(IRealmClient client, PushItemToWhStatus status, IEnumerable<Asda2WhItemStub> sourceItemStubs = null, IEnumerable<Asda2WhItemStub> destItemStubs = null)
        {
            var sourceItems = CreateArrayOfFiveElementsFromEnumerable(sourceItemStubs);
            var destItems = CreateArrayOfFiveElementsFromEnumerable(destItemStubs);
            var fullArray = sourceItems.Concat(destItems);
            using (var packet = new RealmPacketOut(RealmServerOpCode.AvatarItemsStored))
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                if (status == PushItemToWhStatus.Ok)
                {
                    foreach (var item in fullArray)
                    {
                        packet.WriteInt32(item == null ? -1 : item.Slot); //{dstInvSlot}default value : 9 Len : 4
                        packet.WriteByte(item == null ? 0 : (byte)item.Invtentory);//{invType}default value : 2 Len : 1
                        packet.WriteInt32(item == null ? -1 : item.Amount); //{amount}default value : 70 Len : 4
                        packet.WriteInt16(item == null ? 0 : item.Weight); //{weight}default value : 490 Len : 2
                    }
                    packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                }
                client.Send(packet);
            }
        }

        public static void SendItemsTakedFromWarehouseResponse(IRealmClient client, PushItemToWhStatus status, IEnumerable<Asda2WhItemStub> sourceItemStubs = null, IEnumerable<Asda2WhItemStub> destItemStubs = null)
        {
            var sourceItems = CreateArrayOfFiveElementsFromEnumerable(sourceItemStubs);
            var destItems = CreateArrayOfFiveElementsFromEnumerable(destItemStubs);
            var fullArray = destItems.Concat(sourceItems);
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemFormWarehouseTaked))
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                if (status == PushItemToWhStatus.Ok)
                {
                    foreach (var item in fullArray)
                    {
                        packet.WriteInt32(item == null ? -1 : item.Slot); //{dstInvSlot}default value : 9 Len : 4
                        packet.WriteByte(item == null ? 0 : (byte)item.Invtentory);//{invType}default value : 2 Len : 1
                        packet.WriteInt32(item == null ? -1 : item.Amount); //{amount}default value : 70 Len : 4
                        packet.WriteInt16(item == null ? 0 : item.Weight); //{weight}default value : 490 Len : 2
                    }
                    packet.WriteInt32(client.ActiveCharacter.Money);
                    packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                }
                client.Send(packet);
            }
        }

        public static void SendItemsTakedFromAvatarWarehouseResponse(IRealmClient client, PushItemToWhStatus status, IEnumerable<Asda2WhItemStub> sourceItemStubs = null, IEnumerable<Asda2WhItemStub> destItemStubs = null)
        {
            var sourceItems = CreateArrayOfFiveElementsFromEnumerable(sourceItemStubs);
            var destItems = CreateArrayOfFiveElementsFromEnumerable(destItemStubs);
            var fullArray = destItems.Concat(sourceItems);
            using (var packet = new RealmPacketOut((RealmServerOpCode)6261))
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                if (status == PushItemToWhStatus.Ok)
                {
                    foreach (var item in fullArray)
                    {
                        packet.WriteInt32(item == null ? -1 : item.Slot); //{dstInvSlot}default value : 9 Len : 4
                        packet.WriteByte(item == null ? 0 : (byte)item.Invtentory);//{invType}default value : 2 Len : 1
                        packet.WriteInt32(item == null ? -1 : item.Amount); //{amount}default value : 70 Len : 4
                        packet.WriteInt16(item == null ? 0 : item.Weight); //{weight}default value : 490 Len : 2
                    }
                    packet.WriteInt32(client.ActiveCharacter.Money);
                    packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                }
                client.Send(packet);
            }
        }


        [PacketHandler(RealmServerOpCode.TakeItemFromWarehouse)]//5010
        public static void TakeItemFromWarehouseRequest(IRealmClient client, RealmPacketIn packet)
        {
            var itemStubs = ReadItemStubs(packet);
            client.ActiveCharacter.Asda2Inventory.TakeItemsFromWh(itemStubs);
        }

        [PacketHandler(RealmServerOpCode.RetriveItemsFromAvatarWh)]//6260
        public static void RetriveItemsFromAvatarWhRequest(IRealmClient client, RealmPacketIn packet)
        {
            var itemStubs = ReadItemStubs(packet);
            client.ActiveCharacter.Asda2Inventory.TakeItemsFromAvatarWh(itemStubs);
        }

        private static IEnumerable<Asda2WhItemStub> CreateArrayOfFiveElementsFromEnumerable(IEnumerable<Asda2WhItemStub> itemStubs)
        {
            var array = new Asda2WhItemStub[5];
            if (itemStubs == null)
                return array;
            var i = 0;
            foreach (var itemStub in itemStubs)
            {
                array[i] = itemStub;
                i++;
            }
            return array;
        }





        public static void SendShowWarehouseItemsResponse(IRealmClient client, byte page, bool isAvatar)
        {
            var inventory = client.ActiveCharacter.Asda2Inventory;
            var inventoryPacks = new List<List<Asda2Item>>();
            var itemIndex = 0;
            var allItems = isAvatar ? inventory.AvatarWarehouseItems.Skip(page == 255 ? 0 : page * 30).Take(page == 255 ? 270 : 30).Where(it => it != null).ToArray() : inventory.WarehouseItems.Skip(page == 255 ? 0 : page * 30).Take(page == 255 ? 270 : 30).Where(it => it != null).ToArray();
            while (itemIndex < allItems.Length)
            {
                inventoryPacks.Add(new List<Asda2Item>(allItems.Skip(itemIndex).Take(10)));
                itemIndex += 10;
            }
            foreach (var inventoryPack in inventoryPacks)
            {
                using (var packet = isAvatar ? new RealmPacketOut(RealmServerOpCode.AvatarWhItemsList) : new RealmPacketOut(RealmServerOpCode.ShowWarehouseItems)) //4051
                {
                    for (int i = 0; i < inventoryPack.Count; i += 1)
                    {
                        var item = inventoryPack[i];
                        if (item != null)
                            WriteItemInfoToPacket(packet, item, false);
                    }
                    client.Send(packet);
                }
            }
            if (isAvatar)
                SendAvatarWhItemsListEndedResponse(client);
            else
                SendShowWarehouseEndResponse(client);

        }

        public static void SendShowWarehouseEndResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ShowWarehouseEnd))//4052
            {
                client.Send(packet, addEnd: true);
            }
        }

        public static void SendAvatarWhItemsListEndedResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.AvatarWhItemsListEnded))//4212
            {
                client.Send(packet, addEnd: true);
            }
        }


        #endregion

        #region avatar disassemble\synteth

        [PacketHandler(RealmServerOpCode.DisassembleAvatar)]//6629
        public static void DisassembleAvatarRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 4;
            var slot = packet.ReadInt16();//default : 16Len : 2
            var item = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slot);
            if (item == null)
            {
                client.ActiveCharacter.SendInfoMsg("Avatar item not founded.");
                SendAvatarDissasembledResponse(client, AvatarDisassembleStatus.Fail, 0, 0, null);
                return;
            }
            var isPremium = item.Template.Quality == Asda2ItemQuality.Green ||
                            item.Template.Quality == Asda2ItemQuality.Orange ||
                            item.Template.Quality == Asda2ItemQuality.Purple;
            AvatarDisasembleRecord rec = null;
            if (isPremium)
            {
                foreach (var t in Asda2ItemMgr.PremiumAvatarRecords)
                {
                    if (t == null)
                        continue;
                    if (t.Level > client.ActiveCharacter.Level)
                    {
                        if (rec == null)
                            rec = t;
                        break;
                    }
                    rec = t;
                }
            }
            else
            {
                foreach (var t in Asda2ItemMgr.RegularAvatarRecords)
                {
                    if (t == null)
                        continue;
                    if (t.Level > client.ActiveCharacter.Level)
                        break;
                    rec = t;
                }
            }
            if (rec == null)
            {
                client.ActiveCharacter.SendInfoMsg("Avatar template not found.");
                SendAvatarDissasembledResponse(client, AvatarDisassembleStatus.Fail, 0, 0, null);
                return;
            }
            Asda2Item resItem = null;
            var res =
                client.ActiveCharacter.Asda2Inventory.TryAdd((int)rec.GetRandomItemId(), 1,
                                                             true, ref resItem);

            if (res != Asda2InventoryError.Ok)
            {
                client.ActiveCharacter.SendInfoMsg("Error " + res);
                SendAvatarDissasembledResponse(client, AvatarDisassembleStatus.Fail, 0, 0, null);
                return;
            }
            Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                                                 .AddAttribute("source", 0, "disassemble_avatar")
                                                 .AddItemAttributes(item, "source")
                                                 .AddItemAttributes(resItem, "result")
                                                 .Write();
            Asda2TitleChecker.OnAvatarDisasembled(client.ActiveCharacter, item, item.Template.Quality);
            item.Destroy();
            SendAvatarDissasembledResponse(client, AvatarDisassembleStatus.Ok, item.ItemId, item.Slot, resItem);

        }
        public static void SendAvatarDissasembledResponse(IRealmClient client, AvatarDisassembleStatus status, int avatarId, short avatarSlot, Asda2Item item)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.AvatarDissasembled))//6630
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt32(avatarId);//{avatarId}default value : 37408 Len : 4
                packet.WriteInt16(avatarSlot);//{avatarSlot}default value : 16 Len : 2
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);//{weight}default value : 10211 Len : 2
                WriteItemInfoToPacket(packet, item, false);
                client.Send(packet, addEnd: true);
            }
        }


        [PacketHandler(RealmServerOpCode.StartAvatarSynthesis)]//6631
        public static void StartAvatarSynthesisRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 4;
            var avatarItemSlot = packet.ReadInt16(); //default : 9Len : 2
            var avatarMaterialItemid = packet.ReadInt32();
            var avatarMaterialItemSlot = packet.ReadInt16(); //default : -1Len : 2
            var suplItemid = packet.ReadInt32();
            var suplItemSlot = packet.ReadInt16(); //default : -1Len : 2
            packet.Position += 4;
            var toolItemSlot = packet.ReadInt16(); //default : 16Len : 2
            var avatarItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(avatarItemSlot);
            var avatarMaterialItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(avatarMaterialItemSlot);
            var suplItem = suplItemid == 0 ? null : client.ActiveCharacter.Asda2Inventory.GetShopShopItem(suplItemSlot);
            var toolItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(toolItemSlot);
            if (avatarItem == null)
            {
                client.ActiveCharacter.SendInfoMsg("Item not found. Please restart client.");
                SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo);
                return;
            }
            if (toolItem == null && avatarMaterialItem == null)
            {
                client.ActiveCharacter.SendInfoMsg("Tool or avatar material item not found. Please restart client.");
                SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo);
                return;
            }
            if (toolItem != null && avatarMaterialItem != null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Use Tool and avatar material item same time.", 10);
                SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo);
                return;
            }
            if (!avatarItem.Template.IsAvatar)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to syntes not avatar item.", 50);
                SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo);
                return;
            }
            if (suplItem != null && suplItem.Category != Asda2ItemCategory.IncreseAvatarSynethisChanceByPrc)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to syntes with wrong supliment.", 50);
                SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo);
                return;
            }
            if (toolItem != null)
            {
                if (toolItem.Category != Asda2ItemCategory.OpenWarehouseAndRuneSocketTool)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to syntes with wrong tool.", 50);
                    SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo);
                    return;
                }
                toolItem.Amount--;
            }
            if (avatarMaterialItem != null)
            {
                if (!avatarMaterialItem.Template.IsAvatar)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to syntes with wrong avatar material item.", 50);
                    SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.AbnormalInfo);
                    return;
                }
                avatarMaterialItem.Amount--;
            }
            if (suplItem != null)
                suplItem.Amount--;
            var success = CharacterFormulas.IsAvatarSyntesSuccess(avatarItem.Enchant, suplItem != null, avatarMaterialItem == null ? Asda2ItemQuality.Orange : avatarMaterialItem.Template.Quality);
            if (success)
            {
                avatarItem.Enchant++;
                SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.Ok, avatarItem, toolItem ?? avatarMaterialItem, suplItem);
            }
            else
                SendAvatarSynthesisResultResponse(client, AvatarSyntesStatus.Fail, avatarItem, toolItem ?? avatarMaterialItem, suplItem);
        }

        public static void SendAvatarSynthesisResultResponse(IRealmClient client, AvatarSyntesStatus status, Asda2Item avatarItem = null, Asda2Item toolkitItem = null, Asda2Item suplItem = null)
        {
            var itms = new Asda2Item[3];
            itms[0] = avatarItem;
            itms[1] = toolkitItem;
            itms[2] = suplItem;
            using (var packet = new RealmPacketOut(RealmServerOpCode.AvatarSynthesisResult))//6632
            {
                packet.WriteByte((byte)status); //{result}default value : 1 Len : 1
                packet.WriteSkip(unk6); //value name : unk6 default value : unk6Len : 6
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight); //{weight}default value : 10202 Len : 2
                for (int i = 0; i < 3; i += 1)
                {
                    var item = itms[i];
                    WriteItemInfoToPacket(packet, item, false);

                }
                client.Send(packet);
            }
        }
        static readonly byte[] unk6 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF };


        #endregion
        #region advanced enchant

        [PacketHandler(RealmServerOpCode.AdvacedEnchantWeapon)]//6500
        public static void AdvacedEnchantWeaponRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 4;
            var itemSlot = packet.ReadInt16(); //default : 29Len : 2
            var item = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(itemSlot);
            if (item == null)
            {
                client.ActiveCharacter.SendInfoMsg("Item not found. Restart game please.");
                SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.Fail);
                return;
            }
            if (item.Template.Quality != Asda2ItemQuality.Green && item.Template.Quality != Asda2ItemQuality.Purple && item.Template.Quality != Asda2ItemQuality.Orange)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to advanced enchant item with wrong quality.", 50);
                SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.Fail);
                return;
            }
            packet.Position += 1; //nk1 default : 1Len : 1
            var slot1Res = packet.ReadInt32(); //default : 50Len : 4

            packet.Position += 7;
            var slot2Res = packet.ReadInt32(); //default : 50Len : 4
            packet.Position += 7;
            var slot3Res = packet.ReadInt32(); //default : 50Len : 4
            packet.Position += 7;

            var res1 = client.ActiveCharacter.Asda2Inventory.GetRegularItem((short)slot1Res);

            var res2 = client.ActiveCharacter.Asda2Inventory.GetRegularItem((short)slot2Res);

            var res3 = client.ActiveCharacter.Asda2Inventory.GetRegularItem((short)slot3Res);
            if (res1 == null || res2 == null || res3 == null)
            {
                client.ActiveCharacter.SendInfoMsg("Resource not found. Restart game please.");
                SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.Fail);
                return;
            }
            if (item.Template.Quality == Asda2ItemQuality.Green)
            {
                if (res1.ItemId != 33706 || res2.ItemId != 20681 || res3.ItemId != 33705)
                {
                    client.ActiveCharacter.YouAreFuckingCheater(
                        "Trying to advanced enchant with wrong resources.", 50);
                    SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.Fail);
                    return;
                }
                if (item.Template.Quality == Asda2ItemQuality.Orange)
                {
                    if (res1.ItemId != 33706 || res2.ItemId != 20681 || res3.ItemId != 33705)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater(
                            "Trying to advanced enchant with wrong resources.", 50);
                        SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.Fail);
                        return;
                    }
                }
                else if (item.Template.Quality == Asda2ItemQuality.Purple)
                {
                    if (res1.ItemId != 20681 || res2.ItemId != 20680 || res3.ItemId != 33705)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater(
                            "Trying to advanced enchant with wrong resources.", 50);
                        SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.Fail);
                        return;
                    }
                }
            }

            switch (item.Template.AuctionLevelCriterion)
            {
                case AuctionLevelCriterion.Zero:
                    if (res1.Amount < 1 || res2.Amount < 1 || res3.Amount < 3)
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enought resources. Restart game please.");
                        SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.NotEnoughtMaterials);
                        return;
                    }
                    if (!client.ActiveCharacter.SubtractMoney(50000))
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enought money. Restart game please.");
                        SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.NotEnoghtGold);
                        return;
                    }
                    res1.Amount -= 1;
                    res2.Amount -= 1;
                    res3.Amount -= 3;
                    break;
                case AuctionLevelCriterion.One:
                    if (res1.Amount < 1 || res2.Amount < 2 || res3.Amount < 6)
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enought resources. Restart game please.");
                        SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.NotEnoughtMaterials);
                        return;
                    }
                    if (!client.ActiveCharacter.SubtractMoney(100000))
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enought money. Restart game please.");
                        SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.NotEnoghtGold);
                        return;
                    }
                    res1.Amount -= 1;
                    res2.Amount -= 2;
                    res3.Amount -= 6;
                    break;
                case AuctionLevelCriterion.Two:
                    if (res1.Amount < 2 || res2.Amount < 3 || res3.Amount < 9)
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enought resources. Restart game please.");
                        SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.NotEnoughtMaterials);
                        return;
                    }
                    if (!client.ActiveCharacter.SubtractMoney(200000))
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enought money. Restart game please.");
                        SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.NotEnoghtGold);
                        return;
                    }
                    res1.Amount -= 2;
                    res2.Amount -= 3;
                    res3.Amount -= 9;
                    break;
                case AuctionLevelCriterion.Three:
                    if (res1.Amount < 2 || res2.Amount < 4 || res3.Amount < 12)
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enought resources. Restart game please.");
                        SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.NotEnoughtMaterials);
                        return;
                    }
                    if (!client.ActiveCharacter.SubtractMoney(400000))
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enought money. Restart game please.");
                        SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.NotEnoghtGold);
                        return;
                    }
                    res1.Amount -= 2;
                    res2.Amount -= 4;
                    res3.Amount -= 12;
                    break;
                case AuctionLevelCriterion.Four:
                    if (res1.Amount < 3 || res2.Amount < 5 || res3.Amount < 15)
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enought resources. Restart game please.");
                        SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.NotEnoughtMaterials);
                        return;
                    }
                    if (!client.ActiveCharacter.SubtractMoney(800000))
                    {
                        client.ActiveCharacter.SendInfoMsg("Not enought money. Restart game please.");
                        SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.NotEnoghtGold);
                        return;
                    }
                    res1.Amount -= 3;
                    res2.Amount -= 5;
                    res3.Amount -= 15;
                    break;
            }
            item.SetRandomAdvancedEnchant();
            item.Save();
            if (!res1.IsDeleted)
                res1.Save();
            if (!res2.IsDeleted)
                res2.Save();
            if (!res3.IsDeleted)
                res3.Save();
            Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                                                 .AddAttribute("source", 0, "advanced_enchant")
                                                 .AddItemAttributes(item, "main")
                                                 .AddItemAttributes(res1, "res1")
                                                 .AddItemAttributes(res2, "res2")
                                                 .AddItemAttributes(res3, "res3")
                                                 .Write();
            Asda2TitleChecker.OnAdvnacedEnchant(client.ActiveCharacter, item.ItemId, item.Template.Quality);
            SendAdvancedEnchantDoneResponse(client, AdvancedEnchantStatus.Ok, item, res1, res2, res3);
            client.ActiveCharacter.SendMoneyUpdate();

        }

        public static void SendAdvancedEnchantDoneResponse(IRealmClient client, AdvancedEnchantStatus status, Asda2Item enchantedItem = null, Asda2Item firstResource = null, Asda2Item secondResource = null, Asda2Item thirdResource = null)
        {
            var items = new Asda2Item[4];
            items[0] = enchantedItem;
            items[1] = firstResource;
            items[2] = secondResource;
            items[3] = thirdResource;
            using (var packet = new RealmPacketOut(RealmServerOpCode.AdvancedEnchantDone))//6501
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);//{invWeight}default value : 10856 Len : 2
                packet.WriteInt32(client.ActiveCharacter.Money);//{money}default value : 13092591 Len : 4
                for (int i = 0; i < 4; i += 1)
                {
                    var item = items[i];
                    WriteItemInfoToPacket(packet, item, false);

                }
                client.Send(packet, addEnd: true);
            }
        }
        #endregion


        #region item combine
        [PacketHandler(RealmServerOpCode.CombineItem)]//6098
        public static void CombineItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            var comtinationId = packet.ReadInt16();//default : 30Len : 2
            client.ActiveCharacter.Asda2Inventory.CombineItems(comtinationId);


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="resultItem"></param>
        /// <param name="usedItems">max 5 items</param>
        public static void SendItemCombinedResponse(IRealmClient client, Asda2Item resultItem, List<Asda2Item> usedItems)
        {
            var items = new Asda2Item[10];
            items[0] = resultItem;
            for (int i = 0; i < usedItems.Count; i++)
            {
                items[i + 5] = usedItems[i];
            }
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemCombined))//2270
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{sessId}default value : 58 Len : 2
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 361343 Len : 4
                packet.WriteInt16(client.ActiveCharacter.CharNum);//{charNum}default value : 11 Len : 2
                packet.WriteByte(1);//{status}default value : 1 Len : 1
                packet.WriteByte(1);//value name : unk9 default value : 1Len : 1
                for (int i = 0; i < 10; i += 1)
                {
                    var item = items[i];
                    WriteItemInfoToPacket(packet, item);
                }
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);//{weight}default value : 0 Len : 4
                client.Send(packet, addEnd: true);
            }
        }

        [PacketHandler(RealmServerOpCode.SummonBoss)]//6124
        public static void SummonBossRequest(IRealmClient client, RealmPacketIn packet)
        {
            var items = new List<Asda2Item>();
            for (int i = 0; i < 6; i += 1)
            {
                var itemId = packet.ReadInt32(); //default : 31969Len : 4
                var itemInv = packet.ReadByte(); //default : 2Len : 1
                var slot = packet.ReadInt16(); //default : 5Len : 2
                if (itemId == -1)
                    break;
                var item = client.ActiveCharacter.Asda2Inventory.GetRegularItem(slot);
                if (item == null)
                {
                    client.ActiveCharacter.SendInfoMsg("Wrong inventory info please restart game.");
                    return;
                }
                items.Add(item);
            }
            if (items.Count == 0)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to summon boss with wrong info.", 50);
                return;
            }
            var rec = Asda2ItemMgr.SummonRecords[items[0].ItemId];
            if (rec == null)
            {
                client.ActiveCharacter.SendInfoMsg(string.Format("Summon record {0} amount {1} not founed.",
                                                                 items[0].ItemId, items.Count));
                return;
            }
            if (items.Count < rec.Amount)
            {
                client.ActiveCharacter.SendInfoMsg("not enought stones. required " + rec.Amount);
                return;
            }
            for (int i = 1; i < items.Count; i++)
            {
                if (items[i].ItemId != items[0].ItemId)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to summon boss with wrong info.", 50);
                    return;
                }
            }
            foreach (var asda2Item in items)
            {
                asda2Item.Destroy();
            }
            var entry = NPCMgr.GetEntry(rec.MobId);
            if (entry == null)
            {
                client.ActiveCharacter.SendInfoMsg(string.Format("Summon record {0} has invalind npc Id {1}.",
                                                                 items[0].ItemId, rec.MobId));
                return;
            }
            var dest = new WorldLocation(rec.MapId, new Vector3(rec.X + (int)rec.MapId * 1000, rec.Y + (int)rec.MapId * 1000));
            NPC newNpc = entry.SpawnAt(dest);
            newNpc.Brain.EnterDefaultState();
            Asda2TitleChecker.OnBossSummon(client.ActiveCharacter, entry.Id);
            SendMonsterSummonedResponse(client, items);
        }

        public static void SendMonsterSummonedResponse(IRealmClient client, List<Asda2Item> items)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.MonsterSummoned))//6125
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 361343 Len : 4
                for (int i = 0; i < 6; i += 1)
                {
                    var item = items.Count <= i ? null : items[i];
                    packet.WriteInt32(item == null ? -1 : item.ItemId);//{item1}default value : 31969 Len : 4
                    packet.WriteByte((byte)(item == null ? 0 : item.InventoryType));//{itemInv}default value : 2 Len : 1
                    packet.WriteInt16(item == null ? -1 : item.Slot);//{slot}default value : 5 Len : 2
                }
                packet.WriteByte(1);
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                client.Send(packet, addEnd: true);
            }
        }


        #endregion

        #region donationInventory
        public static void SendSomeNewItemRecivedResponse(IRealmClient client, int itemId, byte slot)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SomeNewItemRecived))//6226
            {
                packet.WriteByte(slot);//value name : unk5 default value : 102Len : 1
                packet.WriteInt32(itemId);//{ItemId}default value : 37013 Len : 4
                client.Send(packet, addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.TakeItemFromMail)]//11006
        public static void TakeItemFromMailRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 4;
            var uniqId = packet.ReadInt32();//default : -1Len : 4
            var itemId = packet.ReadInt32();//default : 0Len : 4
            packet.Position += 1;//nk1 default : 1Len : 1
            var slot = packet.ReadByte();//default : 3Len : 1
            if (!client.ActiveCharacter.Asda2Inventory.DonationItems.ContainsKey(uniqId))
            {
                client.ActiveCharacter.SendInfoMsg("Can't found donated item.");
                return;
            }
            var itemsCount = 1;
            var item = client.ActiveCharacter.Asda2Inventory.DonationItems[uniqId];
            if (item.Recived)
            {
                client.ActiveCharacter.SendInfoMsg("Item already recived.");
                return;
            }
            if (client.ActiveCharacter.Asda2Inventory.FreeRegularSlotsCount < itemsCount || client.ActiveCharacter.Asda2Inventory.FreeShopSlotsCount < itemsCount)
            {
                client.ActiveCharacter.SendInfoMsg("Not enought inventory space.");
                return;
            }
            item.Recived = true;
            item.Save();
            client.ActiveCharacter.Asda2Inventory.DonationItems.Remove(item.Guid);
            Asda2Item addedItem = null;
            client.ActiveCharacter.Asda2Inventory.TryAdd(
                item.ItemId, item.Amount, true, ref addedItem);
            var resLog = Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                  .AddAttribute("source", 0, "donation_mail")
                  .AddAttribute("added_amount", item.Amount)
                  .AddItemAttributes(addedItem)
                  .AddAttribute("donation_id", item.Guid)
                  .AddAttribute("creator", 0, item.Creator)
                  .Write();

            if (addedItem != null)
                addedItem.IsSoulbound = true;
            //var itemquality = addedItem.Template.ItemId;
            Asda2TitleChecker.OnMailItems(client.ActiveCharacter, item.ItemId);
            SendItemFromMailTakedResponse(client, TakeItemMallItemsResult.Ok, new List<Asda2Item> { addedItem });


        }
        [PacketHandler(RealmServerOpCode.ShowMeItemMallItems)]//11004
        public static void ShowMeItemMallItemsRequest(IRealmClient client, RealmPacketIn packet)
        {
            var items = client.ActiveCharacter.Asda2Inventory.DonationItems.Values.Take(7).ToList();
            Asda2TitleChecker.OnMainOpenRequest(client.ActiveCharacter);
            SendItemMailListResponse(client, items);
        }
        public static void SendItemMailListResponse(IRealmClient client, List<Asda2DonationItem> items)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemMailList))//11005
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 180309 Len : 4
                for (int i = 0; i < 7; i += 1)
                {
                    var item = items.Count <= i ? null : items[i];
                    packet.WriteInt32(item == null ? -1 : item.Guid);//value name : unk4 default value : -1Len : 4
                    packet.WriteInt32(item == null ? -1 : item.ItemId);//{itemId}default value : 417 Len : 4
                    packet.WriteByte(1);//value name : unk1 default value : 1Len : 1
                    packet.WriteByte(i);//{slot}default value : 3 Len : 1
                }
                client.Send(packet, addEnd: true);
            }
        }
        public static void SendItemFromMailTakedResponse(IRealmClient client, TakeItemMallItemsResult status, List<Asda2Item> items)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemFromMailTaked))//11007
            {
                packet.WriteByte((byte)status);//{result}default value : 1 Len : 1
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 361343 Len : 4
                for (int i = 0; i < 7; i += 1)
                {
                    var item = items.Count <= i ? null : items[i];
                    WriteItemInfoToPacket(packet, item);
                }
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);//{weight}default value : 0 Len : 4
                client.Send(packet, addEnd: true);
            }
        }

        [PacketHandler(RealmServerOpCode.CloseMailBox)]//5466
        public static void CloseMailBoxRequest(IRealmClient client, RealmPacketIn packet)
        {
            SendMailBoxClosedResponse(client);
        }

        public static void SendMailBoxClosedResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.MailBoxClosed))//5467
            {
                packet.WriteByte(1);//{result}default value : 1 Len : 1
                packet.WriteInt32(0);//value name : unk4 default value : 0Len : 4
                client.Send(packet, addEnd: true);
            }
        }


        #endregion

        public static void SendGoldPickupedResponse(uint amount, Character chr)
        {
            if (chr == null || chr.Asda2Inventory == null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemPickuped)) //5013
            {
                packet.WriteByte((byte)Asda2PickUpItemStatus.Ok); //{status}default value : 1 Len : 1
                packet.WriteInt16(-1); //value name : unk5 default value : -1Len : 2
                packet.WriteInt32(20551); //{itemID}default value : 28516 Len : 4
                packet.WriteByte(2); //{bagNum}default value : 1 Len : 1
                packet.WriteInt16(0); //{cellNum}default value : 0 Len : 4
                packet.WriteInt16(0);
                packet.WriteInt32(amount); //{quantity}default value : 0 Len : 4
                packet.WriteByte(0); //{durability}default value : 100 Len : 1
                packet.WriteInt16(0); //{weight}default value : 677 Len : 2
                packet.WriteInt16(-1); //{soul1Id}default value : 7576 Len : 2
                packet.WriteInt16(-1); //{soul2Id}default value : -1 Len : 2
                packet.WriteInt16(-1); //{soul3Id}default value : -1 Len : 2
                packet.WriteInt16(-1); //{soul4Id}default value : -1 Len : 2
                packet.WriteByte(-1); //{enchant}default value : 0 Len : 1
                packet.WriteSkip(Stab31); //value name : stab31 default value : stab31Len : 3
                packet.WriteByte(0); //{sealCount}default value : 0 Len : 1
                packet.WriteInt16(-1);
                packet.WriteInt16(-1); //{stat1Value}default value : 9 Len : 2
                packet.WriteInt16(-1);
                packet.WriteInt16(-1); //{stat1Value}default value : 9 Len : 2
                packet.WriteInt16(-1); //{stat1Type}default value : 1 Len : 2
                packet.WriteInt16(-1); //{stat1Value}default value : 9 Len : 2
                packet.WriteInt16(-1); //{stat1Type}default value : 1 Len : 2
                packet.WriteInt16(-1); //{stat1Value}default value : 9 Len : 2
                packet.WriteInt16(-1); //{stat1Type}default value : 1 Len : 2
                packet.WriteInt16(-1); //{stat1Value}default value : 9 Len : 2
                packet.WriteByte(0); //value name : unk15 default value : 0Len : 1
                packet.WriteByte(0); //{equiped}default value : 0 Len : 1
                packet.WriteInt32(0); //value name : unk17 default value : 0Len : 4
                packet.WriteInt16(0); //value name : unk18 default value : 0Len : 2
                packet.WriteInt16(chr.Asda2Inventory.Weight); //{weight}default value : 5571 Len : 2
                chr.Client.Send(packet, addEnd: true);
            }
        }
    }
    [ActiveRecord(Access = PropertyAccess.Property)]
    public class Asda2DonationItem : ActiveRecordBase<Asda2DonationItem>
    {
        [Property]
        public int ItemId { get; set; }
        [Property]
        public int Amount { get; set; }
        [Property]
        public uint RecieverId { get; set; }
        [Property]
        public string Creator { get; set; }
        [Property]
        public bool IsSoulBound { get; set; }
        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

        private static readonly NHIdGenerator s_idGenerator = new NHIdGenerator(typeof(Asda2DonationItem), "Guid");

        /// <summary>
        /// Returns the next unique Id for a new Item
        /// </summary>
        public static long NextId()
        {
            return s_idGenerator.Next();
        }


        /// <summary>
        /// Create an exisiting MailMessage
        /// </summary>
        public Asda2DonationItem()
        {
        }

        /// <summary>
        /// Create a new Donation item
        /// </summary>
        public Asda2DonationItem(uint recieverId, int itemId, int amount, string name, bool isSoulBound)
        {
            RecieverId = recieverId;
            ItemId = itemId;
            Amount = amount;
            Created = DateTime.Now;
            Guid = (int)NextId();
            Creator = name;
            IsSoulBound = isSoulBound;
        }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public int Guid
        {
            get;
            set;
        }
        [Property]
        public DateTime Created { get; set; }

        [Property]
        public bool Recived { get; set; }

        public static Asda2DonationItem[] LoadAll(Character chr)
        {
            var msgs = FindAllByProperty("RecieverId", chr.EntryId).Where(d => false == d.Recived).ToArray();
            foreach (var asda2MailMessage in msgs)
            {
                asda2MailMessage.Init();
            }
            return msgs;
        }

        private void Init()
        {
        }
    }
    enum TakeItemMallItemsResult
    {
        Fail = 0,
        Ok = 1
    }
    enum AdvancedEnchantStatus
    {
        Fail = 0,
        Ok = 1,
        NotEnoghtGold = 2,
        NotEnoughtMaterials = 5,

    }
    enum AvatarDisassembleStatus
    {
        Fail = 0,
        Ok = 1,
        NotAvatarItem = 2,
        InventoryIsFull = 3,

    }
    enum AvatarSyntesStatus
    {
        Ok = 1,
        Fail = 2,
        AbnormalInfo = 3,
        NoMoreSyntesOnThisItem = 4,


    }
    internal enum PushItemToWhStatus
    {
        CantFindItem = 0,
        Ok = 1,
        NotEnoughtSlots = 2,
        OnlyForSoulmate = 3,
        ItemNotFounded = 4,
        NotEnoughtSlotsInWh = 5,
        NotEnoughtGold = 6,
        NoWeightLimit = 7,
        AlreadyUsingWh
    }


    public enum BuyFromWarShopStatus
    {
        Fail = 0,
        Ok = 1,
        InventoryIsFull = 2,
        InvalidWeight = 3,
        NonEnoghtGold = 4,
        NotEnoghtExchangeItems = 5,
        NonEnoghtHonorRanks = 6,
        CantFoundItem = 7,
        AvalibleOnlyToWiningFaction = 8,
        UnableToPurshace = 9

    }
    public enum SowelRemovedStatus
    {
        Fail = 0,
        Ok = 1,

    }
    public enum Asda2BuyItemStatus
    {
        Fail = 0,
        Ok = 1,
        BadItemId = 5,
        NotEnoughSpace = 2,
        NotEnoughGold = 4
    }
    internal enum Asda2PickUpItemStatus
    {
        Fail = 0,
        Ok = 1,
        NoSpace
    }
    public enum UpgradeItemStatus
    {
        Ok = 1,
        Fail = 2
    }
    public enum ExchangeOptionResult
    {
        ParamertError = 0,
        Ok = 1,
        ScrollInvalid = 2,
        ItemInvalid = 3
    }
    public enum SowelingStatus
    {
        Fail = 0,
        Ok = 1,
        CantAtackSowelRightNow = 2,
        EquipmentError = 3,
        SowelError = 4,
        ProtectScrollError = 5,
        LowLevel = 6,
        ThisItemCantBeSoceted = 7,
        UnableToSocketAtThisSlot = 8,
        MaxSocketSlotError = 9,
        EquipmentTypeError = 10,
        IncorrectWeapon = 11,
        LowCc = 12,
    }

    public class Asda2WhItemStub
    {
        private short _slot = -1;
        public Asda2InventoryType Invtentory { get; set; }

        public short Slot
        {
            get { return _slot; }
            set { _slot = value; }
        }

        public int Amount { get; set; }
        public short Weight { get; set; }
    }
}
