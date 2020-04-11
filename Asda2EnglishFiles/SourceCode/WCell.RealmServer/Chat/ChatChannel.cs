using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Chat;
using WCell.Constants.Misc;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.Util.Collections;

namespace WCell.RealmServer.Chat
{
    /// <summary>
    ///  TODO: Implement Channel ids correctly: Send channel id and hope its used by the client to lookup the correct name locally
    ///  TODO: Have Zone channels stored in a list per ZoneInfo object and updated correctly
    ///  TODO: Cleanup/move special handling to seperate handlers
    ///  TODO: Get rid of ImmutableDictionary and instead put channel-specific stuff into its own message-loop
    /// 
    ///  The chat channel class
    /// </summary>
    public class ChatChannel : IChatTarget, IGenericChatTarget
    {
        /// <summary>Default validator for staff-only channels.</summary>
        public static readonly ChatChannel.JoinValidationHandler StaffValidator =
            (ChatChannel.JoinValidationHandler) ((chan, user) => user.Role.IsStaff);

        /// <summary>Default staff-only channel.</summary>
        public static readonly ChatChannel Staff = new ChatChannel(ChatChannelGroup.Global, nameof(Staff),
            ChatChannelFlags.Global, true, ChatChannel.StaffValidator);

        private string m_name;
        private ChannelMember m_owner;
        private ChatChannelFlagsEntry m_flagsEntry;
        private readonly ChatChannelGroup m_group;
        private readonly ImmutableDictionary<uint, ChannelMember> m_members;
        private readonly List<uint> m_bannedEntities;
        private ChatChannel.JoinValidationHandler m_joinValidator;
        public readonly int ChannelId;

        /// <summary>Default Ctor</summary>
        public ChatChannel(ChatChannelGroup group)
        {
            this.m_group = group;
            this.m_members = new ImmutableDictionary<uint, ChannelMember>();
            this.Name = this.Password = string.Empty;
            this.Announces = true;
            this.m_bannedEntities = new List<uint>();
        }

        /// <summary>Constructor</summary>
        /// <param name="name">name of channel</param>
        public ChatChannel(ChatChannelGroup group, string name)
            : this(group)
        {
            this.Name = name;
        }

        /// <summary>Constructor</summary>
        /// <param name="name">name of channel</param>
        public ChatChannel(ChatChannelGroup group, string name, ChatChannelFlagsEntry flags, bool constant,
            ChatChannel.JoinValidationHandler joinValidator)
            : this(group, name)
        {
            this.m_flagsEntry = flags;
            this.m_joinValidator = joinValidator;
            this.IsConstant = constant;
        }

        /// <summary>Constructor</summary>
        /// <param name="name">name of channel</param>
        public ChatChannel(ChatChannelGroup group, string name, ChatChannelFlags flags, bool constant,
            ChatChannel.JoinValidationHandler joinValidator)
            : this(group, name)
        {
            this.m_flagsEntry = new ChatChannelFlagsEntry()
            {
                Flags = flags,
                ClientFlags = ChatMgr.Convert(flags)
            };
            this.m_joinValidator = joinValidator;
            this.IsConstant = constant;
        }

        public ChatChannel(ChatChannelGroup group, uint channelId, string name)
            : this(group, name)
        {
            ChatChannelGroup.DefaultChannelFlags.TryGetValue(channelId, out this.m_flagsEntry);
        }

        public ChatChannel(ChatChannelGroup group, string name, ChatChannelFlags flags, bool constant)
            : this(group, name)
        {
            this.m_flagsEntry = new ChatChannelFlagsEntry()
            {
                Flags = flags,
                ClientFlags = ChatMgr.Convert(flags)
            };
            this.IsConstant = constant;
        }

        /// <summary>
        /// Gets the chat channel manager that this channel resides under.
        /// </summary>
        public ChatChannelGroup Manager
        {
            get { return this.m_group; }
        }

