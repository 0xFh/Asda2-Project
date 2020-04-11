using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WCell.Util.Conversion;
using WCell.Util.DynamicAccess;

namespace WCell.Util.Data
{
  /// <summary>
  /// Contains Metadata about types that are saved/loaded to/from a persistent storage.
  /// </summary>
  public class DataHolderDefinition
  {
    public readonly Dictionary<string, IDataField> Fields =
      new Dictionary<string, IDataField>(StringComparer.InvariantCultureIgnoreCase);

    private const int DynamicMaximumMembers = 80;
    public const string CacheGetterName = "GetAllDataHolders";
    private readonly string m_name;
    private readonly string m_DependingFieldName;
    private readonly DataHolderAttribute m_Attribute;
    private readonly Type m_Type;
    private readonly IProducer m_defaultProducer;
    private readonly Dictionary<object, IProducer> m_dependingProducers;
    private FlatSimpleDataField m_DependingField;
    private MethodInfo m_cacheGetter;

    public DataHolderDefinition(string name, Type type, string dependingField, DataHolderAttribute attribute)
    {
      m_name = name;
      m_DependingFieldName = dependingField;
      m_Attribute = attribute;
      m_Type = type;
      DependingProducer[] customAttributes = type.GetCustomAttributes<DependingProducer>();
      if(customAttributes.Length == 0)
      {
        m_dependingProducers = null;
      }
      else
      {
        m_dependingProducers = new Dictionary<object, IProducer>();
        foreach(DependingProducer dependingProducer in customAttributes)
          m_dependingProducers.Add(dependingProducer.Key, dependingProducer.Producer);
      }

      if(type.IsAbstract)
      {
        if(m_dependingProducers == null)
          throw new DataHolderException(
            "Cannot define DataHolder because it's Type is abstract and it did not define depending Producers: {0}",
            (object) type.FullName);
        if(m_DependingFieldName == null)
          throw new DataHolderException(
            "Cannot define DataHolder because it's Type is abstract and it did not define the DependsOnField in the DataHolderAttribute: {0}",
            (object) type.FullName);
      }
      else
        m_defaultProducer = new DefaultProducer(type);

      try
      {
        GetDataFields(type, Fields, null);
        if(type.IsAbstract && m_DependingField == null)
          throw new DataHolderException(
            "Cannot define DataHolder because it's DependsOnField (\"{0}\"), as defined in the DataHolderAttribute, does not exist: {1}",
            (object) m_DependingFieldName, (object) type.FullName);
      }
      catch(Exception ex)
      {
        throw new DataHolderException(ex, "Unable to create DataHolderDefinition for: " + name);
      }
    }

    /// <summary>
    /// The name of the field whose values decides which Producer to use.
    /// </summary>
    public string DependingFieldName
    {
      get { return m_DependingFieldName; }
    }

    /// <summary>The field whose values decides which Producer to use.</summary>
    public FlatSimpleDataField DependingField
    {
      get { return m_DependingField; }
    }

    /// <summary>
    /// The Type of the DataHolder-class, defined through this.
    /// </summary>
    public Type Type
    {
      get { return m_Type; }
    }

    public string Name
    {
      get { return m_name; }
    }

    public bool SupportsCaching
    {
      get { return CacheGetter != null; }
    }

    /// <summary>
    /// The method that will yield all DataHolders of
    /// this DataHolder's type.
    /// </summary>
    public MethodInfo CacheGetter
    {
      get
      {
        if(m_cacheGetter == null)
        {
          m_cacheGetter = Type.GetMethod("GetAllDataHolders");
          if(m_cacheGetter != null)
          {
            if(!m_cacheGetter.IsStatic)
              throw new DataHolderException("Getter {0} must be static.", (object) m_cacheGetter.GetFullMemberName());
            Type returnType = m_cacheGetter.ReturnType;
            Type type = returnType.GetInterfaces().FirstOrDefault();
            if(Type !=
               returnType.GetGenericArguments().FirstOrDefault() ||
               type == null || !type.Name.StartsWith("IEnumerable"))
              throw new DataHolderException(
                "Getter {0} has wrong Type \"{1}\" - Expected: IEnumerable<{2}>",
                (object) m_cacheGetter.GetFullMemberName(), (object) returnType.FullName, (object) Type.Name);
          }
        }

        return m_cacheGetter;
      }
    }

