using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Asda2BattleGround
{
    public static class Asda2BattlegroundHandler
    {
        public static void SendMessageServerAboutWarStartsResponse(byte mins)
        {
            var tenMins = mins/10;
            var m = mins%10;
            using (var packet = new RealmPacketOut(RealmServerOpCode.MessageServerAboutWarStarts))//3113
            {
                packet.WriteInt16(0);//value name : unk4 default value : 0Len : 2
                packet.WriteInt16(100);//value name : unk5 default value : 100Len : 2
                packet.WriteByte(48+tenMins);//{mins10StartsFrom48}default value : 49 Len : 1
                packet.WriteByte(48+m);//{minsOneStartFrom48}default value : 48 Len : 1
                packet.WriteSkip(unk80);//value name : unk80 default value : unk80Len : 107
                World.Broadcast(packet,true,Locale.Any);
            }
        }
        static readonly byte[] unk80 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFA, 0x14, 0x7C, 0x50, 0x00, 0x00 };

        [PacketHandler(RealmServerOpCode.RequestUpdateWarScreenManagerData)] //6700
        public static void RequestUpdateWarScreenManagerDataRequest(IRealmClient client, RealmPacketIn packet)
        {
            int town;
            if (client.ActiveCharacter.MapId == MapId.Alpia)
                town = 0;
            else if (client.ActiveCharacter.MapId == MapId.Silaris)
                town = 1;
            else if (client.ActiveCharacter.MapId == MapId.Flamio)
                town = 2;
            else if (client.ActiveCharacter.MapId == MapId.Aquaton)
                town = 3;
            else
                return;
            SendUpdateWarManagerScreenDataResponse(client,Asda2BattlegroundMgr.AllBattleGrounds[(Asda2BattlegroundTown) town][0]);
        }

        public static void SendUpdateWarManagerScreenDataResponse(IRealmClient client, Asda2Battleground btlgrnd)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.UpdateWarManagerScreenData)) //6701
            {
                packet.WriteByte(1); //{status}default value : 1 Len : 1
                packet.WriteByte(0); //value name : unk6 default value : 0Len : 1
                packet.WriteInt16(btlgrnd.StartTime.Hour); //{startTimeHours}default value : 8 Len : 2
                packet.WriteInt16(btlgrnd.StartTime.Minute); //{startTimeMins}default value : 0 Len : 2
                packet.WriteInt16(btlgrnd.EndTime.Hour); //{endTimeHours}default value : 8 Len : 2
                packet.WriteInt16(btlgrnd.EndTime.Minute); //{endTimeMins}default value : 25 Len : 2
                packet.WriteInt16(btlgrnd.AmountOfBattleGroundsInList); //{battlesInListCount}default value : 1 Len : 2
                packet.WriteInt16(btlgrnd.LightTeam.Count); //{forLitePeople}default value : 1 Len : 2
                packet.WriteInt16(btlgrnd.DarkTeam.Count); //{forDarkPeople}default value : 0 Len : 2
                packet.WriteByte(0); //{mustBe0}default value : 0 Len : 1
                packet.WriteInt32(0); //{canBe0}default value : 586427422 Len : 4
                packet.WriteInt16(btlgrnd.LightWins); //{winWarsLight}default value : 639 Len : 2
                packet.WriteInt16(btlgrnd.DarkWins); //{winWarsDark}default value : 548 Len : 2
                packet.WriteInt16(btlgrnd.LightLooses); //{lightLoses}default value : 548 Len : 2
                packet.WriteInt16(btlgrnd.DarkLooses); //{darkLoses}default value : 639 Len : 2
                packet.WriteInt16(btlgrnd.LightWins + btlgrnd.LightLooses); //{totalWars}default value : 1187 Len : 2
                packet.WriteByte(0); //{dailyWarsMinusOne}default value : 0 Len : 1
                packet.WriteInt16(btlgrnd.MinEntryLevel); //{minEnterLevel}default value : 10 Len : 2
                packet.WriteInt16(btlgrnd.MaxEntryLevel); //{maxEnterLevel}default value : 29 Len : 2
                packet.WriteByte((byte) btlgrnd.WarType);
                client.Send(packet, addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.LeaveBattleGround)]//6731
        public static void LeaveBattleGroundRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.CurrentBattleGround!=null)
            {
                client.ActiveCharacter.CurrentBattleGround.Leave(client.ActiveCharacter);
            }
        }
            

        [PacketHandler(RealmServerOpCode.RegisterToWar)] //6706
        public static void RegisterToWarRequest(IRealmClient client, RealmPacketIn packet)
        {
            var btlgrnd =
                Asda2BattlegroundMgr.AllBattleGrounds[
                    client.ActiveCharacter.MapId == MapId.Alpia
                        ? Asda2BattlegroundTown.Alpia
                        : client.ActiveCharacter.MapId == MapId.Silaris
                              ? Asda2BattlegroundTown.Silaris
                              : client.ActiveCharacter.MapId == MapId.Aquaton
                              ?Asda2BattlegroundTown.Aquaton : Asda2BattlegroundTown.Flamio][0];
            if (client.ActiveCharacter.Level < btlgrnd.MinEntryLevel ||
                client.ActiveCharacter.Level > btlgrnd.MaxEntryLevel)
            {
                SendRegisteredToWarResponse(client, RegisterToBattlegroundStatus.WrongLevel);
                return;
            }
            if (client.ActiveCharacter.CurrentBattleGround != null)
            {
                SendRegisteredToWarResponse(client, RegisterToBattlegroundStatus.YouHaveAlreadyRegistered);
                return;
            }
            if (btlgrnd.DissmisedCharacterNames.Contains(client.ActiveCharacter.Name))
            {
                SendRegisteredToWarResponse(client, RegisterToBattlegroundStatus.YouCantEnterCauseYouHaveBeenDissmised);
                return;
            }
            if (client.ActiveCharacter.Asda2FactionId < 0 || client.ActiveCharacter.Asda2FactionId > 1)
            {
                SendRegisteredToWarResponse(client, RegisterToBattlegroundStatus.BattleGroupInfoIsInvalid);
                return;
            }
            if (btlgrnd.Join(client.ActiveCharacter))
            {
                SendRegisteredToWarResponse(client, RegisterToBattlegroundStatus.Ok);
                return;
            }
            else
            {
                SendRegisteredToWarResponse(client, RegisterToBattlegroundStatus.Fail);
                client.ActiveCharacter.SendInfoMsg("Sry no more free war places. Try again later.");
                return;
            }
        }

        public static void SendRegisteredToWarResponse(IRealmClient client, RegisterToBattlegroundStatus status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.RegisteredToWar)) //6707
            {
                packet.WriteByte((byte) status); //{status}default value : 1 Len : 1
                packet.WriteInt16(client.ActiveCharacter.Asda2FactionId); //{factionId}default value : 1 Len : 2
                packet.WriteInt16(0); //{place}default value : 3 Len : 2
                packet.WriteInt16(6); //{battleField}default value : 1 Len : 2
                client.Send(packet);
            }
        }

        public static void SendWarHasBeenCanceledResponse(IRealmClient client, Asda2BattlegroundWarCanceledReason status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.WarHasBeenCanceled)) //6740
            {
                packet.WriteInt16((byte) status); //{status}default value : 3 Len : 2
                client.Send(packet);
            }
        }

        public static void SendWiningFactionInfoResponse(Asda2BattlegroundTown townId,
                                                         int factionId, string mvpName)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.WiningFactionInfo)) //6257
            {
                packet.WriteInt32((int) townId); //{townId}default value : 0 Len : 4
                packet.WriteInt32(factionId); //{factionId}default value : 0 Len : 4
                packet.WriteFixedAsciiString(mvpName, 20); //{MVPName}default value :  Len : 20
                World.Broadcast(packet,true,Locale.Any);
            }
        }

        public static void SendYouCanEnterWarAfterResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.YouCanEnterWarAfter)) //6710
            {
                packet.WriteByte(client.ActiveCharacter.Asda2FactionId); //{faction}default value : 1 Len : 1
                packet.WriteInt32(14); //{charsAmount}default value : 14 Len : 4
                client.Send(packet, addEnd: true);
            }
        }

        public static void SendYouCanEnterWarResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.YouCanEnterWar)) //6711
            {
                packet.WriteByte(0); //{status}default value : 0 Len : 1
                packet.WriteInt16(client.ActiveCharacter.Asda2FactionId); //{faction}default value : 1 Len : 2
                packet.WriteInt16(0); //{charsAmount}default value : 14 Len : 2
                client.Send(packet);
            }
        }

        [PacketHandler(RealmServerOpCode.EnterBatlefield)] //6714
        public static void EnterBatlefieldRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.CurrentBattleGround == null)
            {
                client.ActiveCharacter.SendWarMsg("You are not registered to faction war.");
                //GlobalHandler.SendTeleportedByCristalResponse(client,MapId.Alpia, 0,0,TeleportByCristalStaus.CantEnterWarCauseLowPlayersInOtherFaction);
                return;
            }
            if (!client.ActiveCharacter.CurrentBattleGround.IsRunning)
            {
                client.ActiveCharacter.SendWarMsg(string.Format("War is not started yet. Wait {0} mins.", (int)(client.ActiveCharacter.CurrentBattleGround.StartTime - DateTime.Now).TotalMinutes));
                //GlobalHandler.SendTeleportedByCristalResponse(client,MapId.Alpia, 0,0,TeleportByCristalStaus.CantEnterWarCauseLowPlayersInOtherFaction);
                return;
            }
            if (client.ActiveCharacter.MapId == MapId.BatleField)
            {
                client.ActiveCharacter.SendWarMsg("You already on war.");
                return;
            }
            /*if (client.ActiveCharacter.CurrentBattleGround.DarkTeam.Count + 1 == client.ActiveCharacter.CurrentBattleGround.LightTeam.Count || client.ActiveCharacter.CurrentBattleGround.LightTeam.Count + 1 == client.ActiveCharacter.CurrentBattleGround.DarkTeam.Count)
            {
                client.ActiveCharacter.SendWarMsg("·«  ” ÿÌ⁄ «·«‰÷„«„ ··Õ—»");
                return;
            }*/
            client.ActiveCharacter.CurrentBattleGround.TeleportToWar(client.ActiveCharacter);
        }

        #region OnWar

        public static void SendCharacterPositionInfoOnWarResponse(Character chr)
        {
            if (chr.CurrentBattleGround == null) return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterPositionInfoOnWar)) //6720
            {
                packet.WriteInt16(chr.Asda2FactionId); //{faction}default value : 1 Len : 2
                packet.WriteInt32(chr.AccId); //{accId}default value : 356425 Len : 4
                packet.WriteInt16(chr.SessionId); //{sessId}default value : 93 Len : 2
                packet.WriteInt16((short) chr.Asda2Position.X); //{x}default value : 262 Len : 2
                packet.WriteInt16((short) chr.Asda2Position.Y); //{y}default value : 330 Len : 2
                chr.CurrentBattleGround.Send(packet, addEnd: true, asda2FactionId: chr.Asda2FactionId);
                chr.CurrentBattleGround.Possion(chr);
            }
        }

        public static void SendSomeOneKilledSomeOneResponse(Asda2Battleground btlgrnd, int killerAccId, int killerWarId,
                                                            string killerName, string victimName)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SomeOneKilledSomeOne)) //6730
            {
                packet.WriteInt32(killerAccId); //{killerAccId}default value : 315044 Len : 4
                packet.WriteInt32(killerWarId); //{killerWarId}default value : 8 Len : 4
                packet.WriteInt32(1); //value name : unk7 default value : 1Len : 4
                packet.WriteFixedAsciiString(killerName, 20); //{killerName}default value :  Len : 20
                packet.WriteFixedAsciiString(victimName, 20); //{vicimName}default value :  Len : 20
                btlgrnd.Send(packet, addEnd: true);
            }
        }

        public static void SendTeamPointsResponse(Asda2Battleground btlgrnd,Character chr = null)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.TeamPoints)) //6729
            {
                packet.WriteInt32(btlgrnd.LightScores); //{light}default value : 106 Len : 4
                packet.WriteInt32(btlgrnd.DarkScores); //{dark}default value : 462 Len : 4
                if(chr!=null)
                    chr.Send(packet,addEnd: true);
                else 
                    btlgrnd.Send(packet, addEnd: true);
            }
        }
        public static void SendWarRemainingTimeResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.WarRemainingTime))//6736
            {
                packet.WriteInt16(DateTime.Now.Hour);//{hour}default value : 4 Len : 2
                packet.WriteInt16(DateTime.Now.Minute);//{min}default value : 0 Len : 2
                packet.WriteInt16(DateTime.Now.Second);//{sec}default value : 13 Len : 2
                client.Send(packet);
            }
        }

        [PacketHandler(RealmServerOpCode.WarChatRequest)] //6718
        public static void WarChatRequestRequest(IRealmClient client, RealmPacketIn packet)
        {
            var faction = packet.ReadInt16(); //default : 1Len : 2
            var sessId = packet.ReadInt16(); //default : 26Len : 2
            packet.Position += 20;
            var message = packet.ReadAsdaString(200,client.Locale); //default : Len : 204
            if (message.Length < 1 ||
                Commands.RealmCommandHandler.HandleCommand(client.ActiveCharacter, message,
                                                  client.ActiveCharacter.Target as Character))
                return;
            if (!client.ActiveCharacter.IsAsda2BattlegroundInProgress)
            {
               // client.ActiveCharacter.YouAreFuckingCheater("Trying to war chat in not war.");
                return;
            }
            if (client.ActiveCharacter.ChatBanned)
            {
                client.ActiveCharacter.SendInfoMsg("You are Banned");
                return;
            }
            var locale = Asda2EncodingHelper.MinimumAvailableLocale(client.Locale, message);
            SendWarChatResponseResponse(client.ActiveCharacter.CurrentBattleGround, client.ActiveCharacter.Name, message,
                                        client.ActiveCharacter.Asda2FactionId,locale);
        }

        public static void SendWarChatResponseResponse(Asda2Battleground btlgrnd, string senderName, string message, int factionId, Locale locale)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.WarChatResponse)) //6719
            {
                packet.WriteByte(3); //value name : unk5 default value : 3Len : 1
                packet.WriteFixedAsciiString(senderName, 20); //{senderName}default value :  Len : 20
                packet.WriteFixedAsciiString(message, 200, locale); //{message}default value :  Len : 200
                btlgrnd.Send(packet, true, (short?) factionId, locale);
            }
        }

        public static void SendHowManyPeopleInWarTeamsResponse(Asda2Battleground btlgrnd,Character chr =null)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.HowManyPeopleInWarTeams)) //6742
            {
                packet.WriteInt16(btlgrnd.LightTeam.Count); //{light}default value : 20 Len : 2
                packet.WriteInt16(btlgrnd.DarkTeam.Count); //{dark}default value : 24 Len : 2
                if(chr!=null)
                    chr.Send(packet,addEnd: true);
                else 
                    btlgrnd.Send(packet, addEnd: true);
            }
        }

        public static void SendCharacterHasLeftWarResponse(Asda2Battleground btlgrnd, int leaverAccId, byte leaverWarId,
                                                           string leaverName, int factionId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterHasLeftWar)) //6721
            {
                packet.WriteByte(0); //value name : unk5 default value : 0Len : 1
                packet.WriteInt32(leaverAccId); //{accId}default value : -1 Len : 4
                packet.WriteByte(leaverWarId); //{warId}default value : 10 Len : 1
                packet.WriteFixedAsciiString(leaverName, 20); //{lefterName}default value :  Len : 20
                btlgrnd.Send(packet, true, (short?) factionId);
            }
        }

        #endregion

        //todo players on war info
        //todo dismiss player from war
        //todo change Capitan
        //todo ocupation triggers
        public static void SendWarCurrentActionInfoResponse(Asda2Battleground btlgrnd,
                                                            BattleGroundInfoMessageType status, Int16 value, Character chr = null,short? factionId =null)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.WarCurrentActionInfo)) //6728
            {
                packet.WriteByte((byte) status); //{status}default value : 8 Len : 1
                packet.WriteInt16(value); //{mins}default value : 0 Len : 2
                if (chr == null)
                    btlgrnd.Send(packet,addEnd: false,asda2FactionId: factionId);
                else
                    chr.Send(packet);
            }
        }

        public static void SendWarEndedResponse(IRealmClient client, byte winingFaction, int winingFactionPoints,
                                                int losserFactionPoints, int honorPoints, short honorCoin,
                                                long expReward, string mvpName)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.WarEnded)) //6732
            {
                packet.WriteInt16(winingFaction); //{winingFaction}default value : 1 Len : 2
                packet.WriteSkip(stab9); //value name : stab9 default value : stab9Len : 12
                packet.WriteInt32(winingFactionPoints); //{winingFactionPoints}default value : 911 Len : 4
                packet.WriteInt32(losserFactionPoints); //{losserFactionPoints}default value : 120 Len : 4
                packet.WriteInt32(honorPoints); //{HonorPoints}default value : 353 Len : 4
                packet.WriteInt16(honorCoin); //{honorCoin}default value : 17 Len : 2
                packet.WriteSkip(stab35); //value name : stab35 default value : stab35Len : 3
                packet.WriteInt64(expReward); //{expReward}default value : 10000 Len : 8
                packet.WriteSkip(stab46); //value name : stab46 default value : stab46Len : 16
                packet.WriteFixedAsciiString(mvpName, 20); //{mvpName}default value :  Len : 20
                client.Send(packet, addEnd: true);
            }
        }

        private static readonly byte[] stab9 = new byte[]
                                                   {0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

        private static readonly byte[] stab35 = new byte[] {0x00, 0xFF, 0xFF};

        private static readonly byte[] stab46 = new byte[]
                                                    {0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};

        public static void SendWarEndedOneResponse(IRealmClient client, IEnumerable<Asda2Item> prizeItems)
        {
            var items = new Asda2Item[4];
            var ind = 0;
            foreach (var asda2Item in prizeItems)
            {
                items[ind++] = asda2Item;
            }
            using (var packet = new RealmPacketOut(RealmServerOpCode.WarEndedOne)) //6733
            {
                packet.WriteByte(1); //value name : unk5 default value : 1Len : 1
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight); //{weight}default value : 10945 Len : 2
                packet.WriteInt32(client.ActiveCharacter.Money); //{money}default value : 5514558 Len : 4
                for (int i = 0; i < 4; i += 1)
                {
                    var item = items[i];
                    Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, false);
                }
                client.Send(packet, addEnd: true);
            }
        }

        [PacketHandler(RealmServerOpCode.ShowWarUnit)] //6717
        public static void ShowWarUnitRequest(IRealmClient client, RealmPacketIn packet)
        {
            if(!client.ActiveCharacter.IsAsda2BattlegroundInProgress)
                return;
            SendWarTeamListResponse(client.ActiveCharacter);

        }
        [PacketHandler(RealmServerOpCode.CancleWarPatipication)]//6708
        public static void CancleWarPatipicationRequest(IRealmClient client, RealmPacketIn packet)
        {
            SendWarPartipicationCanceledResponse(client);
            if(client.ActiveCharacter.CurrentBattleGround==null)
                return;
            client.ActiveCharacter.CurrentBattleGround.Leave(client.ActiveCharacter);
        }
        public static void SendWarPartipicationCanceledResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.WarPartipicationCanceled))//6709
            {
                packet.WriteByte(1);//value name : unk5 default value : 1Len : 1
                packet.WriteByte(client.ActiveCharacter.Asda2FactionId);//value name : unk1 default value : 1Len : 1
                client.Send(packet);
            }
        }


        public static void SendWarTeamListResponse(Character chr)
        {
            var btldrnd = chr.CurrentBattleGround;
            var chrsLists = new List<List<Character>>();
            lock (btldrnd.JoinLock)
            {
                if (chr.Asda2FactionId == 0)
                {
                    var listsCount = btldrnd.LightTeam.Count/6;
                    if (listsCount == 0)
                        listsCount = 1;
                    for (int i = 0; i < listsCount; i++)
                    {
                        chrsLists.Add(btldrnd.LightTeam.Values.Skip(i*6).Take(6).ToList());
                    }
                }
                else
                {
                    var listsCount = btldrnd.DarkTeam.Count / 6;
                    if (listsCount == 0)
                        listsCount = 1;
                    for (int i = 0; i < listsCount; i++)
                    {
                        chrsLists.Add(btldrnd.DarkTeam.Values.Skip(i * 6).Take(6).ToList());
                    }
                }
                foreach (var chrsList in chrsLists)
                {
                    using (var packet = new RealmPacketOut(RealmServerOpCode.WarTeamList)) //6715
                    {
                        foreach (var character in chrsList)
                        {
                            packet.WriteByte(character.CurrentBattleGroundId); //{warId}default value : 12 Len : 1
                            packet.WriteByte(character.Asda2FactionId); //{faction}default value : 1 Len : 1
                            packet.WriteInt16(character.SessionId); //{sessId}default value : 59 Len : 2
                            packet.WriteInt32(character.AccId); //{accId}default value : 162676 Len : 4
                            packet.WriteByte(character.CharNum); //{charNum}default value : 10 Len : 1
                            packet.WriteByte(3); //value name : unk10 default value : 3Len : 1
                            packet.WriteByte(character.Asda2FactionRank); //{warRank}default value : 1 Len : 1
                            packet.WriteInt16(character.Level); //{level}default value : 38 Len : 2
                            packet.WriteByte(character.ProfessionLevel); //{proff}default value : 2 Len : 1
                            packet.WriteByte((byte)character.Archetype.ClassId); //{class}default value : 3 Len : 1
                            packet.WriteByte(character.Guild == null ? 0 : character.Guild.ClanCrest == null ? 0 : 1); //{crestExists}default value : 1 Len : 1
                            packet.WriteSkip(character.Guild == null ? guildCrest : character.Guild.ClanCrest ?? guildCrest); //{guildCrest}default value : guildCrest Len : 40
                            packet.WriteInt16(character.IsInGroup ? 1 : -1); //{partyId}default value : -1 Len : 2
                            packet.WriteInt16(character.BattlegroundDeathes); //{deathes}default value : 0 Len : 2
                            packet.WriteInt16(character.BattlegroundKills); //{kills}default value : 0 Len : 2
                            packet.WriteInt16(character.BattlegroundActPoints); //{ActScores}default value : 1 Len : 2
                            packet.WriteFixedAsciiString(character.Name, 20); //{charName}default value :  Len : 20
                            packet.WriteFixedAsciiString(character.Guild == null ? "" : character.Guild.Name, 17); //{clanName}default value :  Len : 17
                            packet.WriteInt16((short)character.Asda2Position.X); //{x}default value : 257 Len : 2
                            packet.WriteInt16((short)character.Asda2Position.Y); //{y}default value : 165 Len : 2
                        }
                        chr.Send(packet,addEnd: false);
                    }
                }
                SendWarTeamListEndedResponse(chr.Client);
                
            }

        }

        private static readonly byte[] guildCrest = new byte[]
                                                        {
                                                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                            0x00, 0x00, 0x00, 0x00, 0x00
                                                        };
        public static void SendUpdatePointInfoResponse(IRealmClient client, Asda2WarPoint point)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.UpdatePointInfo))//6725
            {
                packet.WriteInt16(point.Id);//{pointNum}default value : 4 Len : 2
                packet.WriteInt16(point.X);//{pointX}default value : 209 Len : 2
                packet.WriteInt16(point.Y);//{pointY}default value : 284 Len : 2
                packet.WriteInt16(point.OwnedFaction);//{factionId}default value : 0 Len : 2
                packet.WriteInt16((short)point.Status);//{notOwned0CApturing1Owned2}default value : 1 Len : 2
                packet.WriteByte(0);//value name : unk1 default value : 0Len : 1
                point.BattleGround.Send(packet,addEnd: true);
            }
        }

        public static void SendWarPointsPreInitResponse(IRealmClient client,Asda2WarPoint point)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.WarPointsPreInit))//6726
            {
                packet.WriteInt16(point.Id);//{pointNum}default value : 4 Len : 2
                packet.WriteInt16(point.X);//{pointX}default value : 209 Len : 2
                packet.WriteInt16(point.Y);//{pointY}default value : 284 Len : 2
                packet.WriteInt16(point.OwnedFaction);//{factionId}default value : 0 Len : 2
                packet.WriteInt16((short) point.Status);//{notOwned0CApturing1Owned2}default value : 1 Len : 2
                packet.WriteByte(0);//value name : unk1 default value : 0Len : 1
                client.Send(packet,addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.StartOcupyPoint)]//6723
        public static void StartOcupyPointRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 2;
            var occupationPoint = packet.ReadByte();//default : 3Len : 1
            if(!client.ActiveCharacter.IsAsda2BattlegroundInProgress || client.ActiveCharacter.MapId!=MapId.BatleField)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to occupy point while not in war.",20);
                return;
            }
            var bgrnd = client.ActiveCharacter.CurrentBattleGround;
            if(occupationPoint>=bgrnd.Points.Count)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to occupy unknown war point.", 20);
                return;
            }
            var point = bgrnd.Points[occupationPoint];
            point.TryCapture(client.ActiveCharacter);
        }
        public static void SendOccupyingPointStartedResponse(IRealmClient client,short pointId,OcupationPointStartedStatus status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.OccupyingPointStarted))//6724
            {
                packet.WriteInt16(pointId);//{point}default value : 6 Len : 2
                packet.WriteByte((byte) status);//{factionId}default value : 1 Len : 1
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 361343 Len : 4
                client.ActiveCharacter.SendPacketToArea(packet,true,true);
            }
        }

        public static void SendWarTeamListEndedResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.WarTeamListEnded))//6716
            {
                client.Send(packet);
            }
        }
        [PacketHandler(RealmServerOpCode.DismissPlayerFromWar)]//6186
        public static void DismissPlayerFromWarRequest(IRealmClient client, RealmPacketIn packet)
        {
            var warID = packet.ReadInt16();//default : 6Len : 2
          //  var charACCiD = packet.ReadInt32();//default : 357812Len : 4
            
            if (!client.ActiveCharacter.IsAsda2BattlegroundInProgress)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to dissmis someone while not on war.",50);
                return;
            }
            var target = client.ActiveCharacter.CurrentBattleGround.GetCharacter(client.ActiveCharacter.Asda2FactionId,
                                                                                 (byte) warID);
            if(target == null)
            {
                client.ActiveCharacter.SendWarMsg("Target character not found.");
            }
            using (var p = new RealmPacketOut(RealmServerOpCode.DismissPlayerFromWarRequestResult))//6187
            {
                if (target == null || !client.ActiveCharacter.CurrentBattleGround.TryStartDissmisProgress(client.ActiveCharacter,target) || client.ActiveCharacter.Money < 10000)
                {
                    p.WriteByte(0);//{status}default value : 1 Len : 1
                    p.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);//{invWeight}default value : 10665 Len : 2
                    Asda2InventoryHandler.WriteItemInfoToPacket(p, null, false);
                }
                else
                {
                    
                    p.WriteByte(1);//{status}default value : 1 Len : 1
                    p.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);//{invWeight}default value : 10665 Len : 2
                    Asda2InventoryHandler.WriteItemInfoToPacket(p, client.ActiveCharacter.Asda2Inventory.GetRegularItem(0), false);
                    client.ActiveCharacter.SubtractMoney(10000);
                }
                client.Send(p, addEnd: true);
            }
        }

        public static void SendQuestionDismissPlayerOrNotResponse(Asda2Battleground client,Character initer,Character target)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.QuestionDismissPlayerOrNot))//6188
            {
                packet.WriteInt16(initer.SessionId);//{initerSessId}default value : 0 Len : 2
                packet.WriteInt16(initer.Asda2FactionId);//{factionId}default value : 1 Len : 2
                packet.WriteInt16(target.SessionId);//{targetSessId}default value : 15 Len : 2
                packet.WriteInt32(target.AccId);//{dismissTargetAccId}default value : 361343 Len : 4
                client.Send(packet,true,initer.Asda2FactionId);
            }
        }
        [PacketHandler(RealmServerOpCode.AnswerDismissPlayer)]//6189
        public static void AnswerDismissPlayerRequest(IRealmClient client, RealmPacketIn packet)
        {
            var isOk = packet.ReadByte()==1;//default : 0Len : 1
            if (!client.ActiveCharacter.IsAsda2BattlegroundInProgress)
            {
                client.ActiveCharacter.SendWarMsg("Player not found.");
                return;
            }
            client.ActiveCharacter.CurrentBattleGround.AnswerDismiss(isOk,client.ActiveCharacter);
        }
        public static void SendDissmissResultResponse(Asda2Battleground client,DismissPlayerResult status,short targetSessId,int targetAccId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.DissmissResult))//6190
            {
                packet.WriteByte((byte) status);//{status}default value : 0 Len : 1
                packet.WriteInt16(targetSessId);//{targetSessId}default value : 15 Len : 2
                packet.WriteInt32(targetAccId);//{accId}default value : 361343 Len : 4
                client.Send(packet,addEnd: true);
            }
        }


    }
    public enum DismissPlayerResult
    {
        Fail =0,
        Ok =1
    }
}