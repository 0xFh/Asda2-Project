using System;
using System.Threading;

namespace WCell.Util.Synchronization
{
    /// <summary>
    /// When used with the "using" statement, does pretty much the same as the lock statement.
    /// But we use this class so we can easily change the implementation, if required.
    /// </summary>
    public class SimpleLockWrapper
    {
        private readonly SimpleLockWrapper.LockReleaser releaser = new SimpleLockWrapper.LockReleaser();

        public IDisposable Enter()
        {
            return (IDisposable) this.releaser;
        }

        private class LockReleaser : IDisposable
        {
            public void Dispose()
            {
                Monitor.Exit((object) this);
            }
        }
    }
}