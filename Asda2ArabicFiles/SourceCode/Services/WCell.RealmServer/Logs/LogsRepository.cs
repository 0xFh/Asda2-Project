using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using WCell.Core;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.Util.NLog;
using WCell.Util.Variables;

namespace WCell.RealmServer.Logs
{
    public static class Log
    {
        static readonly SelfRunningTaskQueue TaskQueue = new SelfRunningTaskQueue(1000, "LogsQueue");
        static readonly Dictionary<string, int> AttributeIds = new Dictionary<string, int>();
        static readonly Dictionary<string, int> MessageIds = new Dictionary<string, int>();
        static readonly Dictionary<string, int> TitleIds = new Dictionary<string, int>();

        [Initialization(InitializationPass.Last, "Logging system")]
        public static void Init()
        {
            LoadAttributes();
            LoadMessages();
            LoadTitles();
        }

        private static void LoadAttributes()
        {
            var attrs = LogAttributeRecord.FindAll();
            foreach (var attr in attrs)
            {
                if (AttributeIds.ContainsKey(attr.Value))
                {
                    attr.DeleteLater();
                    continue;
                }
                AttributeIds.Add(attr.Value, attr.Id);
            }
        }

        private static void LoadMessages()
        {
            var attrs = LogMessageRecord.FindAll();
            foreach (var msg in attrs)
            {
                if (MessageIds.ContainsKey(msg.Value))
                {
                    msg.DeleteLater();
                    continue;
                }
                MessageIds.Add(msg.Value, msg.Id);
            }
        }

        private static void LoadTitles()
        {
            var attrs = LogTitleRecord.FindAll();
            foreach (var title in attrs)
            {
                if (TitleIds.ContainsKey(title.Value))
                {
                    title.DeleteLater();
                    continue;
                }
                TitleIds.Add(title.Value, title.Id);
            }
        }

        public static void Write(LogSourceType sourceType, uint triggerId, string title, IEnumerable<LogAttribute> attributes, List<LogHelperEntry> referenceEntries, Action<LogEntryRecord> setRecord)
        {
         /*   TaskQueue.AddMessage(() =>
                                     {
                                         if (!TitleIds.ContainsKey(title))
                                         {
                                             CreateAndAddNewTitle(title);
                                         }

                                         var newLog = Save(sourceType, triggerId, title);
                                         foreach (var referenceEntry in referenceEntries)
                                         {
                                             if (referenceEntry.Record == null)
                                             {
                                                 LogUtil.WarnException("Log reference entries must be saved before this entry. Type : "+ title);
                                                 continue;
                                             }
                                             CreateAndSaveNewLogRef(newLog, referenceEntry);
                                         }
                                         setRecord(newLog);
                                         foreach (var attr in attributes)
                                         {
                                             WriteAttribute(attr, newLog.Id);
                                         }
                                     });*/
        }

        private static void CreateAndSaveNewLogRef(LogEntryRecord newLog, LogHelperEntry referenceEntry)
        {
            var newRefRec = LogReferenceRecord.CreateRecord();
            newRefRec.LogEntryId = newLog.Id;
            newRefRec.ReferenceLogEntryId = referenceEntry.Record.Id;
            newRefRec.Save();
        }

        private static LogEntryRecord Save(LogSourceType sourceType, uint triggerId, string title)
        {
            var newLog = LogEntryRecord.CreateRecord();
            newLog.TitleId = TitleIds[title];
            newLog.Timestamp = DateTime.Now;
            newLog.TrigererId = triggerId;
            newLog.TriggererType = (byte) sourceType;
            newLog.Save();
            return newLog;
        }

        private static void WriteAttribute(LogAttribute attr, long logId)
        {
            var newAttr = LogValueRecord.CreateRecord();
            newAttr.EntryId = logId;
            if (!AttributeIds.ContainsKey(attr.Title))
            {
                CreateAndAddNewAttribute(attr.Title);
            }
            if (!MessageIds.ContainsKey(attr.Message))
            {
                CreateAndAddNewMessage(attr.Message);
            }
            newAttr.MessageId = MessageIds[attr.Message];
            newAttr.AttributeId = AttributeIds[attr.Title];
            newAttr.Value = attr.Value;
            newAttr.Save();
        }

        private static void CreateAndAddNewMessage(string message)
        {
            var newMessage = LogMessageRecord.CreateRecord();
            newMessage.Value = message;
            MessageIds.Add(message, newMessage.Id);
            newMessage.Save();
        }

        private static void CreateAndAddNewAttribute(string title)
        {
            var newAttribute = LogAttributeRecord.CreateRecord();
            newAttribute.Value = title;
            AttributeIds.Add(title, newAttribute.Id);
            newAttribute.Save();
        }

        private static void CreateAndAddNewTitle(string title)
        {
            var newTitle = LogTitleRecord.CreateRecord();
            newTitle.Value = title;
            TitleIds.Add(title, newTitle.Id);
            newTitle.Save();
        }

