using System.Reflection;
using WCell.Util.DB;

namespace WCell.Util.Data
{
    public class FlatSimpleDataField : DataField, IFlatDataFieldAccessor, IDataFieldAccessor
    {
        public FlatSimpleDataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor,
            MemberInfo mappedMember, INestedDataField parent)
            : base(dataHolder, name, accessor, mappedMember, parent)
        {
        }

        public object Get(IDataHolder obj)
        {
            return this.m_accessor.Get(this.GetTargetObject(obj));
        }

        public void Set(IDataHolder obj, object value)
        {
            object key = this.GetTargetObject(obj);
            this.m_accessor.Set(key, value);
            if (!key.GetType().IsValueType)
                return;
            INestedDataField parent = this.m_parent;
            while (parent != null)
            {
                object targetObject = parent.GetTargetObject(obj);
                parent.Accessor.Set(targetObject, key);
                key = targetObject;
                parent = parent.Parent;
                if (!targetObject.GetType().IsValueType)
                    break;
            }
        }

        public override DataFieldType DataFieldType
        {
            get { return DataFieldType.FlatSimple; }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override IDataField Copy(INestedDataField parent)
        {
            return (IDataField) new FlatSimpleDataField(this.m_DataHolderDefinition, this.m_name, this.m_accessor,
                this.m_mappedMember, parent);
        }
    }
}