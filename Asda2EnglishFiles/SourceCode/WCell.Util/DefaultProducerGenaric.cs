using System;

namespace WCell.Util
{
    public class DefaultProducer<T> : IProducer<T>, IProducer
    {
        /// <summary>Creates a new object of Type T</summary>
        public T Produce()
        {
            return Activator.CreateInstance<T>();
        }

        object IProducer.Produce()
        {
            return (object) Activator.CreateInstance<T>();
        }
    }
}