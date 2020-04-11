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
      SimplePersistors[typeof(int)] = new Int32Persistor();
      SimplePersistors[typeof(uint)] = new UInt32Persistor();
      SimplePersistors[typeof(short)] = new Int16Persistor();
      SimplePersistors[typeof(ushort)] = new UInt16Persistor();
      SimplePersistors[typeof(byte)] = new BytePersistor();
      SimplePersistors[typeof(sbyte)] = new SBytePersistor();
      SimplePersistors[typeof(long)] = new Int64Persistor();
      SimplePersistors[typeof(ulong)] = new UInt64Persistor();
      SimplePersistors[typeof(float)] = new FloatPersistor();
      SimplePersistors[typeof(double)] = new DoublePersistor();
      SimplePersistors[typeof(string)] = new StringPersistor();
      SimplePersistors[typeof(bool)] = new BoolPersistor();
    }

    public static ISimpleBinaryPersistor GetSimplePersistor(Type type)
    {
      if(type.IsEnum)
        type = Enum.GetUnderlyingType(type);
      ISimpleBinaryPersistor simpleBinaryPersistor;
      SimplePersistors.TryGetValue(type, out simpleBinaryPersistor);
      if(simpleBinaryPersistor is FloatPersistor)
        type.ToString();
      return simpleBinaryPersistor;
    }

    /// <summary>Returns null if its a String field</summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public static IBinaryPersistor GetPersistor(IDataField field)
    {
      if(field.DataFieldType == DataFieldType.FlatArray || field.DataFieldType == DataFieldType.NestedArray)
        return new ArrayPersistor((ArrayDataField) field);
      return GetPersistorNoArray(field);
    }

    public static IBinaryPersistor GetPersistorNoArray(IDataField field)
    {
      Type actualType = field.MappedMember.GetActualType();
      if(field is INestedDataField)
        return new NestedPersistor((INestedDataField) field);
      ISimpleBinaryPersistor simplePersistor = GetSimplePersistor(actualType);
      if(simplePersistor == null)
        throw new DataHolderException("Simple Type did not have a binary Persistor: " + actualType.FullName);
      return simplePersistor;
    }
  }
}