using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.NPCs;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.Factions
{
  public static class FactionMgr
  {
    public static readonly List<Faction> AlliancePlayerFactions = new List<Faction>(5);
    public static readonly List<Faction> HordePlayerFactions = new List<Faction>(5);
    public static readonly Faction[] ByRace = new Faction[(uint) (Utility.GetMaxEnum<RaceId>() + 1)];
    [NotVariable]public static Faction[] ById = new Faction[(uint) (Utility.GetMaxEnum<FactionId>() + 1U)];

    [NotVariable]public static Faction[] ByReputationIndex =
      new Faction[(uint) (Utility.GetMaxEnum<FactionReputationIndex>() + 1)];

    [NotVariable]public static Faction[] ByTemplateId = new Faction[4000];

    public static readonly Dictionary<FactionId, FactionEntry> FactionEntries =
      new Dictionary<FactionId, FactionEntry>();

    public static readonly Dictionary<uint, FactionTemplateEntry> FactionTplEntries =
      new Dictionary<uint, FactionTemplateEntry>();

    private const uint MaxTemplateId = 4000;
    private static bool initialized;

    [Initialization(InitializationPass.Second, "Initialize Factions")]
    public static void Initialize()
    {
      if(initialized)
        return;
      initialized = true;
    }

    private static void InitFactionDBC()
    {
      foreach(FactionEntry factionEntry in new MappedDBCReader<FactionEntry, FactionConverter>(
        RealmServerConfiguration.GetDBCFile("Faction.dbc")).Entries.Values)
        FactionEntries[factionEntry.Id] = factionEntry;
    }

    private static void InitFactionTemplateDBC()
    {
      foreach(FactionTemplateEntry template in
        new MappedDBCReader<FactionTemplateEntry, FactionTemplateConverter>(
          RealmServerConfiguration.GetDBCFile("FactionTemplate.dbc")).Entries.Values)
      {
        FactionTplEntries[template.Id] = template;
        if(template.FactionId != FactionId.None)
        {
          FactionEntry factionEntry = FactionEntries[template.FactionId];
          Faction val = new Faction(factionEntry, template);
          ArrayUtil.Set(ref ByTemplateId, template.Id, val);
          if(Get(template.FactionId) == null)
          {
            ArrayUtil.Set(ref ById, (uint) factionEntry.Id, val);
            if(factionEntry.FactionIndex > FactionReputationIndex.None)
              ArrayUtil.Set(ref ByReputationIndex, (uint) factionEntry.FactionIndex,
                val);
          }
        }
      }

      (ByRace[1] = ById[1]).SetAlliancePlayer();
      (ByRace[3] = ById[3]).SetAlliancePlayer();
      (ByRace[4] = ById[4]).SetAlliancePlayer();
      (ByRace[7] = ById[8]).SetAlliancePlayer();
      (ByRace[11] = ById[927]).SetAlliancePlayer();
      (ByRace[2] = ById[2]).SetHordePlayer();
      (ByRace[5] = ById[5]).SetHordePlayer();
      (ByRace[6] = ById[6]).SetHordePlayer();
      (ByRace[8] = ById[9]).SetHordePlayer();
      (ByRace[10] = ById[914]).SetHordePlayer();
      foreach(Faction faction1 in ById)
      {
        if(faction1 != null)
        {
          faction1.Init();
          if(faction1.Entry.ParentId != FactionId.None)
          {
            Faction faction2 = Get(faction1.Entry.ParentId);
            if(faction2 != null)
              faction2.Children.Add(faction1);
          }
        }
      }
    }

    public static Faction Get(FactionReputationIndex repuataionIndex)
    {
      return NPCMgr.DefaultFaction;
    }

    public static Faction Get(FactionId id)
    {
      return NPCMgr.DefaultFaction;
    }

    public static Faction Get(FactionTemplateId id)
    {
      return NPCMgr.DefaultFaction;
    }

    public static Faction Get(RaceId race)
    {
      return NPCMgr.DefaultFaction;
    }

    public static FactionId GetId(FactionReputationIndex reputationIndex)
    {
      return FactionId.None;
    }

    public static FactionReputationIndex GetFactionIndex(FactionId id)
    {
      return FactionReputationIndex.None;
    }

    /// <summary>
    /// Returns the FactionGroup of the given race.
    /// Throws KeyNotFoundException if race is not a valid player-race.
    /// </summary>
    /// <param name="race">the race</param>
    /// <returns>The FactionGroup of the race.</returns>
    public static FactionGroup GetFactionGroup(RaceId race)
    {
      return FactionGroup.Alliance;
    }
  }
}