    public IDataField GetField(string name)
    {
      IDataField dataField;
      Fields.TryGetValue(name, out dataField);
      return dataField;
    }

    public object CreateHolder(object firstValue)
    {
      if(m_dependingProducers != null)
      {
        Type variableType = m_DependingField.MappedMember.GetVariableType();
        Type type = firstValue.GetType();
        if(type != variableType && variableType.IsEnum)
        {
          Type underlyingType = Enum.GetUnderlyingType(variableType);
          if(type != variableType)
            firstValue = Convert.ChangeType(firstValue, underlyingType);
          firstValue = Enum.Parse(variableType, firstValue.ToString());
        }

        IProducer producer;
        if(m_dependingProducers.TryGetValue(firstValue, out producer))
          return producer.Produce();
      }

      if(m_defaultProducer == null)
        throw new DataHolderException(
          "Could not create DataHolder \"{0}\" because Value \"{1}\" did not have a Producer assigned (Make sure that the Types match)",
          (object) this, firstValue is Array
            ? (object) ((object[]) firstValue).ToString(", ")
            : firstValue);
      return m_defaultProducer.Produce();
    }

    public static IProducer CreateProducer(Type type)
    {
      return new DefaultProducer(type);
    }

    private static IProducer CreateArrayProducer(Type type, int length)
    {
      return new DefaultArrayProducer(type, length);
    }

