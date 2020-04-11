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
    public readonly IList<ChatChannel> AllianceChatChannels = new List<ChatChannel>();
    public readonly IList<ChatChannel> HordeChatChannels = new List<ChatChannel>();
    private ChatChannel m_allianceGeneralChannel;
    private ChatChannel m_allianceLocalDefenseChannel;
    private ChatChannel m_hordeGeneralChannel;
    private ChatChannel m_hordeLocalDefenseChannel;
    public readonly ZoneTemplate Template;
    public readonly Map Map;

    public Zone(Map rgn, ZoneTemplate template)
    {
      Map = rgn;
      Template = template;
      if(template.WorldStates != null)
        WorldStates = new WorldStateCollection(this, template.WorldStates);
      CreateChatChannels();
    }

    public WorldStateCollection WorldStates { get; private set; }

    public IWorldSpace ParentSpace
    {
      get { return ParentZone ?? (IWorldSpace) Map; }
    }

    public int ExplorationBit
    {
      get { return Template.ExplorationBit; }
    }

    public int AreaLevel
    {
      get { return Template.AreaLevel; }
    }

    /// <summary>The name of the zone.</summary>
    public string Name
    {
      get { return Template.Name; }
    }

    /// <summary>The ID of the zone.</summary>
    public ZoneId Id
    {
      get { return Template.Id; }
    }

    /// <summary>The ID of the zone's parent zone.</summary>
    public ZoneId ParentZoneId
    {
      get { return Template.ParentZoneId; }
    }

    public Zone ParentZone
    {
      get { return Map.GetZone(ParentZoneId); }
    }

    /// <summary>The ID of this zone's parent map.</summary>
    public MapId MapId
    {
      get { return Template.MapId; }
    }

    /// <summary>
    /// The <see cref="P:WCell.RealmServer.Global.Zone.MapTemplate">Map</see> to which this Zone belongs.
    /// </summary>
    public MapTemplate MapTemplate
    {
      get { return Template.MapTemplate; }
      set { Template.MapTemplate = value; }
    }

    /// <summary>The flags for the zone.</summary>
    public ZoneFlags Flags
    {
      get { return Template.Flags; }
    }

    /// <summary>Who does this Zone belong to (if anyone)</summary>
    public FactionGroupMask Ownership
    {
      get { return Template.Ownership; }
    }

    public void CallOnAllCharacters(Action<Character> action)
    {
      Map.CallOnAllCharacters(chr =>
      {
        if(chr.Zone.Id != Id)
          return;
        action(chr);
      });
    }

    internal void EnterZone(Character chr, Zone oldZone)
    {
      if(oldZone != null)
        oldZone.LeaveZone(chr);
      Template.OnPlayerEntered(chr, oldZone);
    }

    internal void LeaveZone(Character chr)
    {
      Template.OnPlayerLeft(chr, this);
    }

    public ChatChannel AllianceLocalDefenseChannel
    {
      get { return m_allianceLocalDefenseChannel; }
    }

    public ChatChannel AllianceGeneralChannel
    {
      get { return m_allianceGeneralChannel; }
    }

    public ChatChannel HordeLocalDefenseChannel
    {
      get { return m_hordeLocalDefenseChannel; }
    }

    public ChatChannel HordeGeneralChannel
    {
      get { return m_hordeGeneralChannel; }
    }

    public IList<ChatChannel> GetChatChannels(FactionGroup group)
    {
      if(group != FactionGroup.Alliance)
        return HordeChatChannels;
      return AllianceChatChannels;
    }

    private void CreateChatChannels()
    {
      ChatChannelGroup alliance = ChatChannelGroup.Alliance;
      ChatChannelGroup horde = ChatChannelGroup.Horde;
      if(Template.Flags.HasFlag(ZoneFlags.Arena))
        return;
      AllianceChatChannels.Add(m_allianceLocalDefenseChannel =
        alliance.CreateLocalDefenseChannel(Template));
      HordeChatChannels.Add(m_hordeLocalDefenseChannel =
        horde.CreateLocalDefenseChannel(Template));
      AllianceChatChannels.Add(m_allianceGeneralChannel = alliance.CreateGeneralChannel(Template));
      HordeChatChannels.Add(m_hordeGeneralChannel = horde.CreateGeneralChannel(Template));
    }

    /// <summary>
    /// Lets the player join/leave the appropriate chat-channels
    /// </summary>
    /// <param name="chr">the player</param>
    private void UpdateChannels(Character chr, Zone oldZone)
    {
      IList<ChatChannel> chatChannels1 = GetChatChannels(chr.FactionGroup);
      if(oldZone != null)
      {
        IList<ChatChannel> chatChannels2 = oldZone.GetChatChannels(chr.FactionGroup);
        if(oldZone.Template.IsCity)
          ChatChannelGroup.GetGroup(chr.FactionGroup).TradeChannel.Leave(chr, false);
        foreach(ChatChannel chatChannel in chatChannels2)
        {
          if(!chatChannels1.Contains(chatChannel))
            chatChannel.Leave(chr, false);
        }

        foreach(ChatChannel chatChannel in chatChannels1)
        {
          if(!chatChannels2.Contains(chatChannel))
            chatChannel.TryJoin(chr);
        }
      }
      else
      {
        foreach(ChatChannel chatChannel in chatChannels1)
          chatChannel.TryJoin(chr);
      }

      if(!Template.IsCity)
        return;
      ChatChannelGroup.GetGroup(chr.FactionGroup).TradeChannel.TryJoin(chr);
    }

    public override string ToString()
    {
      return Name + " (Id: " + Id + ")";
    }
  }
}