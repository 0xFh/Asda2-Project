using System.IO;

namespace WCell.Util.Data
{
    public interface IBinaryPersistor
    {
        void Write(BinaryWriter writer, object obj);

        object Read(BinaryReader reader);
    }
}