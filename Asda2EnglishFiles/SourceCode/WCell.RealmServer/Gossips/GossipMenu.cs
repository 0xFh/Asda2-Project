using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.RealmServer.Lang;

namespace WCell.RealmServer.Gossips
{
    /// <summary>
    /// Represents single menu in conversation with it's items
    /// </summary>
    [Serializable]
    public class GossipMenu
    {
        public static readonly GossipMenuItem[] EmptyGossipItems = new GossipMenuItem[0];
        private IGossipEntry m_textEntry;
        private List<GossipMenuItemBase> m_gossipItems;
        private GossipMenu m_parent;

        /// <summary>Default constructor</summary>
        public GossipMenu()
        {
            this.m_textEntry = (IGossipEntry) GossipMgr.DefaultGossipEntry;
        }

        /// <summary>Constructor initializing menu with body text ID</summary>
        /// <param name="bodyTextID">GossipEntry Id</param>
        public GossipMenu(uint bodyTextID)
        {
            this.m_textEntry = GossipMgr.GetEntry(bodyTextID);
            if (this.m_textEntry != null)
                return;
            this.m_textEntry = (IGossipEntry) GossipMgr.DefaultGossipEntry;
            LogManager.GetCurrentClassLogger()
                .Warn("Tried to create GossipMenu with invalid GossipEntry id: " + (object) bodyTextID);
        }

        public GossipMenu(IGossipEntry textEntry)
        {
            this.m_textEntry = textEntry;
            if (this.m_textEntry == null)
                throw new ArgumentNullException(nameof(textEntry));
        }

        public GossipMenu(uint bodyTextId, List<GossipMenuItemBase> items)
            : this(bodyTextId)
        {
            this.m_gossipItems = items;
        }

        public GossipMenu(uint bodyTextId, params GossipMenuItem[] items)
            : this(bodyTextId)
        {
            this.m_gossipItems = new List<GossipMenuItemBase>(items.Length);
            foreach (GossipMenuItem gossipMenuItem in items)
            {
                this.CheckItem((GossipMenuItemBase) gossipMenuItem);
                this.m_gossipItems.Add((GossipMenuItemBase) gossipMenuItem);
            }
        }

        public GossipMenu(uint bodyTextId, params GossipMenuItemBase[] items)
            : this(bodyTextId)
        {
            this.m_gossipItems = new List<GossipMenuItemBase>(items.Length);
            foreach (GossipMenuItemBase gossipMenuItemBase in items)
            {
                this.CheckItem(gossipMenuItemBase);
                this.m_gossipItems.Add(gossipMenuItemBase);
            }
        }

        public GossipMenu(IGossipEntry text, List<GossipMenuItemBase> items)
            : this(text)
        {
            this.m_gossipItems = items;
        }

        public GossipMenu(IGossipEntry text, params GossipMenuItemBase[] items)
            : this(text)
        {
            this.m_gossipItems = new List<GossipMenuItemBase>(items.Length);
            foreach (GossipMenuItemBase gossipMenuItemBase in items)
            {
                this.CheckItem(gossipMenuItemBase);
                this.m_gossipItems.Add(gossipMenuItemBase);
            }
        }

        public GossipMenu(params GossipMenuItemBase[] items)
            : this()
        {
            this.m_gossipItems = new List<GossipMenuItemBase>(items.Length);
            foreach (GossipMenuItemBase gossipMenuItemBase in items)
            {
                this.CheckItem(gossipMenuItemBase);
                this.m_gossipItems.Add(gossipMenuItemBase);
            }
        }

        public GossipMenu ParentMenu
        {
            get { return this.m_parent; }
        }

        public IList<GossipMenuItemBase> GossipItems
        {
            get
            {
                if (this.m_gossipItems == null)
                    return (IList<GossipMenuItemBase>) GossipMenu.EmptyGossipItems;
                return (IList<GossipMenuItemBase>) this.m_gossipItems;
            }
        }

        /// <summary>ID of text in the body of this menu</summary>
        public IGossipEntry GossipEntry
        {
            get { return this.m_textEntry; }
            set { this.m_textEntry = value; }
        }

