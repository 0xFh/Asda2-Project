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
      syncLock = new ReaderWriterLockWrapper();
      m_subGroups = new SubGroup[maxGroupUnits];
      for(byte groupUnitId = 0; (int) groupUnitId < (int) maxGroupUnits; ++groupUnitId)
        m_subGroups[groupUnitId] = new SubGroup(this, groupUnitId);
      m_targetIcons = new EntityId[8];
      for(int index = 0; index < m_targetIcons.Length; ++index)
        m_targetIcons[index] = EntityId.Zero;
      m_lootMethod = LootMethod.GroupLoot;
      m_lootThreshold = ItemQuality.Uncommon;
      m_DungeonDifficulty = 0U;
      Leader = AddMember(leader, false);
      m_masterLooter = null;
    }

    /// <summary>
    /// The chosen LootMethod.
    /// Make sure to send an Update after changing this, so the Group is informed about the change.
    /// </summary>
    public LootMethod LootMethod
    {
      get { return m_lootMethod; }
      set
      {
        if(value >= LootMethod.End)
          m_lootMethod = LootMethod.GroupLoot;
        else
          m_lootMethod = value;
      }
    }

    /// <summary>
    /// The least Quality of Items to be handled by the MasterLooter or to be rolled for.
    /// Make sure to send an Update after changing this, so the Group is informed about the change.
    /// </summary>
    public ItemQuality LootThreshold
    {
      get { return m_lootThreshold; }
      set { m_lootThreshold = value; }
    }

    /// <summary>
    /// The DungeonDifficulty.
    /// Make sure to send an Update after changing this, so the Group is informed about the change.
    /// </summary>
    public uint DungeonDifficulty
    {
      get { return m_DungeonDifficulty; }
      set
      {
        if((int) value == (int) m_DungeonDifficulty)
          return;
        m_DungeonDifficulty = value;
        if(IsBattleGroup)
          return;
        foreach(Character allCharacter in GetAllCharacters())
        {
          if(Flags.HasFlag(GroupFlags.Raid))
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
      get { return m_subGroups; }
    }

    public bool IsLeader(Character chr)
    {
      GroupMember leader = m_leader;
      if(leader == null)
        return false;
      RoleGroup role = chr.Role;
      if(role != null && role.IsStaff)
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
      get { return m_leader; }
      set
      {
        if(value == m_leader)
          return;
        GroupMember leader = m_leader;
        m_leader = value;
        if(value == null)
          return;
        OnLeaderChanged(leader);
      }
    }

    /// <summary>
    /// The GroupMember who is the looter of the group. Returns null if there isnt one.
    /// </summary>
    public GroupMember MasterLooter
    {
      get { return m_masterLooter; }
      set
      {
        if(value == m_masterLooter)
          return;
        m_masterLooter = value;
        SendUpdate();
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
      get { return m_mainAssistant; }
      set
      {
        if(value == m_mainAssistant)
          return;
        if(value != null)
          value.Flags |= GroupMemberFlags.MainAssistant;
        if(m_mainAssistant != null)
          m_mainAssistant.Flags &= ~GroupMemberFlags.MainAssistant;
        m_mainAssistant = value;
        SendUpdate();
      }
    }

    /// <summary>The MainTank of this Group</summary>
    public GroupMember MainTank
    {
      get { return m_mainTank; }
      set
      {
        if(value == m_mainTank)
          return;
        if(value != null)
          value.Flags |= GroupMemberFlags.MainTank;
        if(m_mainTank != null)
          m_mainTank.Flags &= ~GroupMemberFlags.MainTank;
        m_mainTank = value;
        SendUpdate();
      }
    }

    /// <summary>Whether this Group is full</summary>
    public bool IsFull
    {
      get { return m_Count >= MaxMemberCount; }
    }

    /// <summary>
    /// 
    /// </summary>
    public FactionGroup FactionGroup
    {
      get { return m_leader.Character.FactionGroup; }
    }

    /// <summary>
    /// Executes the given callback in each character's current context
    /// </summary>
    public void ForeachCharacter(Action<Character> callback)
    {
      using(syncLock.EnterReadLock())
      {
        for(int index = 0; index < SubGroups.Length; ++index)
        {
          foreach(GroupMember groupMember in SubGroups[index])
          {
            Character chr = groupMember.Character;
            if(chr != null)
              chr.ExecuteInContext(() =>
              {
                if(chr.Group != this)
                  return;
                callback(chr);
              });
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
      foreach(Character allCharacter in GetAllCharacters())
      {
        if(allCharacter.ContextHandler == context)
          callback(allCharacter);
      }
    }

    /// <summary>The amount of Members in this Group</summary>
    public int CharacterCount
    {
      get { return m_Count; }
    }

    /// <summary>The maximum amount of Members in this Group</summary>
    public int MaxMemberCount
    {
      get { return SubGroups.Length * 5; }
    }

    /// <summary>
    /// The Member who joined this Group earliest, of everyone who is currently in this Group
    /// </summary>
    public GroupMember FirstMember
    {
      get { return m_firstMember; }
    }

    /// <summary>
    /// The Member who joined this Group last, of everyone who is currently in this Group
    /// </summary>
    public GroupMember LastMember
    {
      get { return m_lastMember; }
    }

    /// <summary>
    /// Free spots left in this group (= MaxCount - CurrentCount)
    /// </summary>
    public byte InvitesLeft
    {
      get { return (byte) (MaxMemberCount - CharacterCount); }
    }

    public abstract GroupFlags Flags { get; }

    /// <summary>The member whose turn it is in RoundRobin</summary>
    public GroupMember RoundRobinMember
    {
      get { return m_roundRobinMember; }
    }

    /// <summary>
    /// The SyncRoot against which to synchronize this group (when iterating over it or making certain changes)
    /// </summary>
    internal ReaderWriterLockWrapper SyncRoot
    {
      get { return syncLock; }
    }

    /// <summary>Add member to Group</summary>
    /// <param name="update">Indicates if this group needs to be updated after adding the
    /// new member</param>
    /// <returns>True if the member was added successfully. False otherwise.</returns>
    public GroupMember AddMember(Character chr, bool update)
    {
      GroupMember member = null;
      try
      {
        using(syncLock.EnterReadLock())
        {
          foreach(SubGroup subGroup in m_subGroups)
          {
            if(!subGroup.IsFull)
            {
              member = new GroupMember(chr, GroupMemberFlags.Normal);
              subGroup.AddMember(member);
              break;
            }
          }
        }
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException(ex,
          string.Format("Could not add member {0} to group {1}", chr, this));
      }

      OnAddMember(member);
      if(member != null && update)
        SendUpdate();
      return member;
    }

    /// <summary>Remove member from this Group</summary>
    public virtual void RemoveMember(GroupMember member)
    {
      if(CharacterCount <= 2)
      {
        Disband();
      }
      else
      {
        bool flag = false;
        using(syncLock.EnterReadLock())
        {
          GroupMember next = member.Next;
          OnMemberRemoved(member);
          if(m_firstMember == member)
          {
            m_firstMember = next;
          }
          else
          {
            GroupMember groupMember = m_firstMember;
            while(groupMember.Next != member)
              groupMember = groupMember.Next;
            groupMember.Next = next;
            if(member == m_lastMember)
              m_lastMember = groupMember;
          }

          if(m_leader == member)
          {
            m_leader = GetFirstOnlineMemberUnlocked();
            flag = true;
          }

          if(m_masterLooter == member)
            m_masterLooter = m_firstMember;
          if(m_mainAssistant == member)
          {
            member.Flags &= ~GroupMemberFlags.MainAssistant;
            m_mainAssistant = null;
          }

          if(m_mainTank == member)
          {
            member.Flags &= ~GroupMemberFlags.MainTank;
            m_mainTank = null;
          }

          if(m_roundRobinMember == member)
            m_roundRobinMember = next;
        }

        if(flag)
          OnLeaderChanged(member);
      }

      SendUpdate();
    }

    public GroupMember GetFirstOnlineMember()
    {
      using(syncLock.EnterReadLock())
        return GetFirstOnlineMemberUnlocked();
    }

    internal GroupMember GetFirstOnlineMemberUnlocked()
    {
      GroupMember groupMember = m_firstMember;
      while(groupMember != null && !groupMember.IsOnline)
        groupMember = groupMember.Next;
      return groupMember;
    }

    /// <summary>Called when the given member is added</summary>
    protected virtual void OnAddMember(GroupMember member)
    {
      ++m_Count;
      Character character = member.Character;
      if(character != null)
        character.GroupMember = member;
      if(m_firstMember == null)
      {
        m_firstMember = m_lastMember = member;
      }
      else
      {
        m_lastMember.Next = member;
        m_lastMember = member;
      }

      GroupMemberHandler memberAdded = MemberAdded;
      if(memberAdded == null)
        return;
      memberAdded(member);
    }

    /// <summary>
    /// Called before the given member is removed to clean up everything related to the given member
    /// </summary>
    protected void OnMemberRemoved(GroupMember member)
    {
      Character chr = member.Character;
      if(chr != null && chr.IsInWorld)
      {
        if(!chr.IsInContext)
        {
          chr.ExecuteInContext(() => OnMemberRemoved(member));
          return;
        }

        GroupMemberHandler memberRemoved = MemberRemoved;
        if(memberRemoved != null)
          memberRemoved(member);
        --m_Count;
        SendEmptyUpdate(chr);
        chr.GroupMember = null;
        GroupHandler.SendResult(chr.Client, GroupResult.NoError);
        member.SubGroup.RemoveMember(member);
        member.Character = null;
        if(chr.Map is BaseInstance)
        {
          BaseInstance instance = (BaseInstance) chr.Map;
          chr.Map.CallDelayed(GroupInstanceKickDelayMillis, () =>
          {
            if(!chr.IsInWorld || chr.Map != instance || instance.CanEnter(chr))
              return;
            chr.TeleportToNearestGraveyard();
          });
        }
      }
      else
      {
        GroupMemberHandler memberRemoved = MemberRemoved;
        if(memberRemoved != null)
          memberRemoved(member);
        --m_Count;
        Singleton<GroupMgr>.Instance.OfflineChars.Remove(member.Id);
        member.m_subGroup = null;
      }

      member.m_nextMember = null;
    }

    private void OnLeaderChanged(GroupMember oldLeader)
    {
      if(m_leader != null)
        GroupHandler.SendLeaderChanged(m_leader);
      GroupLeaderChangedHandler leaderChanged = LeaderChanged;
      if(leaderChanged == null)
        return;
      leaderChanged(oldLeader, m_leader);
    }

    /// <summary>Disbands this Group</summary>
    public virtual void Disband()
    {
      using(syncLock.EnterReadLock())
      {
        foreach(SubGroup subGroup in m_subGroups)
        {
          foreach(GroupMember member in subGroup.Members)
          {
            if(member.Character != null)
              Asda2GroupHandler.SendPartyHasBrokenResponse(member.Character.Client);
            OnMemberRemoved(member);
          }
        }
      }
    }

    public GroupMember this[uint lowMemberId]
    {
      get
      {
        using(syncLock.EnterReadLock())
        {
          for(int index = 0; index < m_subGroups.Length; ++index)
          {
            GroupMember groupMember = m_subGroups[index][lowMemberId];
            if(groupMember != null)
              return groupMember;
          }
        }

        return null;
      }
    }

    public GroupMember this[string name]
    {
      get
      {
        using(syncLock.EnterReadLock())
        {
          foreach(SubGroup subGroup in m_subGroups)
          {
            GroupMember groupMember = subGroup[name];
            if(groupMember != null)
              return groupMember;
          }
        }

        return null;
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
      if(group != null && group.IsFull)
      {
        resultCode = GroupResult.GroupIsFull;
        target = null;
        targetName = string.Empty;
      }
      else if(groupMember != null && !groupMember.IsAtLeastAssistant)
      {
        target = null;
        resultCode = GroupResult.DontHavePermission;
      }
      else if(group != null && group.Flags.HasFlag(GroupFlags.Raid) &&
              (character != null && character.IsAllowedLowLevelRaid) &&
              character.Level < MinLevelToBeInvitedInRaid)
      {
        target = null;
        resultCode = GroupResult.RaidDisallowedByLevel;
      }
      else
      {
        target = World.GetCharacter(targetName, false);
        if(target == null || inviter == target || target.Role.IsStaff && !inviter.Role.IsStaff)
          resultCode = GroupResult.OfflineOrDoesntExist;
        else if(inviter.Faction.Group != target.Faction.Group)
          resultCode = GroupResult.TargetIsUnfriendly;
        else if(target.Group != null || target.IsInvitedToGroup)
        {
          resultCode = GroupResult.AlreadyInGroup;
        }
        else
        {
          if(!target.IsIgnoring(inviter) || inviter.Role.IsStaff)
            return GroupResult.NoError;
          resultCode = GroupResult.TargetIsIgnoringYou;
        }
      }

      SendResult(inviter.Client, resultCode, 0U, targetName);
      return resultCode;
    }

    /// <summary>
    /// Checks whether the given target exists in this group and whether the given requestMember has the given privs
    /// </summary>
    public GroupResult CheckAction(GroupMember requestMember, GroupMember target, string targetName,
      GroupPrivs reqPrivs)
    {
      GroupResult resultCode;
      if(target == null || target.Group != requestMember.Group)
      {
        resultCode = GroupResult.NotInYourParty;
      }
      else
      {
        if((reqPrivs != GroupPrivs.Leader || m_leader == requestMember) &&
           (reqPrivs != GroupPrivs.MainAsisstant || requestMember.IsAtLeastMainAssistant) &&
           (reqPrivs != GroupPrivs.Assistant || requestMember.IsAtLeastAssistant))
          return GroupResult.NoError;
        resultCode = GroupResult.DontHavePermission;
        targetName = string.Empty;
      }

      Character character = requestMember.Character;
      if(character != null)
        SendResult(character.Client, resultCode, 0U, targetName);
      return resultCode;
    }

    public bool CheckPrivs(GroupMember member, GroupPrivs reqPrivs)
    {
      if((reqPrivs != GroupPrivs.Leader || m_leader == member) &&
         (reqPrivs != GroupPrivs.MainAsisstant || member.IsAtLeastMainAssistant) &&
         (reqPrivs != GroupPrivs.Assistant || member.IsAtLeastAssistant))
        return true;
      Character character = member.Character;
      if(character != null)
        GroupHandler.SendResult(character.Client, GroupResult.DontHavePermission);
      return false;
    }

    public bool CheckFull(GroupMember member, SubGroup group)
    {
      if(!group.IsFull)
        return true;
      Character character = member.Character;
      if(character != null)
        GroupHandler.SendResult(character.Client, GroupResult.GroupIsFull);
      return false;
    }

    /// <summary>
    /// Send the Updated list of the group state to each group member
    /// </summary>
    public virtual void SendUpdate()
    {
      if(m_leader == null)
        return;
      Asda2GroupHandler.SendPartyInfoResponse(this);
      foreach(GroupMember groupMember in this)
      {
        Asda2GroupHandler.SendPartyMemberInitialInfoResponse(groupMember.Character);
        Asda2GroupHandler.SendPartyMemberPositionInfoResponse(groupMember.Character);
      }
    }

    /// <summary>Send a packet to each group member</summary>
    /// <param name="packet">Realm Packet</param>
    public virtual void SendAll(RealmPacketOut packet)
    {
      SendAll(packet, null);
    }

    /// <summary>
    /// Send a packet to each group member except one specified
    /// </summary>
    /// <param name="packet">Realm Packet</param>
    /// <param name="ignored">Member that won't receive the message</param>
    protected virtual void SendAll(RealmPacketOut packet, GroupMember ignored)
    {
      foreach(SubGroup subGroup in m_subGroups)
        subGroup.Send(packet, ignored);
    }

    /// <summary>Send Empty Group List</summary>
    protected virtual void SendEmptyUpdate(Character chr)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_LIST))
      {
        packet.Fill(0, 24);
        chr.Client.Send(packet, false);
      }
    }

    /// <summary>Send Group Uninvite packet</summary>
    public static void SendGroupUninvite(Character chr)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_UNINVITE, 0))
        chr.Client.Send(packet, false);
    }

    /// <summary>Send Party Disband Packet</summary>
    protected virtual void SendGroupDestroyed(Character chr)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_DESTROYED))
        chr.Client.Send(packet, false);
    }

    /// <summary>Sends result of actions connected with groups</summary>
    /// <param name="client">the client to send to</param>
    /// <param name="resultType">The result type</param>
    /// <param name="resultCode">The <see cref="T:WCell.Constants.GroupResult" /> result code</param>
    /// <param name="name">name of player event has happened to</param>
    public static void SendResult(IPacketReceiver client, GroupResult resultCode, uint resultType, string name)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_PARTY_COMMAND_RESULT))
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
      SendResult(client, resultCode, 0U, name);
    }

    /// <summary>Sends ping to the group, except pinger</summary>
    /// <param name="pinger">The group member who pingged the minimap</param>
    /// <param name="x">x coordinate of ping</param>
    /// <param name="y">y coordinate of ping</param>
    public virtual void SendPing(GroupMember pinger, float x, float y)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_MINIMAP_PING))
      {
        packet.Write(EntityId.GetPlayerId(pinger.Id));
        packet.WriteFloat(x);
        packet.WriteFloat(y);
        SendAll(packet, pinger);
      }
    }

    /// <summary>Sends roll results to the group</summary>
    /// <param name="min">minimal value</param>
    /// <param name="max">maximal value</param>
    /// <param name="roll">value rolled out</param>
    /// <param name="guid">guid of roller</param>
    public virtual void SendRoll(int min, int max, int roll, EntityId guid)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_RANDOM_ROLL))
      {
        packet.Write(min);
        packet.Write(max);
        packet.Write(roll);
        packet.Write(guid.Full);
        SendAll(packet);
      }
    }

    /// <summary>Sends all info about set icons to the client</summary>
    /// <param name="requester">The character requesting the target icon list</param>
    public virtual void SendTargetIconList(Character requester)
    {
      using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_RAID_TARGET_UPDATE))
      {
        packet.WriteByte(1);
        for(byte val = 0; val < (byte) 8; ++val)
        {
          if(!(m_targetIcons[val] == EntityId.Zero))
          {
            packet.WriteByte(val);
            packet.Write(m_targetIcons[val].Full);
          }
        }

        if(requester != null)
          requester.Client.Send(packet, false);
        else
          SendAll(packet);
      }
    }

    /// <summary>Sends all info about set icons to the client</summary>
    public virtual void SendTargetIconList()
    {
      SendTargetIconList(null);
    }

    /// <summary>
    /// Sends info about change of single target icons info to all the party
    /// </summary>
    /// <param name="iconId">what element of array has changed</param>
    /// <param name="targetId">new value</param>
    public virtual void SetTargetIcon(byte iconId, EntityId whoId, EntityId targetId)
    {
      if(iconId >= 8)
        return;
      bool flag = ClearTargetIconList(iconId, targetId);
      m_targetIcons[iconId] = targetId;
      if(flag)
      {
        SendTargetIconList();
      }
      else
      {
        using(RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MSG_RAID_TARGET_UPDATE))
        {
          packet.WriteByte(0);
          packet.Write(whoId);
          packet.WriteByte(iconId);
          packet.Write(targetId.Full);
          SendAll(packet);
        }
      }
    }

    private bool ClearTargetIconList(byte iconId, EntityId targetId)
    {
      bool flag = false;
      using(syncLock.EnterReadLock())
      {
        if(targetId != EntityId.Zero)
        {
          for(int index = 0; index < m_targetIcons.Length; ++index)
          {
            if(m_targetIcons[index] == targetId)
            {
              if(iconId != index)
                flag = true;
              m_targetIcons[index] = EntityId.Zero;
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
      using(syncLock.EnterReadLock())
      {
        foreach(SubGroup subGroup in SubGroups)
        {
          foreach(GroupMember groupMember in subGroup)
          {
            if(groupMember.Character != null)
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
      using(syncLock.EnterReadLock())
      {
        foreach(SubGroup subGroup in SubGroups)
        {
          foreach(GroupMember groupMember in subGroup)
          {
            if((int) groupMember.Id == (int) lowId)
              return groupMember;
          }
        }
      }

      return null;
    }

    /// <summary>
    /// All online characters.
    /// Don't forget to lock the SyncRoot while iterating over a Group.
    /// </summary>
    public Character[] GetAllCharacters()
    {
      Character[] array = new Character[m_Count];
      int charactersUnlocked;
      using(syncLock.EnterReadLock())
        charactersUnlocked = GetAllCharactersUnlocked(array);
      if(array.Length > charactersUnlocked)
        Array.Resize(ref array, charactersUnlocked);
      return array;
    }

    private int GetAllCharactersUnlocked(Character[] chrs)
    {
      int num = 0;
      for(int index = 0; index < SubGroups.Length; ++index)
      {
        foreach(GroupMember groupMember in SubGroups[index])
        {
          if(groupMember.Character != null)
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
        new SynchronizedCharacterList(m_Count, FactionGroup);
      using(syncLock.EnterReadLock())
      {
        for(int index = 0; index < SubGroups.Length; ++index)
        {
          foreach(GroupMember groupMember in SubGroups[index])
          {
            if(groupMember.Character != null)
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
      foreach(Character allCharacter in GetAllCharacters())
        allCharacter.Client.Send(packet, addEnd);
    }

    /// <summary>
    /// Selects and returns the next online Member whose turn it is in RoundRobin.
    /// </summary>
    /// <returns>null if all members of this Group are offline.</returns>
    public GroupMember GetNextRoundRobinMember()
    {
      using(syncLock.EnterWriteLock())
      {
        m_roundRobinMember =
          m_roundRobinMember != null ? m_roundRobinMember.Next : m_firstMember;
        while(m_roundRobinMember.Character == null)
        {
          m_roundRobinMember = m_roundRobinMember.Next;
          if(m_roundRobinMember == m_firstMember)
            return null;
        }
      }

      return m_roundRobinMember;
    }

    public void GetNearbyLooters(IAsda2Lootable lootable, WorldObject initialLooter,
      ICollection<Asda2LooterEntry> looters)
    {
      foreach(Character objectsInRadiu in (!(lootable is WorldObject)
          ? initialLooter
          : (WorldObject) lootable)
        .GetObjectsInRadius(Asda2LootMgr.LootRadius, ObjectTypes.Player, false, 0))
      {
        GroupMember groupMember;
        if(objectsInRadiu.IsAlive && (objectsInRadiu == initialLooter ||
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
      using(syncLock.EnterWriteLock())
      {
        LootMethod = method;
        m_masterLooter = masterLooter;
        LootThreshold = lootThreshold;
      }

      SendUpdate();
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
      if(member.Character == null || member.Group != this)
        return;
      foreach(Character allCharacter in GetAllCharacters())
      {
        if(allCharacter != member.Character && allCharacter != null &&
           (!allCharacter.IsInUpdateRange(member.Character) &&
            member.Character.GroupUpdateFlags != GroupUpdateFlags.None))
          GroupHandler.SendPartyMemberStats(allCharacter.Client, member,
            member.Character.GroupUpdateFlags);
      }

      member.Character.GroupUpdateFlags = GroupUpdateFlags.None;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// All members of this Group and all its SubGroups.
    /// Don't forget to lock the SyncRoot while iterating over a Group.
    /// </summary>
    public IEnumerator<GroupMember> GetEnumerator()
    {
      foreach(SubGroup subGroup in SubGroups)
      {
        IEnumerator<GroupMember> enumerator = subGroup.GetEnumerator();
        while(enumerator.MoveNext())
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
        if(m_leader == null)
          return null;
        return m_leader.Character;
      }
    }

    public InstanceCollection InstanceLeaderCollection
    {
      get
      {
        if(m_leader == null)
          return null;
        return m_leader.Character.Instances;
      }
    }

    public void ForeachInstanceHolder(Action<InstanceCollection> callback)
    {
      foreach(Character allCharacter in GetAllCharacters())
      {
        InstanceCollection instances = allCharacter.Instances;
        if(instances != null)
          callback(instances);
      }
    }

    /// <summary>
    /// Gets the Instance of the given Map of either the Leader or any member
    /// if anyone is already in it.
    /// </summary>
    public BaseInstance GetActiveInstance(MapTemplate map)
    {
      GroupMember leader = m_leader;
      if(leader != null)
      {
        Character character = leader.Character;
        if(character != null)
        {
          InstanceCollection instances = character.Instances;
          if(instances != null)
          {
            BaseInstance activeInstance = instances.GetActiveInstance(map);
            if(activeInstance != null)
              return activeInstance;
          }
        }
      }

      foreach(Character allCharacter in GetAllCharacters())
      {
        BaseInstance activeInstance = allCharacter.GetActiveInstance(map);
        if(activeInstance != null)
          return activeInstance;
      }

      return null;
    }

    public void DistributeGroupHonor(Character earner, Character victim, uint honorPoints)
    {
      if(CharacterCount < 1)
        return;
      uint bonus = honorPoints / (uint) CharacterCount;
      ForeachCharacter(chr =>
      {
        if(!chr.IsInRange(new SimpleRange(0.0f, MaxKillRewardDistance), earner))
          return;
        chr.GiveHonorPoints(bonus);
        ++chr.KillsToday;
        ++chr.LifetimeHonorableKills;
        HonorHandler.SendPVPCredit(chr, bonus * 10U, victim);
      });
    }

    public void OnKill(Character killer, NPC victim)
    {
      if(CharacterCount < 1)
        return;
      ForeachCharacter(chr =>
      {
        if(chr.Map != victim.Map || !chr.IsInRange(new SimpleRange(0.0f, MaxKillRewardDistance),
             killer))
          return;
        chr.QuestLog.OnNPCInteraction(victim);
        chr.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KillCreature, victim.EntryId,
          1U, null);
      });
    }

    /// <summary>Kick every non-staff member</summary>
    public void EnsurePureStaffGroup()
    {
      using(syncLock.EnterReadLock())
      {
        foreach(GroupMember groupMember in this.ToArray())
        {
          Character chr = groupMember.Character;
          if(chr == null || !chr.Role.IsStaff)
          {
            groupMember.LeaveGroup();
            if(chr != null)
              chr.AddMessage(() =>
                chr.SendSystemMessage(
                  "You have been kicked from the group since you are not a staff member."));
          }
        }
      }
    }

    public static event GroupMemberHandler MemberAdded;

    public static event GroupMemberHandler MemberRemoved;

    public static event GroupLeaderChangedHandler LeaderChanged;

    public delegate void GroupMemberHandler(GroupMember member);

    public delegate void GroupLeaderChangedHandler(GroupMember oldLeader, GroupMember newLeader);
  }
}