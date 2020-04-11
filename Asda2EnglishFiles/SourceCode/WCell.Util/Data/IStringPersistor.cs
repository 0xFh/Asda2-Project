using System.IO;

namespace WCell.Util.Data
{
    public interface IStringPersistor
    {
        void WriteText(BinaryWriter writer, string text);

        object ReadText(BinaryReader reader);
    }
}