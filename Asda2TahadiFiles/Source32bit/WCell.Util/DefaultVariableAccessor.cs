using System;
using System.Reflection;

namespace WCell.Util
{
  public class DefaultVariableAccessor : IGetterSetter
  {
    private MemberInfo m_member;

    public DefaultVariableAccessor(MemberInfo member)
    {
      if((object) (member as FieldInfo) == null && !(member is PropertyInfo))
        throw new Exception("Invalid member: " + member);
      m_member = member;
    }

    public object Get(object key)
    {
      if(m_member is FieldInfo)
        return ((FieldInfo) m_member).GetValue(key);
      if(m_member is PropertyInfo)
        return ((PropertyInfo) m_member).GetValue(key, new object[0]);
      throw new Exception("Invalid member: " + m_member);
    }

    public void Set(object key, object value)
    {
      if(m_member is FieldInfo)
      {
        ((FieldInfo) m_member).SetValue(key, value);
      }
      else
      {
        if(!(m_member is PropertyInfo))
          return;
        ((PropertyInfo) m_member).SetValue(key, value, new object[0]);
      }
    }
  }
}