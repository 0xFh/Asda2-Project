using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects
{
    /// <summary>GO Type 6</summary>
    public class TrapHandler : GameObjectHandler
    {
        protected internal override void Initialize(GameObject go)
        {
        }

        public override bool Use(Character user)
        {
            return true;
        }
    }
}