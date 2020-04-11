using System;
using System.IO;

namespace WCell.Util.Data
{
  public class ArrayPersistor : IComplexBinaryPersistor, IBinaryPersistor
  {
    private readonly ArrayDataField m_DataField;
    private IBinaryPersistor m_UnderlyingPersistor;

    public ArrayPersistor(ArrayDataField field)
    {
      m_DataField = field;
      m_UnderlyingPersistor = BinaryPersistors.GetPersistorNoArray(m_DataField);
    }

    public ArrayDataField DataField
    {
      get { return m_DataField; }
    }

    public IBinaryPersistor UnderlyingPersistor
    {
      get { return m_UnderlyingPersistor; }
    }

    public void Write(BinaryWriter writer, object obj)
    {
      int index = 0;
      if(obj != null)
      {
        for(; index < ((Array) obj).Length; ++index)
        {
          object obj1 = ((Array) obj).GetValue(index);
          m_UnderlyingPersistor.Write(writer, obj1);
        }
      }

      if(index >= m_DataField.Length)
        return;
      Type actualMemberType = m_DataField.ActualMemberType;
      object obj2 = !(actualMemberType == typeof(string))
        ? Activator.CreateInstance(actualMemberType)
        : "";
      for(; index < m_DataField.Length; ++index)
        m_UnderlyingPersistor.Write(writer, obj2);
    }

    public object Read(BinaryReader reader)
    {
      Array arr = (Array) m_DataField.ArrayProducer.Produce();
      for(int index = 0; index < m_DataField.Length; ++index)
      {
        object obj;
        if(m_UnderlyingPersistor is NestedPersistor)
        {
          obj = arr.GetValue(index);
          ((NestedPersistor) m_UnderlyingPersistor).Read(reader, ref obj);
        }
        else
          obj = m_UnderlyingPersistor.Read(reader);

        ArrayUtil.SetValue(arr, index, obj);
      }

      return arr;
    }
  }
}