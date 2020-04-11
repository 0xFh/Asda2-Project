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
      Key = id;
      Producer = new DefaultProducer(type);
    }

    public DependingProducer(object id, IProducer producer)
    {
      Key = id;
      Producer = producer;
    }

    public DependingProducer(object id, Func<object> creator)
    {
      Key = id;
      Producer = new CustomProducer(creator);
    }

    public DependingProducer(object id, CustomProducer producer)
    {
      Key = id;
      Producer = producer;
    }
  }
}