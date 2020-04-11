using WCell.Constants;
using WCell.RealmServer.Lang;

namespace WCell.RealmServer.Gossips
{
  public class MultiStringGossipMenuItem : GossipMenuItemBase
  {
    public readonly string[] Texts = new string[8];

    /// <summary>
    /// If set, will show an Accept/Cancel dialog with this text to the player
    /// when selecting this Item.
    /// </summary>
    public string[] ConfirmTexts = new string[8];

    public MultiStringGossipMenuItem()
    {
    }

    public MultiStringGossipMenuItem(GossipMenuIcon type, string[] texts)
    {
      Icon = type;
      Texts = texts;
    }

    public MultiStringGossipMenuItem(string[] texts)
      : this(GossipMenuIcon.Talk, texts)
    {
    }

    public MultiStringGossipMenuItem(string[] texts, IGossipAction action)
      : this(texts)
    {
      Action = action;
    }

    public MultiStringGossipMenuItem(string[] texts, GossipActionHandler callback)
      : this(texts)
    {
      Action = new NonNavigatingGossipAction(callback);
    }

    public MultiStringGossipMenuItem(string[] texts, GossipActionHandler callback, string[] confirmTexts)
      : this(texts)
    {
      ConfirmTexts = confirmTexts;
      Action = new NonNavigatingGossipAction(callback);
    }

    public MultiStringGossipMenuItem(string[] texts, GossipActionHandler callback,
      params MultiStringGossipMenuItem[] items)
      : this(texts)
    {
      Action = new NonNavigatingGossipAction(callback);
      SubMenu = new GossipMenu(items);
    }

    public MultiStringGossipMenuItem(string[] texts, GossipMenu subMenu)
      : this(texts, (IGossipAction) null, subMenu)
    {
    }

    public MultiStringGossipMenuItem(string[] texts, GossipActionHandler callback, GossipMenu subMenu)
      : this(texts)
    {
      Action = new NonNavigatingGossipAction(callback);
      SubMenu = subMenu;
    }

    public MultiStringGossipMenuItem(string[] texts, IGossipAction action, GossipMenu subMenu)
      : this(texts)
    {
      Action = action;
      SubMenu = subMenu;
    }

    public MultiStringGossipMenuItem(string[] texts, params MultiStringGossipMenuItem[] items)
      : this(texts)
    {
      SubMenu = new GossipMenu(items);
    }

    public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, params MultiStringGossipMenuItem[] items)
      : this(texts)
    {
      Icon = icon;
      SubMenu = new GossipMenu(items);
    }

    public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, GossipMenu subMenu)
      : this(texts)
    {
      Icon = icon;
      SubMenu = subMenu;
    }

    public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, IGossipAction action)
      : this(texts)
    {
      Icon = icon;
      Action = action;
    }

    public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, IGossipAction action, GossipMenu subMenu)
      : this(texts)
    {
      Icon = icon;
      SubMenu = subMenu;
      Action = action;
    }

    public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, GossipActionHandler callback)
      : this(texts)
    {
      Icon = icon;
      Action = new NonNavigatingGossipAction(callback);
    }

    public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, GossipActionHandler callback,
      GossipMenu subMenu)
      : this(texts)
    {
      Icon = icon;
      Action = new NonNavigatingGossipAction(callback);
      SubMenu = subMenu;
    }

    public string DefaultText
    {
      get { return Texts.LocalizeWithDefaultLocale(); }
      set { Texts[(int) RealmServerConfiguration.DefaultLocale] = value; }
    }

    public string DefaultConfirmText
    {
      get { return ConfirmTexts.LocalizeWithDefaultLocale(); }
      set { ConfirmTexts[(int) RealmServerConfiguration.DefaultLocale] = value; }
    }

    public override string GetText(GossipConversation convo)
    {
      return Texts.Localize(convo.User.Locale);
    }

    public override string GetConfirmText(GossipConversation convo)
    {
      return ConfirmTexts.Localize(convo.User.Locale);
    }
  }
}