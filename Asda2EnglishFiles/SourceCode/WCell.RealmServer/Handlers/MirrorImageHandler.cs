using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class MirrorImageHandler
    {
        public static void HandleGetMirrorImageData(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            NPC mirrorimage = client.ActiveCharacter.Map.GetObject(id) as NPC;
            MirrorImageHandler.SendMirrorImageData(client, mirrorimage);
        }

        public static void SendMirrorImageData(IRealmClient client, NPC mirrorimage)
        {
        }
    }
}