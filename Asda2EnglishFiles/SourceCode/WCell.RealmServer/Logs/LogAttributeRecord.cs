using Castle.ActiveRecord;
using System;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Logs
{
    [Castle.ActiveRecord.ActiveRecord("LogAttributes", Access = PropertyAccess.Property)]
    public class LogAttributeRecord : WCellRecord<LogAttributeRecord>
    {
        private static readonly NHIdGenerator IdGenerator =
            new NHIdGenerator(typeof(LogAttributeRecord), nameof(Id), 1L);

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        public int Id { get; set; }

        [Property(NotNull = true)] public string Value { get; set; }

        /// <summary>Returns the next unique Id for a new Item</summary>
        public static long NextId()
        {
            return LogAttributeRecord.IdGenerator.Next();
        }

        internal static LogAttributeRecord CreateRecord()
        {
            try
            {
                LogAttributeRecord logAttributeRecord = new LogAttributeRecord();
                logAttributeRecord.Id = (int) LogAttributeRecord.IdGenerator.Next();
                logAttributeRecord.State = RecordState.New;
                return logAttributeRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new LogAttributeRecord.", new object[0]);
            }
        }
    }
}