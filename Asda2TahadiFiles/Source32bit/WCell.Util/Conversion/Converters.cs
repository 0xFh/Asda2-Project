using System;

namespace WCell.Util.Conversion
{
  public static class Converters
  {
    public static IConverterProvider Provider { get; set; }

    public static IFieldReader GetReader(string typeName)
    {
      if(typeName == null)
        return null;
      return GetReader(Utility.GetType(typeName));
    }

    public static IFieldReader GetReader(Type type)
    {
      if(Provider == null)
        throw new InvalidOperationException("Provider must be set before accessing any Type-readers.");
      return Provider.GetReader(type);
    }
  }
}