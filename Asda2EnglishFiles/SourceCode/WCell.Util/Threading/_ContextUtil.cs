using System;
using System.Threading;

namespace WCell.Util.Threading
{
    public static class _ContextUtil
    {
        /// <summary>
        /// Lets the given ContextHandler wait one Tick. Does nothing if within the given Handler's Context.
        /// </summary>
        /// <param name="contextHandler"></param>
        public static void WaitOne(this IContextHandler contextHandler)
        {
            object obj = new object();
            if (contextHandler.IsInContext)
                return;
            lock (obj)
            {
                contextHandler.AddMessage((Action) (() =>
                {
                    lock (obj)
                        Monitor.PulseAll(obj);
                }));
                Monitor.Wait(obj);
            }
        }
    }
}