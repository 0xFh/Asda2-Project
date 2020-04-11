using System;
using System.Threading;
using System.Threading.Tasks;

namespace WCell.Util.Threading.TaskParallel
{
    public static class TaskSchedulerExtensions
    {
        /// <summary>Gets a SynchronizationContext that targets this TaskScheduler.</summary>
        /// <param name="scheduler">The target scheduler.</param>
        /// <returns>A SynchronizationContext that targets this scheduler.</returns>
        public static SynchronizationContext ToSynchronizationContext(this TaskScheduler scheduler)
        {
            return (SynchronizationContext) new TaskSchedulerExtensions.TaskSchedulerSynchronizationContext(scheduler);
        }

        /// <summary>Provides a SynchronizationContext wrapper for a TaskScheduler.</summary>
        private sealed class TaskSchedulerSynchronizationContext : SynchronizationContext
        {
            /// <summary>The scheduler.</summary>
            private TaskScheduler _scheduler;

            /// <summary>Initializes the context with the specified scheduler.</summary>
            /// <param name="scheduler">The scheduler to target.</param>
            internal TaskSchedulerSynchronizationContext(TaskScheduler scheduler)
            {
                if (scheduler == null)
                    throw new ArgumentNullException(nameof(scheduler));
                this._scheduler = scheduler;
            }

            /// <summary>Dispatches an asynchronous message to the synchronization context.</summary>
            /// <param name="d">The System.Threading.SendOrPostCallback delegate to call.</param>
            /// <param name="state">The object passed to the delegate.</param>
            public override void Post(SendOrPostCallback d, object state)
            {
                Task.Factory.StartNew((Action) (() => d(state)), CancellationToken.None, TaskCreationOptions.None,
                    this._scheduler);
            }

            /// <summary>Dispatches a synchronous message to the synchronization context.</summary>
            /// <param name="d">The System.Threading.SendOrPostCallback delegate to call.</param>
            /// <param name="state">The object passed to the delegate.</param>
            public override void Send(SendOrPostCallback d, object state)
            {
                Task task = new Task((Action) (() => d(state)));
                task.RunSynchronously(this._scheduler);
                task.Wait();
            }
        }
    }
}