using System.IO;

namespace WCell.Util.Data
{
    public class BytePersistor : SimpleBinaryPersistor
    {
        public override int BinaryLength
        {
            get { return 1; }
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            writer.Write((byte) obj);
        }

        public override object Read(BinaryReader reader)
        {
            return (object) reader.ReadByte();
        }
    }
}