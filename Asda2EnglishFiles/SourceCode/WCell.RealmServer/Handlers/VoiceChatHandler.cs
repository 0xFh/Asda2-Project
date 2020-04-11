using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class VoiceChatHandler
    {
        public static void HandleStatusUpdate(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadBoolean();
            packet.ReadBoolean();
        }

        public static void HandleQuery(IRealmClient client, RealmPacketIn packet)
        {
        }

        public static void SendSystemStatus(Character chr, VoiceSystemStatus status)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_FEATURE_SYSTEM_STATUS))
            {
                packet.WriteByte(2);
                packet.WriteByte((byte) status);
                chr.Client.Send(packet, false);
            }
        }

        public static void SendVoiceData(ChatChannel chatChannel)
        {
            using (new RealmPacketOut(RealmServerOpCode.SMSG_VOICE_SESSION_ROSTER_UPDATE))
                ;
        }
    }
}