using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Network;
using WCell.Util.Collections;

namespace WCell.RealmServer.Groups
{
    /// <summary>TODO: Group-Tracking (including buffs/debuffs)</summary>
    public sealed class GroupMgr : Manager<GroupMgr>
    {
        /// <summary>
        /// Maps char-id to the corresponding GroupMember object so it can be looked up when char reconnects
        /// </summary>
        internal readonly IDictionary<uint, GroupMember> OfflineChars;

        private GroupMgr()
        {
            this.OfflineChars = (IDictionary<uint, GroupMember>) new SynchronizedDictionary<uint, GroupMember>(100);
        }

        /// <summary>
        /// Removes an offline Character with the given Id
        /// from his/her Group
        /// </summary>
        /// <param name="id"></param>
        public bool RemoveOfflineCharacter(uint id)
        {
            GroupMember groupMember;
            if (!this.OfflineChars.TryGetValue(id, out groupMember))
                return false;
            groupMember.LeaveGroup();
            return true;
        }

        internal void OnCharacterLogin(Character chr)
        {
            GroupMember groupMember;
            if (!this.OfflineChars.TryGetValue(chr.EntityId.Low, out groupMember))
                return;
            this.OfflineChars.Remove(chr.EntityId.Low);
            groupMember.Character = chr;
            Group group = groupMember.Group;
            if (group.Leader == null)
                group.Leader = groupMember;
            else
                group.SendUpdate();
            chr.GroupUpdateFlags |= GroupUpdateFlags.Status;
        }

        /// <summary>
        /// Cleanup character invitations and group leader, looter change on character logout/disconnect
        /// </summary>
        /// <param name="member">The GroupMember logging out / disconnecting (or null if the corresponding Character is not in a Group)</param>
        internal void OnCharacterLogout(GroupMember member)
        {
            if (member == null)
                return;
            foreach (IBaseRelation relation in Singleton<RelationMgr>.Instance
                .GetRelations(member.Character.EntityId.Low, CharacterRelationType.GroupInvite).ToList<IBaseRelation>())
                Singleton<RelationMgr>.Instance.RemoveRelation(relation);
            Group group = member.Group;
            member.LeaveGroup();
            group.SendUpdate();
        }

        private static bool CheckIsLeader(GroupMember member)
        {
            if (member.IsLeader)
                return true;
            Character character = member.Character;
            if (character != null)
                GroupHandler.SendResult((IPacketReceiver) character.Client, GroupResult.DontHavePermission);
            return false;
        }

        private static bool CheckMemberInGroup(Character requester, Group group, uint memberLowId)
        {
            if (group[memberLowId] != null)
                return true;
            GroupHandler.SendResult((IPacketReceiver) requester.Client, GroupResult.NotInYourParty);
            return false;
        }

        private static bool CheckSameGroup(Character requester, Character character)
        {
            if (requester.Group == character.Group)
                return true;
            Group.SendResult((IPacketReceiver) requester.Client, GroupResult.NotInYourParty, character.Name);
            return false;
        }
    }
}