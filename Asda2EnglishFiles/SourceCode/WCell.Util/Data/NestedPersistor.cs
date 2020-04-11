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
            this.m_DataField = dataField;
            this.m_UnderlyingPersistors = new IBinaryPersistor[this.m_DataField.InnerFields.Count];
            this.m_accessors = new IGetterSetter[this.m_DataField.InnerFields.Count];
            int index = 0;
            foreach (IDataField field in this.m_DataField.InnerFields.Values)
            {
                IBinaryPersistor persistor = BinaryPersistors.GetPersistor(field);
                this.m_UnderlyingPersistors[index] = persistor;
                this.m_accessors[index] = field.Accessor;
                ++index;
            }
        }

        public INestedDataField DataField
        {
            get { return this.m_DataField; }
        }

        public IBinaryPersistor[] UnderlyingPersistors
        {
            get { return this.m_UnderlyingPersistors; }
        }

        public void Write(BinaryWriter writer, object obj)
        {
            if (obj == null)
                obj = this.m_DataField.Producer.Produce();
            for (int index = 0; index < this.m_UnderlyingPersistors.Length; ++index)
            {
                IBinaryPersistor underlyingPersistor = this.m_UnderlyingPersistors[index];
                object obj1 = this.m_accessors[index].Get(obj);
                underlyingPersistor.Write(writer, obj1);
            }
        }

        public object Read(BinaryReader reader)
        {
            object obj = (object) null;
            this.Read(reader, ref obj);
            return obj;
        }

        public void Read(BinaryReader reader, ref object obj)
        {
            if (obj == null)
                obj = this.m_DataField.Producer == null
                    ? Activator.CreateInstance(this.m_DataField.BelongsTo.GetActualType())
                    : this.m_DataField.Producer.Produce();
            for (int index = 0; index < this.m_UnderlyingPersistors.Length; ++index)
            {
                object obj1 = this.m_UnderlyingPersistors[index].Read(reader);
                this.m_accessors[index].Set(obj, obj1);
            }
        }
    }
}