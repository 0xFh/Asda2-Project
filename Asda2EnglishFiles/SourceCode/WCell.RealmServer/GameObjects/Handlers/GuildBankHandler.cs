using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
    public class GuildBankHandler : GameObjectHandler
    {
        public override bool Use(Character user)
        {
            return true;
        }
    }
}