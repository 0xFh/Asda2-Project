using System;

namespace WCell.Util
{
  public class DefaultArrayProducer : IProducer
  {
    private readonly Type m_Type;
    private readonly int m_Length;

    public DefaultArrayProducer(Type type, int length)
    {
      m_Type = type;
      m_Length = length;
    }

    public int Length
    {
      get { return m_Length; }
    }

    public Type Type
    {
      get { return m_Type; }
    }

    object IProducer.Produce()
    {
      return Array.CreateInstance(m_Type, m_Length);
    }
  }
}