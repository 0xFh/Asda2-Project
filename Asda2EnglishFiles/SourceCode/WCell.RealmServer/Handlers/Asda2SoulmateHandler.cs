using Castle.ActiveRecord;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.RealmServer.Social;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
    public static class Asda2SoulmateHandler
    {
        private static readonly byte[] unk20 = new byte[6];
        private static readonly byte[] unk26 = new byte[12];

        private static readonly byte[] stab52 = new byte[5]
        {
            (byte) 2,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] stub67 = new byte[6];

        private static readonly byte[] unk7 = new byte[112]
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
            (byte) 0
        };

        private static readonly byte[] unk10 = new byte[58]
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

        private static readonly byte[] stab186 = new byte[17]
        {
            (byte) 3,
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
            (byte) 0
        };

        private static readonly byte[] unk8 = new byte[112]
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
            (byte) 0
        };

        private static readonly byte[] unk11 = new byte[58]
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

        private static readonly byte[] stub180 = new byte[17];

        public static void SendCharacterSoulMateIntrodactionUpdateResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterSoulMateIntrodactionUpdate))
            {
                packet.WriteInt32(-1);
                packet.WriteFixedAsciiString(client.ActiveCharacter.SoulmateIntroduction ?? "", (int) sbyte.MaxValue,
                    Locale.Start);
                packet.WriteByte(1);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.CharacterSoulMateIntrodactionUpdate)]
        public static void ModyifySoulmateIntroductionRequest(IRealmClient client, RealmPacketIn packet)
        {
            string message = packet.ReadAsdaString((int) sbyte.MaxValue, Locale.Start);
            if (Asda2EncodingHelper.MinimumAvailableLocale(client.Locale, message) != Locale.Start)
                client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("Soulmate introduction.");
            client.ActiveCharacter.SoulmateIntroduction = message;
        }

        [PacketHandler(RealmServerOpCode.FindSoulMateReq)]
        public static void FindSoulMateReqRequest(IRealmClient client, RealmPacketIn packet)
        {
            ++packet.Position;
            byte page = packet.ReadByte();
            Asda2SoulmateHandler.SendSoulmatesListResponse(client, page);
        }

        public static void SendSoulmatesListResponse(IRealmClient client, byte page)
        {
            List<Character> list = client.ActiveCharacter.Map.Characters.ToList<Character>();
            list.Remove(client.ActiveCharacter);
            int val1 = list.Count / 7;
            if (val1 == 0)
                val1 = 1;
            IEnumerable<Character> characters = list.Skip<Character>(((int) page - 1) * 7).Take<Character>(7);
            AchievementProgressRecord progressRecord =
                client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(12U);
            switch (++progressRecord.Counter)
            {
                case 500:
                    client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Searching85);
                    break;
                case 1000:
                    client.ActiveCharacter.GetTitle(Asda2TitleId.Searching85);
                    break;
            }

            progressRecord.SaveAndFlush();
            if (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Searching85) &&
                client.ActiveCharacter.isTitleGetted(Asda2TitleId.Friend86) &&
                (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Companion87) &&
                 client.ActiveCharacter.isTitleGetted(Asda2TitleId.Soulmate88)) &&
                (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Heartbreaker89) &&
                 client.ActiveCharacter.isTitleGetted(Asda2TitleId.LoveNote90) &&
                 (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Cherished91) &&
                  client.ActiveCharacter.isTitleGetted(Asda2TitleId.Devoted92))) &&
                client.ActiveCharacter.isTitleGetted(Asda2TitleId.SnowWhite93))
                client.ActiveCharacter.GetTitle(Asda2TitleId.TrueLove94);
            foreach (Character character in characters)
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SoulmatesList))
                {
                    packet.WriteByte(val1);
                    packet.WriteByte(page);
                    packet.WriteByte(character.Level);
                    packet.WriteByte(character.ProfessionLevel);
                    packet.WriteByte((byte) character.Archetype.ClassId);
                    packet.WriteFixedAsciiString(character.Name, 20, Locale.Start);
                    packet.WriteByte((byte) character.Gender);
                    packet.WriteInt16(character.SessionId);
                    packet.WriteInt32(character.AccId);
                    packet.WriteByte(10);
                    int val2 = character.SoulmateIntroduction == null ? 0 : character.SoulmateIntroduction.Length;
                    packet.WriteByte(val2);
                    packet.WriteByte(character.Record.Zodiac);
                    if (val2 > 0)
                        packet.WriteAsciiString(character.SoulmateIntroduction, Locale.Start);
                    client.Send(packet, false);
                }
            }

            Asda2SoulmateHandler.SendSoulmatesListEndedResponse(client);
        }

        public static void SendSoulmatesListEndedResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SoulmatesListEnded))
                client.Send(packet, true);
        }

        [PacketHandler(RealmServerOpCode.SoulmateRequest)]
        public static void SoulmateRequestRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position -= 7;
            Character activeCharacter = client.ActiveCharacter;
            string name = packet.ReadAsdaString(20, Locale.Start);
            Character character = World.GetCharacter(name, true);
            if (character == null)
                client.ActiveCharacter.SendSystemMessage(string.Format("Cant add soulmate cause not found by name {0}",
                    (object) name));
            else if ((int) character.Asda2FactionId != (int) client.ActiveCharacter.Asda2FactionId)
                client.ActiveCharacter.SendSystemMessage(string.Format("Sorry ,but {0} is in other faction.",
                    (object) character.Name));
            else if (!character.EnableSoulmateRequest)
                client.ActiveCharacter.SendSystemMessage(string.Format("Sorry, but {0} rejects all soulmate requests.",
                    (object) name));
            else if (activeCharacter.IsSoulmated)
                Asda2SoulmateHandler.SendSoulmateRequestResponseResponse(client,
                    SoulmateRequestResponseResult.TargetAlreadyHasASoulmate, activeCharacter.AccId,
                    activeCharacter.Name, (byte) 0, (byte) 0, (byte) 0, "");
            else if (character.IsSoulmated)
                Asda2SoulmateHandler.SendSoulmateRequestResponseResponse(client,
                    SoulmateRequestResponseResult.TargetAlreadyHasASoulmate, character.AccId, character.Name, (byte) 0,
                    (byte) 0, (byte) 0, "");
            else
                Asda2SoulmateHandler.SendSoulmateRequestResponseResponse(character.Client,
                    SoulmateRequestResponseResult.YouRecievingSoulmateRequest, activeCharacter.AccId,
                    activeCharacter.Name, (byte) activeCharacter.Level, (byte) activeCharacter.Gender,
                    (byte) activeCharacter.Archetype.ClassId, activeCharacter.SoulmateIntroduction);
        }

        public static void SendSoulmateRequestResponseResponse(IRealmClient client,
            SoulmateRequestResponseResult status, uint senderAccId = 0, string senderName = "", byte senderLevel = 0,
            byte senderSex = 0, byte senderClass = 0, string senderIntroductionMsg = "")
        {
            Character character = World.GetCharacter(senderName, true);
            client.ActiveCharacter.GetTitle(Asda2TitleId.Friend86);
            character.GetTitle(Asda2TitleId.Friend86);
            if (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Searching85) &&
                client.ActiveCharacter.isTitleGetted(Asda2TitleId.Friend86) &&
                (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Companion87) &&
                 client.ActiveCharacter.isTitleGetted(Asda2TitleId.Soulmate88)) &&
                (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Heartbreaker89) &&
                 client.ActiveCharacter.isTitleGetted(Asda2TitleId.LoveNote90) &&
                 (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Cherished91) &&
                  client.ActiveCharacter.isTitleGetted(Asda2TitleId.Devoted92))) &&
                client.ActiveCharacter.isTitleGetted(Asda2TitleId.SnowWhite93))
                client.ActiveCharacter.GetTitle(Asda2TitleId.TrueLove94);
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SoulmateRequestResponse))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(senderAccId);
                packet.WriteInt16(1);
                packet.WriteByte(0);
                packet.WriteByte(1);
                packet.WriteInt16(3);
                packet.WriteFixedAsciiString(senderName, 20, Locale.Start);
                packet.WriteByte(1);
                packet.WriteInt16(senderLevel);
                packet.WriteByte(senderSex);
                packet.WriteByte(senderClass);
                packet.WriteFixedAsciiString(senderIntroductionMsg, 128, Locale.Start);
                client.Send(packet, false);
            }
        }

        public static void SendYouHaveSoulmatedWithResponse(IRealmClient client, SoulmatingResult status,
            uint soulmateId = 0, uint soulmateAccId = 0, string soulmateName = null)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.YouHaveSoulmatedWith))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(soulmateId);
                packet.WriteInt32(soulmateAccId);
                packet.WriteFixedAsciiString(soulmateName, 20, Locale.Start);
                packet.WriteByte(1);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.AnswerSoulmateRequest)]
        public static void AnswerSoulmateRequestRequest(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            packet.Position -= 8;
            byte num = packet.ReadByte();
            packet.Position += 10;
            string name = packet.ReadAsdaString(20, Locale.Start);
            Character character = World.GetCharacter(name, true);
            if (character == null)
                client.ActiveCharacter.SendSystemMessage(string.Format("Cant add soulmate cause not found by name {0}",
                    (object) name));
            else if (num == (byte) 1)
            {
                if (activeCharacter.IsSoulmated)
                    Asda2SoulmateHandler.SendSoulmateRequestResponseResponse(client,
                        SoulmateRequestResponseResult.TargetAlreadyHasASoulmate, activeCharacter.AccId,
                        activeCharacter.Name, (byte) 0, (byte) 0, (byte) 0, "");
                else if (character.IsSoulmated)
                    Asda2SoulmateHandler.SendSoulmateRequestResponseResponse(client,
                        SoulmateRequestResponseResult.TargetAlreadyHasASoulmate, character.AccId, character.Name,
                        (byte) 0, (byte) 0, (byte) 0, "");
                else
                    Asda2SoulmateMgr.CreateNewOrUpdateSoulmateRelation(activeCharacter, character);
            }
            else
                Asda2SoulmateHandler.SendSoulmateRequestResponseResponse(character.Client,
                    SoulmateRequestResponseResult.TargetRefusedSoulmateRequest, activeCharacter.AccId,
                    activeCharacter.Name, (byte) 0, (byte) 0, (byte) 0, "");
        }

        [PacketHandler(RealmServerOpCode.DisbandSoulMate)]
        public static void DisbandSoulMateRequest(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            if (activeCharacter.IsSoulmated)
            {
                activeCharacter.SoulmateRecord.IsActive = false;
                activeCharacter.SoulmateRecord.UpdateCharacters();
                AchievementProgressRecord progressRecord =
                    client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(13U);
                switch (++progressRecord.Counter)
                {
                    case 25:
                        client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Heartbreaker89);
                        break;
                    case 50:
                        client.ActiveCharacter.GetTitle(Asda2TitleId.Heartbreaker89);
                        break;
                }

                progressRecord.SaveAndFlush();
                if (!client.ActiveCharacter.isTitleGetted(Asda2TitleId.Searching85) ||
                    !client.ActiveCharacter.isTitleGetted(Asda2TitleId.Friend86) ||
                    (!client.ActiveCharacter.isTitleGetted(Asda2TitleId.Companion87) ||
                     !client.ActiveCharacter.isTitleGetted(Asda2TitleId.Soulmate88)) ||
                    (!client.ActiveCharacter.isTitleGetted(Asda2TitleId.Heartbreaker89) ||
                     !client.ActiveCharacter.isTitleGetted(Asda2TitleId.LoveNote90) ||
                     (!client.ActiveCharacter.isTitleGetted(Asda2TitleId.Cherished91) ||
                      !client.ActiveCharacter.isTitleGetted(Asda2TitleId.Devoted92))) ||
                    !client.ActiveCharacter.isTitleGetted(Asda2TitleId.SnowWhite93))
                    return;
                client.ActiveCharacter.GetTitle(Asda2TitleId.TrueLove94);
            }
            else
                Asda2SoulmateHandler.SendDisbandSoulMateResultResponse(client,
                    DisbandSoulmateResult.IsNoLongerYporSoulmate, "friend");
        }

        public static void SendDisbandSoulMateResultResponse(IRealmClient client, DisbandSoulmateResult status,
            string friendName = "friend")
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.DisbandSoulMateResult))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteFixedAsciiString(friendName, 20, Locale.Start);
                client.Send(packet, false);
            }
        }

        public static void SendSoulmateLoggedOutResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SoulmateLoggedOut))
                client.Send(packet, false);
        }

        public static void SendSoulmateEnterdGameResponse(IRealmClient client)
        {
            if (client == null)
                return;
            Character activeCharacter = client.ActiveCharacter;
            Character soulmateCharacter = client.ActiveCharacter.SoulmateCharacter;
            if (soulmateCharacter == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SoulmateEnterdGame))
            {
                packet.WriteInt32((int) activeCharacter.SoulmateRecord.SoulmateRelationGuid);
                packet.WriteInt32(activeCharacter.SoulmateCharacter.AccId);
                packet.WriteInt16(activeCharacter.SoulmateCharacter.SessionId);
                packet.WriteByte(0);
                packet.WriteByte(1);
                packet.WriteByte(10);
                packet.WriteInt32(activeCharacter.SoulmateCharacter.MaxHealth);
                packet.WriteInt32(activeCharacter.SoulmateCharacter.Health);
                packet.WriteInt16(activeCharacter.SoulmateCharacter.MaxPower);
                packet.WriteInt16(activeCharacter.SoulmateCharacter.Power);
                packet.WriteInt16((short) activeCharacter.Asda2X);
                packet.WriteInt16((short) activeCharacter.Asda2Y);
                packet.WriteByte(activeCharacter.SoulmateCharacter.Level);
                packet.WriteByte(132);
                packet.WriteByte(111);
                packet.WriteSkip(Asda2SoulmateHandler.unk20);
                packet.WriteInt16((byte) soulmateCharacter.MapId);
                packet.WriteByte(1);
                packet.WriteByte(1);
                packet.WriteInt64((long) activeCharacter.SoulmateRecord.Expirience);
                packet.WriteByte(activeCharacter.SoulmateRecord.Level);
                packet.WriteSkip(Asda2SoulmateHandler.unk26);
                packet.WriteFixedAsciiString(activeCharacter.SoulmateCharacter.Name, 20, Locale.Start);
                Aura[] visibleAuras = activeCharacter.SoulmateCharacter.Auras.VisibleAuras;
                for (int index = 0; index < 28; ++index)
                {
                    packet.WriteByte(visibleAuras[index] == null ? 0 : 1);
                    packet.WriteByte(0);
                    packet.WriteInt32(visibleAuras[index] == null ? -1 : (int) visibleAuras[index].Spell.RealId);
                }

                client.Send(packet, false);
            }
        }

        public static void SendSoulMateHpMpUpdateResponse(IRealmClient client)
        {
            if (client == null || client.ActiveCharacter == null)
                return;
            Character soulmateCharacter = client.ActiveCharacter.SoulmateCharacter;
            if (soulmateCharacter == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SoulMateHpMpUpdate))
            {
                packet.WriteInt32(client.ActiveCharacter.MaxHealth);
                packet.WriteInt32(client.ActiveCharacter.Health);
                packet.WriteInt16(client.ActiveCharacter.MaxPower);
                packet.WriteInt16(client.ActiveCharacter.Power);
                packet.WriteInt16((short) client.ActiveCharacter.Asda2X);
                packet.WriteInt16((short) client.ActiveCharacter.Asda2Y);
                packet.WriteByte(client.ActiveCharacter.Level);
                packet.WriteInt64((long) client.ActiveCharacter.Experience);
                packet.WriteByte(0);
                packet.WriteByte((byte) soulmateCharacter.MapId);
                packet.WriteInt16(0);
                packet.WriteInt64((long) client.ActiveCharacter.SoulmateRecord.Expirience);
                packet.WriteInt64((long) client.ActiveCharacter.SoulmateRecord.Level);
                packet.WriteSkip(Asda2SoulmateHandler.stab52);
                soulmateCharacter.Send(packet, true);
            }
        }

        public static void SendSoulmateBuffUpdateInfoResponse(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SoulmateBuffUpdateInfo))
            {
                Aura[] auraArray = new Aura[28];
                int num = 0;
                foreach (Aura activeAura in chr.Auras.ActiveAuras)
                {
                    if (activeAura.TicksLeft > 0)
                    {
                        auraArray[num++] = activeAura;
                        if (auraArray.Length <= num)
                            break;
                    }
                }

                for (int index = 0; index < 28; ++index)
                {
                    Aura aura = auraArray[index];
                    packet.WriteByte(aura == null ? 0 : 1);
                    packet.WriteByte(0);
                    packet.WriteInt32(aura == null ? -1 : (int) aura.Spell.RealId);
                }

                chr.SoulmateCharacter.Send(packet, false);
            }
        }

        public static void SendSoulmatePositionResponse(IRealmClient client)
        {
            if (client == null || client.ActiveCharacter == null)
                return;
            Character soulmateCharacter = client.ActiveCharacter.SoulmateCharacter;
            if (soulmateCharacter == null || soulmateCharacter.SoulmateRecord == null || client.ActiveCharacter == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SoulmatePosition))
            {
                packet.WriteInt32((int) soulmateCharacter.SoulmateRecord.SoulmateRelationGuid);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteInt16((short) client.ActiveCharacter.Asda2X);
                packet.WriteInt16((short) client.ActiveCharacter.Asda2Y);
                packet.WriteInt16((short) client.ActiveCharacter.Asda2Y);
                client.ActiveCharacter.SoulmateCharacter.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.PreviousSoulmateInfo)]
        public static void PreviousSoulmateInfoRequest(IRealmClient client, RealmPacketIn packet)
        {
            Asda2SoulmateHandler.SendPreviousSoulmateInfoResResponse(client);
        }

        public static void SendPreviousSoulmateInfoResResponse(IRealmClient client)
        {
            AchievementProgressRecord progressRecord =
                client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(18U);
            switch (++progressRecord.Counter)
            {
                case 50:
                    client.ActiveCharacter.DiscoverTitle(Asda2TitleId.OldFlame95);
                    break;
                case 100:
                    client.ActiveCharacter.GetTitle(Asda2TitleId.OldFlame95);
                    break;
            }

            progressRecord.SaveAndFlush();
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PreviousSoulmateInfoRes))
            {
                List<Asda2SoulmateRelationRecord> soulmateRecords =
                    Asda2SoulmateMgr.GetSoulmateRecords(client.ActiveCharacter.AccId);
                int num = 0;
                foreach (Asda2SoulmateRelationRecord soulmateRelationRecord in soulmateRecords)
                {
                    if (num < 3)
                    {
                        CharacterRecord[] allByProperty = ActiveRecordBase<CharacterRecord>.FindAllByProperty(
                            "AccountId",
                            (object) ((int) soulmateRelationRecord.AccId == (int) client.ActiveCharacter.AccId
                                ? (int) soulmateRelationRecord.RelatedAccId
                                : (int) soulmateRelationRecord.AccId));
                        for (int index = 0; index < 3; ++index)
                        {
                            CharacterRecord characterRecord = allByProperty.Get<CharacterRecord>(index);
                            packet.WriteInt32((int) soulmateRelationRecord.SoulmateRelationGuid);
                            packet.WriteByte(soulmateRelationRecord.Level);
                            packet.WriteByte(characterRecord == null ? 0 : characterRecord.Level);
                            packet.WriteByte(characterRecord == null ? 0 : (int) characterRecord.ProfessionLevel);
                            packet.WriteByte(characterRecord == null ? (byte) 0 : (byte) characterRecord.Class);
                            packet.WriteFixedAsciiString(characterRecord == null ? "" : characterRecord.Name, 20,
                                Locale.Start);
                        }

                        ++num;
                    }
                    else
                        break;
                }

                for (; num < 3; ++num)
                {
                    packet.WriteInt32(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteFixedAsciiString("", 20, Locale.Start);
                    packet.WriteInt32(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteFixedAsciiString("", 20, Locale.Start);
                    packet.WriteInt32(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteFixedAsciiString("", 20, Locale.Start);
                }

                client.Send(packet, true);
            }
        }

        public static void SendUpdateFriendShipPointsResponse(Character chr)
        {
            if (!chr.IsSoulmated)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.UpdateFriendShipPoints))
            {
                packet.WriteInt16(chr.SessionId);
                packet.WriteByte(chr.FriendShipPoints);
                chr.Client.Send(packet, false);
            }
        }

        public static void SendSoulMateInfoInitResponse(Character rcv, bool showFriendWindow)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SoulMateInfoInit))
            {
                packet.WriteInt32(rcv.AccId);
                packet.WriteInt16(rcv.SessionId);
                packet.WriteInt32((uint) rcv.SoulmateRecord.SoulmateRelationGuid);
                packet.WriteInt64((long) rcv.SoulmateRecord.Expirience);
                packet.WriteByte(rcv.SoulmateRecord.Level);
                packet.WriteInt32(rcv.SoulmateRecord.FriendAccId(rcv.AccId));
                packet.WriteInt16(rcv.SoulmateCharacter == null ? 0 : (int) rcv.SoulmateCharacter.SessionId);
                packet.WriteInt16(1);
                packet.WriteByte(1);
                packet.WriteByte(3);
                packet.WriteByte(2);
                packet.WriteByte(2);
                packet.WriteInt16(10);
                packet.WriteInt32(showFriendWindow ? 1 : 0);
                packet.WriteInt16(0);
                packet.WriteFixedAsciiString("", 20, Locale.Start);
                packet.WriteFixedAsciiString("", 20, Locale.Start);
                packet.WriteFixedAsciiString("", 20, Locale.Start);
                packet.WriteInt16(1);
                rcv.Send(packet, false);
            }
        }

        public static void SendAppleExpGainedResponse(Character c)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AppleExpGained))
            {
                packet.WriteInt16(c.SessionId);
                packet.WriteInt32(c.SoulmateRecord.ApplePoints);
                packet.WriteByte(c.SoulmateRecord.ApplePoints);
                c.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.EatApple)]
        public static void EatAppleRequest(IRealmClient client, RealmPacketIn packet)
        {
            Asda2SoulmateRelationRecord soulmateRecord = client.ActiveCharacter.SoulmateRecord;
            if (client.ActiveCharacter.EatingAppleStep == (byte) 0)
            {
                lock (soulmateRecord)
                {
                    if (soulmateRecord.ApplePoints != (byte) 100)
                    {
                        Asda2SoulmateHandler.SendAppleEatResultResponse(client, AppleEatResult.Fail,
                            AppleBonusType.Item, 0, (Asda2Item) null);
                        return;
                    }

                    soulmateRecord.ApplePoints = (byte) 0;
                }

                client.ActiveCharacter.EatingAppleStep = (byte) 1;
                Asda2SoulmateHandler.SendAppleRandomResponse(client, (byte) 2, (byte) 1);
            }
            else
            {
                if (client.ActiveCharacter.EatingAppleStep != (byte) 1)
                    return;
                client.ActiveCharacter.EatingAppleStep = (byte) 2;
                Asda2SoulmateHandler.SendAppleRandomResponse(client, (byte) Utility.Random(0, 3), (byte) 1);
            }
        }

        public static void SendAppleRandomResponse(IRealmClient client, byte sector, byte recordId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AppleRandom))
            {
                packet.WriteByte(1);
                packet.WriteByte(sector);
                packet.WriteByte(recordId);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.GetApplePrize)]
        public static void GetApplePrizeRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter == null || client.ActiveCharacter.SoulmateCharacter == null)
                return;
            if (client.ActiveCharacter.EatingAppleStep == (byte) 2)
            {
                client.ActiveCharacter.EatingAppleStep = (byte) 0;
                int lvl = client.ActiveCharacter.Level + client.ActiveCharacter.SoulmateCharacter.Level;
                int num1 = CharacterFormulas.CalcFriendAppleExp(lvl, client.ActiveCharacter.SoulmateRecord.Level);
                int num2 = num1 * client.ActiveCharacter.Level / lvl;
                client.ActiveCharacter.GainXp(num2, "apple", false);
                client.ActiveCharacter.Health = client.ActiveCharacter.MaxHealth;
                client.ActiveCharacter.Power = client.ActiveCharacter.Power;
                Asda2SoulmateHandler.SendAppleEatResultResponse(client, AppleEatResult.Ok, AppleBonusType.Exp, num2,
                    (Asda2Item) null);
                int num3 = num1 * client.ActiveCharacter.SoulmateCharacter.Level / lvl;
                if (client.ActiveCharacter.SoulmateCharacter == null)
                    return;
                client.ActiveCharacter.SoulmateCharacter.GainXp(num3, "apple", false);
                client.ActiveCharacter.SoulmateCharacter.Health = client.ActiveCharacter.MaxHealth;
                client.ActiveCharacter.SoulmateCharacter.Power = client.ActiveCharacter.Power;
                Asda2SoulmateHandler.SendAppleEatResultResponse(client, AppleEatResult.Ok, AppleBonusType.Exp, num3,
                    (Asda2Item) null);
            }
            else
                client.ActiveCharacter.YouAreFuckingCheater("Trying to get apple prize from [I DONT KNOW].", 20);
        }

        public static void SendAppleEatResultResponse(IRealmClient client, AppleEatResult status, AppleBonusType type,
            int expAmount, Asda2Item bonusItem = null)
        {
            AchievementProgressRecord progressRecord =
                client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(17U);
            switch (++progressRecord.Counter)
            {
                case 100:
                    client.ActiveCharacter.DiscoverTitle(Asda2TitleId.SnowWhite93);
                    break;
                case 200:
                    client.ActiveCharacter.GetTitle(Asda2TitleId.SnowWhite93);
                    break;
            }

            progressRecord.SaveAndFlush();
            if (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Searching85) &&
                client.ActiveCharacter.isTitleGetted(Asda2TitleId.Friend86) &&
                (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Companion87) &&
                 client.ActiveCharacter.isTitleGetted(Asda2TitleId.Soulmate88)) &&
                (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Heartbreaker89) &&
                 client.ActiveCharacter.isTitleGetted(Asda2TitleId.LoveNote90) &&
                 (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Cherished91) &&
                  client.ActiveCharacter.isTitleGetted(Asda2TitleId.Devoted92))) &&
                client.ActiveCharacter.isTitleGetted(Asda2TitleId.SnowWhite93))
                client.ActiveCharacter.GetTitle(Asda2TitleId.TrueLove94);
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AppleEatResult))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteByte((byte) type);
                packet.WriteInt32(expAmount);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, bonusItem, false);
                packet.WriteSkip(Asda2SoulmateHandler.stub67);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.SendSoulmateMessage)]
        public static void SendSoulmateMessageRequest(IRealmClient client, RealmPacketIn packet)
        {
            string str = packet.ReadAsciiString(client.Locale);
            if (client.ActiveCharacter.SoulmateCharacter != null &&
                client.ActiveCharacter.SoulmateCharacter.Client.IsConnected)
            {
                Locale locale = Asda2EncodingHelper.MinimumAvailableLocale(client.Locale, str);
                if (locale != Locale.Start && client.ActiveCharacter.SoulmateCharacter.Client.Locale != client.Locale)
                {
                    client.ActiveCharacter.SendInfoMsg("You friend client accepts only english.");
                }
                else
                {
                    Asda2SoulmateHandler.SendIncomingSoulmateMessageResponse(client.ActiveCharacter.SoulmateCharacter,
                        str, locale);
                    Asda2SoulmateHandler.SendSoulmatemessageSendedResponse(client);
                }
            }
            else
                client.ActiveCharacter.SendInfoMsg(
                    "Sry we can't send message to your soulmate while he is offline. Use Regular mail please.");
        }

        public static void SendSoulmatemessageSendedResponse(IRealmClient client)
        {
            AchievementProgressRecord progressRecord =
                client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(14U);
            switch (++progressRecord.Counter)
            {
                case 50:
                    client.ActiveCharacter.DiscoverTitle(Asda2TitleId.LoveNote90);
                    break;
                case 100:
                    client.ActiveCharacter.GetTitle(Asda2TitleId.LoveNote90);
                    break;
            }

            progressRecord.SaveAndFlush();
            if (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Searching85) &&
                client.ActiveCharacter.isTitleGetted(Asda2TitleId.Friend86) &&
                (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Companion87) &&
                 client.ActiveCharacter.isTitleGetted(Asda2TitleId.Soulmate88)) &&
                (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Heartbreaker89) &&
                 client.ActiveCharacter.isTitleGetted(Asda2TitleId.LoveNote90) &&
                 (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Cherished91) &&
                  client.ActiveCharacter.isTitleGetted(Asda2TitleId.Devoted92))) &&
                client.ActiveCharacter.isTitleGetted(Asda2TitleId.SnowWhite93))
                client.ActiveCharacter.GetTitle(Asda2TitleId.TrueLove94);
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SoulmatemessageSended))
            {
                packet.WriteInt32(-1);
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt32(client.ActiveCharacter.AccId);
                packet.WriteByte((byte) client.ActiveCharacter.MapId);
                packet.WriteInt32(2221);
                packet.WriteFixedAsciiString("", 85, Locale.Start);
                client.Send(packet, false);
            }
        }

        public static void SendIncomingSoulmateMessageResponse(Character c, string msg, Locale locale)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.IncomingSoulmateMessage))
            {
                packet.WriteInt32(c.AccId);
                packet.WriteInt32(Utility.Random(0, 10000));
                packet.WriteFixedAsciiString(msg, 65, locale);
                packet.WriteInt32(0);
                packet.WriteInt32(0);
                packet.WriteInt32(0);
                packet.WriteInt32(0);
                packet.WriteInt32(0);
                c.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.UseSoulmateSkill)]
        public static void UseSoulmateSkillRequest(IRealmClient client, RealmPacketIn packet)
        {
            short skillId = packet.ReadInt16();
            packet.Position += 112;
            short targetSessId = packet.ReadInt16();
            if (client.ActiveCharacter.SoulmateCharacter == null)
                Asda2SoulmateHandler.SendSoulmateSkillUsedResponse(client, UseSoulmateSkillStatus.FriendNotInGame,
                    (short) 0, (short) 0);
            else if (client.ActiveCharacter.IsDead)
            {
                Asda2SoulmateHandler.SendSoulmateSkillUsedResponse(client, UseSoulmateSkillStatus.YouAreDead, (short) 0,
                    (short) 0);
            }
            else
            {
                Asda2SoulmateRelationRecord soulmateRecord = client.ActiveCharacter.SoulmateRecord;
                if (!soulmateRecord.Skills.ContainsKey((Asda2SoulmateSkillId) skillId))
                    Asda2SoulmateHandler.SendSoulmateSkillUsedResponse(client, UseSoulmateSkillStatus.Fail, (short) 0,
                        (short) 0);
                else if (soulmateRecord.Skills[(Asda2SoulmateSkillId) skillId].TryCast(client.ActiveCharacter,
                    client.ActiveCharacter.SoulmateCharacter))
                {
                    if (skillId == (short) 39)
                    {
                        AchievementProgressRecord progressRecord =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(15U);
                        switch (++progressRecord.Counter)
                        {
                            case 5:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Cherished91);
                                break;
                            case 10:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Cherished91);
                                break;
                        }

                        progressRecord.SaveAndFlush();
                    }

                    if (skillId == (short) 35)
                    {
                        AchievementProgressRecord progressRecord =
                            client.ActiveCharacter.Achievements.GetOrCreateProgressRecord(16U);
                        switch (++progressRecord.Counter)
                        {
                            case 5:
                                client.ActiveCharacter.DiscoverTitle(Asda2TitleId.Devoted92);
                                break;
                            case 10:
                                client.ActiveCharacter.GetTitle(Asda2TitleId.Devoted92);
                                break;
                        }

                        progressRecord.SaveAndFlush();
                    }

                    if (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Searching85) &&
                        client.ActiveCharacter.isTitleGetted(Asda2TitleId.Friend86) &&
                        (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Companion87) &&
                         client.ActiveCharacter.isTitleGetted(Asda2TitleId.Soulmate88)) &&
                        (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Heartbreaker89) &&
                         client.ActiveCharacter.isTitleGetted(Asda2TitleId.LoveNote90) &&
                         (client.ActiveCharacter.isTitleGetted(Asda2TitleId.Cherished91) &&
                          client.ActiveCharacter.isTitleGetted(Asda2TitleId.Devoted92))) &&
                        client.ActiveCharacter.isTitleGetted(Asda2TitleId.SnowWhite93))
                        client.ActiveCharacter.GetTitle(Asda2TitleId.TrueLove94);
                    Asda2SoulmateHandler.SendSoulmateSkillCastResponse(client.ActiveCharacter, targetSessId, skillId);
                    Asda2SoulmateHandler.SendSoulmateSkillUsedResponse(client, UseSoulmateSkillStatus.Ok, skillId,
                        targetSessId);
                    Asda2SoulmateHandler.SendSoulmateSkillUsedResponse(client.ActiveCharacter.SoulmateCharacter.Client,
                        UseSoulmateSkillStatus.Ok, skillId, targetSessId);
                }
                else
                    Asda2SoulmateHandler.SendSoulmateSkillUsedResponse(client, UseSoulmateSkillStatus.Fail, (short) 0,
                        (short) 0);
            }
        }

        public static void SendSoulmateSkillCastResponse(Character caster, short targetSessId, short skillId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SoulmateSkillCast))
            {
                packet.WriteInt16(caster.SessionId);
                packet.WriteInt16(skillId);
                packet.WriteSkip(Asda2SoulmateHandler.unk7);
                packet.WriteInt16(targetSessId);
                packet.WriteInt16(targetSessId);
                packet.WriteSkip(Asda2SoulmateHandler.unk10);
                packet.WriteByte((byte) caster.MapId);
                packet.WriteSkip(Asda2SoulmateHandler.stab186);
                caster.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendSoulmateSkillUsedResponse(IRealmClient client, UseSoulmateSkillStatus status,
            short skillId, short targetSessId)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SoulmateSkillUsed))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteInt16(skillId);
                packet.WriteSkip(Asda2SoulmateHandler.unk8);
                packet.WriteInt16(targetSessId);
                packet.WriteInt16(targetSessId);
                packet.WriteSkip(Asda2SoulmateHandler.unk11);
                packet.WriteByte((byte) client.ActiveCharacter.MapId);
                packet.WriteSkip(Asda2SoulmateHandler.stub180);
                client.Send(packet, true);
                if (client.ActiveCharacter.SoulmateCharacter == null)
                    return;
                client.ActiveCharacter.SoulmateCharacter.Send(packet, true);
            }
        }

        public static void SendSoulmateSummoningYouResponse(Character summoner, Character friend)
        {
            if (summoner == null || friend == null)
                return;
            friend.CanTeleportToFriend = true;
            friend.TargetSummonMap = summoner.Map.MapId;
            friend.TargetSummonPosition = summoner.Position;
            friend.SendInfoMsg(string.Format("{0} is summoning you to {1} [{2},{3}].", (object) summoner.Name,
                (object) summoner.TargetSummonMap, (object) summoner.Asda2Position.X,
                (object) summoner.Asda2Position.Y));
            friend.SendInfoMsg("Write #acpts to accept summon request or click OK.");
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SoulmateSummoningYou))
            {
                packet.WriteInt32(summoner.AccId);
                packet.WriteInt16(summoner.SessionId);
                packet.WriteByte(0);
                packet.WriteInt16((byte) summoner.MapId);
                packet.WriteByte(1);
                if (friend.Client.AddrTemp.Contains("192.168."))
                    packet.WriteFixedAsciiString(RealmServerConfiguration.ExternalAddress, 16, Locale.Start);
                else
                    packet.WriteFixedAsciiString(RealmServerConfiguration.RealExternalAddress, 16, Locale.Start);
                packet.WriteInt16(ServerApp<WCell.RealmServer.RealmServer>.Instance.Port);
                packet.WriteInt16((short) summoner.Asda2X);
                packet.WriteInt16((short) summoner.Asda2Y);
                packet.WriteInt32(friend.AccId);
                packet.WriteInt16(friend.SessionId);
                packet.WriteByte(0);
                packet.WriteInt16((byte) friend.MapId);
                packet.WriteByte(1);
                friend.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.TeleportToSoulmateAnswer)]
        public static void TeleportToSoulmateAnswerRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.CanTeleportToFriend)
                return;
            client.ActiveCharacter.CanTeleportToFriend = false;
            client.ActiveCharacter.TeleportTo(client.ActiveCharacter.TargetSummonMap,
                client.ActiveCharacter.TargetSummonPosition);
        }

        [PacketHandler((RealmServerOpCode) 5237)]
        public static void SoulmateSummonCancelRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.CanTeleportToFriend)
                return;
            client.ActiveCharacter.CanTeleportToFriend = false;
        }
    }
}