        public static LogHelperEntry Create(string title, LogSourceType sourceType, uint triggerId)
        {
            return new LogHelperEntry(title, sourceType, triggerId);
        }

        public class Types
        {
            [NotVariable]
            public static string ExpChanged = "expirience_changed";
            [NotVariable]
            public static string Cheating = "cheating";
            [NotVariable]
            public static string ItemOperations = "item_operations";
            [NotVariable]
            public static string StatsOperations = "stats_operations";
            [NotVariable]
            public static string AccountOperations = "account_operatons";
             [NotVariable]
            public static string ChangePosition = "change_position";
            [NotVariable]
            public static string EventOperations = "event_operations";
        }
    }

    public class LogHelperEntry
    {
        private readonly string _title;
        private readonly LogSourceType _sourceType;
        private readonly uint _triggerId;
        private readonly List<LogAttribute> _attributes = new List<LogAttribute>();
        private readonly List<LogHelperEntry> _referenceEntries = new List<LogHelperEntry>();
        public LogEntryRecord Record;

        public LogHelperEntry(string title, LogSourceType sourceType, uint triggerId)
        {
            _title = title;
            _sourceType = sourceType;
            _triggerId = triggerId;
        }

        public LogHelperEntry AddAttribute(string title, double value, string message = "")
        {
            _attributes.Add(new LogAttribute { Message = message, Title = title, Value = value });
            return this;
        }

        public LogHelperEntry Write()
        {
            Log.Write(_sourceType, _triggerId, _title, _attributes, _referenceEntries, SetRecord);
            return this;
        }

        private void SetRecord(LogEntryRecord record)
        {
            Record = record;
        }


        public LogHelperEntry AddItemAttributes(Asda2Item item, string itemName = "")
        {
            if (item == null)
                return this;
            AddAttribute("item_id", item.ItemId, item.Name + itemName);
            AddAttribute("is_soulbound", item.IsSoulbound ? 1 : 0, itemName);
            AddAttribute("inventory_type", (long) item.InventoryType, itemName + " " + item.InventoryType.ToString());
            AddAttribute("slot", item.Slot, itemName);
            if (item.Template.IsStackable)
            AddAttribute("amount", item.Amount, itemName);
            if (item.Enchant != 0)
            AddAttribute("enchant", item.Enchant, itemName);
            if (item.Soul1Id != 0)
            AddAttribute("sowel_1_id", item.Soul1Id, itemName);
            if (item.Soul2Id != 0)
                AddAttribute("sowel_2_id", item.Soul2Id, itemName);
            if (item.Soul3Id != 0)
                AddAttribute("sowel_3_id", item.Soul3Id, itemName);
            if (item.Soul4Id != 0)
                AddAttribute("sowel_4_id", item.Soul4Id, itemName);
            if (item.Parametr1Type != 0)
                AddAttribute("parametr_1_type", (long)item.Parametr1Type, itemName + " " + item.Parametr1Type.ToString());
            if (item.Parametr1Value != 0)
                AddAttribute("parametr_1_value", item.Parametr1Value, itemName);
            if (item.Parametr2Type != 0)
                AddAttribute("parametr_2_type", (long)item.Parametr2Type, itemName + " " + item.Parametr2Type.ToString());
            if (item.Parametr2Value != 0)
                AddAttribute("parametr_2_value", item.Parametr2Value, itemName);
            if (item.Parametr3Type != 0)
                AddAttribute("parametr_3_type", (long)item.Parametr3Type, itemName + " " + item.Parametr3Type.ToString());
            if (item.Parametr3Value != 0)
                AddAttribute("parametr_3_value", item.Parametr3Value, itemName);
            if (item.Parametr4Type != 0)
                AddAttribute("parametr_4_type", (long)item.Parametr4Type, itemName + " " + item.Parametr4Type.ToString());
            if (item.Parametr4Value != 0)
                AddAttribute("parametr_4_value", item.Parametr4Value, itemName);
            if (item.Parametr5Type != 0)
                AddAttribute("parametr_5_type", (long)item.Parametr5Type, itemName + " " + item.Parametr5Type.ToString());
            if (item.Parametr5Value != 0)
                AddAttribute("parametr_5_value", item.Parametr5Value, itemName);
            return this;
        }

        public LogHelperEntry AddReference(LogHelperEntry lgDelete)
        {
            if(lgDelete!=null)
                _referenceEntries.Add(lgDelete);
            return this;
        }
    }

    public class LogAttribute
    {
        public string Title { get; set; }
        public double Value { get; set; }
        public string Message { get; set; }
    }

    public enum LogSourceType
    {
        Account = 0,
        Character = 1
    }

    [ActiveRecord("LogEntries", Access = PropertyAccess.Property)]
    public class LogEntryRecord : WCellRecord<LogEntryRecord>
    {
        private static readonly NHIdGenerator IdGenerator = new NHIdGenerator(typeof(LogEntryRecord), "Id");

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        public long Id { get; set; }

        [Property(NotNull = true)]
        public DateTime Timestamp { get; set; }

        [Property(NotNull = true)]
        public byte TriggererType { get; set; }