        /// <summary>
        /// Will keep resending the Gump until deactivated (usually using a Quit button)
        /// </summary>
        public bool KeepOpen { get; set; }

        public void AddRange(params GossipMenuItemBase[] items)
        {
            foreach (GossipMenuItemBase gossipMenuItemBase in items)
            {
                this.CheckItem(gossipMenuItemBase);
                this.m_gossipItems.Add(gossipMenuItemBase);
            }
        }

        protected void CheckItem(GossipMenuItemBase item)
        {
            if (item.SubMenu == null)
                return;
            item.SubMenu.m_parent = this;
        }

        public void AddItem(GossipMenuItemBase item)
        {
            if (this.m_gossipItems == null)
                this.m_gossipItems = new List<GossipMenuItemBase>(1);
            this.CheckItem(item);
            this.m_gossipItems.Add(item);
        }

        /// <summary>
        /// Replaces the item at the given index with the given item.
        /// If index == count, appends item to end.
        /// </summary>
        public void SetItem(int index, GossipMenuItemBase item)
        {
            if (this.m_gossipItems == null)
                this.m_gossipItems = new List<GossipMenuItemBase>(1);
            this.CheckItem(item);
            if (index == this.m_gossipItems.Count)
                this.m_gossipItems.Add(item);
            else
                this.m_gossipItems[index] = item;
        }

        public void AddItem(GossipMenuIcon type)
        {
            this.AddItem((GossipMenuItemBase) new GossipMenuItem(type, type.ToString()));
        }

        public void AddItem(int index, GossipMenuItemBase item)
        {
            if (this.m_gossipItems == null)
                this.m_gossipItems = new List<GossipMenuItemBase>(1);
            if (item == null)
                return;
            this.CheckItem(item);
            this.m_gossipItems.Insert(index, item);
        }

        public void AddQuitMenuItem(RealmLangKey msg = RealmLangKey.Done)
        {
            this.AddItem((GossipMenuItemBase) new QuitGossipMenuItem(msg, new object[0]));
        }

        public void AddQuitMenuItem(RealmLangKey msg, params object[] args)
        {
            this.AddItem((GossipMenuItemBase) new QuitGossipMenuItem(msg, args));
        }

        public void AddQuitMenuItem(GossipActionHandler callback, RealmLangKey msg = RealmLangKey.Done,
            params object[] args)
        {
            this.AddItem((GossipMenuItemBase) new QuitGossipMenuItem(callback, msg, args));
        }

        public void AddGoBackItem()
        {
            this.AddGoBackItem("Go back...");
        }

        public void AddGoBackItem(string text)
        {
            NavigatingGossipAction navigatingGossipAction =
                new NavigatingGossipAction((GossipActionHandler) (convo =>
                    convo.Character.GossipConversation.GoBack()));
            this.AddItem((GossipMenuItemBase) new GossipMenuItem(text, (IGossipAction) navigatingGossipAction));
        }

        public void AddGoBackItem(string text, GossipActionHandler callback)
        {
            NavigatingGossipAction navigatingGossipAction = new NavigatingGossipAction((GossipActionHandler) (convo =>
            {
                callback(convo);
                convo.Character.GossipConversation.GoBack();
            }));
            this.AddItem((GossipMenuItemBase) new GossipMenuItem(text, (IGossipAction) navigatingGossipAction));
        }

        public bool RemoveItem(GossipMenuItemBase item)
        {
            return this.m_gossipItems.Remove(item);
        }

        public void ClearAllItems()
        {
            this.m_gossipItems.Clear();
        }

        /// <summary>Called before menu is sent to Character</summary>
        protected internal virtual void OnDisplay(GossipConversation convo)
        {
        }

        internal void NotifyClose(GossipConversation convo)
        {
        }

        private void Dispose()
        {
            this.m_gossipItems = (List<GossipMenuItemBase>) null;
            if (this.m_parent == null)
                return;
            this.m_parent.Dispose();
        }
    }
}