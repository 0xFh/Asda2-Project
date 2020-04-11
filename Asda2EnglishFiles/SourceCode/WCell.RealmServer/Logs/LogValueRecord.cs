using Castle.ActiveRecord;
using System;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Logs
{
    [Castle.ActiveRecord.ActiveRecord("LogValues", Access = PropertyAccess.Property)]
    public class LogValueRecord : WCellRecord<LogValueRecord>
    {
        private static readonly NHIdGenerator IdGenerator = new NHIdGenerator(typeof(LogValueRecord), nameof(Id), 1L);

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        public long Id { get; set; }

        [Property(NotNull = true)] public long EntryId { get; set; }

        [Property(NotNull = true)] public int AttributeId { get; set; }

        [Property(NotNull = true)] public int MessageId { get; set; }

        [Property(NotNull = true)] public double Value { get; set; }

        /// <summary>Returns the next unique Id for a new Item</summary>
        public static long NextId()
        {
            return LogValueRecord.IdGenerator.Next();
        }

        internal static LogValueRecord CreateRecord()
        {
            try
            {
                LogValueRecord logValueRecord = new LogValueRecord();
                logValueRecord.Id = LogValueRecord.IdGenerator.Next();
                logValueRecord.State = RecordState.New;
                return logValueRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new LogValueRecord.", new object[0]);
            }
        }
    }
}