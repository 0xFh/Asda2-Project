namespace WCell.RealmServer.Commands
{
    public class GossipCommand : RealmServerCommand
    {
        protected override void Initialize()
        {
            this.Init("Gossip");
        }
    }
}