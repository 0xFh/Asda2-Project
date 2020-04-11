using WCell.RealmServer.Handlers;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Talents
{
  public class Talent
  {
    public readonly TalentCollection Talents;
    public readonly TalentEntry Entry;
    private int m_rank;

    internal Talent(TalentCollection talents, TalentEntry entry)
    {
      Talents = talents;
      Entry = entry;
    }

    public Talent(TalentCollection talents, TalentEntry entry, int rank)
    {
      m_rank = -1;
      Talents = talents;
      Entry = entry;
      Rank = rank;
    }

    public Spell Spell
    {
      get { return Entry.Spells[m_rank]; }
    }

    /// <summary>The actual rank, as displayed in the GUI</summary>
    public int ActualRank
    {
      get { return Rank + 1; }
      set { Rank = value - 1; }
    }

    /// <summary>
    /// Current zero-based rank of this Talent.
    /// The rank displayed in the GUI is Rank+1.
    /// </summary>
    public int Rank
    {
      get { return m_rank; }
      set
      {
        int diff;
        if(m_rank > value)
        {
          if(value < -1)
            value = -1;
          int delta = m_rank - value;
          Talents.UpdateFreeTalentPointsSilently(delta);
          for(int rank = m_rank; rank >= value + 1; --rank)
            Talents.Owner.Spells.Remove(Entry.Spells[rank]);
          if(value < 0)
            Talents.ById.Remove(Entry.Id);
          diff = -delta;
        }
        else
        {
          if(value <= m_rank)
            return;
          if(value > Entry.MaxRank - 1)
            value = Entry.MaxRank - 1;
          diff = value - m_rank;
          for(int index = m_rank + 1; index <= value; ++index)
            Talents.Owner.Spells.AddSpell(Entry.Spells[value]);
          Talents.UpdateFreeTalentPointsSilently(-diff);
        }

        Talents.UpdateTreePoint(Entry.Tree.TabIndex, diff);
        m_rank = value;
      }
    }

    /// <summary>
    /// Sets the rank without sending any packets or doing checks.
    /// Also does not increment spent talent points
    /// </summary>
    internal void SetRankSilently(int rank)
    {
      m_rank = rank;
    }

    public void Remove()
    {
      Remove(true);
    }

    /// <summary>Removes all ranks of this talent.</summary>
    internal void Remove(bool update)
    {
      Rank = -1;
      if(!update)
        return;
      TalentHandler.SendTalentGroupList(Talents);
    }
  }
}