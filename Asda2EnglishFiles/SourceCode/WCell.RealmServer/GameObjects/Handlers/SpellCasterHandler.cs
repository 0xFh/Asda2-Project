using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.GOEntries;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GO Type 22</summary>
    public class SpellCasterHandler : GameObjectHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private int chargesLeft;

        protected internal override void Initialize(GameObject go)
        {
            base.Initialize(go);
            this.chargesLeft = ((GOSpellCasterEntry) this.m_go.Entry).Charges;
        }

        public override bool Use(Character user)
        {
            GOSpellCasterEntry entry = (GOSpellCasterEntry) this.m_go.Entry;
            if (entry.Spell == null)
                return false;
            this.m_go.SpellCast.Trigger(entry.Spell, new WorldObject[1]
            {
                (WorldObject) user
            });
            if (this.chargesLeft == 1)
                this.m_go.Delete();
            else if (this.chargesLeft > 0)
                --this.chargesLeft;
            return true;
        }
    }
}