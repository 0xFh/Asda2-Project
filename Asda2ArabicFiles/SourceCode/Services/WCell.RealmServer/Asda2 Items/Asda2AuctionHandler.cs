using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Asda2_Items
{
    internal class Asda2AuctionHandler
    {
        [PacketHandler(RealmServerOpCode.RegisterItemToAuk)] //9901
        public static void RegisterItemToAukRequest(IRealmClient client, RealmPacketIn packet)
        {

            var chr = client.ActiveCharacter;
            var inv = chr.Asda2Inventory;
            packet.Position += 15; //tab35 default : stab35Len : 1
            var totalPrice = 0;
            var items = new List<Asda2ItemTradeRef>();
            for (int i = 0; i < 5; i += 1)
            {
                if (packet.RemainingLength < 50)
                {
                    break;
                }
                packet.Position += 4;
                var invNum = packet.ReadByte(); //default : 1Len : 1
                var cell = packet.ReadInt16(); //default : 1Len : 2
                packet.Position += 12; //tab57 default : stab57Len : 12
                var amount = packet.ReadInt32(); //default : 56Len : 4
                packet.Position += 34; //tab73 default : stab73Len : 34
                var price = packet.ReadInt32(); //default : 8400Len : 4
                packet.Position += 41;
                if (price < 0 || price > 100000000)
                {
                    client.ActiveCharacter.YouAreFuckingCheater(
                        "Tried to use wrong price while registering auk items : " + cell);
                    SendRegisterItemToAukCancelWindowResponse(client);
                    return;
                }
                if (cell < 0 || cell > 70)
                {
                    client.ActiveCharacter.YouAreFuckingCheater(
                        "Tried to use wrong cell while registering auk items : " + cell);
                    SendRegisterItemToAukCancelWindowResponse(client);
                    return;
                }
                Asda2Item item = null;
                switch ((Asda2InventoryType)invNum)
                {
                    case Asda2InventoryType.Regular:
                        item = inv.RegularItems[cell];
                        break;
                    case Asda2InventoryType.Shop:
                        item = inv.ShopItems[cell];
                        break;
                    default:
                        client.ActiveCharacter.YouAreFuckingCheater(
                            "Tried to use wrong inventory while registering auk items : " + invNum);
                        SendRegisterItemToAukCancelWindowResponse(client);
                        return;
                }
                items.Add(new Asda2ItemTradeRef { Item = item, Amount = amount, Price = price });
                totalPrice += price;
            }
                if (client.ActiveCharacter.Money <= totalPrice * CharacterFormulas.AuctionPushComission)
                {
                    client.ActiveCharacter.SendAuctionMsg("نقود وذهب غير كافي لتسجيل الأداة.");
                    SendRegisterItemToAukCancelWindowResponse(client);
                    return;
                }
                SendRegisterItemToAukCancelWindowResponse(client, items);
                foreach (var itemRef in items)
                {
                    if (itemRef.Item == null)
                    {
                        chr.SendAuctionMsg("فشل تسجيل الأداة بسبب عدم العثور عليها."); //رسالة فشل تسجيل الاداة في المزاد
                        SendRegisterItemToAukCancelWindowResponse(client);
                        return;
                    }
                    if (itemRef.Amount < 0 || itemRef.Amount > itemRef.Item.Amount)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater(
                            "Tried to use wrong item amount while registering auk items : " + itemRef.Amount);
                        SendRegisterItemToAukCancelWindowResponse(client);
                        return;
                    }
                    if (itemRef.Item.IsSoulbound)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater(
                            "Tried to use soulbounded item while registering auk items : " + itemRef.Amount);
                        SendRegisterItemToAukCancelWindowResponse(client);
                        return;
                    }
                    inv.AuctionItem(itemRef);
                }
                chr.SendAuctionMsg("تم وضع وتسجيل الأداة بنجاح في المزاد."); //رسالة نجاح وضع الاداة في المزاد
                chr.SendMoneyUpdate();
        }

        public static void SendRegisterItemToAukCancelWindowResponse(IRealmClient client, List<Asda2ItemTradeRef> items = null)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.RegisterItemToAukCancelWindow))//9902
            {
                if (items != null)
                    foreach (var item in items)
                    {
                        packet.WriteByte(0);//{status}default value : 0 Len : 1
                        packet.WriteInt32(item.Item.ItemId);//{itemId}default value : 31855 Len : 4
                        packet.WriteByte((byte)item.Item.InventoryType);//{invNum}default value : 2 Len : 1
                        packet.WriteInt16(item.Item.Slot);//{cell}default value : 5 Len : 2
                        packet.WriteSkip(stab15);//value name : stab15 default value : stab15Len : 12
                        packet.WriteInt32(item.Amount);//{registeredAmount}default value : 250 Len : 4
                        packet.WriteInt32(item.Item.Amount);//{beforeAmount}default value : 250 Len : 4
                        packet.WriteInt16(item.Item.Weight);//{weight}default value : 0 Len : 2
                        packet.WriteSkip(stab37);//value name : stab37 default value : stab37Len : 21
                        packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);//{invWeight}default value : 315 Len : 4
                        packet.WriteInt32(client.ActiveCharacter.Money);//{money}default value : 8503216 Len : 4
                        packet.WriteInt64(-1);//value name : unk8 default value : -1Len : 8
                    }
                client.Send(packet);
            }
        }
        static readonly byte[] stab15 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        static readonly byte[] stab37 = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 };


        [PacketHandler(RealmServerOpCode.ShowMyAukItems)] //9907
        public static void ShowMyAukItemsRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 15; //tab35 default : stab35Len : 1
            if (packet.ReadByte() == 1)
            {
                //regualr items
                SendMyAukItemsInfoResponse(client,
                                           Asda2AuctionMgr.GetCharacterRegularItems(
                                               (uint)client.ActiveCharacter.Record.Guid));
            }
            else
            {
                //equip items
                SendMyAukItemsInfoResponse(client,
                                           Asda2AuctionMgr.GetCharacterShopItems(
                                               (uint)client.ActiveCharacter.Record.Guid));
            }
        }

        public static void SendMyAukItemsInfoResponse(IRealmClient client, List<Asda2ItemRecord> items)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.MyAukItemsInfo)) //9908
            {
                var i = 0;
                foreach (var item in items)
                {
                    if (i == 8)
                        break;
                    packet.WriteInt32(item.AuctionId); //{aukId}default value : 1179 Len : 4
                    packet.WriteByte(0); //value name : unk6 default value : 0Len : 1
                    packet.WriteInt32(item.ItemId); //{itemId}default value : 20579 Len : 4
                    packet.WriteInt32(item.Amount); //{amount}default value : 93 Len : 4
                    packet.WriteByte(item.Durability); //{durability}default value : 0 Len : 1
                    packet.WriteByte(item.Enchant); //{enchant}default value : 0 Len : 1
                    packet.WriteInt32(0); //value name : unk11 default value : 0Len : 4
                    packet.WriteInt32(526300); //value name : unk12 default value : 526300Len : 4
                    packet.WriteInt16(13); //value name : unk2 default value : 13Len : 2
                    packet.WriteInt16(item.Parametr1Type);
                    packet.WriteInt16(item.Parametr1Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Parametr2Type);
                    packet.WriteInt16(item.Parametr2Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Parametr3Type); //{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item.Parametr3Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Parametr4Type); //{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item.Parametr4Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Parametr5Type); //{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item.Parametr5Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteByte(0); //value name : unk23 default value : 0Len : 1

                    packet.WriteInt32((int)(item.AuctionEndTime - DateTime.Now).TotalMilliseconds);
                    //{timeToEnd}default value : 604735000 Len : 4
                    packet.WriteInt32(item.AuctionPrice); //{price}default value : 13950 Len : 4
                    packet.WriteInt16(item.Soul1Id); //{soul1Id}default value : -1 Len : 2
                    packet.WriteInt16(item.Soul2Id); //{soul2id}default value : -1 Len : 2
                    packet.WriteInt16(item.Soul3Id); //{soul3Id}default value : -1 Len : 2
                    packet.WriteInt16(item.Soul4Id); //{soul4Id}default value : -1 Len : 2
                    i++;
                }


                client.Send(packet);
            }
        }


        [PacketHandler(RealmServerOpCode.LoadDataFromAukPage)] //9903
        public static void LoadDataFromAukPageRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 15; //tab35 default : stab35Len : 5
            var category = (AucionCategoties)packet.ReadInt16(); //default : 267Len : 2
            packet.Position += 1; //tab52 default : stab52Len : 1
            var option1 = packet.ReadInt16(); //default : 100Len : 2
            var option2 = packet.ReadByte(); //default : 0Len : 1
            var option3 = packet.ReadByte(); //default : 2Len : 1
            var pageNum = packet.ReadByte(); //default : 0Len : 1
            try
            {
                AuctionLevelCriterion requiredLevelCriterion;
                Asda2ItemAuctionCategory reqCategory = CalcCategory(category, option1, option2, option3,
                                                                    out requiredLevelCriterion);
                var col = Asda2AuctionMgr.CategorizedItemsById[reqCategory][requiredLevelCriterion];
                SendItemsOnAukInfoResponse(client, col.Skip(pageNum * 7).Take(7), (byte)((col.Count - 1) / 7), pageNum);
            }
            catch
            {
                client.ActiveCharacter.YouAreFuckingCheater("Sends wrong auction show items request.");
            }
        }

        private static Asda2ItemAuctionCategory CalcCategory(AucionCategoties category, short option1, byte option2,
                                                             byte option3,
                                                             out AuctionLevelCriterion requiredLevelCriterion)
        {
            requiredLevelCriterion = AuctionLevelCriterion.All;
            switch (category)
            {
                case AucionCategoties.Rings:
                    requiredLevelCriterion = (AuctionLevelCriterion)option1;
                    return Asda2ItemAuctionCategory.Ring;
                case AucionCategoties.Nackless:
                    requiredLevelCriterion = (AuctionLevelCriterion)option1;
                    return Asda2ItemAuctionCategory.Nackless;
                case AucionCategoties.SowelRune:
                    requiredLevelCriterion = (AuctionLevelCriterion)option1;
                    if (option3 == 2)
                    {
                        switch ((Asda2RuneSowelTypes)option2)
                        {
                            case Asda2RuneSowelTypes.Stamina:
                                return Asda2ItemAuctionCategory.RuneStamina;
                            case Asda2RuneSowelTypes.Dexterity:
                                return Asda2ItemAuctionCategory.RuneDexterity;
                            case Asda2RuneSowelTypes.Intellect:
                                return Asda2ItemAuctionCategory.RuneIntellect;
                            case Asda2RuneSowelTypes.Luck:
                                return Asda2ItemAuctionCategory.RuneLuck;
                            case Asda2RuneSowelTypes.Misc:
                                return Asda2ItemAuctionCategory.RuneMisc;
                            case Asda2RuneSowelTypes.Spirit:
                                return Asda2ItemAuctionCategory.RuneSpirit;
                            case Asda2RuneSowelTypes.Strength:
                                return Asda2ItemAuctionCategory.RuneStrength;
                            default:
                                return Asda2ItemAuctionCategory.RuneMisc;
                        }
                    }
                    switch ((Asda2MainSowlelTypes)option2)
                    {
                        case Asda2MainSowlelTypes.Staff:
                            return Asda2ItemAuctionCategory.SowelStaff;
                        case Asda2MainSowlelTypes.AArmor:
                            return Asda2ItemAuctionCategory.SowelArmor;
                        case Asda2MainSowlelTypes.Bow:
                            return Asda2ItemAuctionCategory.SowelBow;
                        case Asda2MainSowlelTypes.Crossbow:
                            return Asda2ItemAuctionCategory.SowelCrossBow;
                        case Asda2MainSowlelTypes.Dexterity:
                            return Asda2ItemAuctionCategory.SowelDexterity;
                        case Asda2MainSowlelTypes.Intellect:
                            return Asda2ItemAuctionCategory.SowelIntellect;
                        case Asda2MainSowlelTypes.Luck:
                            return Asda2ItemAuctionCategory.SowelLuck;
                        case Asda2MainSowlelTypes.MArmor:
                            return Asda2ItemAuctionCategory.SowelArmor;
                        case Asda2MainSowlelTypes.Misc:
                            return Asda2ItemAuctionCategory.SowelMisc;
                        case Asda2MainSowlelTypes.OHS:
                            return Asda2ItemAuctionCategory.SowelOHS;
                        case Asda2MainSowlelTypes.Spear:
                            return Asda2ItemAuctionCategory.SowelSpear;
                        case Asda2MainSowlelTypes.Spirit:
                            return Asda2ItemAuctionCategory.SowelSpirit;
                        case Asda2MainSowlelTypes.Stamina:
                            return Asda2ItemAuctionCategory.SowelStamina;
                        case Asda2MainSowlelTypes.Strength:
                            return Asda2ItemAuctionCategory.SowelStrengs;
                        case Asda2MainSowlelTypes.THS:
                            return Asda2ItemAuctionCategory.SowelThs;
                        case Asda2MainSowlelTypes.WArmor:
                            return Asda2ItemAuctionCategory.SowelArmor;
                        default:
                            return Asda2ItemAuctionCategory.SowelMisc;
                    }
                case AucionCategoties.Upgrade:
                    requiredLevelCriterion = (AuctionLevelCriterion)option1;
                    switch ((Asda2UpgradeTypes)option2)
                    {
                        case Asda2UpgradeTypes.Weapon:
                            return Asda2ItemAuctionCategory.UpgradeWeapon;

                        default:
                            return Asda2ItemAuctionCategory.UpgradeArmor;
                    }
                case AucionCategoties.Potion:
                    switch ((Asda2PotionTypes)option2)
                    {
                        case Asda2PotionTypes.Hp:
                            return Asda2ItemAuctionCategory.PotionHp;
                        case Asda2PotionTypes.Mp:
                            return Asda2ItemAuctionCategory.PotionMp;
                        case Asda2PotionTypes.Fish:
                            return Asda2ItemAuctionCategory.PotionFish;
                        default:
                            return Asda2ItemAuctionCategory.PotionHp;
                    }
                case AucionCategoties.Crafting:
                    switch ((Asda2CraftItemTypes)option2)
                    {
                        case Asda2CraftItemTypes.Recipe:
                            requiredLevelCriterion = (AuctionLevelCriterion)option1;
                            return Asda2ItemAuctionCategory.Recipe;
                        case Asda2CraftItemTypes.Materials:
                            return Asda2ItemAuctionCategory.Materials;
                        default:
                            return Asda2ItemAuctionCategory.Recipe;
                    }
                case AucionCategoties.Other:
                    switch ((Asda2OtherItemTypes)option2)
                    {
                        case Asda2OtherItemTypes.Booster:
                            return Asda2ItemAuctionCategory.Boosters;
                        default:
                            return Asda2ItemAuctionCategory.Misc;
                    }
                case AucionCategoties.Premium:
                    return Asda2ItemAuctionCategory.Premium;
                case AucionCategoties.Shield:
                    return Asda2ItemAuctionCategory.Shield;
                case AucionCategoties.Weapon:
                    switch ((Asda2WeaponCategory)option2)
                    {
                        case Asda2WeaponCategory.Staff:
                            return Asda2ItemAuctionCategory.WeaponStaff;
                        case Asda2WeaponCategory.Bow:
                            return Asda2ItemAuctionCategory.WeaponBow;
                        case Asda2WeaponCategory.Crossbow:
                            return Asda2ItemAuctionCategory.WeaponCrossbow;
                        case Asda2WeaponCategory.OHS:
                            return Asda2ItemAuctionCategory.WeaponOhs;
                        case Asda2WeaponCategory.Spear:
                            return Asda2ItemAuctionCategory.WeaponSpear;
                        case Asda2WeaponCategory.THS:
                            return Asda2ItemAuctionCategory.WeaponThs;
                        default:
                            return Asda2ItemAuctionCategory.WeaponCrossbow;
                    }
                case AucionCategoties.Warrior:
                    switch ((Asda2ArmorCategory)option2)
                    {
                        case Asda2ArmorCategory.Armor:
                            return Asda2ItemAuctionCategory.WarriorArmor;
                        case Asda2ArmorCategory.Boots:
                            return Asda2ItemAuctionCategory.WarriorBoots;
                        case Asda2ArmorCategory.Gloves:
                            return Asda2ItemAuctionCategory.WarriorGloves;
                        case Asda2ArmorCategory.Helmet:
                            return Asda2ItemAuctionCategory.WarriorHelm;
                        case Asda2ArmorCategory.Pants:
                            return Asda2ItemAuctionCategory.WarriorPants;
                        default:
                            return Asda2ItemAuctionCategory.WarriorPants;
                    }
                case AucionCategoties.Archer:
                    switch ((Asda2ArmorCategory)option2)
                    {
                        case Asda2ArmorCategory.Armor:
                            return Asda2ItemAuctionCategory.ArcherArmor;
                        case Asda2ArmorCategory.Boots:
                            return Asda2ItemAuctionCategory.ArcherBoots;
                        case Asda2ArmorCategory.Gloves:
                            return Asda2ItemAuctionCategory.ArcherGloves;
                        case Asda2ArmorCategory.Helmet:
                            return Asda2ItemAuctionCategory.ArcherHelm;
                        case Asda2ArmorCategory.Pants:
                            return Asda2ItemAuctionCategory.ArcherPants;
                        default:
                            return Asda2ItemAuctionCategory.ArcherPants;
                    }
                case AucionCategoties.Mage:
                    switch ((Asda2ArmorCategory)option2)
                    {
                        case Asda2ArmorCategory.Armor:
                            return Asda2ItemAuctionCategory.MageArmor;
                        case Asda2ArmorCategory.Boots:
                            return Asda2ItemAuctionCategory.MageBoots;
                        case Asda2ArmorCategory.Gloves:
                            return Asda2ItemAuctionCategory.MageGloves;
                        case Asda2ArmorCategory.Helmet:
                            return Asda2ItemAuctionCategory.MageHelm;
                        case Asda2ArmorCategory.Pants:
                            return Asda2ItemAuctionCategory.MagePants;
                        default:
                            return Asda2ItemAuctionCategory.MagePants;
                    }
                default:
                    return Asda2ItemAuctionCategory.Misc;
            }
        }

        public static void SendItemsOnAukInfoResponse(IRealmClient client, IEnumerable<Asda2ItemRecord> items,
                                                      byte pagesCount, byte curPage)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemsOnAukInfo)) //9904
            {
                foreach (var item in items)
                {
                    packet.WriteInt32(item.AuctionId); //{aukId}default value : 945 Len : 4
                    packet.WriteByte(0); //value name : unk6 default value : 0Len : 1
                    packet.WriteInt32(item.ItemId); //{itemId}default value : 23802 Len : 4
                    packet.WriteInt32(item.Amount); //{amount}default value : 0 Len : 4
                    packet.WriteByte(item.Durability); //{durability}default value : 90 Len : 1
                    packet.WriteByte(item.Enchant); //{enchant}default value : 10 Len : 1
                    packet.WriteInt32(16777216); //value name : unk11 default value : 16777216Len : 4
                    packet.WriteInt32(0); //value name : unk12 default value : 0Len : 4
                    packet.WriteInt16(0); //value name : unk13 default value : 0Len : 2
                    packet.WriteInt16(item.Parametr1Type);
                    packet.WriteInt16(item.Parametr1Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Parametr2Type);
                    packet.WriteInt16(item.Parametr2Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Parametr3Type); //{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item.Parametr3Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Parametr4Type); //{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item.Parametr4Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Parametr5Type); //{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item.Parametr5Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteByte(0); //value name : unk24 default value : unk24Len : 2
                    packet.WriteByte(curPage);
                    packet.WriteByte(pagesCount); //{pagesCount}default value : 0 Len : 1
                    packet.WriteInt32(item.AuctionPrice); //{money}default value : 850000 Len : 4
                    packet.WriteFixedAsciiString(item.OwnerName, 20); //{name}default value :  Len : 20
                    packet.WriteInt16(item.Soul1Id); //{soul1Id}default value : -1 Len : 2
                    packet.WriteInt16(item.Soul2Id); //{soul2id}default value : -1 Len : 2
                    packet.WriteInt16(item.Soul3Id); //{soul3Id}default value : -1 Len : 2
                    packet.WriteInt16(item.Soul4Id); //{soul4Id}default value : -1 Len : 2
                }

                client.Send(packet);
            }
        }

        private static readonly byte[] unk24 = new byte[] { 0x00, 0x00 };

        [PacketHandler(RealmServerOpCode.BuyFromAuk)] //9905
        public static void BuyFromAukRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 11; //tab35 default : stab35Len : 11
            var aucIds = new List<int>();
            for (int i = 0; i < 7; i += 1)
            {
                packet.Position += 4;
                if (packet.RemainingLength <= 0)
                    break;
                var aukId = packet.ReadInt32(); //default : 924Len : 4
                aucIds.Add(aukId);
                packet.Position += 41; //default : stub8Len : 41
            }
            RealmServer.IOQueue.AddMessage(() => Asda2AuctionMgr.TryBuy(aucIds, client.ActiveCharacter));
        }

        public static void SendItemsBuyedFromAukResponse(IRealmClient client, List<Asda2ItemTradeRef> items)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemsBuyedFromAuk)) //9906
            {
                var i = 0;
                foreach (var itemRef in items)
                {
                    var item = itemRef.Item;
                    if (i >= 7)
                        break;
                    packet.WriteInt32(itemRef.Price); //{aukId}default value : 1179 Len : 4
                    packet.WriteSkip(stub4); //{stub4}default value : stub4 Len : 3
                    packet.WriteInt32(item.ItemId); //{itemId%}default value : 0 Len : 4
                    packet.WriteInt32(itemRef.Amount); //{quantity}default value : 0 Len : 4
                    packet.WriteByte((byte)item.InventoryType); //{invNum}default value : 0 Len : 1
                    packet.WriteInt16(item.Slot); //{slot%}default value : -1 Len : 2
                    packet.WriteInt16(item.Weight); //{weight%}default value : 0 Len : 2
                    packet.WriteByte(item.Durability); //{durability%}default value : 0 Len : 1
                    packet.WriteInt32(item.Enchant); //{enchant}default value : 0 Len : 4
                    packet.WriteByte(0); //value name : unk1 default value : 0Len : 1
                    packet.WriteInt32(0); //value name : unk4 default value : 0Len : 4
                    packet.WriteInt16(0); //value name : unk2 default value : 0Len : 2
                    packet.WriteInt16(item.Record.Parametr1Type);
                    packet.WriteInt16(item.Parametr1Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Record.Parametr2Type);
                    packet.WriteInt16(item.Parametr2Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Record.Parametr3Type); //{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item.Parametr3Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Record.Parametr4Type); //{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item.Parametr4Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Record.Parametr5Type); //{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item.Parametr5Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteByte(0); //value name : unk1 default value : 0Len : 1
                    packet.WriteInt32(item.AuctionPrice); //{price}default value : 0 Len : 4
                    packet.WriteFixedAsciiString(client.ActiveCharacter.Name, 20); //{name}default value :  Len : 20
                    packet.WriteInt16(item.Soul1Id); //{soul1Id}default value : -1 Len : 2
                    packet.WriteInt16(item.Soul2Id); //{soul2id}default value : -1 Len : 2
                    packet.WriteInt16(item.Soul3Id); //{soul3Id}default value : -1 Len : 2
                    packet.WriteInt16(item.Soul4Id); //{soul4Id}default value : -1 Len : 2
                    i++;
                }

                client.Send(packet);
            }
        }


        private static readonly byte[] stub4 = new byte[] { 0x00, 0x00, 0x00 };

        [PacketHandler(RealmServerOpCode.RemoveFromAuk)] //9909
        public static void RemoveFromAukRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 4;
            var items = new List<int>();
            for (int i = 0; i < 8; i += 1)
            {
                packet.Position += 19; //nk7 default : unk7Len : 19
                if (packet.RemainingLength <= 4)
                    break;
                var aukId = packet.ReadInt32(); //default : 1179Len : 4
                packet.Position += 26;
                items.Add(aukId);
            }
            RealmServer.IOQueue.AddMessage(() => 
                Asda2AuctionMgr.TryRemoveItems(client.ActiveCharacter, items));
        }

        private static readonly byte[] unk7 = new byte[]
            {
                0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0x68, 0x81, 0x05, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x00, 0x68, 0x81, 0x05,
                0x00
            };

        private static readonly byte[] stub23 = new byte[]
            {
                0x63, 0x50, 0x00, 0x00, 0x5D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
            };

        public static void SendItemFromAukRemovedResponse(IRealmClient client, List<Asda2ItemTradeRef> asda2Items)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemFromAukRemoved)) //9910
            {
                var i = 0;
                foreach (var itemRef in asda2Items)
                {
                    var item = itemRef.Item;
                    if (i >= 8)
                        break;
                    packet.WriteInt32(itemRef.Price); //{aukId}default value : 1179 Len : 4
                    packet.WriteSkip(stub4); //{stub4}default value : stub4 Len : 3
                    packet.WriteInt32(item.ItemId); //{itemId%}default value : 0 Len : 4
                    packet.WriteInt32(itemRef.Amount); //{quantity}default value : 0 Len : 4
                    packet.WriteByte((byte)item.InventoryType); //{invNum}default value : 0 Len : 1
                    packet.WriteInt16(item.Slot); //{slot%}default value : -1 Len : 2
                    packet.WriteInt16(item.Weight); //{weight%}default value : 0 Len : 2
                    packet.WriteByte(item.Durability); //{durability%}default value : 0 Len : 1
                    packet.WriteInt32(item.Enchant); //{enchant}default value : 0 Len : 4
                    packet.WriteByte(0); //value name : unk1 default value : 0Len : 1
                    packet.WriteInt32(0); //value name : unk4 default value : 0Len : 4
                    packet.WriteInt16(0); //value name : unk2 default value : 0Len : 2
                    packet.WriteInt16(item.Record.Parametr1Type);
                    packet.WriteInt16(item.Parametr1Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Record.Parametr2Type);
                    packet.WriteInt16(item.Parametr2Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Record.Parametr3Type); //{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item.Parametr3Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Record.Parametr4Type); //{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item.Parametr4Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item.Record.Parametr5Type); //{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item.Parametr5Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteByte(0); //value name : unk1 default value : 0Len : 1
                    packet.WriteInt16(item.Soul1Id); //{soul1Id}default value : -1 Len : 2
                    packet.WriteInt16(item.Soul2Id); //{soul2id}default value : -1 Len : 2
                    packet.WriteInt16(item.Soul3Id); //{soul3Id}default value : -1 Len : 2
                    packet.WriteInt16(item.Soul4Id); //{soul4Id}default value : -1 Len : 2
                    i++;
                }
                client.Send(packet);
            }
        }
    }

    public enum AucionCategoties
    {
        //1-10 lvls per 10
        Rings = 5,
        //1-10 lvls per 10
        Nackless = 7,
        //1-10 lvls per 10
        SowelRune = 267,
        //D C B A S = 0 1 2 3 4 
        Upgrade = 523,
        // always 0
        Potion = 779,
        //reviepe level 1 - 10
        Crafting = 1035,
        //
        Other = 1291,
        //D C B A S = 0 1 2 3 4
        Shield = 8,
        Weapon = 9,
        Warrior = 0,
        Archer = 1,
        Mage = 2,
        Premium = 12
    }
    public enum Asda2ArmorCategory
    {
        Helmet = 0,
        Armor,
        Pants,
        Boots,
        Gloves
    }
    public enum Asda2WeaponCategory
    {
        OHS = 0,
        Spear,
        THS,
        Staff,
        Crossbow,
        Bow
    }
    public enum Asda2OtherItemTypes
    {
        Booster = 0,
        Misc = 2
    }
    public enum Asda2CraftItemTypes
    {
        Recipe = 0,
        Materials = 1
    }
    public enum Asda2PotionTypes
    {
        Hp = 0,
        Mp = 1,
        Fish = 2,
    }
    public enum Asda2UpgradeTypes
    {
        Weapon = 0,
        Armor = 1
    }
    public enum Asda2MainSowlelTypes
    {
        OHS = 0,
        Spear,
        THS,
        Bow,
        Crossbow,
        Staff,
        WArmor,
        AArmor,
        MArmor,
        Strength,
        Dexterity,
        Stamina,
        Spirit,
        Intellect,
        Luck,
        Misc
    }

    public enum Asda2RuneSowelTypes
    {
        Strength = 0,
        Dexterity,
        Stamina,
        Spirit,
        Intellect,
        Luck,
        Misc
    }
}
