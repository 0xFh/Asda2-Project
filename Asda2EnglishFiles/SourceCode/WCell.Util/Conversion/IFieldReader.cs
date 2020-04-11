using System.Data;

namespace WCell.Util.Conversion
{
    public interface IFieldReader
    {
        object Read(IDataReader reader, int index);
    }
}