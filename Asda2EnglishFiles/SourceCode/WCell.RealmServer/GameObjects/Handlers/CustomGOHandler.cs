using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
    public class CustomGOHandler : GameObjectHandler
    {
        public override bool Use(Character user)
        {
            return true;
        }
    }
}