using WCell.Constants.World;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Handlers
{
    /// <summary>
    /// These commands can be used by GMs through WoW's internal console
    /// </summary>
    public static class ClientConsoleCommandHandler
    {
        public static void HandleMoveSetRawPosition(IRealmClient client, RealmPacketIn packet)
        {
            Vector3 pos = packet.ReadVector3();
            double num = (double) packet.ReadFloat();
            Map map = client.ActiveCharacter.Map;
            if (map == null)
                return;
            client.ActiveCharacter.TeleportTo(map, ref pos);
        }

        public static void HandleWorldTeleport(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = (int) packet.ReadUInt32();
            MapId mapId = (MapId) packet.ReadUInt32();
            Vector3 pos = packet.ReadVector3();
            double num2 = (double) packet.ReadFloat();
            Map nonInstancedMap = WCell.RealmServer.Global.World.GetNonInstancedMap(mapId);
            if (nonInstancedMap == null)
                return;
            client.ActiveCharacter.TeleportTo(nonInstancedMap, ref pos);
        }

        public static void HandleWhoIs(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadCString();
        }

        public static void HandleTeleportToUnit(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadCString();
        }
    }
}