    private void GetDataFields(Type type, IDictionary<string, IDataField> fields, INestedDataField parent)
    {
      MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetField |
                                             BindingFlags.SetProperty);
      int length = members.Length;
      Dictionary<MemberInfo, IGetterSetter> dictionary1 = length >= 80 || !type.IsClass
        ? null
        : AccessorMgr.GetOrCreateAccessors(type);
      foreach(MemberInfo index1 in members)
      {
        if(!index1.IsReadonly() && index1.GetCustomAttributes<NotPersistentAttribute>().Length <= 0)
        {
          PersistentAttribute attr =
            index1.GetCustomAttributes<DBAttribute>()
              .Where(attribute => attribute is PersistentAttribute)
              .FirstOrDefault() as PersistentAttribute;
          if(attr != null || !m_Attribute.RequirePersistantAttr)
          {
            Type variableType = index1.GetVariableType();
            bool isArray = variableType.IsArray;
            Type type1;
            string name;
            IGetterSetter accessor;
            IFieldReader reader;
            if(attr != null)
            {
              Type actualType = attr.ActualType;
              if((object) actualType == null)
                actualType = index1.GetActualType();
              type1 = actualType;
              name = attr.Name ?? index1.Name;
              if(attr.AccessorType != null)
              {
                object instance = Activator.CreateInstance(attr.AccessorType);
                if(!(instance is IGetterSetter))
                  throw new DataHolderException(
                    "Accessor for Persistent members must be of type IGetterSetter - Found accessor of type {0} for member {1}",
                    (object) instance.GetType(), (object) index1.GetFullMemberName());
                accessor = (IGetterSetter) instance;
              }
              else
                accessor = dictionary1 != null
                  ? dictionary1[index1]
                  : new DefaultVariableAccessor(index1);

              Type type2 = attr.ReadType;
              if((object) type2 == null)
                type2 = type1;
              reader = Converters.GetReader(type2);
            }
            else
            {
              type1 = index1.GetActualType();
              name = index1.Name;
              accessor = dictionary1 != null
                ? dictionary1[index1]
                : new DefaultVariableAccessor(index1);
              reader = Converters.GetReader(type1);
            }

            if(isArray && (variableType.GetArrayRank() > 1 || type1.IsArray))
              throw new DataHolderException(
                "Cannot define Type {0} of {1} because its a multi-dimensional Array.", (object) variableType,
                (object) index1.GetFullMemberName());
            IDataField dataField1;
            if(reader == null)
            {
              if(type.IsAbstract)
                throw new DataHolderException(
                  "Cannot define member \"{0}\" of DataHolder \"{1}\" because it's Type ({2}) is abstract.",
                  (object) index1.GetFullMemberName(), (object) this, (object) type1.FullName);
              IProducer producer = !type1.IsClass
                ? null
                : CreateProducer(type1);
              if(isArray)
              {
                int arrayLengthByAttr = GetArrayLengthByAttr(attr, index1);
                NestedArrayDataField nestedArrayDataField = new NestedArrayDataField(this, name,
                  accessor, index1, producer,
                  CreateArrayProducer(type1, arrayLengthByAttr),
                  arrayLengthByAttr, parent);
                Dictionary<string, IDataField> dictionary2 =
                  new Dictionary<string, IDataField>(
                    StringComparer.InvariantCultureIgnoreCase);
                GetDataFields(type1, dictionary2,
                  nestedArrayDataField);
                foreach(IDataField dataField2 in dictionary2.Values)
                {
                  for(int index2 = 0; index2 < nestedArrayDataField.ArrayAccessors.Length; ++index2)
                  {
                    NestedArrayAccessor arrayAccessor =
                      (NestedArrayAccessor) nestedArrayDataField.ArrayAccessors[index2];
                    IDataField dataField3 =
                      ((DataFieldBase) dataField2).Copy(arrayAccessor);
                    arrayAccessor.InnerFields.Add(dataField3.Name, dataField3);
                  }
                }

                dataField1 = nestedArrayDataField;
              }
              else
              {
                NestedSimpleDataField nestedSimpleDataField =
                  new NestedSimpleDataField(this, name, accessor, index1, producer, parent);
                GetDataFields(type1,
                  nestedSimpleDataField.InnerFields,
                  nestedSimpleDataField);
                if(nestedSimpleDataField.InnerFields.Count == 0)
                  throw new DataHolderException(
                    "Cannot define " + index1.GetFullMemberName() +
                    " as Nested because it does not have any inner fields.");
                dataField1 = nestedSimpleDataField;
              }
            }
            else if(isArray)
            {
              int arrayLengthByAttr = GetArrayLengthByAttr(attr, index1);
              dataField1 = new FlatArrayDataField(this, name, accessor, index1,
                arrayLengthByAttr, CreateArrayProducer(type1, arrayLengthByAttr),
                parent);
            }
            else
            {
              dataField1 = new FlatSimpleDataField(this, name, accessor, index1, parent);
              if(name == m_DependingFieldName)
                m_DependingField = (FlatSimpleDataField) dataField1;
            }

            fields.Add(dataField1.Name, dataField1);
          }
        }
      }

      if(fields.Count() - length == 0)
        throw new ArgumentException("Invalid data Type has no persistent members: " + type.FullName);
    }

    private static int GetArrayLengthByAttr(PersistentAttribute attr, MemberInfo member)
    {
      int num = attr != null ? attr.Length : 0;
      if(num < 1)
        throw new DataHolderException(
          "Cannot define Array-member {0} because it did not define a minimal length through the PersistentAttribute.",
          (object) member);
      return num;
    }

    public string CreateIdString()
    {
      StringBuilder stringBuilder = new StringBuilder(Fields.Values.Count * 15);
      foreach(IDataField dataField in Fields.Values)
        stringBuilder.Append(string.Format("{0}:{1},", dataField.MappedMember.GetActualType().Name,
          dataField.Name));
      return stringBuilder.ToString();
    }

    public override string ToString()
    {
      return m_name;
    }

    public IEnumerator GetEnumerator()
    {
      return Fields.Values.GetEnumerator();
    }
  }
}