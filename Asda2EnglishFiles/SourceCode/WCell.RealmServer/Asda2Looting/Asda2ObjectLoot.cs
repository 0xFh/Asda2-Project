using NLog;
using System;
using WCell.Constants.Factions;
using WCell.Constants.Looting;
using WCell.Constants.Updates;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Factions;
using WCell.RealmServer.UpdateFields;

namespace WCell.RealmServer.Looting
{
    /// <summary>
    /// TODO: Implement seperated loot for everyone when looting Quest-objects
    /// </summary>
    public class Asda2ObjectLoot : Asda2Loot
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        internal Action OnLootFinish;

        public Asda2ObjectLoot()
        {
        }

        public Asda2ObjectLoot(IAsda2Lootable looted, uint money, Asda2LootItem[] items)
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

        public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
        {
            get { return (UpdateFieldHandler.DynamicUpdateFieldHandler[]) null; }
        }

        protected override UpdateFieldCollection _UpdateFieldInfos
        {
            get { return (UpdateFieldCollection) null; }
        }

        public override UpdateFlags UpdateFlags
        {
            get { return (UpdateFlags) 0; }
        }

        public override ObjectTypeId ObjectTypeId
        {
            get { return ObjectTypeId.Object; }
        }

        public override string Name { get; set; }

        public override Faction Faction { get; set; }

        public override FactionId FactionId { get; set; }
    }
}