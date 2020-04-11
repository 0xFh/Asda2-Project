using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Factions
{
  /// <summary>
  /// Represents the Reputation between a Player and all his known factions
  /// </summary>
  public class ReputationCollection
  {
    private static Logger log = LogManager.GetCurrentClassLogger();
    private Character m_owner;
    private Dictionary<FactionReputationIndex, Reputation> m_byIndex;

    public ReputationCollection(Character chr)
    {
      m_byIndex = new Dictionary<FactionReputationIndex, Reputation>();
      m_owner = chr;
    }

    public int Count
    {
      get { return m_byIndex.Count; }
    }

    public Character Owner
    {
      get { return m_owner; }
      set { m_owner = value; }
    }

    /// <summary>
    /// Initializes initial Factions of the owner (used when new Character is created)
    /// </summary>
    public void Initialize()
    {
    }

    /// <summary>
    /// Loads all Factions that this Character already knows from the DB
    /// </summary>
    public void Load()
    {
      foreach(ReputationRecord record in ReputationRecord.Load(m_owner.Record.Guid))
      {
        Faction faction = FactionMgr.Get(record.ReputationIndex);
        if(faction != null)
        {
          if(m_byIndex.ContainsKey(record.ReputationIndex))
          {
            log.Warn("Character {0} had Reputation with Faction {1} more than once.",
              m_owner, record.ReputationIndex);
          }
          else
          {
            Reputation reputation = new Reputation(record, faction);
            m_byIndex.Add(record.ReputationIndex, reputation);
          }
        }
        else
          log.Warn("Character {0} has saved Reputation with invalid Faction: {1}",
            m_owner, record.ReputationIndex);
      }
    }

    /// <summary>Sends all existing factions to the Client</summary>
    public void ResendAllFactions()
    {
      foreach(Reputation rep in m_byIndex.Values)
        FactionHandler.SendReputationStandingUpdate(m_owner.Client, rep);
    }

    public bool IsHostile(Faction faction)
    {
      Reputation reputation = GetOrCreate(faction.ReputationIndex);
      if(reputation != null)
        return reputation.Hostile;
      return false;
    }

    public bool CanAttack(Faction faction)
    {
      Reputation reputation = GetOrCreate(faction.ReputationIndex);
      if(reputation != null)
        return reputation.Hostile;
      return true;
    }

    public Reputation this[FactionReputationIndex key]
    {
      get
      {
        Reputation reputation;
        m_byIndex.TryGetValue(key, out reputation);
        return reputation;
      }
      set
      {
        throw new Exception(
          "To modify the reputation with a specific faction, just modify the values of an already existing Reputation object.");
      }
    }

    /// <summary>
    /// Returns the corresponding Reputation object. Creates a new one
    /// if the player didn't meet this faction yet.
    /// </summary>
    /// <param name="reputationIndex">The repListId of the faction</param>
    internal Reputation GetOrCreate(FactionReputationIndex reputationIndex)
    {
      Reputation reputation;
      if(!m_byIndex.TryGetValue(reputationIndex, out reputation))
        reputation = Create(reputationIndex);
      return reputation;
    }

    /// <summary>
    /// Creates a Reputation object that represents the relation to the given faction, or null
    /// </summary>
    /// <param name="factionIndex">The repListId of the faction</param>
    private Reputation Create(FactionReputationIndex factionIndex)
    {
      Faction faction = FactionMgr.Get(factionIndex);
      if(faction != null)
        return Create(faction);
      return null;
    }

    /// <summary>
    /// Creates a Reputation object that represents the relation to the given faction, or null
    /// </summary>
    /// <param name="faction">The Faction which the Reputation should be with</param>
    private Reputation Create(Faction faction)
    {
      int defaultReputationValue = GetDefaultReputationValue(faction);
      ReputationFlags defaultReputationFlags = GetDefaultReputationFlags(faction);
      Reputation rep = new Reputation(m_owner.Record.CreateReputationRecord(), faction,
        defaultReputationValue, defaultReputationFlags);
      m_byIndex.Add(faction.ReputationIndex, rep);
      FactionHandler.SendReputationStandingUpdate(m_owner.Client, rep);
      return rep;
    }

    private ReputationFlags GetDefaultReputationFlags(Faction faction)
    {
      FactionEntry entry = faction.Entry;
      for(int index = 0; index < 4; ++index)
      {
        if((entry.ClassMask[index] == ClassMask.None ||
            entry.ClassMask[index].HasAnyFlag(Owner.ClassMask)) &&
           (entry.RaceMask[index] == ~RaceMask.AllRaces1 ||
            entry.RaceMask[index].HasAnyFlag(Owner.RaceMask)))
          return (ReputationFlags) entry.BaseFlags[index];
      }

      return ReputationFlags.None;
    }

    private int GetDefaultReputationValue(Faction faction)
    {
      FactionEntry entry = faction.Entry;
      for(int index = 0; index < 4; ++index)
      {
        if((entry.ClassMask[index] == ClassMask.None ||
            entry.ClassMask[index].HasAnyFlag(Owner.ClassMask)) &&
           (entry.RaceMask[index] == ~RaceMask.AllRaces1 ||
            entry.RaceMask[index].HasAnyFlag(Owner.RaceMask)))
          return entry.BaseRepValue[index];
      }

      return 0;
    }

    public int GetValue(FactionReputationIndex reputationIndex)
    {
      Reputation reputation;
      if(m_byIndex.TryGetValue(reputationIndex, out reputation))
        return reputation.Value;
      return 0;
    }

    public Reputation SetValue(FactionReputationIndex reputationIndex, int value)
    {
      Reputation rep = GetOrCreate(reputationIndex);
      if(rep != null)
        SetValue(rep, value);
      return rep;
    }

    public void SetValue(Reputation rep, int value)
    {
      rep.SetValue(value);
      FactionHandler.SendReputationStandingUpdate(m_owner.Client, rep);
    }

    public Reputation ModValue(FactionId factionId, int value)
    {
      return ModValue(FactionMgr.Get(factionId).ReputationIndex, value);
    }

    public Reputation ModValue(FactionReputationIndex reputationIndex, int value)
    {
      Reputation rep = GetOrCreate(reputationIndex);
      if(rep != null)
        ModValue(rep, value);
      return rep;
    }

    public void ModValue(Reputation rep, int value)
    {
      if(rep.SetValue(rep.Value + value))
      {
        if(rep.StandingLevel >= StandingLevel.Honored)
          Owner.Achievements.CheckPossibleAchievementUpdates(
            AchievementCriteriaType.GainHonoredReputation, 0U, 0U, null);
        if(rep.StandingLevel >= StandingLevel.Revered)
          Owner.Achievements.CheckPossibleAchievementUpdates(
            AchievementCriteriaType.GainReveredReputation, 0U, 0U, null);
        if(rep.StandingLevel >= StandingLevel.Exalted)
          Owner.Achievements.CheckPossibleAchievementUpdates(
            AchievementCriteriaType.GainExaltedReputation, 0U, 0U, null);
        if(value > 0)
          Owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.GainReputation, 0U,
            0U, null);
      }

      FactionHandler.SendReputationStandingUpdate(m_owner.Client, rep);
    }

    public StandingLevel GetStandingLevel(FactionReputationIndex reputationIndex)
    {
      Reputation reputation;
      if(m_byIndex.TryGetValue(reputationIndex, out reputation))
        return reputation.StandingLevel;
      return StandingLevel.Hated;
    }

    /// <summary>Only called if the player declared war</summary>
    public void DeclareWar(FactionReputationIndex reputationIndex, bool hostile, bool sendUpdate)
    {
      Reputation rep = GetOrCreate(reputationIndex);
      if(rep.IsForcedAtPeace || rep.Faction.Group == m_owner.Faction.Group || rep.DeclaredWar == hostile)
        return;
      rep.DeclaredWar = hostile;
      if(!sendUpdate || !rep.DeclaredWar)
        return;
      FactionHandler.SendSetAtWar(m_owner.Client, rep);
    }

    public void SetInactive(FactionReputationIndex reputationIndex, bool inactive)
    {
      Reputation reputation = GetOrCreate(reputationIndex);
      if(reputation == null)
        return;
      reputation.IsInactive = true;
    }

    /// <summary>
    /// For GMs/Testers: Introduces the char to all Factions and sets
    /// Reputation to max.
    /// </summary>
    public void LoveAll()
    {
      foreach(Faction faction in FactionMgr.ByReputationIndex)
      {
        if(faction != null)
          SetValue(faction.ReputationIndex, 42999);
      }
    }

    /// <summary>
    /// For GMs/Testers: Introduces the char to all Factions and sets
    /// Reputation to min (oh boy are they gonna hate you).
    /// </summary>
    public void HateAll()
    {
      foreach(Faction faction in FactionMgr.ByReputationIndex)
      {
        if(faction != null)
          SetValue(faction.ReputationIndex, -42000);
      }
    }

    /// <summary>
    /// Returns the cost of this item after the reputation discount has been applied.
    /// </summary>
    public uint GetDiscountedCost(FactionReputationIndex reputationIndex, uint cost)
    {
      StandingLevel standingLevel = GetStandingLevel(reputationIndex);
      return cost * (100U - Reputation.GetReputationDiscountPct(standingLevel)) / 100U;
    }

    /// <summary>Called when interacting with an NPC.</summary>
    public void OnTalkWith(NPC npc)
    {
      FactionReputationIndex reputationIndex = npc.Faction.ReputationIndex;
      if(reputationIndex < FactionReputationIndex.None || reputationIndex >= FactionReputationIndex.End)
        return;
      Reputation reputation = GetOrCreate(reputationIndex);
      if(reputation.IsForcedInvisible)
        return;
      reputation.IsVisible = true;
      Owner.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KnownFactions, 0U, 0U,
        null);
      FactionHandler.SendVisible(m_owner.Client, reputationIndex);
    }

    /// <summary>
    /// Increases or Decreases reputation with the given faction.
    /// </summary>
    /// <param name="factionId">Faction Id.</param>
    /// <param name="value">Amount to add or decrease</param>
    /// <returns></returns>
    public Reputation GainReputation(FactionId factionId, int value)
    {
      value += (int) Math.Round(value * m_owner.ReputationGainModifierPercent / 100.0);
      return ModValue(factionId, value);
    }

    public uint GetVisibleReputations()
    {
      uint num = 0;
      foreach(Reputation reputation in m_byIndex.Values)
      {
        if(reputation.IsVisible)
          ++num;
      }

      return num;
    }

    public uint GetHonoredReputations()
    {
      uint num = 0;
      foreach(Reputation reputation in m_byIndex.Values)
      {
        if(reputation.StandingLevel >= StandingLevel.Honored)
          ++num;
      }

      return num;
    }

    public uint GetReveredReputations()
    {
      uint num = 0;
      foreach(Reputation reputation in m_byIndex.Values)
      {
        if(reputation.StandingLevel >= StandingLevel.Revered)
          ++num;
      }

      return num;
    }

    public uint GetExaltedReputations()
    {
      uint num = 0;
      foreach(Reputation reputation in m_byIndex.Values)
      {
        if(reputation.StandingLevel >= StandingLevel.Exalted)
          ++num;
      }

      return num;
    }
  }
}