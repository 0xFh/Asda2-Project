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
      m_loot = loot;
      m_lootItem = lootItem;
      m_RemainingParticipants = new List<LooterEntry>(looters.Count);
      foreach(LooterEntry looter in looters)
      {
        if(!looter.Owner.PassOnLoot)
          m_RemainingParticipants.Add(looter);
      }

      m_rolls = new SortedDictionary<LootRollEntry, LooterEntry>();
    }

    /// <summary>Participants who did not roll yet</summary>
    public ICollection<LooterEntry> RemainingParticipants
    {
      get { return m_RemainingParticipants; }
    }

    /// <summary>
    /// The rolls that have been casted so far. The winner will receive this item, once the roll ended.
    /// </summary>
    public SortedDictionary<LootRollEntry, LooterEntry> Rolls
    {
      get { return m_rolls; }
    }

    /// <summary>Whether every participant rolled</summary>
    public bool IsRollFinished
    {
      get { return m_RemainingParticipants.Count == 0; }
    }

    /// <summary>
    /// The participant that currently rolled the highest Number - also considering
    /// need/greed priorities.
    /// </summary>
    public Character HighestParticipant
    {
      get
      {
        for(int index = m_rolls.Count - 1; index >= 0; --index)
        {
          KeyValuePair<LootRollEntry, LooterEntry> keyValuePair =
            m_rolls.ElementAt(index);
          if(keyValuePair.Value.Owner != null)
            return keyValuePair.Value.Owner;
        }

        return null;
      }
    }

    public LootRollEntry HighestEntry
    {
      get { return Rolls.ElementAt(0).Key; }
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
      m_lootItem.RollProgress = null;
      m_loot = null;
      m_lootItem = null;
    }
  }
}