        /// <summary>The join validator for this channel.</summary>
        public ChatChannel.JoinValidationHandler JoinValidator
        {
            get { return this.m_joinValidator; }
        }

        /// <summary>
        /// A map of all users in the channel, sorted by low entity ID.
        /// </summary>
        public IDictionary<uint, ChannelMember> Members
        {
            get { return (IDictionary<uint, ChannelMember>) this.m_members; }
        }

        /// <summary>Gets or sets the owner of this channel.</summary>
        public ChannelMember Owner
        {
            get { return this.m_owner; }
            set
            {
                if (this.m_owner != null)
                    this.m_owner.IsOwner = false;
                if (value == null)
                    return;
                this.m_owner = value;
                this.m_owner.Flags =
                    ChannelMemberFlags.Owner | ChannelMemberFlags.Moderator | ChannelMemberFlags.Voiced;
            }
        }

        /// <summary>Gets the name of the channel.</summary>
        public string Name
        {
            get { return this.m_name; }
            set { this.m_name = value; }
        }

        /// <summary>Gets or sets the password for the channel.</summary>
        public string Password { get; set; }

        /// <summary>
        /// Determines whether or not joins/parts of the channel
        /// are announced to all the channel users.
        /// </summary>
        public bool Announces { get; set; }

        /// <summary>
        /// Whether or not the channel is constant. That is, General/LFG/Guild Rec., etc.
        /// </summary>
        public bool IsConstant { get; set; }

        /// <summary>Whether or not the channel is moderated.</summary>
        public bool IsModerated { get; set; }

        public ChatChannelFlags Flags
        {
            get { return this.m_flagsEntry.Flags; }
        }

        public ChatChannelFlagsClient ClientFlags
        {
            get { return this.m_flagsEntry.ClientFlags; }
        }

        public List<uint> BannedEntities
        {
            get { return this.m_bannedEntities; }
        }

        public int MemberCount
        {
            get { return this.m_members.Count; }
        }

        public int RequiredRank { get; set; }

        public bool IsCityOnly
        {
            get { return this.m_flagsEntry.Flags.HasFlag((Enum) ChatChannelFlags.CityOnly); }
        }

        public bool IsZoneSpecific
        {
            get { return this.m_flagsEntry.Flags.HasFlag((Enum) ChatChannelFlags.ZoneSpecific); }
        }

        /// <summary>TODO: Looking for Group</summary>
        public bool IsLFG
        {
            get { return this.m_flagsEntry.Flags.HasFlag((Enum) ChatChannelFlags.LookingForGroup); }
        }

        /// <summary>TODO: Trade</summary>
        public bool IsTrade
        {
            get { return this.m_flagsEntry.Flags.HasFlag((Enum) ChatChannelFlags.Trade); }
        }

        /// <summary>TODO: Guild Recruitment</summary>
        public bool RequiresUnguilded
        {
            get { return this.m_flagsEntry.Flags.HasFlag((Enum) ChatChannelFlags.RequiresUnguilded); }
        }

        public void ToggleModerated(ChannelMember toggler)
        {
            if (toggler.IsOwner || toggler.User.Role.IsStaff)
            {
                this.IsModerated = !this.IsModerated;
                ChannelHandler.SendModerateToEveryone(this, toggler.User.EntityId);
            }
            else
                ChannelHandler.SendNotOwnerReply((IPacketReceiver) toggler.User, this.m_name);
        }

        /// <summary>
        /// Whether the given Entity-id is present in this channel
        /// </summary>
        /// <param name="lowId">the player to look for</param>
        /// <returns>true if the player is in this channel; false otherwise</returns>
        public bool IsPresent(uint lowId)
        {
            return this.m_members.ContainsKey(lowId);
        }

        /// <summary>
        /// Whether the given Entity-id is present in this channel
        /// </summary>
        /// <returns>true if the player is in this channel; false otherwise</returns>
        public bool EnsurePresence(IUser user, out ChannelMember member)
        {
            if (this.m_members.TryGetValue(user.EntityId.Low, out member))
                return true;
            ChannelHandler.SendNotOnChannelReply((IPacketReceiver) user, this.m_name);
            member = (ChannelMember) null;
            return false;
        }

