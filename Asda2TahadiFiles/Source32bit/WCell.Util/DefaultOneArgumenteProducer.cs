using System;
using System.Reflection;
using WCell.Util.Data;

namespace WCell.Util
{
  public class DefaultOneArgumenteProducer : IProducer, IOneArgumentProducer
  {
    private readonly Type m_Type;
    private readonly ConstructorInfo m_ctor;

    public DefaultOneArgumenteProducer(Type type)
    {
      m_Type = type;
      foreach(ConstructorInfo constructor in type.GetConstructors(BindingFlags.Public))
      {
        if(constructor.GetParameters().Length == 1 && !constructor.ContainsGenericParameters)
        {
          m_ctor = constructor;
          break;
        }
      }
    }

    public DefaultOneArgumenteProducer(Type type, ConstructorInfo ctor)
    {
      m_Type = type;
      m_ctor = ctor;
    }

    public ConstructorInfo Ctor
    {
      get { return m_ctor; }
    }

    public Type Type
    {
      get { return m_Type; }
    }

    object IProducer.Produce()
    {
      throw new DataHolderException(
        "Cannot call default ctor on dependent producer for Type: " + m_Type);
    }

    public object Produce(object arg1)
    {
      return m_ctor.Invoke(new object[1] { arg1 });
    }
  }
}