using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Achievements;
using WCell.Core.Initialization;
using WCell.RealmServer.Asda2Titles;
using WCell.RealmServer.Content;

namespace WCell.RealmServer.Achievements
{
    /// <summary>Global container for Achievement-related data</summary>
    public static class AchievementMgr
    {
        private static readonly AchievementCriteriaEntryCreator[] AchievementEntryCreators =
            new AchievementCriteriaEntryCreator[124];

        private static readonly AchievementCriteriaRequirementCreator[] AchievementCriteriaRequirementCreators =
            new AchievementCriteriaRequirementCreator[22];

        public static readonly List<AchievementCriteriaEntry>[] CriteriaEntriesByType =
            new List<AchievementCriteriaEntry>[124];

        public static readonly Dictionary<uint, AchievementCriteriaEntry> CriteriaEntriesById =
            new Dictionary<uint, AchievementCriteriaEntry>();

        public static readonly Dictionary<uint, AchievementEntry> AchievementEntries =
            new Dictionary<uint, AchievementEntry>();

        public static readonly Dictionary<AchievementCategoryEntryId, AchievementCategoryEntry>
            AchievementCategoryEntries = new Dictionary<AchievementCategoryEntryId, AchievementCategoryEntry>();

        public static readonly List<uint> CompletedRealmFirstAchievements = new List<uint>();

        [WCell.Core.Initialization.Initialization(InitializationPass.Fifth, "Initialize Achievements")]
        public static void InitAchievements()
        {
            ContentMgr.Load<Asda2TitleTemplate>();
        }

        public static void LoadRealmFirstAchievements()
        {
            foreach (AchievementRecord achievementRecord in AchievementRecord.Load(AchievementMgr.AchievementEntries
                .Values
                .Where<AchievementEntry
                >((Func<AchievementEntry, bool>) (achievementEntry => achievementEntry.IsRealmFirstType()))
                .Select<AchievementEntry, uint>(
                    (Func<AchievementEntry, uint>) (achievementEntry => achievementEntry.ID)).ToArray<uint>()))
                AchievementMgr.CompletedRealmFirstAchievements.Add(achievementRecord.AchievementEntryId);
        }

        public static List<AchievementCriteriaEntry> GetCriteriaEntriesByType(AchievementCriteriaType type)
        {
            return AchievementMgr.CriteriaEntriesByType[(int) type];
        }

        public static AchievementCriteriaEntry GetCriteriaEntryById(uint id)
        {
            AchievementCriteriaEntry achievementCriteriaEntry;
            AchievementMgr.CriteriaEntriesById.TryGetValue(id, out achievementCriteriaEntry);
            return achievementCriteriaEntry;
        }

        public static AchievementCriteriaEntryCreator GetCriteriaEntryCreator(AchievementCriteriaType criteria)
        {
            return AchievementMgr.AchievementEntryCreators[(int) criteria];
        }

        public static void SetEntryCreator(AchievementCriteriaType criteria, AchievementCriteriaEntryCreator creator)
        {
            AchievementMgr.AchievementEntryCreators[(int) criteria] = creator;
        }