        /// <summary>Adds a player to this channel.</summary>
        public void TryJoin(IUser chatter)
        {
            this.TryJoin(chatter, (string) null, false);
        }

        /// <summary>Adds a player to this channel.</summary>
        public void TryJoin(IUser user, string password, bool silent)
        {
            if (this.IsTrade || this.RequiresUnguilded || this.IsLFG)
                return;
            uint low = user.EntityId.Low;
            if (this.IsBanned(low) || this.m_joinValidator != null && !this.m_joinValidator(this, user))
                ChannelHandler.SendBannedReply((IPacketReceiver) user, this.Name);
            else if (this.IsPresent(low))
            {
                if (this.IsConstant)
                    return;
                ChannelHandler.SendAlreadyOnChannelReply((IPacketReceiver) user, this.Name, user.EntityId);
            }
            else if (this.Password.Length > 0 && this.Password != password)
            {
                ChannelHandler.SendWrongPassReply((IPacketReceiver) user, this.Name);
            }
            else
            {
                ChannelMember sender = user.Role.IsStaff
                    ? new ChannelMember(user, ChannelMemberFlags.Moderator)
                    : new ChannelMember(user);
                if (this.Announces)
                    ChannelHandler.SendJoinedReplyToEveryone(this, sender);
                if (!this.IsConstant && this.m_owner == null)
                    this.Owner = sender;
                this.m_members.Add(low, sender);
                user.ChatChannels.Add(this);
            }
        }

        /// <summary>Removes a player from a channel.</summary>
        /// <param name="silent">whether or not to tell the player/channel they have left</param>
        public void Leave(IUser user, bool silent)
        {
            ChannelMember member;
            if (!this.m_members.TryGetValue(user.EntityId.Low, out member))
            {
                if (silent)
                    return;
                ChannelHandler.SendNotOnChannelReply((IPacketReceiver) user, this.m_name);
            }
            else
            {
                if (!silent)
                    ChannelHandler.SendYouLeftChannelReply((IPacketReceiver) user, this.m_name, this.ChannelId);
                this.OnUserLeft(member);
            }
        }

        private void OnUserLeft(ChannelMember member)
        {
            bool isOwner = member.IsOwner;
            member.User.ChatChannels.Remove(this);
            this.m_members.Remove(member.User.EntityId.Low);
            if (!isOwner)
                return;
            member = this.m_members.Values.FirstOrDefault<ChannelMember>();
            if (member != null)
            {
                if (this.Announces)
                    ChannelHandler.SendLeftReplyToEveryone(this, member.User.EntityId);
                this.Owner = member;
            }
            else
            {
                this.m_group.DeleteChannel(this);
                this.m_owner = (ChannelMember) null;
            }
        }

        /// <summary>Deletes this channel</summary>
        public void Delete()
        {
            foreach (ChannelMember channelMember in (IEnumerable<ChannelMember>) this.m_members.Values)
            {
                this.m_owner.IsOwner = false;
                this.Leave(channelMember.User, false);
                this.m_group.DeleteChannel(this);
            }
        }

        /// <summary>Kicks and -maybe- bans a player from the channel.</summary>
        /// <returns>Whether the player was found and kicked or false if not found or privs were insufficient.</returns>
        public bool Kick(ChannelMember member, string targetName)
        {
            if (!string.IsNullOrEmpty(targetName))
            {
                IUser namedEntity = World.GetNamedEntity(targetName, true) as IUser;
                if (namedEntity != null)
                    return this.Kick(member, namedEntity);
            }

            return false;
        }

