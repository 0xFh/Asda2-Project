using System;

namespace Cell.Core.Exceptions
{
    [Serializable]
    public class NoAvailableAdaptersException : Exception
    {
        public NoAvailableAdaptersException()
        {
        }

        public NoAvailableAdaptersException(string message)
            : base(message)
        {
        }
    }
}