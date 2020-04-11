using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.AreaTriggers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class AreaTriggerHandler
    {
        public static void SendAreaTriggerMessage(IPacketReceiver client, string msg)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_AREA_TRIGGER_MESSAGE,
                msg.Length * 2 + 4))
            {
                packet.WriteUIntPascalString(msg);
                packet.Write((byte) 0);
                client.Send(packet, false);
            }
        }

        public static void HandleAreaTrigger(IRealmClient client, RealmPacketIn packet)
        {
            uint id = packet.ReadUInt32();
            Character activeCharacter = client.ActiveCharacter;
            if (!activeCharacter.IsAlive)
                return;
            AreaTrigger trigger = AreaTriggerMgr.GetTrigger(id);
            if (trigger == null)
                return;
            trigger.Trigger(activeCharacter);
        }
    }
}