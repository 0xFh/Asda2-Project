using WCell.Constants.GameObjects;
using WCell.Constants.Looting;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Looting;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GO Type 3</summary>
    public class ChestHandler : GameObjectHandler
    {
        public override bool Use(Character user)
        {
            if (this.m_go.Entry.IsConsumable)
                this.m_go.State = GameObjectState.Disabled;
            if (this.m_go.Loot == null)
                LootMgr.CreateAndSendObjectLoot((ILootable) this.m_go, user, LootEntryType.GameObject,
                    user.Map.IsHeroic);
            return true;
        }
    }
}