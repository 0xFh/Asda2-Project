using System;
using System.Collections.Generic;
using System.Text;
using WCell.Util.DB;

namespace WCell.Util.Data
{
    public static class BinaryPersistors
    {
        public static readonly Dictionary<Type, ISimpleBinaryPersistor> SimplePersistors =
            new Dictionary<Type, ISimpleBinaryPersistor>();

        public static Encoding DefaultEncoding = Encoding.UTF8;

        static BinaryPersistors()
        {
            BinaryPersistors.SimplePersistors[typeof(int)] = (ISimpleBinaryPersistor) new Int32Persistor();
            BinaryPersistors.SimplePersistors[typeof(uint)] = (ISimpleBinaryPersistor) new UInt32Persistor();
            BinaryPersistors.SimplePersistors[typeof(short)] = (ISimpleBinaryPersistor) new Int16Persistor();
            BinaryPersistors.SimplePersistors[typeof(ushort)] = (ISimpleBinaryPersistor) new UInt16Persistor();
            BinaryPersistors.SimplePersistors[typeof(byte)] = (ISimpleBinaryPersistor) new BytePersistor();
            BinaryPersistors.SimplePersistors[typeof(sbyte)] = (ISimpleBinaryPersistor) new SBytePersistor();
            BinaryPersistors.SimplePersistors[typeof(long)] = (ISimpleBinaryPersistor) new Int64Persistor();
            BinaryPersistors.SimplePersistors[typeof(ulong)] = (ISimpleBinaryPersistor) new UInt64Persistor();
            BinaryPersistors.SimplePersistors[typeof(float)] = (ISimpleBinaryPersistor) new FloatPersistor();
            BinaryPersistors.SimplePersistors[typeof(double)] = (ISimpleBinaryPersistor) new DoublePersistor();
            BinaryPersistors.SimplePersistors[typeof(string)] = (ISimpleBinaryPersistor) new StringPersistor();
            BinaryPersistors.SimplePersistors[typeof(bool)] = (ISimpleBinaryPersistor) new BoolPersistor();
        }

        public static ISimpleBinaryPersistor GetSimplePersistor(Type type)
        {
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);
            ISimpleBinaryPersistor simpleBinaryPersistor;
            BinaryPersistors.SimplePersistors.TryGetValue(type, out simpleBinaryPersistor);
            if (simpleBinaryPersistor is FloatPersistor)
                type.ToString();
            return simpleBinaryPersistor;
        }

        /// <summary>Returns null if its a String field</summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static IBinaryPersistor GetPersistor(IDataField field)
        {
            if (field.DataFieldType == DataFieldType.FlatArray || field.DataFieldType == DataFieldType.NestedArray)
                return (IBinaryPersistor) new ArrayPersistor((ArrayDataField) field);
            return BinaryPersistors.GetPersistorNoArray(field);
        }

        public static IBinaryPersistor GetPersistorNoArray(IDataField field)
        {
            Type actualType = field.MappedMember.GetActualType();
            if (field is INestedDataField)
                return (IBinaryPersistor) new NestedPersistor((INestedDataField) field);
            ISimpleBinaryPersistor simplePersistor = BinaryPersistors.GetSimplePersistor(actualType);
            if (simplePersistor == null)
                throw new DataHolderException("Simple Type did not have a binary Persistor: " + actualType.FullName,
                    new object[0]);
            return (IBinaryPersistor) simplePersistor;
        }
    }
}