using System;
using Castle.ActiveRecord;
using NHibernate.SqlCommand;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core.Database;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Network;
using WCell.Util.Data;
using System.Linq;

namespace WCell.RealmServer.Asda2Fishing
{
    public static class Asda2FishingHandler
    {
        public static void SendFishingLvlResponse(IRealmClient client)
        {
            if (!client.IsGameServerConnection)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.UpdateFishingLvl))//6164
            {
                packet.WriteInt32(client.ActiveCharacter.FishingLevel);//{fishingLvl}default value : 0 Len : 4
                client.Send(packet, addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.StartFishing)]//6167
        public static void StartFishingRequest(IRealmClient client, RealmPacketIn packet)
        {
            var interator = packet.ReadInt16();//default : 5Len : 2
            if (client.ActiveCharacter.Asda2Inventory.Equipment[9] == null || !client.ActiveCharacter.Asda2Inventory.Equipment[9].IsRod)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to fishing without rod.", 30);
                SendFishingStartedResponse(client, Asda2StartFishStatus.YouHaveNoFishRod);
                return;
            }
            if (client.ActiveCharacter.Asda2Inventory.Equipment[10] == null || !client.ActiveCharacter.Asda2Inventory.Equipment[10].Template.IsBait)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to fishing without bait.", 30);
                SendFishingStartedResponse(client, Asda2StartFishStatus.YouHaveNoBait);
                return;
            }
            if (!Asda2FishingMgr.FishingSpotsByMaps.ContainsKey((int)client.ActiveCharacter.MapId))
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to fishing in wrong place.", 30);
                SendFishingStartedResponse(client, Asda2StartFishStatus.YouCantFishHere);
                return;
            }
            FishingSpot spot = null;
            foreach (var fishingSpot in Asda2FishingMgr.FishingSpotsByMaps[(int)client.ActiveCharacter.MapId])
            {
                if (client.ActiveCharacter.Asda2Position.GetDistance(fishingSpot.Position) > fishingSpot.Radius)
                    continue;
                spot = fishingSpot;
                break;
            }
            if (spot == null)
            {
                SendFishingStartedResponse(client, Asda2StartFishStatus.YouCantFishHere);
                return;
            }
            if (client.ActiveCharacter.FishingLevel < spot.RequiredFishingLevel)
            {
                SendFishingStartedResponse(client, Asda2StartFishStatus.YourFishingLevelIsToLowToFishHereItMustBe, 0, (uint)spot.RequiredFishingLevel);
                return;
            }
            if (client.ActiveCharacter.Asda2Inventory.FreeRegularSlotsCount < 1)
            {
                SendFishingStartedResponse(client, Asda2StartFishStatus.NotEnoughtSpace);
                return;
            }
            if (client.ActiveCharacter.CurrentFish != null)
            {
                if (client.ActiveCharacter.FishReadyTime > (uint)Environment.TickCount)
                {
                    SendFishingStartedResponse(client, Asda2StartFishStatus.YouAreAlreadyFishing);
                    return;
                }
                else
                {
                    client.ActiveCharacter.CurrentFish = null;
                }
            }
            var fish = spot.GetRandomFish(client.ActiveCharacter.Asda2Inventory.Equipment[10].Category ==
                                          Asda2ItemCategory.BaitElite);
            client.ActiveCharacter.CancelAllActions();
            client.ActiveCharacter.CurrentFish = fish;
            client.ActiveCharacter.FishReadyTime = (uint)(Environment.TickCount + fish.FishingTime);
            SendSomeOneStartedFishingResponse(client.ActiveCharacter, (int)fish.ItemTemplate.Id, 0);
            SendFishingStartedResponse(client, Asda2StartFishStatus.Ok, interator, fish.ItemTemplate.Id);
        }

        public static void SendFishingStartedResponse(IRealmClient client, Asda2StartFishStatus status, Int16 interator = 0, uint fishId = 0)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.FishingStarted))//6168
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteUInt32(fishId);//{fishId}default value : 31720 Len : 4
                packet.WriteInt16(interator);//{interator}default value : 5 Len : 2
                client.Send(packet, addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.EndFishing)]//6171
        public static void EndFishingRequest(IRealmClient client, RealmPacketIn packet)
        {

            if (client.ActiveCharacter.CurrentFish == null)
            {
                SendFishingEndedResponse(client, Asda2EndFishingStatus.YouAlreadyFishing, 0);
                return;
            }
            var ft = client.ActiveCharacter.CurrentFish;

            client.ActiveCharacter.CurrentFish = null;
            if (client.ActiveCharacter.FishReadyTime > (uint)Environment.TickCount)
            {
                SendFishingEndedResponse(client, Asda2EndFishingStatus.YouAlreadyFishing, 0);
                return;
            }
            if (client.ActiveCharacter.Asda2Inventory.Equipment[9] == null || !client.ActiveCharacter.Asda2Inventory.Equipment[9].IsRod)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to fishing without rod.", 30);
                SendFishingStartedResponse(client, Asda2StartFishStatus.YouHaveNoFishRod);
                return;
            }
            if (client.ActiveCharacter.Asda2Inventory.Equipment[10] == null || !client.ActiveCharacter.Asda2Inventory.Equipment[10].Template.IsBait)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to fishing without bait.", 30);
                SendFishingStartedResponse(client, Asda2StartFishStatus.YouHaveNoBait);
                return;
            }
            var rod = client.ActiveCharacter.Asda2Inventory.Equipment[9];
            if (rod.Category != Asda2ItemCategory.PremiumFishRod && CharacterFormulas.DecraseRodDurability())
            {
                rod.DecreaseDurability(1);
            }
            var bait = client.ActiveCharacter.Asda2Inventory.Equipment[10];
            bait.ModAmount(-1);
            if (bait.Category != Asda2ItemCategory.BaitElite && !ft.BaitIds.Contains(bait.ItemId))
            {
                SendFishingEndedResponse(client, Asda2EndFishingStatus.Ok, 0, bait);
                return;
            }
            FishingSpot spot = null;
            foreach (var fishingSpot in Asda2FishingMgr.FishingSpotsByMaps[(int)client.ActiveCharacter.MapId])
            {
                if (client.ActiveCharacter.Asda2Position.GetDistance(fishingSpot.Position) > fishingSpot.Radius / 2)
                    continue;
                spot = fishingSpot;
                break;
            }
            if (spot == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to fishing in wrong place.", 30);
                SendFishingStartedResponse(client, Asda2StartFishStatus.YouCantFishHere);
                return;
            }
            if (client.ActiveCharacter.FishingLevel < spot.RequiredFishingLevel)
            {
                SendFishingStartedResponse(client, Asda2StartFishStatus.YourFishingLevelIsToLowToFishHereItMustBe, 0, (uint)spot.RequiredFishingLevel);
                return;
            }
            if (client.ActiveCharacter.Asda2Inventory.FreeRegularSlotsCount < 1)
            {
                SendFishingStartedResponse(client, Asda2StartFishStatus.NotEnoughtSpace);
                return;
            }
            var success = CharacterFormulas.CalcFishingSuccess(client.ActiveCharacter.FishingLevel, spot.RequiredFishingLevel, client.ActiveCharacter.Asda2Luck);
            if (!success)
            {
                SendFishingEndedResponse(client, Asda2EndFishingStatus.Ok, 0, bait, null);
                return;
            }
            var fishSize =
                (short)
                Util.Utility.Random(ft.MinLength,
                                    ft.MaxLength);
            fishSize = (short)(fishSize + fishSize * client.ActiveCharacter.GetIntMod(StatModifierInt.Asda2FishingGauge) / 100 * (client.ActiveCharacter.GodMode ? 10 : 1));
            if (CharacterFormulas.CalcFishingLevelRised(client.ActiveCharacter.FishingLevel) && client.ActiveCharacter.Record.FishingLevel < spot.RequiredFishingLevel + 80)
                client.ActiveCharacter.Record.FishingLevel++;

            client.ActiveCharacter.GuildPoints += CharacterFormulas.FishingGuildPoints;

            Asda2Item item = null;
            var err = client.ActiveCharacter.Asda2Inventory.TryAdd((int)ft.ItemTemplate.ItemId, 1, true, ref item);
            var resLog = Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                                                 .AddAttribute("source", 0, "fishing")
                                                 .AddItemAttributes(item)
                                                 .Write();
            client.ActiveCharacter.Map.AddMessage(() =>
            {
                if (err != Asda2InventoryError.Ok)
                {
                    SendFishingEndedResponse(client, Asda2EndFishingStatus.NoSpace, 0, bait, null);
                    return;
                }
                foreach (var registeredFishingBook in client.ActiveCharacter.RegisteredFishingBooks.Values)
                {
                    registeredFishingBook.OnCatchFish(item.ItemId, fishSize);
                }
                client.ActiveCharacter.GainXp(
                    CharacterFormulas.CalcExpForFishing(client.ActiveCharacter.Level,
                                                        client.ActiveCharacter.FishingLevel, item.Template.Quality,
                                                        spot.RequiredFishingLevel, fishSize),
                    "fishing");
                SendFishingEndedResponse(client, Asda2EndFishingStatus.Ok, fishSize, bait, item);
                SendSomeOneStartedFishingResponse(client.ActiveCharacter, (int)ft.ItemTemplate.Id, fishSize);
                Asda2TitleChecker.OnSuccessFishing(client.ActiveCharacter, (int)ft.ItemTemplate.Id, fishSize);
            });

        }
        public static void SendFishingEndedResponse(IRealmClient client, Asda2EndFishingStatus status, short fishSize, Asda2Item bait = null, Asda2Item fish = null)
        {
            var itms = new Asda2Item[2];
            itms[0] = bait;
            itms[1] = fish;
            using (var packet = new RealmPacketOut(RealmServerOpCode.FishingEnded))//6172
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt16(client.ActiveCharacter.Record.FishingLevel);//{fishingSkill}default value : 8 Len : 2
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);//{weight}default value : 4021 Len : 2
                packet.WriteInt16(fishSize);//{fishSize}default value : 89 Len : 2

                for (int i = 0; i < 2; i++)
                {
                    var itm = itms[i];
                    packet.WriteInt32(itm == null ? 0 : itm.ItemId);//{itemId0}default value : 31812 Len : 4
                    packet.WriteByte((byte)(itm == null ? 0 : itm.InventoryType));//{invNum}default value : 3 Len : 1
                    packet.WriteInt16(itm == null ? 0 : itm.Slot);//{slot0}default value : 10 Len : 2
                    packet.WriteInt16(itm == null ? -1 : itm.IsDeleted ? -1 : 0);//value name : unk11 default value : -1Len : 2
                    packet.WriteInt32(itm == null ? 0 : itm.Amount);//{amount}default value : 43 Len : 4
                    packet.WriteByte(itm == null ? 0 : itm.Durability);//{durability0}default value : 0 Len : 1
                    packet.WriteInt16(itm == null ? 0 : itm.Weight);//{weight0}default value : 50 Len : 2
                    packet.WriteInt16(-1);//{soul1Id0}default value : -1 Len : 2
                    packet.WriteInt16(-1);//{soul2Id0}default value : -1 Len : 2
                    packet.WriteInt16(-1);//{soul3Id0}default value : -1 Len : 2
                    packet.WriteInt16(-1);//{soul4Id0}default value : -1 Len : 2
                    packet.WriteInt16(0);//{enchant0}default value : 0 Len : 2
                    packet.WriteInt16(0);//value name : unk20 default value : 0Len : 2
                    packet.WriteByte(0);//value name : unk21 default value : 0Len : 1
                    packet.WriteInt16(-1);//{parametr1Type0}default value : -1 Len : 2
                    packet.WriteInt16(-1);//{paramtetr1Value0}default value : -1 Len : 2
                    packet.WriteInt16(-1);//{parametr2Type0}default value : -1 Len : 2
                    packet.WriteInt16(-1);//{paramtetr2Value0}default value : -1 Len : 2
                    packet.WriteInt16(-1);//{parametr3Type0}default value : -1 Len : 2
                    packet.WriteInt16(-1);//{paramtetr3Value0}default value : -1 Len : 2
                    packet.WriteInt16(-1);//{parametr4Type0}default value : -1 Len : 2
                    packet.WriteInt16(-1);//{paramtetr4Value0}default value : -1 Len : 2
                    packet.WriteInt16(-1);//{parametr5Type0}default value : -1 Len : 2
                    packet.WriteInt16(-1);//{paramtetr5Value0}default value : -1 Len : 2
                    packet.WriteByte(0);//value name : unk32 default value : 0Len : 1
                    packet.WriteByte(0);//{isDressed0}default value : 0 Len : 1
                    packet.WriteInt32(0);//value name : unk34 default value : 0Len : 4
                    packet.WriteInt16(0);//value name : unk35 default value : 0Len : 2

                }
                client.Send(packet, addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.RegisterFishingBook)]//6176
        public static void RegisterFishingBookRequest(IRealmClient client, RealmPacketIn packet)
        {
            var cell = packet.ReadInt16();//default : 17Len : 2
            var itm = client.ActiveCharacter.Asda2Inventory.GetRegularItem(cell);
            if (itm == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Tryes to register not existing fishing book", 10);
                SendFishingBookRegisteredResponse(client, RegisterFishingBookStatus.Fail, null, null);
                return;
            }
            if (itm.Category != Asda2ItemCategory.FishingBook)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Tryes to register not a fishing book", 70);
                SendFishingBookRegisteredResponse(client, RegisterFishingBookStatus.Fail, null, null);
                return;
            }
            var fb = client.ActiveCharacter.RegisteredFishingBooks.Values.FirstOrDefault(f => f.BookId == itm.ItemId);
            if (fb != null)
            {
                SendFishingBookRegisteredResponse(client, RegisterFishingBookStatus.AlreadyRegistered, null, null);
                return;
            }
            RealmServer.IOQueue.AddMessage(() =>
            {
                var nfb = new Asda2FishingBook(itm.ItemId, client.ActiveCharacter,
                                               (byte)client.ActiveCharacter.RegisteredFishingBooks.Count);
                nfb.CreateLater();
                client.ActiveCharacter.RegisteredFishingBooks.Add(nfb.Num, nfb);
                itm.Destroy();
                SendFishingBookRegisteredResponse(client, RegisterFishingBookStatus.Ok, itm, nfb);
            });
        }

        public static void SendFishingBookRegisteredResponse(IRealmClient client, RegisterFishingBookStatus status, Asda2Item book, Asda2FishingBook fishingBook)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.FishingBookRegistered))//6177
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                if (fishingBook != null)
                {
                    packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                    //{invWeight}default value : 8980 Len : 2
                    packet.WriteInt32(book == null ? 0 : book.ItemId); //{bookId}default value : 31824 Len : 4
                    packet.WriteByte((byte)(book == null ? 0 : book.InventoryType)); //{inv}default value : 2 Len : 1
                    packet.WriteInt16(book == null ? 0 : book.Slot); //{cell}default value : 17 Len : 2
                    packet.WriteSkip(unk10); //value name : unk10 default value : unk10Len : 50
                    packet.WriteByte(fishingBook.Num); //{bookNum}default value : 1 Len : 1
                    packet.WriteInt32(0); //value name : unk4 default value : 0Len : 4

                    for (int i = 0; i < 30; i += 1)
                    {
                        packet.WriteInt32(fishingBook.FishIds[0]); //{fishId}default value : 0 Len : 4

                    }
                    for (int i = 0; i < 30; i += 1)
                    {
                        packet.WriteInt16(fishingBook.Amounts[i]); //{amounts}default value : 0 Len : 2

                    }
                    for (int i = 0; i < 30; i += 1)
                    {
                        packet.WriteInt16(fishingBook.MaxLength[i]); //{maxLength}default value : 0 Len : 2

                    }
                    for (int i = 0; i < 30; i += 1)
                    {
                        packet.WriteInt16(fishingBook.MinLengths[i]); //{minLength}default value : 0 Len : 2

                    }
                }
                client.Send(packet, addEnd: true);
            }
        }
        static readonly byte[] unk10 = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x28, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public static void SendFishingBooksInfoResponse(IRealmClient client, Asda2FishingBook book)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.FishingBooksInfo)) //6174
            {
                packet.WriteByte(book.Num); //{fishBookNum}default value : 0 Len : 1
                packet.WriteInt32(0); //value name : unk4 default value : 0Len : 4
                for (int i = 0; i < 30; i += 1)
                {
                    packet.WriteInt32(book.FishIds[i]); //{fishId}default value : 0 Len : 4

                }
                for (int i = 0; i < 30; i += 1)
                {
                    packet.WriteInt16(book.Amounts[i]); //{amounts}default value : 0 Len : 2

                }
                for (int i = 0; i < 30; i += 1)
                {
                    packet.WriteInt16(book.MaxLength[i]); //{maxLength}default value : 0 Len : 2

                }
                for (int i = 0; i < 30; i += 1)
                {
                    packet.WriteInt16(book.MinLengths[i]); //{minLength}default value : 0 Len : 2

                }
                client.Send(packet, addEnd: true);
            }
        }

        public static void SendFishingBookListEndedResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.FishingBookListEnded))//6175
            {
                client.Send(packet, addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.ReciveFishingBookReward)]//6182
        public static void ReciveFishingBookRewardRequest(IRealmClient client, RealmPacketIn packet)
        {
            var bookNum = (byte)packet.ReadInt32();//default : 0Len : 4
            var rewardItemId = packet.ReadInt32();
            if (!client.ActiveCharacter.RegisteredFishingBooks.ContainsKey(bookNum))
            {
                client.ActiveCharacter.YouAreFuckingCheater(
                    "Tryes to recive fish book reward from not registered book.", 80);
                return;
            }
            var book = client.ActiveCharacter.RegisteredFishingBooks[bookNum];
            if (!book.Template.Rewards.Contains(rewardItemId))
            {
                client.ActiveCharacter.YouAreFuckingCheater(
                    "Tryes to recive fish book reward that this book not contained.", 80);
                return;
            }
            var indexOfRewardItem = Array.IndexOf(book.Template.Rewards, rewardItemId);
            Asda2Item rewItem = null;

            var err = client.ActiveCharacter.Asda2Inventory.TryAdd(rewardItemId,
                                                                   book.Template.RewardAmounts[indexOfRewardItem
                                                                       ], true,
                                                                       ref rewItem);
            Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                                                 .AddAttribute("source", 0, "fishing_book")
                                                 .AddItemAttributes(rewItem)
                                                 .Write();
            client.ActiveCharacter.Map.AddMessage(() =>
            {
                if (!book.IsComleted)
                {
                    client.ActiveCharacter.YouAreFuckingCheater(
                        "Tryes to recive fish book reward from not completed book.", 150);
                    return;
                }
                if (err != Asda2InventoryError.Ok)
                {
                    client.ActiveCharacter.SendInfoMsg("Can't recive reward cause " + err);
                    return;
                }
                book.ResetBook();
                SendFishingBookRewardRecivedResponse(client, GetFishingBookRewardStatus.Ok, rewItem, bookNum);
            });
        }
        public static void SendFishingBookRewardRecivedResponse(IRealmClient client, GetFishingBookRewardStatus status, Asda2Item rewardItem, byte fishingBookNum)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.FishingBookRewardRecived))//6183
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt32(fishingBookNum);//{fishBookNum}default value : 0 Len : 4
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);//{invWeight}default value : 9020 Len : 2
                packet.WriteInt32(rewardItem == null ? 0 : rewardItem.ItemId);//{itemId}default value : 31824 Len : 4
                packet.WriteByte((byte)(rewardItem == null ? 0 : rewardItem.InventoryType));//{invType}default value : 2 Len : 1
                packet.WriteInt16(rewardItem == null ? 0 : rewardItem.Slot);//{cell}default value : 17 Len : 2
                packet.WriteSkip(stab21);//value name : stab21 default value : stab21Len : 2
                packet.WriteInt32(rewardItem == null ? 0 : rewardItem.Amount);//{amount}default value : 0 Len : 4
                packet.WriteSkip(stab27);//value name : stab27 default value : stab27Len : 1
                packet.WriteInt16(rewardItem == null ? 0 : rewardItem.Weight);//{weight}default value : 40 Len : 2
                packet.WriteSkip(stub23);//{stub23}default value : stub23 Len : 41
                client.Send(packet, addEnd: true);
            }
        }
        static readonly byte[] stab21 = new byte[] { 0xFF, 0xFF };
        static readonly byte[] stab27 = new byte[] { 0x00 };
        static readonly byte[] stub23 = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public static void SendSomeOneStartedFishingResponse(Character fisher, int fishId, short fishSize)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SomeOneStartedFishing))//6173
            {
                packet.WriteByte(1);//{status}default value : 1 Len : 1
                packet.WriteInt32(fisher.AccId);//{accId}default value : 361343 Len : 4
                packet.WriteInt32(fishId);//{fishId}default value : 31740 Len : 4
                packet.WriteInt16(fishSize);//{fishSize}default value : 64 Len : 2
                fisher.SendPacketToArea(packet);
            }
        }

    }
    [ActiveRecord("Asda2FishingBook", Access = PropertyAccess.Property)]
    public class Asda2FishingBook : WCellRecord<Asda2FishingBook>
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(Asda2FishingBook), "Guid");
        public FishingBookTemplate Template { get; set; }
        public Character Owner { get; set; }
        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }
        [Property]
        public uint OwnerId { get; set; }
        [Property]
        public int BookId { get; set; }
        [Property]
        public byte Num { get; set; }
        public int[] FishIds { get { return Template.RequiredFishes; } }
        [Property]
        [Persistent(Length = 30)]
        public short[] Amounts { get; set; }
        [Property]
        [Persistent(Length = 30)]
        public short[] MinLengths { get; set; }
        [Property]
        [Persistent(Length = 30)]
        public short[] MaxLength { get; set; }

        public bool IsComleted
        {
            get
            {
                for (int i = 0; i < 30; i++)
                {
                    if (FishIds[i] == -1)
                        continue;
                    if (Amounts[i] < Template.RequiredFishesAmounts[i])
                        return false;
                }
                return true;
            }
        }
        public void ResetBook()
        {
            for (int i = 0; i < Amounts.Length; i++)
            {
                Amounts[i] = 0;
            }
        }
        public void OnCatchFish(int fishId, short fishLen)
        {
            if (!Template.FishIndexes.ContainsKey(fishId))
                return;
            var indexOfFish = Template.FishIndexes[fishId];
            if (Amounts[indexOfFish] < Template.RequiredFishesAmounts[indexOfFish])
                Amounts[indexOfFish]++;
            if (MinLengths[indexOfFish] == 0 || MinLengths[indexOfFish] > fishLen)
                MinLengths[indexOfFish] = fishLen;
            if (MaxLength[indexOfFish] == 0 || MaxLength[indexOfFish] < fishLen)
                MaxLength[indexOfFish] = fishLen;
        }

        public Asda2FishingBook()
        {
        }
        public Asda2FishingBook(int bookId, Character owner, byte num)
        {
            Guid = _idGenerator.Next();
            BookId = bookId;
            OwnerId = owner.EntityId.Low;
            Owner = owner;
            Template = Asda2FishingMgr.FishingBookTemplates[bookId];
            Num = num;
            MaxLength = new short[30];
            MinLengths = new short[30];
            Amounts = new short[30];
        }
        void InitAfterLoad()
        {
            Template = Asda2FishingMgr.FishingBookTemplates[BookId];
            Owner = World.GetCharacter(OwnerId);
        }
        public static Asda2FishingBook[] LoadAll(Character chr)
        {
            var r = FindAllByProperty("OwnerId", chr.EntityId.Low);
            foreach (var asda2FishingBook in r)
            {
                asda2FishingBook.InitAfterLoad();
            }
            return r;
        }

        public void Complete()
        {
            for (int i = 0; i < 30; i++)
            {
                Amounts[i] = (short)Template.RequiredFishesAmounts[i];
            }
        }
    }
    public enum GetFishingBookRewardStatus
    {
        Fail = 0,
        Ok = 1,
    }
    public enum RegisterFishingBookStatus
    {
        Fail = 0,
        Ok = 1,
        AlreadyRegistered = 5,
    }
    public enum Asda2EndFishingStatus
    {
        Ok = 1,
        YouAlreadyFishing = 3,
        NoBait = 4,
        NoSpace = 5,
        NoWeight = 6,
    }

    public enum Asda2StartFishStatus
    {
        Ok = 1,
        YouAreAlreadyFishing = 2,
        NotEnoughtSpace = 3,
        YouCantFishHere = 4,
        YouHaveNoFishRod = 5,
        YouHaveNoBait = 6,
        YourFishingLevelIsToLowToFishHereItMustBe = 7,
        InsuficientFishingRodDurability = 8,

    }
}