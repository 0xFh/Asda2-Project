using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class TutorialHandler
    {
        public static void HandleSetTutorialFlag(IRealmClient client, RealmPacketIn packet)
        {
            uint flagIndex = packet.ReadUInt32();
            if (flagIndex >= 256U)
                return;
            client.ActiveCharacter.TutorialFlags.SetFlag(flagIndex);
        }

        public static void HandleClearTutorialFlags(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.TutorialFlags.ClearFlags();
        }

        public static void HandleResetTutorialFlags(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.TutorialFlags.ResetFlags();
        }

        public static void SendTutorialFlags(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_TUTORIAL_FLAGS, 32))
            {
                packet.Write(chr.TutorialFlags.FlagData);
                chr.Client.Send(packet, false);
            }
        }
    }
}