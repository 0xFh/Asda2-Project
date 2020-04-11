using System.Collections.Generic;

namespace WCell.Core.DBC
{
  public class ListDBCReader<TEntry, TConverter> : DBCReader<TConverter>
    where TConverter : AdvancedDBCRecordConverter<TEntry>, new()
  {
    public List<TEntry> EntryList;

    public ListDBCReader(string fileName)
      : base(fileName)
    {
    }

    protected override void Convert(byte[] bytes)
    {
      int currentIndex = this.currentIndex;
      EntryList.Add(
        ((AdvancedDBCRecordConverter<TEntry>) converter).ConvertTo(bytes, ref currentIndex));
    }

    protected override void InitReader()
    {
      EntryList = new List<TEntry>(m_recordCount);
    }
  }
}