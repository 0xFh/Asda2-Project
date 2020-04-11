using System;
using System.Collections.Generic;
using WCell.Constants.Chat;
using WCell.Constants.Factions;
using WCell.Core.Initialization;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Chat
{
    /// <summary>Manager for chat channels of one Faction.</summary>
    public class ChatChannelGroup
    {
        public static readonly Dictionary<uint, ChatChannelFlagsEntry> DefaultChannelFlags =
            new Dictionary<uint, ChatChannelFlagsEntry>();

        public static readonly ChatChannelFlags
            GeneralFlags = ChatChannelFlags.AutoJoin | ChatChannelFlags.ZoneSpecific;

        public static readonly ChatChannelFlags TradeFlags = ChatChannelFlags.Trade | ChatChannelFlags.CityOnly;
        public static readonly ChatChannelFlags LFGFlags = ChatChannelFlags.LookingForGroup;

        public static readonly ChatChannelFlags LocalDefenseFlags =
            ChatChannelFlags.AutoJoin | ChatChannelFlags.CityOnly | ChatChannelFlags.Defense;

        /// <summary>Channel manager for the Alliance channels.</summary>
        public static readonly ChatChannelGroup Alliance = new ChatChannelGroup(FactionGroup.Alliance);

        /// <summary>Channel manager for the Horde channels.</summary>
        public static readonly ChatChannelGroup Horde = new ChatChannelGroup(FactionGroup.Horde);

        /// <summary>Channel manager for Global channels.</summary>
        public static readonly ChatChannelGroup Global = new ChatChannelGroup(FactionGroup.Invalid);

        /// <summary>
        /// All based Channels that exist per Zone, indexed by <see cref="T:WCell.Constants.World.ZoneId" />
        /// </summary>
        public readonly List<ChatChannel>[] ZoneChannels;

        public readonly FactionGroup FactionGroup;
        private Dictionary<string, ChatChannel> m_Channels;
        private ChatChannel m_tradeChannel;
        private ChatChannel m_lfgChannel;

        /// <summary>
        /// Initializes all the default zone channels. (general, local defense, etc)
        /// </summary>
        [WCell.Core.Initialization.Initialization(InitializationPass.Fifth, "Create default channels")]
        public static void InitializeDefaultChannels()
        {
            World.InitializeWorld();
            ChatChannelGroup.Alliance.TradeChannel = new ChatChannel(ChatChannelGroup.Alliance, "Trade - City",
                ChatChannelGroup.TradeFlags, true);
            ChatChannelGroup.Alliance.LFGChannel = new ChatChannel(ChatChannelGroup.Alliance, "LookingForGroup",
                ChatChannelGroup.LFGFlags, true);
            ChatChannelGroup.Horde.TradeChannel = new ChatChannel(ChatChannelGroup.Horde, "Trade - City",
                ChatChannelGroup.TradeFlags, true);
            ChatChannelGroup.Horde.LFGChannel = new ChatChannel(ChatChannelGroup.Horde, "LookingForGroup",
                ChatChannelGroup.LFGFlags, true);
        }

        /// <summary>Default constructor</summary>
        public ChatChannelGroup(FactionGroup factionGroup)
        {
            this.FactionGroup = factionGroup;
            this.ZoneChannels = new List<ChatChannel>[5023];
            this.Channels =
                new Dictionary<string, ChatChannel>(
                    (IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
        }

        public ChatChannel TradeChannel
        {
            get { return this.m_tradeChannel; }
            private set
            {
                this.m_tradeChannel = value;
                this.m_Channels.Add(this.m_tradeChannel.Name, this.m_tradeChannel);
            }
        }

        public ChatChannel LFGChannel
        {
            get { return this.m_lfgChannel; }
            private set
            {
                this.m_lfgChannel = value;
                this.m_Channels.Add(this.m_lfgChannel.Name, this.m_lfgChannel);
            }
        }

        /// <summary>The channels for this manager;</summary>
        public Dictionary<string, ChatChannel> Channels
        {
            get { return this.m_Channels; }
            set { this.m_Channels = value; }
        }

        /// <summary>
        /// Creates a zone channel, which is a constant, non-moderated channel specific to one or more Zones.
        /// TODO: Fix: Channels exist per Zone instance
        /// </summary>
        internal ChatChannel CreateGeneralChannel(ZoneTemplate zone)
        {
            string str = string.Format("General - {0}", (object) zone.Name);
            ChatChannel chatChannel;
            if (!this.m_Channels.TryGetValue(str, out chatChannel))
            {
                chatChannel = new ChatChannel(this, str, ChatChannelGroup.GeneralFlags, true,
                    (ChatChannel.JoinValidationHandler) null)
                {
                    Announces = false
                };
                this.m_Channels.Add(chatChannel.Name, chatChannel);
            }

            return chatChannel;
        }

        /// <summary>
        /// Creates a zone channel, which is a constant, non-moderated channel specific to one or more Zones.
        /// </summary>
        internal ChatChannel CreateLocalDefenseChannel(ZoneTemplate zone)
        {
            string str = string.Format("LocalDefense - {0}", (object) zone.Name);
            ChatChannel chatChannel;
            if (!this.m_Channels.TryGetValue(str, out chatChannel))
            {
                chatChannel = new ChatChannel(this, str, ChatChannelGroup.LocalDefenseFlags, true,
                    (ChatChannel.JoinValidationHandler) null)
                {
                    Announces = false
                };
                this.m_Channels.Add(chatChannel.Name, chatChannel);
            }

            return chatChannel;
        }

        /// <summary>Deletes a channel.</summary>
        /// <param name="chnl">the channel to delete</param>
        public void DeleteChannel(ChatChannel chnl)
        {
            this.m_Channels.Remove(chnl.Name);
        }

        /// <summary>Attempts to retrieve a specific channel.</summary>
        /// <param name="name">the name of the channel</param>
        /// <param name="create">whether or not to create the channel if it doesn't exist</param>
        /// <returns>the channel instance</returns>
        public ChatChannel GetChannel(string name, bool create)
        {
            if (this.Channels.ContainsKey(name))
                return this.Channels[name];
            if (!create)
                return (ChatChannel) null;
            ChatChannel chatChannel = new ChatChannel(this, name);
            this.m_Channels.Add(name, chatChannel);
            return chatChannel;
        }

        public ChatChannel GetChannel(string name, uint channelId, bool create)
        {
            ChatChannel chatChannel;
            if (!this.m_Channels.TryGetValue(name, out chatChannel) && create)
            {
                chatChannel = new ChatChannel(this, channelId, name);
                this.m_Channels.Add(name, chatChannel);
            }

            return chatChannel;
        }

        public bool CanJoin(IUser user)
        {
            if (this.FactionGroup != FactionGroup.Invalid && user.FactionGroup != this.FactionGroup)
                return user.Role.IsStaff;
            return true;
        }

        /// <summary>
        /// Gets the appropriate <see cref="T:WCell.RealmServer.Chat.ChatChannelGroup" /> for the given <see cref="F:WCell.RealmServer.Chat.ChatChannelGroup.FactionGroup" />.
        /// </summary>
        /// <param name="faction">the faction</param>
        /// <returns>the appropriate channel manager, or null if an invalid faction is given</returns>
        public static ChatChannelGroup GetGroup(FactionGroup faction)
        {
            if (faction == FactionGroup.Alliance)
                return ChatChannelGroup.Alliance;
            if (faction == FactionGroup.Horde)
                return ChatChannelGroup.Horde;
            throw new Exception("Invalid FactionGroup: " + (object) faction);
        }

        /// <summary>Tries to retrieve a channel for the given character.</summary>
        /// <param name="channelName">the channel name</param>
        /// <returns>the requested channel; null if there was an error or it doesn't exist</returns>
        public static ChatChannel RetrieveChannel(IUser user, string channelName)
        {
            if (channelName == string.Empty)
                return (ChatChannel) null;
            ChatChannel channel = ChatChannelGroup.Global.GetChannel(channelName, false);
            if (channel == null)
            {
                ChatChannelGroup group = ChatChannelGroup.GetGroup(user.FactionGroup);
                if (group == null)
                    return (ChatChannel) null;
                channel = group.GetChannel(channelName, false);
            }

            return channel;
        }
    }
}