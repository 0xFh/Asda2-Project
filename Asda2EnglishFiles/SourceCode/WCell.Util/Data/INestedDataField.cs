using System.Collections.Generic;
using System.Reflection;

namespace WCell.Util.Data
{
    public interface INestedDataField : IDataFieldBase
    {
        IGetterSetter Accessor { get; }

        IProducer Producer { get; }

        Dictionary<string, IDataField> InnerFields { get; }

        MemberInfo BelongsTo { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootObject"></param>
        /// <returns></returns>
        object GetTargetObject(IDataHolder rootObject);
    }
}