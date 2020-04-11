using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.Misc;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Privileges;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.Groups
{
    /// <summary>
    /// Base group class for every group type.
    /// Don't forget to lock the SyncRoot while iterating over a Group.
    /// </summary>
    public abstract class Group : IInstanceHolderSet, ICharacterSet, IPacketReceiver, IEnumerable<GroupMember>,
        IEnumerable
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The time after which a member who left a Group will be teleported out of a Group owned instance in seconds.
        /// </summary>
        public static int GroupInstanceKickDelayMillis = 60000;

        /// <summary>Max distance in yards to be rewarded for a group kill</summary>
        public static float MaxKillRewardDistance = 100f;

        /// <summary>Minimun level to be invited in a raid</summary>
        public static int MinLevelToBeInvitedInRaid = 10;

        protected const byte MinGroupMemberCount = 2;
        protected const byte TargetIconCount = 8;
        protected SubGroup[] m_subGroups;
        protected GroupMember m_leader;
        protected GroupMember m_masterLooter;
        protected GroupMember m_mainAssistant;
        protected GroupMember m_mainTank;
        protected LootMethod m_lootMethod;
        protected ItemQuality m_lootThreshold;
        protected uint m_DungeonDifficulty;
        protected EntityId[] m_targetIcons;
        protected int m_Count;
        protected GroupMember m_roundRobinMember;
        protected internal GroupMember m_firstMember;
        protected internal GroupMember m_lastMember;
        protected internal ReaderWriterLockWrapper syncLock;

        protected Group(Character leader, byte maxGroupUnits)
        {
            this.syncLock = new ReaderWriterLockWrapper();
            this.m_subGroups = new SubGroup[(int) maxGroupUnits];
            for (byte groupUnitId = 0; (int) groupUnitId < (int) maxGroupUnits; ++groupUnitId)
                this.m_subGroups[(int) groupUnitId] = new SubGroup(this, groupUnitId);
            this.m_targetIcons = new EntityId[8];
            for (int index = 0; index < this.m_targetIcons.Length; ++index)
                this.m_targetIcons[index] = EntityId.Zero;
            this.m_lootMethod = LootMethod.GroupLoot;
            this.m_lootThreshold = ItemQuality.Uncommon;
            this.m_DungeonDifficulty = 0U;
            this.Leader = this.AddMember(leader, false);
            this.m_masterLooter = (GroupMember) null;
        }

        /// <summary>
        /// The chosen LootMethod.
        /// Make sure to send an Update after changing this, so the Group is informed about the change.
        /// </summary>
        public LootMethod LootMethod
        {
            get { return this.m_lootMethod; }
            set
            {
                if (value >= LootMethod.End)
                    this.m_lootMethod = LootMethod.GroupLoot;
                else
                    this.m_lootMethod = value;
            }
        }

        /// <summary>
        /// The least Quality of Items to be handled by the MasterLooter or to be rolled for.
        /// Make sure to send an Update after changing this, so the Group is informed about the change.
        /// </summary>
        public ItemQuality LootThreshold
        {
            get { return this.m_lootThreshold; }
            set { this.m_lootThreshold = value; }
        }

        /// <summary>
        /// The DungeonDifficulty.
        /// Make sure to send an Update after changing this, so the Group is informed about the change.
        /// </summary>
        public uint DungeonDifficulty
        {
            get { return this.m_DungeonDifficulty; }
            set
            {
                if ((int) value == (int) this.m_DungeonDifficulty)
                    return;
                this.m_DungeonDifficulty = value;
                if (this.IsBattleGroup)
                    return;
                foreach (Character allCharacter in this.GetAllCharacters())
                {
                    if (this.Flags.HasFlag((Enum) GroupFlags.Raid))
                        InstanceHandler.SendRaidDifficulty(allCharacter);
                    else
                        InstanceHandler.SendDungeonDifficulty(allCharacter);
                }
            }
        }

        /// <summary>
        /// All SubGroups of this Group.
        /// By default there is only 1 SubGroup, except for RaidGroups which always have 8 (some might be empty)
        /// </summary>
        public SubGroup[] SubGroups
        {
            get { return this.m_subGroups; }
        }

        public bool IsLeader(Character chr)
        {
            GroupMember leader = this.m_leader;
            if (leader == null)
                return false;
            RoleGroup role = chr.Role;
            if ((object) role != null && role.IsStaff)
                return true;
            return leader.Character == chr;
        }

        /// <summary>
        /// The GroupMember who is leader of this Group.
        /// Is null if the Group has no online members.
        /// This will only ever be null if no one in the Group is online.
        /// </summary>
        public GroupMember Leader
        {
            get { return this.m_leader; }
            set
            {
                if (value == this.m_leader)
                    return;
                GroupMember leader = this.m_leader;
                this.m_leader = value;
                if (value == null)
                    return;
                this.OnLeaderChanged(leader);
            }
        }

        /// <summary>
        /// The GroupMember who is the looter of the group. Returns null if there isnt one.
        /// </summary>
        public GroupMember MasterLooter
        {
            get { return this.m_masterLooter; }
            set
            {
                if (value == this.m_masterLooter)
                    return;
                this.m_masterLooter = value;
                this.SendUpdate();
            }
        }

        /// <summary>Returns true if this is a BattleGroup. //TODO:</summary>
        public virtual bool IsBattleGroup
        {
            get { return false; }
        }

        /// <summary>The MainAssistant of this Group</summary>
        public GroupMember MainAssistant
        {
            get { return this.m_mainAssistant; }
            set
            {
                if (value == this.m_mainAssistant)
                    return;
                if (value != null)
                    value.Flags |= GroupMemberFlags.MainAssistant;
                if (this.m_mainAssistant != null)
                    this.m_mainAssistant.Flags &= ~GroupMemberFlags.MainAssistant;
                this.m_mainAssistant = value;
                this.SendUpdate();
            }
        }

        /// <summary>The MainTank of this Group</summary>
        public GroupMember MainTank
        {
            get { return this.m_mainTank; }
            set
            {
                if (value == this.m_mainTank)
                    return;
                if (value != null)
                    value.Flags |= GroupMemberFlags.MainTank;
                if (this.m_mainTank != null)
                    this.m_mainTank.Flags &= ~GroupMemberFlags.MainTank;
                this.m_mainTank = value;
                this.SendUpdate();
            }
        }

        /// <summary>Whether this Group is full</summary>
        public bool IsFull
        {
            get { return this.m_Count >= this.MaxMemberCount; }
        }

        /// <summary>
        /// 
        /// </summary>
        public FactionGroup FactionGroup
        {
            get { return this.m_leader.Character.FactionGroup; }
        }

        /// <summary>
        /// Executes the given callback in each character's current context
        /// </summary>
        public void ForeachCharacter(Action<Character> callback)
        {
            using (this.syncLock.EnterReadLock())
            {
                for (int index = 0; index < this.SubGroups.Length; ++index)
                {
                    foreach (GroupMember groupMember in this.SubGroups[index])
                    {
                        Character chr = groupMember.Character;
                        if (chr != null)
                            chr.ExecuteInContext((Action) (() =>
                            {
                                if (chr.Group != this)
                                    return;
                                callback(chr);
                            }));
                    }
                }
            }
        }

        /// <summary>
        /// Executes the given callback immediately for every online group member in the given context.
        /// IMPORTANT: Must be called from within context.
        /// </summary>
        public void CallOnAllInSameContext(IContextHandler context, Action<Character> callback)
        {
            context.EnsureContext();
            foreach (Character allCharacter in this.GetAllCharacters())
            {
                if (allCharacter.ContextHandler == context)
                    callback(allCharacter);
            }
        }

        /// <summary>The amount of Members in this Group</summary>
        public int CharacterCount
        {
            get { return this.m_Count; }
        }

        /// <summary>The maximum amount of Members in this Group</summary>
        public int MaxMemberCount
        {
            get { return this.SubGroups.Length * 5; }
        }

        /// <summary>
        /// The Member who joined this Group earliest, of everyone who is currently in this Group
        /// </summary>
        public GroupMember FirstMember
        {
            get { return this.m_firstMember; }
        }

        /// <summary>
        /// The Member who joined this Group last, of everyone who is currently in this Group
        /// </summary>
        public GroupMember LastMember
        {
            get { return this.m_lastMember; }
        }

        /// <summary>
        /// Free spots left in this group (= MaxCount - CurrentCount)
        /// </summary>
        public byte InvitesLeft
        {
            get { return (byte) (this.MaxMemberCount - this.CharacterCount); }
        }

        public abstract GroupFlags Flags { get; }

        /// <summary>The member whose turn it is in RoundRobin</summary>
        public GroupMember RoundRobinMember
        {
            get { return this.m_roundRobinMember; }
        }

        /// <summary>
        /// The SyncRoot against which to synchronize this group (when iterating over it or making certain changes)
        /// </summary>
        internal ReaderWriterLockWrapper SyncRoot
        {
            get { return this.syncLock; }
        }

        /// <summary>Add member to Group</summary>
        /// <param name="update">Indicates if this group needs to be updated after adding the
        /// new member</param>
        /// <returns>True if the member was added successfully. False otherwise.</returns>
        public GroupMember AddMember(Character chr, bool update)
        {
            GroupMember member = (GroupMember) null;
            try
            {
                using (this.syncLock.EnterReadLock())
                {
                    foreach (SubGroup subGroup in this.m_subGroups)
                    {
                        if (!subGroup.IsFull)
                        {
                            member = new GroupMember(chr, GroupMemberFlags.Normal);
                            subGroup.AddMember(member);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex,
                    string.Format("Could not add member {0} to group {1}", (object) chr, (object) this), new object[0]);
            }

            this.OnAddMember(member);
            if (member != null && update)
                this.SendUpdate();
            return member;
        }

        /// <summary>Remove member from this Group</summary>
        public virtual void RemoveMember(GroupMember member)
        {
            if (this.CharacterCount <= 2)
            {
                this.Disband();
            }
            else
            {
                bool flag = false;
                using (this.syncLock.EnterReadLock())
                {
                    GroupMember next = member.Next;
                    this.OnMemberRemoved(member);
                    if (this.m_firstMember == member)
                    {
                        this.m_firstMember = next;
                    }
                    else
                    {
                        GroupMember groupMember = this.m_firstMember;
                        while (groupMember.Next != member)
                            groupMember = groupMember.Next;
                        groupMember.Next = next;
                        if (member == this.m_lastMember)
                            this.m_lastMember = groupMember;
                    }

                    if (this.m_leader == member)
                    {
                        this.m_leader = this.GetFirstOnlineMemberUnlocked();
                        flag = true;
                    }

                    if (this.m_masterLooter == member)
                        this.m_masterLooter = this.m_firstMember;
                    if (this.m_mainAssistant == member)
                    {
                        member.Flags &= ~GroupMemberFlags.MainAssistant;
                        this.m_mainAssistant = (GroupMember) null;
                    }

                    if (this.m_mainTank == member)
                    {
                        member.Flags &= ~GroupMemberFlags.MainTank;
                        this.m_mainTank = (GroupMember) null;
                    }

                    if (this.m_roundRobinMember == member)
                        this.m_roundRobinMember = next;
                }

                if (flag)
                    this.OnLeaderChanged(member);
            }

            this.SendUpdate();
        }

        public GroupMember GetFirstOnlineMember()
        {
            using (this.syncLock.EnterReadLock())
                return this.GetFirstOnlineMemberUnlocked();
        }

        internal GroupMember GetFirstOnlineMemberUnlocked()
        {
            GroupMember groupMember = this.m_firstMember;
            while (groupMember != null && !groupMember.IsOnline)
                groupMember = groupMember.Next;
            return groupMember;
        }

        /// <summary>Called when the given member is added</summary>
        protected virtual void OnAddMember(GroupMember member)
        {
            ++this.m_Count;
            Character character = member.Character;
            if (character != null)
                character.GroupMember = member;
            if (this.m_firstMember == null)
            {
                this.m_firstMember = this.m_lastMember = member;
            }
            else
            {
                this.m_lastMember.Next = member;
                this.m_lastMember = member;
            }

            Group.GroupMemberHandler memberAdded = Group.MemberAdded;
            if (memberAdded == null)
                return;
            memberAdded(member);
        }

        /// <summary>
        /// Called before the given member is removed to clean up everything related to the given member
        /// </summary>
        protected void OnMemberRemoved(GroupMember member)
        {
            Character chr = member.Character;
            if (chr != null && chr.IsInWorld)
            {
                if (!chr.IsInContext)
                {
                    chr.ExecuteInContext((Action) (() => this.OnMemberRemoved(member)));
                    return;
                }

                Group.GroupMemberHandler memberRemoved = Group.MemberRemoved;
                if (memberRemoved != null)
                    memberRemoved(member);
                --this.m_Count;
                this.SendEmptyUpdate(chr);
                chr.GroupMember = (GroupMember) null;
                GroupHandler.SendResult((IPacketReceiver) chr.Client, GroupResult.NoError);
                member.SubGroup.RemoveMember(member);
                member.Character = (Character) null;
                if (chr.Map is BaseInstance)
                {
                    BaseInstance instance = (BaseInstance) chr.Map;
                    chr.Map.CallDelayed(Group.GroupInstanceKickDelayMillis, (Action) (() =>
                    {
                        if (!chr.IsInWorld || chr.Map != instance || instance.CanEnter(chr))
                            return;
                        chr.TeleportToNearestGraveyard();
                    }));
                }
            }
            else
            {
                Group.GroupMemberHandler memberRemoved = Group.MemberRemoved;
                if (memberRemoved != null)
                    memberRemoved(member);
                --this.m_Count;
                Singleton<GroupMgr>.Instance.OfflineChars.Remove(member.Id);
                member.m_subGroup = (SubGroup) null;
            }

            member.m_nextMember = (GroupMember) null;
        }

        private void OnLeaderChanged(GroupMember oldLeader)
        {
            if (this.m_leader != null)
                GroupHandler.SendLeaderChanged(this.m_leader);
            Group.GroupLeaderChangedHandler leaderChanged = Group.LeaderChanged;
            if (leaderChanged == null)
                return;
            leaderChanged(oldLeader, this.m_leader);
        }

        /// <summary>Disbands this Group</summary>
        public virtual void Disband()
        {
            using (this.syncLock.EnterReadLock())
            {
                foreach (SubGroup subGroup in this.m_subGroups)
                {
                    foreach (GroupMember member in subGroup.Members)
                    {
                        if (member.Character != null)
                            Asda2GroupHandler.SendPartyHasBrokenResponse(member.Character.Client);
                        this.OnMemberRemoved(member);
                    }
                }
            }
        }

        public GroupMember this[uint lowMemberId]
        {
            get
            {
                using (this.syncLock.EnterReadLock())
                {
                    for (int index = 0; index < this.m_subGroups.Length; ++index)
                    {
                        GroupMember groupMember = this.m_subGroups[index][lowMemberId];
                        if (groupMember != null)
                            return groupMember;
                    }
                }

                return (GroupMember) null;
            }
        }

        public GroupMember this[string name]
        {
            get
            {
                using (this.syncLock.EnterReadLock())
                {
                    foreach (SubGroup subGroup in this.m_subGroups)
                    {
                        GroupMember groupMember = subGroup[name];
                        if (groupMember != null)
                            return groupMember;
                    }
                }

                return (GroupMember) null;
            }
        }

        /// <summary>
        /// Check whether the given inviter may invite the given target
        /// </summary>
        public static GroupResult CheckInvite(Character inviter, out Character target, string targetName)
        {
            GroupMember groupMember = inviter.GroupMember;
            Group group = groupMember?.Group;
            Character character = World.GetCharacter(targetName, true);
            GroupResult resultCode;
            if (group != null && group.IsFull)
            {
                resultCode = GroupResult.GroupIsFull;
                target = (Character) null;
                targetName = string.Empty;
            }
            else if (groupMember != null && !groupMember.IsAtLeastAssistant)
            {
                target = (Character) null;
                resultCode = GroupResult.DontHavePermission;
            }
            else if (group != null && group.Flags.HasFlag((Enum) GroupFlags.Raid) &&
                     (character != null && character.IsAllowedLowLevelRaid) &&
                     character.Level < Group.MinLevelToBeInvitedInRaid)
            {
                target = (Character) null;
                resultCode = GroupResult.RaidDisallowedByLevel;
            }
            else
            {
                target = World.GetCharacter(targetName, false);
                if (target == null || inviter == target || target.Role.IsStaff && !inviter.Role.IsStaff)
                    resultCode = GroupResult.OfflineOrDoesntExist;
                else if (inviter.Faction.Group != target.Faction.Group)
                    resultCode = GroupResult.TargetIsUnfriendly;
                else if (target.Group != null || target.IsInvitedToGroup)
                {
                    resultCode = GroupResult.AlreadyInGroup;
                }
                else
                {
                    if (!target.IsIgnoring((IUser) inviter) || inviter.Role.IsStaff)
                        return GroupResult.NoError;
                    resultCode = GroupResult.TargetIsIgnoringYou;
                }
            }

            Group.SendResult((IPacketReceiver) inviter.Client, resultCode, 0U, targetName);
            return resultCode;
        }

        /// <summary>
        /// Checks whether the given target exists in this group and whether the given requestMember has the given privs
        /// </summary>
        public GroupResult CheckAction(GroupMember requestMember, GroupMember target, string targetName,
            GroupPrivs reqPrivs)
        {
            GroupResult resultCode;
            if (target == null || target.Group != requestMember.Group)
            {
                resultCode = GroupResult.NotInYourParty;
            }
            else
            {
                if ((reqPrivs != GroupPrivs.Leader || this.m_leader == requestMember) &&
                    (reqPrivs != GroupPrivs.MainAsisstant || requestMember.IsAtLeastMainAssistant) &&
                    (reqPrivs != GroupPrivs.Assistant || requestMember.IsAtLeastAssistant))
                    return GroupResult.NoError;
                resultCode = GroupResult.DontHavePermission;
                targetName = string.Empty;
            }

            Character character = requestMember.Character;
            if (character != null)
                Group.SendResult((IPacketReceiver) character.Client, resultCode, 0U, targetName);
            return resultCode;
        }

        public bool CheckPrivs(GroupMember member, GroupPrivs reqPrivs)
        {
            if ((reqPrivs != GroupPrivs.Leader || this.m_leader == member) &&
                (reqPrivs != GroupPrivs.MainAsisstant || member.IsAtLeastMainAssistant) &&
                (reqPrivs != GroupPrivs.Assistant || member.IsAtLeastAssistant))
                return true;
            Character character = member.Character;
            if (character != null)
                GroupHandler.SendResult((IPacketReceiver) character.Client, GroupResult.DontHavePermission);
            return false;
        }

        public bool CheckFull(GroupMember member, SubGroup group)
        {
            if (!group.IsFull)
                return true;
            Character character = member.Character;
            if (character != null)
                GroupHandler.SendResult((IPacketReceiver) character.Client, GroupResult.GroupIsFull);
            return false;
        }

        /// <summary>
        /// Send the Updated list of the group state to each group member
        /// </summary>
        public virtual void SendUpdate()
        {
            if (this.m_leader == null)
                return;
            Asda2GroupHandler.SendPartyInfoResponse(this);
            foreach (GroupMember groupMember in this)
            {
                Asda2GroupHandler.SendPartyMemberInitialInfoResponse(groupMember.Character);
                Asda2GroupHandler.SendPartyMemberPositionInfoResponse(groupMember.Character);
            }
        }

        /// <summary>Send a packet to each group member</summary>
        /// <param name="packet">Realm Packet</param>
        public virtual void SendAll(RealmPacketOut packet)
        {
            this.SendAll(packet, (GroupMember) null);
        }

        /// <summary>
        /// Send a packet to each group member except one specified
        /// </summary>
        /// <param name="packet">Realm Packet</param>
        /// <param name="ignored">Member that won't receive the message</param>
        protected virtual void SendAll(RealmPacketOut packet, GroupMember ignored)
        {
            foreach (SubGroup subGroup in this.m_subGroups)
                subGroup.Send(packet, ignored);
        }

        /// <summary>Send Empty Group List</summary>
        protected virtual void SendEmptyUpdate(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_LIST))
            {
                packet.Fill((byte) 0, 24);
                chr.Client.Send(packet, false);
            }
        }

        /// <summary>Send Group Uninvite packet</summary>
        public static void SendGroupUninvite(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_GROUP_UNINVITE, 0))
                chr.Client.Send(packet, false);
        }

        /// <summary>Send Party Disband Packet</summary>
        protected virtual void SendGroupDestroyed(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_DESTROYED))
                chr.Client.Send(packet, false);
        }

        /// <summary>Sends result of actions connected with groups</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="resultType">The result type</param>
        /// <param name="resultCode">The <see cref="T:WCell.Constants.GroupResult" /> result code</param>
        /// <param name="name">name of player event has happened to</param>
        public static void SendResult(IPacketReceiver client, GroupResult resultCode, uint resultType, string name)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_PARTY_COMMAND_RESULT))
            {
                packet.Write(resultType);
                packet.WriteCString(name);
                packet.Write((uint) resultCode);
                packet.Write(0U);
                client.Send(packet, false);
            }
        }

        /// <summary>Sends result of actions connected with groups</summary>
        /// <param name="client">the client to send to</param>
        /// <param name="resultCode">The <see cref="T:WCell.Constants.GroupResult" /> result code</param>
        /// <param name="name">name of player event has happened to</param>
        public static void SendResult(IPacketReceiver client, GroupResult resultCode, string name)
        {
            Group.SendResult(client, resultCode, 0U, name);
        }

        /// <summary>Sends ping to the group, except pinger</summary>
        /// <param name="pinger">The group member who pingged the minimap</param>
        /// <param name="x">x coordinate of ping</param>
        /// <param name="y">y coordinate of ping</param>
        public virtual void SendPing(GroupMember pinger, float x, float y)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_MINIMAP_PING))
            {
                packet.Write((ulong) EntityId.GetPlayerId(pinger.Id));
                packet.WriteFloat(x);
                packet.WriteFloat(y);
                this.SendAll(packet, pinger);
            }
        }

        /// <summary>Sends roll results to the group</summary>
        /// <param name="min">minimal value</param>
        /// <param name="max">maximal value</param>
        /// <param name="roll">value rolled out</param>
        /// <param name="guid">guid of roller</param>
        public virtual void SendRoll(int min, int max, int roll, EntityId guid)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_RANDOM_ROLL))
            {
                packet.Write(min);
                packet.Write(max);
                packet.Write(roll);
                packet.Write(guid.Full);
                this.SendAll(packet);
            }
        }

        /// <summary>Sends all info about set icons to the client</summary>
        /// <param name="requester">The character requesting the target icon list</param>
        public virtual void SendTargetIconList(Character requester)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_RAID_TARGET_UPDATE))
            {
                packet.WriteByte(1);
                for (byte val = 0; val < (byte) 8; ++val)
                {
                    if (!(this.m_targetIcons[(int) val] == EntityId.Zero))
                    {
                        packet.WriteByte(val);
                        packet.Write(this.m_targetIcons[(int) val].Full);
                    }
                }

                if (requester != null)
                    requester.Client.Send(packet, false);
                else
                    this.SendAll(packet);
            }
        }

        /// <summary>Sends all info about set icons to the client</summary>
        public virtual void SendTargetIconList()
        {
            this.SendTargetIconList((Character) null);
        }

        /// <summary>
        /// Sends info about change of single target icons info to all the party
        /// </summary>
        /// <param name="iconId">what element of array has changed</param>
        /// <param name="targetId">new value</param>
        public virtual void SetTargetIcon(byte iconId, EntityId whoId, EntityId targetId)
        {
            if (iconId >= (byte) 8)
                return;
            bool flag = this.ClearTargetIconList(iconId, targetId);
            this.m_targetIcons[(int) iconId] = targetId;
            if (flag)
            {
                this.SendTargetIconList();
            }
            else
            {
                using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_RAID_TARGET_UPDATE))
                {
                    packet.WriteByte(0);
                    packet.Write((ulong) whoId);
                    packet.WriteByte(iconId);
                    packet.Write(targetId.Full);
                    this.SendAll(packet);
                }
            }
        }

        private bool ClearTargetIconList(byte iconId, EntityId targetId)
        {
            bool flag = false;
            using (this.syncLock.EnterReadLock())
            {
                if (targetId != EntityId.Zero)
                {
                    for (int index = 0; index < this.m_targetIcons.Length; ++index)
                    {
                        if (this.m_targetIcons[index] == targetId)
                        {
                            if ((int) iconId != index)
                                flag = true;
                            this.m_targetIcons[index] = EntityId.Zero;
                        }
                    }
                }
            }

            return flag;
        }

        /// <summary>The name of the this ChatTarget</summary>
        public string Name
        {
            get { return string.Empty; }
        }

        /// <summary>The EntityId (only set for Owner)</summary>
        public EntityId EntityId
        {
            get { return EntityId.Zero; }
        }

        public void SendSystemMsg(string message)
        {
            using (this.syncLock.EnterReadLock())
            {
                foreach (SubGroup subGroup in this.SubGroups)
                {
                    foreach (GroupMember groupMember in subGroup)
                    {
                        if (groupMember.Character != null)
                            groupMember.Character.SendSystemMessage(message);
                    }
                }
            }
        }

        public void SendMessage(IChatter sender, ChatLanguage language, string message)
        {
            throw new NotImplementedException();
        }

        public GroupMember GetMember(uint lowId)
        {
            using (this.syncLock.EnterReadLock())
            {
                foreach (SubGroup subGroup in this.SubGroups)
                {
                    foreach (GroupMember groupMember in subGroup)
                    {
                        if ((int) groupMember.Id == (int) lowId)
                            return groupMember;
                    }
                }
            }

            return (GroupMember) null;
        }

        /// <summary>
        /// All online characters.
        /// Don't forget to lock the SyncRoot while iterating over a Group.
        /// </summary>
        public Character[] GetAllCharacters()
        {
            Character[] array = new Character[this.m_Count];
            int charactersUnlocked;
            using (this.syncLock.EnterReadLock())
                charactersUnlocked = this.GetAllCharactersUnlocked(array);
            if (array.Length > charactersUnlocked)
                Array.Resize<Character>(ref array, charactersUnlocked);
            return array;
        }

        private int GetAllCharactersUnlocked(Character[] chrs)
        {
            int num = 0;
            for (int index = 0; index < this.SubGroups.Length; ++index)
            {
                foreach (GroupMember groupMember in this.SubGroups[index])
                {
                    if (groupMember.Character != null)
                        chrs[num++] = groupMember.Character;
                }
            }

            return num;
        }

        /// <summary>
        /// Returns all online Characters in a <see cref="T:WCell.RealmServer.Entities.SynchronizedCharacterList" />.
        /// </summary>
        /// <returns></returns>
        public SynchronizedCharacterList GetCharacterSet()
        {
            SynchronizedCharacterList synchronizedCharacterList =
                new SynchronizedCharacterList(this.m_Count, this.FactionGroup);
            using (this.syncLock.EnterReadLock())
            {
                for (int index = 0; index < this.SubGroups.Length; ++index)
                {
                    foreach (GroupMember groupMember in this.SubGroups[index])
                    {
                        if (groupMember.Character != null)
                            synchronizedCharacterList.Add(groupMember.Character);
                    }
                }
            }

            return synchronizedCharacterList;
        }

        public bool IsRussianClient { get; set; }

        public Locale Locale { get; set; }

        /// <summary>Sends the given packet to all online characters</summary>
        public void Send(RealmPacketOut packet, bool addEnd = false)
        {
            foreach (Character allCharacter in this.GetAllCharacters())
                allCharacter.Client.Send(packet, addEnd);
        }

        /// <summary>
        /// Selects and returns the next online Member whose turn it is in RoundRobin.
        /// </summary>
        /// <returns>null if all members of this Group are offline.</returns>
        public GroupMember GetNextRoundRobinMember()
        {
            using (this.syncLock.EnterWriteLock())
            {
                this.m_roundRobinMember =
                    this.m_roundRobinMember != null ? this.m_roundRobinMember.Next : this.m_firstMember;
                while (this.m_roundRobinMember.Character == null)
                {
                    this.m_roundRobinMember = this.m_roundRobinMember.Next;
                    if (this.m_roundRobinMember == this.m_firstMember)
                        return (GroupMember) null;
                }
            }

            return this.m_roundRobinMember;
        }

        public void GetNearbyLooters(IAsda2Lootable lootable, WorldObject initialLooter,
            ICollection<Asda2LooterEntry> looters)
        {
            foreach (Character objectsInRadiu in (IEnumerable<WorldObject>) (!(lootable is WorldObject)
                    ? initialLooter
                    : (WorldObject) lootable)
                .GetObjectsInRadius<WorldObject>(Asda2LootMgr.LootRadius, ObjectTypes.Player, false, 0))
            {
                GroupMember groupMember;
                if (objectsInRadiu.IsAlive && (objectsInRadiu == initialLooter ||
                                               (groupMember = objectsInRadiu.GroupMember) != null &&
                                               groupMember.Group == this))
                    looters.Add(objectsInRadiu.LooterEntry);
            }
        }

        /// <summary>
        /// Sets the given Looting-parameters and updates the Group.
        /// </summary>
        public void SetLootMethod(LootMethod method, GroupMember masterLooter, ItemQuality lootThreshold)
        {
            using (this.syncLock.EnterWriteLock())
            {
                this.LootMethod = method;
                this.m_masterLooter = masterLooter;
                this.LootThreshold = lootThreshold;
            }

            this.SendUpdate();
        }

        /// <summary>
        /// Update the stats of the given <see cref="T:WCell.RealmServer.Groups.GroupMember" /> to all
        /// out of range members of this group.
        /// </summary>
        /// <remarks>Method requires Group-synchronization.</remarks>
        /// <param name="member">The <see cref="T:WCell.RealmServer.Groups.GroupMember" /> who needs to send
        /// the update</param>
        internal void UpdateOutOfRangeMembers(GroupMember member)
        {
            if (member.Character == null || member.Group != this)
                return;
            foreach (Character allCharacter in this.GetAllCharacters())
            {
                if (allCharacter != member.Character && allCharacter != null &&
                    (!allCharacter.IsInUpdateRange((WorldObject) member.Character) &&
                     member.Character.GroupUpdateFlags != GroupUpdateFlags.None))
                    GroupHandler.SendPartyMemberStats((IPacketReceiver) allCharacter.Client, member,
                        member.Character.GroupUpdateFlags);
            }

            member.Character.GroupUpdateFlags = GroupUpdateFlags.None;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }

        /// <summary>
        /// All members of this Group and all its SubGroups.
        /// Don't forget to lock the SyncRoot while iterating over a Group.
        /// </summary>
        public IEnumerator<GroupMember> GetEnumerator()
        {
            foreach (SubGroup subGroup in this.SubGroups)
            {
                IEnumerator<GroupMember> enumerator = subGroup.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    GroupMember member = enumerator.Current;
                    yield return member;
                }
            }
        }

        public Character InstanceLeader
        {
            get
            {
                if (this.m_leader == null)
                    return (Character) null;
                return this.m_leader.Character;
            }
        }

        public InstanceCollection InstanceLeaderCollection
        {
            get
            {
                if (this.m_leader == null)
                    return (InstanceCollection) null;
                return this.m_leader.Character.Instances;
            }
        }

        public void ForeachInstanceHolder(Action<InstanceCollection> callback)
        {
            foreach (Character allCharacter in this.GetAllCharacters())
            {
                InstanceCollection instances = allCharacter.Instances;
                if (instances != null)
                    callback(instances);
            }
        }

        /// <summary>
        /// Gets the Instance of the given Map of either the Leader or any member
        /// if anyone is already in it.
        /// </summary>
        public BaseInstance GetActiveInstance(MapTemplate map)
        {
            GroupMember leader = this.m_leader;
            if (leader != null)
            {
                Character character = leader.Character;
                if (character != null)
                {
                    InstanceCollection instances = character.Instances;
                    if (instances != null)
                    {
                        BaseInstance activeInstance = instances.GetActiveInstance(map);
                        if (activeInstance != null)
                            return activeInstance;
                    }
                }
            }

            foreach (Character allCharacter in this.GetAllCharacters())
            {
                BaseInstance activeInstance = allCharacter.GetActiveInstance(map);
                if (activeInstance != null)
                    return activeInstance;
            }

            return (BaseInstance) null;
        }

        public void DistributeGroupHonor(Character earner, Character victim, uint honorPoints)
        {
            if (this.CharacterCount < 1)
                return;
            uint bonus = honorPoints / (uint) this.CharacterCount;
            this.ForeachCharacter((Action<Character>) (chr =>
            {
                if (!chr.IsInRange(new SimpleRange(0.0f, Group.MaxKillRewardDistance), (WorldObject) earner))
                    return;
                chr.GiveHonorPoints(bonus);
                ++chr.KillsToday;
                ++chr.LifetimeHonorableKills;
                HonorHandler.SendPVPCredit((IPacketReceiver) chr, bonus * 10U, victim);
            }));
        }

        public void OnKill(Character killer, NPC victim)
        {
            if (this.CharacterCount < 1)
                return;
            this.ForeachCharacter((Action<Character>) (chr =>
            {
                if (chr.Map != victim.Map || !chr.IsInRange(new SimpleRange(0.0f, Group.MaxKillRewardDistance),
                        (WorldObject) killer))
                    return;
                chr.QuestLog.OnNPCInteraction(victim);
                chr.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KillCreature, victim.EntryId,
                    1U, (Unit) null);
            }));
        }

        /// <summary>Kick every non-staff member</summary>
        public void EnsurePureStaffGroup()
        {
            using (this.syncLock.EnterReadLock())
            {
                foreach (GroupMember groupMember in this.ToArray<GroupMember>())
                {
                    Character chr = groupMember.Character;
                    if (chr == null || !chr.Role.IsStaff)
                    {
                        groupMember.LeaveGroup();
                        if (chr != null)
                            chr.AddMessage((Action) (() =>
                                chr.SendSystemMessage(
                                    "You have been kicked from the group since you are not a staff member.")));
                    }
                }
            }
        }

        public static event Group.GroupMemberHandler MemberAdded;

        public static event Group.GroupMemberHandler MemberRemoved;

        public static event Group.GroupLeaderChangedHandler LeaderChanged;

        public delegate void GroupMemberHandler(GroupMember member);

        public delegate void GroupLeaderChangedHandler(GroupMember oldLeader, GroupMember newLeader);
    }
}