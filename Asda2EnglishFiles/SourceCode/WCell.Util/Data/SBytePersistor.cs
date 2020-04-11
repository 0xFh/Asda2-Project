using System.IO;

namespace WCell.Util.Data
{
    public class SBytePersistor : SimpleBinaryPersistor
    {
        public override int BinaryLength
        {
            get { return 1; }
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            writer.Write((sbyte) obj);
        }

        public override object Read(BinaryReader reader)
        {
            return (object) reader.ReadSByte();
        }
    }
}