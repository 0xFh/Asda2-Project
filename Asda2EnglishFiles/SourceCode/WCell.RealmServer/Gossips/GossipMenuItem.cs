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
            this.Text = string.Empty;
            this.ConfirmText = string.Empty;
        }

        public GossipMenuItem(GossipMenuIcon type, string text)
        {
            this.Icon = type;
            this.Text = text;
            this.ConfirmText = string.Empty;
        }

        public GossipMenuItem(string text)
            : this(GossipMenuIcon.Talk, text)
        {
        }

        public GossipMenuItem(string text, IGossipAction action)
            : this(text)
        {
            this.Action = action;
        }

        public GossipMenuItem(string text, GossipActionHandler callback)
            : this(text)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
        }

        public GossipMenuItem(string text, GossipActionHandler callback, string confirmText)
            : this(text)
        {
            this.ConfirmText = confirmText;
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
        }

        public GossipMenuItem(string text, GossipActionHandler callback, params GossipMenuItem[] items)
            : this(text)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
            this.SubMenu = new GossipMenu((GossipMenuItemBase[]) items);
        }

        public GossipMenuItem(string text, GossipMenu subMenu)
            : this(text, (IGossipAction) null, subMenu)
        {
        }

        public GossipMenuItem(string text, GossipActionHandler callback, GossipMenu subMenu)
            : this(text)
        {
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
            this.SubMenu = subMenu;
        }

        public GossipMenuItem(string text, IGossipAction action, GossipMenu subMenu)
            : this(text)
        {
            this.Action = action;
            this.SubMenu = subMenu;
        }

        public GossipMenuItem(string text, params GossipMenuItem[] items)
            : this(text)
        {
            this.SubMenu = new GossipMenu((GossipMenuItemBase[]) items);
        }

        public GossipMenuItem(GossipMenuIcon icon, string text, params GossipMenuItem[] items)
            : this(text)
        {
            this.Icon = icon;
            this.SubMenu = new GossipMenu((GossipMenuItemBase[]) items);
        }

        public GossipMenuItem(GossipMenuIcon icon, string text, IGossipAction action)
            : this(text)
        {
            this.Icon = icon;
            this.Action = action;
        }

        public GossipMenuItem(GossipMenuIcon icon, string text, GossipActionHandler callback)
            : this(text)
        {
            this.Icon = icon;
            this.Action = (IGossipAction) new NonNavigatingGossipAction(callback);
        }

        public override string GetText(GossipConversation convo)
        {
            return this.Text;
        }

        public override string GetConfirmText(GossipConversation convo)
        {
            return this.ConfirmText;
        }
    }
}