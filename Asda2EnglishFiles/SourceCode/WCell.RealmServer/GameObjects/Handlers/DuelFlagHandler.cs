using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>GO Type 16</summary>
    public class DuelFlagHandler : GameObjectHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>The Duel that is currently being fought</summary>
        public Duel Duel;

        public override bool Use(Character user)
        {
            return true;
        }

        protected internal override void OnRemove()
        {
            if (this.Duel == null)
                return;
            this.Duel.Cleanup();
            this.Duel = (Duel) null;
        }
    }
}