using System;

namespace WCell.Util
{
    /// <summary>
    /// </summary>
    public class CustomGetterSetter : IGetterSetter
    {
        private readonly Func<object, object> m_Getter;
        private readonly Action<object, object> m_Setter;

        public CustomGetterSetter(Func<object, object> getter, Action<object, object> setter)
        {
            this.m_Getter = getter;
            this.m_Setter = setter;
        }

        public object Get(object key)
        {
            return this.m_Getter(key);
        }

        public void Set(object key, object value)
        {
            this.m_Setter(key, value);
        }
    }
}