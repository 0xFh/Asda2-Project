using System;
using System.IO;
using WCell.Constants;

namespace WCell.Core.DBC
{
  public class DBCReader<TConverter> where TConverter : DBCRecordConverter, new()
  {
    public const int DBCHeader = 1128416343;
    protected readonly int m_recordSize;
    protected readonly int m_recordCount;
    protected readonly int m_fieldCount;
    protected readonly string m_fileName;
    protected DBCRecordConverter converter;
    protected int currentIndex;

    public static void ReadDBC(string fileName)
    {
      DBCReader<TConverter> dbcReader = new DBCReader<TConverter>(fileName);
    }

    public DBCReader(string fileName)
    {
      if(!File.Exists(fileName))
        throw new FileNotFoundException("The required DBC file \"" + fileName + "\" was not found.");
      m_fileName = fileName;
      using(FileStream fileStream = new FileStream(m_fileName, FileMode.Open, FileAccess.Read))
      {
        using(BinaryReader binReader = new BinaryReader(fileStream))
        {
          if(binReader.ReadUInt32() != 1128416343U)
            throw new InvalidDataException("Not a (W)DBC file.");
          m_recordCount = binReader.ReadInt32();
          m_fieldCount = binReader.ReadInt32();
          m_recordSize = binReader.ReadInt32();
          int count = binReader.ReadInt32();
          binReader.BaseStream.Position = binReader.BaseStream.Length - count;
          byte[] stringTable = binReader.ReadBytes(count);
          using(converter = Activator.CreateInstance<TConverter>())
          {
            converter.Init(stringTable);
            InitReader();
            MapRecords(binReader);
          }
        }
      }
    }

    public int RecordSize
    {
      get { return m_recordSize; }
    }

    public int FieldCount
    {
      get { return m_fieldCount; }
    }

    public string FileName
    {
      get { return m_fileName; }
    }

    protected virtual void InitReader()
    {
    }

    private void MapRecords(BinaryReader binReader)
    {
      try
      {
        binReader.BaseStream.Position = 20L;
        for(currentIndex = 0; currentIndex < m_recordCount; ++currentIndex)
          Convert(binReader.ReadBytes(m_recordSize));
      }
      catch(Exception ex)
      {
        throw new Exception(
          "Error when reading DBC-file \"" + m_fileName + "\" (Required client version: " +
          WCellInfo.RequiredVersion + ")", ex);
      }
    }

    protected virtual void Convert(byte[] bytes)
    {
      converter.Convert(bytes);
    }
  }
}