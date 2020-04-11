using Castle.ActiveRecord;
using System;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Logs
{
    [Castle.ActiveRecord.ActiveRecord("LogTitles", Access = PropertyAccess.Property)]
    public class LogTitleRecord : WCellRecord<LogTitleRecord>
    {
        private static readonly NHIdGenerator IdGenerator = new NHIdGenerator(typeof(LogTitleRecord), nameof(Id), 1L);

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        public int Id { get; set; }

        [Property(NotNull = true)] public string Value { get; set; }

        /// <summary>Returns the next unique Id for a new Item</summary>
        public static long NextId()
        {
            return LogTitleRecord.IdGenerator.Next();
        }

        internal static LogTitleRecord CreateRecord()
        {
            try
            {
                LogTitleRecord logTitleRecord = new LogTitleRecord();
                logTitleRecord.Id = (int) LogTitleRecord.IdGenerator.Next();
                logTitleRecord.State = RecordState.New;
                return logTitleRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new LogTitleRecord.", new object[0]);
            }
        }
    }
}