using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class WorldStateHandler
    {
        public static void SendInitWorldStates(IPacketReceiver rcv, WorldStateCollection states, Zone newZone)
        {
        }

        private static int AppendWorldStates(RealmPacketOut packet, IWorldSpace space)
        {
            int num = 0;
            if (space.ParentSpace != null)
                num += WorldStateHandler.AppendWorldStates(packet, space.ParentSpace);
            if (space.WorldStates != null)
            {
                num += space.WorldStates.FieldCount;
                packet.Write(space.WorldStates.CompiledState);
            }

            return num;
        }

        public static void SendInitWorldStates(IPacketReceiver rcv, MapId map, ZoneId zone, uint areaId,
            params WorldState[] states)
        {
        }

        public static void SendUpdateWorldState(IPacketReceiver rcv, WorldState state)
        {
            WorldStateHandler.SendUpdateWorldState(rcv, state.Key, state.DefaultValue);
        }

        public static void SendUpdateWorldState(IPacketReceiver rcv, WorldStateId key, int value)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_UPDATE_WORLD_STATE, 300))
            {
                packet.Write((uint) key);
                packet.Write(value);
                rcv.Send(packet, false);
            }
        }
    }
}