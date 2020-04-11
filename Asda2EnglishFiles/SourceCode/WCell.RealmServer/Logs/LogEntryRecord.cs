using Castle.ActiveRecord;
using System;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Logs
{
    [Castle.ActiveRecord.ActiveRecord("LogEntries", Access = PropertyAccess.Property)]
    public class LogEntryRecord : WCellRecord<LogEntryRecord>
    {
        private static readonly NHIdGenerator IdGenerator = new NHIdGenerator(typeof(LogEntryRecord), nameof(Id), 1L);

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        public long Id { get; set; }

        [Property(NotNull = true)] public DateTime Timestamp { get; set; }

        [Property(NotNull = true)] public byte TriggererType { get; set; }

        [Property(NotNull = true)] public uint TrigererId { get; set; }

        [Property(NotNull = true)] public int TitleId { get; set; }

        /// <summary>Returns the next unique Id for a new Item</summary>
        public static long NextId()
        {
            return LogEntryRecord.IdGenerator.Next();
        }

        internal static LogEntryRecord CreateRecord()
        {
            try
            {
                LogEntryRecord logEntryRecord = new LogEntryRecord();
                logEntryRecord.Id = LogEntryRecord.IdGenerator.Next();
                logEntryRecord.State = RecordState.New;
                return logEntryRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new LogEntryRecord.", new object[0]);
            }
        }
    }
}