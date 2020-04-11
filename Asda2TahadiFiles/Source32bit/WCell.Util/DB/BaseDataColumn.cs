namespace WCell.Util.DB
{
  public abstract class BaseDataColumn
  {
    protected readonly string m_ColumnName;

    protected BaseDataColumn(string column)
    {
      m_ColumnName = column;
    }

    /// <summary>The name of the Column</summary>
    public string ColumnName
    {
      get { return m_ColumnName; }
    }
  }
}