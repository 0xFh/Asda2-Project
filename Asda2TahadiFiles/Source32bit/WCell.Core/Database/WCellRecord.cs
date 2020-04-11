using Castle.ActiveRecord;

namespace WCell.Core.Database
{
  public class WCellRecord<T> : ActiveRecordBase<T> where T : ActiveRecordBase
  {
    public RecordState State { get; set; }

    public bool IsNew
    {
      get { return State == RecordState.New; }
    }

    public bool IsDirty
    {
      get { return State == RecordState.New || State == RecordState.Dirty; }
    }

    public bool IsDeleted
    {
      get { return State == RecordState.Deleted; }
    }

    public override void Save()
    {
      if(IsNew)
        Create();
      else
        Update();
    }

    public override void Create()
    {
      State = RecordState.Ok;
      base.Create();
    }

    public override void SaveAndFlush()
    {
      if(IsNew)
        CreateAndFlush();
      else
        UpdateAndFlush();
    }

    public override void CreateAndFlush()
    {
      State = RecordState.Ok;
      base.CreateAndFlush();
    }

    public override void Delete()
    {
      if(IsNew)
        return;
      base.Delete();
    }

    public override void DeleteAndFlush()
    {
      if(IsDeleted || IsNew)
        return;
      State = RecordState.Deleted;
      base.DeleteAndFlush();
    }
  }
}