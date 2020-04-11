using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Talents;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Talents
{
  /// <summary>Represents all Talents of a Character or Pet</summary>
  public abstract class TalentCollection : IEnumerable<Talent>, IEnumerable
  {
    internal Dictionary<TalentId, Talent> ById = new Dictionary<TalentId, Talent>();

    internal readonly int[] m_treePoints = new int[2]
    {
      1,
      2
    };

    internal TalentCollection(Unit unit)
    {
      Owner = unit;
    }

    internal void CalcSpentTalentPoints()
    {
    }

    internal void UpdateTreePoint(uint tabIndex, int diff)
    {
      m_treePoints[tabIndex] += diff;
    }

    public TalentTree[] Trees
    {
      get { return TalentMgr.GetTrees(Owner.Class); }
    }

    public int TotalPointsSpent
    {
      get { return 0; }
    }

    public int Count
    {
      get { return ById.Count; }
    }

    public Unit Owner { get; internal set; }

    /// <summary>
    /// Whether the given talent can be learned by this Character
    /// </summary>
    public bool CanLearn(TalentId id, int rank)
    {
      TalentEntry entry = TalentMgr.GetEntry(id);
      if(entry != null)
        return CanLearn(entry, rank);
      return false;
    }

    /// <summary>
    /// Whether the given talent can be learned by an average player.
    /// Does not check for available Talent points, since that is checked when the Rank is changed.
    /// </summary>
    public virtual bool CanLearn(TalentEntry entry, int rank)
    {
      TalentTree tree = entry.Tree;
      int num = rank - GetRank(entry.Id);
      if(tree.Class != Owner.Class ||
         m_treePoints[tree.TabIndex] < entry.RequiredTreePoints ||
         (rank > entry.Spells.Length || num < 1) || FreeTalentPoints < num)
        return false;
      if(entry.RequiredId == TalentId.None)
        return true;
      Talent talent;
      if(!ById.TryGetValue(entry.RequiredId, out talent))
        return false;
      if(entry.RequiredRank != 0U)
        return talent.Rank >= entry.RequiredRank;
      return true;
    }

    public Talent Learn(TalentId id, int rank)
    {
      TalentEntry entry = TalentMgr.GetEntry(id);
      if(entry != null)
        return Learn(entry, rank);
      return null;
    }

    /// <summary>Tries to learn the given talent on the given rank</summary>
    /// <returns>Whether it was learnt</returns>
    public Talent Learn(TalentEntry entry, int rank)
    {
      if(!CanLearn(entry, rank))
        return null;
      return Set(entry, rank);
    }

    /// <summary>Learn all talents of your own class</summary>
    public void LearnAll()
    {
      LearnAll(Owner.Class);
    }

    /// <summary>Learns all talents of the given class</summary>
    public void LearnAll(ClassId clss)
    {
      int freeTalentPoints = FreeTalentPoints;
      FreeTalentPoints = 300;
      foreach(TalentTree talentTree in TalentMgr.TreesByClass[(int) clss])
      {
        if(talentTree != null)
        {
          foreach(TalentEntry entry in talentTree)
          {
            if(entry != null)
              Learn(entry, entry.MaxRank);
          }
        }
      }

      FreeTalentPoints = freeTalentPoints;
    }

    /// <summary>
    /// Sets the given talent to the given rank without any checks.
    /// Make sure that the given TalentId is valid for this Character's class.
    /// </summary>
    public Talent Set(TalentId id, int rank)
    {
      return Set(TalentMgr.GetEntry(id), rank);
    }

    /// <summary>
    /// Sets the given talent to the given rank without any checks
    /// </summary>
    public Talent Set(TalentEntry entry, int rank)
    {
      Talent talent;
      if(!ById.TryGetValue(entry.Id, out talent))
        ById.Add(entry.Id, talent = new Talent(this, entry, rank));
      else
        talent.Rank = rank;
      return talent;
    }

    internal void AddExisting(TalentEntry entry, int rank)
    {
      Talent talent = new Talent(this, entry);
      rank = Math.Max(0, rank - 1);
      talent.SetRankSilently(rank);
      ById[entry.Id] = talent;
    }

    /// <summary>
    /// Returns the current rank that this player has of this talent
    /// </summary>
    public Talent GetTalent(TalentId id)
    {
      Talent talent;
      ById.TryGetValue(id, out talent);
      return talent;
    }

    /// <summary>
    /// Returns the current rank that this player has of this talent
    /// </summary>
    public int GetRank(TalentId id)
    {
      Talent talent;
      if(ById.TryGetValue(id, out talent))
        return talent.Rank;
      return -1;
    }

    /// <summary>Whether this Owner has a certain Talent.</summary>
    /// <param name="id">The TalentId of the Talent</param>
    /// <returns>True if the Owner has the specified Talent</returns>
    public bool HasTalent(TalentId id)
    {
      return ById.ContainsKey(id);
    }

    /// <summary>Whether this Owner has a certain Talent.</summary>
    /// <param name="talent">The Talent in question.</param>
    /// <returns>True if the Owner has the specified Talent</returns>
    public bool HasTalent(Talent talent)
    {
      return ById.ContainsKey(talent.Entry.Id);
    }

    public bool Remove(TalentId id)
    {
      Talent talent = GetTalent(id);
      if(talent == null)
        return false;
      talent.Remove();
      return true;
    }

    /// <summary>
    /// Removes the given amount of arbitrarily selected talents (always removes higher level talents first)
    /// </summary>
    public void RemoveTalents(int count)
    {
      TalentTree[] trees = Trees;
      for(int index1 = 0; index1 < trees.Length; ++index1)
      {
        TalentTree talentTree = trees[index1];
        while(m_treePoints[index1] > 0 && count > 0)
        {
          for(int index2 = talentTree.TalentTable.Length - 1; index2 >= 0; --index2)
          {
            foreach(TalentEntry talentEntry in talentTree.TalentTable[index2])
            {
              if(talentEntry != null)
              {
                Talent talent = GetTalent(talentEntry.Id);
                if(talent != null)
                {
                  if(count >= talent.ActualRank)
                  {
                    count -= talent.ActualRank;
                    talent.Remove();
                  }
                  else
                  {
                    talent.ActualRank -= count;
                    count = 0;
                    TalentHandler.SendTalentGroupList(this);
                    return;
                  }
                }
              }
            }
          }
        }
      }

      TalentHandler.SendTalentGroupList(this);
    }

    /// <summary>Resets all talents for free</summary>
    public void ResetAllForFree()
    {
      foreach(Talent talent in ById.Values.ToArray())
        talent.Remove();
      ById.Clear();
      FreeTalentPoints = GetFreeTalentPointsForLevel(Owner.Level);
      LastResetTime = DateTime.Now;
    }

    /// <summary>
    /// 
    /// </summary>
    public int CalculateCurrentResetTier()
    {
      if(OwnerCharacter.GodMode)
        return 0;
      int currentResetTier = CurrentResetTier;
      DateTime? lastResetTime = LastResetTime;
      if(!lastResetTime.HasValue)
        return 0;
      int num1 = (int) (DateTime.Now - lastResetTime.Value).TotalHours / ResetTierDecayHours;
      int num2 = currentResetTier - num1;
      if(num2 < 0)
        return 0;
      return num2;
    }

    /// <summary>
    /// The amount of copper that it costs to reset all talents.
    /// Updates the current tier, according to the amount of time passed.
    /// </summary>
    public uint GetResetPrice()
    {
      int index = CalculateCurrentResetTier();
      uint[] resetPricesPerTier = ResetPricesPerTier;
      if(index >= resetPricesPerTier.Length)
        index = resetPricesPerTier.Length - 1;
      return resetPricesPerTier[index];
    }

    /// <summary>
    /// Returns whether reseting succeeded or if it failed (due to not having enough gold)
    /// </summary>
    public bool ResetTalents()
    {
      int index = CalculateCurrentResetTier();
      uint[] resetPricesPerTier = ResetPricesPerTier;
      if(index >= resetPricesPerTier.Length)
        index = resetPricesPerTier.Length - 1;
      uint amount = resetPricesPerTier[index];
      Character ownerCharacter = OwnerCharacter;
      if(amount <= ownerCharacter.Money && !ownerCharacter.GodMode)
        return false;
      ResetAllForFree();
      ownerCharacter.SubtractMoney(amount);
      CurrentResetTier = index + 1;
      ownerCharacter.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.GoldSpentForTalents,
        amount, 0U, null);
      return true;
    }

    /// <summary>
    /// The Owner of this TalentCollection or the owning pet's Master
    /// </summary>
    public abstract Character OwnerCharacter { get; }

    public abstract int FreeTalentPoints { get; set; }

    public virtual int SpecProfileCount
    {
      get { return 1; }
      internal set
      {
        LogManager.GetCurrentClassLogger()
          .Warn("Tried to set Talents.TalentGroupCount for: {0}", Owner);
      }
    }

    /// <summary>
    /// The index of the currently used group of talents (one can have multiple groups due to multi spec)
    /// </summary>
    public abstract int CurrentSpecIndex { get; }

    public abstract uint[] ResetPricesPerTier { get; }

    protected abstract int CurrentResetTier { get; set; }

    public abstract DateTime? LastResetTime { get; set; }

    /// <summary>
    /// Amount of hours that it takes the reset price tier to go down by one
    /// </summary>
    public abstract int ResetTierDecayHours { get; }

    public abstract int GetFreeTalentPointsForLevel(int level);

    public abstract void UpdateFreeTalentPointsSilently(int delta);

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    public IEnumerator<Talent> GetEnumerator()
    {
      return ById.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}