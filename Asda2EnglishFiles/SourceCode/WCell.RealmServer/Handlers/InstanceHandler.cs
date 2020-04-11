using System;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public class InstanceHandler
    {
        public static void HandleInstanceReset(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter == null || !client.ActiveCharacter.HasInstanceCollection)
                return;
            client.ActiveCharacter.Instances.TryResetInstances();
        }

        public static void RequestRaidInfo(IRealmClient client, RealmPacketIn packet)
        {
            InstanceHandler.SendRaidInfo(client.ActiveCharacter);
        }

        public static void HandleRaidDifficulty(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            int num = (int) packet.ReadUInt32();
        }

        public static void HandleDungeonDifficulty(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            if (activeCharacter.Map.IsInstance)
                return;
            uint num = packet.ReadUInt32();
            Group group = activeCharacter.Group;
            if (group != null && group.Leader.Character != activeCharacter ||
                !activeCharacter.Instances.TryResetInstances())
                return;
            if (group != null)
                group.DungeonDifficulty = num;
            else
                activeCharacter.DungeonDifficulty = (DungeonDifficulty) num;
        }

        /// <summary>An instance has been reset</summary>
        public static void SendInstanceReset(IPacketReceiver client, MapId mapId)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_UPDATE_LAST_INSTANCE, 4))
            {
                packet.Write((int) mapId);
                client.Send(packet, false);
            }
        }

        /// <summary>An instance has been saved</summary>
        public static void SendInstanceSave(IPacketReceiver client, MapId mapId)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_UPDATE_LAST_INSTANCE, 4))
            {
                packet.Write((int) mapId);
                client.Send(packet, false);
            }
        }

        /// <summary>Starts the kick timer</summary>
        public static void SendRequiresRaid(IPacketReceiver client, int time)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_RAID_GROUP_ONLY, 8))
            {
                packet.Write(time);
                packet.Write(1);
                client.Send(packet, false);
            }
        }

        /// <summary>Stops the kick timer</summary>
        public static void SendRaidTimerReset(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_RAID_GROUP_ONLY, 8))
            {
                packet.Write(0);
                packet.Write(0);
                client.Send(packet, false);
            }
        }

        public static void SendRaidInfo(Character chr)
        {
            RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_RAID_INSTANCE_INFO);
            try
            {
                if (chr.HasInstanceCollection)
                {
                    packet.Position += 4L;
                    uint count = 0;
                    chr.Instances.ForeachBinding(BindingType.Hard, (Action<InstanceBinding>) (binding =>
                    {
                        TimeSpan timeSpan = binding.NextResetTime - DateTime.Now;
                        if (timeSpan.Ticks <= 0L)
                            return;
                        ++count;
                        packet.Write((uint) binding.MapId);
                        packet.Write(binding.DifficultyIndex);
                        packet.Write(binding.InstanceId);
                        packet.WriteByte(1);
                        packet.WriteByte(0);
                        packet.Write((uint) timeSpan.TotalSeconds);
                    }));
                    packet.Position = (long) packet.HeaderSize;
                    packet.Write(count);
                }
                else
                    packet.Write(0);

                chr.Client.Send(packet, false);
            }
            finally
            {
                if (packet != null)
                    packet.Dispose();
            }
        }

        /// <summary>Sends the result of an instance reset attempt</summary>
        /// <param name="client"></param>
        /// <param name="reason"></param>
        public static void SendResetFailure(IPacketReceiver client, MapId map, InstanceResetFailed reason)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_INSTANCE_RESET_FAILED, 8))
            {
                packet.Write((uint) reason);
                packet.Write((uint) map);
                client.Send(packet, false);
            }
        }

        /// <summary>
        /// Warns a player within the instance that the leader is attempting to reset the instance
        /// </summary>
        /// <param name="client"></param>
        public static void SendResetWarning(IPacketReceiver client, MapId map)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_RESET_FAILED_NOTIFY, 4))
            {
                packet.Write((uint) map);
                client.Send(packet, false);
            }
        }

        public static void SendRaidDifficulty(Character chr)
        {
            using (RealmPacketOut realmPacketOut =
                new RealmPacketOut((PacketId) RealmServerOpCode.MSG_SET_RAID_DIFFICULTY, 12))
            {
                Group group = chr.Group;
                if (group is RaidGroup)
                {
                    realmPacketOut.Write(group.DungeonDifficulty);
                    realmPacketOut.Write(1);
                    realmPacketOut.Write(1);
                }
                else
                {
                    realmPacketOut.Write((int) chr.Record.DungeonDifficulty);
                    realmPacketOut.Write(1);
                    realmPacketOut.Write(0);
                }
            }
        }

        public static void SendDungeonDifficulty(Character chr)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.MSG_SET_DUNGEON_DIFFICULTY, 12))
            {
                Group group = chr.Group;
                if (group != null && !group.Flags.HasFlag((Enum) GroupFlags.Raid))
                {
                    packet.Write(group.DungeonDifficulty);
                    packet.Write(1);
                    packet.Write(1);
                }
                else
                {
                    packet.Write((int) chr.Record.DungeonDifficulty);
                    packet.Write(1);
                    packet.Write(0);
                }

                chr.Send(packet, false);
            }
        }
    }
}