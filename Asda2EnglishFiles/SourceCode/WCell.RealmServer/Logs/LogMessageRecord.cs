using Castle.ActiveRecord;
using System;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Logs
{
    [Castle.ActiveRecord.ActiveRecord("LogMessages", Access = PropertyAccess.Property)]
    public class LogMessageRecord : WCellRecord<LogMessageRecord>
    {
        private static readonly NHIdGenerator IdGenerator = new NHIdGenerator(typeof(LogMessageRecord), nameof(Id), 1L);

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        public int Id { get; set; }

        [Property(NotNull = true)] public string Value { get; set; }

        /// <summary>Returns the next unique Id for a new Item</summary>
        public static long NextId()
        {
            return LogMessageRecord.IdGenerator.Next();
        }

        internal static LogMessageRecord CreateRecord()
        {
            try
            {
                LogMessageRecord logMessageRecord = new LogMessageRecord();
                logMessageRecord.Id = (int) LogMessageRecord.IdGenerator.Next();
                logMessageRecord.State = RecordState.New;
                return logMessageRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new LogMessageRecord.", new object[0]);
            }
        }
    }
}