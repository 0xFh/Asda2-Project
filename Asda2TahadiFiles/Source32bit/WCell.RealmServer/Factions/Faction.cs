using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Factions;

namespace WCell.RealmServer.Factions
{
  /// <summary>
  /// TODO: Load faction info completely at startup to avoid synchronization issues
  /// </summary>
  [Serializable]
  public class Faction
  {
    public static readonly HashSet<Faction> EmptySet = new HashSet<Faction>();
    public static readonly Faction NullFaction = new Faction(EmptySet, EmptySet, EmptySet);
    public readonly HashSet<Faction> Enemies = new HashSet<Faction>();
    public readonly HashSet<Faction> Friends = new HashSet<Faction>();
    public readonly HashSet<Faction> Neutrals = new HashSet<Faction>();
    public List<Faction> Children = new List<Faction>();
    public FactionGroup Group;
    public FactionEntry Entry;
    public readonly FactionTemplateEntry Template;
    public FactionId Id;
    public FactionReputationIndex ReputationIndex;

    /// <summary>whether this is a Player faction</summary>
    public bool IsPlayer;

    /// <summary>whether this is the Alliance or an Alliance faction</summary>
    public bool IsAlliance;

    /// <summary>whether this is the Horde or a Horde faction</summary>
    public bool IsHorde;

    /// <summary>
    /// whether this is a neutral faction (always stays neutral).
    /// </summary>
    public bool IsNeutral;

    /// <summary>
    /// Default ctor can be used for customizing your own Faction
    /// </summary>
    public Faction()
    {
      Entry = new FactionEntry
      {
        Name = "Null Faction"
      };
      Template = new FactionTemplateEntry
      {
        EnemyFactions = new FactionId[0],
        FriendlyFactions = new FactionId[0]
      };
    }

    private Faction(HashSet<Faction> enemies, HashSet<Faction> friends, HashSet<Faction> neutrals)
    {
      Enemies = enemies;
      Friends = friends;
      Neutrals = neutrals;
      Entry = new FactionEntry
      {
        Name = "Null Faction"
      };
      Template = new FactionTemplateEntry
      {
        EnemyFactions = new FactionId[0],
        FriendlyFactions = new FactionId[0]
      };
    }

    public Faction(FactionEntry entry, FactionTemplateEntry template)
    {
      Entry = entry;
      Template = template;
      Id = entry.Id;
      ReputationIndex = entry.FactionIndex;
      IsAlliance = template.FactionGroup.HasFlag(FactionGroupMask.Alliance);
      IsHorde = template.FactionGroup.HasFlag(FactionGroupMask.Horde);
    }

    internal void Init()
    {
      if(Id == FactionId.Alliance || Entry.ParentId == FactionId.Alliance)
      {
        IsAlliance = true;
        Group = FactionGroup.Alliance;
      }
      else if(Id == FactionId.Horde || Entry.ParentId == FactionId.Horde)
      {
        IsHorde = true;
        Group = FactionGroup.Horde;
      }

      foreach(Faction faction in FactionMgr.ByTemplateId.Where(
        faction => faction != null))
      {
        if(IsPlayer && faction.Template.FriendGroup.HasAnyFlag(FactionGroupMask.Player))
        {
          Friends.Add(faction);
          faction.Friends.Add(this);
        }

        if(Template.FriendGroup.HasAnyFlag(faction.Template.FactionGroup))
          Friends.Add(faction);
      }

      foreach(FactionId friendlyFaction in Template.FriendlyFactions)
      {
        Faction faction = FactionMgr.Get(friendlyFaction);
        if(faction != null)
        {
          Friends.Add(faction);
          faction.Friends.Add(this);
        }
      }

      Friends.Add(this);
      foreach(Faction faction in FactionMgr.ByTemplateId.Where(
        faction => faction != null))
      {
        if(IsPlayer && faction.Template.EnemyGroup.HasAnyFlag(FactionGroupMask.Player))
        {
          Enemies.Add(faction);
          faction.Enemies.Add(this);
        }

        if(Template.EnemyGroup.HasAnyFlag(faction.Template.FactionGroup))
          Enemies.Add(faction);
      }

      foreach(FactionId enemyFaction in Template.EnemyFactions)
      {
        Faction faction = FactionMgr.Get(enemyFaction);
        if(faction != null)
        {
          if(!Template.Flags.HasAnyFlag(FactionTemplateFlags.Flagx400))
            Enemies.Add(faction);
          faction.Enemies.Add(this);
        }
      }

      foreach(Faction faction in FactionMgr.ByTemplateId.Where(
        faction => faction != null))
      {
        if(!Friends.Contains(faction) && !Enemies.Contains(faction))
          Neutrals.Add(faction);
      }

      if(Id == FactionId.Prey)
        Enemies.Clear();
      IsNeutral = Enemies.Count == 0 && Friends.Count == 0;
    }

    /// <summary>Make this an alliance player faction</summary>
    internal void SetAlliancePlayer()
    {
      IsPlayer = true;
      Entry.ParentId = FactionId.Alliance;
      FactionMgr.AlliancePlayerFactions.Add(this);
    }

    /// <summary>Make this a horde player faction</summary>
    internal void SetHordePlayer()
    {
      IsPlayer = true;
      Entry.ParentId = FactionId.Horde;
      FactionMgr.HordePlayerFactions.Add(this);
    }

    public bool IsHostileTowards(Faction otherFaction)
    {
      if(Enemies.Contains(otherFaction))
        return true;
      if(Template.EnemyFactions.Length > 0)
      {
        for(int index = 0; index < Template.EnemyFactions.Length; ++index)
        {
          if(Template.EnemyFactions[index] == otherFaction.Template.FactionId)
            return true;
          if(Template.FriendlyFactions[index] == otherFaction.Template.FactionId)
            return false;
        }
      }

      return Template.EnemyGroup.HasAnyFlag(otherFaction.Template.FactionGroup);
    }

    public bool IsNeutralWith(Faction otherFaction)
    {
      return Neutrals.Contains(otherFaction);
    }

    public bool IsFriendlyTowards(Faction otherFaction)
    {
      if(Friends.Contains(otherFaction))
        return true;
      if(Template.EnemyFactions.Length > 0)
      {
        for(int index = 0; index < Template.FriendlyFactions.Length; ++index)
        {
          if(Template.FriendlyFactions[index] == otherFaction.Template.FactionId)
            return true;
          if(Template.EnemyFactions[index] == otherFaction.Template.FactionId)
            return false;
        }
      }

      return Template.FriendGroup.HasAnyFlag(otherFaction.Template.FactionGroup);
    }

    public override int GetHashCode()
    {
      return (int) Id;
    }

    public override bool Equals(object obj)
    {
      if(obj is Faction)
        return Id == ((Faction) obj).Id;
      return false;
    }

    public override string ToString()
    {
      return Entry.Name + " (" + (int) Id + ")";
    }
  }
}