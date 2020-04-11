using System.Threading;
using WCell.Constants.Looting;
using WCell.Core;
using WCell.RealmServer.Looting;

namespace WCell.RealmServer.Asda2Looting
{
    public class Asda2NPCLoot : Asda2Loot
    {
        public static int LastUniqId;

        public Asda2NPCLoot()
        {
            int lastUniqId = Asda2NPCLoot.LastUniqId;
            Interlocked.Increment(ref Asda2NPCLoot.LastUniqId);
            this.EntityId = new EntityId((uint) lastUniqId, HighId.Loot);
        }

        public Asda2NPCLoot(IAsda2Lootable looted, uint money, Asda2LootItem[] items)
            : base(looted, money, items)
        {
        }

        public override LootResponseType ResponseType
        {
            get { return LootResponseType.Default; }
        }
    }
}