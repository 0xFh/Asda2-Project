using System.Reflection;
using WCell.Util.DB;

namespace WCell.Util.Data
{
  public class FlatSimpleDataField : DataField, IFlatDataFieldAccessor, IDataFieldAccessor
  {
    public FlatSimpleDataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor,
      MemberInfo mappedMember, INestedDataField parent)
      : base(dataHolder, name, accessor, mappedMember, parent)
    {
    }

    public object Get(IDataHolder obj)
    {
      return m_accessor.Get(GetTargetObject(obj));
    }

    public void Set(IDataHolder obj, object value)
    {
      object key = GetTargetObject(obj);
      m_accessor.Set(key, value);
      if(!key.GetType().IsValueType)
        return;
      INestedDataField parent = m_parent;
      while(parent != null)
      {
        object targetObject = parent.GetTargetObject(obj);
        parent.Accessor.Set(targetObject, key);
        key = targetObject;
        parent = parent.Parent;
        if(!targetObject.GetType().IsValueType)
          break;
      }
    }

    public override DataFieldType DataFieldType
    {
      get { return DataFieldType.FlatSimple; }
    }

    public override string ToString()
    {
      return Name;
    }

    public override IDataField Copy(INestedDataField parent)
    {
      return new FlatSimpleDataField(m_DataHolderDefinition, m_name, m_accessor,
        m_mappedMember, parent);
    }
  }
}