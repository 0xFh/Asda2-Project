using System.Threading;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Looting;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.RealmServer.UpdateFields;

namespace WCell.RealmServer.Asda2Looting
{
	public class Asda2NPCLoot : Asda2Loot
	{
	    public static int LastUniqId;
		public Asda2NPCLoot()
		{
		    var id = LastUniqId;
		    Interlocked.Increment(ref LastUniqId);
            EntityId = new EntityId((uint) id, HighId.Loot);
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
