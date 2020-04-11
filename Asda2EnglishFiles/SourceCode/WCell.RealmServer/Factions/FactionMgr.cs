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
        [NotVariable] public static Faction[] ById = new Faction[(uint) (Utility.GetMaxEnum<FactionId>() + 1U)];

        [NotVariable] public static Faction[] ByReputationIndex =
            new Faction[(uint) (Utility.GetMaxEnum<FactionReputationIndex>() + 1)];

        [NotVariable] public static Faction[] ByTemplateId = new Faction[4000];

        public static readonly Dictionary<FactionId, FactionEntry> FactionEntries =
            new Dictionary<FactionId, FactionEntry>();

        public static readonly Dictionary<uint, FactionTemplateEntry> FactionTplEntries =
            new Dictionary<uint, FactionTemplateEntry>();

        private const uint MaxTemplateId = 4000;
        private static bool initialized;

        [WCell.Core.Initialization.Initialization(InitializationPass.Second, "Initialize Factions")]
        public static void Initialize()
        {
            if (FactionMgr.initialized)
                return;
            FactionMgr.initialized = true;
        }

        private static void InitFactionDBC()
        {
            foreach (FactionEntry factionEntry in new MappedDBCReader<FactionEntry, FactionConverter>(
                RealmServerConfiguration.GetDBCFile("Faction.dbc")).Entries.Values)
                FactionMgr.FactionEntries[factionEntry.Id] = factionEntry;
        }

        private static void InitFactionTemplateDBC()
        {
            foreach (FactionTemplateEntry template in
                new MappedDBCReader<FactionTemplateEntry, FactionTemplateConverter>(
                    RealmServerConfiguration.GetDBCFile("FactionTemplate.dbc")).Entries.Values)
            {
                FactionMgr.FactionTplEntries[template.Id] = template;
                if (template.FactionId != FactionId.None)
                {
                    FactionEntry factionEntry = FactionMgr.FactionEntries[template.FactionId];
                    Faction val = new Faction(factionEntry, template);
                    ArrayUtil.Set<Faction>(ref FactionMgr.ByTemplateId, template.Id, val);
                    if (FactionMgr.Get(template.FactionId) == null)
                    {
                        ArrayUtil.Set<Faction>(ref FactionMgr.ById, (uint) factionEntry.Id, val);
                        if (factionEntry.FactionIndex > FactionReputationIndex.None)
                            ArrayUtil.Set<Faction>(ref FactionMgr.ByReputationIndex, (uint) factionEntry.FactionIndex,
                                val);
                    }
                }
            }

            (FactionMgr.ByRace[1] = FactionMgr.ById[1]).SetAlliancePlayer();
            (FactionMgr.ByRace[3] = FactionMgr.ById[3]).SetAlliancePlayer();
            (FactionMgr.ByRace[4] = FactionMgr.ById[4]).SetAlliancePlayer();
            (FactionMgr.ByRace[7] = FactionMgr.ById[8]).SetAlliancePlayer();
            (FactionMgr.ByRace[11] = FactionMgr.ById[927]).SetAlliancePlayer();
            (FactionMgr.ByRace[2] = FactionMgr.ById[2]).SetHordePlayer();
            (FactionMgr.ByRace[5] = FactionMgr.ById[5]).SetHordePlayer();
            (FactionMgr.ByRace[6] = FactionMgr.ById[6]).SetHordePlayer();
            (FactionMgr.ByRace[8] = FactionMgr.ById[9]).SetHordePlayer();
            (FactionMgr.ByRace[10] = FactionMgr.ById[914]).SetHordePlayer();
            foreach (Faction faction1 in FactionMgr.ById)
            {
                if (faction1 != null)
                {
                    faction1.Init();
                    if (faction1.Entry.ParentId != FactionId.None)
                    {
                        Faction faction2 = FactionMgr.Get(faction1.Entry.ParentId);
                        if (faction2 != null)
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