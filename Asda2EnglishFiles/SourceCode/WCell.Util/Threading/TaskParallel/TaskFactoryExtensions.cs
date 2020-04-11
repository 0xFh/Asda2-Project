using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WCell.Util.Threading.TaskParallel
{
    /// <summary>Extensions for TaskFactory.</summary>
    public static class TaskFactoryExtensions
    {
        /// <summary>Creates a generic TaskFactory from a non-generic one.</summary>
        /// <typeparam name="TResult">Specifies the type of Task results for the Tasks created by the new TaskFactory.</typeparam>
        /// <param name="factory">The TaskFactory to serve as a template.</param>
        /// <returns>The created TaskFactory.</returns>
        public static TaskFactory<TResult> ToGeneric<TResult>(this TaskFactory factory)
        {
            return new TaskFactory<TResult>(factory.CancellationToken, factory.CreationOptions,
                factory.ContinuationOptions, factory.Scheduler);
        }

        /// <summary>Creates a generic TaskFactory from a non-generic one.</summary>
        /// <typeparam name="TResult">Specifies the type of Task results for the Tasks created by the new TaskFactory.</typeparam>
        /// <param name="factory">The TaskFactory to serve as a template.</param>
        /// <returns>The created TaskFactory.</returns>
        public static TaskFactory ToNonGeneric<TResult>(this TaskFactory<TResult> factory)
        {
            return new TaskFactory(factory.CancellationToken, factory.CreationOptions, factory.ContinuationOptions,
                factory.Scheduler);
        }

        /// <summary>Gets the TaskScheduler instance that should be used to schedule tasks.</summary>
        public static TaskScheduler GetTargetScheduler(this TaskFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.Scheduler ?? TaskScheduler.Current;
        }

        /// <summary>Gets the TaskScheduler instance that should be used to schedule tasks.</summary>
        public static TaskScheduler GetTargetScheduler<TResult>(this TaskFactory<TResult> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.Scheduler != null ? factory.Scheduler : TaskScheduler.Current;
        }

        /// <summary>Converts TaskCreationOptions into TaskContinuationOptions.</summary>
        /// <param name="creationOptions"></param>
        /// <returns></returns>
        private static TaskContinuationOptions ContinuationOptionsFromCreationOptions(
            TaskCreationOptions creationOptions)
        {
            return (TaskContinuationOptions) (creationOptions & TaskCreationOptions.AttachedToParent |
                                              creationOptions & TaskCreationOptions.PreferFairness |
                                              creationOptions & TaskCreationOptions.LongRunning);
        }

        /// <summary>Asynchronously executes a sequence of tasks, maintaining a list of all tasks processed.</summary>
        /// <param name="factory">The TaskFactory to use to create the task.</param>
        /// <param name="functions">
        /// The functions that generate the tasks through which to iterate sequentially.
        /// Iteration will cease if a task faults.
        /// </param>
        /// <returns>A Task that will return the list of tracked tasks iterated.</returns>
        public static Task<IList<Task>> TrackedSequence(this TaskFactory factory, params Func<Task>[] functions)
        {
            TaskCompletionSource<IList<Task>> tcs = new TaskCompletionSource<IList<Task>>();
            factory.Iterate(
                (IEnumerable<object>) TaskFactoryExtensions.TrackedSequenceInternal((IEnumerable<Func<Task>>) functions,
                    tcs));
            return tcs.Task;
        }

        /// <summary>Creates the enumerable to iterate through with Iterate.</summary>
        /// <param name="functions">
        /// The functions that generate the tasks through which to iterate sequentially.
        /// Iteration will cease if a task faults.
        /// </param>
        /// <param name="tcs">The TaskCompletionSource to resolve with the asynchronous results.</param>
        /// <returns>The enumerable through which to iterate.</returns>
        private static IEnumerable<Task> TrackedSequenceInternal(IEnumerable<Func<Task>> functions,
            TaskCompletionSource<IList<Task>> tcs)
        {
            List<Task> tasks = new List<Task>();
            foreach (Func<Task> function in functions)
            {
                Task nextTask = (Task) null;
                try
                {
                    nextTask = function();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }

                if (nextTask == null)
                {
                    yield break;
                }
                else
                {
                    tasks.Add(nextTask);
                    yield return nextTask;
                    if (nextTask.IsFaulted)
                        break;
                }
            }

            tcs.TrySetResult((IList<Task>) tasks);
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, object state)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.Iterate(source, state, factory.CancellationToken, factory.CreationOptions,
                factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the iteration.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, object state,
            CancellationToken cancellationToken)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.Iterate(source, state, cancellationToken, factory.CreationOptions,
                factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, object state,
            TaskCreationOptions creationOptions)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.Iterate(source, state, factory.CancellationToken, creationOptions,
                factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, object state,
            TaskScheduler scheduler)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.Iterate(source, state, factory.CancellationToken, factory.CreationOptions, scheduler);
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="state">The asynchronous state for the returned Task.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the iteration.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, object state,
            CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (source == null)
                throw new ArgumentNullException("asyncIterator");
            if (scheduler == null)
                throw new ArgumentNullException(nameof(scheduler));
            IEnumerator<object> enumerator = source.GetEnumerator();
            if (enumerator == null)
                throw new InvalidOperationException("Invalid enumerable - GetEnumerator returned null");
            TaskCompletionSource<object> trs = new TaskCompletionSource<object>(state, creationOptions);
            trs.Task.ContinueWith((Action<Task<object>>) (_ => enumerator.Dispose()), CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            Action<Task> recursiveBody = (Action<Task>) null;
            recursiveBody = (Action<Task>) (antecedent =>
            {
                try
                {
                    if (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        if (current is Task)
                        {
                            Task task = (Task) current;
                            task.IgnoreExceptions();
                            task.ContinueWith(recursiveBody).IgnoreExceptions();
                        }
                        else if (current is TaskScheduler)
                            Task.Factory.StartNew((Action) (() => recursiveBody((Task) null)), CancellationToken.None,
                                TaskCreationOptions.None, (TaskScheduler) current).IgnoreExceptions();
                        else
                            trs.TrySetException(
                                (Exception) new InvalidOperationException(
                                    "Task or TaskScheduler object expected in Iterate"));
                    }
                    else
                        trs.TrySetResult((object) null);
                }
                catch (Exception ex)
                {
                    OperationCanceledException canceledException = ex as OperationCanceledException;
                    if (canceledException != null && canceledException.CancellationToken == cancellationToken)
                        trs.TrySetCanceled();
                    else
                        trs.TrySetException(ex);
                }
            });
            factory.StartNew((Action) (() => recursiveBody((Task) null)), CancellationToken.None,
                TaskCreationOptions.None, scheduler).IgnoreExceptions();
            return (Task) trs.Task;
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.Iterate(source, (object) null, factory.CancellationToken, factory.CreationOptions,
                factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the iteration.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source,
            CancellationToken cancellationToken)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.Iterate(source, (object) null, cancellationToken, factory.CreationOptions,
                factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source,
            TaskCreationOptions creationOptions)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.Iterate(source, (object) null, factory.CancellationToken, creationOptions,
                factory.GetTargetScheduler());
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source, TaskScheduler scheduler)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.Iterate(source, (object) null, factory.CancellationToken, factory.CreationOptions,
                scheduler);
        }

        /// <summary>Asynchronously iterates through an enumerable of tasks.</summary>
        /// <param name="factory">The target factory.</param>
        /// <param name="source">The enumerable containing the tasks to be iterated through.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the iteration.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which tasks will be scheduled.</param>
        /// <returns>A Task that represents the complete asynchronous operation.</returns>
        public static Task Iterate(this TaskFactory factory, IEnumerable<object> source,
            CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            return factory.Iterate(source, (object) null, cancellationToken, creationOptions, scheduler);
        }

        /// <summary>Creates a Task that will be completed when the specified WaitHandle is signaled.</summary>
        /// <param name="waitHandle">The WaitHandle.</param>
        /// <returns>The created Task.</returns>
        public static Task FromAsync(WaitHandle waitHandle)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            RegisteredWaitHandle rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle,
                (WaitOrTimerCallback) ((param0, param1) => tcs.TrySetResult((object) null)), (object) null, -1, true);
            Task<object> task = tcs.Task;
            task.ContinueWith<bool>((Func<Task<object>, bool>) (_ => rwh.Unregister((WaitHandle) null)),
                TaskContinuationOptions.ExecuteSynchronously);
            return (Task) task;
        }

        /// <summary>Creates a Task that has completed in the Faulted state with the specified exception.</summary>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="exception">The exception with which the Task should fault.</param>
        /// <returns>The completed Task.</returns>
        public static Task FromException(this TaskFactory factory, Exception exception)
        {
            TaskCompletionSource<object> completionSource = new TaskCompletionSource<object>(factory.CreationOptions);
            completionSource.SetException(exception);
            return (Task) completionSource.Task;
        }

        /// <summary>Creates a Task that has completed in the Faulted state with the specified exception.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new Task.</typeparam>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="exception">The exception with which the Task should fault.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromException<TResult>(this TaskFactory factory, Exception exception)
        {
            TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>(factory.CreationOptions);
            completionSource.SetException(exception);
            return completionSource.Task;
        }

        /// <summary>Creates a Task that has completed in the RanToCompletion state with the specified result.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new Task.</typeparam>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="result">The result with which the Task should complete.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromResult<TResult>(this TaskFactory factory, TResult result)
        {
            TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>(factory.CreationOptions);
            completionSource.SetResult(result);
            return completionSource.Task;
        }

        /// <summary>Creates a Task that has completed in the Faulted state with the specified exception.</summary>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="exception">The exception with which the Task should fault.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromException<TResult>(this TaskFactory<TResult> factory, Exception exception)
        {
            TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>(factory.CreationOptions);
            completionSource.SetException(exception);
            return completionSource.Task;
        }

        /// <summary>Creates a Task that has completed in the RanToCompletion state with the specified result.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new Task.</typeparam>
        /// <param name="factory">The target TaskFactory.</param>
        /// <param name="result">The result with which the Task should complete.</param>
        /// <returns>The completed Task.</returns>
        public static Task<TResult> FromResult<TResult>(this TaskFactory<TResult> factory, TResult result)
        {
            TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>(factory.CreationOptions);
            completionSource.SetResult(result);
            return completionSource.Task;
        }

        /// <summary>Creates a Task that will complete after the specified delay.</summary>
        /// <param name="factory">The TaskFactory.</param>
        /// <param name="millisecondsDelay">The delay after which the Task should transition to RanToCompletion.</param>
        /// <returns>A Task that will be completed after the specified duration.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay)
        {
            return factory.StartNewDelayed(millisecondsDelay, CancellationToken.None);
        }

        /// <summary>Creates a Task that will complete after the specified delay.</summary>
        /// <param name="factory">The TaskFactory.</param>
        /// <param name="millisecondsDelay">The delay after which the Task should transition to RanToCompletion.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the timed task.</param>
        /// <returns>A Task that will be completed after the specified duration and that's cancelable with the specified token.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay,
            CancellationToken cancellationToken)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (millisecondsDelay < 0)
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(factory.CreationOptions);
            CancellationTokenRegistration ctr = new CancellationTokenRegistration();
            Timer timer = new Timer((TimerCallback) (self =>
            {
                ctr.Dispose();
                ((Timer) self).Dispose();
                tcs.TrySetResult((object) null);
            }));
            if (cancellationToken.CanBeCanceled)
                ctr = cancellationToken.Register((Action) (() =>
                {
                    timer.Dispose();
                    tcs.TrySetCanceled();
                }));
            timer.Change(millisecondsDelay, -1);
            return (Task) tcs.Task;
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action action)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.StartNewDelayed(millisecondsDelay, action, factory.CancellationToken,
                factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action action,
            TaskCreationOptions creationOptions)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.StartNewDelayed(millisecondsDelay, action, factory.CancellationToken, creationOptions,
                factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="cancellationToken">The cancellation token to assign to the created Task.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action action,
            CancellationToken cancellationToken)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.StartNewDelayed(millisecondsDelay, action, cancellationToken, factory.CreationOptions,
                factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="cancellationToken">The cancellation token to assign to the created Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which the Task will be scheduled.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action action,
            CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (millisecondsDelay < 0)
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (scheduler == null)
                throw new ArgumentNullException(nameof(scheduler));
            return factory.StartNewDelayed(millisecondsDelay, cancellationToken).ContinueWith(
                (Action<Task>) (_ => action()), cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion,
                scheduler);
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action<object> action,
            object state)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.StartNewDelayed(millisecondsDelay, action, state, factory.CancellationToken,
                factory.CreationOptions, factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action<object> action,
            object state, TaskCreationOptions creationOptions)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.StartNewDelayed(millisecondsDelay, action, state, factory.CancellationToken, creationOptions,
                factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="cancellationToken">The cancellation token to assign to the created Task.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action<object> action,
            object state, CancellationToken cancellationToken)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.StartNewDelayed(millisecondsDelay, action, state, cancellationToken, factory.CreationOptions,
                factory.GetTargetScheduler());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="action">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="cancellationToken">The cancellation token to assign to the created Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which the Task will be scheduled.</param>
        /// <returns>The created Task.</returns>
        public static Task StartNewDelayed(this TaskFactory factory, int millisecondsDelay, Action<object> action,
            object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions,
            TaskScheduler scheduler)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (millisecondsDelay < 0)
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (scheduler == null)
                throw new ArgumentNullException(nameof(scheduler));
            TaskCompletionSource<object> result = new TaskCompletionSource<object>(state);
            factory.StartNewDelayed(millisecondsDelay, cancellationToken).ContinueWith((Action<Task>) (t =>
            {
                if (t.IsCanceled)
                {
                    result.TrySetCanceled();
                }
                else
                {
                    try
                    {
                        action(state);
                    }
                    catch (Exception ex)
                    {
                        result.TrySetException(ex);
                    }
                    finally
                    {
                        result.TrySetResult((object) null);
                        if (result.Task.Status == TaskStatus.RanToCompletion)
                            result.Task.Dispose();
                    }
                }
            }), scheduler);
            return (Task) result.Task;
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay,
            Func<TResult> function)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.StartNewDelayed<TResult>(millisecondsDelay, function, factory.CancellationToken,
                factory.CreationOptions, factory.GetTargetScheduler<TResult>());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay,
            Func<TResult> function, TaskCreationOptions creationOptions)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.StartNewDelayed<TResult>(millisecondsDelay, function, factory.CancellationToken,
                creationOptions, factory.GetTargetScheduler<TResult>());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="cancellationToken">The CancellationToken to assign to the Task.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay,
            Func<TResult> function, CancellationToken cancellationToken)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.StartNewDelayed<TResult>(millisecondsDelay, function, cancellationToken,
                factory.CreationOptions, factory.GetTargetScheduler<TResult>());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="cancellationToken">The CancellationToken to assign to the Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which the Task will be scheduled.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay,
            Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions,
            TaskScheduler scheduler)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (millisecondsDelay < 0)
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
            if (function == null)
                throw new ArgumentNullException(nameof(function));
            if (scheduler == null)
                throw new ArgumentNullException(nameof(scheduler));
            TaskCompletionSource<object> completionSource = new TaskCompletionSource<object>();
            Timer timer =
                new Timer((TimerCallback) (obj => ((TaskCompletionSource<object>) obj).SetResult((object) null)),
                    (object) completionSource, millisecondsDelay, -1);
            return completionSource.Task.ContinueWith<TResult>((Func<Task<object>, TResult>) (_ =>
                {
                    timer.Dispose();
                    return function();
                }), cancellationToken, TaskFactoryExtensions.ContinuationOptionsFromCreationOptions(creationOptions),
                scheduler);
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay,
            Func<object, TResult> function, object state)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.StartNewDelayed<TResult>(millisecondsDelay, function, state, factory.CancellationToken,
                factory.CreationOptions, factory.GetTargetScheduler<TResult>());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="cancellationToken">The CancellationToken to assign to the Task.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay,
            Func<object, TResult> function, object state, CancellationToken cancellationToken)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.StartNewDelayed<TResult>(millisecondsDelay, function, state, cancellationToken,
                factory.CreationOptions, factory.GetTargetScheduler<TResult>());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay,
            Func<object, TResult> function, object state, TaskCreationOptions creationOptions)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return factory.StartNewDelayed<TResult>(millisecondsDelay, function, state, factory.CancellationToken,
                creationOptions, factory.GetTargetScheduler<TResult>());
        }

        /// <summary>Creates and schedules a task for execution after the specified time delay.</summary>
        /// <param name="factory">The factory to use to create the task.</param>
        /// <param name="millisecondsDelay">The delay after which the task will be scheduled.</param>
        /// <param name="function">The delegate executed by the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="cancellationToken">The CancellationToken to assign to the Task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <param name="scheduler">The scheduler to which the Task will be scheduled.</param>
        /// <returns>The created Task.</returns>
        public static Task<TResult> StartNewDelayed<TResult>(this TaskFactory<TResult> factory, int millisecondsDelay,
            Func<object, TResult> function, object state, CancellationToken cancellationToken,
            TaskCreationOptions creationOptions, TaskScheduler scheduler)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (millisecondsDelay < 0)
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
            if (function == null)
                throw new ArgumentNullException("action");
            if (scheduler == null)
                throw new ArgumentNullException(nameof(scheduler));
            TaskCompletionSource<TResult> result = new TaskCompletionSource<TResult>(state);
            Timer timer = (Timer) null;
            Task<TResult> task = new Task<TResult>(function, state, creationOptions);
            task.ContinueWith((Action<Task<TResult>>) (t =>
                {
                    result.SetFromTask<TResult>(t);
                    timer.Dispose();
                }), cancellationToken,
                TaskFactoryExtensions.ContinuationOptionsFromCreationOptions(creationOptions) |
                TaskContinuationOptions.ExecuteSynchronously, scheduler);
            timer = new Timer((TimerCallback) (obj => ((Task) obj).Start(scheduler)), (object) task, millisecondsDelay,
                -1);
            return result.Task;
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="action">The delegate for the task.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task Create(this TaskFactory factory, Action action)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return new Task(action, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="action">The delegate for the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task Create(this TaskFactory factory, Action action, TaskCreationOptions creationOptions)
        {
            return new Task(action, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="action">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task Create(this TaskFactory factory, Action<object> action, object state)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return new Task(action, state, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="action">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task Create(this TaskFactory factory, Action<object> action, object state,
            TaskCreationOptions creationOptions)
        {
            return new Task(action, state, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory factory, Func<TResult> function)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return new Task<TResult>(function, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory factory, Func<TResult> function,
            TaskCreationOptions creationOptions)
        {
            return new Task<TResult>(function, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory factory, Func<object, TResult> function,
            object state)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return new Task<TResult>(function, state, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory factory, Func<object, TResult> function,
            object state, TaskCreationOptions creationOptions)
        {
            return new Task<TResult>(function, state, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory<TResult> factory, Func<TResult> function)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return new Task<TResult>(function, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory<TResult> factory, Func<TResult> function,
            TaskCreationOptions creationOptions)
        {
            return new Task<TResult>(function, factory.CancellationToken, creationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory<TResult> factory, Func<object, TResult> function,
            object state)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            return new Task<TResult>(function, state, factory.CancellationToken, factory.CreationOptions);
        }

        /// <summary>Creates a Task using the TaskFactory.</summary>
        /// <param name="factory">The factory to use.</param>
        /// <param name="function">The delegate for the task.</param>
        /// <param name="state">An object provided to the delegate.</param>
        /// <param name="creationOptions">Options that control the task's behavior.</param>
        /// <returns>The created task.  The task has not been scheduled.</returns>
        public static Task<TResult> Create<TResult>(this TaskFactory<TResult> factory, Func<object, TResult> function,
            object state, TaskCreationOptions creationOptions)
        {
            return new Task<TResult>(function, state, factory.CancellationToken, creationOptions);
        }

        /// <summary>
        /// Creates a continuation Task that will compplete upon
        /// the completion of a set of provided Tasks.
        /// </summary>
        /// <param name="factory">The TaskFactory to use to create the continuation task.</param>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <returns>A task that, when completed, will return the array of completed tasks.</returns>
        public static Task<Task[]> WhenAll(this TaskFactory factory, params Task[] tasks)
        {
            return factory.ContinueWhenAll<Task[]>(tasks, (Func<Task[], Task[]>) (completedTasks => completedTasks));
        }

        /// <summary>
        /// Creates a continuation Task that will compplete upon
        /// the completion of a set of provided Tasks.
        /// </summary>
        /// <param name="factory">The TaskFactory to use to create the continuation task.</param>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <returns>A task that, when completed, will return the array of completed tasks.</returns>
        public static Task<Task<TAntecedentResult>[]> WhenAll<TAntecedentResult>(this TaskFactory factory,
            params Task<TAntecedentResult>[] tasks)
        {
            return factory.ContinueWhenAll<TAntecedentResult, Task<TAntecedentResult>[]>(tasks,
                (Func<Task<TAntecedentResult>[], Task<TAntecedentResult>[]>) (completedTasks => completedTasks));
        }

        /// <summary>
        /// Creates a continuation Task that will complete upon
        /// the completion of any one of a set of provided Tasks.
        /// </summary>
        /// <param name="factory">The TaskFactory to use to create the continuation task.</param>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <returns>A task that, when completed, will return the completed task.</returns>
        public static Task<Task> WhenAny(this TaskFactory factory, params Task[] tasks)
        {
            return factory.ContinueWhenAny<Task>(tasks, (Func<Task, Task>) (completedTask => completedTask));
        }

        /// <summary>
        /// Creates a continuation Task that will complete upon
        /// the completion of any one of a set of provided Tasks.
        /// </summary>
        /// <param name="factory">The TaskFactory to use to create the continuation task.</param>
        /// <param name="tasks">The array of tasks from which to continue.</param>
        /// <returns>A task that, when completed, will return the completed task.</returns>
        public static Task<Task<TAntecedentResult>> WhenAny<TAntecedentResult>(this TaskFactory factory,
            params Task<TAntecedentResult>[] tasks)
        {
            return factory.ContinueWhenAny<TAntecedentResult, Task<TAntecedentResult>>(tasks,
                (Func<Task<TAntecedentResult>, Task<TAntecedentResult>>) (completedTask => completedTask));
        }
    }
}