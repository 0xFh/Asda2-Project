using System.Reflection;
using WCell.Util.DB;

namespace WCell.Util.Data
{
    public interface IDataField : IDataFieldBase
    {
        string Name { get; }

        string FullName { get; }

        IGetterSetter Accessor { get; }

        MemberInfo MappedMember { get; }

        DataFieldType DataFieldType { get; }
    }
}