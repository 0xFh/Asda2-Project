using System;
using System.Runtime.InteropServices;
using WCell.Util.Threading;

namespace WCell.Core
{
    /// <summary>Base class used for all Manager classes</summary>
    public abstract class Manager<T> : Singleton<T>, IContextHandler where T : class
    {
        public bool IsInContext
        {
            get { throw new NotImplementedException(); }
        }

        public void AddMessage(IMessage message)
        {
            throw new NotImplementedException();
        }

        public void AddMessage(Action action)
        {
            throw new NotImplementedException();
        }

        public bool ExecuteInContext(Action action)
        {
            throw new NotImplementedException();
        }

        public void EnsureContext()
        {
            throw new NotImplementedException();
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct ManagerOperation
        {
        }
    }
}