using System;
using System.IO;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Core.Network;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Spells;
using WCell.Util.Graphics;
using System.Linq;
using WCell.RealmServer.Commands;

namespace WCell.RealmServer.Handlers
{
    public static class FunctionalItemsHandler
    {
        [PacketHandler(RealmServerOpCode.StopUseFunctionalItem)]//5455
        public static void StopUseFunctionalItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            var funcItemId = packet.ReadInt32();//default : 585Len : 4
            client.ActiveCharacter.CancelTransports();
        }

        public static void SendShopItemUsedResponse(IRealmClient client, int itemId, int durationSecs = -1)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ShopItemUsed))//5453
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{sessId}default value : 96 Len : 2
                packet.WriteInt32(itemId);//{itemId}default value : 82 Len : 4
                packet.WriteInt32(durationSecs);//{durationSecs}default value : 1800 Len : 4
                client.ActiveCharacter.SendPacketToArea(packet, true, true);
                client.Send(packet, addEnd: true);
            }
        }
        public static void SendShopItemUsedResponse(IRealmClient rcv, Character trigger, int durationSecs = -1)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ShopItemUsed))//5453
            {
                packet.WriteInt16(trigger.SessionId);//{sessId}default value : 96 Len : 2
                packet.WriteInt32(trigger.TransportItemId);//{itemId}default value : 82 Len : 4
                packet.WriteInt32(durationSecs);//{durationSecs}default value : 1800 Len : 4
                rcv.ActiveCharacter.SendPacketToArea(packet, true, true);
                rcv.Send(packet, addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.UseTeleportScroll)]//5458
        public static void UseTeleportScrollRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;//nk8 default : 1Len : 2
            var targetCharName = packet.ReadAsdaString(20, Locale.En);//default : Len : 20
            var targetChr = World.GetCharacter(targetCharName, false);
            if (targetChr == null)
            {
                client.ActiveCharacter.SendWarMsg(string.Format("{0} is not in game.", targetCharName));
                return;
            }
            if (targetChr.IsAsda2BattlegroundInProgress)
            {
                client.ActiveCharacter.SendWarMsg(string.Format("{0} is  on war.", targetCharName));
                return;
            }
            if (!targetChr.EnableWishpers && !client.ActiveCharacter.Role.IsStaff)
            {
                client.ActiveCharacter.SendInfoMsg("you cant do that!");
                return;
            }
            if (client.ActiveCharacter.IsAsda2BattlegroundInProgress)
            {
                client.ActiveCharacter.SendWarMsg("You cant teleport on war.");
                return;
            }
            var scrollRemoved = client.ActiveCharacter.Asda2Inventory.UseTeleportScroll();
            if (scrollRemoved)
            {
                client.ActiveCharacter.TeleportTo(targetChr);
                Asda2TitleChecker.OnTeleportScrolUse(client.ActiveCharacter);
            }
            else
                client.ActiveCharacter.SendSystemMessage("You have not teleport scroll");
        }


        static readonly byte[] unk14 = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        static readonly byte[] stab90 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        static readonly byte[] stab291 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 };

        [PacketHandler(RealmServerOpCode.UseShopItem)]//5450
        public static void UseShopItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 6;
            var slot = packet.ReadInt16();//default : 4Len : 2
            packet.Position += 4;//nk10 default : 0Len : 4
            packet.Position += 1;//nk11 default : 1Len : 1
            packet.Position += 2;//nk12 default : -1Len : 2
            var sessId0 = packet.ReadInt16();//default : 35Len : 2
            packet.Position += 38;//nk14 default : unk14Len : 38
            packet.Position += 200;//tab90 default : stab90Len : 200

            uint parametr = 0;//default : 3Len : 1
            try { parametr = packet.ReadUInt32(); }
            catch (EndOfStreamException) { }
            client.ActiveCharacter.AddMessage(() => ProcessFunctionalItem(client, parametr, slot));
        }

        private static void ProcessFunctionalItem(IRealmClient client, uint parametr, short slot)
        {
            var item = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slot);
            if (item == null)
            {
                SendUpdateShopItemInfoResponse(client, UseFunctionalItemError.FunctionalItemDoesNotExist);
                return;
            }
            var status = UseFunctionalItemError.Ok;
            if (item.RequiredLevel > client.ActiveCharacter.Level)
            {
                SendUpdateShopItemInfoResponse(client, UseFunctionalItemError.YorLevelIsNotHightEnoght, item);
                return;
            }
            RealmServer.IOQueue.AddMessage(() =>
            {
                try
                {
                    switch (item.Category)
                    {
                        case Asda2ItemCategory.ExpandWarehouse:
                            if (client.ActiveCharacter.Record.PremiumWarehouseBagsCount >= 8)
                                status = UseFunctionalItemError.WarehouseHasReachedMaxCapacity;

                            else
                            {
                                client.ActiveCharacter.Record.PremiumWarehouseBagsCount++;
                                SendWarehouseSlotsExpandedResponse(client, false);
                            }
                            break;
                       /* case Asda2ItemCategory.AvatarWarehouseExpand:

                            if (client.ActiveCharacter.Record.PremiumAvatarWarehouseBagsCount >= 8)
                                status = UseFunctionalItemError.WarehouseHasReachedMaxCapacity;
                            else
                            {
                                client.ActiveCharacter.Record.PremiumAvatarWarehouseBagsCount++;
                                SendWarehouseSlotsExpandedResponse(client, false);
                            }
                            break;*/
                        case Asda2ItemCategory.IncExp:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncMoveSpeed:
                            if (item.Record.IsSoulBound && item.Record.AuctionEndTime != DateTime.MinValue &&
                                DateTime.Now > item.Record.AuctionEndTime)
                            {
                                client.ActiveCharacter.Asda2Inventory.RemoveItemFromInventory(item);
                                item.Destroy();
                                client.ActiveCharacter.SendInfoMsg("Vehicle expired.");
                                status = UseFunctionalItemError.TheDurationOfTheShopitemHaExprised;

                            }
                            else
                            {
                                if (!item.IsSoulbound)
                                {
                                    Asda2TitleChecker.OnUseVeiche(client.ActiveCharacter);
                                }
                                item.IsSoulbound = true;
                                item.Record.AuctionEndTime = DateTime.Now + TimeSpan.FromDays(item.AttackTime);
                                client.ActiveCharacter.TransportItemId = item.ItemId;
                            }
                            break;
                        case Asda2ItemCategory.IncHp:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncMp:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncAtackSpeed:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncDex:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncDigChance:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncDropChance:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncExpStackable:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncInt:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncLuck:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncMAtk:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncMdef:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncPAtk:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncPDef:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncSpi:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncSta:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.IncStr:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.DoublePetExpirience:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.PetNotEating:
                            client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId);
                            SendShopItemUsedResponse(client, item.ItemId, item.Template.AtackRange);
                            break;
                        case Asda2ItemCategory.ResetAllSkill:
                            ResetSkills(client.ActiveCharacter);
                            Asda2CharacterHandler.SendLearnedSkillsInfo(client.ActiveCharacter);
                            Asda2TitleChecker.OnResetAllSkills(client.ActiveCharacter);
                            break;
                        case Asda2ItemCategory.ExpandInventory:
                            SendPremiumLongBuffInfoResponse(client,
                                (byte)client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, true),
                                item.ItemId, (short)item.Template.PackageId);
                            break;
                        case Asda2ItemCategory.RemoveDeathPenaltiesByDays:
                            SendPremiumLongBuffInfoResponse(client,
                                (byte)client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, true),
                                item.ItemId, (short)item.Template.PackageId);
                            break;
                        case Asda2ItemCategory.ShopBanner:
                            if (client.ActiveCharacter.Level < 10)
                                status = UseFunctionalItemError.YorLevelIsNotHightEnoght;
                            else
                                SendPremiumLongBuffInfoResponse(client,
                                    (byte)client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, true),
                                    item.ItemId, (short)item.Template.PackageId);
                            break;
                        case Asda2ItemCategory.PetNotEatingByDays:
                            SendPremiumLongBuffInfoResponse(client,
                                (byte)client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, true),
                                item.ItemId, (short)item.Template.PackageId);
                            break;
                        case Asda2ItemCategory.InstantRecover100PrcHPandMP:
                            if (client.ActiveCharacter.Last100PrcRecoveryUsed + 30000 > (uint)Environment.TickCount)
                                status = UseFunctionalItemError.CoolingTimeRemain;
                            else
                            {
                                client.ActiveCharacter.Last100PrcRecoveryUsed = (uint)Environment.TickCount;
                                client.ActiveCharacter.HealPercent(100);
                                client.ActiveCharacter.Power = client.ActiveCharacter.MaxPower;

                            }
                            break;
                        case Asda2ItemCategory.InstantRecover100PrcHP:
                            if (client.ActiveCharacter.Last100PrcRecoveryUsed + 30000 > (uint)Environment.TickCount)
                                status = UseFunctionalItemError.CoolingTimeRemain;
                            else
                            {
                                client.ActiveCharacter.Last100PrcRecoveryUsed = (uint)Environment.TickCount;
                                client.ActiveCharacter.HealPercent(100);
                            }
                            break;
                        case Asda2ItemCategory.RecoverHp10TimesByPrcOver30Sec:
                            PereodicAction a = null;
                            if (client.ActiveCharacter.PereodicActions.ContainsKey(Asda2PereodicActionType.HpRegenPrc))
                                a = client.ActiveCharacter.PereodicActions[Asda2PereodicActionType.HpRegenPrc];
                            if (a != null && a.CallsNum >= 6 && a.Value >= item.Template.ValueOnUse)
                                status = UseFunctionalItemError.CoolingTimeRemain;
                            else
                            {
                                if (
                                    client.ActiveCharacter.PereodicActions.ContainsKey(
                                        Asda2PereodicActionType.HpRegenPrc))
                                    client.ActiveCharacter.PereodicActions.Remove(Asda2PereodicActionType.HpRegenPrc);
                                a = new PereodicAction(client.ActiveCharacter, item.Template.ValueOnUse, 10, 5000,
                                    Asda2PereodicActionType.HpRegenPrc);
                                client.ActiveCharacter.PereodicActions.Add(Asda2PereodicActionType.HpRegenPrc, a);
                            }
                            break;
                        case Asda2ItemCategory.PremiumPotions:
                            SendPremiumLongBuffInfoResponse(client,
                                (byte)client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, true),
                                item.ItemId, (short)item.Template.PackageId);
                            break;
                        case Asda2ItemCategory.TeleportToCharacter:
                            if (parametr >= 10 ||
                                client.ActiveCharacter.TeleportPoints[parametr] == null)
                            {
                                status = UseFunctionalItemError.FailedToUse;
                            }
                            else
                            {
                                var point = client.ActiveCharacter.TeleportPoints[parametr];
                                Asda2TitleChecker.OnTeleportingToTelepotPoint(client.ActiveCharacter);
                                client.ActiveCharacter.TeleportTo(point.MapId, new Vector3(point.X, point.Y));
                            }
                            break;
                       // case Asda2ItemCategory.Monstertransformpotion:

                           // break;
                        case Asda2ItemCategory.OpenWarehouse:

                            break;
                        case Asda2ItemCategory.ResetOneSkill:
                            var spell = client.ActiveCharacter.Spells.First(s => s.RealId == parametr);
                            if (spell != null)
                            {
                                var totalcost = 0;
                                totalcost += spell.Cost;
                                for (int i = spell.Level - 1; i > 0; i--)
                                {
                                    var lowSpell = SpellHandler.Get((uint)(spell.RealId + i * 1000));
                                    if (lowSpell != null)
                                        totalcost += lowSpell.Cost;
                                }
                                var tm = (uint)(totalcost / 2);
                                client.ActiveCharacter.Spells.Remove(spell);
                                client.ActiveCharacter.AddMoney(tm);
                                Asda2CharacterHandler.SendPreResurectResponse(client.ActiveCharacter);
                                SendSkillResetedResponse(client, spell.RealId, (short)spell.Level, tm);
                                Asda2CharacterHandler.SendUpdateStatsOneResponse(client);
                                Asda2CharacterHandler.SendUpdateStatsResponse(client);
                                client.ActiveCharacter.SendMoneyUpdate();
                            }
                            else
                            {
                                status = UseFunctionalItemError.FailedToUse;
                                client.ActiveCharacter.SendInfoMsg("Skill is not learned. Restart client.");
                            }
                            break;
                        default:
                            status = UseFunctionalItemError.NotAunctionalItem;
                            break;
                    }
                }
                catch (AlreadyBuffedExcepton)
                {
                    status = UseFunctionalItemError.AlreadyFeelingTheEffectOfSimilarSkillType;
                }
                if (status == UseFunctionalItemError.Ok && item.Category != Asda2ItemCategory.IncMoveSpeed)
                    item.ModAmount(-1);
                SendUpdateShopItemInfoResponse(client, status, item);
            });
        }
        public static void SendWingsInfoResponse(Character chr, IRealmClient reciever)
        {
            if (chr.Asda2WingsItemId == -1)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.WingsInfo))//6585
            {
                packet.WriteInt32(chr.AccId);//{accId}default value : 361343 Len : 4
                packet.WriteInt16(chr.Asda2WingsItemId);//value name : unk6 default value : 58Len : 2
                packet.WriteSkip(stub6);//{stub6}default value : stub6 Len : 5
                if (reciever == null)
                    chr.SendPacketToArea(packet, true, true);
                else
                    reciever.Send(packet, addEnd: true);
            }
        }
        static readonly byte[] stub6 = new byte[] { 0x00, 0x8D, 0x27, 0x00, 0x01 };

        [PacketHandler(RealmServerOpCode.UseChangeFactionItem)]//6704
        public static void UseChangeFactionItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 5;//tab32 default : stab32Len : 8
            var slot = packet.ReadInt16();//default : 4Len : 2
            var item = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slot);
            if (item == null || item.Category != Asda2ItemCategory.ResetFaction || client.ActiveCharacter.Asda2FactionId == -1)
            {
                SendFactionResetedResponse(client, false, null);
                return;
            }
            if (client.ActiveCharacter.CurrentBattleGround != null)
            {
                SendFactionResetedResponse(client, false, null);
                return;
            }
            if (client.ActiveCharacter.IsAsda2BattlegroundInProgress)
            {
                SendFactionResetedResponse(client, false, null);
                return;
            }
            item.Amount--;
            client.ActiveCharacter.Asda2FactionId = -1;
            SendFactionResetedResponse(client, true, item);
        }

        static readonly byte[] stab32 = new byte[] { 0x00, 0x00, 0x00, 0x0D, 0x00, 0x00, 0x00, 0x01 };
        public static void SendFactionResetedResponse(IRealmClient client, bool ok, Asda2Item item)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.FactionReseted))//6705
            {
                packet.WriteByte(ok ? 1 : 0);//{status}default value : 1 Len : 1
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, true);
                client.Send(packet);
            }
        }
        static readonly byte[] stab31 = new byte[] { 0x00, 0x00, 0x00 };

        public static void ResetSkills(Character chr)
        {
            chr.Spells.Clear();
            chr.Spells.AddDefaultSpells();
            SendPreResurectResponse(chr.Client);
            SendAllSkillsResetedInfoMsgResponse(chr.Client);
            SendAllSkillsResetedResponse(chr.Client);
        }

        public static void SendUpdateShopItemInfoResponse(IRealmClient client, UseFunctionalItemError status, Asda2Item funcItem = null)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.UpdateShopItemInfo))//5451
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{sessId}default value : 57 Len : 2
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, funcItem, false);
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);//{invWeight}default value : 10364 Len : 2
                packet.WriteInt32((int)(funcItem == null ? 0 : funcItem.Record == null ? -1 : (funcItem.Record.AuctionEndTime - DateTime.Now).TotalHours));//value name : unk4 default value : 0Len : 4
                packet.WriteInt16(15000);
                client.Send(packet, addEnd: true);
            }
        }
        public static void SendWarehouseSlotsExpandedResponse(IRealmClient client, bool isAvatar)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.WarehouseSlotsExpanded))//5456
            {
                packet.WriteByte(isAvatar ? 3 : 2);//{status}default value : 2 Len : 1
                packet.WriteInt16((isAvatar ? client.ActiveCharacter.Record.PremiumAvatarWarehouseBagsCount + 1 : client.ActiveCharacter.Record.PremiumWarehouseBagsCount + 1) * 30);//{slots}default value : 60 Len : 2
                client.Send(packet, addEnd: true);
            }
        }
        public static void SendCancelCancelFunctionalItemResponse(IRealmClient client, short itemId)
        {
            if (client.ActiveCharacter == null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.CancelFunctionalItem))//5454
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{sessId}default value : 45 Len : 2
                packet.WriteInt32(itemId);//{transportId}default value : 77 Len : 4
                client.ActiveCharacter.SendPacketToArea(packet, true, true);
            }
        }

        #region reset  skills
        public static void SendSkillResetedResponse(IRealmClient client, short skillId, short skillLevel, uint goldRecived)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SkillReseted))//5464
            {
                packet.WriteInt32(skillId);//{skillId}default value : 505 Len : 4
                packet.WriteInt16(skillLevel);//{skillLevel}default value : 7 Len : 2
                packet.WriteInt32(goldRecived);//{goldRecived}default value : 80500 Len : 4
                client.Send(packet, addEnd: true);
            }
        }

        public static void SendPreResurectResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.PreResurect))//5306
            {
                client.Send(packet);
            }
        }
        public static void SendAllSkillsResetedInfoMsgResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.AllSkillsResetedInfoMsg))//5462
            {
                packet.WriteByte(1);//{status}default value : 1 Len : 1
                packet.WriteInt16(client.ActiveCharacter.Spells.AvalibleSkillPoints);//{skillPoints}default value : 51 Len : 2
                packet.WriteInt32(client.ActiveCharacter.Money);//{money}default value : 5548558 Len : 4
                client.Send(packet, addEnd: true);
            }
        }
        public static void SendAllSkillsResetedResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.AllSkillsReseted))//6057
            {
                client.Send(packet);
            }
        }
        public static void SendPremiumLongBuffInfoResponse(IRealmClient client, byte slot, int itemId, short itemCategory)
        {
            if (client.ActiveCharacter == null || client.ActiveCharacter.LongTimePremiumBuffs == null)
                return;

            using (var packet = new RealmPacketOut(RealmServerOpCode.PremiumLongBuffInfo))//6634
            {
                packet.WriteByte(slot);//{slot}default value : 0 Len : 1
                packet.WriteInt16(itemCategory);//value name : stab8 default value : stab8Len : 2
                packet.WriteInt16(itemId);//{itemId}default value : 39 Len : 2
                packet.WriteInt32((int)((client.ActiveCharacter.LongTimePremiumBuffs[slot].EndsDate - DateTime.Now).TotalSeconds));
                client.Send(packet, addEnd: true);
            }
        }
        static readonly byte[] stab8 = new byte[] { 0xDA, 0x00 };

        #endregion
        [PacketHandler(RealmServerOpCode.ExpandPetBox)]//6121
        public static void ExpandPetBoxRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 1;
            var slot = packet.ReadInt16();//default : 24Len : 2
            if (client.ActiveCharacter.Record.PetBoxEnchants > 14)
            {
                SendPetBoxExpandedResponse(client, ExpandPetBoxStatus.Fail, slot);
                client.ActiveCharacter.SendInfoMsg("Pet box expand for maximum 90 slots.");
                return;
            }
            var item = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slot);
            if (item == null)
            {
                SendPetBoxExpandedResponse(client, ExpandPetBoxStatus.Fail, slot);
                client.ActiveCharacter.SendInfoMsg("Item not found restart client.");
                return;
            }
            if (item.Category != Asda2ItemCategory.ExpandPetBoxBy6)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Tryes to expand pet box with wrong item.", 50);
                return;
            }
            item.Amount--;
            client.ActiveCharacter.Record.PetBoxEnchants++;
            SendPetBoxExpandedResponse(client, ExpandPetBoxStatus.Ok, slot);
        }
        public static void SendPetBoxExpandedResponse(IRealmClient client, ExpandPetBoxStatus status, short slot)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.PetBoxExpanded))//6122
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 361343 Len : 4
                packet.WriteByte((byte)status);//{result}default value : 1 Len : 1
                packet.WriteByte((client.ActiveCharacter.Record.PetBoxEnchants + 1) * 6);//{petStorageSize}default value : 12 Len : 1
                packet.WriteByte(1);//{inv}default value : 1 Len : 1
                packet.WriteInt16(slot);//{slot}default value : 24 Len : 2
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);//{weight}default value : 11639 Len : 4
                client.Send(packet, addEnd: true);
            }
        }

        public enum ExpandPetBoxStatus
        {
            Fail = 0,
            Ok = 1,
        }
    }
    public enum UseFunctionalItemError
    {
        FailedToUse = 0,
        Ok = 1,
        VeicheOk = 1,
        FunctionalItemDoesNotExist = 2,
        YorLevelIsNotHightEnoght = 3,
        YouCantUseItCauseYouHaveMagicProtection = 4,
        YouCantUseItCauseYourTargetHaveMagicProtection = 5,
        IncorectTargetInformaton = 6,
        WarehouseHasReachedMaxCapacity = 7,
        CoolingTimeRemain = 8,
        NotAunctionalItem = 9,
        CoordinateHasNotBeenTargetedInSkillScope = 10,
        TheUserTargetedByThisItemDoesNotExist = 11,
        ItemEnduranceIsAlready100Prc = 13,
        CannotUseItemToProtectAgainstExpirienceLostWhileYouRevivingYourSelf = 14,
        CannotUseableByUserWhoNotChangeHisJob = 15,
        AlreadyFeelingTheEffectOfSimilarSkillType = 16,
        HpIs100Prc = 17,
        MpIs100Prc = 18,
        HpAdMpIs100Prc = 19,
        YouCantUseThisItemWhileYourStatusIsSoulmate = 20,
        YouCanonlyUseItWhenYouCompleteQuest = 21,
        VeicheHasExprised = 22,
        YouCantRideVeicheInDungeon = 23,
        YouCanOnlyUseAMaxOf4WeightIncreaseItems = 25,
        ItIsNotAllowedCauseShopIsAlreadyOpened = 27,
        TheDurationOfTheShopitemHaExprised = 28,
        ThisTypeOfItemIsAlreadyInUse = 29,
        UnvalidItemInformation = 30,
        YouCantChangeToTheSameJob = 31,
        UnavlidJobInformationToChanging = 32,
        CannotChangeableToOtherClass = 33,
        CannotChangeJobAnyMore = 34,


    }
}