using NHibernate;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using WCell.Util.Conversion;

namespace WCell.RealmServer.Content
{
    public class NHibernateConverterProvider : IConverterProvider
    {
        public static readonly Dictionary<System.Type, IConverter> StandardConverters =
            new Dictionary<System.Type, IConverter>();

        static NHibernateConverterProvider()
        {
            NHibernateConverterProvider.StandardConverters[typeof(int)] = (IConverter) new ToIntConverter();
            NHibernateConverterProvider.StandardConverters[typeof(uint)] = (IConverter) new ToUIntConverter();
            NHibernateConverterProvider.StandardConverters[typeof(string)] = (IConverter) new ToStringConverter();
        }

        private static bool IsNullableEnum(System.Type typeClass)
        {
            if (!typeClass.IsGenericType || !typeof(Nullable<>).Equals(typeClass.GetGenericTypeDefinition()))
                return false;
            return typeClass.GetGenericArguments()[0].IsSubclassOf(typeof(Enum));
        }

        public IConverter GetStandardConverter(System.Type type)
        {
            IConverter converter;
            NHibernateConverterProvider.StandardConverters.TryGetValue(type, out converter);
            return converter;
        }

        public IFieldReader GetReader(System.Type type)
        {
            IConverter standardConverter = this.GetStandardConverter(type);
            if (standardConverter != null)
                return (IFieldReader) new CustomReader(standardConverter);
            if (type.IsEnum && Enum.GetUnderlyingType(type) == typeof(uint))
                return (IFieldReader) new CustomReader((IConverter) new ToUIntEnumConverter(type));
            IType type1 = TypeFactory.Basic(type.FullName);
            if (type.IsEnum)
                type1 = NHibernateUtil.Enum(type);
            else if (NHibernateConverterProvider.IsNullableEnum(type))
                type1 = NHibernateUtil.Enum(type.GetGenericArguments()[0]);
            if (type1 == null)
                return (IFieldReader) null;
            if (!(type1 is NullableType))
                throw new ArgumentException("Invalid Type must be nullable - Found: " + (object) type1);
            return (IFieldReader) new NullableTypeReader((NullableType) type1);
        }
    }
}