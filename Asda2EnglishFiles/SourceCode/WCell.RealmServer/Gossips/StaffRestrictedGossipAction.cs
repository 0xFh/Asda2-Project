namespace WCell.RealmServer.Gossips
{
    /// <summary>
    /// Gossip action. Associated Menu item can only be seen by staff members
    /// </summary>
    public class StaffRestrictedGossipAction : NonNavigatingGossipAction
    {
        public StaffRestrictedGossipAction(GossipActionHandler handler)
            : base(handler)
        {
        }

        public override bool CanUse(GossipConversation convo)
        {
            return convo.Character.Role.IsStaff;
        }
    }
}