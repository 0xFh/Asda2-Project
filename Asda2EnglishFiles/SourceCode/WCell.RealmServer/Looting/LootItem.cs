using System;
using System.Collections.Generic;
using WCell.Constants.Looting;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Looting
{
    public class LootItem
    {
        public static readonly LootItem[] EmptyArray = new LootItem[0];
        public readonly ItemTemplate Template;

        /// <summary>
        /// The random id of the Property to be applied to the Item.
        /// </summary>
        public readonly uint RandomPropertyId;

        public readonly uint Index;

        /// <summary>
        /// Creates an array of LootItems from the given array of ItemStackDescriptions
        /// </summary>
        /// <param name="descs"></param>
        /// <returns></returns>
        public static LootItem[] Create(ItemStackTemplate[] descs)
        {
            LootItem[] lootItemArray = new LootItem[descs.Length];
            for (uint index = 0; (long) index < (long) descs.Length; ++index)
            {
                ItemStackTemplate desc = descs[index];
                lootItemArray[index] =
                    new LootItem(desc.Template, desc.Amount, index, desc.Template.RandomPropertiesId);
            }

            return lootItemArray;
        }

        public LootItem(ItemTemplate templ, int amount, uint index, uint randomPropertyId)
        {
            this.Template = templ;
            this.Amount = amount;
            this.Index = index;
            this.RandomPropertyId = randomPropertyId;
        }

        /// <summary>The Amount of the stack of this Item</summary>
        public int Amount { get; internal set; }

        /// <summary>
        /// TODO:
        /// whether this Item is above Threshold and no one claimed it (meaning its now FFA).
        /// </summary>
        public bool Passed { get; internal set; }

        /// <summary>
        /// The current LootDecision determines how distribution of this item is decided
        /// </summary>
        public LootDecision Decision { get; internal set; }

        /// <summary>whether this Item has already been taken.</summary>
        public bool Taken { get; internal set; }

        public LootRollProgress RollProgress { get; internal set; }

        /// <summary>
        /// The list of Looters that are guaranteed to get this Item.
        /// If not null is returned, only these Looters can get this Item.
        /// </summary>
        public ICollection<LooterEntry> MultiLooters { get; private set; }

        /// <summary>
        /// Adds the given Looters to the list of Looters that are guaranteed to get this Item.
        /// If there are any MultiLooters, no one else can get this Item.
        /// </summary>
        /// <param name="looters"></param>
        public void AddMultiLooters(IEnumerable<LooterEntry> looters)
        {
            if (this.MultiLooters == null)
                this.MultiLooters = (ICollection<LooterEntry>) new List<LooterEntry>();
            foreach (LooterEntry looter in looters)
            {
                if (this.Template.CheckLootConstraints(looter.Owner))
                    this.MultiLooters.Add(looter);
            }
        }

        public override string ToString()
        {
            return this.Template.DefaultName + " (" + (object) this.Template.Id + ")";
        }
    }
}