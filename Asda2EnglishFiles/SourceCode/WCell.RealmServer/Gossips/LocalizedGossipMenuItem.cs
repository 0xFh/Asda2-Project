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
            this.Icon = type;
            this.Text = text;
        }

        public LocalizedGossipMenuItem(TranslatableItem text)
            : this(GossipMenuIcon.Talk, text)
        {
        }

        public LocalizedGossipMenuItem(RealmLangKey msgKey)
        {
            this.Text = new TranslatableItem(msgKey, new object[0]);
        }

        public LocalizedGossipMenuItem(TranslatableItem text, IGossipAction action)
            : this(text)
        {
            this.Action = action;
        }

        public LocalizedGossipMenuItem(TranslatableItem text, GossipActionHandler callback)
            : this(text)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
        }

        public LocalizedGossipMenuItem(TranslatableItem text, GossipActionHandler callback,
            TranslatableItem confirmText)
            : this(text)
        {
            this.ConfirmText = confirmText;
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
        }

        public LocalizedGossipMenuItem(TranslatableItem text, GossipActionHandler callback,
            params LocalizedGossipMenuItem[] items)
            : this(text)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
            this.SubMenu = new GossipMenu((GossipMenuItemBase[]) items);
        }

        public LocalizedGossipMenuItem(TranslatableItem text, GossipMenu subMenu)
            : this(text, (IGossipAction) null, subMenu)
        {
        }

        public LocalizedGossipMenuItem(TranslatableItem text, GossipActionHandler callback, GossipMenu subMenu)
            : this(text)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
            this.SubMenu = subMenu;
        }

        public LocalizedGossipMenuItem(TranslatableItem text, IGossipAction action, GossipMenu subMenu)
            : this(text)
        {
            this.Action = action;
            this.SubMenu = subMenu;
        }

        public LocalizedGossipMenuItem(TranslatableItem text, params LocalizedGossipMenuItem[] items)
            : this(text)
        {
            this.SubMenu = new GossipMenu((GossipMenuItemBase[]) items);
        }

        public LocalizedGossipMenuItem(GossipMenuIcon icon, TranslatableItem text,
            params LocalizedGossipMenuItem[] items)
            : this(text)
        {
            this.Icon = icon;
            this.SubMenu = new GossipMenu((GossipMenuItemBase[]) items);
        }

        public LocalizedGossipMenuItem(GossipMenuIcon icon, TranslatableItem text, IGossipAction action)
            : this(text)
        {
            this.Icon = icon;
            this.Action = action;
        }

        public LocalizedGossipMenuItem(GossipMenuIcon icon, TranslatableItem text, GossipActionHandler callback)
            : this(text)
        {
            this.Icon = icon;
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
        }

        public LocalizedGossipMenuItem(GossipMenuIcon icon, RealmLangKey msgKey, params object[] msgArgs)
            : this(msgKey, msgArgs)
        {
            this.Icon = icon;
        }

        public LocalizedGossipMenuItem(RealmLangKey msgKey, params object[] msgArgs)
            : this(GossipMenuIcon.Talk, new TranslatableItem(msgKey, msgArgs))
        {
        }

        public LocalizedGossipMenuItem(IGossipAction action, RealmLangKey msgKey, params object[] msgArgs)
            : this(msgKey, msgArgs)
        {
            this.Action = action;
        }

        public LocalizedGossipMenuItem(IGossipAction action, RealmLangKey confirmLangKey, RealmLangKey msgKey,
            params object[] msgArgs)
            : this(action, msgKey, msgArgs)
        {
            this.ConfirmText = new TranslatableItem(confirmLangKey, new object[0]);
        }

        public LocalizedGossipMenuItem(GossipActionHandler callback, RealmLangKey msgKey, params object[] msgArgs)
            : this(msgKey, msgArgs)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
        }

        public LocalizedGossipMenuItem(GossipActionHandler callback, RealmLangKey confirmLangKey, RealmLangKey msgKey,
            params object[] msgArgs)
            : this(msgKey, msgArgs)
        {
            this.ConfirmText = new TranslatableItem(confirmLangKey, new object[0]);
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
        }

        public LocalizedGossipMenuItem(GossipActionHandler callback, GossipActionDecider decider, RealmLangKey msgKey,
            params object[] msgArgs)
            : this(msgKey, msgArgs)
        {
            this.Action = (IGossipAction) new NonNavigatingDecidingGossipAction(callback, decider);
        }

        public LocalizedGossipMenuItem(GossipActionHandler callback, GossipActionDecider decider,
            RealmLangKey confirmLangKey, RealmLangKey msgKey, params object[] msgArgs)
            : this(msgKey, msgArgs)
        {
            this.ConfirmText = new TranslatableItem(confirmLangKey, new object[0]);
            this.Action = (IGossipAction) new NonNavigatingDecidingGossipAction(callback, decider);
        }

        public LocalizedGossipMenuItem(GossipMenu subMenu, RealmLangKey msgKey, params object[] msgArgs)
            : this(msgKey, new object[3]
            {
                (object) msgArgs,
                null,
                (object) subMenu
            })
        {
        }

        public LocalizedGossipMenuItem(GossipActionHandler callback, GossipMenu subMenu, RealmLangKey msgKey,
            params object[] msgArgs)
            : this(msgKey, msgArgs)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
            this.SubMenu = subMenu;
        }

        public LocalizedGossipMenuItem(IGossipAction action, GossipMenu subMenu, RealmLangKey msgKey,
            params object[] msgArgs)
            : this(msgKey, msgArgs)
        {
            this.Action = action;
            this.SubMenu = subMenu;
        }

        public LocalizedGossipMenuItem(RealmLangKey msgKey, params LocalizedGossipMenuItem[] items)
            : this(msgKey)
        {
            this.SubMenu = new GossipMenu((GossipMenuItemBase[]) items);
        }

        public LocalizedGossipMenuItem(GossipMenuIcon icon, RealmLangKey langKey,
            params LocalizedGossipMenuItem[] items)
            : this(langKey)
        {
            this.Icon = icon;
            this.SubMenu = new GossipMenu((GossipMenuItemBase[]) items);
        }

        public LocalizedGossipMenuItem(GossipMenuIcon icon, IGossipAction action, RealmLangKey msgKey,
            params object[] msgArgs)
            : this(msgKey, msgArgs)
        {
            this.Icon = icon;
            this.Action = action;
        }

        public LocalizedGossipMenuItem(GossipMenuIcon icon, GossipActionHandler callback, RealmLangKey msgKey,
            params object[] msgArgs)
            : this(msgKey, msgArgs)
        {
            this.Icon = icon;
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
        }

        public string DefaultText
        {
            get { return this.Text.TranslateDefault(); }
        }

        public string DefaultConfirmText
        {
            get
            {
                if (this.ConfirmText != null)
                    return this.ConfirmText.TranslateDefault();
                return "";
            }
        }

        public override string GetText(GossipConversation convo)
        {
            return this.Text.Translate(convo.User.Locale);
        }

        public override string GetConfirmText(GossipConversation convo)
        {
            if (this.ConfirmText != null)
                return this.ConfirmText.Translate(convo.User.Locale);
            return "";
        }
    }
}