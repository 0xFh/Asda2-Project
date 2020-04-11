using System.IO;

namespace WCell.Util.Data
{
    public class Int32Persistor : SimpleBinaryPersistor
    {
        public override int BinaryLength
        {
            get { return 4; }
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            writer.Write((int) obj);
        }

        public override object Read(BinaryReader reader)
        {
            return (object) reader.ReadInt32();
        }
    }
}