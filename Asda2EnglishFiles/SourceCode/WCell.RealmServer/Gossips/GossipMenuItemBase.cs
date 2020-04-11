using System;
using WCell.Constants;

namespace WCell.RealmServer.Gossips
{
    /// <summary>
    /// Represents action (battlemaster, flightmaster, etc.) gossip item in menu
    /// </summary>
    [Serializable]
    public abstract class GossipMenuItemBase
    {
        public GossipMenuIcon Icon;
        public int RequiredMoney;
        public byte Input;

        /// <summary>
        /// Determines if character is elligible for viewing this item and action taken on item selection
        /// </summary>
        public IGossipAction Action;

        /// <summary>
        /// The <see cref="T:WCell.RealmServer.Gossips.GossipMenu" /> to be shown when selecting this Item
        /// </summary>
        public GossipMenu SubMenu { get; set; }

        public void SetAction(NonNavigatingGossipAction action)
        {
            this.Action = (IGossipAction) action;
        }

        public abstract string GetText(GossipConversation convo);

        public abstract string GetConfirmText(GossipConversation convo);
    }
}