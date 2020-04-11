using System.IO;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    /// <summary>Packets that don't fit into any other category</summary>
    public static class MiscHandler
    {
        public static void HandleRealmStateRequest(IRealmClient client, RealmPacketIn packet)
        {
            uint realmNo = packet.ReadUInt32();
            MiscHandler.SendRealmStateResponse((IPacketReceiver) client, realmNo);
        }

        /// <summary>Handles an incoming ping request.</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void PingRequest(IRealmClient client, RealmPacketIn packet)
        {
            MiscHandler.SendPingReply((IPacketReceiver) client, packet.ReadUInt32());
            client.Latency = packet.ReadInt32();
        }

        public static void HandleTogglePvP(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            if (packet.ContentLength > 0)
            {
                bool state = packet.ReadBoolean();
                activeCharacter.SetPvPFlag(state);
            }
            else
                activeCharacter.TogglePvPFlag();
        }

        public static void SendRealmStateResponse(IPacketReceiver client, uint realmNo)
        {
            string str = "01/01/01";
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_REALM_SPLIT, 9 + str.Length))
            {
                packet.WriteUInt(realmNo);
                packet.WriteUInt(0U);
                packet.WriteCString(str);
                client.Send(packet, false);
            }
        }

        /// <summary>Sends a ping reply to the client.</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="sequence">the sequence number sent by client</param>
        public static void SendPingReply(IPacketReceiver client, uint sequence)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PONG, 4))
            {
                packet.Write(sequence);
                client.Send(packet, false);
            }
        }

        /// <summary>
        /// Sends the given list of messages as motd (displays as a regular system-msg)
        /// </summary>
        public static void SendMotd(IPacketReceiver client, params string[] messages)
        {
        }

        /// <summary>Flashes a message in the middle of the screen.</summary>
        public static void SendNotification(IPacketReceiver client, string msg)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_NOTIFICATION))
            {
                packet.WriteCString(msg);
                client.Send(packet, false);
            }
        }

        public static void SendCancelAutoRepeat(IPacketReceiver client, IEntity entity)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CANCEL_AUTO_REPEAT))
            {
                entity.EntityId.WritePacked((BinaryWriter) packet);
                client.Send(packet, false);
            }
        }

        public static void SendHealthUpdate(Unit unit, int health)
        {
        }

        public static void SendPlayObjectSound(WorldObject obj, uint sound)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PLAY_OBJECT_SOUND, 13))
            {
                packet.Write(sound);
                packet.Write((ulong) obj.EntityId);
                obj.SendPacketToArea(packet, true, false, Locale.Any, new float?());
            }
        }

        public static void SendPlaySoundToMap(Map map, uint sound)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_PLAY_SOUND, 4))
            {
                packet.WriteUInt(sound);
                map.SendPacketToMap(packet);
            }
        }

        public static void SendPlayMusic(WorldObject obj, uint sound, float range)
        {
        }

        public static void SendGameObjectTextPage(IPacketReceiver rcv, IEntity obj)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_GAMEOBJECT_PAGETEXT, 8))
            {
                packet.Write((ulong) obj.EntityId);
                rcv.Send(packet, false);
            }
        }
    }
}