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
            this.m_Type = type;
            foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.Public))
            {
                if (constructor.GetParameters().Length == 1 && !constructor.ContainsGenericParameters)
                {
                    this.m_ctor = constructor;
                    break;
                }
            }
        }

        public DefaultOneArgumenteProducer(Type type, ConstructorInfo ctor)
        {
            this.m_Type = type;
            this.m_ctor = ctor;
        }

        public ConstructorInfo Ctor
        {
            get { return this.m_ctor; }
        }

        public Type Type
        {
            get { return this.m_Type; }
        }

        object IProducer.Produce()
        {
            throw new DataHolderException(
                "Cannot call default ctor on dependent producer for Type: " + (object) this.m_Type, new object[0]);
        }

        public object Produce(object arg1)
        {
            return this.m_ctor.Invoke(new object[1] {arg1});
        }
    }
}