        public static void InitCriteria()
        {
            for (int index = 0; index < AchievementMgr.CriteriaEntriesByType.Length; ++index)
                AchievementMgr.CriteriaEntriesByType[index] = new List<AchievementCriteriaEntry>();
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.KillCreature,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new KillCreatureAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.WinBg,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new WinBattleGroundAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.ReachLevel,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new ReachLevelAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.ReachSkillLevel,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new ReachSkillLevelAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.CompleteAchievement,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new CompleteAchievementAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.CompleteQuestCount,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new CompleteQuestCountAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.CompleteDailyQuestDaily,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new CompleteDailyQuestDailyAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.CompleteQuestsInZone,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new CompleteQuestsInZoneAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.CompleteDailyQuest,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new CompleteDailyQuestAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.CompleteBattleground,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new CompleteBattlegroundAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.DeathAtMap,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new DeathAtMapAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.DeathInDungeon,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new DeathInDungeonAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.CompleteRaid,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new CompleteRaidAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.KilledByCreature,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new KilledByCreatureAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.FallWithoutDying,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new FallWithoutDyingAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.DeathsFrom,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new DeathsFromAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.CompleteQuest,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new CompleteQuestAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.BeSpellTarget,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new BeSpellTargetAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.BeSpellTarget2,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new BeSpellTargetAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.CastSpell,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new CastSpellAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.HonorableKillAtArea,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new HonorableKillAtAreaAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.WinArena,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new WinArenaAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.PlayArena,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new PlayArenaAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.LearnSpell,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new LearnSpellAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.OwnItem,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new OwnItemAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.WinRatedArena,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new WinRatedArenaAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.HighestTeamRating,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new HighestTeamRatingAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.ReachTeamRating,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new ReachTeamRatingAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.LearnSkillLevel,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new LearnSkillLevelAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.LootItem,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new LootItemAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.ExploreArea,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new ExploreAreaAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.BuyBankSlot,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new BuyBankSlotAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.GainReputation,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new GainReputationAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.GainExaltedReputation,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new GainExaltedReputationAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.VisitBarberShop,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new VisitBarberShopAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.DoEmote,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new DoEmoteAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.MoneyFromVendors,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new IncrementAtValue1AchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.GoldSpentForTalents,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new IncrementAtValue1AchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.MoneyFromQuestReward,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new IncrementAtValue1AchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.GoldSpentForTravelling,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new IncrementAtValue1AchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.GoldSpentAtBarber,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new IncrementAtValue1AchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.GoldSpentForMail,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new IncrementAtValue1AchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.LootMoney,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new IncrementAtValue1AchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.WinDuel,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new WinDuelLevelAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.LoseDuel,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new LoseDuelLevelAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.GainReveredReputation,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new GainReveredReputationAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.GainHonoredReputation,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new GainHonoredReputationAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.KnownFactions,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new KnownFactionsAchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.TotalDamageReceived,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new IncrementAtValue1AchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.TotalHealingReceived,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new IncrementAtValue1AchievementCriteriaEntry()));
            AchievementMgr.SetEntryCreator(AchievementCriteriaType.FlightPathsTaken,
                (AchievementCriteriaEntryCreator) (() =>
                    (AchievementCriteriaEntry) new FlightPathsTakenAchievementCriteriaEntry()));
        }

        private static void LoadDBCs()
        {
        }

        public static AchievementCriteriaRequirementCreator GetCriteriaRequirementCreator(
            AchievementCriteriaRequirementType type)
        {
            return AchievementMgr.AchievementCriteriaRequirementCreators[(int) type];
        }

        public static void SetRequirementCreator(AchievementCriteriaRequirementType type,
            AchievementCriteriaRequirementCreator creator)
        {
            AchievementMgr.AchievementCriteriaRequirementCreators[(int) type] = creator;
        }

        public static void InitCriteriaRequirements()
        {
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.None,
                (AchievementCriteriaRequirementCreator) (() => new AchievementCriteriaRequirement()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.Creature,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementCreature()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.PlayerClassRace,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementPlayerClassRace()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.PlayerLessHealth,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementPlayerLessHealth()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.PlayerDead,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementPlayerDead()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.Aura1,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementAura1()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.Area,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementArea()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.Aura2,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementAura2()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.Value,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementValue()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.Gender,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementGender()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.Disabled,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementDisabled()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.MapDifficulty,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementMapDifficulty()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.MapPlayerCount,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementMapPlayerCount()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.Team,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementTeam()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.Drunk,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementDrunk()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.Holiday,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementHoliday()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.BgLossTeamScore,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementBgLossTeamScore()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.InstanceScript,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementInstanceScript()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.EquippedItemLevel,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementEquippedItemLevel()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.NthBirthday,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementNthBirthday()));
            AchievementMgr.SetRequirementCreator(AchievementCriteriaRequirementType.KnownTitle,
                (AchievementCriteriaRequirementCreator) (() =>
                    (AchievementCriteriaRequirement) new AchievementCriteriaRequirementKnownTitle()));
        }

        public static AchievementEntry GetAchievementEntry(uint achievementEntryId)
        {
            AchievementEntry achievementEntry;
            AchievementMgr.AchievementEntries.TryGetValue(achievementEntryId, out achievementEntry);
            return achievementEntry;
        }

        public static AchievementCategoryEntry GetCategoryEntry(AchievementCategoryEntryId achievementCategoryEntryId)
        {
            AchievementCategoryEntry achievementCategoryEntry;
            AchievementMgr.AchievementCategoryEntries.TryGetValue(achievementCategoryEntryId,
                out achievementCategoryEntry);
            return achievementCategoryEntry;
        }

        public static AchievementCategoryEntry GetCriteria(AchievementCategoryEntryId achievementCategoryEntryId)
        {
            return AchievementMgr.AchievementCategoryEntries[achievementCategoryEntryId];
        }

        /// <summary>
        /// </summary>
        /// <param name="achievementEntryId">Achievement entry</param>
        /// <returns>Return false only if the achievement has RealmFirst flag and already achieved by someone</returns>
        public static bool IsRealmFirst(uint achievementEntryId)
        {
            return !AchievementMgr.CompletedRealmFirstAchievements.Contains(achievementEntryId);
        }
    }
}