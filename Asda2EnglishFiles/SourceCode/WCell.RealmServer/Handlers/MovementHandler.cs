using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Network;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Handlers
{
    public static class MovementHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static void HandleMovement(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            Unit mover = activeCharacter.MoveControl.Mover as Unit;
            if (mover == null || !mover.UnitFlags.HasFlag((Enum) UnitFlags.PlayerControlled) ||
                mover.UnitFlags.HasFlag((Enum) UnitFlags.Influenced))
                return;
            mover.CancelEmote();
            EntityId id = packet.ReadPackedEntityId();
            if (packet.PacketId.RawId == 721U)
            {
                mover = client.ActiveCharacter.Map.GetObject(id) as Unit;
                if (mover == null)
                    return;
            }

            uint clientTime;
            if (!MovementHandler.ReadMovementInfo(packet, activeCharacter, mover, out clientTime))
                return;
            MovementHandler.BroadcastMovementInfo((PacketIn) packet, activeCharacter, mover, clientTime);
        }

        public static void HandleMoveNotActiveMover(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadPackedEntityId();
            Unit mover = client.ActiveCharacter.Map.GetObject(id) as Unit;
            if (mover == null)
                return;
            mover.CancelEmote();
            uint clientTime;
            if (!MovementHandler.ReadMovementInfo(packet, activeCharacter, mover, out clientTime))
                return;
            MovementHandler.BroadcastMovementInfo((PacketIn) packet, activeCharacter, mover, clientTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet">The packet to read the info from</param>
        /// <param name="chr">The active character in the client that send the movement packet</param>
        /// <param name="mover">The unit we want this movement info to affect</param>
        /// <param name="clientTime">Used to return the read client time</param>
        /// <returns>A boolean value used to determine broadcasting of this movement info to other clients</returns>
        public static bool ReadMovementInfo(RealmPacketIn packet, Character chr, Unit mover, out uint clientTime)
        {
            MovementFlags movementFlags = (MovementFlags) packet.ReadInt32();
            MovementFlags2 movementFlags2 = (MovementFlags2) packet.ReadInt16();
            clientTime = packet.ReadUInt32();
            Vector3 pt = packet.ReadVector3();
            float orientation = packet.ReadFloat();
            if (movementFlags.HasFlag((Enum) MovementFlags.OnTransport))
            {
                EntityId id = packet.ReadPackedEntityId();
                Vector3 vector3 = packet.ReadVector3();
                float num1 = packet.ReadFloat();
                uint num2 = packet.ReadUInt32();
                int num3 = (int) packet.ReadByte();
                ITransportInfo transportInfo = mover.Map.GetObject(id) as ITransportInfo;
                bool flag = transportInfo is Vehicle;
                if (transportInfo == null)
                {
                    if (mover.Transport != null)
                        mover.Transport.RemovePassenger(mover);
                    return false;
                }

                if (mover.TransportInfo != transportInfo)
                {
                    if (flag)
                        return false;
                    ((Transport) transportInfo).AddPassenger(mover);
                }

                if (!flag)
                {
                    mover.TransportPosition = vector3;
                    mover.TransportOrientation = num1;
                    mover.TransportTime = num2;
                }
            }
            else if (mover.Transport != null)
                mover.Transport.RemovePassenger(mover);

            if (movementFlags.HasFlag((Enum) (MovementFlags.Swimming | MovementFlags.Flying)) ||
                movementFlags2.HasFlag((Enum) MovementFlags2.AlwaysAllowPitching))
            {
                if (movementFlags.HasFlag((Enum) MovementFlags.Flying) && !chr.CanFly)
                    return false;
                float moveAngle = packet.ReadFloat();
                if (chr == mover)
                    chr.MovePitch(moveAngle);
            }

            int num4 = (int) packet.ReadUInt32();
            if (movementFlags.HasFlag((Enum) MovementFlags.Falling))
            {
                double num1 = (double) packet.ReadFloat();
                double num2 = (double) packet.ReadFloat();
                double num3 = (double) packet.ReadFloat();
                double num5 = (double) packet.ReadFloat();
            }

            if (packet.PacketId.RawId == 201U && chr == mover)
                chr.OnFalling();
            if (movementFlags.HasFlag((Enum) MovementFlags.Swimming) && chr == mover)
                chr.OnSwim();
            else if (chr.IsSwimming && chr == mover)
                chr.OnStopSwimming();
            if (movementFlags.HasFlag((Enum) MovementFlags.SplineElevation))
            {
                double num6 = (double) packet.ReadFloat();
            }

            bool flag1 = pt == mover.Position;
            if (!flag1 && mover.IsInWorld && !mover.SetPosition(pt, orientation))
                return false;
            if (flag1)
                mover.Orientation = orientation;
            else
                mover.OnMove();
            mover.MovementFlags = movementFlags;
            mover.MovementFlags2 = movementFlags2;
            if (flag1)
                mover.Orientation = orientation;
            else if (!mover.CanMove)
                return false;
            return true;
        }

        private static void BroadcastMovementInfo(PacketIn packet, Character chr, Unit mover, uint clientTime)
        {
            ICollection<IRealmClient> nearbyClients = chr.GetNearbyClients<Character>(false);
            if (nearbyClients.Count <= 0)
                return;
            using (RealmPacketOut pak = new RealmPacketOut(packet.PacketId))
            {
                int num = mover.EntityId.WritePacked((BinaryWriter) pak);
                packet.Position = packet.HeaderSize + num;
                pak.Write(packet.ReadBytes(packet.RemainingLength));
                foreach (IRealmClient client in (IEnumerable<IRealmClient>) nearbyClients)
                    MovementHandler.SendMovementPacket(client, pak, 10 + num, clientTime);
            }
        }

        public static void SendMovementPacket(IRealmClient client, RealmPacketOut pak, int moveTimePos,
            uint clientMoveTime)
        {
            uint num = Utility.GetSystemTime() + client.OutOfSyncDelay * 800U;
            long position = pak.Position;
            pak.Position = (long) moveTimePos;
            pak.Write(num);
            pak.Position = position;
            client.LastClientMoveTime = clientMoveTime;
            client.Send(pak, false);
        }

        public static void HandleTimeSkipped(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadPackedEntityId();
            client.OutOfSyncDelay = packet.ReadUInt32();
        }

        /// <summary>
        /// The client sends this after map-change (when the loading screen finished)
        /// </summary>
        public static void HandleWorldPortAck(IRealmClient client, RealmPacketIn packet)
        {
            client.TickCount = 0U;
            Character activeCharacter = client.ActiveCharacter;
            if (activeCharacter == null || activeCharacter.Map == null)
                return;
            Zone zone = activeCharacter.Map.GetZone(activeCharacter.Position.X, activeCharacter.Position.Y);
            if (zone != null)
                activeCharacter.SetZone(zone);
            else
                activeCharacter.SetZone(activeCharacter.Map.DefaultZone);
        }

        /// <summary>The client sends this after he was rooted</summary>
        public static void HandleRootAck(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            packet.ReadPackedEntityId();
            int num1 = (int) packet.ReadUInt32();
            int num2 = (int) packet.ReadUInt32();
            double num3 = (double) packet.ReadFloat();
            double num4 = (double) packet.ReadFloat();
            double num5 = (double) packet.ReadFloat();
            double num6 = (double) packet.ReadFloat();
        }

        public static void HandlerTeleportAck(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadPackedEntityId();
            int num1 = (int) packet.ReadUInt32();
            int num2 = (int) packet.ReadUInt32();
            client.ActiveCharacter.Map.GetObject(id);
        }

        public static void SendEnterTransport(Unit unit)
        {
            ITransportInfo transportInfo = unit.TransportInfo;
            MovementHandler.SendMonsterMoveTransport(unit, transportInfo.Position - unit.Position,
                unit.TransportPosition);
        }

        public static void SendLeaveTransport(Unit unit)
        {
            ITransportInfo transportInfo = unit.TransportInfo;
            MovementHandler.SendMonsterMoveTransport(unit, unit.TransportPosition, transportInfo.Position);
        }

        /// <summary>
        /// Move from current position  to
        /// new position on the Transport (relative to transport)
        /// </summary>
        /// <param name="unit"></param>
        public static void SendMonsterMoveTransport(Unit unit, Vector3 from, Vector3 to)
        {
            if (!unit.IsAreaActive)
                return;
            using (RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.SMSG_MONSTER_MOVE_TRANSPORT))
            {
                ITransportInfo transportInfo = unit.TransportInfo;
                unit.EntityId.WritePacked((BinaryWriter) realmPacketOut);
                transportInfo.EntityId.WritePacked((BinaryWriter) realmPacketOut);
                realmPacketOut.Write((ushort) 0);
                realmPacketOut.Write(from);
                realmPacketOut.Write(Utility.GetSystemTime());
                if (unit is Character)
                {
                    realmPacketOut.Write((byte) 4);
                    realmPacketOut.Write(unit.TransportOrientation);
                    realmPacketOut.Write(8388608U);
                }
                else
                {
                    realmPacketOut.Write((byte) 0);
                    realmPacketOut.Write(4096U);
                }

                realmPacketOut.Write(0);
                realmPacketOut.Write(1);
                realmPacketOut.Write(to);
                unit.SendPacketToArea(realmPacketOut, true, true, Locale.Any, new float?());
            }
        }

        /// <summary>Jumping while not moving when mounted</summary>
        public static void HandleMountSpecialAnim(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            if (!(activeCharacter.MoveControl.Mover is Unit) || !((Unit) activeCharacter.MoveControl.Mover).IsMounted)
                return;
            MovementHandler.SendMountSpecialAnim(client);
        }

        public static void SendMountSpecialAnim(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_MOUNTSPECIAL_ANIM, 8))
            {
                client.ActiveCharacter.EntityId.WritePacked((BinaryWriter) packet);
                client.ActiveCharacter.SendPacketToArea(packet, false, false, Locale.Any, new float?());
            }
        }

        public static void SendMoveToPacket(Unit movingUnit, ref Vector3 pos, float orientation, uint moveTime,
            MonsterMoveFlags moveFlags)
        {
            if (movingUnit.IsAreaActive)
                return;
            Character characterMaster = movingUnit.CharacterMaster;
        }

        public static void SendFacingPacket(Unit movingUnit, float orientation, uint moveTimeMillis)
        {
            if (!movingUnit.IsAreaActive)
                return;
            using (RealmPacketOut realmPacketOut =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_MONSTER_MOVE, 53))
            {
                movingUnit.EntityId.WritePacked((BinaryWriter) realmPacketOut);
                realmPacketOut.Write((byte) 0);
                realmPacketOut.Write(movingUnit.Position);
                realmPacketOut.Write(Utility.GetSystemTime());
                realmPacketOut.Write((byte) 4);
                realmPacketOut.Write(orientation);
                realmPacketOut.Write(0U);
                realmPacketOut.Write(moveTimeMillis);
                realmPacketOut.Write(1);
                realmPacketOut.Write(movingUnit.Position);
                movingUnit.SendPacketToArea(realmPacketOut, true, false, Locale.Any, new float?());
            }
        }

        public static void SendMoveToPacket<T>(Unit movingUnit, uint moveTime, MonsterMoveFlags moveFlags,
            IEnumerable<T> waypoints) where T : IPathVertex
        {
            if (!movingUnit.IsAreaActive)
                return;
            using (RealmPacketOut packet =
                MovementHandler.ConstructMultiWaypointMovePacket<T>(movingUnit, moveTime, moveFlags, waypoints))
                movingUnit.SendPacketToArea(packet, true, false, Locale.Any, new float?());
        }

        public static void SendMoveToPacket<T>(Unit movingUnit, int speed, MonsterMoveFlags moveFlags,
            LinkedListNode<T> firstNode) where T : IPathVertex
        {
            if (!movingUnit.IsAreaActive)
                return;
            using (RealmPacketOut packet =
                MovementHandler.ConstructMultiWaypointMovePacket<T>(movingUnit, speed, moveFlags, firstNode))
                movingUnit.SendPacketToArea(packet, true, false, Locale.Any, new float?());
        }

        public static void SendMoveToPacketToSingleClient<T>(IRealmClient client, Unit movingUnit, uint moveTime,
            MonsterMoveFlags moveFlags, IEnumerable<T> waypoints) where T : IPathVertex
        {
            using (RealmPacketOut packet =
                MovementHandler.ConstructMultiWaypointMovePacket<T>(movingUnit, moveTime, moveFlags, waypoints))
                client.Send(packet, false);
        }

        public static RealmPacketOut ConstructMultiWaypointMovePacket<T>(Unit movingUnit, uint moveTime,
            MonsterMoveFlags moveFlags, IEnumerable<T> waypoints) where T : IPathVertex
        {
            int num = waypoints.Count<T>();
            RealmPacketOut writer =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_MONSTER_MOVE, 38 + num * 4 * 3);
            movingUnit.EntityId.WritePacked((BinaryWriter) writer);
            writer.Write(false);
            writer.Write(movingUnit.Position);
            writer.Write(Utility.GetSystemTime());
            writer.Write((byte) 0);
            writer.Write((uint) moveFlags);
            if (moveFlags.HasFlag((Enum) MonsterMoveFlags.Flag_0x200000))
            {
                writer.Write((byte) 0);
                writer.Write(0);
            }

            writer.Write(moveTime);
            if (moveFlags.HasFlag((Enum) MonsterMoveFlags.Flag_0x800))
            {
                writer.Write(0.0f);
                writer.Write(0);
            }

            writer.Write(num);
            if (moveFlags.HasAnyFlag(MonsterMoveFlags.Flag_0x2000_FullPoints_1 |
                                     MonsterMoveFlags.Flag_0x40000_FullPoints_2))
            {
                foreach (T waypoint in waypoints)
                {
                    IPathVertex pathVertex = (IPathVertex) waypoint;
                    writer.Write(pathVertex.Position);
                }
            }
            else
            {
                foreach (T waypoint in waypoints)
                {
                    IPathVertex pathVertex = (IPathVertex) waypoint;
                    writer.Write(pathVertex.Position.ToDeltaPacked(movingUnit.Position, waypoints.First<T>().Position));
                }
            }

            return writer;
        }

        /// <summary>
        /// Constructs a waypoint packet, starting with the given firstNode (until the end of the LinkedList).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="unit"></param>
        /// <param name="speed">The speed that the Unit should move with in yards/second</param>
        /// <param name="moveFlags"></param>
        /// <param name="firstNode"></param>
        /// <returns></returns>
        public static RealmPacketOut ConstructMultiWaypointMovePacket<T>(Unit unit, int speed,
            MonsterMoveFlags moveFlags, LinkedListNode<T> firstNode) where T : IPathVertex
        {
            RealmPacketOut writer = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_MONSTER_MOVE,
                39 + firstNode.List.Count * 4 * 3);
            unit.EntityId.WritePacked((BinaryWriter) writer);
            writer.Write(false);
            writer.Write(unit.Position);
            writer.Write(Utility.GetSystemTime());
            writer.Write((byte) 0);
            writer.Write((uint) moveFlags);
            if (moveFlags.HasFlag((Enum) MonsterMoveFlags.Flag_0x200000))
            {
                writer.Write((byte) 0);
                writer.Write(0);
            }

            long position1 = writer.Position;
            writer.Position += 4L;
            if (moveFlags.HasFlag((Enum) MonsterMoveFlags.Flag_0x800))
            {
                writer.Write(0.0f);
                writer.Write(0);
            }

            long position2 = writer.Position;
            writer.Position += 4L;
            int num1 = (int) (1000.0 * (double) unit.Position.GetDistance(firstNode.Value.Position) / (double) speed);
            int num2 = 0;
            LinkedListNode<T> linkedListNode = firstNode;
            while (true)
            {
                ++num2;
                writer.Write(linkedListNode.Value.Position);
                LinkedListNode<T> next = linkedListNode.Next;
                if (next != null)
                {
                    num1 += (int) (1000.0 * (double) linkedListNode.Value.GetDistanceToNext() / (double) speed);
                    linkedListNode = next;
                }
                else
                    break;
            }

            writer.Position = position1;
            writer.Write(num1);
            writer.Position = position2;
            writer.Write(num2);
            return writer;
        }

        public static void SendStopMovementPacket(Unit movingUnit)
        {
        }

        public static void SendHeartbeat(Unit unit, Vector3 pos, float orientation)
        {
        }

        public static void SendKnockBack(WorldObject source, WorldObject target, float horizontalSpeed,
            float verticalSpeed)
        {
        }

        public static void SendRooted(Character chr, int unk)
        {
        }

        public static void SendUnrooted(Character chr)
        {
        }

        public static void SendWaterWalk(Character chr)
        {
        }

        public static void SendWalk(Character chr)
        {
        }

        public static void SendMoved(Character chr)
        {
        }

        public static void SendNewWorld(IRealmClient client, MapId map, ref Vector3 pos, float orientation)
        {
        }

        /// <summary>
        /// TODO: Find the difference between SMSG_MOVE_ABANDON_TRANSPORT and MSG_MOVE_ABANDON_TRANSPORT
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="value"></param>
        public static void Send_SMSG_MOVE_ABANDON_TRANSPORT(Unit unit, ushort value)
        {
        }

        public static void SendHoverModeStart(Unit unit)
        {
        }

        public static void SendHoverModeStop(Unit unit)
        {
        }

        public static void SendFeatherModeStart(Unit unit)
        {
        }

        public static void SendFeatherModeStop(Unit unit)
        {
        }

        public static void SendFlyModeStart(Unit unit)
        {
        }

        public static void SendFlyModeStop(Unit unit)
        {
        }

        public static void SendTransferFailure(IPacketReceiver client, MapId mapId, MapTransferError reason)
        {
        }

        public static void SendTransferFailure(IPacketReceiver client, MapId mapId, MapTransferError reason, byte arg)
        {
        }

        public static void SendSetWalkSpeed(Unit unit)
        {
        }

        public static void SendSetRunSpeed(Unit unit)
        {
        }

        public static void SendSetRunBackSpeed(Unit unit)
        {
        }

        public static void SendSetSwimSpeed(Unit unit)
        {
        }

        public static void SendSetSwimBackSpeed(Unit unit)
        {
        }

        public static void SendSetFlightSpeed(Unit unit)
        {
        }

        public static void SendSetFlightBackSpeed(Unit unit)
        {
        }

        public static void SendSetTurnRate(Unit unit)
        {
        }

        public static void SendSetPitchRate(Unit unit)
        {
        }
    }
}