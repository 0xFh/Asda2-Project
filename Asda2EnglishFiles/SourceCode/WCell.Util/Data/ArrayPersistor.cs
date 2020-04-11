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
            this.m_DataField = field;
            this.m_UnderlyingPersistor = BinaryPersistors.GetPersistorNoArray((IDataField) this.m_DataField);
        }

        public ArrayDataField DataField
        {
            get { return this.m_DataField; }
        }

        public IBinaryPersistor UnderlyingPersistor
        {
            get { return this.m_UnderlyingPersistor; }
        }

        public void Write(BinaryWriter writer, object obj)
        {
            int index = 0;
            if (obj != null)
            {
                for (; index < ((Array) obj).Length; ++index)
                {
                    object obj1 = ((Array) obj).GetValue(index);
                    this.m_UnderlyingPersistor.Write(writer, obj1);
                }
            }

            if (index >= this.m_DataField.Length)
                return;
            Type actualMemberType = this.m_DataField.ActualMemberType;
            object obj2 = !(actualMemberType == typeof(string))
                ? Activator.CreateInstance(actualMemberType)
                : (object) "";
            for (; index < this.m_DataField.Length; ++index)
                this.m_UnderlyingPersistor.Write(writer, obj2);
        }

        public object Read(BinaryReader reader)
        {
            Array arr = (Array) this.m_DataField.ArrayProducer.Produce();
            for (int index = 0; index < this.m_DataField.Length; ++index)
            {
                object obj;
                if (this.m_UnderlyingPersistor is NestedPersistor)
                {
                    obj = arr.GetValue(index);
                    ((NestedPersistor) this.m_UnderlyingPersistor).Read(reader, ref obj);
                }
                else
                    obj = this.m_UnderlyingPersistor.Read(reader);

                ArrayUtil.SetValue(arr, index, obj);
            }

            return (object) arr;
        }
    }
}