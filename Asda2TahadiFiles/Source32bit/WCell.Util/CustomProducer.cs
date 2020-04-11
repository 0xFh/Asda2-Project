using System;

namespace WCell.Util
{
  public class CustomProducer : IProducer
  {
    private readonly Func<object> m_Creator;

    public CustomProducer(Func<object> creator)
    {
      m_Creator = creator;
    }

    public Func<object> Creator
    {
      get { return m_Creator; }
    }

    object IProducer.Produce()
    {
      return m_Creator();
    }

    public static implicit operator CustomProducer(Func<object> creator)
    {
      return new CustomProducer(creator);
    }
  }
}