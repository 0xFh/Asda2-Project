using System.IO;

namespace WCell.Util.Data
{
    public class FloatPersistor : SimpleBinaryPersistor
    {
        public override int BinaryLength
        {
            get { return 4; }
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            writer.Write((float) obj);
        }

        public override object Read(BinaryReader reader)
        {
            return (object) reader.ReadSingle();
        }
    }
}