using Castle.ActiveRecord;
using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Achievements
{
    /// <summary>Represents the Player's Achievements.</summary>
    public class AchievementCollection
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public static bool DisableStaffAchievements = true;
        public static readonly uint[] ClassSpecificAchievementId = new uint[12];
        public static readonly uint[] RaceSpecificAchievementId = new uint[16];

        internal Dictionary<uint, AchievementRecord>
            m_completedAchievements = new Dictionary<uint, AchievementRecord>();

        internal Dictionary<uint, AchievementProgressRecord> m_progressRecords =
            new Dictionary<uint, AchievementProgressRecord>();

        internal Character m_owner;

        public AchievementCollection(Character chr)
        {
            this.m_owner = chr;
            AchievementCollection.ClassSpecificAchievementId[0] = 0U;
            AchievementCollection.ClassSpecificAchievementId[1] = 459U;
            AchievementCollection.ClassSpecificAchievementId[2] = 465U;
            AchievementCollection.ClassSpecificAchievementId[3] = 462U;
            AchievementCollection.ClassSpecificAchievementId[4] = 458U;
            AchievementCollection.ClassSpecificAchievementId[5] = 464U;
            AchievementCollection.ClassSpecificAchievementId[6] = 461U;
            AchievementCollection.ClassSpecificAchievementId[7] = 467U;
            AchievementCollection.ClassSpecificAchievementId[8] = 460U;
            AchievementCollection.ClassSpecificAchievementId[9] = 463U;
            AchievementCollection.ClassSpecificAchievementId[11] = 466U;
            AchievementCollection.RaceSpecificAchievementId[0] = 0U;
            AchievementCollection.RaceSpecificAchievementId[1] = 1408U;
            AchievementCollection.RaceSpecificAchievementId[2] = 1410U;
            AchievementCollection.RaceSpecificAchievementId[3] = 1407U;
            AchievementCollection.RaceSpecificAchievementId[4] = 1409U;
            AchievementCollection.RaceSpecificAchievementId[5] = 1413U;
            AchievementCollection.RaceSpecificAchievementId[6] = 1411U;
            AchievementCollection.RaceSpecificAchievementId[7] = 1404U;
            AchievementCollection.RaceSpecificAchievementId[8] = 1412U;
            AchievementCollection.RaceSpecificAchievementId[9] = 0U;
            AchievementCollection.RaceSpecificAchievementId[10] = 1405U;
            AchievementCollection.RaceSpecificAchievementId[11] = 1406U;
            AchievementCollection.RaceSpecificAchievementId[12] = 0U;
            AchievementCollection.RaceSpecificAchievementId[13] = 0U;
            AchievementCollection.RaceSpecificAchievementId[14] = 0U;
            AchievementCollection.RaceSpecificAchievementId[15] = 0U;
        }

        /// <summary>Checks if player has completed the given achievement.</summary>
        /// <param name="achievementEntry"></param>
        /// <returns></returns>
        public bool HasCompleted(uint achievementEntry)
        {
            return this.m_completedAchievements.ContainsKey(achievementEntry);
        }

        /// <summary>Returns progress with given achievement's criteria</summary>
        /// <param name="achievementCriteriaId"></param>
        /// <returns></returns>
        public AchievementProgressRecord GetAchievementCriteriaProgress(uint achievementCriteriaId)
        {
            AchievementProgressRecord achievementProgressRecord;
            this.m_progressRecords.TryGetValue(achievementCriteriaId, out achievementProgressRecord);
            return achievementProgressRecord;
        }

        /// <summary>Returns the Achievement's Owner.</summary>
        public Character Owner
        {
            get { return this.m_owner; }
        }

        /// <summary>Returns the amount of completed achievements.</summary>
        public int AchievementsCount
        {
            get { return this.m_completedAchievements.Count; }
        }

        /// <summary>Checks if the given achievement is completable.</summary>
        /// <param name="achievementEntry"></param>
        /// <returns></returns>
        public bool IsAchievementCompletable(AchievementEntry achievementEntry)
        {
            if (achievementEntry.Flags.HasFlag((Enum) AchievementFlags.Counter) ||
                !AchievementMgr.IsRealmFirst(achievementEntry.ID))
                return false;
            if (achievementEntry.RefAchievement == 0U)
            {
                int id = (int) achievementEntry.ID;
            }
            else
            {
                int refAchievement = (int) achievementEntry.RefAchievement;
            }

            uint count = achievementEntry.Count;
            List<AchievementCriteriaEntry> criteria = achievementEntry.Criteria;
            if (criteria.Count == 0)
                return false;
            uint num = 0;
            bool flag = true;
            foreach (AchievementCriteriaEntry achievementCriteriaEntry in criteria)
            {
                if (this.IsCriteriaCompletable(achievementCriteriaEntry))
                    ++num;
                else
                    flag = false;
                if (count > 0U && count <= num)
                    return true;
            }

            if (flag)
                return count == 0U;
            return false;
        }

        /// <summary>Checks if the given criteria is completable</summary>
        /// <param name="achievementCriteriaEntry"></param>
        /// <returns></returns>
        public bool IsCriteriaCompletable(AchievementCriteriaEntry achievementCriteriaEntry)
        {
            if (achievementCriteriaEntry.AchievementEntry.Flags.HasFlag((Enum) AchievementFlags.Counter))
                return false;
            AchievementProgressRecord criteriaProgress =
                this.m_owner.Achievements.GetAchievementCriteriaProgress(achievementCriteriaEntry
                    .AchievementCriteriaId);
            if (criteriaProgress == null)
                return false;
            return achievementCriteriaEntry.IsAchieved(criteriaProgress);
        }

        /// <summary>Adds a new achievement to the list.</summary>
        /// <param name="achievementRecord"></param>
        public void AddAchievement(AchievementRecord achievementRecord)
        {
            this.m_completedAchievements.Add(achievementRecord.AchievementEntryId, achievementRecord);
        }

        /// <summary>
        /// Adds a new achievement to the list, when achievement is earned.
        /// </summary>
        /// <param name="achievementEntry"></param>
        public void EarnAchievement(uint achievementEntryId)
        {
            AchievementEntry achievementEntry = AchievementMgr.GetAchievementEntry(achievementEntryId);
            if (achievementEntry == null)
                return;
            this.EarnAchievement(achievementEntry);
        }

        /// <summary>
        /// Adds a new achievement to the list, when achievement is earned.
        /// </summary>
        /// <param name="achievementEntry"></param>
        public void EarnAchievement(AchievementEntry achievement)
        {
            this.AddAchievement(AchievementRecord.CreateNewAchievementRecord(this.m_owner, achievement.ID));
            this.CheckPossibleAchievementUpdates(AchievementCriteriaType.CompleteAchievement, achievement.ID, 1U,
                (Unit) null);
            this.RemoveAchievementProgress(achievement);
            foreach (AchievementReward reward in achievement.Rewards)
                reward.GiveReward(this.Owner);
            if (this.m_owner.IsInGuild)
                this.m_owner.Guild.Broadcast(
                    AchievementHandler.CreateAchievementEarnedToGuild(achievement.ID, this.m_owner));
            if (achievement.IsRealmFirstType())
                AchievementHandler.SendServerFirstAchievement(achievement.ID, this.m_owner);
            AchievementHandler.SendAchievementEarned(achievement.ID, this.m_owner);
        }

        /// <summary>
        /// Returns the corresponding ProgressRecord. Creates a new one if the player doesn't have progress record.
        /// </summary>
        /// <param name="achievementCriteriaId"></param>
        /// <returns></returns>
        internal AchievementProgressRecord GetOrCreateProgressRecord(uint achievementCriteriaId)
        {
            AchievementProgressRecord achievementProgressRecord;
            if (!this.m_progressRecords.TryGetValue(achievementCriteriaId, out achievementProgressRecord))
            {
                achievementProgressRecord =
                    AchievementProgressRecord.CreateAchievementProgressRecord(this.Owner, achievementCriteriaId, 0U);
                this.AddProgressRecord(achievementProgressRecord);
            }

            return achievementProgressRecord;
        }

        /// <summary>Adds a new progress record to the list.</summary>
        /// <param name="achievementProgressRecord"></param>
        private void AddProgressRecord(AchievementProgressRecord achievementProgressRecord)
        {
            this.m_progressRecords.Add(achievementProgressRecord.AchievementCriteriaId, achievementProgressRecord);
        }

        /// <summary>Removes achievement from the player.</summary>
        /// <param name="achievementEntryId"></param>
        /// <returns></returns>
        public bool RemoveAchievement(uint achievementEntryId)
        {
            AchievementRecord achievementRecord;
            if (!this.m_completedAchievements.TryGetValue(achievementEntryId, out achievementRecord))
                return false;
            this.RemoveAchievement(achievementRecord);
            return true;
        }

        /// <summary>Removes achievement from the player.</summary>
        /// <param name="achievementRecord"></param>
        public void RemoveAchievement(AchievementRecord achievementRecord)
        {
            this.m_completedAchievements.Remove(achievementRecord.AchievementEntryId);
        }

        /// <summary>Removes criteria progress from the player.</summary>
        /// <param name="achievementCriteriaId"></param>
        /// <returns></returns>
        public bool RemoveProgress(uint achievementCriteriaId)
        {
            AchievementProgressRecord achievementProgressRecord;
            if (!this.m_progressRecords.TryGetValue(achievementCriteriaId, out achievementProgressRecord))
                return false;
            this.RemoveProgress(achievementProgressRecord);
            return true;
        }

        /// <summary>Removes criteria progress from the player.</summary>
        /// <param name="achievementProgressRecord"></param>
        public void RemoveProgress(AchievementProgressRecord achievementProgressRecord)
        {
            this.m_progressRecords.Remove(achievementProgressRecord.AchievementCriteriaId);
        }

        /// <summary>Removes all the progress of a given achievement.</summary>
        /// <param name="achievementEntry"></param>
        public void RemoveAchievementProgress(AchievementEntry achievementEntry)
        {
            foreach (AchievementCriteriaEntry criterion in achievementEntry.Criteria)
                this.RemoveProgress(criterion.AchievementCriteriaId);
        }

        /// <summary>
        /// Checks if the player can ever complete the given achievement.
        /// </summary>
        /// <param name="achievementCriteriaEntry"></param>
        /// <returns></returns>
        private bool IsAchieveable(AchievementCriteriaEntry achievementCriteriaEntry)
        {
            return (!AchievementCollection.DisableStaffAchievements || !this.Owner.Role.IsStaff) &&
                   !this.HasCompleted(achievementCriteriaEntry.AchievementEntryId) &&
                   ((achievementCriteriaEntry.AchievementEntry.FactionFlag != 1 ||
                     this.Owner.FactionGroup == FactionGroup.Alliance) &&
                    (achievementCriteriaEntry.AchievementEntry.FactionFlag != 0 ||
                     this.Owner.FactionGroup == FactionGroup.Horde)) &&
                   (!achievementCriteriaEntry.GroupFlag.HasFlag((Enum) AchievementCriteriaGroupFlags
                        .AchievementCriteriaGroupNotInGroup) || !this.Owner.IsInGroup);
        }

        /// <summary>
        /// A method that will try to update the progress of all the related criterias.
        /// </summary>
        /// <param name="type">The Criteria Type.</param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="involved"></param>
        internal void CheckPossibleAchievementUpdates(AchievementCriteriaType type, uint value1 = 0, uint value2 = 0,
            Unit involved = null)
        {
            List<AchievementCriteriaEntry> criteriaEntriesByType = AchievementMgr.GetCriteriaEntriesByType(type);
            if (criteriaEntriesByType == null)
                return;
            foreach (AchievementCriteriaEntry achievementCriteriaEntry in criteriaEntriesByType)
            {
                if (this.IsAchieveable(achievementCriteriaEntry) &&
                    (achievementCriteriaEntry.RequirementSet == null ||
                     achievementCriteriaEntry.RequirementSet.Meets(this.Owner, involved, value1)))
                    achievementCriteriaEntry.OnUpdate(this, value1, value2, (ObjectBase) involved);
            }
        }

        /// <summary>Sets the progress with a given Criteria entry.</summary>
        /// <param name="entry"></param>
        /// <param name="newValue"></param>
        /// <param name="progressType"></param>
        internal void SetCriteriaProgress(AchievementCriteriaEntry entry, uint newValue,
            ProgressType progressType = ProgressType.ProgressSet)
        {
            if (newValue == 0U)
                return;
            AchievementProgressRecord progressRecord = this.GetOrCreateProgressRecord(entry.AchievementCriteriaId);
            uint num;
            switch (progressType)
            {
                case ProgressType.ProgressAccumulate:
                    num = newValue + progressRecord.Counter;
                    break;
                case ProgressType.ProgressHighest:
                    num = progressRecord.Counter < newValue ? newValue : progressRecord.Counter;
                    break;
                default:
                    num = newValue;
                    break;
            }

            if ((int) num == (int) progressRecord.Counter)
                return;
            progressRecord.Counter = num;
            if (entry.TimeLimit > 0U)
            {
                DateTime now = DateTime.Now;
                if (progressRecord.StartOrUpdateTime.AddSeconds((double) entry.TimeLimit) < now)
                    progressRecord.Counter = 1U;
                progressRecord.StartOrUpdateTime = now;
            }

            AchievementHandler.SendAchievmentStatus(progressRecord, this.Owner);
            if (!this.IsAchievementCompletable(entry.AchievementEntry))
                return;
            this.EarnAchievement(entry.AchievementEntry);
        }

        public void SaveNow()
        {
            foreach (ActiveRecordBase activeRecordBase in this.m_completedAchievements.Values)
                activeRecordBase.Save();
            foreach (ActiveRecordBase activeRecordBase in this.m_progressRecords.Values)
                activeRecordBase.Save();
        }

        public void Load()
        {
            foreach (AchievementRecord achievementRecord in AchievementRecord.Load((int) this.Owner.EntityId.Low))
            {
                AchievementEntry achievementEntry =
                    AchievementMgr.GetAchievementEntry(achievementRecord.AchievementEntryId);
                if (achievementEntry != null)
                {
                    if (this.m_completedAchievements.ContainsKey(achievementEntry.ID))
                        AchievementCollection.log.Warn("Character {0} had Achievement {1} more than once.",
                            (object) this.m_owner, (object) achievementEntry.ID);
                    else
                        this.AddAchievement(achievementRecord);
                }
                else
                    AchievementCollection.log.Warn("Character {0} has invalid Achievement: {1}", (object) this.m_owner,
                        (object) achievementRecord.AchievementEntryId);
            }

            foreach (AchievementProgressRecord achievementProgressRecord in AchievementProgressRecord.Load(
                (int) this.Owner.EntityId.Low))
            {
                if (this.m_progressRecords.ContainsKey(achievementProgressRecord.AchievementCriteriaId))
                    AchievementCollection.log.Warn(
                        "Character {0} had progress for Achievement Criteria {1} more than once.",
                        (object) this.m_owner, (object) achievementProgressRecord.AchievementCriteriaId);
                else
                    this.AddProgressRecord(achievementProgressRecord);
            }
        }
    }
}