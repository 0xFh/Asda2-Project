using System;
using System.Threading;

namespace Cell.Core
{
    /// <summary>
    /// Efficient method for performing thread safety while staying in user-mode.
    /// </summary>
    /// <remarks>
    /// <para>This is a value type so it works very efficiently when used as a field in a class.</para>
    /// <para>Avoid boxing or you will lose thread safety.</para>
    /// <para>This structure is based on Jeffrey Richter's article "Concurrent Affairs" in the October 2005 issue of MSDN Magazine.</para>
    /// </remarks>
    public struct SpinWaitLock
    {
        private static readonly bool IsSingleCpuMachine = Environment.ProcessorCount == 1;
        private const int NoOwner = 0;
        private int _owner;
        private int _recursionCount;

        /// <summary>Attempts to lock a resource.</summary>
        public void Enter()
        {
            Thread.BeginCriticalRegion();
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (this._owner == managedThreadId)
            {
                ++this._recursionCount;
            }
            else
            {
                label_5:
                if (Interlocked.CompareExchange(ref this._owner, managedThreadId, 0) == 0)
                    return;
                while (Thread.VolatileRead(ref this._owner) != 0)
                    SpinWaitLock.StallThread();
                goto label_5;
            }
        }

        /// <summary>Releases a resource.</summary>
        public void Exit()
        {
            if (this._recursionCount > 0)
            {
                --this._recursionCount;
            }
            else
            {
                Interlocked.Exchange(ref this._owner, 0);
                Thread.EndCriticalRegion();
            }
        }

        private static void StallThread()
        {
            if (SpinWaitLock.IsSingleCpuMachine)
                NativeMethods.OsSwitchToThread();
            else
                Thread.SpinWait(1);
        }
    }
}