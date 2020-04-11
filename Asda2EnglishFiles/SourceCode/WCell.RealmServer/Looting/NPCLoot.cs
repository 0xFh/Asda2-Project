using WCell.Constants.Looting;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Looting
{
    public class NPCLoot : Loot
    {
        public NPCLoot()
        {
        }

        public NPCLoot(ILootable looted, uint money, LootItem[] items)
            : base(looted, money, items)
        {
        }

        public NPCLoot(ILootable looted, uint money, ItemStackTemplate[] items)
            : base(looted, money, LootItem.Create(items))
        {
        }

        public override LootResponseType ResponseType
        {
            get { return LootResponseType.Default; }
        }
    }
}