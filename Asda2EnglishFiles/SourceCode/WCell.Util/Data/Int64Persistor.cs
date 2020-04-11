using System.IO;

namespace WCell.Util.Data
{
    public class Int64Persistor : SimpleBinaryPersistor
    {
        public override int BinaryLength
        {
            get { return 8; }
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            writer.Write((long) obj);
        }

        public override object Read(BinaryReader reader)
        {
            return (object) reader.ReadInt64();
        }
    }
}