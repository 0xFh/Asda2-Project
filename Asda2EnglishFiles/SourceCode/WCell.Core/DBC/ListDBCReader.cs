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
            this.EntryList.Add(
                ((AdvancedDBCRecordConverter<TEntry>) this.converter).ConvertTo(bytes, ref currentIndex));
        }

        protected override void InitReader()
        {
            this.EntryList = new List<TEntry>(this.m_recordCount);
        }
    }
}