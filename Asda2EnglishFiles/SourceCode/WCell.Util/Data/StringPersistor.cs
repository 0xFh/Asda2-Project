using System.IO;

namespace WCell.Util.Data
{
    public class StringPersistor : SimpleBinaryPersistor
    {
        /// <summary>Redundant</summary>
        public override int BinaryLength
        {
            get { return -1; }
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            if (string.IsNullOrEmpty(obj as string))
            {
                writer.Write((ushort) 0);
            }
            else
            {
                byte[] bytes = BinaryPersistors.DefaultEncoding.GetBytes((string) obj);
                writer.Write((ushort) bytes.Length);
                writer.Write(bytes);
            }
        }

        public override object Read(BinaryReader reader)
        {
            ushort num = reader.ReadUInt16();
            if (num == (ushort) 0)
                return (object) "";
            byte[] bytes = reader.ReadBytes((int) num);
            return (object) BinaryPersistors.DefaultEncoding.GetString(bytes);
        }
    }
}