        /// <summary>Kicks and -maybe- bans a player from the channel.</summary>
        /// <returns>Whether the player was kicked or false if privs were insufficient.</returns>
        public bool Kick(ChannelMember member, IUser targetUser)
        {
            IUser user = member.User;
            ChannelMember member1;
            if (!this.EnsurePresence(targetUser, out member1))
            {
                ChannelHandler.SendTargetNotOnChannelReply((IPacketReceiver) user, this.m_name, targetUser.Name);
                return false;
            }

            EntityId entityId = targetUser.EntityId;
            if (this.CheckPrivs(member, member1, false))
                ChannelHandler.SendKickedToEveryone(this, user.EntityId, entityId);
            this.m_members.Remove(entityId.Low);
            this.OnUserLeft(member1);
            return true;
        }

        public EntityId EntityId
        {
            get { return EntityId.Zero; }
        }

        public IEnumerable<IUser> GetUsers()
        {
            foreach (ChannelMember channelMember in (IEnumerable<ChannelMember>) this.m_members.Values)
                yield return channelMember.User;
        }

        public void Send(RealmPacketOut packet)
        {
            foreach (IPacketReceiver user in this.GetUsers())
                user.Send(packet, false);
        }

        public bool MakeOwner(ChannelMember oldOwner, ChannelMember newOwner)
        {
            if (!newOwner.IsOwner)
            {
                if (!oldOwner.IsOwner && !oldOwner.User.Role.IsStaff)
                {
                    ChannelHandler.SendNotOwnerReply((IPacketReceiver) oldOwner.User, this.m_name);
                }
                else
                {
                    ChannelHandler.SendModeratorStatusToEveryone(this, oldOwner.User.EntityId, newOwner.User.EntityId,
                        true);
                    ChannelHandler.SendOwnerChangedToEveryone(this, oldOwner.User.EntityId, newOwner.User.EntityId);
                    this.Owner = newOwner;
                    return true;
                }
            }

            return false;
        }

        public bool SetModerator(ChannelMember mod, ChannelMember newMod, bool makeMod)
        {
            if (newMod.IsModerator != makeMod)
            {
                if (!mod.IsOwner && newMod.IsOwner && !mod.User.Role.IsStaff)
                {
                    ChannelHandler.SendNotOwnerReply((IPacketReceiver) mod.User, this.m_name);
                }
                else
                {
                    newMod.IsModerator = makeMod;
                    ChannelHandler.SendModeratorStatusToEveryone(this, mod.User.EntityId, newMod.User.EntityId,
                        makeMod);
                    return true;
                }
            }

            return false;
        }

        public void SetMuted(ChannelMember member, ChannelMember targetMember, bool muted)
        {
            if (targetMember.IsMuted == muted || !this.CheckPrivs(member, targetMember, !muted))
                return;
            targetMember.IsMuted = muted;
            ChannelHandler.SendMuteStatusToEveryone(this, member.User.EntityId, targetMember.User.EntityId, muted);
        }

        /// <summary>Whether given Entity-id is banned from this channel</summary>
        /// <param name="lowId">the player to look for</param>
        /// <returns>true if the player is banned; false otherwise</returns>
        public bool IsBanned(uint lowId)
        {
            return this.m_bannedEntities.Contains(lowId);
        }

        public bool SetBanned(ChannelMember member, string targetName, bool addBan)
        {
            IUser namedEntity = World.GetNamedEntity(targetName, true) as IUser;
            if (namedEntity != null)
                return this.SetBanned(member, namedEntity, false);
            ChatMgr.SendChatPlayerNotFoundReply((IPacketReceiver) member.User, targetName);
            return false;
        }

