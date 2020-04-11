using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.World;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Global
{
    public class Zone : IWorldSpace
    {
        public readonly IList<ChatChannel> AllianceChatChannels = (IList<ChatChannel>) new List<ChatChannel>();
        public readonly IList<ChatChannel> HordeChatChannels = (IList<ChatChannel>) new List<ChatChannel>();
        private ChatChannel m_allianceGeneralChannel;
        private ChatChannel m_allianceLocalDefenseChannel;
        private ChatChannel m_hordeGeneralChannel;
        private ChatChannel m_hordeLocalDefenseChannel;
        public readonly ZoneTemplate Template;
        public readonly Map Map;

        public Zone(Map rgn, ZoneTemplate template)
        {
            this.Map = rgn;
            this.Template = template;
            if (template.WorldStates != null)
                this.WorldStates = new WorldStateCollection((IWorldSpace) this, template.WorldStates);
            this.CreateChatChannels();
        }

        public WorldStateCollection WorldStates { get; private set; }

        public IWorldSpace ParentSpace
        {
            get { return (IWorldSpace) this.ParentZone ?? (IWorldSpace) this.Map; }
        }

        public int ExplorationBit
        {
            get { return this.Template.ExplorationBit; }
        }

        public int AreaLevel
        {
            get { return this.Template.AreaLevel; }
        }

        /// <summary>The name of the zone.</summary>
        public string Name
        {
            get { return this.Template.Name; }
        }

        /// <summary>The ID of the zone.</summary>
        public ZoneId Id
        {
            get { return this.Template.Id; }
        }

        /// <summary>The ID of the zone's parent zone.</summary>
        public ZoneId ParentZoneId
        {
            get { return this.Template.ParentZoneId; }
        }

        public Zone ParentZone
        {
            get { return this.Map.GetZone(this.ParentZoneId); }
        }

        /// <summary>The ID of this zone's parent map.</summary>
        public MapId MapId
        {
            get { return this.Template.MapId; }
        }

        /// <summary>
        /// The <see cref="P:WCell.RealmServer.Global.Zone.MapTemplate">Map</see> to which this Zone belongs.
        /// </summary>
        public MapTemplate MapTemplate
        {
            get { return this.Template.MapTemplate; }
            set { this.Template.MapTemplate = value; }
        }

        /// <summary>The flags for the zone.</summary>
        public ZoneFlags Flags
        {
            get { return this.Template.Flags; }
        }

        /// <summary>Who does this Zone belong to (if anyone)</summary>
        public FactionGroupMask Ownership
        {
            get { return this.Template.Ownership; }
        }

        public void CallOnAllCharacters(Action<Character> action)
        {
            this.Map.CallOnAllCharacters((Action<Character>) (chr =>
            {
                if (chr.Zone.Id != this.Id)
                    return;
                action(chr);
            }));
        }

        internal void EnterZone(Character chr, Zone oldZone)
        {
            if (oldZone != null)
                oldZone.LeaveZone(chr);
            this.Template.OnPlayerEntered(chr, oldZone);
        }

        internal void LeaveZone(Character chr)
        {
            this.Template.OnPlayerLeft(chr, this);
        }

        public ChatChannel AllianceLocalDefenseChannel
        {
            get { return this.m_allianceLocalDefenseChannel; }
        }

        public ChatChannel AllianceGeneralChannel
        {
            get { return this.m_allianceGeneralChannel; }
        }

        public ChatChannel HordeLocalDefenseChannel
        {
            get { return this.m_hordeLocalDefenseChannel; }
        }

        public ChatChannel HordeGeneralChannel
        {
            get { return this.m_hordeGeneralChannel; }
        }

        public IList<ChatChannel> GetChatChannels(FactionGroup group)
        {
            if (group != FactionGroup.Alliance)
                return this.HordeChatChannels;
            return this.AllianceChatChannels;
        }

        private void CreateChatChannels()
        {
            ChatChannelGroup alliance = ChatChannelGroup.Alliance;
            ChatChannelGroup horde = ChatChannelGroup.Horde;
            if (this.Template.Flags.HasFlag((Enum) ZoneFlags.Arena))
                return;
            this.AllianceChatChannels.Add(this.m_allianceLocalDefenseChannel =
                alliance.CreateLocalDefenseChannel(this.Template));
            this.HordeChatChannels.Add(this.m_hordeLocalDefenseChannel =
                horde.CreateLocalDefenseChannel(this.Template));
            this.AllianceChatChannels.Add(this.m_allianceGeneralChannel = alliance.CreateGeneralChannel(this.Template));
            this.HordeChatChannels.Add(this.m_hordeGeneralChannel = horde.CreateGeneralChannel(this.Template));
        }

        /// <summary>
        /// Lets the player join/leave the appropriate chat-channels
        /// </summary>
        /// <param name="chr">the player</param>
        private void UpdateChannels(Character chr, Zone oldZone)
        {
            IList<ChatChannel> chatChannels1 = this.GetChatChannels(chr.FactionGroup);
            if (oldZone != null)
            {
                IList<ChatChannel> chatChannels2 = oldZone.GetChatChannels(chr.FactionGroup);
                if (oldZone.Template.IsCity)
                    ChatChannelGroup.GetGroup(chr.FactionGroup).TradeChannel.Leave((IUser) chr, false);
                foreach (ChatChannel chatChannel in (IEnumerable<ChatChannel>) chatChannels2)
                {
                    if (!chatChannels1.Contains(chatChannel))
                        chatChannel.Leave((IUser) chr, false);
                }

                foreach (ChatChannel chatChannel in (IEnumerable<ChatChannel>) chatChannels1)
                {
                    if (!chatChannels2.Contains(chatChannel))
                        chatChannel.TryJoin((IUser) chr);
                }
            }
            else
            {
                foreach (ChatChannel chatChannel in (IEnumerable<ChatChannel>) chatChannels1)
                    chatChannel.TryJoin((IUser) chr);
            }

            if (!this.Template.IsCity)
                return;
            ChatChannelGroup.GetGroup(chr.FactionGroup).TradeChannel.TryJoin((IUser) chr);
        }

        public override string ToString()
        {
            return this.Name + " (Id: " + (object) this.Id + ")";
        }
    }
}