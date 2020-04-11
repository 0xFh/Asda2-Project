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
using WCell.RealmServer.Logs;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Asda2Fishing
{
    public static class Asda2FishingHandler
    {
        private static readonly byte[] unk10 = new byte[50]
        {
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 40,
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
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] stab21 = new byte[2]
        {
            byte.MaxValue,
            byte.MaxValue
        };

        private static readonly byte[] stab27 = new byte[1];

        private static readonly byte[] stub23 = new byte[41]
        {
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
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        public static void SendFishingLvlResponse(IRealmClient client)
        {
            if (!client.IsGameServerConnection)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpdateFishingLvl))
            {
                packet.WriteInt32(client.ActiveCharacter.FishingLevel);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.StartFishing)]
        public static void StartFishingRequest(IRealmClient client, RealmPacketIn packet)
        {
            short interator = packet.ReadInt16();
            if (client.ActiveCharacter.Asda2Inventory.Equipment[9] == null ||
                !client.ActiveCharacter.Asda2Inventory.Equipment[9].IsRod)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to fishing without rod.", 30);
                Asda2FishingHandler.SendFishingStartedResponse(client, Asda2StartFishStatus.YouHaveNoFishRod, (short) 0,
                    0U);
            }
            else if (client.ActiveCharacter.Asda2Inventory.Equipment[10] == null ||
                     !client.ActiveCharacter.Asda2Inventory.Equipment[10].Template.IsBait)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to fishing without bait.", 30);
                Asda2FishingHandler.SendFishingStartedResponse(client, Asda2StartFishStatus.YouHaveNoBait, (short) 0,
                    0U);
            }
            else if (!Asda2FishingMgr.FishingSpotsByMaps.ContainsKey((int) client.ActiveCharacter.MapId))
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to fishing in wrong place.", 30);
                Asda2FishingHandler.SendFishingStartedResponse(client, Asda2StartFishStatus.YouCantFishHere, (short) 0,
                    0U);
            }
            else
            {
                FishingSpot fishingSpot1 = (FishingSpot) null;
                foreach (FishingSpot fishingSpot2 in Asda2FishingMgr.FishingSpotsByMaps[
                    (int) client.ActiveCharacter.MapId])
                {
                    if ((double) client.ActiveCharacter.Asda2Position.GetDistance(fishingSpot2.Position) <=
                        (double) fishingSpot2.Radius)
                    {
                        fishingSpot1 = fishingSpot2;
                        break;
                    }
                }

                if (fishingSpot1 == null)
                    Asda2FishingHandler.SendFishingStartedResponse(client, Asda2StartFishStatus.YouCantFishHere,
                        (short) 0, 0U);
                else if (client.ActiveCharacter.FishingLevel < fishingSpot1.RequiredFishingLevel)
                    Asda2FishingHandler.SendFishingStartedResponse(client,
                        Asda2StartFishStatus.YourFishingLevelIsToLowToFishHereItMustBe, (short) 0,
                        (uint) fishingSpot1.RequiredFishingLevel);
                else if (client.ActiveCharacter.Asda2Inventory.FreeRegularSlotsCount < 1)
                {
                    Asda2FishingHandler.SendFishingStartedResponse(client, Asda2StartFishStatus.NotEnoughtSpace,
                        (short) 0, 0U);
                }
                else
                {
                    if (client.ActiveCharacter.CurrentFish != null)
                    {
                        if (client.ActiveCharacter.FishReadyTime > (uint) Environment.TickCount)
                        {
                            Asda2FishingHandler.SendFishingStartedResponse(client,
                                Asda2StartFishStatus.YouAreAlreadyFishing, (short) 0, 0U);
                            return;
                        }

                        client.ActiveCharacter.CurrentFish = (Fish) null;
                    }

                    Fish randomFish = fishingSpot1.GetRandomFish(
                        client.ActiveCharacter.Asda2Inventory.Equipment[10].Category == Asda2ItemCategory.BaitElite);
                    if (client.ActiveCharacter.Asda2Inventory.Equipment[10].Category == Asda2ItemCategory.BaitElite)
                    {
                        AchievementProgressRecord progressRecord1 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(91U);
                        AchievementProgressRecord progressRecord2 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(92U);
                        ++progressRecord1.Counter;
                        if (progressRecord1.Counter >= 1000U || progressRecord2.Counter >= 1000U)
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Automatic225);
                        progressRecord1.SaveAndFlush();
                    }

                    AchievementProgressRecord progressRecord =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(128U);
                    switch (++progressRecord.Counter)
                    {
                        case 1500:
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Fisherman305);
                            break;
                        case 3000:
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Fisherman305);
                            break;
                    }

                    progressRecord.SaveAndFlush();
                    client.ActiveCharacter.CancelAllActions();
                    client.ActiveCharacter.CurrentFish = randomFish;
                    client.ActiveCharacter.FishReadyTime = (uint) (Environment.TickCount + randomFish.FishingTime);
                    Asda2FishingHandler.SendSomeOneStartedFishingResponse(client.ActiveCharacter,
                        (int) randomFish.ItemTemplate.Id, (short) 0);
                    Asda2FishingHandler.SendFishingStartedResponse(client, Asda2StartFishStatus.Ok, interator,
                        randomFish.ItemTemplate.Id);
                }
            }
        }

        public static void SendFishingStartedResponse(IRealmClient client, Asda2StartFishStatus status,
            short interator = 0, uint fishId = 0)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FishingStarted))
            {
                packet.WriteByte((byte) status);
                packet.WriteUInt32(fishId);
                packet.WriteInt16(interator);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.EndFishing)]
        public static void EndFishingRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.CurrentFish == null)
            {
                Asda2FishingHandler.SendFishingEndedResponse(client, Asda2EndFishingStatus.YouAlreadyFishing, (short) 0,
                    (Asda2Item) null, (Asda2Item) null);
            }
            else
            {
                Fish ft = client.ActiveCharacter.CurrentFish;
                client.ActiveCharacter.CurrentFish = (Fish) null;
                if (client.ActiveCharacter.FishReadyTime > (uint) Environment.TickCount)
                    Asda2FishingHandler.SendFishingEndedResponse(client, Asda2EndFishingStatus.YouAlreadyFishing,
                        (short) 0, (Asda2Item) null, (Asda2Item) null);
                else if (client.ActiveCharacter.Asda2Inventory.Equipment[9] == null ||
                         !client.ActiveCharacter.Asda2Inventory.Equipment[9].IsRod)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to fishing without rod.", 30);
                    Asda2FishingHandler.SendFishingStartedResponse(client, Asda2StartFishStatus.YouHaveNoFishRod,
                        (short) 0, 0U);
                }
                else if (client.ActiveCharacter.Asda2Inventory.Equipment[10] == null ||
                         !client.ActiveCharacter.Asda2Inventory.Equipment[10].Template.IsBait)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to fishing without bait.", 30);
                    Asda2FishingHandler.SendFishingStartedResponse(client, Asda2StartFishStatus.YouHaveNoBait,
                        (short) 0, 0U);
                }
                else
                {
                    Asda2Item asda2Item = client.ActiveCharacter.Asda2Inventory.Equipment[9];
                    if (asda2Item.Category != Asda2ItemCategory.PremiumFishRod &&
                        CharacterFormulas.DecraseRodDurability())
                        asda2Item.DecreaseDurability((byte) 1, false);
                    Asda2Item bait = client.ActiveCharacter.Asda2Inventory.Equipment[10];
                    bait.ModAmount(-1);
                    if (bait.Category != Asda2ItemCategory.BaitElite && !ft.BaitIds.Contains(bait.ItemId))
                    {
                        Asda2FishingHandler.SendFishingEndedResponse(client, Asda2EndFishingStatus.Ok, (short) 0, bait,
                            (Asda2Item) null);
                    }
                    else
                    {
                        FishingSpot spot = (FishingSpot) null;
                        foreach (FishingSpot fishingSpot in Asda2FishingMgr.FishingSpotsByMaps[
                            (int) client.ActiveCharacter.MapId])
                        {
                            if ((double) client.ActiveCharacter.Asda2Position.GetDistance(fishingSpot.Position) <=
                                (double) ((int) fishingSpot.Radius / 2))
                            {
                                spot = fishingSpot;
                                break;
                            }
                        }

                        if (spot == null)
                        {
                            client.ActiveCharacter.YouAreFuckingCheater("Trying to fishing in wrong place.", 30);
                            Asda2FishingHandler.SendFishingStartedResponse(client, Asda2StartFishStatus.YouCantFishHere,
                                (short) 0, 0U);
                        }
                        else if (client.ActiveCharacter.FishingLevel < spot.RequiredFishingLevel)
                            Asda2FishingHandler.SendFishingStartedResponse(client,
                                Asda2StartFishStatus.YourFishingLevelIsToLowToFishHereItMustBe, (short) 0,
                                (uint) spot.RequiredFishingLevel);
                        else if (client.ActiveCharacter.Asda2Inventory.FreeRegularSlotsCount < 1)
                            Asda2FishingHandler.SendFishingStartedResponse(client, Asda2StartFishStatus.NotEnoughtSpace,
                                (short) 0, 0U);
                        else if (!CharacterFormulas.CalcFishingSuccess(client.ActiveCharacter.FishingLevel,
                            spot.RequiredFishingLevel, client.ActiveCharacter.Asda2Luck))
                        {
                            Asda2FishingHandler.SendFishingEndedResponse(client, Asda2EndFishingStatus.Ok, (short) 0,
                                bait, (Asda2Item) null);
                        }
                        else
                        {
                            short fishSize = (short) Utility.Random((int) ft.MinLength, (int) ft.MaxLength);
                            fishSize += (short) ((int) fishSize *
                                                 client.ActiveCharacter.GetIntMod(StatModifierInt.Asda2FishingGauge) /
                                                 100 * (client.ActiveCharacter.GodMode ? 10 : 1));
                            if (CharacterFormulas.CalcFishingLevelRised(client.ActiveCharacter.FishingLevel) &&
                                client.ActiveCharacter.Record.FishingLevel < spot.RequiredFishingLevel + 80)
                                ++client.ActiveCharacter.Record.FishingLevel;
                            client.ActiveCharacter.GuildPoints += CharacterFormulas.FishingGuildPoints;
                            Asda2Item item = (Asda2Item) null;
                            Asda2InventoryError err = client.ActiveCharacter.Asda2Inventory.TryAdd(
                                (int) ft.ItemTemplate.ItemId, 1, true, ref item, new Asda2InventoryType?(),
                                (Asda2Item) null);
                            Log.Create(Log.Types.ItemOperations, LogSourceType.Character,
                                    client.ActiveCharacter.EntryId).AddAttribute("source", 0.0, "fishing")
                                .AddItemAttributes(item, "").Write();
                            client.ActiveCharacter.Map.AddMessage((Action) (() =>
                            {
                                if (err != Asda2InventoryError.Ok)
                                {
                                    Asda2FishingHandler.SendFishingEndedResponse(client, Asda2EndFishingStatus.NoSpace,
                                        (short) 0, bait, (Asda2Item) null);
                                }
                                else
                                {
                                    foreach (Asda2FishingBook asda2FishingBook in client.ActiveCharacter
                                        .RegisteredFishingBooks.Values)
                                        asda2FishingBook.OnCatchFish(item.ItemId, fishSize);
                                    client.ActiveCharacter.GainXp(
                                        CharacterFormulas.CalcExpForFishing(client.ActiveCharacter.Level,
                                            client.ActiveCharacter.FishingLevel, item.Template.Quality,
                                            spot.RequiredFishingLevel, fishSize), "fishing", false);
                                    Asda2FishingHandler.SendFishingEndedResponse(client, Asda2EndFishingStatus.Ok,
                                        fishSize, bait, item);
                                    Asda2FishingHandler.SendSomeOneStartedFishingResponse(client.ActiveCharacter,
                                        (int) ft.ItemTemplate.Id, fishSize);
                                }
                            }));
                        }
                    }
                }
            }
        }

        public static void SendFishingEndedResponse(IRealmClient client, Asda2EndFishingStatus status, short fishSize,
            Asda2Item bait = null, Asda2Item fish = null)
        {
            AchievementProgressRecord progressRecord1 =
                client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(130U);
            switch (++progressRecord1.Counter)
            {
                case 150:
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Alpeon307);
                    break;
                case 200:
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Inferos308);
                    break;
                case 250:
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Aqueon309);
                    break;
            }

            progressRecord1.SaveAndFlush();
            if (fish != null)
            {
                switch (fish.ItemId)
                {
                    case 31715:
                        AchievementProgressRecord progressRecord2 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(131U);
                        switch (++progressRecord2.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Carp310);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Carp310);
                                break;
                        }

                        progressRecord2.SaveAndFlush();
                        break;
                    case 31716:
                        AchievementProgressRecord progressRecord3 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(140U);
                        switch (++progressRecord3.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Copper320);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Copper320);
                                break;
                        }

                        progressRecord3.SaveAndFlush();
                        break;
                    case 31718:
                        AchievementProgressRecord progressRecord4 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(148U);
                        switch (++progressRecord4.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Golden328);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Golden328);
                                break;
                        }

                        progressRecord4.SaveAndFlush();
                        break;
                    case 31719:
                        AchievementProgressRecord progressRecord5 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(156U);
                        switch (++progressRecord5.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Rainbow336);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Rainbow336);
                                break;
                        }

                        progressRecord5.SaveAndFlush();
                        break;
                    case 31720:
                        AchievementProgressRecord progressRecord6 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(132U);
                        switch (++progressRecord6.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Koi311);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Koi311);
                                break;
                        }

                        progressRecord6.SaveAndFlush();
                        break;
                    case 31721:
                        AchievementProgressRecord progressRecord7 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(141U);
                        switch (++progressRecord7.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Clay321);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Clay321);
                                break;
                        }

                        progressRecord7.SaveAndFlush();
                        break;
                    case 31723:
                        AchievementProgressRecord progressRecord8 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(149U);
                        switch (++progressRecord8.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Thief329);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Thief329);
                                break;
                        }

                        progressRecord8.SaveAndFlush();
                        break;
                    case 31724:
                        AchievementProgressRecord progressRecord9 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(157U);
                        switch (++progressRecord9.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Glowing337);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Glowing337);
                                break;
                        }

                        progressRecord9.SaveAndFlush();
                        break;
                    case 31725:
                        AchievementProgressRecord progressRecord10 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(133U);
                        switch (++progressRecord10.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Goldfish312);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Goldfish312);
                                break;
                        }

                        progressRecord10.SaveAndFlush();
                        break;
                    case 31726:
                        AchievementProgressRecord progressRecord11 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(142U);
                        switch (++progressRecord11.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Metallic322);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Metallic322);
                                break;
                        }

                        progressRecord11.SaveAndFlush();
                        break;
                    case 31728:
                        AchievementProgressRecord progressRecord12 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(150U);
                        switch (++progressRecord12.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Spotted330);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Spotted330);
                                break;
                        }

                        progressRecord12.SaveAndFlush();
                        break;
                    case 31729:
                        AchievementProgressRecord progressRecord13 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(158U);
                        switch (++progressRecord13.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Ruby338);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Ruby338);
                                break;
                        }

                        progressRecord13.SaveAndFlush();
                        break;
                    case 31730:
                        AchievementProgressRecord progressRecord14 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(134U);
                        switch (++progressRecord14.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Eel313);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Eel313);
                                break;
                        }

                        progressRecord14.SaveAndFlush();
                        break;
                    case 31731:
                        AchievementProgressRecord progressRecord15 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(143U);
                        switch (++progressRecord15.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Sharp323);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Sharp323);
                                break;
                        }

                        progressRecord15.SaveAndFlush();
                        break;
                    case 31733:
                        AchievementProgressRecord progressRecord16 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(151U);
                        switch (++progressRecord16.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Armored331);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Armored331);
                                break;
                        }

                        progressRecord16.SaveAndFlush();
                        break;
                    case 31734:
                        AchievementProgressRecord progressRecord17 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(159U);
                        switch (++progressRecord17.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Blast339);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Blast339);
                                break;
                        }

                        progressRecord17.SaveAndFlush();
                        break;
                    case 31735:
                        AchievementProgressRecord progressRecord18 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(135U);
                        switch (++progressRecord18.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Catfish314);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Catfish314);
                                break;
                        }

                        progressRecord18.SaveAndFlush();
                        break;
                    case 31736:
                        AchievementProgressRecord progressRecord19 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(144U);
                        switch (++progressRecord19.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Stone324);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Stone324);
                                break;
                        }

                        progressRecord19.SaveAndFlush();
                        break;
                    case 31738:
                        AchievementProgressRecord progressRecord20 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(152U);
                        switch (++progressRecord20.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Stray332);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Stray332);
                                break;
                        }

                        progressRecord20.SaveAndFlush();
                        break;
                    case 31739:
                        AchievementProgressRecord progressRecord21 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(160U);
                        switch (++progressRecord21.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Gravity340);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Gravity340);
                                break;
                        }

                        progressRecord21.SaveAndFlush();
                        break;
                    case 31740:
                        AchievementProgressRecord progressRecord22 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(136U);
                        switch (++progressRecord22.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Mackerel315);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Mackerel315);
                                break;
                        }

                        progressRecord22.SaveAndFlush();
                        break;
                    case 31741:
                        AchievementProgressRecord progressRecord23 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(145U);
                        switch (++progressRecord23.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Angry325);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Angry325);
                                break;
                        }

                        progressRecord23.SaveAndFlush();
                        break;
                    case 31743:
                        AchievementProgressRecord progressRecord24 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(153U);
                        switch (++progressRecord24.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.School333);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.School333);
                                break;
                        }

                        progressRecord24.SaveAndFlush();
                        break;
                    case 31744:
                        AchievementProgressRecord progressRecord25 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(161U);
                        switch (++progressRecord25.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Powerful341);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Powerful341);
                                break;
                        }

                        progressRecord25.SaveAndFlush();
                        break;
                    case 31745:
                        AchievementProgressRecord progressRecord26 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(137U);
                        switch (++progressRecord26.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Tuna316);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Tuna316);
                                break;
                        }

                        progressRecord26.SaveAndFlush();
                        break;
                    case 31746:
                        AchievementProgressRecord progressRecord27 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(146U);
                        switch (++progressRecord27.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Iron326);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Iron326);
                                break;
                        }

                        progressRecord27.SaveAndFlush();
                        break;
                    case 31748:
                        AchievementProgressRecord progressRecord28 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(154U);
                        switch (++progressRecord28.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Tiger334);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Tiger334);
                                break;
                        }

                        progressRecord28.SaveAndFlush();
                        break;
                    case 31749:
                        AchievementProgressRecord progressRecord29 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(162U);
                        switch (++progressRecord29.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Millenium342);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Millenium342);
                                break;
                        }

                        progressRecord29.SaveAndFlush();
                        break;
                    case 31750:
                        AchievementProgressRecord progressRecord30 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(138U);
                        switch (++progressRecord30.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Cod317);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Cod317);
                                break;
                        }

                        progressRecord30.SaveAndFlush();
                        break;
                    case 31751:
                        AchievementProgressRecord progressRecord31 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(147U);
                        switch (++progressRecord31.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Wooden327);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Wooden327);
                                break;
                        }

                        progressRecord31.SaveAndFlush();
                        break;
                    case 31753:
                        AchievementProgressRecord progressRecord32 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(155U);
                        switch (++progressRecord32.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Muscular335);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Muscular335);
                                break;
                        }

                        progressRecord32.SaveAndFlush();
                        break;
                    case 31754:
                        AchievementProgressRecord progressRecord33 =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(163U);
                        switch (++progressRecord33.Counter)
                        {
                            case 50:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Emerald343);
                                break;
                            case 100:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Emerald343);
                                break;
                        }

                        progressRecord33.SaveAndFlush();
                        break;
                }

                if (fish.ItemId >= 31755 && 31762 <= fish.ItemId)
                {
                    AchievementProgressRecord progressRecord34 =
                        client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(139U);
                    switch (++progressRecord34.Counter)
                    {
                        case 400:
                            client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Black318);
                            break;
                        case 800:
                            client.ActiveCharacter.GetTitle(Asda2TitleId.Black318);
                            break;
                    }

                    progressRecord34.SaveAndFlush();
                }

                if (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Carp310) &&
                    client.ActiveCharacter.isTitleGetted(Asda2TitleId.Koi311) &&
                    (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Goldfish312) &&
                     client.ActiveCharacter.isTitleGetted(Asda2TitleId.Eel313)) &&
                    (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Catfish314) &&
                     client.ActiveCharacter.isTitleGetted(Asda2TitleId.Mackerel315) &&
                     (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Tuna316) &&
                      client.ActiveCharacter.isTitleGetted(Asda2TitleId.Cod317))))
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Fish319);
            }

            Asda2Item[] asda2ItemArray = new Asda2Item[2]
            {
                bait,
                fish
            };
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FishingEnded))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(client.ActiveCharacter.Record.FishingLevel);
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                packet.WriteInt16(fishSize);
                for (int index = 0; index < 2; ++index)
                {
                    Asda2Item asda2Item = asda2ItemArray[index];
                    packet.WriteInt32(asda2Item == null ? 0 : asda2Item.ItemId);
                    packet.WriteByte(asda2Item == null ? (byte) 0 : (byte) asda2Item.InventoryType);
                    packet.WriteInt16(asda2Item == null ? 0 : (int) asda2Item.Slot);
                    packet.WriteInt16(asda2Item == null ? -1 : (asda2Item.IsDeleted ? -1 : 0));
                    packet.WriteInt32(asda2Item == null ? 0 : asda2Item.Amount);
                    packet.WriteByte(asda2Item == null ? 0 : (int) asda2Item.Durability);
                    packet.WriteInt16(asda2Item == null ? 0 : (int) asda2Item.Weight);
                    packet.WriteInt16(-1);
                    packet.WriteInt16(-1);
                    packet.WriteInt16(-1);
                    packet.WriteInt16(-1);
                    packet.WriteInt16(0);
                    packet.WriteInt16(0);
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
                }

                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.RegisterFishingBook)]
        public static void RegisterFishingBookRequest(IRealmClient client, RealmPacketIn packet)
        {
            short slotInq = packet.ReadInt16();
            Asda2Item itm = client.ActiveCharacter.Asda2Inventory.GetRegularItem(slotInq);
            if (itm == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Tryes to register not existing fishing book", 10);
                Asda2FishingHandler.SendFishingBookRegisteredResponse(client, RegisterFishingBookStatus.Fail,
                    (Asda2Item) null, (Asda2FishingBook) null);
            }
            else if (itm.Category != Asda2ItemCategory.FishingBook)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Tryes to register not a fishing book", 70);
                Asda2FishingHandler.SendFishingBookRegisteredResponse(client, RegisterFishingBookStatus.Fail,
                    (Asda2Item) null, (Asda2FishingBook) null);
            }
            else if (client.ActiveCharacter.RegisteredFishingBooks.Values.FirstOrDefault<Asda2FishingBook>(
                         (Func<Asda2FishingBook, bool>) (f => f.BookId == itm.ItemId)) != null)
                Asda2FishingHandler.SendFishingBookRegisteredResponse(client,
                    RegisterFishingBookStatus.AlreadyRegistered, (Asda2Item) null, (Asda2FishingBook) null);
            else
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                {
                    Asda2FishingBook asda2FishingBook = new Asda2FishingBook(itm.ItemId, client.ActiveCharacter,
                        (byte) client.ActiveCharacter.RegisteredFishingBooks.Count);
                    asda2FishingBook.CreateLater();
                    client.ActiveCharacter.RegisteredFishingBooks.Add(asda2FishingBook.Num, asda2FishingBook);
                    itm.Destroy();
                    Asda2FishingHandler.SendFishingBookRegisteredResponse(client, RegisterFishingBookStatus.Ok, itm,
                        asda2FishingBook);
                }));
        }

        public static void SendFishingBookRegisteredResponse(IRealmClient client, RegisterFishingBookStatus status,
            Asda2Item book, Asda2FishingBook fishingBook)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FishingBookRegistered))
            {
                packet.WriteByte((byte) status);
                if (fishingBook != null)
                {
                    packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                    packet.WriteInt32(book == null ? 0 : book.ItemId);
                    packet.WriteByte(book == null ? (byte) 0 : (byte) book.InventoryType);
                    packet.WriteInt16(book == null ? 0 : (int) book.Slot);
                    packet.WriteSkip(Asda2FishingHandler.unk10);
                    packet.WriteByte(fishingBook.Num);
                    packet.WriteInt32(0);
                    for (int index = 0; index < 30; ++index)
                        packet.WriteInt32(fishingBook.FishIds[0]);
                    for (int index = 0; index < 30; ++index)
                        packet.WriteInt16(fishingBook.Amounts[index]);
                    for (int index = 0; index < 30; ++index)
                        packet.WriteInt16(fishingBook.MaxLength[index]);
                    for (int index = 0; index < 30; ++index)
                        packet.WriteInt16(fishingBook.MinLengths[index]);
                }

                client.Send(packet, true);
            }
        }

        public static void SendFishingBooksInfoResponse(IRealmClient client, Asda2FishingBook book)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FishingBooksInfo))
            {
                packet.WriteByte(book.Num);
                packet.WriteInt32(0);
                for (int index = 0; index < 30; ++index)
                    packet.WriteInt32(book.FishIds[index]);
                for (int index = 0; index < 30; ++index)
                    packet.WriteInt16(book.Amounts[index]);
                for (int index = 0; index < 30; ++index)
                    packet.WriteInt16(book.MaxLength[index]);
                for (int index = 0; index < 30; ++index)
                    packet.WriteInt16(book.MinLengths[index]);
                client.Send(packet, true);
            }
        }

        public static void SendFishingBookListEndedResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FishingBookListEnded))
                client.Send(packet, true);
        }

        [PacketHandler(RealmServerOpCode.ReciveFishingBookReward)]
        public static void ReciveFishingBookRewardRequest(IRealmClient client, RealmPacketIn packet)
        {
            byte bookNum = (byte) packet.ReadInt32();
            int itemId = packet.ReadInt32();
            if (!client.ActiveCharacter.RegisteredFishingBooks.ContainsKey(bookNum))
            {
                client.ActiveCharacter.YouAreFuckingCheater(
                    "Tryes to recive fish book reward from not registered book.", 80);
            }
            else
            {
                Asda2FishingBook book = client.ActiveCharacter.RegisteredFishingBooks[bookNum];
                if (!((IEnumerable<int>) book.Template.Rewards).Contains<int>(itemId))
                {
                    client.ActiveCharacter.YouAreFuckingCheater(
                        "Tryes to recive fish book reward that this book not contained.", 80);
                }
                else
                {
                    int index = Array.IndexOf<int>(book.Template.Rewards, itemId);
                    Asda2Item rewItem = (Asda2Item) null;
                    Asda2InventoryError err = client.ActiveCharacter.Asda2Inventory.TryAdd(itemId,
                        book.Template.RewardAmounts[index], true, ref rewItem, new Asda2InventoryType?(),
                        (Asda2Item) null);
                    Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                        .AddAttribute("source", 0.0, "fishing_book").AddItemAttributes(rewItem, "").Write();
                    client.ActiveCharacter.Map.AddMessage((Action) (() =>
                    {
                        if (!book.IsComleted)
                            client.ActiveCharacter.YouAreFuckingCheater(
                                "Tryes to recive fish book reward from not completed book.", 150);
                        else if (err != Asda2InventoryError.Ok)
                        {
                            client.ActiveCharacter.SendInfoMsg("Can't recive reward cause " + (object) err);
                        }
                        else
                        {
                            book.ResetBook();
                            Asda2FishingHandler.SendFishingBookRewardRecivedResponse(client,
                                GetFishingBookRewardStatus.Ok, rewItem, bookNum);
                        }
                    }));
                }
            }
        }

        public static void SendFishingBookRewardRecivedResponse(IRealmClient client, GetFishingBookRewardStatus status,
            Asda2Item rewardItem, byte fishingBookNum)
        {
            AchievementProgressRecord progressRecord =
                client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(129U);
            switch (++progressRecord.Counter)
            {
                case 250:
                    client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Angler306);
                    break;
                case 500:
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Angler306);
                    break;
            }

            progressRecord.SaveAndFlush();
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.FishingBookRewardRecived))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(fishingBookNum);
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                packet.WriteInt32(rewardItem == null ? 0 : rewardItem.ItemId);
                packet.WriteByte(rewardItem == null ? (byte) 0 : (byte) rewardItem.InventoryType);
                packet.WriteInt16(rewardItem == null ? 0 : (int) rewardItem.Slot);
                packet.WriteSkip(Asda2FishingHandler.stab21);
                packet.WriteInt32(rewardItem == null ? 0 : rewardItem.Amount);
                packet.WriteSkip(Asda2FishingHandler.stab27);
                packet.WriteInt16(rewardItem == null ? 0 : (int) rewardItem.Weight);
                packet.WriteSkip(Asda2FishingHandler.stub23);
                client.Send(packet, true);
            }
        }

        public static void SendSomeOneStartedFishingResponse(Character fisher, int fishId, short fishSize)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SomeOneStartedFishing))
            {
                packet.WriteByte(1);
                packet.WriteInt32(fisher.AccId);
                packet.WriteInt32(fishId);
                packet.WriteInt16(fishSize);
                fisher.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }
    }
}