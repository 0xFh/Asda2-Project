using System;
using WCell.Constants;

namespace WCell.RealmServer.Gossips
{
  [Serializable]
  public class GossipMenuItem : GossipMenuItemBase
  {
    public string Text;

    /// <summary>
    /// If set, will show an Accept/Cancel dialog with this text to the player
    /// when selecting this Item.
    /// </summary>
    public string ConfirmText;

    private GossipMenuItem()
    {
      Text = string.Empty;
      ConfirmText = string.Empty;
    }

    public GossipMenuItem(GossipMenuIcon type, string text)
    {
      Icon = type;
      Text = text;
      ConfirmText = string.Empty;
    }

    public GossipMenuItem(string text)
      : this(GossipMenuIcon.Talk, text)
    {
    }

    public GossipMenuItem(string text, IGossipAction action)
      : this(text)
    {
      Action = action;
    }

    public GossipMenuItem(string text, GossipActionHandler callback)
      : this(text)
    {
      Action = new NonNavigatingGossipAction(callback);
    }

    public GossipMenuItem(string text, GossipActionHandler callback, string confirmText)
      : this(text)
    {
      ConfirmText = confirmText;
      Action = new NonNavigatingGossipAction(callback);
    }

    public GossipMenuItem(string text, GossipActionHandler callback, params GossipMenuItem[] items)
      : this(text)
    {
      Action = new NonNavigatingGossipAction(callback);
      SubMenu = new GossipMenu(items);
    }

    public GossipMenuItem(string text, GossipMenu subMenu)
      : this(text, (IGossipAction) null, subMenu)
    {
    }

    public GossipMenuItem(string text, GossipActionHandler callback, GossipMenu subMenu)
      : this(text)
    {
      Action = new NonNavigatingGossipAction(callback);
      SubMenu = subMenu;
    }

    public GossipMenuItem(string text, IGossipAction action, GossipMenu subMenu)
      : this(text)
    {
      Action = action;
      SubMenu = subMenu;
    }

    public GossipMenuItem(string text, params GossipMenuItem[] items)
      : this(text)
    {
      SubMenu = new GossipMenu(items);
    }

    public GossipMenuItem(GossipMenuIcon icon, string text, params GossipMenuItem[] items)
      : this(text)
    {
      Icon = icon;
      SubMenu = new GossipMenu(items);
    }

    public GossipMenuItem(GossipMenuIcon icon, string text, IGossipAction action)
      : this(text)
    {
      Icon = icon;
      Action = action;
    }

    public GossipMenuItem(GossipMenuIcon icon, string text, GossipActionHandler callback)
      : this(text)
    {
      Icon = icon;
      Action = new NonNavigatingGossipAction(callback);
    }

    public override string GetText(GossipConversation convo)
    {
      return Text;
    }

    public override string GetConfirmText(GossipConversation convo)
    {
      return ConfirmText;
    }
  }
}