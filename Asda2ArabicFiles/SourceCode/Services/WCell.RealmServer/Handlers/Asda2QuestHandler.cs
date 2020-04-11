using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class Asda2QuestHandler
    {

        public static void SendQuestsListResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.QuestsList))//5047
            {
                for (int i = 0; i < 12; i += 1)
                {
                    packet.WriteInt32(-1);//{questId}default value : -1 Len : 4
                    packet.WriteByte(0);//value name : unk1 default value : 0Len : 1
                    packet.WriteInt16(-1);//{questSlot}default value : -1 Len : 2
                    packet.WriteByte(0);//{questStage}default value : 0 Len : 1 2 - in progress 1 - completed
                    packet.WriteInt16(-1);//{oneMoreQuestId}default value : -1 Len : 2
                    packet.WriteInt16(0);//{IsCompleted}default value : 2 Len : 2  0 or 1
                    packet.WriteInt16(-1);//value name : unk2 default value : -1Len : 2
                    packet.WriteInt32(-1);//{questItemId}default value : -1 Len : 4
                    packet.WriteInt16(0);//{questItemAmount}default value : 0 Len : 2
                    packet.WriteInt32(-1);//{questItemId}default value : -1 Len : 4
                    packet.WriteInt16(0);//{questItemAmount}default value : 0 Len : 2
                    packet.WriteInt32(-1);//{questItemId}default value : -1 Len : 4
                    packet.WriteInt16(0);//{questItemAmount}default value : 0 Len : 2
                    packet.WriteInt32(-1);//{questItemId}default value : -1 Len : 4
                    packet.WriteInt16(0);//{questItemAmount}default value : 0 Len : 2
                    packet.WriteInt32(-1);//{questItemId}default value : -1 Len : 4
                    packet.WriteInt16(0);//{questItemAmount}default value : 0 Len : 2*/

                }
                for (int i = 0; i < 1; i++)
                {
                    packet.WriteByte(254);
                }
                for (int i = 0; i < 149; i++)
                {
                    packet.WriteByte(255);
                }
                client.Send(packet, addEnd: false);
            }
        }

    }
}