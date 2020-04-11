using System.Collections.Generic;

namespace WCell.Core.DBC
{
  public class MappedDBCReader<TEntry, TConverter> : DBCReader<TConverter>
    where TConverter : AdvancedDBCRecordConverter<TEntry>, new()
  {
    public Dictionary<int, TEntry> Entries;

    public MappedDBCReader(string fileName)
      : base(fileName)
    {
    }

    public TEntry this[int id]
    {
      get
      {
        TEntry entry;
        Entries.TryGetValue(id, out entry);
        return entry;
      }
    }

    public TEntry this[uint id]
    {
      get { return this[(int) id]; }
    }

    protected override void Convert(byte[] bytes)
    {
      int currentIndex = this.currentIndex;
      TEntry entry = ((AdvancedDBCRecordConverter<TEntry>) converter).ConvertTo(bytes, ref currentIndex);
      Entries.Add(currentIndex, entry);
    }

    protected override void InitReader()
    {
      Entries = new Dictionary<int, TEntry>(m_recordCount);
    }
  }
}