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
            this.CurrentMenu = menu;
            this.Character = chr;
            this.Speaker = speaker;
            this.StayOpen = keepOpen;
        }

        /// <summary>Current menu</summary>
        public GossipMenu CurrentMenu { get; protected internal set; }

        public IUser User
        {
            get { return (IUser) this.Character; }
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
            this.DisplayMenu(this.CurrentMenu);
        }

        /// <summary>Handles selection of item in menu by player</summary>
        /// <param name="itemID">ID of selected item</param>
        /// <param name="extra">additional parameter supplied by user</param>
        public void HandleSelectedItem(uint itemID, string extra)
        {
            IList<GossipMenuItemBase> gossipItems = this.CurrentMenu.GossipItems;
            if ((long) itemID >= (long) gossipItems.Count)
                return;
            GossipMenuItemBase gossipMenuItemBase = gossipItems[(int) itemID];
            if (gossipMenuItemBase == null)
                return;
            if (gossipMenuItemBase.Action != null && gossipMenuItemBase.Action.CanUse(this))
            {
                GossipMenu currentMenu = this.CurrentMenu;
                gossipMenuItemBase.Action.OnSelect(this);
                if (currentMenu != this.CurrentMenu || gossipMenuItemBase.Action.Navigates)
                    return;
            }

            if (gossipMenuItemBase.SubMenu != null)
                this.DisplayMenu(gossipMenuItemBase.SubMenu);
            else if (this.StayOpen)
            {
                this.DisplayCurrentMenu();
            }
            else
            {
                this.CurrentMenu.NotifyClose(this);
                this.Dispose();
            }
        }

        /// <summary>Shows menu to player</summary>
        /// <param name="menu">menu to show</param>
        public void DisplayMenu(GossipMenu menu)
        {
            this.CurrentMenu = menu;
            menu.OnDisplay(this);
            if (this.Speaker is IQuestHolder && ((IQuestHolder) this.Speaker).QuestHolderInfo != null)
                GossipHandler.SendPageToCharacter(this,
                    (IList<QuestMenuItem>) ((IQuestHolder) this.Speaker).QuestHolderInfo.GetQuestMenuItems(
                        this.Character));
            else
                GossipHandler.SendPageToCharacter(this, (IList<QuestMenuItem>) null);
        }

        public void Dispose()
        {
            GossipHandler.SendConversationComplete((IPacketReceiver) this.Character);
            if (this.Character.GossipConversation != this)
                return;
            this.Character.GossipConversation = (GossipConversation) null;
            this.CurrentMenu = (GossipMenu) null;
            this.Speaker = (WorldObject) null;
        }

        /// <summary>Cancels the current conversation</summary>
        public void Cancel()
        {
            GossipHandler.SendConversationComplete((IPacketReceiver) this.Character);
            this.Dispose();
        }

        /// <summary>Closes any open Client menu and sends the CurrentMenu</summary>
        public void Invalidate()
        {
            GossipHandler.SendConversationComplete((IPacketReceiver) this.Character);
            this.DisplayCurrentMenu();
        }

        public void GoBack()
        {
            if (this.CurrentMenu.ParentMenu != null)
                this.DisplayMenu(this.CurrentMenu.ParentMenu);
            else
                this.DisplayCurrentMenu();
        }
    }
}