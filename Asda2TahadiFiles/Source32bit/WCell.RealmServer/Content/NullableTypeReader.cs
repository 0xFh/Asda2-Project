using NHibernate.Type;
using System.Data;
using WCell.Util.Conversion;

namespace WCell.RealmServer.Content
{
  public class NullableTypeReader : IFieldReader
  {
    private readonly NullableType m_Type;

    public NullableTypeReader(NullableType type)
    {
      m_Type = type;
    }

    public NullableType Type
    {
      get { return m_Type; }
    }

    public object Read(IDataReader reader, int index)
    {
      return m_Type.Get(reader, index);
    }
  }
}