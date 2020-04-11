using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.RealmServer.Entities;
using WCell.Util.Data;

namespace WCell.RealmServer.Achievements
{
    /// <summary>Do not change the layout of this class!</summary>
    [StructLayout(LayoutKind.Sequential)]
    public abstract class AchievementCriteriaEntry
    {
        [NotPersistent] public AchievementCriteriaType Criteria;
        [NotPersistent] public uint AchievementCriteriaId;
        [NotPersistent] public uint AchievementEntryId;
        [NotPersistent] public uint CompletionFlag;
        [NotPersistent] public AchievementCriteriaGroupFlags GroupFlag;
        [NotPersistent] public uint TimeLimit;

        [NotPersistent] public AchievementCriteriaRequirementSet RequirementSet { get; internal set; }

        public AchievementEntry AchievementEntry
        {
            get { return AchievementMgr.GetAchievementEntry(this.AchievementEntryId); }
        }

        public virtual bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return false;
        }

        public virtual void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
        }

        public override string ToString()
        {
            return this.Criteria.ToString();
        }
    }
}