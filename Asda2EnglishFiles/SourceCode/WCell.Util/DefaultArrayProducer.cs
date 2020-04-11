using System;

namespace WCell.Util
{
    public class DefaultArrayProducer : IProducer
    {
        private readonly Type m_Type;
        private readonly int m_Length;

        public DefaultArrayProducer(Type type, int length)
        {
            this.m_Type = type;
            this.m_Length = length;
        }

        public int Length
        {
            get { return this.m_Length; }
        }

        public Type Type
        {
            get { return this.m_Type; }
        }

        object IProducer.Produce()
        {
            return (object) Array.CreateInstance(this.m_Type, this.m_Length);
        }
    }
}