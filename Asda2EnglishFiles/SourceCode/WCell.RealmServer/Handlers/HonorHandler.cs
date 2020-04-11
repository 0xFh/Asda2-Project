using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class HonorHandler
    {
        public static void SendPVPCredit(IPacketReceiver receiver, uint points, Character victim)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_PVP_CREDIT))
            {
                packet.Write(points);
                packet.Write((ulong) victim.EntityId);
                packet.Write((int) victim.PvPRank);
                receiver.Send(packet, false);
            }
        }
    }
}