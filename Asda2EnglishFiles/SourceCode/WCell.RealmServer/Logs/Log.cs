using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.Util.Variables;

namespace WCell.RealmServer.Logs
{
    public static class Log
    {
        private static readonly SelfRunningTaskQueue TaskQueue = new SelfRunningTaskQueue(1000, "LogsQueue", true);
        private static readonly Dictionary<string, int> AttributeIds = new Dictionary<string, int>();
        private static readonly Dictionary<string, int> MessageIds = new Dictionary<string, int>();
        private static readonly Dictionary<string, int> TitleIds = new Dictionary<string, int>();

        [WCell.Core.Initialization.Initialization(InitializationPass.Last, "Logging system")]
        public static void Init()
        {
            Log.LoadAttributes();
            Log.LoadMessages();
            Log.LoadTitles();
        }

        private static void LoadAttributes()
        {
            foreach (LogAttributeRecord record in ActiveRecordBase<LogAttributeRecord>.FindAll())
            {
                if (Log.AttributeIds.ContainsKey(record.Value))
                    record.DeleteLater();
                else
                    Log.AttributeIds.Add(record.Value, record.Id);
            }
        }

        private static void LoadMessages()
        {
            foreach (LogMessageRecord record in ActiveRecordBase<LogMessageRecord>.FindAll())
            {
                if (Log.MessageIds.ContainsKey(record.Value))
                    record.DeleteLater();
                else
                    Log.MessageIds.Add(record.Value, record.Id);
            }
        }

        private static void LoadTitles()
        {
            foreach (LogTitleRecord record in ActiveRecordBase<LogTitleRecord>.FindAll())
            {
                if (Log.TitleIds.ContainsKey(record.Value))
                    record.DeleteLater();
                else
                    Log.TitleIds.Add(record.Value, record.Id);
            }
        }

        public static void Write(LogSourceType sourceType, uint triggerId, string title,
            IEnumerable<LogAttribute> attributes, List<LogHelperEntry> referenceEntries,
            Action<LogEntryRecord> setRecord)
        {
        }

        private static void CreateAndSaveNewLogRef(LogEntryRecord newLog, LogHelperEntry referenceEntry)
        {
            LogReferenceRecord record = LogReferenceRecord.CreateRecord();
            record.LogEntryId = newLog.Id;
            record.ReferenceLogEntryId = referenceEntry.Record.Id;
            record.Save();
        }

        private static LogEntryRecord Save(LogSourceType sourceType, uint triggerId, string title)
        {
            LogEntryRecord record = LogEntryRecord.CreateRecord();
            record.TitleId = Log.TitleIds[title];
            record.Timestamp = DateTime.Now;
            record.TrigererId = triggerId;
            record.TriggererType = (byte) sourceType;
            record.Save();
            return record;
        }

        private static void WriteAttribute(LogAttribute attr, long logId)
        {
            LogValueRecord record = LogValueRecord.CreateRecord();
            record.EntryId = logId;
            if (!Log.AttributeIds.ContainsKey(attr.Title))
                Log.CreateAndAddNewAttribute(attr.Title);
            if (!Log.MessageIds.ContainsKey(attr.Message))
                Log.CreateAndAddNewMessage(attr.Message);
            record.MessageId = Log.MessageIds[attr.Message];
            record.AttributeId = Log.AttributeIds[attr.Title];
            record.Value = attr.Value;
            record.Save();
        }

        private static void CreateAndAddNewMessage(string message)
        {
            LogMessageRecord record = LogMessageRecord.CreateRecord();
            record.Value = message;
            Log.MessageIds.Add(message, record.Id);
            record.Save();
        }

        private static void CreateAndAddNewAttribute(string title)
        {
            LogAttributeRecord record = LogAttributeRecord.CreateRecord();
            record.Value = title;
            Log.AttributeIds.Add(title, record.Id);
            record.Save();
        }

        private static void CreateAndAddNewTitle(string title)
        {
            LogTitleRecord record = LogTitleRecord.CreateRecord();
            record.Value = title;
            Log.TitleIds.Add(title, record.Id);
            record.Save();
        }

        public static LogHelperEntry Create(string title, LogSourceType sourceType, uint triggerId)
        {
            return new LogHelperEntry(title, sourceType, triggerId);
        }

        public class Types
        {
            [NotVariable] public static string ExpChanged = "expirience_changed";
            [NotVariable] public static string Cheating = "cheating";
            [NotVariable] public static string ItemOperations = "item_operations";
            [NotVariable] public static string StatsOperations = "stats_operations";
            [NotVariable] public static string AccountOperations = "account_operatons";
            [NotVariable] public static string ChangePosition = "change_position";
            [NotVariable] public static string EventOperations = "event_operations";
        }
    }
}