        [Property(NotNull = true)]
        public uint TrigererId { get; set; }

        [Property(NotNull = true)]
        public int TitleId { get; set; }
        /// <summary>
        /// Returns the next unique Id for a new Item
        /// </summary>
        public static long NextId()
        {
            return IdGenerator.Next();
        }

        internal static LogEntryRecord CreateRecord()
        {
            try
            {
                var itemRecord = new LogEntryRecord
                {
                    Id = IdGenerator.Next(),
                    State = RecordState.New
                };

                return itemRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new LogEntryRecord.");
            }
        }

    }
    [ActiveRecord("LogAttributes", Access = PropertyAccess.Property)]
    public class LogAttributeRecord : WCellRecord<LogAttributeRecord>
    {
        private static readonly NHIdGenerator IdGenerator = new NHIdGenerator(typeof(LogAttributeRecord), "Id");

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        public int Id { get; set; }

        [Property(NotNull = true)]
        public String Value { get; set; }

        /// <summary>
        /// Returns the next unique Id for a new Item
        /// </summary>
        public static long NextId()
        {
            return IdGenerator.Next();
        }

        internal static LogAttributeRecord CreateRecord()
        {
            try
            {
                var itemRecord = new LogAttributeRecord
                {
                    Id = (int)IdGenerator.Next(),
                    State = RecordState.New
                };

                return itemRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new LogAttributeRecord.");
            }
        }
    }
    [ActiveRecord("LogReferences", Access = PropertyAccess.Property)]
    public class LogReferenceRecord : WCellRecord<LogReferenceRecord>
    {
        private static readonly NHIdGenerator IdGenerator = new NHIdGenerator(typeof(LogReferenceRecord), "Id");

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        public int Id { get; set; }


        [Property(NotNull = true)]
        public long LogEntryId { get; set; }

        [Property(NotNull = true)]
        public long ReferenceLogEntryId { get; set; }
        /// <summary>
        /// Returns the next unique Id for a new Item
        /// </summary>
        public static long NextId()
        {
            return IdGenerator.Next();
        }

        internal static LogReferenceRecord CreateRecord()
        {
            try
            {
                var itemRecord = new LogReferenceRecord
                {
                    Id = (int)IdGenerator.Next(),
                    State = RecordState.New
                };

                return itemRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new LogReferenceRecord.");
            }
        }
    }

    [ActiveRecord("LogMessages", Access = PropertyAccess.Property)]
    public class LogMessageRecord : WCellRecord<LogMessageRecord>
    {
        private static readonly NHIdGenerator IdGenerator = new NHIdGenerator(typeof(LogMessageRecord), "Id");

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        public int Id { get; set; }

        [Property(NotNull = true)]
        public String Value { get; set; }

        /// <summary>
        /// Returns the next unique Id for a new Item
        /// </summary>
        public static long NextId()
        {
            return IdGenerator.Next();
        }

        internal static LogMessageRecord CreateRecord()
        {
            try
            {
                var itemRecord = new LogMessageRecord
                {
                    Id = (int)IdGenerator.Next(),
                    State = RecordState.New
                };

                return itemRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new LogMessageRecord.");
            }
        }
    }

    [ActiveRecord("LogTitles", Access = PropertyAccess.Property)]
    public class LogTitleRecord : WCellRecord<LogTitleRecord>
    {
        private static readonly NHIdGenerator IdGenerator = new NHIdGenerator(typeof(LogTitleRecord), "Id");

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        public int Id { get; set; }

        [Property(NotNull = true)]
        public String Value { get; set; }

        /// <summary>
        /// Returns the next unique Id for a new Item
        /// </summary>
        public static long NextId()
        {
            return IdGenerator.Next();
        }

        internal static LogTitleRecord CreateRecord()
        {
            try
            {
                var itemRecord = new LogTitleRecord
                {
                    Id = (int)IdGenerator.Next(),
                    State = RecordState.New
                };

                return itemRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new LogTitleRecord.");
            }
        }
    }

    [ActiveRecord("LogValues", Access = PropertyAccess.Property)]
    public class LogValueRecord : WCellRecord<LogValueRecord>
    {
        private static readonly NHIdGenerator IdGenerator = new NHIdGenerator(typeof(LogValueRecord), "Id");

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        public long Id { get; set; }

        [Property(NotNull = true)]
        public long EntryId { get; set; }

        [Property(NotNull = true)]
        public int AttributeId { get; set; }

        [Property(NotNull = true)]
        public int MessageId { get; set; }

        [Property(NotNull = true)]
        public double Value { get; set; }

        /// <summary>
        /// Returns the next unique Id for a new Item
        /// </summary>
        public static long NextId()
        {
            return IdGenerator.Next();
        }

        internal static LogValueRecord CreateRecord()
        {
            try
            {
                var itemRecord = new LogValueRecord
                {
                    Id = IdGenerator.Next(),
                    State = RecordState.New
                };

                return itemRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new LogValueRecord.");
            }
        }
    }
}
