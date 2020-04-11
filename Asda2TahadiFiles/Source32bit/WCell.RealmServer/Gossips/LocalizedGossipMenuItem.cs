using WCell.Constants;
using WCell.RealmServer.Lang;

namespace WCell.RealmServer.Gossips
{
  public class LocalizedGossipMenuItem : GossipMenuItemBase
  {
    public readonly TranslatableItem Text;

    /// <summary>
    /// If set, will show an Accept/Cancel dialog with this text to the player
    /// when selecting this Item.
    /// </summary>
    public TranslatableItem ConfirmText;

    public LocalizedGossipMenuItem()
    {
    }

    public LocalizedGossipMenuItem(GossipMenuIcon type, TranslatableItem text)
    {
      Icon = type;
      Text = text;
    }

    public LocalizedGossipMenuItem(TranslatableItem text)
      : this(GossipMenuIcon.Talk, text)
    {
    }

    public LocalizedGossipMenuItem(RealmLangKey msgKey)
    {
      Text = new TranslatableItem(msgKey);
    }

    public LocalizedGossipMenuItem(TranslatableItem text, IGossipAction action)
      : this(text)
    {
      Action = action;
    }

    public LocalizedGossipMenuItem(TranslatableItem text, GossipActionHandler callback)
      : this(text)
    {
      Action = new NonNavigatingGossipAction(callback);
    }

    public LocalizedGossipMenuItem(TranslatableItem text, GossipActionHandler callback,
      TranslatableItem confirmText)
      : this(text)
    {
      ConfirmText = confirmText;
      Action = new NonNavigatingGossipAction(callback);
    }

    public LocalizedGossipMenuItem(TranslatableItem text, GossipActionHandler callback,
      params LocalizedGossipMenuItem[] items)
      : this(text)
    {
      Action = new NonNavigatingGossipAction(callback);
      SubMenu = new GossipMenu(items);
    }

    public LocalizedGossipMenuItem(TranslatableItem text, GossipMenu subMenu)
      : this(text, (IGossipAction) null, subMenu)
    {
    }

    public LocalizedGossipMenuItem(TranslatableItem text, GossipActionHandler callback, GossipMenu subMenu)
      : this(text)
    {
      Action = new NonNavigatingGossipAction(callback);
      SubMenu = subMenu;
    }

    public LocalizedGossipMenuItem(TranslatableItem text, IGossipAction action, GossipMenu subMenu)
      : this(text)
    {
      Action = action;
      SubMenu = subMenu;
    }

    public LocalizedGossipMenuItem(TranslatableItem text, params LocalizedGossipMenuItem[] items)
      : this(text)
    {
      SubMenu = new GossipMenu(items);
    }

    public LocalizedGossipMenuItem(GossipMenuIcon icon, TranslatableItem text,
      params LocalizedGossipMenuItem[] items)
      : this(text)
    {
      Icon = icon;
      SubMenu = new GossipMenu(items);
    }

    public LocalizedGossipMenuItem(GossipMenuIcon icon, TranslatableItem text, IGossipAction action)
      : this(text)
    {
      Icon = icon;
      Action = action;
    }

    public LocalizedGossipMenuItem(GossipMenuIcon icon, TranslatableItem text, GossipActionHandler callback)
      : this(text)
    {
      Icon = icon;
      Action = new NonNavigatingGossipAction(callback);
    }

    public LocalizedGossipMenuItem(GossipMenuIcon icon, RealmLangKey msgKey, params object[] msgArgs)
      : this(msgKey, msgArgs)
    {
      Icon = icon;
    }

    public LocalizedGossipMenuItem(RealmLangKey msgKey, params object[] msgArgs)
      : this(GossipMenuIcon.Talk, new TranslatableItem(msgKey, msgArgs))
    {
    }

    public LocalizedGossipMenuItem(IGossipAction action, RealmLangKey msgKey, params object[] msgArgs)
      : this(msgKey, msgArgs)
    {
      Action = action;
    }