        /// <summary>
        /// Returns whether the Ban could be added/removed or false if privs were insufficient.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="target"></param>
        /// <param name="addBan"></param>
        /// <returns></returns>
        public bool SetBanned(ChannelMember member, IUser target, bool addBan)
        {
            if (!member.IsModerator && !member.User.Role.IsStaff)
                return false;
            ChannelMember member1;
            if (addBan && this.EnsurePresence(target, out member1) && member1 > member)
            {
                if (member1.IsModerator)
                    ChannelHandler.SendNotOwnerReply((IPacketReceiver) member.User, this.m_name);
                return false;
            }

            if (addBan)
            {
                if (!this.m_bannedEntities.Contains(target.EntityId.Low))
                {
                    ChannelHandler.SendTargetNotOnChannelReply((IPacketReceiver) member.User, this.m_name, target.Name);
                }
                else
                {
                    this.m_bannedEntities.Add(target.EntityId.Low);
                    ChannelHandler.SendBannedToEveryone(this, member.User.EntityId, target.EntityId);
                }
            }
            else if (!this.m_bannedEntities.Remove(target.EntityId.Low))
                ChannelHandler.SendTargetNotOnChannelReply((IPacketReceiver) member.User, this.m_name, target.Name);
            else
                ChannelHandler.SendUnbannedToEveryone(this, member.User.EntityId, target.EntityId);

            return true;
        }

        /// <summary>
        /// Ensures that the given user is on the channel and has mod status.
        /// If not, sends corresponding error messages to user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public static ChatChannel EnsureModerator(IUser user, string channelName)
        {
            ChatChannel chatChannel = ChatChannelGroup.RetrieveChannel(user, channelName);
            if (chatChannel == null)
                return (ChatChannel) null;
            uint low = user.EntityId.Low;
            ChannelMember channelMember;
            if (!chatChannel.Members.TryGetValue(low, out channelMember))
            {
                ChannelHandler.SendNotOnChannelReply((IPacketReceiver) user, channelName);
            }
            else
            {
                if (channelMember.IsModerator)
                    return chatChannel;
                ChannelHandler.SendNotModeratorReply((IPacketReceiver) user, channelName);
            }

            return (ChatChannel) null;
        }

        /// <summary>
        /// Retrieves the given ChannelMember and the Channel it is on.
        /// If not, sends corresponding error messages to user and returns null.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public static ChatChannel EnsurePresence(IUser user, string channelName, out ChannelMember member)
        {
            ChatChannel chatChannel = ChatChannelGroup.RetrieveChannel(user, channelName);
            if (chatChannel != null)
            {
                uint low = user.EntityId.Low;
                if (chatChannel.Members.TryGetValue(low, out member))
                    return chatChannel;
                ChannelHandler.SendNotOnChannelReply((IPacketReceiver) user, channelName);
                return (ChatChannel) null;
            }

            member = (ChannelMember) null;
            return (ChatChannel) null;
        }

        /// <summary>
        /// Retrieve the given two ChannelMembers and the Channel they are on, if they
        /// are all on the channel. If not, sends corresponding error messages to user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="channelName"></param>
        /// <param name="targetName"></param>
        /// <param name="userMember"></param>
        /// <param name="targetMember"></param>
        /// <returns></returns>
        public static ChatChannel EnsurePresence(IUser user, string channelName, string targetName,
            out ChannelMember userMember, out ChannelMember targetMember)
        {
            if (!string.IsNullOrEmpty(targetName))
            {
                ChatChannel chatChannel = ChatChannelGroup.RetrieveChannel(user, channelName);
                if (chatChannel != null)
                {
                    uint low = user.EntityId.Low;
                    if (!chatChannel.Members.TryGetValue(low, out userMember))
                    {
                        ChannelHandler.SendNotOnChannelReply((IPacketReceiver) user, channelName);
                    }
                    else
                    {
                        IUser namedEntity = World.GetNamedEntity(targetName, false) as IUser;
                        if (namedEntity == null ||
                            !chatChannel.Members.TryGetValue(namedEntity.EntityId.Low, out targetMember))
                            ChannelHandler.SendTargetNotOnChannelReply((IPacketReceiver) user, channelName, targetName);
                        else if (namedEntity != user)
                            return chatChannel;
                    }
                }
            }

            userMember = (ChannelMember) null;
            targetMember = (ChannelMember) null;
            return (ChatChannel) null;
        }

