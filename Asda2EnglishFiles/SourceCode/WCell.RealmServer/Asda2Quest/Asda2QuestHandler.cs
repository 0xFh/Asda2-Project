using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Asda2Quest
{
    public static class Asda2QuestHandler
    {
        public static void SendQuestsListResponse(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.QuestsList))
            {
                for (int index = 0; index < 12; ++index)
                {
                    packet.WriteInt32(-1);
                    packet.WriteByte(0);
                    packet.WriteInt16(-1);
                    packet.WriteByte(0);
                    packet.WriteInt16(-1);
                    packet.WriteInt16(0);
                    packet.WriteInt16(-1);
                    packet.WriteInt32(-1);
                    packet.WriteInt16(0);
                    packet.WriteInt32(-1);
                    packet.WriteInt16(0);
                    packet.WriteInt32(-1);
                    packet.WriteInt16(0);
                    packet.WriteInt32(-1);
                    packet.WriteInt16(0);
                    packet.WriteInt32(-1);
                    packet.WriteInt16(0);
                }

                for (int index = 0; index < 1; ++index)
                    packet.WriteByte(254);
                for (int index = 0; index < 149; ++index)
                    packet.WriteByte((int) byte.MaxValue);
                client.Send(packet, false);
            }
        }
    }
}