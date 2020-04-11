using System.Collections.Generic;
using WCell.Constants.Looting;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Asda2Looting
{
	public class Asda2LootItem
	{
        public static readonly Asda2LootItem[] EmptyArray = new Asda2LootItem[0];

		public readonly Asda2ItemTemplate Template;
        
        public readonly uint Index;

	    public Asda2LootItem(Asda2ItemTemplate templ, int amount, uint index)
		{
			Template = templ;
			Amount = amount;
		    Index = index;
		}

		/// <summary>
		/// The Amount of the stack of this Item
		/// </summary>
		public int Amount { get; internal set; }

	    public Asda2Loot Loot { get; set; }

	    /// <summary>
		/// whether this Item has already been taken.
		/// </summary>
		public bool Taken
		{
			get;
			internal set;
		}

		/// <summary>
	    /// The list of Looters that are guaranteed to get this Item.
	    /// If not null is returned, only these Looters can get this Item.
	    /// </summary>
	    public ICollection<Asda2LooterEntry> MultiLooters { get; private set; }

	    public Vector2Short Position { get; set; }

	    /// <summary>
		/// Adds the given Looters to the list of Looters that are guaranteed to get this Item.
		/// If there are any MultiLooters, no one else can get this Item.
		/// </summary>
		/// <param name="looters"></param>
		public void AddMultiLooters(IEnumerable<Asda2LooterEntry> looters)
		{
			if (MultiLooters == null)
			{
				MultiLooters = new List<Asda2LooterEntry>();
			}

			foreach (var looter in looters)
			{
				if (Template.CheckLootConstraints(looter.Owner))
				{
					MultiLooters.Add(looter);
				}
			}
		}

		public override string ToString()
		{
			return Template.Name + " (" + Template.Id + ")";
		}
	}
}