using System;

namespace WCell.Util.Data
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DependingProducer : Attribute
    {
        public object Key;
        public IProducer Producer;

        public DependingProducer(object id, Type type)
        {
            this.Key = id;
            this.Producer = (IProducer) new DefaultProducer(type);
        }

        public DependingProducer(object id, IProducer producer)
        {
            this.Key = id;
            this.Producer = producer;
        }

        public DependingProducer(object id, Func<object> creator)
        {
            this.Key = id;
            this.Producer = (IProducer) new CustomProducer(creator);
        }

        public DependingProducer(object id, CustomProducer producer)
        {
            this.Key = id;
            this.Producer = (IProducer) producer;
        }
    }
}