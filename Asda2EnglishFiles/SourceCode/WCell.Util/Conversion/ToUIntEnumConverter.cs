using System;

namespace WCell.Util.Conversion
{
    public class ToUIntEnumConverter : ToUIntConverter
    {
        private readonly Type m_EnumType;

        public ToUIntEnumConverter(Type enumType)
        {
            this.m_EnumType = enumType;
        }

        public Type EnumType
        {
            get { return this.m_EnumType; }
        }

        public override object Convert(object input)
        {
            return (object) (uint) base.Convert(input);
        }
    }
}