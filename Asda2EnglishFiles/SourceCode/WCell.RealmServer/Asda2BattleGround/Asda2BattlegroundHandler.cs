using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Asda2BattleGround
{
    public static class Asda2BattlegroundHandler
    {
        private static readonly byte[] unk80 = new byte[107]
        {
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
            (byte) 250,
            (byte) 20,
            (byte) 124,
            (byte) 80,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] stab9 = new byte[12]
        {
            (byte) 1,
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
            (byte) 0
        };

        private static readonly byte[] stab35 = new byte[3]
        {
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue
        };

        private static readonly byte[] stab46 = new byte[16]
        {
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
            byte.MaxValue
        };

        private static readonly byte[] guildCrest = new byte[40];

        public static void SendMessageServerAboutWarStartsResponse(byte mins)
        {
            int num1 = (int) mins / 10;
            int num2 = (int) mins % 10;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MessageServerAboutWarStarts))
            {
                packet.WriteInt16(0);
                packet.WriteInt16(100);
                packet.WriteByte(48 + num1);
                packet.WriteByte(48 + num2);
                packet.WriteSkip(Asda2BattlegroundHandler.unk80);
                WCell.RealmServer.Global.World.Broadcast(packet, true, Locale.Any);
            }
        }

        [PacketHandler(RealmServerOpCode.RequestUpdateWarScreenManagerData)]
        public static void RequestUpdateWarScreenManagerDataRequest(IRealmClient client, RealmPacketIn packet)
        {
            int num;
            if (client.ActiveCharacter.MapId == MapId.Alpia)
                num = 0;
            else if (client.ActiveCharacter.MapId == MapId.Silaris)
                num = 1;
            else if (client.ActiveCharacter.MapId == MapId.Flamio)
            {
                num = 2;
            }
            else
            {
                if (client.ActiveCharacter.MapId != MapId.Aquaton)
                    return;
                num = 3;
            }

            Asda2BattlegroundHandler.SendUpdateWarManagerScreenDataResponse(client,
                Asda2BattlegroundMgr.AllBattleGrounds[(Asda2BattlegroundTown) num][0]);
        }

        public static void SendUpdateWarManagerScreenDataResponse(IRealmClient client, Asda2Battleground btlgrnd)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpdateWarManagerScreenData))
            {
                packet.WriteByte(1);
                packet.WriteByte(0);
                packet.WriteInt16(btlgrnd.StartTime.Hour);
                packet.WriteInt16(btlgrnd.StartTime.Minute);
                packet.WriteInt16(btlgrnd.EndTime.Hour);
                packet.WriteInt16(btlgrnd.EndTime.Minute);
                packet.WriteInt16(btlgrnd.AmountOfBattleGroundsInList);
                packet.WriteInt16(btlgrnd.LightTeam.Count);
                packet.WriteInt16(btlgrnd.DarkTeam.Count);
                packet.WriteByte(0);
                packet.WriteInt32(0);
                packet.WriteInt16(btlgrnd.LightWins);
                packet.WriteInt16(btlgrnd.DarkWins);
                packet.WriteInt16(btlgrnd.LightLooses);
                packet.WriteInt16(btlgrnd.DarkLooses);
                packet.WriteInt16((int) btlgrnd.LightWins + (int) btlgrnd.LightLooses);
                packet.WriteByte(0);
                packet.WriteInt16(btlgrnd.MinEntryLevel);
                packet.WriteInt16(btlgrnd.MaxEntryLevel);
                packet.WriteByte((byte) btlgrnd.WarType);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.LeaveBattleGround)]
        public static void LeaveBattleGroundRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.CurrentBattleGround == null)
                return;
            client.ActiveCharacter.CurrentBattleGround.Leave(client.ActiveCharacter);
        }

        [PacketHandler(RealmServerOpCode.RegisterToWar)]
        public static void RegisterToWarRequest(IRealmClient client, RealmPacketIn packet)
        {
            Asda2Battleground asda2Battleground = Asda2BattlegroundMgr.AllBattleGrounds[
                client.ActiveCharacter.MapId == MapId.Alpia
                    ? Asda2BattlegroundTown.Alpia
                    : (client.ActiveCharacter.MapId == MapId.Silaris
                        ? Asda2BattlegroundTown.Silaris
                        : (client.ActiveCharacter.MapId == MapId.Aquaton
                            ? Asda2BattlegroundTown.Aquaton
                            : Asda2BattlegroundTown.Flamio))][0];
            if (client.ActiveCharacter.Level < (int) asda2Battleground.MinEntryLevel ||
                client.ActiveCharacter.Level > (int) asda2Battleground.MaxEntryLevel)
                Asda2BattlegroundHandler.SendRegisteredToWarResponse(client, RegisterToBattlegroundStatus.WrongLevel);
            else if (client.ActiveCharacter.CurrentBattleGround != null)
                Asda2BattlegroundHandler.SendRegisteredToWarResponse(client,
                    RegisterToBattlegroundStatus.YouHaveAlreadyRegistered);
            else if (asda2Battleground.DissmisedCharacterNames.Contains(client.ActiveCharacter.Name))
                Asda2BattlegroundHandler.SendRegisteredToWarResponse(client,
                    RegisterToBattlegroundStatus.YouCantEnterCauseYouHaveBeenDissmised);
            else if (client.ActiveCharacter.Asda2FactionId < (short) 0 ||
                     client.ActiveCharacter.Asda2FactionId > (short) 1)
                Asda2BattlegroundHandler.SendRegisteredToWarResponse(client,
                    RegisterToBattlegroundStatus.BattleGroupInfoIsInvalid);
            else if (asda2Battleground.Join(client.ActiveCharacter))
            {
                Asda2BattlegroundHandler.SendRegisteredToWarResponse(client, RegisterToBattlegroundStatus.Ok);
            }
            else
            {
                Asda2BattlegroundHandler.SendRegisteredToWarResponse(client, RegisterToBattlegroundStatus.Fail);
                client.ActiveCharacter.SendInfoMsg("Sry no more free war places. Try again later.");
            }
        }

        public static void SendRegisteredToWarResponse(IRealmClient client, RegisterToBattlegroundStatus status)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.RegisteredToWar))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(client.ActiveCharacter.Asda2FactionId);
                packet.WriteInt16(0);
                packet.WriteInt16(6);
                client.Send(packet, false);
            }
        }

        public static void SendWarHasBeenCanceledResponse(IRealmClient client,
            Asda2BattlegroundWarCanceledReason status)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WarHasBeenCanceled))
            {
                packet.WriteInt16((byte) status);
                client.Send(packet, false);
            }
        }

        public static void SendWiningFactionInfoResponse(Asda2BattlegroundTown townId, int factionId, string mvpName)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WiningFactionInfo))
            {
                packet.WriteInt32((int) townId);
                packet.WriteInt32(factionId);
                packet.WriteFixedAsciiString(mvpName, 20, Locale.Start);
                WCell.RealmServer.Global.World.Broadcast(packet, true, Locale.Any);
            }
        }

        public static void SendYouCanEnterWarAfterResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.YouCanEnterWarAfter))
            {
                packet.WriteByte(client.ActiveCharacter.Asda2FactionId);
                packet.WriteInt32(14);
                client.Send(packet, true);
            }
        }

        public static void SendYouCanEnterWarResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.YouCanEnterWar))
            {
                packet.WriteByte(0);
                packet.WriteInt16(client.ActiveCharacter.Asda2FactionId);
                packet.WriteInt16(0);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.EnterBatlefield)]
        public static void EnterBatlefieldRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.CurrentBattleGround == null)
                client.ActiveCharacter.SendWarMsg("You are not registered to faction war.");
            else if (!client.ActiveCharacter.CurrentBattleGround.IsRunning)
                client.ActiveCharacter.SendWarMsg(string.Format("War is not started yet. Wait {0} mins.",
                    (object) (int) (client.ActiveCharacter.CurrentBattleGround.StartTime - DateTime.Now).TotalMinutes));
            else if (client.ActiveCharacter.MapId == MapId.BatleField)
                client.ActiveCharacter.SendWarMsg("You already on war.");
            else
                client.ActiveCharacter.CurrentBattleGround.TeleportToWar(client.ActiveCharacter);
        }

        public static void SendCharacterPositionInfoOnWarResponse(Character chr)
        {
            if (chr.CurrentBattleGround == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterPositionInfoOnWar))
            {
                packet.WriteInt16(chr.Asda2FactionId);
                packet.WriteInt32(chr.AccId);
                packet.WriteInt16(chr.SessionId);
                packet.WriteInt16((short) chr.Asda2Position.X);
                packet.WriteInt16((short) chr.Asda2Position.Y);
                chr.CurrentBattleGround.Send(packet, true, new short?(chr.Asda2FactionId), Locale.Any);
            }
        }

        public static void SendSomeOneKilledSomeOneResponse(Asda2Battleground btlgrnd, int killerAccId, int killerWarId,
            string killerName, string victimName)
        {
            Character characterByAccId = WCell.RealmServer.Global.World.GetCharacterByAccId((uint) killerAccId);
            AchievementProgressRecord progressRecord = characterByAccId.Achievements.GetOrCreateProgressRecord(21U);
            switch (++progressRecord.Counter)
            {
                case 13:
                    characterByAccId.DiscoverTitle(Asda2TitleId.Soldier129);
                    break;
                case 25:
                    characterByAccId.GetTitle(Asda2TitleId.Soldier129);
                    break;
                case 75:
                    characterByAccId.DiscoverTitle(Asda2TitleId.Killer130);
                    break;
                case 100:
                    characterByAccId.GetTitle(Asda2TitleId.Killer130);
                    break;
                case 500:
                    characterByAccId.DiscoverTitle(Asda2TitleId.Assassin131);
                    break;
                case 1000:
                    characterByAccId.GetTitle(Asda2TitleId.Assassin131);
                    break;
            }

            progressRecord.SaveAndFlush();
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SomeOneKilledSomeOne))
            {
                packet.WriteInt32(killerAccId);
                packet.WriteInt32(killerWarId);
                packet.WriteInt32(1);
                packet.WriteFixedAsciiString(killerName, 20, Locale.Start);
                packet.WriteFixedAsciiString(victimName, 20, Locale.Start);
                btlgrnd.Send(packet, true, new short?(), Locale.Any);
            }
        }

        public static void SendTeamPointsResponse(Asda2Battleground btlgrnd, Character chr = null)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.TeamPoints))
            {
                packet.WriteInt32(btlgrnd.LightScores);
                packet.WriteInt32(btlgrnd.DarkScores);
                if (chr != null)
                    chr.Send(packet, true);
                else
                    btlgrnd.Send(packet, true, new short?(), Locale.Any);
            }
        }

        public static void SendWarRemainingTimeResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WarRemainingTime))
            {
                packet.WriteInt16(DateTime.Now.Hour);
                packet.WriteInt16(DateTime.Now.Minute);
                packet.WriteInt16(DateTime.Now.Second);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.WarChatRequest)]
        public static void WarChatRequestRequest(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = (int) packet.ReadInt16();
            int num2 = (int) packet.ReadInt16();
            packet.Position += 20;
            string str = packet.ReadAsdaString(200, client.Locale);
            if (str.Length < 1 ||
                RealmCommandHandler.HandleCommand((IUser) client.ActiveCharacter, str,
                    (IGenericChatTarget) (client.ActiveCharacter.Target as Character)) ||
                !client.ActiveCharacter.IsAsda2BattlegroundInProgress)
                return;
            Locale locale = Asda2EncodingHelper.MinimumAvailableLocale(client.Locale, str);
            Asda2BattlegroundHandler.SendWarChatResponseResponse(client.ActiveCharacter.CurrentBattleGround,
                client.ActiveCharacter.Name, str, (int) client.ActiveCharacter.Asda2FactionId, locale);
        }

        public static void SendWarChatResponseResponse(Asda2Battleground btlgrnd, string senderName, string message,
            int factionId, Locale locale)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WarChatResponse))
            {
                packet.WriteByte(3);
                packet.WriteFixedAsciiString(senderName, 20, Locale.Start);
                packet.WriteFixedAsciiString(message, 200, locale);
                btlgrnd.Send(packet, true, new short?((short) factionId), locale);
            }
        }

        public static void SendHowManyPeopleInWarTeamsResponse(Asda2Battleground btlgrnd, Character chr = null)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.HowManyPeopleInWarTeams))
            {
                packet.WriteInt16(btlgrnd.LightTeam.Count);
                packet.WriteInt16(btlgrnd.DarkTeam.Count);
                if (chr != null)
                    chr.Send(packet, true);
                else
                    btlgrnd.Send(packet, true, new short?(), Locale.Any);
            }
        }

        public static void SendCharacterHasLeftWarResponse(Asda2Battleground btlgrnd, int leaverAccId, byte leaverWarId,
            string leaverName, int factionId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterHasLeftWar))
            {
                packet.WriteByte(0);
                packet.WriteInt32(leaverAccId);
                packet.WriteByte(leaverWarId);
                packet.WriteFixedAsciiString(leaverName, 20, Locale.Start);
                btlgrnd.Send(packet, true, new short?((short) factionId), Locale.Any);
            }
        }

        public static void SendWarCurrentActionInfoResponse(Asda2Battleground btlgrnd,
            BattleGroundInfoMessageType status, short value, Character chr = null, short? factionId = null)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WarCurrentActionInfo))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(value);
                if (chr == null)
                    btlgrnd.Send(packet, false, factionId, Locale.Any);
                else
                    chr.Send(packet, false);
            }
        }

        public static void SendWarEndedResponse(IRealmClient client, byte winingFaction, int winingFactionPoints,
            int losserFactionPoints, int honorPoints, short honorCoin, long expReward, string mvpName)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WarEnded))
            {
                packet.WriteInt16(winingFaction);
                packet.WriteSkip(Asda2BattlegroundHandler.stab9);
                packet.WriteInt32(winingFactionPoints);
                packet.WriteInt32(losserFactionPoints);
                packet.WriteInt32(honorPoints);
                packet.WriteInt16(honorCoin);
                packet.WriteSkip(Asda2BattlegroundHandler.stab35);
                packet.WriteInt64(expReward);
                packet.WriteSkip(Asda2BattlegroundHandler.stab46);
                packet.WriteFixedAsciiString(mvpName, 20, Locale.Start);
                client.Send(packet, true);
            }
        }

        public static void SendWarEndedOneResponse(IRealmClient client, IEnumerable<Asda2Item> prizeItems)
        {
            Asda2Item[] asda2ItemArray = new Asda2Item[4];
            int num = 0;
            foreach (Asda2Item prizeItem in prizeItems)
                asda2ItemArray[num++] = prizeItem;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WarEndedOne))
            {
                packet.WriteByte(1);
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

        [PacketHandler(RealmServerOpCode.ShowWarUnit)]
        public static void ShowWarUnitRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsAsda2BattlegroundInProgress)
                return;
            Asda2BattlegroundHandler.SendWarTeamListResponse(client.ActiveCharacter);
        }

        [PacketHandler(RealmServerOpCode.CancleWarPatipication)]
        public static void CancleWarPatipicationRequest(IRealmClient client, RealmPacketIn packet)
        {
            Asda2BattlegroundHandler.SendWarPartipicationCanceledResponse(client);
            if (client.ActiveCharacter.CurrentBattleGround == null)
                return;
            client.ActiveCharacter.CurrentBattleGround.Leave(client.ActiveCharacter);
        }

        public static void SendWarPartipicationCanceledResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WarPartipicationCanceled))
            {
                packet.WriteByte(1);
                packet.WriteByte(client.ActiveCharacter.Asda2FactionId);
                client.Send(packet, false);
            }
        }

        public static void SendWarTeamListResponse(Character chr)
        {
            Asda2Battleground currentBattleGround = chr.CurrentBattleGround;
            List<List<Character>> characterListList = new List<List<Character>>();
            lock (currentBattleGround.JoinLock)
            {
                if (chr.Asda2FactionId == (short) 0)
                {
                    int num = currentBattleGround.LightTeam.Count / 6;
                    if (num == 0)
                        num = 1;
                    for (int index = 0; index < num; ++index)
                        characterListList.Add(currentBattleGround.LightTeam.Values.Skip<Character>(index * 6)
                            .Take<Character>(6).ToList<Character>());
                }
                else
                {
                    int num = currentBattleGround.DarkTeam.Count / 6;
                    if (num == 0)
                        num = 1;
                    for (int index = 0; index < num; ++index)
                        characterListList.Add(currentBattleGround.DarkTeam.Values.Skip<Character>(index * 6)
                            .Take<Character>(6).ToList<Character>());
                }

                foreach (List<Character> characterList in characterListList)
                {
                    using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WarTeamList))
                    {
                        foreach (Character character in characterList)
                        {
                            packet.WriteByte(character.CurrentBattleGroundId);
                            packet.WriteByte(character.Asda2FactionId);
                            packet.WriteInt16(character.SessionId);
                            packet.WriteInt32(character.AccId);
                            packet.WriteByte(character.CharNum);
                            packet.WriteByte(3);
                            packet.WriteByte(character.Asda2FactionRank);
                            packet.WriteInt16(character.Level);
                            packet.WriteByte(character.ProfessionLevel);
                            packet.WriteByte((byte) character.Archetype.ClassId);
                            packet.WriteByte(character.Guild == null ? 0 : (character.Guild.ClanCrest == null ? 0 : 1));
                            packet.WriteSkip(character.Guild == null
                                ? Asda2BattlegroundHandler.guildCrest
                                : character.Guild.ClanCrest ?? Asda2BattlegroundHandler.guildCrest);
                            packet.WriteInt16(character.IsInGroup ? 1 : -1);
                            packet.WriteInt16(character.BattlegroundDeathes);
                            packet.WriteInt16(character.BattlegroundKills);
                            packet.WriteInt16(character.BattlegroundActPoints);
                            packet.WriteFixedAsciiString(character.Name, 20, Locale.Start);
                            packet.WriteFixedAsciiString(character.Guild == null ? "" : character.Guild.Name, 17,
                                Locale.Start);
                            packet.WriteInt16((short) character.Asda2Position.X);
                            packet.WriteInt16((short) character.Asda2Position.Y);
                        }

                        chr.Send(packet, false);
                    }
                }

                Asda2BattlegroundHandler.SendWarTeamListEndedResponse(chr.Client);
            }
        }

        public static void SendUpdatePointInfoResponse(IRealmClient client, Asda2WarPoint point)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpdatePointInfo))
            {
                packet.WriteInt16(point.Id);
                packet.WriteInt16(point.X);
                packet.WriteInt16(point.Y);
                packet.WriteInt16(point.OwnedFaction);
                packet.WriteInt16((short) point.Status);
                packet.WriteByte(0);
                point.BattleGround.Send(packet, true, new short?(), Locale.Any);
            }
        }

        public static void SendWarPointsPreInitResponse(IRealmClient client, Asda2WarPoint point)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WarPointsPreInit))
            {
                packet.WriteInt16(point.Id);
                packet.WriteInt16(point.X);
                packet.WriteInt16(point.Y);
                packet.WriteInt16(point.OwnedFaction);
                packet.WriteInt16((short) point.Status);
                packet.WriteByte(0);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.StartOcupyPoint)]
        public static void StartOcupyPointRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            byte num = packet.ReadByte();
            if (!client.ActiveCharacter.IsAsda2BattlegroundInProgress ||
                client.ActiveCharacter.MapId != MapId.BatleField)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to occupy point while not in war.", 20);
            }
            else
            {
                Asda2Battleground currentBattleGround = client.ActiveCharacter.CurrentBattleGround;
                if ((int) num >= currentBattleGround.Points.Count)
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to occupy unknown war point.", 20);
                else
                    currentBattleGround.Points[(int) num].TryCapture(client.ActiveCharacter);
            }
        }

        public static void SendOccupyingPointStartedResponse(IRealmClient client, short pointId,
            OcupationPointStartedStatus status)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.OccupyingPointStarted))
            {
                packet.WriteInt16(pointId);
                packet.WriteByte((byte) status);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                client.ActiveCharacter.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendWarTeamListEndedResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.WarTeamListEnded))
                client.Send(packet, false);
        }

        [PacketHandler(RealmServerOpCode.DismissPlayerFromWar)]
        public static void DismissPlayerFromWarRequest(IRealmClient client, RealmPacketIn packet)
        {
            short num = packet.ReadInt16();
            if (!client.ActiveCharacter.IsAsda2BattlegroundInProgress)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to dissmis someone while not on war.", 50);
            }
            else
            {
                Character character =
                    client.ActiveCharacter.CurrentBattleGround.GetCharacter(client.ActiveCharacter.Asda2FactionId,
                        (byte) num);
                if (character == null)
                    client.ActiveCharacter.SendWarMsg("Target character not found.");
                using (RealmPacketOut packet1 = new RealmPacketOut(RealmServerOpCode.DismissPlayerFromWarRequestResult))
                {
                    if (character == null ||
                        !client.ActiveCharacter.CurrentBattleGround.TryStartDissmisProgress(client.ActiveCharacter,
                            character) || client.ActiveCharacter.Money < 10000U)
                    {
                        packet1.WriteByte(0);
                        packet1.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                        Asda2InventoryHandler.WriteItemInfoToPacket(packet1, (Asda2Item) null, false);
                    }
                    else
                    {
                        packet1.WriteByte(1);
                        packet1.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                        Asda2InventoryHandler.WriteItemInfoToPacket(packet1,
                            client.ActiveCharacter.Asda2Inventory.GetRegularItem((short) 0), false);
                        client.ActiveCharacter.SubtractMoney(10000U);
                    }

                    client.Send(packet1, true);
                }
            }
        }

        public static void SendQuestionDismissPlayerOrNotResponse(Asda2Battleground client, Character initer,
            Character target)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.QuestionDismissPlayerOrNot))
            {
                packet.WriteInt16(initer.SessionId);
                packet.WriteInt16(initer.Asda2FactionId);
                packet.WriteInt16(target.SessionId);
                packet.WriteInt32(target.AccId);
                client.Send(packet, true, new short?(initer.Asda2FactionId), Locale.Any);
            }
        }

        [PacketHandler(RealmServerOpCode.AnswerDismissPlayer)]
        public static void AnswerDismissPlayerRequest(IRealmClient client, RealmPacketIn packet)
        {
            bool kick = packet.ReadByte() == (byte) 1;
            if (!client.ActiveCharacter.IsAsda2BattlegroundInProgress)
                client.ActiveCharacter.SendWarMsg("Player not found.");
            else
                client.ActiveCharacter.CurrentBattleGround.AnswerDismiss(kick, client.ActiveCharacter);
        }

        public static void SendDissmissResultResponse(Asda2Battleground client, DismissPlayerResult status,
            short targetSessId, int targetAccId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.DissmissResult))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(targetSessId);
                packet.WriteInt32(targetAccId);
                client.Send(packet, true, new short?(), Locale.Any);
            }
        }
    }
}