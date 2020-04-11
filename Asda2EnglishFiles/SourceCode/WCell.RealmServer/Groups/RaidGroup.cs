using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Groups
{
    /// <summary>Represents a raid group.</summary>
    public sealed class RaidGroup : Group
    {
        public const byte MaxSubGroupCount = 8;
        private GroupMember m_readyCheckRequester;

        /// <summary>
        /// Creates a raid group with the given character as the leader.
        /// </summary>
        /// <param name="leader">the character to be the leader</param>
        public RaidGroup(Character leader)
            : base(leader, (byte) 8)
        {
            this.m_readyCheckRequester = (GroupMember) null;
        }

        /// <summary>
        /// Creates a raid group from an existing party group.
        /// TODO: This looks wrong
        /// </summary>
        /// <param name="group">the group to convert</param>
        public RaidGroup(Group group)
            : this(group.Leader.Character)
        {
            foreach (GroupMember groupMember in group)
            {
                if (groupMember != this.m_firstMember)
                    this.AddMember(groupMember.Character, false);
            }
        }

        /// <summary>The type of group.</summary>
        public override GroupFlags Flags
        {
            get { return GroupFlags.Raid; }
        }

        /// <summary>Sets whether or not the given player is an assistant.</summary>
        /// <param name="member">the member to set</param>
        /// <param name="isAssistant">whether or not the member is an assistant</param>
        public void SetAssistant(Character member, bool isAssistant)
        {
            GroupMember groupMember = this[member.EntityId.Low];
            if (groupMember == null)
                return;
            if (isAssistant)
                groupMember.Flags |= GroupMemberFlags.Assistant;
            else
                groupMember.Flags &= ~GroupMemberFlags.Assistant;
        }

        /// <summary>
        /// Sets whether or not the given player is the main assistant.
        /// </summary>
        /// <param name="member">the member to set</param>
        /// <param name="add">whether or not the member is an assistant</param>
        public void SetMainAssistant(Character member, bool add)
        {
            GroupMember groupMember = this[member.EntityId.Low];
            if (groupMember == null)
                return;
            if (add)
                groupMember.Flags |= GroupMemberFlags.MainAssistant;
            else
                groupMember.Flags &= ~GroupMemberFlags.MainAssistant;
        }

        /// <summary>
        /// Moves a member of the raid group into another subgroup.
        /// </summary>
        /// <param name="member">the member to move</param>
        /// <param name="group">the target subgroup</param>
        /// <returns>Whether the move was successful or false if the target group was full</returns>
        public bool MoveMember(GroupMember member, SubGroup group)
        {
            if (group.IsFull)
                return false;
            member.SubGroup.m_members.Remove(member);
            group.AddMember(member);
            return true;
        }

        /// <summary>
        /// Sends a ready check request to the members of the raid group.
        /// </summary>
        /// <param name="member">the member who requested the check</param>
        public void SendReadyCheckRequest(GroupMember member)
        {
            this.m_readyCheckRequester = member;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_RAID_READY_CHECK))
            {
                packet.Write((ulong) EntityId.GetPlayerId(member.Id));
                this.SendAll(packet, member);
            }
        }

        /// <summary>
        /// Sends the response of a member to the original ready check requester.
        /// </summary>
        /// <param name="member">the responding member</param>
        /// <param name="status">their ready status</param>
        public void SendReadyCheckResponse(GroupMember member, ReadyCheckStatus status)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_RAID_READY_CHECK_CONFIRM))
            {
                packet.Write((ulong) EntityId.GetPlayerId(member.Id));
                packet.WriteByte((byte) status);
                Character character = World.GetCharacter(this.m_readyCheckRequester.Id);
                if (character == null)
                    return;
                character.Client.Send(packet, false);
            }
        }
    }
}