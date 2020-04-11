using System;

namespace WCell.Util.Conversion
{
  public class ToUIntEnumConverter : ToUIntConverter
  {
    private readonly Type m_EnumType;

    public ToUIntEnumConverter(Type enumType)
    {
      m_EnumType = enumType;
    }

    public Type EnumType
    {
      get { return m_EnumType; }
    }

    public override object Convert(object input)
    {
      return (uint) base.Convert(input);
    }
  }
}