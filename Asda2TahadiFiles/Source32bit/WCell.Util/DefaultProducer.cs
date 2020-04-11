using System;

namespace WCell.Util
{
  public class DefaultProducer : IProducer
  {
    private readonly Type m_Type;

    public DefaultProducer(Type type)
    {
      m_Type = type;
    }

    public Type Type
    {
      get { return m_Type; }
    }

    object IProducer.Produce()
    {
      try
      {
        return Activator.CreateInstance(m_Type);
      }
      catch(Exception ex)
      {
        throw new InvalidOperationException("Cannot create Object of Type: " + m_Type.FullName, ex);
      }
    }

    public static implicit operator DefaultProducer(Type type)
    {
      return new DefaultProducer(type);
    }
  }
}