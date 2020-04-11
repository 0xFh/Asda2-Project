using NHibernate;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using WCell.Util.Conversion;

namespace WCell.RealmServer.Content
{
  public class NHibernateConverterProvider : IConverterProvider
  {
    public static readonly Dictionary<Type, IConverter> StandardConverters =
      new Dictionary<Type, IConverter>();

    static NHibernateConverterProvider()
    {
      StandardConverters[typeof(int)] = new ToIntConverter();
      StandardConverters[typeof(uint)] = new ToUIntConverter();
      StandardConverters[typeof(string)] = new ToStringConverter();
    }

    private static bool IsNullableEnum(Type typeClass)
    {
      if(!typeClass.IsGenericType || !typeof(Nullable<>).Equals(typeClass.GetGenericTypeDefinition()))
        return false;
      return typeClass.GetGenericArguments()[0].IsSubclassOf(typeof(Enum));
    }

    public IConverter GetStandardConverter(Type type)
    {
      IConverter converter;
      StandardConverters.TryGetValue(type, out converter);
      return converter;
    }

    public IFieldReader GetReader(Type type)
    {
      IConverter standardConverter = GetStandardConverter(type);
      if(standardConverter != null)
        return new CustomReader(standardConverter);
      if(type.IsEnum && Enum.GetUnderlyingType(type) == typeof(uint))
        return new CustomReader(new ToUIntEnumConverter(type));
      IType type1 = TypeFactory.Basic(type.FullName);
      if(type.IsEnum)
        type1 = NHibernateUtil.Enum(type);
      else if(IsNullableEnum(type))
        type1 = NHibernateUtil.Enum(type.GetGenericArguments()[0]);
      if(type1 == null)
        return null;
      if(!(type1 is NullableType))
        throw new ArgumentException("Invalid Type must be nullable - Found: " + type1);
      return new NullableTypeReader((NullableType) type1);
    }
  }
}