using System;
using System.IO;

namespace WCell.Util.Data
{
  public class NestedPersistor : IComplexBinaryPersistor, IBinaryPersistor
  {
    private readonly INestedDataField m_DataField;
    private IBinaryPersistor[] m_UnderlyingPersistors;
    private IGetterSetter[] m_accessors;

    public NestedPersistor(INestedDataField dataField)
    {
      m_DataField = dataField;
      m_UnderlyingPersistors = new IBinaryPersistor[m_DataField.InnerFields.Count];
      m_accessors = new IGetterSetter[m_DataField.InnerFields.Count];
      int index = 0;
      foreach(IDataField field in m_DataField.InnerFields.Values)
      {
        IBinaryPersistor persistor = BinaryPersistors.GetPersistor(field);
        m_UnderlyingPersistors[index] = persistor;
        m_accessors[index] = field.Accessor;
        ++index;
      }
    }

    public INestedDataField DataField
    {
      get { return m_DataField; }
    }

    public IBinaryPersistor[] UnderlyingPersistors
    {
      get { return m_UnderlyingPersistors; }
    }

    public void Write(BinaryWriter writer, object obj)
    {
      if(obj == null)
        obj = m_DataField.Producer.Produce();
      for(int index = 0; index < m_UnderlyingPersistors.Length; ++index)
      {
        IBinaryPersistor underlyingPersistor = m_UnderlyingPersistors[index];
        object obj1 = m_accessors[index].Get(obj);
        underlyingPersistor.Write(writer, obj1);
      }
    }

    public object Read(BinaryReader reader)
    {
      object obj = null;
      Read(reader, ref obj);
      return obj;
    }

    public void Read(BinaryReader reader, ref object obj)
    {
      if(obj == null)
        obj = m_DataField.Producer == null
          ? Activator.CreateInstance(m_DataField.BelongsTo.GetActualType())
          : m_DataField.Producer.Produce();
      for(int index = 0; index < m_UnderlyingPersistors.Length; ++index)
      {
        object obj1 = m_UnderlyingPersistors[index].Read(reader);
        m_accessors[index].Set(obj, obj1);
      }
    }
  }
}