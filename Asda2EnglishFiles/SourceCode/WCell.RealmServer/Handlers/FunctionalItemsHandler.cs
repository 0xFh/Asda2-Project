using System;
using System.IO;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Handlers
{
    public static class FunctionalItemsHandler
    {
        private static readonly byte[] unk14 = new byte[38]
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
            byte.MaxValue
        };

        private static readonly byte[] stab90 = new byte[200];
        private static readonly byte[] stab291 = new byte[5];

        private static readonly byte[] stub6 = new byte[5]
        {
            (byte) 0,
            (byte) 141,
            (byte) 39,
            (byte) 0,
            (byte) 1
        };

        private static readonly byte[] stab32 = new byte[8]
        {
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 13,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 1
        };

        private static readonly byte[] stab31 = new byte[3];

        private static readonly byte[] stab8 = new byte[2]
        {
            (byte) 218,
            (byte) 0
        };

        [PacketHandler(RealmServerOpCode.StopUseFunctionalItem)]
        public static void StopUseFunctionalItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadInt32();
            client.ActiveCharacter.CancelTransports();
        }

        public static void SendShopItemUsedResponse(IRealmClient client, int itemId, int durationSecs = -1)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ShopItemUsed))
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt32(itemId);
                packet.WriteInt32(durationSecs);
                client.Send(packet, true);
                client.ActiveCharacter.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendShopItemUsedResponse(IRealmClient rcv, Character trigger, int durationSecs = -1)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ShopItemUsed))
            {
                packet.WriteInt16(trigger.SessionId);
                packet.WriteInt32(trigger.TransportItemId);
                packet.WriteInt32(durationSecs);
                rcv.Send(packet, true);
                rcv.ActiveCharacter.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        [PacketHandler(RealmServerOpCode.UseTeleportScroll)]
        public static void UseTeleportScrollRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            string name = packet.ReadAsdaString(20, Locale.Start);
            Character character = World.GetCharacter(name, false);
            if (character == null)
                client.ActiveCharacter.SendSystemMessage(string.Format("{0} is not in game.", (object) name));
            else if (character.IsAsda2BattlegroundInProgress)
                client.ActiveCharacter.SendSystemMessage(string.Format("{0} is  on war.", (object) name));
            else if (client.ActiveCharacter.IsAsda2BattlegroundInProgress)
                client.ActiveCharacter.SendSystemMessage("You cant teleport on war.");
            else if (client.ActiveCharacter.Name == character.Name)
            {
                client.ActiveCharacter.SendSystemMessage("You cant teleport oneself.");
            }
            else
            {
                bool flag = client.ActiveCharacter.Asda2Inventory.UseTeleportScroll(false);
                AchievementProgressRecord progressRecord =
                    client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(88U);
                switch (++progressRecord.Counter)
                {
                    case 50:
                        client.ActiveCharacter.Map.CallDelayed(500,
                            (Action) (() => client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Stalker222)));
                        break;
                    case 100:
                        client.ActiveCharacter.Map.CallDelayed(500,
                            (Action) (() => client.ActiveCharacter.GetTitle(Asda2TitleId.Stalker222)));
                        break;
                }

                progressRecord.SaveAndFlush();
                if (flag)
                    client.ActiveCharacter.TeleportTo((IWorldLocation) character);
                else
                    client.ActiveCharacter.SendSystemMessage("You have not teleport scroll");
            }
        }

        [PacketHandler((RealmServerOpCode) 6134)]
        public static void UseSummonScrollRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            string name = packet.ReadAsdaString(20, Locale.Start);
            Character character = World.GetCharacter(name, false);
            if (character == null)
                client.ActiveCharacter.SendSystemMessage(string.Format("{0} is not in game.", (object) name));
            else if (character.IsAsda2BattlegroundInProgress)
                client.ActiveCharacter.SendSystemMessage(string.Format("{0} is on war.", (object) name));
            else if (client.ActiveCharacter.IsAsda2BattlegroundInProgress)
                client.ActiveCharacter.SendSystemMessage("You cant teleport on war.");
            else if (client.ActiveCharacter.Name == character.Name)
            {
                client.ActiveCharacter.SendSystemMessage("You cant teleport oneself.");
            }
            else
            {
                bool flag = client.ActiveCharacter.Asda2Inventory.UseTeleportScroll(true);
                AchievementProgressRecord progressRecord =
                    client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(90U);
                switch (++progressRecord.Counter)
                {
                    case 50:
                        client.ActiveCharacter.Map.CallDelayed(500,
                            (Action) (() => client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Lonely224)));
                        break;
                    case 100:
                        client.ActiveCharacter.Map.CallDelayed(500,
                            (Action) (() => client.ActiveCharacter.GetTitle(Asda2TitleId.Lonely224)));
                        break;
                }

                progressRecord.SaveAndFlush();
                if (flag)
                    Asda2CharacterHandler.SendSummonChar(client.ActiveCharacter, character);
                else
                    client.ActiveCharacter.SendSystemMessage("You have not summon scroll");
            }
        }

        [PacketHandler(RealmServerOpCode.UseShopItem)]
        public static void UseShopItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 6;
            short slot = packet.ReadInt16();
            packet.Position += 4;
            ++packet.Position;
            packet.Position += 2;
            int num = (int) packet.ReadInt16();
            packet.Position += 38;
            packet.Position += 200;
            uint parametr = 0;
            try
            {
                parametr = packet.ReadUInt32();
            }
            catch (EndOfStreamException ex)
            {
            }

            client.ActiveCharacter.AddMessage((Action) (() =>
                FunctionalItemsHandler.ProcessFunctionalItem(client, parametr, slot)));
        }

        private static void ProcessFunctionalItem(IRealmClient client, uint parametr, short slot)
        {
            Asda2Item item = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slot);
            if (item == null)
            {
                FunctionalItemsHandler.SendUpdateShopItemInfoResponse(client,
                    UseFunctionalItemError.FunctionalItemDoesNotExist, (Asda2Item) null);
            }
            else
            {
                UseFunctionalItemError status = UseFunctionalItemError.Ok;
                if ((int) item.RequiredLevel > client.ActiveCharacter.Level)
                    FunctionalItemsHandler.SendUpdateShopItemInfoResponse(client,
                        UseFunctionalItemError.YorLevelIsNotHightEnoght, item);
                else
                    ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                    {
                        try
                        {
                            switch (item.Category)
                            {
                                case Asda2ItemCategory.IncPAtk:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncMAtk:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncPDef:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncMdef:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncHp:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncMp:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncStr:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncSta:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncInt:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncSpi:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncDex:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncLuck:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncMoveSpeed:
                                    if (client.ActiveCharacter.LastTransportUsedTime +
                                        TimeSpan.FromMilliseconds(30000.0) > DateTime.Now)
                                    {
                                        status = UseFunctionalItemError.CoolingTimeRemain;
                                        break;
                                    }

                                    if (item.Record.IsSoulBound && item.Record.AuctionEndTime != DateTime.MinValue &&
                                        DateTime.Now > item.Record.AuctionEndTime)
                                    {
                                        Asda2InventoryHandler.ItemRemovedFromInventoryResponse(client.ActiveCharacter,
                                            item, DeleteOrSellItemStatus.Ok, 0);
                                        item.Destroy();
                                        client.ActiveCharacter.SendInfoMsg("Vehicle expired.");
                                        status = UseFunctionalItemError.TheDurationOfTheShopitemHaExprised;
                                        break;
                                    }

                                    if (item.Record.AuctionEndTime == DateTime.MinValue)
                                        item.Record.AuctionEndTime =
                                            DateTime.Now + TimeSpan.FromDays((double) item.AttackTime);
                                    client.ActiveCharacter.LastTransportUsedTime = DateTime.Now;
                                    item.IsSoulbound = true;
                                    client.ActiveCharacter.TransportItemId = item.ItemId;
                                    AchievementProgressRecord progressRecord1 =
                                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(85U);
                                    switch (++progressRecord1.Counter)
                                    {
                                        case 1:
                                            client.ActiveCharacter.Map.CallDelayed(500,
                                                (Action) (() =>
                                                    client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Rapid219)));
                                            break;
                                        case 1000:
                                            client.ActiveCharacter.Map.CallDelayed(500,
                                                (Action) (() =>
                                                    client.ActiveCharacter.GetTitle(Asda2TitleId.Rapid219)));
                                            break;
                                    }

                                    progressRecord1.SaveAndFlush();
                                    break;
                                case Asda2ItemCategory.IncExp:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncDropChance:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncDigChance:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncExpStackable:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.IncAtackSpeed:
                                    client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, false);
                                    FunctionalItemsHandler.SendShopItemUsedResponse(client, item.ItemId,
                                        (int) item.Template.AtackRange);
                                    break;
                                case Asda2ItemCategory.ExpandWarehouse:
                                    if (client.ActiveCharacter.Record.PremiumWarehouseBagsCount >= (byte) 8)
                                    {
                                        status = UseFunctionalItemError.WarehouseHasReachedMaxCapacity;
                                        break;
                                    }

                                    ++client.ActiveCharacter.Record.PremiumWarehouseBagsCount;
                                    FunctionalItemsHandler.SendWarehouseSlotsExpandedResponse(client, false);
                                    break;
                                case Asda2ItemCategory.ResetAllSkill:
                                    FunctionalItemsHandler.ResetSkills(client.ActiveCharacter);
                                    Asda2CharacterHandler.SendLearnedSkillsInfo(client.ActiveCharacter);
                                    AchievementProgressRecord progressRecord2 =
                                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(84U);
                                    switch (++progressRecord2.Counter)
                                    {
                                        case 3:
                                            client.ActiveCharacter.Map.CallDelayed(500,
                                                (Action) (() =>
                                                    client.ActiveCharacter.DiscoverTitle(Asda2TitleId
                                                        .Perfectionist218)));
                                            break;
                                        case 5:
                                            client.ActiveCharacter.Map.CallDelayed(500,
                                                (Action) (() =>
                                                    client.ActiveCharacter.GetTitle(Asda2TitleId.Perfectionist218)));
                                            break;
                                    }

                                    progressRecord2.SaveAndFlush();
                                    break;
                                case Asda2ItemCategory.ResetOneSkill:
                                    Spell spell1 =
                                        client.ActiveCharacter.Spells.First<Spell>(
                                            (Func<Spell, bool>) (s => (long) s.RealId == (long) parametr));
                                    if (spell1 != null)
                                    {
                                        int num1 = 0 + spell1.Cost;
                                        for (int index = spell1.Level - 1; index > 0; --index)
                                        {
                                            Spell spell2 =
                                                SpellHandler.Get((uint) spell1.RealId + (uint) (index * 1000));
                                            if (spell2 != null)
                                                num1 += spell2.Cost;
                                        }

                                        uint num2 = (uint) (num1 / 2);
                                        client.ActiveCharacter.Spells.Remove(spell1);
                                        client.ActiveCharacter.AddMoney(num2);
                                        Asda2CharacterHandler.SendPreResurectResponse(client.ActiveCharacter);
                                        FunctionalItemsHandler.SendSkillResetedResponse(client, spell1.RealId,
                                            (short) spell1.Level, num2);
                                        Asda2CharacterHandler.SendUpdateStatsOneResponse(client);
                                        Asda2CharacterHandler.SendUpdateStatsResponse(client);
                                        client.ActiveCharacter.SendMoneyUpdate();
                                        AchievementProgressRecord progressRecord3 =
                                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(84U);
                                        switch (++progressRecord3.Counter)
                                        {
                                            case 3:
                                                client.ActiveCharacter.Map.CallDelayed(500,
                                                    (Action) (() =>
                                                        client.ActiveCharacter.DiscoverTitle(Asda2TitleId
                                                            .Perfectionist218)));
                                                break;
                                            case 5:
                                                client.ActiveCharacter.Map.CallDelayed(500,
                                                    (Action) (() =>
                                                        client.ActiveCharacter.GetTitle(Asda2TitleId
                                                            .Perfectionist218)));
                                                break;
                                        }

                                        progressRecord3.SaveAndFlush();
                                        break;
                                    }

                                    status = UseFunctionalItemError.FailedToUse;
                                    client.ActiveCharacter.SendInfoMsg("Skill is not learned. Restart client.");
                                    break;
                                case Asda2ItemCategory.TeleportToCharacter:
                                    if (parametr >= 10U || client.ActiveCharacter.TeleportPoints[parametr] == null)
                                    {
                                        status = UseFunctionalItemError.FailedToUse;
                                        break;
                                    }

                                    Asda2TeleportingPointRecord teleportPoint =
                                        client.ActiveCharacter.TeleportPoints[parametr];
                                    client.ActiveCharacter.TeleportTo(teleportPoint.MapId,
                                        new Vector3((float) teleportPoint.X, (float) teleportPoint.Y));
                                    AchievementProgressRecord progressRecord4 =
                                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(89U);
                                    switch (++progressRecord4.Counter)
                                    {
                                        case 50:
                                            client.ActiveCharacter.Map.CallDelayed(500,
                                                (Action) (() =>
                                                    client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Traveler223)));
                                            break;
                                        case 100:
                                            client.ActiveCharacter.Map.CallDelayed(500,
                                                (Action) (() =>
                                                    client.ActiveCharacter.GetTitle(Asda2TitleId.Traveler223)));
                                            break;
                                    }

                                    progressRecord4.SaveAndFlush();
                                    break;
                                case Asda2ItemCategory.InstantRecover100PrcHP:
                                    if (client.ActiveCharacter.Last100PrcRecoveryUsed + 30000U >
                                        (uint) Environment.TickCount)
                                    {
                                        status = UseFunctionalItemError.CoolingTimeRemain;
                                        break;
                                    }

                                    client.ActiveCharacter.Last100PrcRecoveryUsed = (uint) Environment.TickCount;
                                    client.ActiveCharacter.HealPercent(100, (Unit) null, (SpellEffect) null);
                                    break;
                                case Asda2ItemCategory.InstantRecover100PrcHPandMP:
                                    if (client.ActiveCharacter.Last100PrcRecoveryUsed + 30000U <
                                        (uint) Environment.TickCount)
                                        status = UseFunctionalItemError.CoolingTimeRemain;
                                    client.ActiveCharacter.HealPercent(100, (Unit) null, (SpellEffect) null);
                                    client.ActiveCharacter.Power = client.ActiveCharacter.MaxPower;
                                    client.ActiveCharacter.Last100PrcRecoveryUsed = (uint) Environment.TickCount;
                                    break;
                                case Asda2ItemCategory.RecoverHp10TimesByPrcOver30Sec:
                                    PereodicAction pereodicAction = (PereodicAction) null;
                                    if (client.ActiveCharacter.PereodicActions.ContainsKey(Asda2PereodicActionType
                                        .HpRegenPrc))
                                        pereodicAction =
                                            client.ActiveCharacter.PereodicActions[Asda2PereodicActionType.HpRegenPrc];
                                    if (pereodicAction != null && pereodicAction.CallsNum >= 10 &&
                                        pereodicAction.Value >= item.Template.ValueOnUse)
                                    {
                                        status = UseFunctionalItemError.CoolingTimeRemain;
                                        break;
                                    }

                                    if (client.ActiveCharacter.PereodicActions.ContainsKey(Asda2PereodicActionType
                                        .HpRegenPrc))
                                        client.ActiveCharacter.PereodicActions.Remove(
                                            Asda2PereodicActionType.HpRegenPrc);
                                    client.ActiveCharacter.PereodicActions.Add(Asda2PereodicActionType.HpRegenPrc,
                                        new PereodicAction(client.ActiveCharacter, item.Template.ValueOnUse, 10, 3000,
                                            Asda2PereodicActionType.HpRegenPrc));
                                    break;
                                case Asda2ItemCategory.ShopBanner:
                                    if (client.ActiveCharacter.Level < 10)
                                    {
                                        status = UseFunctionalItemError.YorLevelIsNotHightEnoght;
                                        break;
                                    }

                                    FunctionalItemsHandler.SendPremiumLongBuffInfoResponse(client,
                                        (byte) client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, true),
                                        item.ItemId, (short) item.Template.PackageId);
                                    break;
                                case Asda2ItemCategory.OpenWarehouse:
                                    break;
                                case Asda2ItemCategory.PremiumPotions:
                                    FunctionalItemsHandler.SendPremiumLongBuffInfoResponse(client,
                                        (byte) client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, true),
                                        item.ItemId, (short) item.Template.PackageId);
                                    client.ActiveCharacter.Asda2WingsItemId = (short) item.ItemId;
                                    AchievementProgressRecord progressRecord5 =
                                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(86U);
                                    switch (++progressRecord5.Counter)
                                    {
                                        case 50:
                                            client.ActiveCharacter.Map.CallDelayed(500,
                                                (Action) (() =>
                                                    client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Winged220)));
                                            break;
                                        case 100:
                                            client.ActiveCharacter.Map.CallDelayed(500,
                                                (Action) (() =>
                                                    client.ActiveCharacter.GetTitle(Asda2TitleId.Winged220)));
                                            break;
                                    }

                                    progressRecord5.SaveAndFlush();
                                    break;
                                case Asda2ItemCategory.ExpandInventory:
                                    FunctionalItemsHandler.SendPremiumLongBuffInfoResponse(client,
                                        (byte) client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, true),
                                        item.ItemId, (short) item.Template.PackageId);
                                    AchievementProgressRecord progressRecord6 =
                                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(87U);
                                    switch (++progressRecord6.Counter)
                                    {
                                        case 3:
                                            client.ActiveCharacter.Map.CallDelayed(500,
                                                (Action) (() =>
                                                    client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Packrat221)));
                                            break;
                                        case 5:
                                            client.ActiveCharacter.Map.CallDelayed(500,
                                                (Action) (() =>
                                                    client.ActiveCharacter.GetTitle(Asda2TitleId.Packrat221)));
                                            break;
                                    }

                                    progressRecord6.SaveAndFlush();
                                    break;
                                case Asda2ItemCategory.PetNotEatingByDays:
                                    FunctionalItemsHandler.SendPremiumLongBuffInfoResponse(client,
                                        (byte) client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, true),
                                        item.ItemId, (short) item.Template.PackageId);
                                    client.ActiveCharacter.Map.CallDelayed(500,
                                        (Action) (() => client.ActiveCharacter.GetTitle(Asda2TitleId.Treat366)));
                                    break;
                                case Asda2ItemCategory.RemoveDeathPenaltiesByDays:
                                    FunctionalItemsHandler.SendPremiumLongBuffInfoResponse(client,
                                        (byte) client.ActiveCharacter.ApplyFunctionItemBuff(item.ItemId, true),
                                        item.ItemId, (short) item.Template.PackageId);
                                    break;
                                default:
                                    status = UseFunctionalItemError.NotAunctionalItem;
                                    break;
                            }
                        }
                        catch (AlreadyBuffedExcepton ex)
                        {
                            status = UseFunctionalItemError.AlreadyFeelingTheEffectOfSimilarSkillType;
                        }

                        if (status == UseFunctionalItemError.Ok && item.Category != Asda2ItemCategory.IncMoveSpeed)
                            item.ModAmount(-1);
                        FunctionalItemsHandler.SendUpdateShopItemInfoResponse(client, status, item);
                    }));
            }
        }

        public static void SendWingsInfoResponse(Character chr, IRealmClient reciever)
        {
            if (chr.Asda2WingsItemId == (short) -1)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WingsInfo))
            {
                packet.WriteInt32(chr.AccId);
                packet.WriteInt16(chr.Asda2WingsItemId);
                packet.WriteSkip(FunctionalItemsHandler.stub6);
                if (reciever == null)
                {
                    chr.SendPacketToArea(packet, true, true, Locale.Any, new float?());
                    chr.Send(packet, true);
                }
                else
                    reciever.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.UseChangeFactionItem)]
        public static void UseChangeFactionItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 5;
            short slotInq = packet.ReadInt16();
            Asda2Item shopShopItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq);
            if (shopShopItem == null || shopShopItem.Category != Asda2ItemCategory.ResetFaction ||
                client.ActiveCharacter.Asda2FactionId == (short) -1)
            {
                FunctionalItemsHandler.SendFactionResetedResponse(client, false, (Asda2Item) null);
            }
            else
            {
                --shopShopItem.Amount;
                client.ActiveCharacter.Asda2FactionId = (short) -1;
                FunctionalItemsHandler.SendFactionResetedResponse(client, true, shopShopItem);
            }
        }

        public static void SendFactionResetedResponse(IRealmClient client, bool ok, Asda2Item item)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FactionReseted))
            {
                packet.WriteByte(ok ? 1 : 0);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, true);
                client.Send(packet, false);
            }
        }

        public static void ResetSkills(Character chr)
        {
            chr.Spells.Clear();
            chr.Spells.AddDefaultSpells();
            FunctionalItemsHandler.SendPreResurectResponse(chr.Client);
            FunctionalItemsHandler.SendAllSkillsResetedInfoMsgResponse(chr.Client);
            FunctionalItemsHandler.SendAllSkillsResetedResponse(chr.Client);
        }

        public static void SendUpdateShopItemInfoResponse(IRealmClient client, UseFunctionalItemError status,
            Asda2Item funcItem = null)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpdateShopItemInfo))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, funcItem, false);
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                packet.WriteInt32(funcItem == null
                    ? 0
                    : (funcItem.Record == null
                        ? -1
                        : (int) (funcItem.Record.AuctionEndTime - DateTime.Now).TotalHours));
                packet.WriteInt16(15000);
                client.Send(packet, true);
                client.ActiveCharacter.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendWarehouseSlotsExpandedResponse(IRealmClient client, bool isAvatar)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WarehouseSlotsExpanded))
            {
                packet.WriteByte(isAvatar ? 3 : 2);
                packet.WriteInt16((isAvatar
                                      ? (int) client.ActiveCharacter.Record.PremiumAvatarWarehouseBagsCount
                                      : (int) client.ActiveCharacter.Record.PremiumWarehouseBagsCount + 1) * 30);
                client.Send(packet, true);
            }
        }

        public static void SendCancelCancelFunctionalItemResponse(IRealmClient client, short itemId)
        {
            if (client.ActiveCharacter == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CancelFunctionalItem))
            {
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt32(itemId);
                client.ActiveCharacter.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendSkillResetedResponse(IRealmClient client, short skillId, short skillLevel,
            uint goldRecived)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SkillReseted))
            {
                packet.WriteInt32(skillId);
                packet.WriteInt16(skillLevel);
                packet.WriteInt32(goldRecived);
                client.Send(packet, true);
            }
        }

        public static void SendPreResurectResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PreResurect))
                client.Send(packet, false);
        }

        public static void SendAllSkillsResetedInfoMsgResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AllSkillsResetedInfoMsg))
            {
                packet.WriteByte(1);
                packet.WriteInt16(client.ActiveCharacter.Spells.AvalibleSkillPoints);
                packet.WriteInt32(client.ActiveCharacter.Money);
                client.Send(packet, true);
            }
        }

        public static void SendAllSkillsResetedResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AllSkillsReseted))
                client.Send(packet, false);
        }

        public static void SendPremiumLongBuffInfoResponse(IRealmClient client, byte slot, int itemId,
            short itemCategory)
        {
            if (client.ActiveCharacter == null || client.ActiveCharacter.LongTimePremiumBuffs == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PremiumLongBuffInfo))
            {
                packet.WriteByte(slot);
                packet.WriteInt16(itemCategory);
                packet.WriteInt16(itemId);
                packet.WriteInt32(
                    (int) (client.ActiveCharacter.LongTimePremiumBuffs[(int) slot].EndsDate - DateTime.Now)
                    .TotalSeconds);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.ExpandPetBox)]
        public static void ExpandPetBoxRequest(IRealmClient client, RealmPacketIn packet)
        {
            ++packet.Position;
            short num = packet.ReadInt16();
            if (client.ActiveCharacter.Record.PetBoxEnchants > (byte) 14)
            {
                FunctionalItemsHandler.SendPetBoxExpandedResponse(client,
                    FunctionalItemsHandler.ExpandPetBoxStatus.Fail, num);
                client.ActiveCharacter.SendInfoMsg("Pet box expand for maximum 90 slots.");
            }
            else
            {
                Asda2Item shopShopItem = client.ActiveCharacter.Asda2Inventory.GetShopShopItem(num);
                if (shopShopItem == null)
                {
                    FunctionalItemsHandler.SendPetBoxExpandedResponse(client,
                        FunctionalItemsHandler.ExpandPetBoxStatus.Fail, num);
                    client.ActiveCharacter.SendInfoMsg("Item not found restart client.");
                }
                else if (shopShopItem.Category != Asda2ItemCategory.ExpandPetBoxBy6)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Tryes to expand pet box with wrong item.", 50);
                }
                else
                {
                    --shopShopItem.Amount;
                    ++client.ActiveCharacter.Record.PetBoxEnchants;
                    FunctionalItemsHandler.SendPetBoxExpandedResponse(client,
                        FunctionalItemsHandler.ExpandPetBoxStatus.Ok, num);
                }
            }
        }

        public static void SendPetBoxExpandedResponse(IRealmClient client,
            FunctionalItemsHandler.ExpandPetBoxStatus status, short slot)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PetBoxExpanded))
            {
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteByte((byte) status);
                packet.WriteByte(((int) client.ActiveCharacter.Record.PetBoxEnchants + 1) * 6);
                packet.WriteByte(1);
                packet.WriteInt16(slot);
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                client.Send(packet, true);
            }
        }

        public enum ExpandPetBoxStatus
        {
            Fail,
            Ok,
        }
    }
}