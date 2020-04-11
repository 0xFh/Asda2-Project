using Castle.ActiveRecord;
using System;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Misc
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class BugReport : WCellRecord<BugReport>
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(BugReport), nameof(_id), 1L);
        [Field("Type", NotNull = true)] private string _type;
        [Field("Content", NotNull = true)] private string _content;
        [Field("Created", NotNull = true)] private DateTime _reportDate;

        public static BugReport CreateNewBugReport(string type, string content)
        {
            BugReport bugReport = new BugReport();
            bugReport._id = (int) BugReport._idGenerator.Next();
            bugReport._type = type;
            bugReport._content = content;
            bugReport._reportDate = DateTime.Now;
            bugReport.State = RecordState.New;
            return bugReport;
        }

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        private int _id { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - ID : {1}, Type : {2}, Content : {3}, Created : {4}", (object) this.GetType(),
                (object) this._id, (object) this._type, (object) this._content, (object) this._reportDate);
        }
    }
}