        /// <summary>
        /// Checks for whether the given member can perform a beneficial or harmful action on the targetMember
        /// within this channel.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="targetMember"></param>
        /// <param name="beneficial"></param>
        /// <returns></returns>
        public bool CheckPrivs(ChannelMember member, ChannelMember targetMember, bool beneficial)
        {
            if (beneficial)
            {
                if (!member.IsModerator || member.User.Role.IsStaff)
                {
                    ChannelHandler.SendNotModeratorReply((IPacketReceiver) member.User, this.m_name);
                    return false;
                }
            }
            else if (targetMember > member)
            {
                if (targetMember.IsModerator)
                    ChannelHandler.SendNotOwnerReply((IPacketReceiver) member.User, this.m_name);
                else
                    ChannelHandler.SendNotModeratorReply((IPacketReceiver) member.User, this.m_name);
                return false;
            }

            return true;
        }

        public void Invite(ChannelMember inviter, string targetName)
        {
            IUser namedEntity = World.GetNamedEntity(targetName, false) as IUser;
            if (namedEntity != null)
                this.Invite(inviter, namedEntity);
            else
                ChatMgr.SendChatPlayerNotFoundReply((IPacketReceiver) inviter.User, targetName);
        }

        public void Invite(ChannelMember inviter, IUser target)
        {
            if (!target.IsIgnoring(inviter.User) || inviter.User.Role.IsStaff)
            {
                if (this.IsPresent(target.EntityId.Low))
                    ChannelHandler.SendAlreadyOnChannelReply((IPacketReceiver) inviter.User, target.Name,
                        target.EntityId);
                else if (target.FactionGroup != inviter.User.FactionGroup)
                {
                    ChannelHandler.SendWrongFaction((IPacketReceiver) inviter.User, this.m_name, target.Name);
                }
                else
                {
                    ChannelHandler.SendInvitedMessage((IPacketReceiver) target, this.m_name, inviter.User.EntityId);
                    ChannelHandler.SendYouInvitedReply((IPacketReceiver) inviter.User, this.m_name, target.Name);
                }
            }
            else
                ChatMgr.SendChatPlayerNotFoundReply((IPacketReceiver) inviter.User, target.Name);
        }

        public void SendMessage(string message)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_MESSAGECHAT))
            {
                packet.Write((byte) 17);
                packet.Write(7U);
                packet.Write((ulong) EntityId.Zero);
                packet.WriteCString("");
                packet.Write((ulong) EntityId.Zero);
                packet.WriteUIntPascalString(message);
                packet.Write((byte) 0);
                ChannelHandler.SendPacketToChannel(this, packet);
            }

            ChatMgr.ChatNotify((IChatter) null, message, ChatLanguage.Common, ChatMsgType.Channel,
                (IGenericChatTarget) this);
        }

        /// <summary>Sends a message to this channel.</summary>
        /// <param name="sender">the chatter saying the message</param>
        public void SendMessage(IChatter sender, string message)
        {
            ChannelMember channelMember;
            if (!this.Members.TryGetValue(sender.EntityId.Low, out channelMember))
                ChannelHandler.SendNotOnChannelReply((IPacketReceiver) sender, this.m_name);
            else if (channelMember.IsMuted)
                ChannelHandler.SendMutedReply((IPacketReceiver) sender, this.m_name);
            else if (this.IsModerated && !channelMember.IsModerator)
            {
                ChannelHandler.SendNotOnChannelReply((IPacketReceiver) sender, this.m_name);
            }
            else
            {
                if (sender is IUser &&
                    RealmCommandHandler.HandleCommand((IUser) sender, message, (IGenericChatTarget) this))
                    return;
                ChannelHandler.SendPacketToChannel(this,
                    ChatMgr.CreateNormalChatMessagePacket(sender.Name, message, Locale.Start, (Character) null));
            }
        }

        /// <summary>Delegate for join validators.</summary>
        /// <param name="chatChannel">the channel being joined</param>
        /// <param name="user">the user joining the channel</param>
        /// <returns>whether or not the user can join the channel</returns>
        public delegate bool JoinValidationHandler(ChatChannel chatChannel, IUser user);
    }
}