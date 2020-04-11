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
            this.Icon = type;
            this.Texts = texts;
        }

        public MultiStringGossipMenuItem(string[] texts)
            : this(GossipMenuIcon.Talk, texts)
        {
        }

        public MultiStringGossipMenuItem(string[] texts, IGossipAction action)
            : this(texts)
        {
            this.Action = action;
        }

        public MultiStringGossipMenuItem(string[] texts, GossipActionHandler callback)
            : this(texts)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
        }

        public MultiStringGossipMenuItem(string[] texts, GossipActionHandler callback, string[] confirmTexts)
            : this(texts)
        {
            this.ConfirmTexts = confirmTexts;
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
        }

        public MultiStringGossipMenuItem(string[] texts, GossipActionHandler callback,
            params MultiStringGossipMenuItem[] items)
            : this(texts)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
            this.SubMenu = new GossipMenu((GossipMenuItemBase[]) items);
        }

        public MultiStringGossipMenuItem(string[] texts, GossipMenu subMenu)
            : this(texts, (IGossipAction) null, subMenu)
        {
        }

        public MultiStringGossipMenuItem(string[] texts, GossipActionHandler callback, GossipMenu subMenu)
            : this(texts)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
            this.SubMenu = subMenu;
        }

        public MultiStringGossipMenuItem(string[] texts, IGossipAction action, GossipMenu subMenu)
            : this(texts)
        {
            this.Action = action;
            this.SubMenu = subMenu;
        }

        public MultiStringGossipMenuItem(string[] texts, params MultiStringGossipMenuItem[] items)
            : this(texts)
        {
            this.SubMenu = new GossipMenu((GossipMenuItemBase[]) items);
        }

        public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, params MultiStringGossipMenuItem[] items)
            : this(texts)
        {
            this.Icon = icon;
            this.SubMenu = new GossipMenu((GossipMenuItemBase[]) items);
        }

        public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, GossipMenu subMenu)
            : this(texts)
        {
            this.Icon = icon;
            this.SubMenu = subMenu;
        }

        public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, IGossipAction action)
            : this(texts)
        {
            this.Icon = icon;
            this.Action = action;
        }

        public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, IGossipAction action, GossipMenu subMenu)
            : this(texts)
        {
            this.Icon = icon;
            this.SubMenu = subMenu;
            this.Action = action;
        }

        public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, GossipActionHandler callback)
            : this(texts)
        {
            this.Icon = icon;
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
        }

        public MultiStringGossipMenuItem(GossipMenuIcon icon, string[] texts, GossipActionHandler callback,
            GossipMenu subMenu)
            : this(texts)
        {
            this.Icon = icon;
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
            this.SubMenu = subMenu;
        }

        public string DefaultText
        {
            get { return this.Texts.LocalizeWithDefaultLocale(); }
            set { this.Texts[(int) RealmServerConfiguration.DefaultLocale] = value; }
        }

        public string DefaultConfirmText
        {
            get { return this.ConfirmTexts.LocalizeWithDefaultLocale(); }
            set { this.ConfirmTexts[(int) RealmServerConfiguration.DefaultLocale] = value; }
        }

        public override string GetText(GossipConversation convo)
        {
            return this.Texts.Localize(convo.User.Locale);
        }

        public override string GetConfirmText(GossipConversation convo)
        {
            return this.ConfirmTexts.Localize(convo.User.Locale);
        }
    }
}