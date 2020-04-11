using System.IO;

namespace WCell.Util.Data
{
    public class Int16Persistor : SimpleBinaryPersistor
    {
        public override int BinaryLength
        {
            get { return 2; }
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            writer.Write((short) obj);
        }

        public override object Read(BinaryReader reader)
        {
            return (object) reader.ReadInt16();
        }
    }
}