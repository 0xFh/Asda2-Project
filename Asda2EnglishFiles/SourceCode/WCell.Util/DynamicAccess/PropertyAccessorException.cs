using System;

namespace WCell.Util.DynamicAccess
{
    /// <summary>PropertyAccessorException class.</summary>
    public class PropertyAccessorException : Exception
    {
        public PropertyAccessorException(string message)
            : base(message)
        {
        }

        public PropertyAccessorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}