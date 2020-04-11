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
      m_fileName = fileName;
      m_list = new List<TEntry>();
      using(FileStream fileStream =
        new FileStream(m_fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        using(BinaryReader binReader = new BinaryReader(fileStream))
        {
          m_magic = binReader.ReadUInt32();
          m_build = binReader.ReadUInt32();
          m_locale = binReader.ReadUInt32();
          m_unkHeader1 = binReader.ReadUInt32();
          m_unkHeader2 = binReader.ReadUInt32();
          TConverter instance = Activator.CreateInstance<TConverter>();
          while(binReader.BaseStream.Position < binReader.BaseStream.Length - 20L - 8L)
            m_list.Add(instance.Convert(binReader));
        }
      }
    }

    public uint Build
    {
      get { return m_build; }
    }

    public List<TEntry> Entries
    {
      get { return m_list; }
    }

    public void Sort()
    {
      m_list.Sort((x, y) =>
      {
        if(x.EntryId < y.EntryId)
          return -1;
        return x.EntryId > y.EntryId ? 1 : 0;
      });
    }
  }
}