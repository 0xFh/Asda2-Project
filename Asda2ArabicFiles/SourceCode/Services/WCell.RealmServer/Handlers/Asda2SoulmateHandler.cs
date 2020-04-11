using System;
using System.Collections.Generic;
using System.IO;
using Cell.Core;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Network;
using WCell.RealmServer.Social;
using WCell.RealmServer.Spells.Auras;
using System.Linq;
using WCell.Util;
namespace WCell.RealmServer.Handlers
{
    public static class Asda2SoulmateHandler
    {
        public static void SendCharacterSoulMateIntrodactionUpdateResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterSoulMateIntrodactionUpdate)) //6080
            {
                packet.WriteInt32(-1); //value name : unk5 default value : -1Len : 4
                packet.WriteFixedAsciiString(client.ActiveCharacter.SoulmateIntroduction ?? "", 127);
                //{message}default value :  Len : 127
                packet.WriteByte(1); //value name : unk1 default value : 1Len : 1
                client.Send(packet, addEnd: true);
            }
        }

        [PacketHandler(RealmServerOpCode.CharacterSoulMateIntrodactionUpdate)] //6080
        public static void ModyifySoulmateIntroductionRequest(IRealmClient client, RealmPacketIn packet)
        {
            var message = packet.ReadAsdaString(127, Locale.Any); //default : Len : 127
            var locale = Asda2EncodingHelper.MinimumAvailableLocale(client.Locale, message);
            /*if (locale != Locale.En)
            {
                client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("Soulmate introduction.");
            }*/
            client.ActiveCharacter.SoulmateIntroduction = message;
        }

        [PacketHandler(RealmServerOpCode.FindSoulMateReq)]//5483
        public static void FindSoulMateReqRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 1;//nk8 default : 0Len : 1
            var page = packet.ReadByte();//default : 2Len : 1
            SendSoulmatesListResponse(client, page);
            Asda2TitleChecker.OnFindSoulmateWindowOpened(client.ActiveCharacter);
        }


        public static void SendSoulmatesListResponse(IRealmClient client, byte page)
        {
            var chrsOnMap = client.ActiveCharacter.Map.Characters.ToList();
            chrsOnMap.Remove(client.ActiveCharacter);
            var pagesCount = chrsOnMap.Count / 7;
            if (pagesCount == 0)
                pagesCount = 1;
            var chrs = chrsOnMap.Skip((page - 1) * 7).Take(7);
            foreach (var character in chrs)
            {
                using (var packet = new RealmPacketOut(RealmServerOpCode.SoulmatesList)) //5484
                {
                    packet.WriteByte(pagesCount); //{pages}default value : 1 Len : 1
                    packet.WriteByte(page); //{curPage}default value : 1 Len : 1
                    packet.WriteByte(character.Level); //{level}default value : 13 Len : 1
                    packet.WriteByte(character.ProfessionLevel); //{profLevel0}default value : 1 Len : 1
                    packet.WriteByte((byte)character.Archetype.ClassId); //{class}default value : 3 Len : 1
                    packet.WriteFixedAsciiString(character.Name, 20);
                    //{name}default value :  Len : 20
                    packet.WriteByte((byte)character.Gender); //{sex}default value : 1 Len : 1
                    packet.WriteInt16(character.SessionId); //{sessId}default value : 61 Len : 2
                    packet.WriteInt32(character.AccId); //{accNum}default value : 355081 Len : 4
                    packet.WriteByte(10); //{charNumOnAcc}default value : 10 Len : 1
                    var introLen = character.SoulmateIntroduction == null ? 0 : character.SoulmateIntroduction.Length;
                    packet.WriteByte(introLen);//{introLen}default value : 0 Len : 1
                    packet.WriteByte(character.Record.Zodiac); //{zodiac}default value : 0 Len : 1
                    if (introLen > 0)
                        packet.WriteAsciiString(character.SoulmateIntroduction);
                    client.Send(packet);
                }
            }
            SendSoulmatesListEndedResponse(client);
        }


        public static void SendSoulmatesListEndedResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SoulmatesListEnded)) //5485
            {
                client.Send(packet, addEnd: true);
            }
        }

        [PacketHandler(RealmServerOpCode.SoulmateRequest)] //5200
        public static void SoulmateRequestRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 7;
            var chr = client.ActiveCharacter;
            var targetSoulmateName = packet.ReadAsdaString(20, Locale.En); //default : Len : 17
            var targetChr = World.GetCharacter(targetSoulmateName, true);
            if (targetChr == null)
            {
                client.ActiveCharacter.SendSystemMessage(string.Format("Cant add soulmate cause not found by name {0}",
                                                                       targetSoulmateName));
                return;
            }
            //if (targetChr.Asda2FactionId != client.ActiveCharacter.Asda2FactionId)
            //{
            //    client.ActiveCharacter.SendSystemMessage(string.Format("Sorry ,but {0} is in other faction.", targetChr.Name));
            //    return;
            //}
            if (!targetChr.EnableSoulmateRequest)
            {
                client.ActiveCharacter.SendSystemMessage(string.Format("Sorry, but {0} rejects all soulmate requests.", targetSoulmateName));
                return;
            }
            if (chr.IsSoulmated)
            {
                SendSoulmateRequestResponseResponse(client, SoulmateRequestResponseResult.TargetAlreadyHasASoulmate, chr.AccId, chr.Name);
                return;
            }
            if (targetChr.IsSoulmated)
            {
                SendSoulmateRequestResponseResponse(client, SoulmateRequestResponseResult.TargetAlreadyHasASoulmate, targetChr.AccId, targetChr.Name);
                return;
            }
            //todo add ignoring logic
            SendSoulmateRequestResponseResponse(targetChr.Client, SoulmateRequestResponseResult.YouRecievingSoulmateRequest, chr.AccId, chr.Name, (byte)chr.Level, (byte)chr.Gender, (byte)chr.Archetype.ClassId, chr.SoulmateIntroduction);
        }

        public static void SendSoulmateRequestResponseResponse(IRealmClient client, SoulmateRequestResponseResult status, uint senderAccId = 0, string senderName = "",
                                                               byte senderLevel = 0, byte senderSex = 0, byte senderClass = 0,
                                                               string senderIntroductionMsg = "")
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SoulmateRequestResponse)) //5201
            {
                packet.WriteByte((byte)status); //value name : unk5 default value : 0Len : 1
                packet.WriteInt32(senderAccId); //{senderAccId}default value : 355929 Len : 4
                packet.WriteInt16(1); //value name : unk7 default value : 1Len : 2
                packet.WriteByte(0); //value name : unk8 default value : 0Len : 1
                packet.WriteByte(1); //value name : unk1 default value : 1Len : 1
                packet.WriteInt16(3); //value name : unk80 default value : 3Len : 2
                packet.WriteFixedAsciiString(senderName, 20); //{senderName}default value :  Len : 20
                packet.WriteByte(1); //value name : unk1 default value : 1Len : 1
                packet.WriteInt16(senderLevel); //{level}default value : 14 Len : 2
                packet.WriteByte(senderSex); //{sex}default value : 1 Len : 1
                packet.WriteByte(senderClass); //{class}default value : 1 Len : 1
                packet.WriteFixedAsciiString(senderIntroductionMsg, 128); //{introductionMsg}default value :  Len : 128
                client.Send(packet, addEnd: false);
            }
        }

        public static void SendYouHaveSoulmatedWithResponse(IRealmClient client, SoulmatingResult status,
                                                            uint soulmateId = 0, uint soulmateAccId = 0,
                                                            string soulmateName = null)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.YouHaveSoulmatedWith)) //5203
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                packet.WriteInt32(soulmateId); //{soulmateId}default value : 14895 Len : 4
                packet.WriteInt32(soulmateAccId); //{soulmateAccId}default value : 355929 Len : 4
                packet.WriteFixedAsciiString(soulmateName, 20); //{soulmateName}default value :  Len : 20
                packet.WriteByte(1); //value name : unk1 default value : 1Len : 1
                client.Send(packet, addEnd: false);
            }
        }

        [PacketHandler(RealmServerOpCode.AnswerSoulmateRequest)] //5202
        public static void AnswerSoulmateRequestRequest(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            packet.Position -= 8; //nk7 default : 0Len : 1
            var status = packet.ReadByte(); //default : 5Len : 1
            packet.Position += 10; //nk10 default : 0Len : 1
            var targetSoulmateName = packet.ReadAsdaString(20, Locale.En); //default : Len : 20

            var targetChr = World.GetCharacter(targetSoulmateName, true);
            if (targetChr == null)
            {
                client.ActiveCharacter.SendSystemMessage(string.Format("Cant add soulmate cause not found by name {0}",
                                                                       targetSoulmateName));
                return;
            }
            if (status == 1)
            {

                if (chr.IsSoulmated)
                {
                    SendSoulmateRequestResponseResponse(client, SoulmateRequestResponseResult.TargetAlreadyHasASoulmate, chr.AccId, chr.Name);
                    return;
                }
                if (targetChr.IsSoulmated)
                {
                    SendSoulmateRequestResponseResponse(client, SoulmateRequestResponseResult.TargetAlreadyHasASoulmate, targetChr.AccId, targetChr.Name);
                    return;
                }
                Asda2SoulmateMgr.CreateNewOrUpdateSoulmateRelation(chr, targetChr);
                Asda2TitleChecker.OnNewSoulmate(chr);
                Asda2TitleChecker.OnNewSoulmate(targetChr);

            }
            else
            {
                SendSoulmateRequestResponseResponse(targetChr.Client, SoulmateRequestResponseResult.TargetRefusedSoulmateRequest, chr.AccId, chr.Name);
            }
        }


        [PacketHandler(RealmServerOpCode.DisbandSoulMate)] //5204
        public static void DisbandSoulMateRequest(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            if (chr.IsSoulmated)
            {
                chr.SoulmateRecord.IsActive = false;
                chr.SoulmateRecord.UpdateCharacters();
                Asda2TitleChecker.OnNewSoulmatingEnd(chr);
            }
            else
            {
                SendDisbandSoulMateResultResponse(client, DisbandSoulmateResult.IsNoLongerYporSoulmate);
            }
        }

        public static void SendDisbandSoulMateResultResponse(IRealmClient client, DisbandSoulmateResult status,
                                                             string friendName = "friend")
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.DisbandSoulMateResult)) //5207
            {
                packet.WriteByte((byte)status); //{status}default value : 3 Len : 1
                packet.WriteInt32(client.ActiveCharacter.AccId); //{rcvAccId}default value : 340701 Len : 4
                packet.WriteFixedAsciiString(friendName, 20); //{friendName}default value :  Len : 20
                client.Send(packet, addEnd: false);
            }
        }

        public static void SendSoulmateLoggedOutResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SoulmateLoggedOut)) //5226
            {
                client.Send(packet, addEnd: false);
            }
        }

        public static void SendSoulmateEnterdGameResponse(IRealmClient client)
        {
            if (client == null || client.ActiveCharacter == null) return;
            var chr = client.ActiveCharacter;
            var soulmateChr = client.ActiveCharacter.SoulmateCharacter;
            if (soulmateChr == null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.SoulmateEnterdGame)) //5110
            {
                packet.WriteInt32((int)chr.SoulmateRecord.SoulmateRelationGuid);
                //{soulmateId}default value : 14895 Len : 4
                packet.WriteInt32(chr.SoulmateCharacter.AccId); //{soulmateAccId}default value : 355929 Len : 4
                packet.WriteInt16(chr.SoulmateCharacter.SessionId); //{soulmateSessId}default value : 85 Len : 2
                packet.WriteByte(0); //value name : unk8 default value : 0Len : 1
                packet.WriteByte(1); //value name : unk1 default value : 1Len : 1
                packet.WriteByte(10); //value name : unk10 default value : 10Len : 1
                packet.WriteInt32(chr.SoulmateCharacter.MaxHealth); //{soulmateMaxHp}default value : 503 Len : 4
                packet.WriteInt32(chr.SoulmateCharacter.Health); //{soulmateHp}default value : 503 Len : 4
                packet.WriteInt16(chr.SoulmateCharacter.MaxPower); //{soulmateMaxMp}default value : 152 Len : 2
                packet.WriteInt16(chr.SoulmateCharacter.Power); //{soulmateMp}default value : 152 Len : 2
                packet.WriteInt16((short)chr.Asda2X); //{x}default value : 295 Len : 2
                packet.WriteInt16((short)chr.Asda2Y); //{y}default value : 235 Len : 2
                packet.WriteByte(chr.SoulmateCharacter.Level); //{solmateLevel}default value : 14 Len : 1
                packet.WriteByte(132); //value name : unk18 default value : 132Len : 1
                packet.WriteByte(111); //value name : unk19 default value : 111Len : 1
                packet.WriteSkip(unk20); //value name : unk20 default value : unk20Len : 6
                packet.WriteInt16((byte)soulmateChr.MapId); //value name : unk21 default value : 1Len : 2
                packet.WriteByte(1); //value name : unk22 default value : 1Len : 1
                packet.WriteByte(1); //value name : unk23 default value : 1Len : 1
                packet.WriteInt64((long)chr.SoulmateRecord.Expirience); //{soulmateExp}default value : 0 Len : 8
                packet.WriteByte(chr.SoulmateRecord.Level); //{soulmatingLevel}default value : 1 Len : 1
                packet.WriteSkip(unk26); //value name : unk26 default value : unk26Len : 12
                packet.WriteFixedAsciiString(chr.SoulmateCharacter.Name, 20); //{soulmateName}default value :  Len : 20

                var auras = chr.SoulmateCharacter.Auras.VisibleAuras;
                for (int i = 0; i < 28; i += 1)
                {
                    packet.WriteByte(auras[i] == null ? 0 : 1);//exist?
                    packet.WriteByte(0);//spadaet?
                    packet.WriteInt32(auras[i] == null ? -1 : auras[i].Spell.RealId);//{duration}default value : -1 Len : 4
                }
                client.Send(packet, addEnd: false);
            }
        }

        private static readonly byte[] unk20 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        private static readonly byte[] unk26 = new byte[]
                                                   {
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                       0x00
                                                   };

        public static void SendSoulMateHpMpUpdateResponse(IRealmClient client)
        {
            if (client == null || client.ActiveCharacter == null)
                return;
            var soulmateChr = client.ActiveCharacter.SoulmateCharacter;
            if (soulmateChr == null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.SoulMateHpMpUpdate)) //5111
            {
                packet.WriteInt32(client.ActiveCharacter.MaxHealth); //{MaxHealth}default value : 503 Len : 4
                packet.WriteInt32(client.ActiveCharacter.Health); //{Health}default value : 503 Len : 4
                packet.WriteInt16(client.ActiveCharacter.MaxPower); //{maxMana}default value : 152 Len : 2
                packet.WriteInt16(client.ActiveCharacter.Power); //{mana}default value : 152 Len : 2
                packet.WriteInt16((short)client.ActiveCharacter.Asda2X); //{x}default value : 295 Len : 2
                packet.WriteInt16((short)client.ActiveCharacter.Asda2Y); //{y}default value : 235 Len : 2
                packet.WriteByte(client.ActiveCharacter.Level); //{level}default value : 14 Len : 1
                packet.WriteInt64(client.ActiveCharacter.Experience);//{exp}default value : 66 Len : 8
                packet.WriteByte(0);//{chanel}default value : 3 Len : 1
                packet.WriteByte((byte)soulmateChr.MapId);//{map}default value : 3 Len : 1
                packet.WriteInt16(0);//value name : unk15 default value : 0Len : 2
                packet.WriteInt64((long)client.ActiveCharacter.SoulmateRecord.Expirience);//{friendExp}default value : 47 Len : 8
                packet.WriteInt64(client.ActiveCharacter.SoulmateRecord.Level);//{frienshilLevel}default value : 2 Len : 8
                packet.WriteSkip(stab52);//value name : stab52 default value : stab52Len : 5
                soulmateChr.Send(packet, addEnd: true);
            }
        }
        static readonly byte[] stab52 = new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00 };

        public static void SendSoulmateBuffUpdateInfoResponse(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SoulmateBuffUpdateInfo))//5112
            {
                var auras = new Aura[28];
                var it = 0;
                foreach (var visibleAura in chr.Auras.ActiveAuras)
                {
                    if (visibleAura.TicksLeft <= 0)
                        continue;
                    auras[it++] = visibleAura;
                    if (auras.Length <= it)
                        break;
                }
                for (int i = 0; i < 28; i += 1)
                {
                    var aura = auras[i];
                    packet.WriteByte(aura == null ? 0 : 1);//exist?
                    packet.WriteByte(0);//spadaet?
                    packet.WriteInt32(aura == null ? -1 : aura.Spell.RealId);//{duration}default value : -1 Len : 4
                }
                chr.SoulmateCharacter.Send(packet, addEnd: false);
            }
        }


        public static void SendSoulmatePositionResponse(IRealmClient client)
        {
            if (client == null || client.ActiveCharacter == null) return;
            var soulmateChr = client.ActiveCharacter.SoulmateCharacter;
            if (soulmateChr == null || soulmateChr.SoulmateRecord == null || client.ActiveCharacter == null)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.SoulmatePosition)) //5223
            {
                packet.WriteInt32((int)soulmateChr.SoulmateRecord.SoulmateRelationGuid);//{soulmateId}default value : 14895 Len : 4
                packet.WriteInt32(client.ActiveCharacter.AccId); //{soulmateAccId}default value : 355929 Len : 4
                packet.WriteInt16((short)client.ActiveCharacter.Asda2X); //{x}default value : 90 Len : 2
                packet.WriteInt16((short)client.ActiveCharacter.Asda2Y); //{y}default value : 243 Len : 2
                packet.WriteInt16((short)client.ActiveCharacter.Asda2Y); //{y}default value : 243 Len : 2
                client.ActiveCharacter.SoulmateCharacter.Send(packet, addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.PreviousSoulmateInfo)]//2244
        public static void PreviousSoulmateInfoRequest(IRealmClient client, RealmPacketIn packet)
        {
            SendPreviousSoulmateInfoResResponse(client);
            Asda2TitleChecker.OnSoulmateInfoRequest(client.ActiveCharacter);
        }


        public static void SendPreviousSoulmateInfoResResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.PreviousSoulmateInfoRes))//2243
            {
                var soulmateRecords = Asda2SoulmateMgr.GetSoulmateRecords(client.ActiveCharacter.AccId);
                var i = 0;
                foreach (var rec in soulmateRecords)
                {
                    if (i >= 3)
                        break;
                    var relChrs = CharacterRecord.FindAllByProperty("AccountId", rec.AccId == client.ActiveCharacter.AccId
                                                                    ? (int)rec.RelatedAccId
                                                                    : (int)rec.AccId);

                    for (int j = 0; j < 3; j++)
                    {
                        var relChr = relChrs.Get(j);
                        packet.WriteInt32((int)rec.SoulmateRelationGuid);//{soulmateId}default value : 14895 Len : 4
                        packet.WriteByte(rec.Level);//{soulmatingLevel}default value : 1 Len : 1
                        packet.WriteByte(relChr == null ? 0 : relChr.Level);//{soulmateLvl}default value : 16 Len : 1
                        packet.WriteByte(relChr == null ? 0 : relChr.ProfessionLevel);//{proffNum}default value : 1 Len : 1
                        packet.WriteByte((byte)(relChr == null ? 0 : relChr.Class));//{classId}default value : 1 Len : 1
                        packet.WriteFixedAsciiString(relChr == null ? "" : relChr.Name, 20);//{name}default value :  Len : 20
                    }
                    i++;
                }
                for (; i < 3; i += 1)
                {
                    packet.WriteInt32(0);//{soulmateId}default value : 14895 Len : 4
                    packet.WriteByte(0);//{soulmatingLevel}default value : 1 Len : 1
                    packet.WriteByte(0);//{soulmateLvl}default value : 16 Len : 1
                    packet.WriteByte(0);//{proffNum}default value : 1 Len : 1
                    packet.WriteByte(0);//{classId}default value : 1 Len : 1
                    packet.WriteFixedAsciiString("", 20);//{name}default value :  Len : 20
                    packet.WriteInt32(0);//{soulmateId}default value : 14895 Len : 4
                    packet.WriteByte(0);//{soulmatingLevel}default value : 1 Len : 1
                    packet.WriteByte(0);//{soulmateLvl}default value : 16 Len : 1
                    packet.WriteByte(0);//{proffNum}default value : 1 Len : 1
                    packet.WriteByte(0);//{classId}default value : 1 Len : 1
                    packet.WriteFixedAsciiString("", 20);//{name}default value :  Len : 20
                    packet.WriteInt32(0);//{soulmateId}default value : 14895 Len : 4
                    packet.WriteByte(0);//{soulmatingLevel}default value : 1 Len : 1
                    packet.WriteByte(0);//{soulmateLvl}default value : 16 Len : 1
                    packet.WriteByte(0);//{proffNum}default value : 1 Len : 1
                    packet.WriteByte(0);//{classId}default value : 1 Len : 1
                    packet.WriteFixedAsciiString("", 20);//{name}default value :  Len : 20

                }
                client.Send(packet, addEnd: true);
            }
        }
        public static void SendUpdateFriendShipPointsResponse(Character chr)
        {
            if (!chr.IsSoulmated)
                return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.UpdateFriendShipPoints))//6047
            {
                packet.WriteInt16(chr.SessionId);//{mySessId}default value : 4 Len : 2
                packet.WriteByte(chr.FriendShipPoints);//{points}default value : 48 Len : 1
                chr.Client.Send(packet, addEnd: false);
            }
        }
        public static void SendSoulMateInfoInitResponse(Character rcv, bool showFriendWindow)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SoulMateInfoInit))//5220
            {
                packet.WriteInt32(rcv.AccId);//{rcvAccId}default value : 354889 Len : 4
                packet.WriteInt16(rcv.SessionId);//{rcvSessId}default value : 5 Len : 2
                packet.WriteInt32((uint)rcv.SoulmateRecord.SoulmateRelationGuid);//{friendRecordId}default value : 15102 Len : 4
                packet.WriteInt64((long)rcv.SoulmateRecord.Expirience);//{friendExo}default value : 0 Len : 8
                packet.WriteByte(rcv.SoulmateRecord.Level);//{friendLevel}default value : 1 Len : 1
                packet.WriteInt32(rcv.SoulmateRecord.FriendAccId(rcv.AccId));//{friendAccId}default value : 340701 Len : 4
                packet.WriteInt16(rcv.SoulmateCharacter == null ? 0 : rcv.SoulmateCharacter.SessionId);//{friendSessId}default value : 26 Len : 2
                packet.WriteInt16(1);//value name : unk12 default value : 1Len : 2
                packet.WriteByte(1);//value name : unk13 default value : 1Len : 1
                packet.WriteByte(3);//value name : unk14 default value : 3Len : 1
                packet.WriteByte(2);//value name : unk15 default value : 2Len : 1
                packet.WriteByte(2);//value name : unk16 default value : 2Len : 1
                packet.WriteInt16(10);//{charNumOnAcc}default value : 12 Len : 2
                packet.WriteInt32(showFriendWindow ? 1 : 0);//value name : showSoulmateWindow default value : 1Len : 4
                packet.WriteInt16(0);//value name : unk19 default value : 0Len : 2
                packet.WriteFixedAsciiString("", 20);//{nick1}default value :  Len : 20
                packet.WriteFixedAsciiString("", 20);//{nick2}default value :  Len : 20
                packet.WriteFixedAsciiString("", 20);//{nick3}default value :  Len : 20
                packet.WriteInt16(1);//value name : unk23 default value : 1Len : 2
                rcv.Send(packet, addEnd: false);
            }
        }
        public static void SendAppleExpGainedResponse(Character c)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.AppleExpGained))//6049
            {
                packet.WriteInt16(c.SessionId);//{mySessId}default value : 44 Len : 2
                packet.WriteInt32(c.SoulmateRecord.ApplePoints);//{exp}default value : 1 Len : 4
                packet.WriteByte(c.SoulmateRecord.ApplePoints);//value name : unk1 default value : 1Len : 1
                c.Send(packet, addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.EatApple)]//6575
        public static void EatAppleRequest(IRealmClient client, RealmPacketIn packet)
        {
            var soulmateRecord = client.ActiveCharacter.SoulmateRecord;
            if (client.ActiveCharacter.EatingAppleStep == 0)
            {
                lock (soulmateRecord)
                {
                    if (soulmateRecord.ApplePoints != 100)
                    {
                        SendAppleEatResultResponse(client, AppleEatResult.Fail, AppleBonusType.Item, 0);
                        return;
                    }
                    soulmateRecord.ApplePoints = 0;
                    Asda2TitleChecker.OnSoulmateEatApple(client.ActiveCharacter);
                }
                client.ActiveCharacter.EatingAppleStep = 1;
                SendAppleRandomResponse(client, 2, 1);
            }
            else if (client.ActiveCharacter.EatingAppleStep == 1)
            {
                client.ActiveCharacter.EatingAppleStep = 2;
                SendAppleRandomResponse(client, (byte)Utility.Random(0, 3), 1);
            }
        }

        public static void SendAppleRandomResponse(IRealmClient client, byte sector, byte recordId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.AppleRandom))//6576
            {
                packet.WriteByte(1);//{status}default value : 1 Len : 1
                packet.WriteByte(sector);//{sector}default value : 2 Len : 1
                packet.WriteByte(recordId);//{recordId}default value : 1 Len : 1
                client.Send(packet, addEnd: true);
            }
        }
        [PacketHandler(RealmServerOpCode.GetApplePrize)]//6579
        public static void GetApplePrizeRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter == null || client.ActiveCharacter.SoulmateCharacter == null)
                return;
            if (client.ActiveCharacter.EatingAppleStep == 2)
            {
                client.ActiveCharacter.EatingAppleStep = 0;
                var sumLvl = client.ActiveCharacter.Level + client.ActiveCharacter.SoulmateCharacter.Level;
                var exp = CharacterFormulas.CalcFriendAppleExp(sumLvl, client.ActiveCharacter.SoulmateRecord.Level);
                var exp1 = exp * client.ActiveCharacter.Level / sumLvl;
                client.ActiveCharacter.GainXp(exp1, "apple");
                client.ActiveCharacter.Health = client.ActiveCharacter.MaxHealth;
                client.ActiveCharacter.Power = client.ActiveCharacter.Power;
                SendAppleEatResultResponse(client, AppleEatResult.Ok, AppleBonusType.Exp, exp1);
                var exp2 = exp * client.ActiveCharacter.SoulmateCharacter.Level / sumLvl;
                if (client.ActiveCharacter.SoulmateCharacter == null)
                    return;
                client.ActiveCharacter.SoulmateCharacter.GainXp(exp2, "apple");
                client.ActiveCharacter.SoulmateCharacter.Health = client.ActiveCharacter.MaxHealth;
                client.ActiveCharacter.SoulmateCharacter.Power = client.ActiveCharacter.Power;
                SendAppleEatResultResponse(client, AppleEatResult.Ok, AppleBonusType.Exp, exp2);
            }
            else
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to get apple prize from [I DONT KNOW].", 20);
            }
        }
        public static void SendAppleEatResultResponse(IRealmClient client, AppleEatResult status, AppleBonusType type, int expAmount, Asda2Item bonusItem = null)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.AppleEatResult))//6580
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 6626 Len : 4
                packet.WriteByte((byte)type);//{type}default value : 2 Len : 1
                packet.WriteInt32(expAmount);//{amount}default value : 0 Len : 4
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, bonusItem);
                packet.WriteSkip(stub67);//{stub67}default value : stub67 Len : 6
                client.Send(packet, addEnd: true);
            }
        }
        static readonly byte[] stub67 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };


        [PacketHandler(RealmServerOpCode.SendSoulmateMessage)]//6052
        public static void SendSoulmateMessageRequest(IRealmClient client, RealmPacketIn packet)
        {
            var msg = packet.ReadAsciiString(client.Locale);//default : Len : 0
            if (client.ActiveCharacter.SoulmateCharacter != null && client.ActiveCharacter.SoulmateCharacter.Client.IsConnected)
            {
                var locale = Asda2EncodingHelper.MinimumAvailableLocale(client.Locale, msg);
                if (locale != Locale.En && client.ActiveCharacter.SoulmateCharacter.Client.Locale != client.Locale)
                {
                    client.ActiveCharacter.SendInfoMsg("You friend client accepts only english."); return;
                }
                //if (client.ActiveCharacter.ChatBanned)
                //{
                //    client.ActiveCharacter.SendInfoMsg("you are banned");
                //    return;

                //}
               
                SendIncomingSoulmateMessageResponse(client.ActiveCharacter.SoulmateCharacter, msg, locale);
                SendSoulmatemessageSendedResponse(client);
                Asda2TitleChecker.OnSoulmateMessage(client.ActiveCharacter);
            }
            else
            {
                client.ActiveCharacter.SendInfoMsg("Sry we can't send message to your soulmate while he is offline. Use Regular mail please.");
            }
        }

        public static void SendSoulmatemessageSendedResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SoulmatemessageSended))//6053
            {
                packet.WriteInt32(-1);//value name : unk5 default value : -1Len : 4
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{sessId}default value : 18 Len : 2
                packet.WriteInt32(client.ActiveCharacter.AccId);//{accId}default value : 6626 Len : 4
                packet.WriteByte((byte)client.ActiveCharacter.MapId);//{map}default value : 3 Len : 1
                packet.WriteInt32(2221);//value name : unk9 default value : 2221Len : 4
                packet.WriteFixedAsciiString("", 85);//{message}default value :  Len : 85
                client.Send(packet);
            }
        }

        public static void SendIncomingSoulmateMessageResponse(Character c, string msg, Locale locale)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.IncomingSoulmateMessage))//6055
            {
                packet.WriteInt32(c.AccId);//{accId}default value : 6319 Len : 4
                packet.WriteInt32(Util.Utility.Random(0, 10000));//{MessageId}default value : 2221 Len : 4
                packet.WriteFixedAsciiString(msg, 65, locale);//{msg}default value :  Len : 65
                packet.WriteInt32(0);//{year}default value : 2012 Len : 4
                packet.WriteInt32(0);//{mounth}default value : 10 Len : 4
                packet.WriteInt32(0);//{day}default value : 12 Len : 4
                packet.WriteInt32(0);//{hour}default value : 14 Len : 4
                packet.WriteInt32(0);//{minute}default value : 52 Len : 4
                c.Send(packet);
            }
        }

        #region skills

        [PacketHandler(RealmServerOpCode.UseSoulmateSkill)]//5231
        public static void UseSoulmateSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            var skillId = packet.ReadInt16();//default : 32Len : 2
            packet.Position += 112;//nk9 default : unk9Len : 112
            var targetSessId = packet.ReadInt16();//default : 5Len : 2
            if (client.ActiveCharacter.SoulmateCharacter == null)
            {
                SendSoulmateSkillUsedResponse(client, UseSoulmateSkillStatus.FriendNotInGame, 0, 0); return;
            }
            if (client.ActiveCharacter.IsDead)
            {
                SendSoulmateSkillUsedResponse(client, UseSoulmateSkillStatus.YouAreDead, 0, 0); return;
            }
            var soulmateRec = client.ActiveCharacter.SoulmateRecord;
            if (!soulmateRec.Skills.ContainsKey((Asda2SoulmateSkillId)skillId))
            {
                SendSoulmateSkillUsedResponse(client, UseSoulmateSkillStatus.Fail, 0, 0); return;
            }
            var skill = soulmateRec.Skills[(Asda2SoulmateSkillId)skillId];
            if (skill.TryCast(client.ActiveCharacter, client.ActiveCharacter.SoulmateCharacter))
            {
                SendSoulmateSkillCastResponse(client.ActiveCharacter, targetSessId, skillId);
                SendSoulmateSkillUsedResponse(client, UseSoulmateSkillStatus.Ok, skillId, targetSessId);
                SendSoulmateSkillUsedResponse(client.ActiveCharacter.SoulmateCharacter.Client, UseSoulmateSkillStatus.Ok, skillId, targetSessId);
                if (skill.Id == Asda2SoulmateSkillId.Heal || skill.Id == Asda2SoulmateSkillId.Heal1 || skill.Id == Asda2SoulmateSkillId.Heal2)
                    Asda2TitleChecker.OnSoulmateHealing(client.ActiveCharacter);
            }
            else
            {
                SendSoulmateSkillUsedResponse(client, UseSoulmateSkillStatus.Fail, 0, 0);
                return;
            }
        }

        public static void SendSoulmateSkillCastResponse(Character caster, short targetSessId, short skillId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SoulmateSkillCast))//5233
            {
                packet.WriteInt16(caster.SessionId);//{casterSessId}default value : 5 Len : 2
                packet.WriteInt16(skillId);//{skillId}default value : 45 Len : 2
                packet.WriteSkip(unk7);//value name : unk7 default value : unk7Len : 112
                packet.WriteInt16(targetSessId);//{targetSessId}default value : 10 Len : 2
                packet.WriteInt16(targetSessId);//{targetSessId0}default value : 10 Len : 2
                packet.WriteSkip(unk10);//value name : unk10 default value : unk10Len : 58
                packet.WriteByte((byte)caster.MapId);//{mapId}default value : 3 Len : 1
                packet.WriteSkip(stab186);//value name : stab186 default value : stab186Len : 17
                caster.SendPacketToArea(packet, true, true);
            }
        }
        static readonly byte[] unk7 = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        static readonly byte[] unk10 = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        static readonly byte[] stab186 = new byte[] { 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public static void SendSoulmateSkillUsedResponse(IRealmClient client, UseSoulmateSkillStatus status, short skillId, short targetSessId)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SoulmateSkillUsed))//5234
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteInt16(client.ActiveCharacter.SessionId);//{userSessId}default value : 5 Len : 2
                packet.WriteInt16(skillId);//{skillId}default value : 32 Len : 2
                packet.WriteSkip(unk8);//value name : unk8 default value : unk8Len : 112
                packet.WriteInt16(targetSessId);//{targetSessId}default value : 5 Len : 2
                packet.WriteInt16(targetSessId);//{targetSessId0}default value : 5 Len : 2
                packet.WriteSkip(unk11);//value name : unk11 default value : unk11Len : 58
                packet.WriteByte((byte)client.ActiveCharacter.MapId);//{map}default value : 3 Len : 1
                packet.WriteSkip(stub180);//{stub180}default value : stub180 Len : 17
                client.Send(packet, addEnd: true);
                if (client.ActiveCharacter.SoulmateCharacter != null)
                    client.ActiveCharacter.SoulmateCharacter.Send(packet, addEnd: true);
            }
        }
        static readonly byte[] unk8 = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        static readonly byte[] unk11 = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        static readonly byte[] stub180 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public static void SendSoulmateSummoningYouResponse(Character summoner, Character friend)
        {
            if (summoner == null || friend == null) return;

            friend.CanTeleportToFriend = true;
            friend.TargetSummonMap = summoner.Map.MapId;
            friend.TargetSummonPosition = summoner.Position;
            friend.SendInfoMsg(string.Format("{0} is summoning you to {1} [{2},{3}].", summoner.Name, summoner.TargetSummonMap, summoner.Asda2Position.X, summoner.Asda2Position.Y));
            friend.SendInfoMsg("Write #acpts to accept summon request or click OK.");
            using (var packet = new RealmPacketOut(RealmServerOpCode.SoulmateSummoningYou))//5236
            {
                packet.WriteInt32(summoner.AccId);//{friendAccId}default value : 6626 Len : 4
                packet.WriteInt16(summoner.SessionId);//{friendSessId}default value : 5 Len : 2
                packet.WriteByte(0);//value name : unk1 default value : 0Len : 1
                packet.WriteInt16((byte)summoner.MapId);//{frinedMap}default value : 3 Len : 2
                packet.WriteByte(1);//{friendChanel}default value : 3 Len : 1
                if (friend.Client.AddrTemp.Contains("192.168."))
                    packet.WriteFixedAsciiString(RealmServerConfiguration.ExternalAddress, 16);
                else
                    packet.WriteFixedAsciiString(RealmServerConfiguration.RealExternalAddress, 16);//{serverIp}default value :  Len : 16
                packet.WriteInt16(RealmServer.Instance.Port);//{serverPort}default value : 15604 Len : 2
                packet.WriteInt16((short)summoner.Asda2Position.X);//{x}default value : 96 Len : 2
                packet.WriteInt16((short)summoner.Asda2Position.Y);//{y}default value : 253 Len : 2
                packet.WriteInt32(friend.AccId);//{myAccId}default value : 6319 Len : 4
                packet.WriteInt16(friend.SessionId);//{mySessId}default value : 50 Len : 2
                packet.WriteByte(0);//value name : unk16 default value : 0Len : 1
                packet.WriteInt16((byte)friend.MapId);//{myMap}default value : 3 Len : 2
                packet.WriteByte(1);//{myCh}default value : 3 Len : 1
                friend.Send(packet);
            }
        }

        [PacketHandler(RealmServerOpCode.TeleportToSoulmateAnswer)]//5427
        public static void TeleportToSoulmateAnswerRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.CanTeleportToFriend)
            {
                client.ActiveCharacter.CanTeleportToFriend = false;
                client.ActiveCharacter.TeleportTo(client.ActiveCharacter.TargetSummonMap, client.ActiveCharacter.TargetSummonPosition);
            }

        }



        #endregion
    }
    public enum AppleBonusType
    {
        Exp = 1,
        Item = 2
    }
    public enum AppleEatResult
    {
        Ok = 1,
        Fail = 10,
    }
    public enum UseSoulmateSkillStatus
    {
        Fail = 0,
        Ok = 1,
        NoFriend = 2,
        FriendNotInGame = 3,
        FriendNakazan = 4,
        FriendCantUseThisSkill = 5,
        CantUseOnThisMap = 6,
        TooFarFromFriend = 7,
        NotEnoughtMana = 8,
        WrongFriendGender = 9,
        ThisMonsterIsAlreadyDead = 11,
        CantUseWhileMove = 12,
        YouAreDead = 13,
        FriendDead = 14,
        Cooldown = 16,
    }
    public abstract class Asda2SoulmateSkill
    {
        public virtual bool TryCast(Character caster, Character friend)
        {
            if (caster.SoulmateRecord.Level < Level)
                return false;
            if (DateTime.Now < ReadyTime)
                return false;
            Action(caster, friend);
            ReadyTime = DateTime.Now.AddSeconds(CooldownTimeSecs);
            return true;
        }

        protected int CooldownTimeSecs { get; set; }

        public DateTime ReadyTime { get; set; }
        public Asda2SoulmateSkillId Id { get; set; }
        public byte Level { get; set; }
        public virtual void Action(Character caster, Character friend)
        {

        }

        protected Asda2SoulmateSkill(Asda2SoulmateSkillId id, byte level, int cooldownTimeSecs)
        {
            Id = id;
            Level = level;
            CooldownTimeSecs = cooldownTimeSecs;
            ReadyTime = DateTime.MinValue;
        }
    }
    public class Asda2SoulmateSkillCall : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillCall()
            : base(Asda2SoulmateSkillId.Call, 3, 1200)
        {
        }
        public override bool TryCast(Character caster, Character friend)
        {
            if (caster.IsAsda2BattlegroundInProgress || friend.IsAsda2BattlegroundInProgress)
                return false;
            return base.TryCast(caster, friend);
        }
        public override void Action(Character caster, Character friend)
        {
            Asda2SoulmateHandler.SendSoulmateSummoningYouResponse(caster, friend);
            base.Action(caster, friend);
        }
    }
    public class Asda2SoulmateSkillTeleport : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillTeleport()
            : base(Asda2SoulmateSkillId.Call, 20, 1200)
        {
        }
        public override bool TryCast(Character caster, Character friend)
        {
            if (caster.IsAsda2BattlegroundInProgress || friend.IsAsda2BattlegroundInProgress)
                return false;
            return base.TryCast(caster, friend);
        }
        public override void Action(Character caster, Character friend)
        {
            caster.TeleportTo(friend);
            base.Action(caster, friend);
        }
    }
    public class Asda2SoulmateSkillHeal : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillHeal()
            : base(Asda2SoulmateSkillId.Heal, 5, 60)
        {
        }
        public override bool TryCast(Character caster, Character friend)
        {
            if (caster.GetDistance(friend) > 40 || friend.HealthPct >= 90 || friend.IsDead)
                return false;
            return base.TryCast(caster, friend);
        }
        public override void Action(Character caster, Character friend)
        {
            friend.HealPercent(50);
            base.Action(caster, friend);
        }
    }
    public class Asda2SoulmateSkillEmpower : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillEmpower()
            : base(Asda2SoulmateSkillId.Empower, 5, 60)
        {
        }
        public override bool TryCast(Character caster, Character friend)
        {
            if (caster.GetDistance(friend) > 40 || friend.IsDead)
                return false;
            return base.TryCast(caster, friend);
        }
        public override void Action(Character caster, Character friend)
        {
            caster.AddFriendEmpower(false);
            friend.AddFriendEmpower(true);
            base.Action(caster, friend);
        }
    }
    public class Asda2SoulmateSkillSoulSave : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillSoulSave()
            : base(Asda2SoulmateSkillId.SoulSave, 10, 1200)
        {
        }
        public override bool TryCast(Character caster, Character friend)
        {
            if (caster.GetDistance(friend) > 40 || friend.IsDead || caster.IsSoulmateSoulSaved || friend.IsSoulmateSoulSaved)
                return false;
            return base.TryCast(caster, friend);
        }
        public override void Action(Character caster, Character friend)
        {
            friend.IsSoulmateSoulSaved = true;
            friend.SendInfoMsg("Your friend saved your soul.");
            base.Action(caster, friend);
        }
    }
    public class Asda2SoulmateSkillResurect : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillResurect()
            : base(Asda2SoulmateSkillId.SoulSave, 18, 600)
        {
        }
        public override bool TryCast(Character caster, Character friend)
        {
            if (caster.GetDistance(friend) > 40 || friend.IsAlive)
                return false;
            return base.TryCast(caster, friend);
        }
        public override void Action(Character caster, Character friend)
        {
            friend.Resurrect();
            base.Action(caster, friend);
        }
    }
    public class Asda2SoulmateSkillSoulSong : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillSoulSong()
            : base(Asda2SoulmateSkillId.Empower, 30, 43200)
        {
        }
        public override bool TryCast(Character caster, Character friend)
        {
            if (caster.GetDistance(friend) > 40 || friend.IsDead)
                return false;
            return base.TryCast(caster, friend);
        }
        public override void Action(Character caster, Character friend)
        {
            caster.AddSoulmateSong();
            friend.AddSoulmateSong();
            base.Action(caster, friend);
        }
    }
    public enum Asda2SoulmateSkillId
    {
        Call = 32,//1200 3
        Heal = 33,//60 5
        Empower = 34,//60 7
        SoulSave = 35,//1200 10
        Heal1 = 37,
        Resurect = 39,//600 18
        Teleport = 40,//1200 20
        Heal2 = 42,
        Empower1 = 43,
        SoulSong = 44//43200 30
    }
    public enum DisbandSoulmateResult
    {
        HasDeniedYourRequest = 0,
        IsNoLongerYporSoulmate = 1,
        CantAgreeCauseLoggedOut = 2,
        SoulmateReleased = 3,
    }

    public enum SoulmatingResult
    {
        YouWillFindSomeOneBetter = 0,
        Ok = 1,

    }

    public enum SoulmateRequestResponseResult
    {
        YouRecievingSoulmateRequest = 0,
        TargetRefusedSoulmateRequest = 1,
        TargetAlreadyHasASoulmate = 2,
        TargetCantBeYourSoulmateNow = 3,

    }

}