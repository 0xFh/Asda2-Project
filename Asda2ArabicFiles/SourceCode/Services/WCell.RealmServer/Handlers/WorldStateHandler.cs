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

		static int AppendWorldStates(RealmPacketOut packet, IWorldSpace space)
		{
			var count = 0;
			if (space.ParentSpace != null)
			{
				count += AppendWorldStates(packet, space.ParentSpace);
			}
			if (space.WorldStates != null)
			{
				count += space.WorldStates.FieldCount;
				packet.Write(space.WorldStates.CompiledState);
			}
			return count;
		}

		public static void SendInitWorldStates(IPacketReceiver rcv, MapId map, ZoneId zone, uint areaId, params WorldState[] states)
		{
			
		}

		public static void SendUpdateWorldState(IPacketReceiver rcv, WorldState state)
		{
			SendUpdateWorldState(rcv, state.Key, state.DefaultValue);
		}

		public static void SendUpdateWorldState(IPacketReceiver rcv, WorldStateId key, int value)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_UPDATE_WORLD_STATE, 300))
			{
				packet.Write((uint)key);
				packet.Write(value);
				rcv.Send(packet, addEnd: false);
			}
		}
	}
}
