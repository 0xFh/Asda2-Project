using System;
using WCell.Constants.Factions;
using WCell.RealmServer.Database;
using WCell.Util;

namespace WCell.RealmServer.Factions
{
  public class Reputation
  {
    /// <summary>Discounts indexed by StandingLevel</summary>
    public static uint[] DiscountPercents = new uint[8]
    {
      0U,
      0U,
      0U,
      0U,
      5U,
      10U,
      15U,
      20U
    };

    public static readonly Standing[] Standings = (Standing[]) Enum.GetValues(typeof(Standing));
    public const int Max = 42999;
    public const int Min = -42000;
    private Standing m_standing;
    private readonly ReputationRecord m_record;
    public readonly Faction Faction;

    /// <summary>Loads an existing Reputation from the given Record.</summary>
    public Reputation(ReputationRecord record, Faction faction)
    {
      m_record = record;
      Faction = faction;
      m_standing = GetStanding(record.Value);
    }

    public Reputation(ReputationRecord record, Faction faction, int defaultValue, ReputationFlags defaultFlags)
    {
      m_record = record;
      m_record.ReputationIndex = faction.ReputationIndex;
      m_record.Value = defaultValue;
      m_record.Flags = defaultFlags;
      Faction = faction;
      m_standing = GetStanding(defaultValue);
      m_record.Save();
    }

    /// <summary>The reputation value</summary>
    public int Value
    {
      get { return m_record.Value; }
    }

    public Standing Standing
    {
      get { return m_standing; }
    }

    /// <summary>Exalted, Honored, Neutral, Hated</summary>
    public StandingLevel StandingLevel
    {
      get { return GetStandingLevel(m_record.Value); }
    }

    public ReputationFlags Flags
    {
      get { return m_record.Flags; }
    }

    /// <summary>
    /// Whether racial and faction mounts/tabards etc can be purchased.
    /// </summary>
    public bool SpecialItems
    {
      get { return m_standing >= Standing.Exalted; }
    }

    /// <summary>
    /// Whether Heroic mode keys can be purchased for Outland dungeons.
    /// <see href="http://www.wowwiki.com/Heroic" />
    /// </summary>
    public bool HeroicModeAllowed
    {
      get { return m_standing >= Standing.Honored; }
    }

    /// <summary>
    /// Enough reputation to interact with NPCs of that Faction
    /// </summary>
    public bool CanInteract
    {
      get { return m_standing >= Standing.Neutral; }
    }

    /// <summary>
    /// Either very bad rep or the player declared war.
    /// Will cause mobs to attack on sight.
    /// </summary>
    public bool Hostile
    {
      get
      {
        if(!DeclaredWar)
          return IsHostileStanding(m_standing);
        return true;
      }
    }

    public bool IsVisible
    {
      get { return m_record.Flags.HasFlag(ReputationFlags.Visible); }
      internal set
      {
        if(IsForcedInvisible)
          return;
        if(value)
          m_record.Flags |= ReputationFlags.Visible;
        else
          m_record.Flags &= ~ReputationFlags.Visible;
      }
    }

    /// <summary>whether the player actively declared war</summary>
    public bool DeclaredWar
    {
      get { return m_record.Flags.HasFlag(ReputationFlags.AtWar); }
      internal set
      {
        if(IsForcedAtPeace)
          return;
        if(value)
          m_record.Flags |= ReputationFlags.AtWar;
        else
          m_record.Flags &= ~ReputationFlags.AtWar;
      }
    }

    public bool IsHidden
    {
      get { return m_record.Flags.HasFlag(ReputationFlags.Hidden); }
      set
      {
        if(value)
          m_record.Flags |= ReputationFlags.Hidden;
        else
          m_record.Flags &= ~ReputationFlags.Hidden;
      }
    }

    public bool IsForcedInvisible
    {
      get { return m_record.Flags.HasFlag(ReputationFlags.ForcedInvisible); }
      internal set
      {
        if(value)
          m_record.Flags |= ReputationFlags.ForcedInvisible;
        else
          m_record.Flags &= ~ReputationFlags.ForcedInvisible;
      }
    }

    public bool IsForcedAtPeace
    {
      get { return m_record.Flags.HasFlag(ReputationFlags.ForcedPeace); }
      internal set
      {
        if(value)
          m_record.Flags |= ReputationFlags.ForcedPeace;
        else
          m_record.Flags &= ~ReputationFlags.ForcedPeace;
      }
    }

    public bool IsInactive
    {
      get { return m_record.Flags.HasFlag(ReputationFlags.Inactive); }
      set
      {
        if(value)
          m_record.Flags |= ReputationFlags.Inactive;
        else
          m_record.Flags &= ~ReputationFlags.Inactive;
      }
    }

    /// <summary>
    /// Changes the reputation value with a specific Faction.
    /// Is called by ReputationCollect.SetValue
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Whether hostility changed due to the stending change</returns>
    internal bool SetValue(int value)
    {
      Standing standing = m_standing;
      bool hostile1 = Hostile;
      m_standing = GetStanding(value);
      bool hostile2 = Hostile;
      m_record.Value = value;
      if(standing != m_standing)
        return hostile1 != hostile2;
      return false;
    }

    static Reputation()
    {
      Array.Sort(Standings);
    }

    public static Standing GetStanding(int repValue)
    {
      for(int index = Standings.Length - 1; index >= 0; --index)
      {
        if((Standing) repValue >= Standings[index])
          return Standings[index];
      }

      return Standing.Hated;
    }

    public static StandingLevel GetStandingLevel(int repValue)
    {
      for(int index = 0; index < Standings.Length; ++index)
      {
        if((Standing) repValue >= Standings[index])
          return (StandingLevel) (Standings.Length - index);
      }

      return StandingLevel.Hated;
    }

    public static bool IsHostileStanding(Standing standing)
    {
      return standing <= Standing.Hostile;
    }

    public static uint GetReputationDiscountPct(StandingLevel lvl)
    {
      return DiscountPercents.Get((uint) lvl);
    }
  }
}