    public LocalizedGossipMenuItem(IGossipAction action, RealmLangKey confirmLangKey, RealmLangKey msgKey,
      params object[] msgArgs)
      : this(action, msgKey, msgArgs)
    {
      ConfirmText = new TranslatableItem(confirmLangKey);
    }

    public LocalizedGossipMenuItem(GossipActionHandler callback, RealmLangKey msgKey, params object[] msgArgs)
      : this(msgKey, msgArgs)
    {
      Action = new NonNavigatingGossipAction(callback);
    }

    public LocalizedGossipMenuItem(GossipActionHandler callback, RealmLangKey confirmLangKey, RealmLangKey msgKey,
      params object[] msgArgs)
      : this(msgKey, msgArgs)
    {
      ConfirmText = new TranslatableItem(confirmLangKey);
      Action = new NonNavigatingGossipAction(callback);
    }

    public LocalizedGossipMenuItem(GossipActionHandler callback, GossipActionDecider decider, RealmLangKey msgKey,
      params object[] msgArgs)
      : this(msgKey, msgArgs)
    {
      Action = new NonNavigatingDecidingGossipAction(callback, decider);
    }

    public LocalizedGossipMenuItem(GossipActionHandler callback, GossipActionDecider decider,
      RealmLangKey confirmLangKey, RealmLangKey msgKey, params object[] msgArgs)
      : this(msgKey, msgArgs)
    {
      ConfirmText = new TranslatableItem(confirmLangKey);
      Action = new NonNavigatingDecidingGossipAction(callback, decider);
    }

    public LocalizedGossipMenuItem(GossipMenu subMenu, RealmLangKey msgKey, params object[] msgArgs)
      : this(msgKey, (object) msgArgs, null, (object) subMenu)
    {
    }

    public LocalizedGossipMenuItem(GossipActionHandler callback, GossipMenu subMenu, RealmLangKey msgKey,
      params object[] msgArgs)
      : this(msgKey, msgArgs)
    {
      Action = new NonNavigatingGossipAction(callback);
      SubMenu = subMenu;
    }

    public LocalizedGossipMenuItem(IGossipAction action, GossipMenu subMenu, RealmLangKey msgKey,
      params object[] msgArgs)
      : this(msgKey, msgArgs)
    {
      Action = action;
      SubMenu = subMenu;
    }

    public LocalizedGossipMenuItem(RealmLangKey msgKey, params LocalizedGossipMenuItem[] items)
      : this(msgKey)
    {
      SubMenu = new GossipMenu(items);
    }

    public LocalizedGossipMenuItem(GossipMenuIcon icon, RealmLangKey langKey,
      params LocalizedGossipMenuItem[] items)
      : this(langKey)
    {
      Icon = icon;
      SubMenu = new GossipMenu(items);
    }

    public LocalizedGossipMenuItem(GossipMenuIcon icon, IGossipAction action, RealmLangKey msgKey,
      params object[] msgArgs)
      : this(msgKey, msgArgs)
    {
      Icon = icon;
      Action = action;
    }

    public LocalizedGossipMenuItem(GossipMenuIcon icon, GossipActionHandler callback, RealmLangKey msgKey,
      params object[] msgArgs)
      : this(msgKey, msgArgs)
    {
      Icon = icon;
      Action = new NonNavigatingGossipAction(callback);
    }

    public string DefaultText
    {
      get { return Text.TranslateDefault(); }
    }

    public string DefaultConfirmText
    {
      get
      {
        if(ConfirmText != null)
          return ConfirmText.TranslateDefault();
        return "";
      }
    }

    public override string GetText(GossipConversation convo)
    {
      return Text.Translate(convo.User.Locale);
    }

    public override string GetConfirmText(GossipConversation convo)
    {
      if(ConfirmText != null)
        return ConfirmText.Translate(convo.User.Locale);
      return "";
    }
  }
}