using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer;
using WCell.RealmServer.Network;

namespace WCell.Handlers
{
  class SealItemHandler
  {
    //[PacketHandler(RealmServerOpCode.SealItemRequest)]
    //public static void SealItemRequest(IRealmClient client, RealmPacketIn packet)
    //{
    //  using(RealmPacketOut pkt = new RealmPacketOut(RealmServerOpCode.SealItemResponse))
    //  {
    //    pkt.WriteInt64(0);
    //    pkt.WriteInt16(0);

    //    byte result = 0;

    //    pkt.WriteByte(result);
    //    pkt.WriteSkip(new byte[138]);
    //    client.Send(pkt);
    //  }
    //}
  }
}
