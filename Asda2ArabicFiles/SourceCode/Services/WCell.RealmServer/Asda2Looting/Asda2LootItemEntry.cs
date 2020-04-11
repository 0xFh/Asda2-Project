using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.RealmServer.Items;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2Looting
{
	

	[DataHolder(Inherit = true)]
	public class Asda2NPCLootItemEntry : Asda2LootItemEntry
	{
        
	}

	public class Asda2LootEntity
	{
		public uint MonstrId;
		public int MinAmount, MaxAmount;
		public Asda2ItemId ItemId;
        public int RequiredQuestId;

		/// <summary>
		/// A value between 0 and 100 to indicate the chance of this Entry to drop
		/// </summary>
		public float DropChance;

		public Asda2ItemTemplate ItemTemplate
		{
			get { return Asda2ItemMgr.GetTemplate(ItemId); }
		}
	}

	public abstract class Asda2LootItemEntry : Asda2LootEntity, IDataHolder
	{
		public Asda2LootEntryType LootType;
		public uint GroupId;
	    public int Guid;
		public object GetId()
		{
			return MonstrId;
		}

		public void FinalizeDataHolder()
		{
			if (MinAmount < 1)
			{
				MinAmount = 1;
			}

			if (MinAmount > MaxAmount)
			{
				MaxAmount = MinAmount;
			}

			if (DropChance < 0)
			{
				DropChance = -DropChance;
			}

			Asda2LootMgr.AddEntry(this);
		}

		protected static void AddItems<T>(Asda2LootEntryType t, List<T> all)
			where T : Asda2LootItemEntry
		{
			var entries = Asda2LootMgr.GetEntries(t);
			foreach (var list in entries)
			{
				if (list != null)
				{
					foreach (var entry in list)
					{
						all.Add((T)entry);
					}
				}
			}
		}

		public override string ToString()
		{
			return ItemTemplate + " (" + DropChance + "%)";
		}
	}
}