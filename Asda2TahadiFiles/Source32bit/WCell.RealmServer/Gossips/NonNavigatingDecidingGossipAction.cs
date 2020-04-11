namespace WCell.RealmServer.Gossips
{
  public class NonNavigatingDecidingGossipAction : NonNavigatingGossipAction
  {
    public GossipActionDecider Decider { get; set; }

    public NonNavigatingDecidingGossipAction(GossipActionHandler handler, GossipActionDecider decider)
      : base(handler)
    {
      Decider = decider;
    }

    public override bool CanUse(GossipConversation convo)
    {
      return Decider(convo);
    }
  }
}