using System;
using System.Collections.Generic;
using System.IO;

namespace WCell.Core.WDB
{
    public class WDBReader<TEntry, TConverter> where TEntry : IWDBEntry
        where TConverter : WDBRecordConverter<TEntry>, new()
    {
        private string m_fileName;
        private readonly uint m_magic;
        private readonly uint m_build;
        private readonly uint m_locale;
        private readonly uint m_unkHeader1;
        private readonly uint m_unkHeader2;
        private readonly List<TEntry> m_list;

        public WDBReader(string fileName)
        {
            this.m_fileName = fileName;
            this.m_list = new List<TEntry>();
            using (FileStream fileStream =
                new FileStream(this.m_fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader binReader = new BinaryReader((Stream) fileStream))
                {
                    this.m_magic = binReader.ReadUInt32();
                    this.m_build = binReader.ReadUInt32();
                    this.m_locale = binReader.ReadUInt32();
                    this.m_unkHeader1 = binReader.ReadUInt32();
                    this.m_unkHeader2 = binReader.ReadUInt32();
                    TConverter instance = Activator.CreateInstance<TConverter>();
                    while (binReader.BaseStream.Position < binReader.BaseStream.Length - 20L - 8L)
                        this.m_list.Add(instance.Convert(binReader));
                }
            }
        }

        public uint Build
        {
            get { return this.m_build; }
        }

        public List<TEntry> Entries
        {
            get { return this.m_list; }
        }

        public void Sort()
        {
            this.m_list.Sort((Comparison<TEntry>) ((x, y) =>
            {
                if (x.EntryId < y.EntryId)
                    return -1;
                return x.EntryId > y.EntryId ? 1 : 0;
            }));
        }
    }
}