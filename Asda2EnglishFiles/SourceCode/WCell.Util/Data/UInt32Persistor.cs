using System.IO;

namespace WCell.Util.Data
{
    public class UInt32Persistor : SimpleBinaryPersistor
    {
        public override int BinaryLength
        {
            get { return 4; }
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            writer.Write((uint) obj);
        }

        public override object Read(BinaryReader reader)
        {
            return (object) reader.ReadUInt32();
        }
    }
}