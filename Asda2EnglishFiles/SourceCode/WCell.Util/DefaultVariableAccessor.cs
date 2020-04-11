using System;
using System.Reflection;

namespace WCell.Util
{
    public class DefaultVariableAccessor : IGetterSetter
    {
        private MemberInfo m_member;

        public DefaultVariableAccessor(MemberInfo member)
        {
            if ((object) (member as FieldInfo) == null && !(member is PropertyInfo))
                throw new Exception("Invalid member: " + (object) member);
            this.m_member = member;
        }

        public object Get(object key)
        {
            if (this.m_member is FieldInfo)
                return ((FieldInfo) this.m_member).GetValue(key);
            if (this.m_member is PropertyInfo)
                return ((PropertyInfo) this.m_member).GetValue(key, new object[0]);
            throw new Exception("Invalid member: " + (object) this.m_member);
        }

        public void Set(object key, object value)
        {
            if (this.m_member is FieldInfo)
            {
                ((FieldInfo) this.m_member).SetValue(key, value);
            }
            else
            {
                if (!(this.m_member is PropertyInfo))
                    return;
                ((PropertyInfo) this.m_member).SetValue(key, value, new object[0]);
            }
        }
    }
}