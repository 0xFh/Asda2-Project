using System.Collections.Generic;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class ChannelHandler
    {
        /// <summary>Handles an incoming channel join request</summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleJoinChannel(IRealmClient client, RealmPacketIn packet)
        {
            uint channelId = packet.ReadUInt32();
            int num1 = (int) packet.ReadByte();
            int num2 = (int) packet.ReadByte();
            string name = packet.ReadCString();
            string password = packet.ReadCString();
            ChatChannelGroup group = ChatChannelGroup.GetGroup(client.ActiveCharacter.Faction.Group);
            if (group == null || string.IsNullOrEmpty(name))
                return;
            group.GetChannel(name, channelId, true).TryJoin((IUser) client.ActiveCharacter, password, false);
        }

        /// <summary>Handles an incoming channel leave request</summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleLeaveChannel(IRealmClient client, RealmPacketIn packet)
        {
            int num = (int) packet.ReadUInt32();
            string channelName = packet.ReadCString();
            ChatChannel chatChannel = ChatChannelGroup.RetrieveChannel((IUser) client.ActiveCharacter, channelName);
            if (chatChannel == null)
                return;
            chatChannel.Leave((IUser) client.ActiveCharacter, false);
        }

        /// <summary>Handles an incoming channel list request</summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleListChannel(IRealmClient client, RealmPacketIn packet)
        {
            string str = packet.ReadCString();
            ChatChannel chan = ChatChannelGroup.RetrieveChannel((IUser) client.ActiveCharacter, str);
            if (chan != null)
                ChannelHandler.SendChannelList((IPacketReceiver) client, chan);
            else
                ChannelHandler.SendNotOnChannelReply((IPacketReceiver) client, str);
        }

        /// <summary>Handles an incoming channel password change request</summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandlePasswordChange(IRealmClient client, RealmPacketIn packet)
        {
            string str1 = packet.ReadCString();
            string str2 = packet.ReadCString();
            ChannelMember member;
            ChatChannel chan = ChatChannel.EnsurePresence((IUser) client.ActiveCharacter, str1, out member);
            if (chan != null)
            {
                if (!member.IsModerator)
                {
                    ChannelHandler.SendNotModeratorReply((IPacketReceiver) client, str1);
                }
                else
                {
                    chan.Password = str2;
                    ChannelHandler.SendPasswordChangedToEveryone(chan, client.ActiveCharacter.EntityId);
                }
            }
            else
                ChannelHandler.SendNotOnChannelReply((IPacketReceiver) client, str1);
        }

        /// <summary>Handles an incoming request of current owner</summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleCurrentOwnerRequest(IRealmClient client, RealmPacketIn packet)
        {
            string channelName = packet.ReadCString();
            ChatChannel chan = ChatChannel.EnsureModerator((IUser) client.ActiveCharacter, channelName);
            if (chan == null)
                return;
            ChannelHandler.SendCurrentOwner((IPacketReceiver) client, chan);
        }

        /// <summary>Handles an incoming owner set request</summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleOwnerChange(IRealmClient client, RealmPacketIn packet)
        {
            string channelName = packet.ReadCString();
            string targetName = packet.ReadCString();
            ChannelMember userMember;
            ChannelMember targetMember;
            ChatChannel chatChannel = ChatChannel.EnsurePresence((IUser) client.ActiveCharacter, channelName,
                targetName, out userMember, out targetMember);
            if (chatChannel == null)
                return;
            chatChannel.MakeOwner(userMember, targetMember);
        }

        /// <summary>
        /// Handles a request of making channel member a moderator
        /// </summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleSetModeratorRequest(IRealmClient client, RealmPacketIn packet)
        {
            string channelName = packet.ReadCString();
            string targetName = packet.ReadCString();
            ChannelMember userMember;
            ChannelMember targetMember;
            ChatChannel chatChannel = ChatChannel.EnsurePresence((IUser) client.ActiveCharacter, channelName,
                targetName, out userMember, out targetMember);
            if (chatChannel == null)
                return;
            chatChannel.SetModerator(userMember, targetMember, true);
        }

        /// <summary>
        /// Handles a request of making channel member a non-moderator
        /// </summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleUnsetModeratorRequest(IRealmClient client, RealmPacketIn packet)
        {
            string channelName = packet.ReadCString();
            string targetName = packet.ReadCString();
            ChannelMember userMember;
            ChannelMember targetMember;
            ChatChannel chatChannel = ChatChannel.EnsurePresence((IUser) client.ActiveCharacter, channelName,
                targetName, out userMember, out targetMember);
            if (chatChannel == null)
                return;
            chatChannel.SetModerator(userMember, targetMember, false);
        }

        /// <summary>Handles a request of muting a channel member</summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleMuteRequest(IRealmClient client, RealmPacketIn packet)
        {
            string channelName = packet.ReadCString();
            string targetName = packet.ReadCString();
            ChannelMember userMember;
            ChannelMember targetMember;
            ChatChannel chatChannel = ChatChannel.EnsurePresence((IUser) client.ActiveCharacter, channelName,
                targetName, out userMember, out targetMember);
            if (chatChannel == null)
                return;
            chatChannel.SetMuted(userMember, targetMember, true);
        }

        /// <summary>Handles a request of unmuting a channel member</summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleUnMuteRequest(IRealmClient client, RealmPacketIn packet)
        {
            string channelName = packet.ReadCString();
            string targetName = packet.ReadCString();
            ChannelMember userMember;
            ChannelMember targetMember;
            ChatChannel chatChannel = ChatChannel.EnsurePresence((IUser) client.ActiveCharacter, channelName,
                targetName, out userMember, out targetMember);
            if (chatChannel == null)
                return;
            chatChannel.SetMuted(userMember, targetMember, false);
        }

        /// <summary>Handles a invite to channel packet</summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleChannelInvite(IRealmClient client, RealmPacketIn packet)
        {
            string channelName = packet.ReadCString();
            string targetName = packet.ReadCString();
            ChannelMember member;
            ChatChannel chatChannel =
                ChatChannel.EnsurePresence((IUser) client.ActiveCharacter, channelName, out member);
            if (chatChannel == null)
                return;
            chatChannel.Invite(member, targetName);
        }

        /// <summary>Handles a request of kicking a channel member</summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleKickRequest(IRealmClient client, RealmPacketIn packet)
        {
            string channelName = packet.ReadCString();
            string targetName = packet.ReadCString();
            ChannelMember member;
            ChatChannel chatChannel =
                ChatChannel.EnsurePresence((IUser) client.ActiveCharacter, channelName, out member);
            if (chatChannel == null)
                return;
            chatChannel.Kick(member, targetName);
        }

        /// <summary>Handles a request of banning a channel member</summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleBanRequest(IRealmClient client, RealmPacketIn packet)
        {
            string channelName = packet.ReadCString();
            string targetName = packet.ReadCString();
            ChannelMember member;
            ChatChannel chatChannel =
                ChatChannel.EnsurePresence((IUser) client.ActiveCharacter, channelName, out member);
            if (chatChannel == null)
                return;
            chatChannel.SetBanned(member, targetName, true);
        }

        /// <summary>Handles a request of unbanning a channel member</summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleUnbanRequest(IRealmClient client, RealmPacketIn packet)
        {
            string channelName = packet.ReadCString();
            string targetName = packet.ReadCString();
            ChannelMember member;
            ChatChannel chatChannel =
                ChatChannel.EnsurePresence((IUser) client.ActiveCharacter, channelName, out member);
            if (chatChannel == null)
                return;
            chatChannel.SetBanned(member, targetName, false);
        }

        /// <summary>
        /// Handles a request of toggling the announce mode of the channel
        /// </summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleAnnouncementsRequest(IRealmClient client, RealmPacketIn packet)
        {
            string channelName = packet.ReadCString();
            ChatChannel chan = ChatChannel.EnsureModerator((IUser) client.ActiveCharacter, channelName);
            if (chan == null)
                return;
            chan.Announces = !chan.Announces;
            ChannelHandler.SendAnnouncementToEveryone(chan, client.ActiveCharacter.EntityId, chan.Announces);
        }

        /// <summary>
        /// Handles a request of toggling the moderate mode of the channel
        /// </summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleModerateRequest(IRealmClient client, RealmPacketIn packet)
        {
            string channelName = packet.ReadCString();
            ChannelMember member;
            ChatChannel chatChannel =
                ChatChannel.EnsurePresence((IUser) client.ActiveCharacter, channelName, out member);
            if (chatChannel == null)
                return;
            chatChannel.ToggleModerated(member);
        }

        /// <summary>
        /// Handles a request of getting the number of members in a channel
        /// </summary>
        /// <param name="client">the client the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        public static void HandleMemberCountRequest(IRealmClient client, RealmPacketIn packet)
        {
            string channelName = packet.ReadCString();
            Character activeCharacter = client.ActiveCharacter;
            if (activeCharacter == null)
            {
                ChannelHandler.SendMemberCountReply((IPacketReceiver) client, channelName, (byte) 0, 0U);
            }
            else
            {
                ChatChannel chatChannel = ChatChannelGroup.RetrieveChannel((IUser) client.ActiveCharacter, channelName);
                if (chatChannel == null)
                    return;
                ChannelHandler.SendMemberCountReply((IPacketReceiver) activeCharacter, chatChannel.Name,
                    (byte) chatChannel.Flags, (uint) chatChannel.MemberCount);
            }
        }

        /// <summary>Send the "already on channel" reply</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">name of channel</param>
        public static void SendAlreadyOnChannelReply(IPacketReceiver client, string chan, EntityId entityId)
        {
        }

        /// <summary>Send the "you are banned" reply</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">name of channel</param>
        public static void SendBannedReply(IPacketReceiver client, string chan)
        {
        }

        /// <summary>Send the "wrong password" reply</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">name of channel</param>
        public static void SendWrongPassReply(IPacketReceiver client, string chan)
        {
        }

        /// <summary>Send the "wrong password" reply</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">name of channel</param>
        public static void SendWrongFaction(IPacketReceiver client, string chan, string invitedName)
        {
        }

        /// <summary>Send the "you have joined channel" reply</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">name of channel</param>
        public static void SendYouJoinedReply(IPacketReceiver client, ChatChannel chan)
        {
        }

        /// <summary>Send the "you are not on the channel" reply</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">name of channel</param>
        public static void SendNotOnChannelReply(IPacketReceiver client, string chan)
        {
        }

        /// <summary>Send the "name is not on the channel" reply</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">name of channel</param>
        public static void SendTargetNotOnChannelReply(IPacketReceiver client, string chan, string playerName)
        {
        }

        /// <summary>Send the "you have left the channel" reply</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">name of channel</param>
        /// <param name="channelId">Id of official channel in client (or 0 for custom channels)</param>
        public static void SendYouLeftChannelReply(IPacketReceiver client, string chan, int channelId)
        {
        }

        /// <summary>Send the "you are not a moderator" reply</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">name of channel</param>
        public static void SendNotModeratorReply(IPacketReceiver client, string chan)
        {
        }

        /// <summary>Send the "you have invited ... to channel" reply</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">name of channel</param>
        public static void SendYouInvitedReply(IPacketReceiver client, string chan, string invitedName)
        {
        }

        /// <summary>Send the "... has invited you to channel" message</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">name of channel</param>
        /// <param name="sender">entityid of sender</param>
        public static void SendInvitedMessage(IPacketReceiver client, string chan, EntityId sender)
        {
        }

        /// <summary>Send the "you are not an owner" reply</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">name of channel</param>
        public static void SendNotOwnerReply(IPacketReceiver client, string chan)
        {
        }

        /// <summary>Send the "you are muted" reply</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">name of channel</param>
        public static void SendMutedReply(IPacketReceiver client, string chan)
        {
        }

        /// <summary>Send the name of current of the channel</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">the channel</param>
        public static void SendCurrentOwner(IPacketReceiver client, ChatChannel chan)
        {
        }

        /// <summary>Send the list of channel members</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        /// <param name="chan">channel to be listed</param>
        public static void SendChannelList(IPacketReceiver client, ChatChannel chan)
        {
        }

        /// <summary>Send the "name has joined channel" reply to everyone</summary>
        /// <param name="chan">name of channel</param>
        /// <param name="sender">sender (to check the ignore list)</param>
        public static void SendJoinedReplyToEveryone(ChatChannel chan, ChannelMember sender)
        {
        }

        /// <summary>Send the "name has left channel" reply to everyone</summary>
        /// <param name="chan">name of channel</param>
        /// <param name="sender">sender (to check the ignore list)</param>
        public static void SendLeftReplyToEveryone(ChatChannel chan, EntityId sender)
        {
        }

        /// <summary>Send owner changed message to everyone</summary>
        /// <param name="chan">name of channel</param>
        /// <param name="sender">sender (to check the ignore list)</param>
        /// <param name="newOwner">new owner</param>
        public static void SendOwnerChangedToEveryone(ChatChannel chan, EntityId sender, EntityId newOwner)
        {
        }

        /// <summary>Send the password changed message to everyone</summary>
        /// <param name="chan">name of channel</param>
        /// <param name="sender">sender (to check the ignore list)</param>
        public static void SendPasswordChangedToEveryone(ChatChannel chan, EntityId sender)
        {
        }

        /// <summary>Send the ban message to everyone</summary>
        /// <param name="chan">name of channel</param>
        /// <param name="sender">sender (aka banner)</param>
        /// <param name="banned">banned</param>
        public static void SendBannedToEveryone(ChatChannel chan, EntityId sender, EntityId banned)
        {
        }

        /// <summary>Send the kick message to everyone</summary>
        /// <param name="chan">name of channel</param>
        /// <param name="sender">sender (aka kicker)</param>
        /// <param name="kicked">kicked</param>
        public static void SendKickedToEveryone(ChatChannel chan, EntityId sender, EntityId kicked)
        {
        }

        /// <summary>Send the unbanned message to everyone</summary>
        /// <param name="chan">name of channel</param>
        /// <param name="sender">sender (aka unbanner)</param>
        /// <param name="unbanned">unbanned</param>
        public static void SendUnbannedToEveryone(ChatChannel chan, EntityId sender, EntityId unbanned)
        {
        }

        /// <summary>Send the moderate status change message to everyone</summary>
        /// <param name="chan">name of channel</param>
        /// <param name="sender">sender (status changer)</param>
        public static void SendModerateToEveryone(ChatChannel chan, EntityId sender)
        {
        }

        /// <summary>Send the announce status change message to everyone</summary>
        /// <param name="chan">name of channel</param>
        /// <param name="sender">sender (status changer)</param>
        /// <param name="newStatus">new announcements status</param>
        public static void SendAnnouncementToEveryone(ChatChannel chan, EntityId sender, bool newStatus)
        {
        }

        /// <summary>
        /// Send the message about change of moderator status of player to everyone
        /// </summary>
        /// <param name="chan">name of channel</param>
        /// <param name="sender">sender (to check the ignore list)</param>
        /// <param name="target">one who has changed his status</param>
        public static void SendModeratorStatusToEveryone(ChatChannel chan, EntityId sender, EntityId target,
            bool newStatus)
        {
        }

        /// <summary>
        /// Send the message about change of muted status of player to everyone
        /// </summary>
        /// <param name="chan">name of channel</param>
        /// <param name="sender">sender (to check the ignore list)</param>
        /// <param name="target">one who has changed his status</param>
        /// <param name="newStatus">the new status</param>
        public static void SendMuteStatusToEveryone(ChatChannel chan, EntityId sender, EntityId target, bool newStatus)
        {
        }

        /// <summary>Send a packet to everyone on a channel.</summary>
        /// <param name="chan">the channel to which the packet is sent</param>
        /// <param name="packet">the packet to send</param>
        public static void SendPacketToChannel(ChatChannel chan, RealmPacketOut packet)
        {
            foreach (ChannelMember channelMember in (IEnumerable<ChannelMember>) chan.Members.Values)
                channelMember.User.Send(packet, false);
        }

        /// <summary>
        /// Send a packet to everyone on a channel that does not ignore the given sender.
        /// </summary>
        /// <param name="chan">the channel to which the packet is sent</param>
        /// <param name="packet">the packet to send</param>
        /// <param name="sender">sender (to check the ignore list)</param>
        public static void SendPacketToChannel(ChatChannel chan, RealmPacketOut packet, EntityId sender)
        {
            foreach (ChannelMember channelMember in (IEnumerable<ChannelMember>) chan.Members.Values)
            {
                if (!Singleton<RelationMgr>.Instance.HasRelation(channelMember.User.EntityId.Low, sender.Low,
                    CharacterRelationType.Ignored))
                    channelMember.User.Send(packet, false);
            }
        }

        /// <summary>Send a reply to the number of members request</summary>
        /// <param name="client">the client the outdoing packet belongs to</param>
        public static void SendMemberCountReply(IPacketReceiver client, string channelName, byte channelFlags,
            uint memberCount)
        {
        }
    }
}