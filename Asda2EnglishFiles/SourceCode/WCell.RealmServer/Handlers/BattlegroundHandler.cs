using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class BattlegroundHandler
    {
        public static void HandleBattlemasterHello(IRealmClient client, RealmPacketIn packet)
        {
            EntityId id = packet.ReadEntityId();
            Character activeCharacter = client.ActiveCharacter;
            NPC bm = activeCharacter.Map.GetObject(id) as NPC;
            if (bm == null || !bm.NPCFlags.HasFlag((Enum) NPCFlags.BattleMaster))
                return;
            bm.TalkToBattlemaster(activeCharacter);
        }

        public static void HandleBattlemasterJoin(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadEntityId();
            BattlegroundId bgId = (BattlegroundId) packet.ReadUInt32();
            uint instanceId = packet.ReadUInt32();
            bool asGroup = packet.ReadBoolean();
            if (bgId <= BattlegroundId.None || bgId >= BattlegroundId.End)
                return;
            BattlegroundMgr.EnqueuePlayers(client.ActiveCharacter, bgId, instanceId, asGroup);
        }

        public static void HandleBattlefieldLeave(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = (int) packet.ReadInt16();
            BattlegroundId bgId = (BattlegroundId) packet.ReadUInt32();
            int num2 = (int) packet.ReadInt16();
            BattlegroundInfo battlegrounds = client.ActiveCharacter.Battlegrounds;
            if (!battlegrounds.IsParticipating(bgId))
                return;
            battlegrounds.TeleportBack();
        }

        public static void HandleBattlemasterJoinArena(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadEntityId();
            int num = (int) packet.ReadByte();
            packet.ReadBoolean();
            packet.ReadBoolean();
        }

        public static void HandlePort(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = (int) packet.ReadInt16();
            BattlegroundId id = (BattlegroundId) packet.ReadUInt32();
            int num2 = (int) packet.ReadInt16();
            bool flag = packet.ReadBoolean();
            Character activeCharacter = client.ActiveCharacter;
            if (flag)
            {
                BattlegroundInvitation invitation = activeCharacter.Battlegrounds.Invitation;
                if (invitation == null || invitation.Team.Battleground == null)
                    return;
                Battleground battleground = invitation.Team.Battleground;
                if (battleground.Template.Id != id)
                    return;
                battleground.TeleportInside(activeCharacter);
            }
            else
                activeCharacter.Battlegrounds.CancelRelation(id);
        }

        public static void HandlePlayerPositionQuery(IRealmClient client, RealmPacketIn packet)
        {
        }

        public static void HandleBattleFieldStatusRequest(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            if (!activeCharacter.Battlegrounds.IsEnqueuedForBattleground)
                return;
            BattlegroundRelation[] relations = activeCharacter.Battlegrounds.Relations;
            int length = activeCharacter.Battlegrounds.Relations.Length;
            for (int index = 0; index < length; ++index)
            {
                BattlegroundRelation relation = relations[index];
                if (relation != null)
                {
                    if (relation.IsEnqueued)
                        BattlegroundHandler.SendStatusEnqueued(activeCharacter, index, relation,
                            relation.Queue.ParentQueue);
                    else if (activeCharacter.Map is Battleground &&
                             relation.BattlegroundId == ((Battleground) activeCharacter.Map).Template.Id)
                        BattlegroundHandler.SendStatusActive(activeCharacter, (Battleground) activeCharacter.Map,
                            index);
                }
            }
        }

        public static void HandleBattlefieldList(IRealmClient client, RealmPacketIn packet)
        {
            BattlegroundId bgid = (BattlegroundId) packet.ReadUInt32();
            packet.ReadBoolean();
            int num = (int) packet.ReadByte();
            BattlegroundTemplate template = BattlegroundMgr.GetTemplate(bgid);
            Character activeCharacter = client.ActiveCharacter;
            if (template == null)
                return;
            GlobalBattlegroundQueue queue = template.GetQueue(activeCharacter.Level);
            if (queue == null)
                return;
            BattlegroundHandler.SendBattlefieldList(activeCharacter, queue);
        }

        public static void SendBattlefieldList(Character chr, GlobalBattlegroundQueue queue)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_BATTLEFIELD_LIST))
            {
                bool flag = true;
                packet.Write(0L);
                packet.Write(flag);
                packet.Write((uint) queue.Template.Id);
                packet.Write((byte) queue.BracketId);
                packet.Write((byte) 0);
                long position = packet.Position;
                packet.Position += 4L;
                int num = 0;
                for (int index = 0; index < queue.Instances.Count; ++index)
                {
                    Battleground instance = queue.Instances[index];
                    if (chr.Role.IsStaff || instance.CanEnter(chr))
                    {
                        packet.Write(instance.InstanceId);
                        ++num;
                    }
                }

                packet.Position = position;
                packet.Write(num);
                chr.Send(packet, false);
            }
        }

        public static void ClearStatus(IPacketReceiver client, int queueIndex)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_BATTLEFIELD_STATUS))
            {
                packet.Write(queueIndex);
                packet.Write(0UL);
                client.Send(packet, false);
            }
        }

        public static void SendStatusEnqueued(Character chr, int index, BattlegroundRelation relation,
            BattlegroundQueue queue)
        {
            BattlegroundStatus battlegroundStatus = BattlegroundStatus.Enqueued;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_BATTLEFIELD_STATUS))
            {
                packet.Write(index);
                BattlegroundId id = queue.Template.Id;
                packet.Write((byte) 0);
                packet.Write((byte) 1);
                packet.Write((uint) id);
                packet.Write((ushort) 8080);
                packet.Write((byte) 0);
                packet.Write((byte) 0);
                packet.Write(queue.InstanceId);
                packet.Write(false);
                packet.Write((int) battlegroundStatus);
                packet.Write(queue.AverageWaitTime);
                packet.Write((int) relation.QueueTime.TotalMilliseconds);
                chr.Send(packet, false);
            }
        }

        /// <summary>
        /// Make sure that <see cref="P:WCell.RealmServer.Battlegrounds.BattlegroundInfo.Invitation" /> is set.
        /// </summary>
        public static void SendStatusInvited(Character chr)
        {
            BattlegroundHandler.SendStatusInvited(chr, BattlegroundMgr.InvitationTimeoutMillis);
        }

        public static void SendStatusInvited(Character chr, int inviteTimeout)
        {
            BattlegroundStatus battlegroundStatus = BattlegroundStatus.Preparing;
            BattlegroundInvitation invitation = chr.Battlegrounds.Invitation;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_BATTLEFIELD_STATUS))
            {
                packet.Write(invitation.QueueIndex);
                Battleground battleground = invitation.Team.Battleground;
                BattlegroundId id = battleground.Template.Id;
                packet.Write((byte) 0);
                packet.Write((byte) 1);
                packet.Write((uint) id);
                packet.Write((ushort) 8080);
                packet.Write((byte) 0);
                packet.Write((byte) 0);
                packet.Write(battleground.InstanceId);
                packet.Write((byte) chr.FactionGroup.GetBattlegroundSide());
                packet.Write((int) battlegroundStatus);
                packet.Write((int) battleground.Id);
                packet.Write(inviteTimeout);
                chr.Send(packet, false);
            }
        }

        public static void SendStatusActive(Character chr, Battleground bg, int queueIndex)
        {
            BattlegroundStatus battlegroundStatus = BattlegroundStatus.Active;
            BattlegroundSide battlegroundSide = chr.FactionGroup.GetBattlegroundSide();
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_BATTLEFIELD_STATUS))
            {
                packet.Write(queueIndex);
                BattlegroundId id = bg.Template.Id;
                packet.Write((byte) 0);
                packet.Write((byte) 1);
                packet.Write((uint) id);
                packet.Write((ushort) 8080);
                packet.Write((byte) 0);
                packet.Write((byte) 0);
                packet.Write(bg.InstanceId);
                packet.Write((byte) 0);
                packet.Write((int) battlegroundStatus);
                packet.Write((int) bg.Id);
                packet.Write(bg.RemainingShutdownDelay);
                packet.Write(bg.RuntimeMillis);
                packet.Write((byte) battlegroundSide);
                chr.Send(packet, false);
            }
        }

        public static void PvPLogDataRequest(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            if (!activeCharacter.IsInBattleground)
                return;
            BattlegroundTeam team = activeCharacter.Battlegrounds.Team;
            if (team == null)
                return;
            BattlegroundHandler.SendPvpData((IPacketReceiver) client, team.Side, team.Battleground);
        }

        public static void SendPvpData(IPacketReceiver reciever, BattlegroundSide side, Battleground bg)
        {
            bg.EnsureContext();
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.MSG_PVP_LOG_DATA, 10 + bg.PlayerCount * 40))
            {
                BattlegroundTeam winner = bg.Winner;
                packet.Write(bg.IsArena);
                if (bg.IsArena)
                {
                    for (int index = 0; index < 2; ++index)
                    {
                        packet.Write(0);
                        packet.Write(3999);
                        packet.Write(0);
                    }

                    packet.WriteCString(string.Empty);
                    packet.WriteCString(string.Empty);
                }

                bool flag = bg.Winner != null;
                packet.Write(flag);
                if (flag)
                    packet.Write((byte) bg.Winner.Side);
                List<Character> characters = bg.Characters;
                List<BattlegroundStats> listStats = new List<BattlegroundStats>(characters.Count);
                characters.ForEach((Action<Character>) (chr => listStats.Add(chr.Battlegrounds.Stats)));
                packet.Write(listStats.Count);
                for (int index = 0; index < listStats.Count; ++index)
                {
                    Character character = characters[index];
                    if (character.IsInBattleground)
                    {
                        BattlegroundStats stats = character.Battlegrounds.Stats;
                        packet.Write((ulong) character.EntityId);
                        packet.Write(stats.KillingBlows);
                        if (bg.IsArena)
                        {
                            packet.Write(winner != null && character.Battlegrounds.Team == winner);
                        }
                        else
                        {
                            packet.Write(stats.HonorableKills);
                            packet.Write(stats.Deaths);
                            packet.Write(stats.BonusHonor);
                        }

                        packet.Write(stats.TotalDamage);
                        packet.Write(stats.TotalHealing);
                        packet.Write(stats.SpecialStatCount);
                        stats.WriteSpecialStats(packet);
                    }
                }

                reciever.Send(packet, false);
            }
        }

        public static void SendPlayerJoined(IPacketReceiver rcv, Character joiningCharacter)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_BATTLEGROUND_PLAYER_JOINED, 8))
            {
                packet.Write((ulong) joiningCharacter.EntityId);
                rcv.Send(packet, false);
            }
        }

        public static void SendPlayerLeft(IPacketReceiver rcv, Character leavingCharacter)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_BATTLEGROUND_PLAYER_LEFT, 8))
            {
                packet.Write((ulong) leavingCharacter.EntityId);
                rcv.Send(packet, false);
            }
        }

        public static void SendBattlegroundError(IPacketReceiver rcv, BattlegroundJoinError err)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_GROUP_JOINED_BATTLEGROUND, 4))
            {
                packet.Write((int) err);
                if (err == BattlegroundJoinError.JoinTimedOut || err == BattlegroundJoinError.JoinFailed)
                    packet.Write(0UL);
                rcv.Send(packet, false);
            }
        }

        /// <summary>"Your group joined Name"</summary>
        /// <param name="battleground"></param>
        public static void SendGroupJoinedBattleground(IPacketReceiver rcv, BattlegroundId battleground)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_GROUP_JOINED_BATTLEGROUND, 4))
            {
                packet.Write((int) battleground);
                rcv.Send(packet, false);
            }
        }

        public static void SendPlayerPositions(IPacketReceiver client, IList<Character> players,
            IList<Character> flagCarriers)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_BATTLEGROUND_PLAYER_POSITIONS))
            {
                if (players != null)
                {
                    packet.Write(players.Count);
                    foreach (Character player in (IEnumerable<Character>) players)
                    {
                        packet.Write((ulong) player.EntityId);
                        packet.Write(player.Position.X);
                        packet.Write(player.Position.Y);
                    }
                }
                else
                    packet.Write(0);

                if (flagCarriers != null)
                {
                    packet.Write(flagCarriers.Count);
                    foreach (Character flagCarrier in (IEnumerable<Character>) flagCarriers)
                    {
                        packet.Write((ulong) flagCarrier.EntityId);
                        packet.Write(flagCarrier.Position.X);
                        packet.Write(flagCarrier.Position.Y);
                    }
                }
                else
                    packet.Write(0);

                client.Send(packet, false);
            }
        }
    }
}