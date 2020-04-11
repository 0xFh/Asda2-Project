using System;
using System.Reflection;

namespace WCell.Util.Data
{
    public abstract class ArrayDataField : DataField, IIndexedGetterSetter
    {
        protected int m_length;
        protected readonly IProducer m_arrProducer;
        protected IDataFieldAccessor[] m_ArrayAccessors;

        protected ArrayDataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor,
            MemberInfo mappedMember, INestedDataField parent, int length, IProducer arrProducer)
            : base(dataHolder, name, accessor, mappedMember, parent)
        {
            this.m_length = length;
            this.m_arrProducer = arrProducer;
        }

        /// <summary>The minimal required length of this Array</summary>
        public int Length
        {
            get { return this.m_length; }
        }

        public IProducer ArrayProducer
        {
            get { return this.m_arrProducer; }
        }

        public IDataFieldAccessor[] ArrayAccessors
        {
            get { return this.m_ArrayAccessors; }
        }

        public Array GetArray(object arrayContainer)
        {
            Array array = (Array) this.m_accessor.Get(arrayContainer);
            if (array == null)
            {
                array = (Array) this.m_arrProducer.Produce();
                this.m_accessor.Set(arrayContainer, (object) array);
            }

            return array;
        }

        /// <summary>
        /// Returns the object at the given index (might be null).
        /// </summary>
        /// <param name="arrayContainer"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public object Get(object arrayContainer, int index)
        {
            return this.GetArray(arrayContainer).GetValue(index);
        }

        public void Set(object arrayContainer, int index, object value)
        {
            try
            {
                ArrayUtil.SetValue(this.GetArray(arrayContainer), index, value);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to set Array-element: " + (object) this, ex);
            }
        }
    }
}