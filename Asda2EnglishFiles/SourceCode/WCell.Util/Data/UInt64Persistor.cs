using System.IO;

namespace WCell.Util.Data
{
    public class UInt64Persistor : SimpleBinaryPersistor
    {
        public override int BinaryLength
        {
            get { return 8; }
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            writer.Write((ulong) obj);
        }

        public override object Read(BinaryReader reader)
        {
            return (object) reader.ReadUInt64();
        }
    }
}