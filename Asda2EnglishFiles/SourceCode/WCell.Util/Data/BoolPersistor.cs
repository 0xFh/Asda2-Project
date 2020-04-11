using System.IO;

namespace WCell.Util.Data
{
    public class BoolPersistor : SimpleBinaryPersistor
    {
        public override int BinaryLength
        {
            get { return 1; }
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            writer.Write((bool) obj ? (byte) 1 : (byte) 0);
        }

        public override object Read(BinaryReader reader)
        {
            return (object) (reader.ReadByte() == (byte) 1);
        }
    }
}