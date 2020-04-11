using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using System;
using WCell.Core.Database;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Achievements
{
    /// <summary>
    /// Represents the progress in one criterion of one Achievement.
    /// One Achievement can have many criteria.
    /// </summary>
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class AchievementProgressRecord : WCellRecord<AchievementProgressRecord>
    {
        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

        [Field("CharacterId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _characterGuid;

        [Field("Criteria", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _achievementCriteriaId;

        [Field("Counter", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _counter;

        /// <summary>
        /// Creates a new AchievementProgressRecord row in the database with the given information.
        /// </summary>
        /// <param name="account">the account this character is on</param>
        /// <param name="name">the name of the new character</param>
        /// <returns>the <seealso cref="T:WCell.RealmServer.Achievements.AchievementProgressRecord" /> object</returns>
        public static AchievementProgressRecord CreateAchievementProgressRecord(Character chr,
            uint achievementCriteriaId, uint counter)
        {
            AchievementProgressRecord achievementProgressRecord1;
            try
            {
                AchievementProgressRecord achievementProgressRecord2 = new AchievementProgressRecord();
                achievementProgressRecord2._characterGuid = (int) chr.EntityId.Low;
                achievementProgressRecord2._achievementCriteriaId = (int) achievementCriteriaId;
                achievementProgressRecord2._counter = (int) counter;
                achievementProgressRecord2.StartOrUpdateTime = DateTime.Now;
                achievementProgressRecord2.State = RecordState.New;
                achievementProgressRecord1 = achievementProgressRecord2;
                achievementProgressRecord1.SaveAndFlush();
            }
            catch (Exception ex)
            {
                AchievementProgressRecord.s_log.Error(
                    "AchievementProgressRecord creation error (DBS: " + RealmServerConfiguration.DatabaseType + "): ",
                    (object) ex);
                achievementProgressRecord1 = (AchievementProgressRecord) null;
            }

            return achievementProgressRecord1;
        }

        public static AchievementProgressRecord CreateGlobalAchievementProgressRecord(uint achievementCriteriaId,
            uint counter)
        {
            AchievementProgressRecord achievementProgressRecord1;
            try
            {
                AchievementProgressRecord achievementProgressRecord2 = new AchievementProgressRecord();
                achievementProgressRecord2._characterGuid = -1;
                achievementProgressRecord2._achievementCriteriaId = (int) achievementCriteriaId;
                achievementProgressRecord2._counter = (int) counter;
                achievementProgressRecord2.StartOrUpdateTime = DateTime.Now;
                achievementProgressRecord2.State = RecordState.New;
                achievementProgressRecord1 = achievementProgressRecord2;
                achievementProgressRecord1.SaveAndFlush();
            }
            catch (Exception ex)
            {
                AchievementProgressRecord.s_log.Error(
                    "AchievementProgressRecord creation error (DBS: " + RealmServerConfiguration.DatabaseType + "): ",
                    (object) ex);
                achievementProgressRecord1 = (AchievementProgressRecord) null;
            }

            return achievementProgressRecord1;
        }

        public static AchievementProgressRecord GetOrCreateGlobalProgressRecord(uint achievementCriteriaId,
            uint counter)
        {
            foreach (AchievementProgressRecord achievementProgressRecord in ActiveRecordBase<AchievementProgressRecord>
                .FindAll())
            {
                if ((int) achievementProgressRecord.AchievementCriteriaId == (int) achievementCriteriaId)
                    return achievementProgressRecord;
            }

            return AchievementProgressRecord.CreateGlobalAchievementProgressRecord(achievementCriteriaId, counter);
        }

        /// <summary>Encode char id and achievement id into RecordId</summary>
        [PrimaryKey(PrimaryKeyType.Assigned)]
        public long RecordId
        {
            get { return Utility.MakeLong(this._characterGuid, this._achievementCriteriaId); }
            set { Utility.UnpackLong(value, ref this._characterGuid, ref this._achievementCriteriaId); }
        }

        /// <summary>
        /// The time when this record was inserted or last updated (depends on the kind of criterion)
        /// </summary>
        [Property]
        public DateTime StartOrUpdateTime { get; set; }

        public uint CharacterGuid
        {
            get { return (uint) this._characterGuid; }
            set { this._characterGuid = (int) value; }
        }

        public uint AchievementCriteriaId
        {
            get { return (uint) this._achievementCriteriaId; }
            set { this._achievementCriteriaId = (int) value; }
        }

        public uint Counter
        {
            get { return (uint) this._counter; }
            set { this._counter = (int) value; }
        }

        public static AchievementProgressRecord[] Load(int chrRecordId)
        {
            return ActiveRecordBase<AchievementProgressRecord>.FindAll(new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("_characterGuid", (object) chrRecordId)
            });
        }

        public override string ToString()
        {
            return string.Format("{0} (Char: {1}, Criteria: {2})", (object) this.GetType().Name,
                (object) this._characterGuid, (object) this._achievementCriteriaId);
        }
    }
}