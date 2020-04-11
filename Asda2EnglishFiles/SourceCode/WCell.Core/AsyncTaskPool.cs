using NLog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WCell.Util.Collections;
using WCell.Util.Threading;
using WCell.Util.Threading.TaskParallel;

namespace WCell.Core
{
    /// <summary>
    /// A task pool that processes messages asynchronously on the application thread pool.
    /// </summary>
    public class AsyncTaskPool
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly object obj = (object) "";
        protected LockfreeQueue<IMessage> TaskQueue;
        protected Stopwatch TaskTimer;
        protected long UpdateFrequency;

        /// <summary>
        /// Creates a new task pool with an update frequency of 100ms
        /// </summary>
        public AsyncTaskPool()
            : this(100L)
        {
        }

        /// <summary>
        /// Creates a new task pool with the specified update frequency.
        /// </summary>
        /// <param name="updateFrequency">the update frequency of the task pool</param>
        public AsyncTaskPool(long updateFrequency)
        {
            this.TaskQueue = new LockfreeQueue<IMessage>();
            this.TaskTimer = Stopwatch.StartNew();
            this.UpdateFrequency = updateFrequency;
            Task.Factory.StartNewDelayed((int) this.UpdateFrequency, new Action<object>(this.TaskUpdateCallback),
                (object) this);
        }

        /// <summary>
        /// Enqueues a new task in the queue that will be executed during the next
        /// tick.
        /// </summary>
        public void EnqueueTask(IMessage task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task), "task cannot be null");
            this.TaskQueue.Enqueue(task);
        }

        /// <summary>
        /// Waits until all currently enqueued messages have been processed.
        /// </summary>
        public void WaitOneTick()
        {
            Message message = new Message((Action) (() =>
            {
                lock (AsyncTaskPool.obj)
                    Monitor.PulseAll(AsyncTaskPool.obj);
            }));
            lock (AsyncTaskPool.obj)
            {
                this.TaskQueue.Enqueue((IMessage) message);
                Monitor.Wait(AsyncTaskPool.obj);
            }
        }

        public void ChangeUpdateFrequency(long frequency)
        {
            if (frequency < 0L)
                throw new ArgumentException("frequency cannot be less than 0", nameof(frequency));
            this.UpdateFrequency = frequency;
        }

        protected void TaskUpdateCallback(object state)
        {
            long elapsedMilliseconds1 = this.TaskTimer.ElapsedMilliseconds;
            this.ProcessTasks(elapsedMilliseconds1);
            long elapsedMilliseconds2 = this.TaskTimer.ElapsedMilliseconds;
            Task.Factory.StartNewDelayed(
                elapsedMilliseconds2 - elapsedMilliseconds1 > this.UpdateFrequency
                    ? 0
                    : (int) (elapsedMilliseconds1 + this.UpdateFrequency - elapsedMilliseconds2),
                new Action<object>(this.TaskUpdateCallback), (object) this);
        }

        protected virtual void ProcessTasks(long startTime)
        {
            IMessage message;
            while (this.TaskQueue.TryDequeue(out message))
                message.Execute();
        }
    }
}