using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs.Vehicles;

namespace WCell.RealmServer.Handlers
{
    public static class VehicleHandler
    {
        public static void HandleDismissControlledVehicle(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.Vehicle.ClearAllSeats(true);
            MovementHandler.HandleMovement(client, packet);
        }

        public static void HandleRequestVehicleExit(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            if (activeCharacter.Vehicle == null)
                return;
            activeCharacter.VehicleSeat.ClearSeat();
        }

        public static void HandleRequestVehicleNextSeat(IRealmClient client, RealmPacketIn packet)
        {
        }

        public static void HandleRequestVehicleSwitchSeat(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadPackedEntityId();
            int num = (int) packet.ReadByte();
        }

        public static void HandleChangeSeatsOnControlledVehicle(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id1 = packet.ReadPackedEntityId();
            Vehicle vehicle1 = client.ActiveCharacter.Map.GetObject(id1) as Vehicle;
            if (vehicle1 == null)
                return;
            uint clientTime;
            MovementHandler.ReadMovementInfo(packet, client.ActiveCharacter, (Unit) vehicle1, out clientTime);
            EntityId id2 = packet.ReadPackedEntityId();
            Vehicle vehicle2 = client.ActiveCharacter.Map.GetObject(id2) as Vehicle;
            if (vehicle2 == null)
                return;
            Character activeCharacter = client.ActiveCharacter;
            VehicleSeat vehicleSeat = activeCharacter.m_vehicleSeat;
            if (vehicleSeat == null)
                vehicle1.FindSeatOccupiedBy((Unit) activeCharacter);
            if (vehicleSeat == null)
                return;
            byte num = packet.ReadByte();
            VehicleSeat seat = vehicle2.Seats[(int) num];
            if (seat == null || seat.Passenger != null)
                return;
            vehicleSeat.ClearSeat();
            seat.Enter((Unit) activeCharacter);
        }

        public static void SendBreakTarget(IPacketReceiver rcvr, IEntity target)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_BREAK_TARGET, 8))
            {
                packet.Write((ulong) target.EntityId);
                rcvr.Send(packet, false);
            }
        }

        public static void Send_SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA(Character chr)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA, 0))
                chr.Send(packet, false);
        }
    }
}