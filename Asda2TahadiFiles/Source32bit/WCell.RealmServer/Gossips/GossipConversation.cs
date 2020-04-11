using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Quests;

namespace WCell.RealmServer.Gossips
{
  /// <summary>
  /// Represents specific gossip conversation between character and an object
  /// </summary>
  public class GossipConversation
  {
    /// <summary>Creates gossip conversation by its fields</summary>
    /// <param name="menu">starting menu</param>
    /// <param name="chr">character which started the conversation</param>
    /// <param name="speaker">respondent</param>
    public GossipConversation(GossipMenu menu, Character chr, WorldObject speaker)
      : this(menu, chr, speaker, menu.KeepOpen)
    {
    }

    /// <summary>Creates gossip conversation by its fields</summary>
    /// <param name="menu">starting menu</param>
    /// <param name="chr">character which started the conversation</param>
    /// <param name="speaker">respondent</param>
    public GossipConversation(GossipMenu menu, Character chr, WorldObject speaker, bool keepOpen)
    {
      CurrentMenu = menu;
      Character = chr;
      Speaker = speaker;
      StayOpen = keepOpen;
    }

    /// <summary>Current menu</summary>
    public GossipMenu CurrentMenu { get; protected internal set; }

    public IUser User
    {
      get { return Character; }
    }

    /// <summary>Character who initiated the conversation</summary>
    public Character Character { get; protected set; }

    /// <summary>
    /// The speaker that the Character is talking to (usually an NPC)
    /// </summary>
    public WorldObject Speaker { get; protected set; }

    /// <summary>
    /// If set to true, will always keep the menu open until
    /// (preferrable some Option) set this to false or the client cancelled it.
    /// </summary>
    public bool StayOpen { get; set; }

    /// <summary>Shows current menu</summary>
    public void DisplayCurrentMenu()
    {
      DisplayMenu(CurrentMenu);
    }

    /// <summary>Handles selection of item in menu by player</summary>
    /// <param name="itemID">ID of selected item</param>
    /// <param name="extra">additional parameter supplied by user</param>
    public void HandleSelectedItem(uint itemID, string extra)
    {
      IList<GossipMenuItemBase> gossipItems = CurrentMenu.GossipItems;
      if(itemID >= gossipItems.Count)
        return;
      GossipMenuItemBase gossipMenuItemBase = gossipItems[(int) itemID];
      if(gossipMenuItemBase == null)
        return;
      if(gossipMenuItemBase.Action != null && gossipMenuItemBase.Action.CanUse(this))
      {
        GossipMenu currentMenu = CurrentMenu;
        gossipMenuItemBase.Action.OnSelect(this);
        if(currentMenu != CurrentMenu || gossipMenuItemBase.Action.Navigates)
          return;
      }

      if(gossipMenuItemBase.SubMenu != null)
        DisplayMenu(gossipMenuItemBase.SubMenu);
      else if(StayOpen)
      {
        DisplayCurrentMenu();
      }
      else
      {
        CurrentMenu.NotifyClose(this);
        Dispose();
      }
    }

    /// <summary>Shows menu to player</summary>
    /// <param name="menu">menu to show</param>
    public void DisplayMenu(GossipMenu menu)
    {
      CurrentMenu = menu;
      menu.OnDisplay(this);
      if(Speaker is IQuestHolder && ((IQuestHolder) Speaker).QuestHolderInfo != null)
        GossipHandler.SendPageToCharacter(this,
          ((IQuestHolder) Speaker).QuestHolderInfo.GetQuestMenuItems(
            Character));
      else
        GossipHandler.SendPageToCharacter(this, null);
    }

    public void Dispose()
    {
      GossipHandler.SendConversationComplete(Character);
      if(Character.GossipConversation != this)
        return;
      Character.GossipConversation = null;
      CurrentMenu = null;
      Speaker = null;
    }

    /// <summary>Cancels the current conversation</summary>
    public void Cancel()
    {
      GossipHandler.SendConversationComplete(Character);
      Dispose();
    }

    /// <summary>Closes any open Client menu and sends the CurrentMenu</summary>
    public void Invalidate()
    {
      GossipHandler.SendConversationComplete(Character);
      DisplayCurrentMenu();
    }

    public void GoBack()
    {
      if(CurrentMenu.ParentMenu != null)
        DisplayMenu(CurrentMenu.ParentMenu);
      else
        DisplayCurrentMenu();
    }
  }
}