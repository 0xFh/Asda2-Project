using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using System;
using System.Collections;
using WCell.Core.Database;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Achievements
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class AchievementRecord : WCellRecord<AchievementRecord>
    {
        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

        [Field("CharacterId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _characterGuid;

        [Field("Achievement", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _achievementEntryId;

        /// <summary>
        /// Creates a new AchievementRecord row in the database with the given information.
        /// </summary>
        /// <param name="account">the account this character is on</param>
        /// <param name="name">the name of the new character</param>
        /// <returns>the <seealso cref="T:WCell.RealmServer.Achievements.AchievementRecord" /> object</returns>
        public static AchievementRecord CreateNewAchievementRecord(Character chr, uint achievementEntryId)
        {
            try
            {
                AchievementRecord achievementRecord = new AchievementRecord();
                achievementRecord._achievementEntryId = (int) achievementEntryId;
                achievementRecord._characterGuid = (int) chr.EntityId.Low;
                achievementRecord.CompleteDate = DateTime.Now;
                achievementRecord.State = RecordState.New;
                return achievementRecord;
            }
            catch (Exception ex)
            {
                AchievementRecord.s_log.ErrorException(
                    "AchievementRecord creation error (DBS: " + RealmServerConfiguration.DatabaseType + "): ", ex);
                return (AchievementRecord) null;
            }
        }

        /// <summary>Encode char id and achievement id into RecordId</summary>
        [PrimaryKey(PrimaryKeyType.Assigned)]
        public long RecordId
        {
            get { return Utility.MakeLong(this._characterGuid, this._achievementEntryId); }
            set { Utility.UnpackLong(value, ref this._characterGuid, ref this._achievementEntryId); }
        }

        [Property] public DateTime CompleteDate { get; set; }

        public uint CharacterGuid
        {
            get { return (uint) this._characterGuid; }
            set { this._characterGuid = (int) value; }
        }

        public uint AchievementEntryId
        {
            get { return (uint) this._achievementEntryId; }
            set { this._achievementEntryId = (int) value; }
        }

        public static AchievementRecord[] Load(int chrId)
        {
            return ActiveRecordBase<AchievementRecord>.FindAll(new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("_characterGuid", (object) chrId)
            });
        }

        public static AchievementRecord[] Load(uint[] achievementEntryIds)
        {
            return ActiveRecordBase<AchievementRecord>.FindAll(new ICriterion[1]
            {
                (ICriterion) Restrictions.In("_achievementEntryId", (ICollection) achievementEntryIds)
            });
        }

        public override string ToString()
        {
            return string.Format("{0} - Char: {1}, Achievement: {2}, RecordId: {3}", (object) this.GetType(),
                (object) this._characterGuid, (object) this._achievementEntryId, (object) this.RecordId);
        }
    }
}