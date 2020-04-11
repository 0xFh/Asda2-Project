using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    internal class Asda2PvpHandler
    {
        private static readonly byte[] stab6 = new byte[2];

        [PacketHandler(RealmServerOpCode.PvpRquest)]
        public static void PvpRquestRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 22;
            Character characterBySessionId = World.GetCharacterBySessionId(packet.ReadUInt16());
            if (characterBySessionId == null)
                client.ActiveCharacter.SendSystemMessage("The character you asking for duel is not found.");
            else if (client.ActiveCharacter.IsAsda2Dueling || characterBySessionId.IsAsda2Dueling)
            {
                client.ActiveCharacter.SendInfoMsg("Already dueling.");
            }
            else
            {
                Asda2PvpHandler.SendPvpRequestToCharFromSrvResponse(client.ActiveCharacter, characterBySessionId);
                characterBySessionId.Asda2DuelingOponent = client.ActiveCharacter;
            }
        }

        public static void SendPvpRequestToCharFromSrvResponse(Character sender, Character rcv)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PvpRquest))
            {
                packet.WriteInt16(sender.SessionId);
                packet.WriteInt32(sender.AccId);
                packet.WriteFixedAsciiString(sender.Name, 20, Locale.Start);
                packet.WriteInt16(rcv.SessionId);
                packet.WriteInt32(rcv.AccId);
                packet.WriteInt16(0);
                rcv.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.AnswerPvpRequestOrStartPvp)]
        public static void AnswerPvpRequestRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.IsAsda2Dueling)
                client.ActiveCharacter.SendInfoMsg("You already dueling.");
            else if (client.ActiveCharacter.Asda2DuelingOponent == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to answer pvp without oponent", 20);
            }
            else
            {
                packet.Position -= 4;
                if (packet.ReadByte() == (byte) 1)
                {
                    Asda2Pvp asda2Pvp =
                        new Asda2Pvp(client.ActiveCharacter.Asda2DuelingOponent, client.ActiveCharacter);
                }
                else
                {
                    Asda2PvpHandler.SendPvpStartedResponse(Asda2PvpResponseStatus.Rejected,
                        client.ActiveCharacter.Asda2DuelingOponent, client.ActiveCharacter);
                    client.ActiveCharacter.Asda2DuelingOponent = (Character) null;
                }
            }
        }

        public static void SendPvpStartedResponse(Asda2PvpResponseStatus status, Character rcv, Character answerer)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.AnswerPvpRequestOrStartPvp))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(rcv.SessionId);
                packet.WriteInt32(rcv.AccId);
                packet.WriteFixedAsciiString(answerer.Name, 20, Locale.Start);
                packet.WriteInt16(0);
                packet.WriteInt16((short) answerer.Asda2X);
                packet.WriteInt16((short) answerer.Asda2Y);
                rcv.Send(packet, false);
            }
        }

        public static void SendPvpRoundEffectResponse(Character firstDueler, Character secondDueler)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PvpRoundEffect))
            {
                packet.WriteSkip(Asda2PvpHandler.stab6);
                packet.WriteInt32(firstDueler.AccId);
                packet.WriteInt32(secondDueler.AccId);
                packet.WriteInt16((short) (((double) firstDueler.Asda2X + (double) secondDueler.Asda2X) / 2.0));
                packet.WriteInt16((short) (((double) firstDueler.Asda2Y + (double) secondDueler.Asda2Y) / 2.0));
                firstDueler.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        public static void SendDuelEndedResponse(Character winer, Character looser)
        {
            AchievementProgressRecord progressRecord = winer.Achievements.GetOrCreateProgressRecord(19U);
            switch (++progressRecord.Counter)
            {
                case 13:
                    winer.DiscoverTitle(Asda2TitleId.Duelist122);
                    break;
                case 25:
                    winer.GetTitle(Asda2TitleId.Duelist122);
                    break;
                case 50:
                    winer.DiscoverTitle(Asda2TitleId.Brawler123);
                    break;
                case 100:
                    winer.GetTitle(Asda2TitleId.Brawler123);
                    break;
                case 500:
                    winer.DiscoverTitle(Asda2TitleId.Undefeated124);
                    break;
                case 1000:
                    winer.GetTitle(Asda2TitleId.Undefeated124);
                    break;
            }

            progressRecord.SaveAndFlush();
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.DuelEnded))
            {
                packet.WriteByte(0);
                packet.WriteByte(0);
                packet.WriteInt16(winer.SessionId);
                packet.WriteInt32(winer.AccId);
                packet.WriteInt16(looser.SessionId);
                packet.WriteInt32(looser.AccId);
                packet.WriteByte(2);
                packet.WriteFixedAsciiString(winer.Name, 20, Locale.Start);
                packet.WriteFixedAsciiString(looser.Name, 20, Locale.Start);
                winer.SendPacketToArea(packet, true, true, Locale.Any, new float?());
                looser.Send(packet, true);
            }
        }
    }
}