using System;
using System.Collections.Generic;

namespace WCell.Util.Data
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataHolderAttribute : DBAttribute
    {
        public readonly IDictionary<object, IProducer> DependingProducers =
            (IDictionary<object, IProducer>) new Dictionary<object, IProducer>();

        public bool Inherit;
        public bool RequirePersistantAttr;

        public DataHolderAttribute()
        {
        }

        public DataHolderAttribute(string dependsOnField)
        {
            this.DependsOnField = dependsOnField;
        }

        public DataHolderAttribute(IEnumerable<KeyValuePair<object, IProducer>> dependingPoducers)
        {
            foreach (KeyValuePair<object, IProducer> dependingPoducer in dependingPoducers)
                this.DependingProducers.Add(dependingPoducer);
        }

        /// <summary>
        /// The field that delivers the values to decide the depending Producer
        /// </summary>
        public string DependsOnField { get; set; }
    }
}