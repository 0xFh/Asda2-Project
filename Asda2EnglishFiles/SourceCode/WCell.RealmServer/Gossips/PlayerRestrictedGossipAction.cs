namespace WCell.RealmServer.Gossips
{
    /// <summary>
    /// Gossip action. Associated Menu item can only be seen by players that are not staff members
    /// </summary>
    public class PlayerRestrictedGossipAction : NonNavigatingGossipAction
    {
        public PlayerRestrictedGossipAction(GossipActionHandler handler)
            : base(handler)
        {
        }

        public override bool CanUse(GossipConversation convo)
        {
            return !convo.Character.Role.IsStaff;
        }
    }
}