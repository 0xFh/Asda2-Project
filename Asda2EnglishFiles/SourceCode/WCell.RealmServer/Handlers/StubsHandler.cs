using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class StubsHandler
    {
        [PacketHandler(RealmServerOpCode.U8226)]
        public static void U8226(IRealmClient client, RealmPacketIn packet)
        {
        }

        [PacketHandler(RealmServerOpCode.U8215)]
        public static void U8215(IRealmClient client, RealmPacketIn packet)
        {
        }
    }
}