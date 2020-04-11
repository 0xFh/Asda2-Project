using Castle.ActiveRecord;

namespace WCell.Core.Database
{
    public class WCellRecord<T> : ActiveRecordBase<T> where T : ActiveRecordBase
    {
        public RecordState State { get; set; }

        public bool IsNew
        {
            get { return this.State == RecordState.New; }
        }

        public bool IsDirty
        {
            get { return this.State == RecordState.New || this.State == RecordState.Dirty; }
        }

        public bool IsDeleted
        {
            get { return this.State == RecordState.Deleted; }
        }

        public override void Save()
        {
            if (this.IsNew)
                this.Create();
            else
                this.Update();
        }

        public override void Create()
        {
            this.State = RecordState.Ok;
            base.Create();
        }

        public override void SaveAndFlush()
        {
            if (this.IsNew)
                this.CreateAndFlush();
            else
                this.UpdateAndFlush();
        }

        public override void CreateAndFlush()
        {
            this.State = RecordState.Ok;
            base.CreateAndFlush();
        }

        public override void Delete()
        {
            if (this.IsNew)
                return;
            base.Delete();
        }

        public override void DeleteAndFlush()
        {
            if (this.IsDeleted || this.IsNew)
                return;
            this.State = RecordState.Deleted;
            base.DeleteAndFlush();
        }
    }
}