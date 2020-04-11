using System;

namespace WCell.Util
{
    public class DefaultProducer : IProducer
    {
        private readonly Type m_Type;

        public DefaultProducer(Type type)
        {
            this.m_Type = type;
        }

        public Type Type
        {
            get { return this.m_Type; }
        }

        object IProducer.Produce()
        {
            try
            {
                return Activator.CreateInstance(this.m_Type);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot create Object of Type: " + this.m_Type.FullName, ex);
            }
        }

        public static implicit operator DefaultProducer(Type type)
        {
            return new DefaultProducer(type);
        }
    }
}