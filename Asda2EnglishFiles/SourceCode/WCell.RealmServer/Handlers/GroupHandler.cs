using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
    public static class GroupHandler
    {
        /// <summary>
        /// Handles an incoming group invite request (/invite Player)
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void GroupInviteRequest(IRealmClient client, RealmPacketIn packet)
        {
            string str = packet.ReadCString();
            Character activeCharacter = client.ActiveCharacter;
            Group group = activeCharacter.Group;
            Character target;
            if (Group.CheckInvite(activeCharacter, out target, str) != GroupResult.NoError)
                return;
            HashSet<IBaseRelation> relations =
                Singleton<RelationMgr>.Instance.GetRelations(activeCharacter.EntityId.Low,
                    CharacterRelationType.GroupInvite);
            if (group != null && relations.Count >= (int) group.InvitesLeft)
                return;
            Singleton<RelationMgr>.Instance.AddRelation(RelationMgr.CreateRelation(activeCharacter.EntityId.Low,
                target.EntityId.Low, CharacterRelationType.GroupInvite));
            Group.SendResult((IPacketReceiver) activeCharacter.Client, GroupResult.NoError, str);
            GroupHandler.SendGroupInvite((IPacketReceiver) target.Client, activeCharacter.Name);
        }

        /// <summary>Handles an incoming accept on group invite request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void GroupAccept(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            IBaseRelation relation = Singleton<RelationMgr>.Instance
                .GetPassiveRelations(activeCharacter.EntityId.Low, CharacterRelationType.GroupInvite)
                .FirstOrDefault<IBaseRelation>();
            if (relation == null)
                return;
            Singleton<RelationMgr>.Instance.RemoveRelation(relation);
            Character character = World.GetCharacter(relation.CharacterId);
            if (character == null)
                return;
            (character.Group ?? (Group) new PartyGroup(character)).AddMember(activeCharacter, true);
        }

        /// <summary>Handles an incoming decline on group invite request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void GroupDecline(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            IBaseRelation relation = Singleton<RelationMgr>.Instance
                .GetPassiveRelations(activeCharacter.EntityId.Low, CharacterRelationType.GroupInvite)
                .FirstOrDefault<IBaseRelation>();
            if (relation == null)
                return;
            Singleton<RelationMgr>.Instance.RemoveRelation(relation);
            Character character = World.GetCharacter(relation.CharacterId);
            if (character == null)
                return;
            GroupHandler.SendGroupDecline((IPacketReceiver) character.Client, activeCharacter.Name);
        }

        /// <summary>
        /// Handles an incoming request on uninviting a Character from the group by name
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void GroupUninviteByName(IRealmClient client, RealmPacketIn packet)
        {
            string index = packet.ReadCString();
            if (index.Length == 0)
                return;
            GroupMember groupMember = client.ActiveCharacter.GroupMember;
            if (groupMember == null || !groupMember.IsLeader)
                return;
            Group group = groupMember.Group;
            GroupMember member = group[index];
            if (member == null)
                return;
            group.RemoveMember(member);
        }

        /// <summary>
        /// Handles an incoming request on uninviting a Character from the group by GUID
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleGroupUninviteByGUID(IRealmClient client, RealmPacketIn packet)
        {
            EntityId entityId = packet.ReadEntityId();
            int num = (int) packet.ReadByte();
            GroupMember groupMember = client.ActiveCharacter.GroupMember;
            if (groupMember == null || !groupMember.IsLeader)
                return;
            Group group = groupMember.Group;
            GroupMember member = group[entityId.Low];
            if (member == null || member.SubGroup.Group != group)
                return;
            group.RemoveMember(member);
        }

        /// <summary>Handles an incoming request on leader change</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void GroupSetLeader(IRealmClient client, RealmPacketIn packet)
        {
            EntityId entityId = packet.ReadEntityId();
            GroupMember groupMember = client.ActiveCharacter.GroupMember;
            if (groupMember == null)
                return;
            Group group = groupMember.Group;
            GroupMember target = group[entityId.Low];
            if (group.CheckAction(groupMember, target, target != null ? target.Name : string.Empty,
                    GroupPrivs.Leader) != GroupResult.NoError)
                return;
            group.Leader = target;
            group.SendUpdate();
        }

        /// <summary>
        /// Handles an incoming group disband packet (used for leaving party in fact)
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void GroupDisband(IRealmClient client, RealmPacketIn packet)
        {
            GroupMember groupMember = client.ActiveCharacter.GroupMember;
            if (groupMember == null)
                return;
            groupMember.LeaveGroup();
        }

        /// <summary>Handles an incoming request on loot method change</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void GroupSetLootMethod(IRealmClient client, RealmPacketIn packet)
        {
            GroupMember groupMember1 = client.ActiveCharacter.GroupMember;
            if (groupMember1 == null)
                return;
            LootMethod method = (LootMethod) packet.ReadUInt32();
            EntityId entityId = packet.ReadEntityId();
            ItemQuality lootThreshold = (ItemQuality) packet.ReadUInt32();
            Group group = groupMember1.Group;
            GroupMember groupMember2 = group[entityId.Low];
            if (!(groupMember2 == null
                ? group.CheckPrivs(groupMember1, GroupPrivs.Leader)
                : group.CheckAction(groupMember1, groupMember2, groupMember2.Name, GroupPrivs.Leader) ==
                  GroupResult.NoError))
                return;
            group.SetLootMethod(method, groupMember2, lootThreshold);
        }

        /// <summary>Handles an incoming minimap ping</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void MinimapPing(IRealmClient client, RealmPacketIn packet)
        {
            float x = packet.ReadFloat();
            float y = packet.ReadFloat();
            GroupMember groupMember = client.ActiveCharacter.GroupMember;
            if (groupMember == null)
                return;
            groupMember.Group.SendPing(groupMember, x, y);
        }

        /// <summary>Handles an incoming request on random roll</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void RandomRollRequest(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = packet.ReadInt32();
            int num2 = packet.ReadInt32();
            if (num1 > num2 || num2 > 10000)
                return;
            int roll = new Random().Next(num1, num2);
            Group group = client.ActiveCharacter.Group;
            if (group == null)
                GroupHandler.SendRoll((IPacketReceiver) client, num1, num2, roll, client.ActiveCharacter.EntityId);
            else
                group.SendRoll(num1, num2, roll, client.ActiveCharacter.EntityId);
        }

        /// <summary>Handles an incoming request on random roll</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void RaidIconTarget(IRealmClient client, RealmPacketIn packet)
        {
            GroupMember groupMember = client.ActiveCharacter.GroupMember;
            if (groupMember == null)
                return;
            Group group = groupMember.Group;
            byte iconId = packet.ReadByte();
            if (iconId == byte.MaxValue)
            {
                group.SendTargetIconList(client.ActiveCharacter);
            }
            else
            {
                if (!group.CheckPrivs(groupMember, GroupPrivs.Assistant))
                    return;
                EntityId targetId = packet.ReadEntityId();
                group.SetTargetIcon(iconId, client.ActiveCharacter.EntityId, targetId);
            }
        }

        /// <summary>Handles an incoming convert party to raid request</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void ConvertToRaidRequest(IRealmClient client, RealmPacketIn packet)
        {
            GroupMember groupMember = client.ActiveCharacter.GroupMember;
            if (groupMember == null)
                return;
            Group group1 = groupMember.Group;
            if (!(group1 is PartyGroup) || !group1.CheckPrivs(groupMember, GroupPrivs.Leader))
                return;
            Group group2 = (Group) ((PartyGroup) group1).ConvertTo();
            GroupHandler.SendResult((IPacketReceiver) client, GroupResult.NoError);
            group2.SendUpdate();
        }

        /// <summary>
        /// Handles an incoming request on moving Character from one subgroup to another
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void ChangeSubgroupRequest(IRealmClient client, RealmPacketIn packet)
        {
            GroupMember groupMember1 = client.ActiveCharacter.GroupMember;
            if (groupMember1 == null)
                return;
            RaidGroup group1 = groupMember1.Group as RaidGroup;
            if (group1 == null)
                return;
            string targetName = packet.ReadCString();
            byte num = packet.ReadByte();
            GroupMember groupMember2 = group1[targetName];
            if (group1.CheckAction(groupMember1, groupMember2, targetName, GroupPrivs.Assistant) != GroupResult.NoError)
                return;
            SubGroup group2 = group1.SubGroups.Get<SubGroup>((int) num);
            if (group2 == null)
                return;
            if (!group1.MoveMember(groupMember2, group2))
                GroupHandler.SendResult((IPacketReceiver) client, GroupResult.GroupIsFull);
            else
                group1.SendUpdate();
        }

        /// <summary>
        /// Handles an incoming request on changing Assistant flag of specified Character
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void ChangeAssistantFlagRequest(IRealmClient client, RealmPacketIn packet)
        {
            GroupMember groupMember = client.ActiveCharacter.GroupMember;
            if (groupMember == null)
                return;
            RaidGroup group = groupMember.Group as RaidGroup;
            if (group == null)
                return;
            EntityId entityId = packet.ReadEntityId();
            GroupMember target = group[entityId.Low];
            if (group.CheckAction(groupMember, target, target != null ? target.Name : string.Empty,
                    GroupPrivs.Leader) != GroupResult.NoError)
                return;
            bool flag = packet.ReadBoolean();
            target.IsAssistant = flag;
            group.SendUpdate();
        }

        /// <summary>
        /// Handles an incoming request for changing the Main Tank or Main Assistant flag
        /// of specified Character
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void GroupPromoteFlagRequest(IRealmClient client, RealmPacketIn packet)
        {
            GroupMember groupMember = client.ActiveCharacter.GroupMember;
            if (groupMember == null)
                return;
            RaidGroup group = groupMember.Group as RaidGroup;
            if (group == null)
                return;
            byte num = packet.ReadByte();
            packet.ReadBoolean();
            EntityId entityId = packet.ReadEntityId();
            GroupMember target = group[entityId.Low];
            if (group.CheckAction(groupMember, target, target != null ? target.Name : string.Empty,
                    GroupPrivs.Leader) != GroupResult.NoError)
                return;
            if (num != (byte) 0)
                group.MainAssistant = target;
            group.SendUpdate();
        }

        /// <summary>
        /// Handles an incoming request or answer to raid ready check
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void RaidReadyCheck(IRealmClient client, RealmPacketIn packet)
        {
            GroupMember groupMember = client.ActiveCharacter.GroupMember;
            if (groupMember == null)
                return;
            RaidGroup group = groupMember.Group as RaidGroup;
            if (group == null)
                return;
            if (packet.RemainingLength == 0)
            {
                if (!group.CheckPrivs(groupMember, GroupPrivs.Assistant))
                    return;
                group.SendReadyCheckRequest(groupMember);
            }
            else
            {
                ReadyCheckStatus status = (ReadyCheckStatus) packet.ReadByte();
                group.SendReadyCheckResponse(groupMember, status);
            }
        }

        /// <summary>Handles an incoming request for a group member status</summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void RequestPartyMemberStats(IRealmClient client, RealmPacketIn packet)
        {
            EntityId entityId = packet.ReadEntityId();
            Group group = client.ActiveCharacter.Group;
            if (group == null)
                return;
            GroupMember member = group.GetMember(entityId.Low);
            if (member == null)
                return;
            GroupUpdateFlags groupUpdateFlags = GroupUpdateFlags.None;
            GroupUpdateFlags flags = member.Character == null
                ? groupUpdateFlags | GroupUpdateFlags.Status
                : groupUpdateFlags | GroupUpdateFlags.UpdateFull;
            GroupHandler.SendPartyMemberStatsFull((IPacketReceiver) client, member, flags);
        }

        public static void HandleMeetingStoneInfoRequest(IRealmClient client, RealmPacketIn packet)
        {
            GroupHandler.SendMeetingStoneSetQueue((IPacketReceiver) client.ActiveCharacter);
        }

        public static void HandleSetAllowLowLevelRaid1(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.IsAllowedLowLevelRaid = packet.ReadBoolean();
        }

        public static void SendLeaderChanged(GroupMember leader)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_SET_LEADER))
            {
                packet.WriteCString(leader.Name);
                leader.SubGroup.Group.SendAll(packet);
            }
        }

        /// <summary>Sends result of actions connected with groups</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="resultCode">The <see cref="T:WCell.Constants.GroupResult" /> result code</param>
        public static void SendResult(IPacketReceiver client, GroupResult resultCode)
        {
            Group.SendResult(client, resultCode, 0U, string.Empty);
        }

        /// <summary>Send Group Invite packet</summary>
        /// <param name="client">realm client</param>
        /// <param name="inviter">nick of player invited you</param>
        public static void SendGroupInvite(IPacketReceiver client, string inviter)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_GROUP_INVITE, inviter.Length + 10))
            {
                packet.Write((byte) 1);
                packet.WriteCString(inviter);
                packet.Write(0U);
                packet.Write((byte) 0);
                packet.Write(0U);
                client.Send(packet, false);
            }
        }

        /// <summary>Sends group invitation decline</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="decliner">player who has declined your request</param>
        public static void SendGroupDecline(IPacketReceiver client, string decliner)
        {
            using (RealmPacketOut packet =
                new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_GROUP_DECLINE, decliner.Length + 1))
            {
                packet.WriteCString(decliner);
                client.Send(packet, false);
            }
        }

        /// <summary>Sends roll results to the group</summary>
        /// <param name="client">realm client</param>
        /// <param name="min">minimal value</param>
        /// <param name="max">maximal value</param>
        /// <param name="value">value rolled out</param>
        /// <param name="guid">guid of roller</param>
        public static void SendRoll(IPacketReceiver client, int min, int max, int value, EntityId guid)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.MSG_RANDOM_ROLL, 20))
            {
                packet.Write(min);
                packet.Write(max);
                packet.Write(value);
                packet.Write(guid.Full);
                client.Send(packet, false);
            }
        }

        /// <summary>
        /// Sends the requested party member stats data to the client
        /// </summary>
        /// <param name="client">realm client</param>
        /// <param name="member">The character whose stats is going to be retrieved</param>
        /// <param name="flags">The stats to be retrieved from the <paramref name="member" /></param>
        public static void SendPartyMemberStatsFull(IPacketReceiver client, GroupMember member, GroupUpdateFlags flags)
        {
            GroupHandler.SendPartyMemberStatsInternal(client, member, flags,
                RealmServerOpCode.SMSG_PARTY_MEMBER_STATS_FULL);
        }

        /// <summary>
        /// Sends the requested party member stats data to the client
        /// </summary>
        /// <param name="client">realm client</param>
        /// <param name="member">The character whose stats is going to be retrieved</param>
        /// <param name="flags">The stats to be retrieved from the <paramref name="member" /></param>
        public static void SendPartyMemberStats(IPacketReceiver client, GroupMember member, GroupUpdateFlags flags)
        {
            GroupHandler.SendPartyMemberStatsInternal(client, member, flags, RealmServerOpCode.SMSG_PARTY_MEMBER_STATS);
        }

        /// <summary>
        /// Sends the requested party member stats data to the client
        /// </summary>
        /// <param name="client">realm client</param>
        /// <param name="member">The character whose stats is going to be retrieved</param>
        /// <param name="flags">The stats to be retrieved from the <paramref name="member" /></param>
        private static void SendPartyMemberStatsInternal(IPacketReceiver client, GroupMember member,
            GroupUpdateFlags flags, RealmServerOpCode opcode)
        {
            using (RealmPacketOut packet = new RealmPacketOut(opcode))
            {
                if (opcode == RealmServerOpCode.SMSG_PARTY_MEMBER_STATS_FULL)
                    packet.Write((byte) 0);
                member.WriteIdPacked(packet);
                if (!member.IsOnline)
                {
                    packet.WriteUShort((ushort) 0);
                    client.Send(packet, false);
                }
                else
                {
                    packet.Write((uint) flags);
                    Character character = member.Character;
                    if (flags.HasFlag((Enum) GroupUpdateFlags.Status))
                        packet.Write((ushort) character.Status);
                    if (flags.HasFlag((Enum) GroupUpdateFlags.Health))
                        packet.Write(character.Health);
                    if (flags.HasFlag((Enum) GroupUpdateFlags.MaxHealth))
                        packet.Write(character.MaxHealth);
                    if (flags.HasFlag((Enum) GroupUpdateFlags.PowerType))
                        packet.Write((byte) character.PowerType);
                    if (flags.HasFlag((Enum) GroupUpdateFlags.Power))
                        packet.Write((ushort) character.Power);
                    if (flags.HasFlag((Enum) GroupUpdateFlags.MaxPower))
                        packet.Write((ushort) character.MaxPower);
                    if (flags.HasFlag((Enum) GroupUpdateFlags.Level))
                        packet.Write((ushort) character.Level);
                    if (flags.HasFlag((Enum) GroupUpdateFlags.ZoneId))
                        packet.Write(character.Zone != null ? (ushort) character.Zone.Id : (ushort) 0);
                    if (flags.HasFlag((Enum) GroupUpdateFlags.Position))
                    {
                        packet.Write((ushort) character.Position.X);
                        packet.Write((ushort) character.Position.Y);
                    }

                    if (flags.HasFlag((Enum) GroupUpdateFlags.Auras))
                    {
                        ulong auraUpdateMask = character.AuraUpdateMask;
                        packet.Write(auraUpdateMask);
                        for (byte index = 0; index < (byte) 56; ++index)
                        {
                            if (((long) auraUpdateMask & 1L << (int) index) != 0L)
                            {
                                Aura at = character.Auras.GetAt((uint) index);
                                packet.Write(at.Spell.Id);
                                packet.Write((byte) at.Flags);
                            }
                        }
                    }

                    NPC activePet = character.ActivePet;
                    if (activePet == null)
                    {
                        packet.Write((byte) 0);
                        packet.Write(0UL);
                        client.Send(packet, false);
                    }
                    else
                    {
                        if (flags.HasFlag((Enum) GroupUpdateFlags.PetGuid))
                            packet.Write((ulong) activePet.EntityId);
                        if (flags.HasFlag((Enum) GroupUpdateFlags.PetName))
                            packet.WriteCString(activePet.Name);
                        if (flags.HasFlag((Enum) GroupUpdateFlags.PetDisplayId))
                            packet.Write((ushort) activePet.DisplayId);
                        if (flags.HasFlag((Enum) GroupUpdateFlags.PetHealth))
                            packet.Write(activePet.Health);
                        if (flags.HasFlag((Enum) GroupUpdateFlags.PetMaxHealth))
                            packet.Write(activePet.MaxHealth);
                        if (flags.HasFlag((Enum) GroupUpdateFlags.PetPowerType))
                            packet.Write((byte) activePet.PowerType);
                        if (flags.HasFlag((Enum) GroupUpdateFlags.PetPower))
                            packet.Write((ushort) activePet.Power);
                        if (flags.HasFlag((Enum) GroupUpdateFlags.PetMaxPower))
                            packet.Write((ushort) activePet.MaxPower);
                        if (flags.HasFlag((Enum) GroupUpdateFlags.PetAuras))
                        {
                            ulong auraUpdateMask = activePet.AuraUpdateMask;
                            packet.Write(auraUpdateMask);
                            for (byte index = 0; index < (byte) 56; ++index)
                            {
                                if (((long) auraUpdateMask & 1L << (int) index) != 0L)
                                {
                                    Aura at = activePet.Auras.GetAt((uint) index);
                                    packet.Write(at.Spell.Id);
                                    packet.Write((byte) at.Flags);
                                }
                            }
                        }

                        client.Send(packet, false);
                    }
                }
            }
        }

        private static void SendMeetingStoneSetQueue(IPacketReceiver client)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_MEETINGSTONE_SETQUEUE))
            {
                packet.Write(0U);
                packet.Write((byte) 6);
                client.Send(packet, false);
            }
        }
    }
}