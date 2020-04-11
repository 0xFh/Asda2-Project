using Castle.ActiveRecord;
using System;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Logs
{
    [Castle.ActiveRecord.ActiveRecord("LogReferences", Access = PropertyAccess.Property)]
    public class LogReferenceRecord : WCellRecord<LogReferenceRecord>
    {
        private static readonly NHIdGenerator IdGenerator =
            new NHIdGenerator(typeof(LogReferenceRecord), nameof(Id), 1L);

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        public int Id { get; set; }

        [Property(NotNull = true)] public long LogEntryId { get; set; }

        [Property(NotNull = true)] public long ReferenceLogEntryId { get; set; }

        /// <summary>Returns the next unique Id for a new Item</summary>
        public static long NextId()
        {
            return LogReferenceRecord.IdGenerator.Next();
        }

        internal static LogReferenceRecord CreateRecord()
        {
            try
            {
                LogReferenceRecord logReferenceRecord = new LogReferenceRecord();
                logReferenceRecord.Id = (int) LogReferenceRecord.IdGenerator.Next();
                logReferenceRecord.State = RecordState.New;
                return logReferenceRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new LogReferenceRecord.", new object[0]);
            }
        }
    }
}