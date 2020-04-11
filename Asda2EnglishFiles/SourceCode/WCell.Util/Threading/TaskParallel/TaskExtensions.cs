using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WCell.Util.Threading.TaskParallel
{
    /// <summary>Extensions methods for Task.</summary>
    public static class TaskExtensions
    {
        /// <summary>Creates a continuation task using the specified TaskFactory.</summary>
        /// <param name="task">The antecedent Task.</param>
        /// <param name="continuationAction">The continuation action.</param>
        /// <param name="factory">The TaskFactory.</param>
        /// <returns>A continuation task.</returns>
        public static Task ContinueWith(this Task task, Action<Task> continuationAction, TaskFactory factory)
        {
            return task.ContinueWith(continuationAction, factory.CancellationToken, factory.ContinuationOptions,
                factory.Scheduler);
        }

        /// <summary>Creates a continuation task using the specified TaskFactory.</summary>
        /// <param name="task">The antecedent Task.</param>
        /// <param name="continuationFunction">The continuation function.</param>
        /// <param name="factory">The TaskFactory.</param>
        /// <returns>A continuation task.</returns>
        public static Task<TResult> ContinueWith<TResult>(this Task task, Func<Task, TResult> continuationFunction,
            TaskFactory factory)
        {
            return task.ContinueWith<TResult>(continuationFunction, factory.CancellationToken,
                factory.ContinuationOptions, factory.Scheduler);
        }

        /// <summary>Creates a continuation task using the specified TaskFactory.</summary>
        /// <param name="task">The antecedent Task.</param>
        /// <param name="continuationAction">The continuation action.</param>
        /// <param name="factory">The TaskFactory.</param>
        /// <returns>A continuation task.</returns>
        public static Task ContinueWith<TResult>(this Task<TResult> task, Action<Task<TResult>> continuationAction,
            TaskFactory<TResult> factory)
        {
            return task.ContinueWith(continuationAction, factory.CancellationToken, factory.ContinuationOptions,
                factory.Scheduler);
        }

        /// <summary>Creates a continuation task using the specified TaskFactory.</summary>
        /// <param name="task">The antecedent Task.</param>
        /// <param name="continuationFunction">The continuation function.</param>
        /// <param name="factory">The TaskFactory.</param>
        /// <returns>A continuation task.</returns>
        public static Task<TNewResult> ContinueWith<TResult, TNewResult>(this Task<TResult> task,
            Func<Task<TResult>, TNewResult> continuationFunction, TaskFactory<TResult> factory)
        {
            return task.ContinueWith<TNewResult>(continuationFunction, factory.CancellationToken,
                factory.ContinuationOptions, factory.Scheduler);
        }

        /// <summary>
        /// Creates a Task that represents the completion of another Task, and
        /// that schedules an AsyncCallback to run upon completion.
        /// </summary>
        /// <param name="task">The antecedent Task.</param>
        /// <param name="callback">The AsyncCallback to run.</param>
        /// <param name="state">The object state to use with the AsyncCallback.</param>
        /// <returns>The new task.</returns>
        public static Task ToAsync(this Task task, AsyncCallback callback, object state)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(state);
            task.ContinueWith((Action<Task>) (_ =>
            {
                tcs.SetFromTask<object>(task);
                if (callback == null)
                    return;
                callback((IAsyncResult) tcs.Task);
            }));
            return (Task) tcs.Task;
        }

        /// <summary>
        /// Creates a Task that represents the completion of another Task, and
        /// that schedules an AsyncCallback to run upon completion.
        /// </summary>
        /// <param name="task">The antecedent Task.</param>
        /// <param name="callback">The AsyncCallback to run.</param>
        /// <param name="state">The object state to use with the AsyncCallback.</param>
        /// <returns>The new task.</returns>
        public static Task<TResult> ToAsync<TResult>(this Task<TResult> task, AsyncCallback callback, object state)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>(state);
            task.ContinueWith((Action<Task<TResult>>) (_ =>
            {
                tcs.SetFromTask<TResult>(task);
                if (callback == null)
                    return;
                callback((IAsyncResult) tcs.Task);
            }));
            return tcs.Task;
        }

        /// <summary>Suppresses default exception handling of a Task that would otherwise reraise the exception on the finalizer thread.</summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task IgnoreExceptions(this Task task)
        {
            AggregateException exception;
            task.ContinueWith((Action<Task>) (t => exception = t.Exception), CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
            return task;
        }

        /// <summary>Suppresses default exception handling of a Task that would otherwise reraise the exception on the finalizer thread.</summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task<T> IgnoreExceptions<T>(this Task<T> task)
        {
            return (Task<T>) task.IgnoreExceptions();
        }

        /// <summary>Fails immediately when an exception is encountered.</summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task FailFastOnException(this Task task)
        {
            task.ContinueWith((Action<Task>) (t => Environment.FailFast("A task faulted.", (Exception) t.Exception)),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
            return task;
        }

        /// <summary>Fails immediately when an exception is encountered.</summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task<T> FailFastOnException<T>(this Task<T> task)
        {
            return (Task<T>) task.FailFastOnException();
        }

        /// <summary>Propagates any exceptions that occurred on the specified task.</summary>
        /// <param name="task">The Task whose exceptions are to be propagated.</param>
        public static void PropagateExceptions(this Task task)
        {
            if (!task.IsCompleted)
                throw new InvalidOperationException("The task has not completed.");
            if (!task.IsFaulted)
                return;
            task.Wait();
        }

        /// <summary>Propagates any exceptions that occurred on the specified tasks.</summary>
        /// <param name="task">The Tassk whose exceptions are to be propagated.</param>
        public static void PropagateExceptions(this Task[] tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));
            if (((IEnumerable<Task>) tasks).Any<Task>((Func<Task, bool>) (t => t == null)))
                throw new ArgumentException(nameof(tasks));
            if (((IEnumerable<Task>) tasks).Any<Task>((Func<Task, bool>) (t => !t.IsCompleted)))
                throw new InvalidOperationException("A task has not completed.");
            Task.WaitAll(tasks);
        }

        /// <summary>
        /// Ensures that a parent task can't transition into a completed state
        /// until the specified task has also completed, even if it's not
        /// already a child task.
        /// </summary>
        /// <param name="task">The task to attach to the current task as a child.</param>
        public static void AttachToParent(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            task.ContinueWith((Action<Task>) (t => t.Wait()), CancellationToken.None,
                TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        /// <summary>Creates a new Task that mirrors the supplied task but that will be canceled after the specified timeout.</summary>
        /// <param name="task">The task.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The new Task that may time out.</returns>
        public static Task WithTimeout(this Task task, TimeSpan timeout)
        {
            TaskCompletionSource<object> result = new TaskCompletionSource<object>(task.AsyncState);
            Timer timer = new Timer((TimerCallback) (state => ((TaskCompletionSource<object>) state).TrySetCanceled()),
                (object) result, timeout, TimeSpan.FromMilliseconds(-1.0));
            task.ContinueWith((Action<Task>) (t =>
            {
                timer.Dispose();
                result.TrySetFromTask<object>(t);
            }), TaskContinuationOptions.ExecuteSynchronously);
            return (Task) result.Task;
        }

        /// <summary>Creates a new Task that mirrors the supplied task but that will be canceled after the specified timeout.</summary>
        /// <typeparam name="TResult">Specifies the type of data contained in the task.</typeparam>
        /// <param name="task">The task.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The new Task that may time out.</returns>
        public static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            TaskCompletionSource<TResult> result = new TaskCompletionSource<TResult>(task.AsyncState);
            Timer timer = new Timer((TimerCallback) (state => ((TaskCompletionSource<TResult>) state).TrySetCanceled()),
                (object) result, timeout, TimeSpan.FromMilliseconds(-1.0));
            task.ContinueWith((Action<Task<TResult>>) (t =>
            {
                timer.Dispose();
                result.TrySetFromTask<TResult>(t);
            }), TaskContinuationOptions.ExecuteSynchronously);
            return result.Task;
        }

        /// <summary>Creates an IObservable that represents the completion of a Task.</summary>
        /// <typeparam name="TResult">Specifies the type of data returned by the Task.</typeparam>
        /// <param name="task">The Task to be represented as an IObservable.</param>
        /// <returns>An IObservable that represents the completion of the Task.</returns>
        public static IObservable<TResult> ToObservable<TResult>(this Task<TResult> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            return (IObservable<TResult>) new TaskExtensions.TaskObservable<TResult>()
            {
                _task = task
            };
        }

        /// <summary>Waits for the task to complete execution, returning the task's final status.</summary>
        /// <param name="task">The task for which to wait.</param>
        /// <returns>The completion status of the task.</returns>
        /// <remarks>Unlike Wait, this method will not throw an exception if the task ends in the Faulted or Canceled state.</remarks>
        public static TaskStatus WaitForCompletionStatus(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            ((IAsyncResult) task).AsyncWaitHandle.WaitOne();
            return task.Status;
        }

        /// <summary>An implementation of IObservable that wraps a Task.</summary>
        /// <typeparam name="TResult">The type of data returned by the task.</typeparam>
        private class TaskObservable<TResult> : IObservable<TResult>
        {
            internal Task<TResult> _task;

            public IDisposable Subscribe(IObserver<TResult> observer)
            {
                if (observer == null)
                    throw new ArgumentNullException(nameof(observer));
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                this._task.ContinueWith((Action<Task<TResult>>) (t =>
                {
                    switch (t.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            observer.OnNext(this._task.Result);
                            observer.OnCompleted();
                            break;
                        case TaskStatus.Canceled:
                            observer.OnError((Exception) new TaskCanceledException((Task) t));
                            break;
                        case TaskStatus.Faulted:
                            observer.OnError((Exception) this._task.Exception);
                            break;
                    }
                }), cancellationTokenSource.Token);
                return (IDisposable) new TaskExtensions.CancelOnDispose()
                {
                    Source = cancellationTokenSource
                };
            }
        }

        /// <summary>Translate a call to IDisposable.Dispose to a CancellationTokenSource.Cancel.</summary>
        private class CancelOnDispose : IDisposable
        {
            internal CancellationTokenSource Source;

            void IDisposable.Dispose()
            {
                this.Source.Cancel();
            }
        }
    }
}