using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Looting;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Looting
{
    /// <summary>Represents the Loot-Progress of one LootItem</summary>
    public class LootRollProgress : IDisposable
    {
        private readonly ICollection<LooterEntry> m_RemainingParticipants;
        private readonly SortedDictionary<LootRollEntry, LooterEntry> m_rolls;
        private Loot m_loot;
        private LootItem m_lootItem;

        public LootRollProgress(Loot loot, LootItem lootItem, ICollection<LooterEntry> looters)
        {
            this.m_loot = loot;
            this.m_lootItem = lootItem;
            this.m_RemainingParticipants = (ICollection<LooterEntry>) new List<LooterEntry>(looters.Count);
            foreach (LooterEntry looter in (IEnumerable<LooterEntry>) looters)
            {
                if (!looter.Owner.PassOnLoot)
                    this.m_RemainingParticipants.Add(looter);
            }

            this.m_rolls = new SortedDictionary<LootRollEntry, LooterEntry>();
        }

        /// <summary>Participants who did not roll yet</summary>
        public ICollection<LooterEntry> RemainingParticipants
        {
            get { return this.m_RemainingParticipants; }
        }

        /// <summary>
        /// The rolls that have been casted so far. The winner will receive this item, once the roll ended.
        /// </summary>
        public SortedDictionary<LootRollEntry, LooterEntry> Rolls
        {
            get { return this.m_rolls; }
        }

        /// <summary>Whether every participant rolled</summary>
        public bool IsRollFinished
        {
            get { return this.m_RemainingParticipants.Count == 0; }
        }

        /// <summary>
        /// The participant that currently rolled the highest Number - also considering
        /// need/greed priorities.
        /// </summary>
        public Character HighestParticipant
        {
            get
            {
                for (int index = this.m_rolls.Count - 1; index >= 0; --index)
                {
                    KeyValuePair<LootRollEntry, LooterEntry> keyValuePair =
                        this.m_rolls.ElementAt<KeyValuePair<LootRollEntry, LooterEntry>>(index);
                    if (keyValuePair.Value.Owner != null)
                        return keyValuePair.Value.Owner;
                }

                return (Character) null;
            }
        }

        public LootRollEntry HighestEntry
        {
            get { return this.Rolls.ElementAt<KeyValuePair<LootRollEntry, LooterEntry>>(0).Key; }
        }

        /// <summary>Lets the given Character roll</summary>
        /// <param name="chr"></param>
        /// <param name="type"></param>
        public void Roll(Character chr, LootRollType type)
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            this.m_lootItem.RollProgress = (LootRollProgress) null;
            this.m_loot = (Loot) null;
            this.m_lootItem = (LootItem) null;
        }
    }
}