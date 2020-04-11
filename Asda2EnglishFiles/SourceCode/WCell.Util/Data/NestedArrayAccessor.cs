using System;
using System.Collections.Generic;
using System.Reflection;

namespace WCell.Util.Data
{
    public class NestedArrayAccessor : INestedDataField, IDataFieldBase, IGetterSetter, IDataFieldAccessor
    {
        private readonly Dictionary<string, IDataField> m_innerFields =
            new Dictionary<string, IDataField>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);

        private readonly NestedArrayDataField m_ArrayField;
        private readonly int m_Index;

        public NestedArrayAccessor(NestedArrayDataField arrayField, int index)
        {
            this.m_ArrayField = arrayField;
            this.m_Index = index;
        }

        public Dictionary<string, IDataField> InnerFields
        {
            get { return this.m_innerFields; }
        }

        public MemberInfo BelongsTo
        {
            get { return this.m_ArrayField.MappedMember; }
        }

        public int Index
        {
            get { return this.m_Index; }
        }

        public NestedArrayDataField ArrayField
        {
            get { return this.m_ArrayField; }
        }

        public INestedDataField Parent
        {
            get { return this.m_ArrayField.Parent; }
        }

        public IGetterSetter Accessor
        {
            get { return (IGetterSetter) this; }
        }

        public DataHolderDefinition DataHolderDefinition
        {
            get { return this.m_ArrayField.DataHolderDefinition; }
        }

        public IProducer Producer
        {
            get { return this.m_ArrayField.Producer; }
        }

        public object GetTargetObject(IDataHolder rootObject)
        {
            return this.m_ArrayField.GetTargetObject(rootObject);
        }

        public override string ToString()
        {
            return this.m_ArrayField.Name + "[" + (object) this.m_Index + "]";
        }

        public object Get(object arrayContainer)
        {
            Array array = this.m_ArrayField.GetArray(arrayContainer);
            object obj = array.GetValue(this.m_Index);
            if (obj == null)
            {
                obj = this.m_ArrayField.Producer.Produce();
                ArrayUtil.SetValue(array, this.m_Index, obj);
            }

            return obj;
        }

        public void Set(object arrayContainer, object value)
        {
            this.m_ArrayField.Set(arrayContainer, this.m_Index, value);
        }
    }
}