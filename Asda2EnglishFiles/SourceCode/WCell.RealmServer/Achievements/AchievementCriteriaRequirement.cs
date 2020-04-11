using WCell.Constants.Achievements;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.Util.Data;

namespace WCell.RealmServer.Achievements
{
    [DependingProducer(AchievementCriteriaRequirementType.BgLossTeamScore,
        typeof(AchievementCriteriaRequirementBgLossTeamScore))]
    [DependingProducer(AchievementCriteriaRequirementType.Disabled, typeof(AchievementCriteriaRequirementDisabled))]
    [DependingProducer(AchievementCriteriaRequirementType.MapDifficulty,
        typeof(AchievementCriteriaRequirementMapDifficulty))]
    [DependingProducer(AchievementCriteriaRequirementType.NthBirthday,
        typeof(AchievementCriteriaRequirementNthBirthday))]
    [DependingProducer(AchievementCriteriaRequirementType.None, typeof(AchievementCriteriaRequirement))]
    [DependingProducer(AchievementCriteriaRequirementType.EquippedItemLevel,
        typeof(AchievementCriteriaRequirementEquippedItemLevel))]
    [DependingProducer(AchievementCriteriaRequirementType.PlayerClassRace,
        typeof(AchievementCriteriaRequirementPlayerClassRace))]
    [DependingProducer(AchievementCriteriaRequirementType.PlayerLessHealth,
        typeof(AchievementCriteriaRequirementPlayerLessHealth))]
    [DependingProducer(AchievementCriteriaRequirementType.PlayerDead, typeof(AchievementCriteriaRequirementPlayerDead))]
    [DependingProducer(AchievementCriteriaRequirementType.Aura1, typeof(AchievementCriteriaRequirementAura1))]
    [DependingProducer(AchievementCriteriaRequirementType.Area, typeof(AchievementCriteriaRequirementArea))]
    [DependingProducer(AchievementCriteriaRequirementType.Aura2, typeof(AchievementCriteriaRequirementAura2))]
    [DependingProducer(AchievementCriteriaRequirementType.Value, typeof(AchievementCriteriaRequirementValue))]
    [DependingProducer(AchievementCriteriaRequirementType.Level, typeof(AchievementCriteriaRequirementLevel))]
    [DependingProducer(AchievementCriteriaRequirementType.Gender, typeof(AchievementCriteriaRequirementGender))]
    [DependingProducer(AchievementCriteriaRequirementType.KnownTitle, typeof(AchievementCriteriaRequirementKnownTitle))]
    [DependingProducer(AchievementCriteriaRequirementType.Creature, typeof(AchievementCriteriaRequirementCreature))]
    [DependingProducer(AchievementCriteriaRequirementType.MapPlayerCount,
        typeof(AchievementCriteriaRequirementMapPlayerCount))]
    [DependingProducer(AchievementCriteriaRequirementType.Team, typeof(AchievementCriteriaRequirementTeam))]
    [DependingProducer(AchievementCriteriaRequirementType.Drunk, typeof(AchievementCriteriaRequirementDrunk))]
    [DependingProducer(AchievementCriteriaRequirementType.Holiday, typeof(AchievementCriteriaRequirementHoliday))]
    [DependingProducer(AchievementCriteriaRequirementType.InstanceScript,
        typeof(AchievementCriteriaRequirementInstanceScript))]
    public class AchievementCriteriaRequirement : IDataHolder
    {
        public uint CriteriaId;
        public AchievementCriteriaRequirementType Type;
        public uint Value1;
        public uint Value2;

        public void FinalizeDataHolder()
        {
            AchievementCriteriaEntry criteriaEntryById = AchievementMgr.GetCriteriaEntryById(this.CriteriaId);
            if (criteriaEntryById == null)
                ContentMgr.OnInvalidDBData("{0} had an invalid criteria id.", (object) this);
            else
                criteriaEntryById.RequirementSet.Add(this);
        }

        public virtual bool Meets(Character chr, Unit target, uint miscValue)
        {
            return true;
        }
    }
}