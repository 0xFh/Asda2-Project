using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Asda2_Items
{
    internal class Asda2AuctionHandler
    {
        private static readonly byte[] stab15 = new byte[12];

        private static readonly byte[] stab37 = new byte[21]
        {
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
            (byte) 0
        };

        private static readonly byte[] unk24 = new byte[2];
        private static readonly byte[] stub4 = new byte[3];

        private static readonly byte[] unk7 = new byte[19]
        {
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 104,
            (byte) 129,
            (byte) 5,
            (byte) 0,
            (byte) 0,
            (byte) 100,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 104,
            (byte) 129,
            (byte) 5,
            (byte) 0
        };

        private static readonly byte[] stub23 = new byte[26]
        {
            (byte) 99,
            (byte) 80,
            (byte) 0,
            (byte) 0,
            (byte) 93,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
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
            byte.MaxValue
        };

        [PacketHandler(RealmServerOpCode.RegisterItemToAuk)]
        public static void RegisterItemToAukRequest(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            Asda2PlayerInventory asda2Inventory = activeCharacter.Asda2Inventory;
            packet.Position += 15;
            int num1 = 0;
            List<Asda2ItemTradeRef> items = new List<Asda2ItemTradeRef>();
            for (int index = 0; index < 5 && packet.RemainingLength >= 50; ++index)
            {
                packet.Position += 4;
                byte num2 = packet.ReadByte();
                short num3 = packet.ReadInt16();
                packet.Position += 12;
                int num4 = packet.ReadInt32();
                packet.Position += 34;
                int num5 = packet.ReadInt32();
                packet.Position += 41;
                if (num5 < 0 || num5 > 100000000)
                {
                    client.ActiveCharacter.YouAreFuckingCheater(
                        "Tried to use wrong price while registering auk items : " + (object) num3, 1);
                    Asda2AuctionHandler.SendRegisterItemToAukCancelWindowResponse(client,
                        (List<Asda2ItemTradeRef>) null);
                    return;
                }

                if (num3 < (short) 0 || num3 > (short) 70)
                {
                    client.ActiveCharacter.YouAreFuckingCheater(
                        "Tried to use wrong cell while registering auk items : " + (object) num3, 1);
                    Asda2AuctionHandler.SendRegisterItemToAukCancelWindowResponse(client,
                        (List<Asda2ItemTradeRef>) null);
                    return;
                }

                Asda2Item asda2Item;
                switch (num2)
                {
                    case 1:
                        asda2Item = asda2Inventory.ShopItems[(int) num3];
                        break;
                    case 2:
                        asda2Item = asda2Inventory.RegularItems[(int) num3];
                        break;
                    default:
                        client.ActiveCharacter.YouAreFuckingCheater(
                            "Tried to use wrong inventory while registering auk items : " + (object) num2, 1);
                        Asda2AuctionHandler.SendRegisterItemToAukCancelWindowResponse(client,
                            (List<Asda2ItemTradeRef>) null);
                        return;
                }

                items.Add(new Asda2ItemTradeRef()
                {
                    Item = asda2Item,
                    Amount = num4,
                    Price = num5
                });
                num1 += num5;
            }

            if ((double) client.ActiveCharacter.Money <=
                (double) num1 * (double) CharacterFormulas.AuctionPushComission)
            {
                client.ActiveCharacter.SendAuctionMsg("Not enought money to register items.");
                Asda2AuctionHandler.SendRegisterItemToAukCancelWindowResponse(client, (List<Asda2ItemTradeRef>) null);
            }
            else
            {
                Asda2AuctionHandler.SendRegisterItemToAukCancelWindowResponse(client, items);
                foreach (Asda2ItemTradeRef itemRef in items)
                {
                    if (itemRef.Item == null)
                    {
                        activeCharacter.SendAuctionMsg("Failed to register item cause not founded.");
                        Asda2AuctionHandler.SendRegisterItemToAukCancelWindowResponse(client,
                            (List<Asda2ItemTradeRef>) null);
                        return;
                    }

                    if (itemRef.Amount < 0 || itemRef.Amount > itemRef.Item.Amount)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater(
                            "Tried to use wrong item amount while registering auk items : " + (object) itemRef.Amount,
                            1);
                        Asda2AuctionHandler.SendRegisterItemToAukCancelWindowResponse(client,
                            (List<Asda2ItemTradeRef>) null);
                        return;
                    }

                    if (itemRef.Item.IsSoulbound)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater(
                            "Tried to use soulbounded item while registering auk items : " + (object) itemRef.Amount,
                            1);
                        Asda2AuctionHandler.SendRegisterItemToAukCancelWindowResponse(client,
                            (List<Asda2ItemTradeRef>) null);
                        return;
                    }

                    asda2Inventory.AuctionItem(itemRef);
                }

                AchievementProgressRecord progressRecord =
                    client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(10U);
                switch (++progressRecord.Counter)
                {
                    case 100:
                        client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Merchant48);
                        break;
                    case 200:
                        client.ActiveCharacter.GetTitle(Asda2TitleId.Merchant48);
                        break;
                }

                progressRecord.SaveAndFlush();
                activeCharacter.SendAuctionMsg("You have success with registering auction items.");
                activeCharacter.SendMoneyUpdate();
            }
        }

        public static void SendRegisterItemToAukCancelWindowResponse(IRealmClient client,
            List<Asda2ItemTradeRef> items = null)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.RegisterItemToAukCancelWindow))
            {
                if (items != null)
                {
                    foreach (Asda2ItemTradeRef asda2ItemTradeRef in items)
                    {
                        packet.WriteByte(0);
                        packet.WriteInt32(asda2ItemTradeRef.Item.ItemId);
                        packet.WriteByte((byte) asda2ItemTradeRef.Item.InventoryType);
                        packet.WriteInt16(asda2ItemTradeRef.Item.Slot);
                        packet.WriteSkip(Asda2AuctionHandler.stab15);
                        packet.WriteInt32(asda2ItemTradeRef.Amount);
                        packet.WriteInt32(asda2ItemTradeRef.Item.Amount);
                        packet.WriteInt16(asda2ItemTradeRef.Item.Weight);
                        packet.WriteSkip(Asda2AuctionHandler.stab37);
                        packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                        packet.WriteInt32(client.ActiveCharacter.Money);
                        packet.WriteInt64(-1L);
                    }
                }

                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.ShowMyAukItems)]
        public static void ShowMyAukItemsRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 15;
            if (packet.ReadByte() == (byte) 1)
                Asda2AuctionHandler.SendMyAukItemsInfoResponse(client,
                    Asda2AuctionMgr.GetCharacterRegularItems((uint) client.ActiveCharacter.Record.Guid));
            else
                Asda2AuctionHandler.SendMyAukItemsInfoResponse(client,
                    Asda2AuctionMgr.GetCharacterShopItems((uint) client.ActiveCharacter.Record.Guid));
        }

        public static void SendMyAukItemsInfoResponse(IRealmClient client, List<Asda2ItemRecord> items)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MyAukItemsInfo))
            {
                int num = 0;
                foreach (Asda2ItemRecord asda2ItemRecord in items)
                {
                    if (num != 8)
                    {
                        packet.WriteInt32(asda2ItemRecord.AuctionId);
                        packet.WriteByte(0);
                        packet.WriteInt32(asda2ItemRecord.ItemId);
                        packet.WriteInt32(asda2ItemRecord.Amount);
                        packet.WriteByte(asda2ItemRecord.Durability);
                        packet.WriteByte(asda2ItemRecord.Enchant);
                        packet.WriteInt32(0);
                        packet.WriteInt32(526300);
                        packet.WriteInt16(13);
                        packet.WriteInt16(asda2ItemRecord.Parametr1Type);
                        packet.WriteInt16(asda2ItemRecord.Parametr1Value);
                        packet.WriteInt16(asda2ItemRecord.Parametr2Type);
                        packet.WriteInt16(asda2ItemRecord.Parametr2Value);
                        packet.WriteInt16(asda2ItemRecord.Parametr3Type);
                        packet.WriteInt16(asda2ItemRecord.Parametr3Value);
                        packet.WriteInt16(asda2ItemRecord.Parametr4Type);
                        packet.WriteInt16(asda2ItemRecord.Parametr4Value);
                        packet.WriteInt16(asda2ItemRecord.Parametr5Type);
                        packet.WriteInt16(asda2ItemRecord.Parametr5Value);
                        packet.WriteByte(0);
                        packet.WriteInt32((int) (asda2ItemRecord.AuctionEndTime - DateTime.Now).TotalMilliseconds);
                        packet.WriteInt32(asda2ItemRecord.AuctionPrice);
                        packet.WriteInt16(asda2ItemRecord.Soul1Id);
                        packet.WriteInt16(asda2ItemRecord.Soul2Id);
                        packet.WriteInt16(asda2ItemRecord.Soul3Id);
                        packet.WriteInt16(asda2ItemRecord.Soul4Id);
                        ++num;
                    }
                    else
                        break;
                }

                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.LoadDataFromAukPage)]
        public static void LoadDataFromAukPageRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 15;
            AucionCategoties category = (AucionCategoties) packet.ReadInt16();
            ++packet.Position;
            short option1 = packet.ReadInt16();
            byte option2 = packet.ReadByte();
            byte option3 = packet.ReadByte();
            byte curPage = packet.ReadByte();
            try
            {
                AuctionLevelCriterion requiredLevelCriterion;
                Asda2ItemAuctionCategory index = Asda2AuctionHandler.CalcCategory(category, option1, option2, option3,
                    out requiredLevelCriterion);
                SortedSet<Asda2ItemRecord> source = Asda2AuctionMgr.CategorizedItemsById[index][requiredLevelCriterion];
                Asda2AuctionHandler.SendItemsOnAukInfoResponse(client,
                    source.Skip<Asda2ItemRecord>((int) curPage * 7).Take<Asda2ItemRecord>(7),
                    (byte) ((source.Count - 1) / 7), curPage);
            }
            catch
            {
                client.ActiveCharacter.YouAreFuckingCheater("Sends wrong auction show items request.", 1);
            }
        }

        private static Asda2ItemAuctionCategory CalcCategory(AucionCategoties category, short option1, byte option2,
            byte option3, out AuctionLevelCriterion requiredLevelCriterion)
        {
            requiredLevelCriterion = AuctionLevelCriterion.All;
            switch (category)
            {
                case AucionCategoties.Warrior:
                    switch (option2)
                    {
                        case 0:
                            return Asda2ItemAuctionCategory.WarriorHelm;
                        case 1:
                            return Asda2ItemAuctionCategory.WarriorArmor;
                        case 2:
                            return Asda2ItemAuctionCategory.WarriorPants;
                        case 3:
                            return Asda2ItemAuctionCategory.WarriorBoots;
                        case 4:
                            return Asda2ItemAuctionCategory.WarriorGloves;
                        default:
                            return Asda2ItemAuctionCategory.WarriorPants;
                    }
                case AucionCategoties.Archer:
                    switch (option2)
                    {
                        case 0:
                            return Asda2ItemAuctionCategory.ArcherHelm;
                        case 1:
                            return Asda2ItemAuctionCategory.ArcherArmor;
                        case 2:
                            return Asda2ItemAuctionCategory.ArcherPants;
                        case 3:
                            return Asda2ItemAuctionCategory.ArcherBoots;
                        case 4:
                            return Asda2ItemAuctionCategory.ArcherGloves;
                        default:
                            return Asda2ItemAuctionCategory.ArcherPants;
                    }
                case AucionCategoties.Mage:
                    switch (option2)
                    {
                        case 0:
                            return Asda2ItemAuctionCategory.MageHelm;
                        case 1:
                            return Asda2ItemAuctionCategory.MageArmor;
                        case 2:
                            return Asda2ItemAuctionCategory.MagePants;
                        case 3:
                            return Asda2ItemAuctionCategory.MageBoots;
                        case 4:
                            return Asda2ItemAuctionCategory.MageGloves;
                        default:
                            return Asda2ItemAuctionCategory.MagePants;
                    }
                case AucionCategoties.Rings:
                    requiredLevelCriterion = (AuctionLevelCriterion) option1;
                    return Asda2ItemAuctionCategory.Ring;
                case AucionCategoties.Nackless:
                    requiredLevelCriterion = (AuctionLevelCriterion) option1;
                    return Asda2ItemAuctionCategory.Nackless;
                case AucionCategoties.Shield:
                    return Asda2ItemAuctionCategory.Shield;
                case AucionCategoties.Weapon:
                    switch (option2)
                    {
                        case 0:
                            return Asda2ItemAuctionCategory.WeaponOhs;
                        case 1:
                            return Asda2ItemAuctionCategory.WeaponSpear;
                        case 2:
                            return Asda2ItemAuctionCategory.WeaponThs;
                        case 3:
                            return Asda2ItemAuctionCategory.WeaponStaff;
                        case 4:
                            return Asda2ItemAuctionCategory.WeaponCrossbow;
                        case 5:
                            return Asda2ItemAuctionCategory.WeaponBow;
                        default:
                            return Asda2ItemAuctionCategory.WeaponCrossbow;
                    }
                case AucionCategoties.Premium:
                    return Asda2ItemAuctionCategory.Premium;
                case AucionCategoties.SowelRune:
                    requiredLevelCriterion = (AuctionLevelCriterion) option1;
                    if (option3 == (byte) 2)
                    {
                        switch (option2)
                        {
                            case 0:
                                return Asda2ItemAuctionCategory.RuneStrength;
                            case 1:
                                return Asda2ItemAuctionCategory.RuneDexterity;
                            case 2:
                                return Asda2ItemAuctionCategory.RuneStamina;
                            case 3:
                                return Asda2ItemAuctionCategory.RuneSpirit;
                            case 4:
                                return Asda2ItemAuctionCategory.RuneIntellect;
                            case 5:
                                return Asda2ItemAuctionCategory.RuneLuck;
                            case 6:
                                return Asda2ItemAuctionCategory.RuneMisc;
                            default:
                                return Asda2ItemAuctionCategory.RuneMisc;
                        }
                    }
                    else
                    {
                        switch (option2)
                        {
                            case 0:
                                return Asda2ItemAuctionCategory.SowelOHS;
                            case 1:
                                return Asda2ItemAuctionCategory.SowelSpear;
                            case 2:
                                return Asda2ItemAuctionCategory.SowelThs;
                            case 3:
                                return Asda2ItemAuctionCategory.SowelBow;
                            case 4:
                                return Asda2ItemAuctionCategory.SowelCrossBow;
                            case 5:
                                return Asda2ItemAuctionCategory.SowelStaff;
                            case 6:
                                return Asda2ItemAuctionCategory.SowelArmor;
                            case 7:
                                return Asda2ItemAuctionCategory.SowelArmor;
                            case 8:
                                return Asda2ItemAuctionCategory.SowelArmor;
                            case 9:
                                return Asda2ItemAuctionCategory.SowelStrengs;
                            case 10:
                                return Asda2ItemAuctionCategory.SowelDexterity;
                            case 11:
                                return Asda2ItemAuctionCategory.SowelStamina;
                            case 12:
                                return Asda2ItemAuctionCategory.SowelSpirit;
                            case 13:
                                return Asda2ItemAuctionCategory.SowelIntellect;
                            case 14:
                                return Asda2ItemAuctionCategory.SowelLuck;
                            case 15:
                                return Asda2ItemAuctionCategory.SowelMisc;
                            default:
                                return Asda2ItemAuctionCategory.SowelMisc;
                        }
                    }
                case AucionCategoties.Upgrade:
                    requiredLevelCriterion = (AuctionLevelCriterion) option1;
                    return option2 == (byte) 0
                        ? Asda2ItemAuctionCategory.UpgradeWeapon
                        : Asda2ItemAuctionCategory.UpgradeArmor;
                case AucionCategoties.Potion:
                    switch (option2)
                    {
                        case 0:
                            return Asda2ItemAuctionCategory.PotionHp;
                        case 1:
                            return Asda2ItemAuctionCategory.PotionMp;
                        case 2:
                            return Asda2ItemAuctionCategory.PotionFish;
                        default:
                            return Asda2ItemAuctionCategory.PotionHp;
                    }
                case AucionCategoties.Crafting:
                    switch (option2)
                    {
                        case 0:
                            requiredLevelCriterion = (AuctionLevelCriterion) option1;
                            return Asda2ItemAuctionCategory.Recipe;
                        case 1:
                            return Asda2ItemAuctionCategory.Materials;
                        default:
                            return Asda2ItemAuctionCategory.Recipe;
                    }
                case AucionCategoties.Other:
                    return option2 == (byte) 0 ? Asda2ItemAuctionCategory.Boosters : Asda2ItemAuctionCategory.Misc;
                default:
                    return Asda2ItemAuctionCategory.Misc;
            }
        }

        public static void SendItemsOnAukInfoResponse(IRealmClient client, IEnumerable<Asda2ItemRecord> items,
            byte pagesCount, byte curPage)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemsOnAukInfo))
            {
                foreach (Asda2ItemRecord asda2ItemRecord in items)
                {
                    packet.WriteInt32(asda2ItemRecord.AuctionId);
                    packet.WriteByte(0);
                    packet.WriteInt32(asda2ItemRecord.ItemId);
                    packet.WriteInt32(asda2ItemRecord.Amount);
                    packet.WriteByte(asda2ItemRecord.Durability);
                    packet.WriteByte(asda2ItemRecord.Enchant);
                    packet.WriteInt32(16777216);
                    packet.WriteInt32(0);
                    packet.WriteInt16(0);
                    packet.WriteInt16(asda2ItemRecord.Parametr1Type);
                    packet.WriteInt16(asda2ItemRecord.Parametr1Value);
                    packet.WriteInt16(asda2ItemRecord.Parametr2Type);
                    packet.WriteInt16(asda2ItemRecord.Parametr2Value);
                    packet.WriteInt16(asda2ItemRecord.Parametr3Type);
                    packet.WriteInt16(asda2ItemRecord.Parametr3Value);
                    packet.WriteInt16(asda2ItemRecord.Parametr4Type);
                    packet.WriteInt16(asda2ItemRecord.Parametr4Value);
                    packet.WriteInt16(asda2ItemRecord.Parametr5Type);
                    packet.WriteInt16(asda2ItemRecord.Parametr5Value);
                    packet.WriteByte(0);
                    packet.WriteByte(curPage);
                    packet.WriteByte(pagesCount);
                    packet.WriteInt32(asda2ItemRecord.AuctionPrice);
                    packet.WriteFixedAsciiString(asda2ItemRecord.OwnerName, 20, Locale.Start);
                    packet.WriteInt16(asda2ItemRecord.Soul1Id);
                    packet.WriteInt16(asda2ItemRecord.Soul2Id);
                    packet.WriteInt16(asda2ItemRecord.Soul3Id);
                    packet.WriteInt16(asda2ItemRecord.Soul4Id);
                }

                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.BuyFromAuk)]
        public static void BuyFromAukRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 11;
            List<int> aucIds = new List<int>();
            for (int index = 0; index < 7; ++index)
            {
                packet.Position += 4;
                if (packet.RemainingLength > 0)
                {
                    int num = packet.ReadInt32();
                    aucIds.Add(num);
                    packet.Position += 41;
                }
                else
                    break;
            }

            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                Asda2AuctionMgr.TryBuy(aucIds, client.ActiveCharacter)));
        }

        public static void SendItemsBuyedFromAukResponse(IRealmClient client, List<Asda2ItemTradeRef> items)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemsBuyedFromAuk))
            {
                int num = 0;
                foreach (Asda2ItemTradeRef asda2ItemTradeRef in items)
                {
                    Asda2Item asda2Item = asda2ItemTradeRef.Item;
                    if (num < 7)
                    {
                        packet.WriteInt32(asda2ItemTradeRef.Price);
                        packet.WriteSkip(Asda2AuctionHandler.stub4);
                        packet.WriteInt32(asda2Item.ItemId);
                        packet.WriteInt32(asda2ItemTradeRef.Amount);
                        packet.WriteByte((byte) asda2Item.InventoryType);
                        packet.WriteInt16(asda2Item.Slot);
                        packet.WriteInt16(asda2Item.Weight);
                        packet.WriteByte(asda2Item.Durability);
                        packet.WriteInt32(asda2Item.Enchant);
                        packet.WriteByte(0);
                        packet.WriteInt32(0);
                        packet.WriteInt16(0);
                        packet.WriteInt16(asda2Item.Record.Parametr1Type);
                        packet.WriteInt16(asda2Item.Parametr1Value);
                        packet.WriteInt16(asda2Item.Record.Parametr2Type);
                        packet.WriteInt16(asda2Item.Parametr2Value);
                        packet.WriteInt16(asda2Item.Record.Parametr3Type);
                        packet.WriteInt16(asda2Item.Parametr3Value);
                        packet.WriteInt16(asda2Item.Record.Parametr4Type);
                        packet.WriteInt16(asda2Item.Parametr4Value);
                        packet.WriteInt16(asda2Item.Record.Parametr5Type);
                        packet.WriteInt16(asda2Item.Parametr5Value);
                        packet.WriteByte(0);
                        packet.WriteInt32(asda2Item.AuctionPrice);
                        packet.WriteFixedAsciiString(client.ActiveCharacter.Name, 20, Locale.Start);
                        packet.WriteInt16(asda2Item.Soul1Id);
                        packet.WriteInt16(asda2Item.Soul2Id);
                        packet.WriteInt16(asda2Item.Soul3Id);
                        packet.WriteInt16(asda2Item.Soul4Id);
                        ++num;
                    }
                    else
                        break;
                }

                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.RemoveFromAuk)]
        public static void RemoveFromAukRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 4;
            List<int> items = new List<int>();
            for (int index = 0; index < 8; ++index)
            {
                packet.Position += 19;
                if (packet.RemainingLength > 4)
                {
                    int num = packet.ReadInt32();
                    packet.Position += 26;
                    items.Add(num);
                }
                else
                    break;
            }

            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                Asda2AuctionMgr.TryRemoveItems(client.ActiveCharacter, items)));
        }

        public static void SendItemFromAukRemovedResponse(IRealmClient client, List<Asda2ItemTradeRef> asda2Items)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemFromAukRemoved))
            {
                int num = 0;
                foreach (Asda2ItemTradeRef asda2Item1 in asda2Items)
                {
                    Asda2Item asda2Item2 = asda2Item1.Item;
                    if (num < 8)
                    {
                        packet.WriteInt32(asda2Item1.Price);
                        packet.WriteSkip(Asda2AuctionHandler.stub4);
                        packet.WriteInt32(asda2Item2.ItemId);
                        packet.WriteInt32(asda2Item1.Amount);
                        packet.WriteByte((byte) asda2Item2.InventoryType);
                        packet.WriteInt16(asda2Item2.Slot);
                        packet.WriteInt16(asda2Item2.Weight);
                        packet.WriteByte(asda2Item2.Durability);
                        packet.WriteInt32(asda2Item2.Enchant);
                        packet.WriteByte(0);
                        packet.WriteInt32(0);
                        packet.WriteInt16(0);
                        packet.WriteInt16(asda2Item2.Record.Parametr1Type);
                        packet.WriteInt16(asda2Item2.Parametr1Value);
                        packet.WriteInt16(asda2Item2.Record.Parametr2Type);
                        packet.WriteInt16(asda2Item2.Parametr2Value);
                        packet.WriteInt16(asda2Item2.Record.Parametr3Type);
                        packet.WriteInt16(asda2Item2.Parametr3Value);
                        packet.WriteInt16(asda2Item2.Record.Parametr4Type);
                        packet.WriteInt16(asda2Item2.Parametr4Value);
                        packet.WriteInt16(asda2Item2.Record.Parametr5Type);
                        packet.WriteInt16(asda2Item2.Parametr5Value);
                        packet.WriteByte(0);
                        packet.WriteInt16(asda2Item2.Soul1Id);
                        packet.WriteInt16(asda2Item2.Soul2Id);
                        packet.WriteInt16(asda2Item2.Soul3Id);
                        packet.WriteInt16(asda2Item2.Soul4Id);
                        ++num;
                    }
                    else
                        break;
                }

                client.Send(packet, false);
            }
        }
    }
}