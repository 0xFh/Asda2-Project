using NLog;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.Titles;

namespace WCell.RealmServer.Handlers
{
    public static class TitleHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static void SendTitleEarned(Character character, CharacterTitleEntry titleEntry, bool lost)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_TITLE_EARNED, 8))
            {
                packet.WriteUInt((uint) titleEntry.BitIndex);
                packet.WriteUInt(lost ? 0 : 1);
                character.Send(packet, false);
            }
        }

        public static void HandleChooseTitle(IRealmClient client, RealmPacketIn packet)
        {
            int num = packet.ReadInt32();
            if (num > 0)
            {
                if (!client.ActiveCharacter.HasTitle((TitleBitId) num))
                    return;
            }
            else
                num = 0;

            client.ActiveCharacter.ChosenTitle = (TitleBitId) num;
        }
    }
}