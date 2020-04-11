using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
    public static class DuelHandler
    {
        public static void HandleAccept(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.Duel == null)
                return;
            client.ActiveCharacter.Duel.Accept(client.ActiveCharacter);
        }

        public static void HandleCancel(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.Duel == null)
                return;
            client.ActiveCharacter.Duel.Finish(DuelWin.Knockout, client.ActiveCharacter);
        }

        public static void SendCountdown(Character duelist, uint millis)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_DUEL_COUNTDOWN, 4))
            {
                packet.Write(millis);
                duelist.Send(packet, false);
            }
        }

        public static void SendRequest(GameObject duelFlag, Character challenger, Character rival)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_DUEL_REQUESTED))
            {
                packet.Write((ulong) duelFlag.EntityId);
                packet.Write((ulong) challenger.EntityId);
                rival.Client.Send(packet, false);
                challenger.Client.Send(packet, false);
            }
        }

        public static void SendOutOfBounds(Character duelist, int cancelDelayMillis)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_DUEL_OUTOFBOUNDS, 4))
            {
                packet.Write(cancelDelayMillis);
                duelist.Send(packet, false);
            }
        }

        public static void SendInBounds(Character duelist)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_DUEL_INBOUNDS, 4))
                duelist.Send(packet, false);
        }

        public static void SendComplete(Character duelist1, Character duelist2, bool complete)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_DUEL_COMPLETE))
            {
                packet.Write(complete ? (byte) 1 : (byte) 0);
                if (duelist1 != null)
                    duelist1.Client.Send(packet, false);
                if (duelist2 == null)
                    return;
                duelist2.Client.Send(packet, false);
            }
        }

        public static void SendWinner(DuelWin win, Unit winner, INamed loser)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_DUEL_WINNER))
            {
                packet.Write((byte) win);
                packet.Write(winner.Name);
                packet.Write(loser.Name);
                winner.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }
    }
}