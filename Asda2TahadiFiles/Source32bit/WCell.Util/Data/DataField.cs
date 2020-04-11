using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WCell.Util.DB;

namespace WCell.Util.Data
{
  public abstract class DataField : DataFieldBase, IDataField, IDataFieldBase
  {
    protected MemberInfo m_mappedMember;
    protected string m_name;
    protected string m_fullName;

    protected DataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor,
      MemberInfo mappedMember, INestedDataField parent)
      : base(dataHolder, accessor, parent)
    {
      m_mappedMember = mappedMember;
      m_name = name;
    }

    public string Name
    {
      get { return m_name; }
    }

    public string FullName
    {
      get { return m_fullName; }
    }

    public MemberInfo MappedMember
    {
      get { return m_mappedMember; }
    }

    public Type ActualMemberType
    {
      get
      {
        PersistentAttribute persistentAttribute =
          (PersistentAttribute) m_mappedMember.GetCustomAttributes(
            typeof(PersistentAttribute), true).FirstOrDefault();
        if(persistentAttribute != null && persistentAttribute.ActualType != null)
          return persistentAttribute.ActualType;
        return m_mappedMember.GetActualType();
      }
    }

    public abstract DataFieldType DataFieldType { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rootObject"></param>
    /// <returns></returns>
    public object GetTargetObject(IDataHolder rootObject)
    {
      if(m_parent == null)
        return rootObject;
      object obj = m_parent.Accessor.Get(m_parent.GetTargetObject(rootObject));
      if(obj == null)
      {
        obj = m_parent.Producer.Produce();
        m_parent.Accessor.Set(rootObject, obj);
      }

      return obj;
    }

    public override string ToString()
    {
      return m_name;
    }
  }
}