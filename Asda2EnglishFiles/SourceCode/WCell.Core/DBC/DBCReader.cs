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
            if (!File.Exists(fileName))
                throw new FileNotFoundException("The required DBC file \"" + fileName + "\" was not found.");
            this.m_fileName = fileName;
            using (FileStream fileStream = new FileStream(this.m_fileName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader binReader = new BinaryReader((Stream) fileStream))
                {
                    if (binReader.ReadUInt32() != 1128416343U)
                        throw new InvalidDataException("Not a (W)DBC file.");
                    this.m_recordCount = binReader.ReadInt32();
                    this.m_fieldCount = binReader.ReadInt32();
                    this.m_recordSize = binReader.ReadInt32();
                    int count = binReader.ReadInt32();
                    binReader.BaseStream.Position = binReader.BaseStream.Length - (long) count;
                    byte[] stringTable = binReader.ReadBytes(count);
                    using (this.converter = (DBCRecordConverter) Activator.CreateInstance<TConverter>())
                    {
                        this.converter.Init(stringTable);
                        this.InitReader();
                        this.MapRecords(binReader);
                    }
                }
            }
        }

        public int RecordSize
        {
            get { return this.m_recordSize; }
        }

        public int FieldCount
        {
            get { return this.m_fieldCount; }
        }

        public string FileName
        {
            get { return this.m_fileName; }
        }

        protected virtual void InitReader()
        {
        }

        private void MapRecords(BinaryReader binReader)
        {
            try
            {
                binReader.BaseStream.Position = 20L;
                for (this.currentIndex = 0; this.currentIndex < this.m_recordCount; ++this.currentIndex)
                    this.Convert(binReader.ReadBytes(this.m_recordSize));
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error when reading DBC-file \"" + this.m_fileName + "\" (Required client version: " +
                    (object) WCellInfo.RequiredVersion + ")", ex);
            }
        }

        protected virtual void Convert(byte[] bytes)
        {
            this.converter.Convert(bytes);
        }
    }
}