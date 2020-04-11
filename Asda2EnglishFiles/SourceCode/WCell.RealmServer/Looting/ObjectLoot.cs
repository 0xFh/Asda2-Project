using NLog;
using System;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
    /// <summary>
    /// TODO: Implement seperated loot for everyone when looting Quest-objects
    /// </summary>
    public class ObjectLoot : Loot
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        internal Action OnLootFinish;

        public ObjectLoot()
        {
        }

        public ObjectLoot(ILootable looted, uint money, LootItem[] items)
            : base(looted, money, items)
        {
        }

        public override LootResponseType ResponseType
        {
            get { return LootResponseType.Profession; }
        }

        protected override void OnDispose()
        {
            if (this.OnLootFinish != null)
            {
                this.OnLootFinish();
                this.OnLootFinish = (Action) null;
            }

            base.OnDispose();
        }
    }
}