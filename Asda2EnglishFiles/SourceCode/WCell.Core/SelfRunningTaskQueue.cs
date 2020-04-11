using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WCell.Core.Timers;
using WCell.Util.Collections;
using WCell.Util.NLog;
using WCell.Util.Threading;
using WCell.Util.Threading.TaskParallel;

namespace WCell.Core
{
    public class SelfRunningTaskQueue : IContextHandler
    {
        private bool _running;
        protected List<SimpleTimerEntry> m_timers;
        protected List<IUpdatable> m_updatables;
        protected LockfreeQueue<IMessage> m_messageQueue;
        protected Task _updateTask;
        protected int _currentUpdateThreadId;
        protected Stopwatch m_queueTimer;
        protected int m_updateInterval;
        protected int m_lastUpdate;

        public SelfRunningTaskQueue(int updateInterval, string name, bool running = true)
        {
            this.Name = name;
            this.m_timers = new List<SimpleTimerEntry>();
            this.m_updatables = new List<IUpdatable>();
            this.m_messageQueue = new LockfreeQueue<IMessage>();
            this.m_queueTimer = Stopwatch.StartNew();
            this.m_updateInterval = updateInterval;
            this.m_lastUpdate = 0;
            this.IsRunning = running;
        }

        public string Name { get; set; }

        public bool IsRunning
        {
            get { return this._running; }
            set
            {
                if (this._running == value)
                    return;
                this._running = value;
                if (value)
                    this._updateTask = Task.Factory.StartNewDelayed(this.m_updateInterval,
                        new Action<object>(this.QueueUpdateCallback), (object) this);
            }
        }

        /// <summary>Update interval in milliseconds</summary>
        public int UpdateInterval
        {
            get { return this.m_updateInterval; }
            set { this.m_updateInterval = value; }
        }

        public long LastUpdateTime
        {
            get { return (long) this.m_lastUpdate; }
        }

        /// <summary>
        /// Registers an updatable object in the server timer pool.
        /// </summary>
        /// <param name="updatable">the object to register</param>
        public void RegisterUpdatable(IUpdatable updatable)
        {
            this.AddMessage((Action) (() => this.m_updatables.Add(updatable)));
        }

        /// <summary>
        /// Unregisters an updatable object from the server timer pool.
        /// </summary>
        /// <param name="updatable">the object to unregister</param>
        public void UnregisterUpdatable(IUpdatable updatable)
        {
            this.AddMessage((Action) (() => this.m_updatables.Remove(updatable)));
        }

        /// <summary>
        /// Registers the given Updatable during the next Map Tick
        /// </summary>
        public void RegisterUpdatableLater(IUpdatable updatable)
        {
            this.m_messageQueue.Enqueue((IMessage) new Message((Action) (() => this.RegisterUpdatable(updatable))));
        }

        /// <summary>
        /// Unregisters the given Updatable during the next Map Update
        /// </summary>
        public void UnregisterUpdatableLater(IUpdatable updatable)
        {
            this.m_messageQueue.Enqueue((IMessage) new Message((Action) (() => this.UnregisterUpdatable(updatable))));
        }

        public SimpleTimerEntry CallPeriodically(int delayMillis, Action callback)
        {
            SimpleTimerEntry simpleTimerEntry =
                new SimpleTimerEntry(delayMillis, callback, (long) this.m_lastUpdate, false);
            this.m_timers.Add(simpleTimerEntry);
            return simpleTimerEntry;
        }

        public SimpleTimerEntry CallDelayed(int delayMillis, Action callback)
        {
            SimpleTimerEntry simpleTimerEntry =
                new SimpleTimerEntry(delayMillis, callback, (long) this.m_lastUpdate, true);
            this.m_timers.Add(simpleTimerEntry);
            return simpleTimerEntry;
        }

        /// <summary>Stops running the given timer</summary>
        public void CancelTimer(SimpleTimerEntry entry)
        {
            this.m_timers.Remove(entry);
        }

        internal int GetDelayUntilNextExecution(SimpleTimerEntry timer)
        {
            return timer.Delay - (int) (this.LastUpdateTime - timer.LastCallTime);
        }

        /// <summary>Queues a task for execution in the server task pool.</summary>
        public void AddMessage(Action action)
        {
            this.m_messageQueue.Enqueue((IMessage) new Message(action));
        }

        /// <summary>removes all messages from queue</summary>
        public void Clear()
        {
            this.m_messageQueue.Clear();
        }

        public bool ExecuteInContext(Action action)
        {
            if (!this.IsInContext)
            {
                this.AddMessage((IMessage) new Message(action));
                return false;
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex);
            }

            return true;
        }

        public void EnsureContext()
        {
            if (Thread.CurrentThread.ManagedThreadId != this._currentUpdateThreadId)
                throw new InvalidOperationException("Not in context");
        }

        /// <summary>
        /// Indicates whether the current Thread is the processor of the MessageQueue
        /// </summary>
        public bool IsInContext
        {
            get { return Thread.CurrentThread.ManagedThreadId == this._currentUpdateThreadId; }
        }

        /// <summary>Queues a task for execution in the server task pool.</summary>
        /// <param name="msg"></param>
        public void AddMessage(IMessage msg)
        {
            this.m_messageQueue.Enqueue(msg);
        }

        protected void QueueUpdateCallback(object state)
        {
            try
            {
                if (!this._running || Interlocked.CompareExchange(ref this._currentUpdateThreadId,
                        Thread.CurrentThread.ManagedThreadId, 0) != 0)
                    return;
                long elapsedMilliseconds1 = this.m_queueTimer.ElapsedMilliseconds;
                int dt = (int) (elapsedMilliseconds1 - (long) this.m_lastUpdate);
                this.m_lastUpdate = (int) elapsedMilliseconds1;
                foreach (IUpdatable updatable in this.m_updatables)
                {
                    try
                    {
                        updatable.Update(dt);
                    }
                    catch (Exception ex)
                    {
                        LogUtil.ErrorException(ex, "Failed to update: " + (object) updatable, new object[0]);
                    }
                }

                for (int index = this.m_timers.Count - 1; index >= 0; --index)
                {
                    SimpleTimerEntry timer = this.m_timers[index];
                    if (this.GetDelayUntilNextExecution(timer) <= 0)
                    {
                        try
                        {
                            timer.Execute(this);
                        }
                        catch (Exception ex)
                        {
                            LogUtil.ErrorException(ex, "Failed to execute timer: " + (object) timer, new object[0]);
                        }
                    }
                }

                IMessage message;
                while (this.m_messageQueue.TryDequeue(out message))
                {
                    try
                    {
                        message.Execute();
                    }
                    catch (Exception ex)
                    {
                        LogUtil.ErrorException(ex, "Failed to execute message: " + (object) message, new object[0]);
                    }

                    if (!this._running)
                        return;
                }

                long elapsedMilliseconds2 = this.m_queueTimer.ElapsedMilliseconds;
                long num = elapsedMilliseconds2 - elapsedMilliseconds1 > (long) this.m_updateInterval
                    ? 0L
                    : elapsedMilliseconds1 + (long) this.m_updateInterval - elapsedMilliseconds2;
                Interlocked.Exchange(ref this._currentUpdateThreadId, 0);
                if (this._running)
                    this._updateTask = Task.Factory.StartNewDelayed((int) num,
                        new Action<object>(this.QueueUpdateCallback), (object) this);
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex, "Failed to run TaskQueue callback for \"{0}\"", new object[1]
                {
                    (object) this.Name
                });
            }
        }

        /// <summary>Ensures execution outside the Map-context.</summary>
        /// <exception cref="T:System.InvalidOperationException">thrown if the calling thread is the map thread</exception>
        public void EnsureNoContext()
        {
            if (Thread.CurrentThread.ManagedThreadId == this._currentUpdateThreadId)
                throw new InvalidOperationException(string.Format("Application Queue context prohibited."));
        }

        /// <summary>
        /// Adds the given message to the map's message queue and does not return
        /// until the message is processed.
        /// </summary>
        /// <remarks>Make sure that the map is running before calling this method.</remarks>
        /// <remarks>Must not be called from the map context.</remarks>
        public void AddMessageAndWait(bool allowInstantExecution, Action action)
        {
            this.AddMessageAndWait(allowInstantExecution, (IMessage) new Message(action));
        }

        /// <summary>
        /// Adds the given message to the map's message queue and does not return
        /// until the message is processed.
        /// </summary>
        /// <remarks>Make sure that the map is running before calling this method.</remarks>
        /// <remarks>Must not be called from the map context.</remarks>
        public void AddMessageAndWait(bool allowInstantExecution, IMessage msg)
        {
            if (allowInstantExecution && this.IsInContext)
            {
                msg.Execute();
            }
            else
            {
                this.EnsureNoContext();
                SimpleUpdatable updatable = new SimpleUpdatable();
                updatable.Callback = (Action) (() => this.AddMessage((IMessage) new Message((Action) (() =>
                {
                    msg.Execute();
                    lock (msg)
                        Monitor.PulseAll((object) msg);
                    this.UnregisterUpdatable((IUpdatable) updatable);
                }))));
                lock (msg)
                {
                    this.RegisterUpdatableLater((IUpdatable) updatable);
                    Monitor.Wait((object) msg);
                }
            }
        }

        /// <summary>Waits for one map tick before returning.</summary>
        /// <remarks>Must not be called from the map context.</remarks>
        public void WaitOneTick()
        {
            this.AddMessageAndWait(false, (IMessage) new Message((Action) (() => { })));
        }

        /// <summary>
        /// Waits for the given amount of ticks.
        /// One tick might take 0 until Map.UpdateSpeed milliseconds.
        /// </summary>
        /// <remarks>Make sure that the map is running before calling this method.</remarks>
        /// <remarks>Must not be called from the map context.</remarks>
        public void WaitTicks(int ticks)
        {
            this.EnsureNoContext();
            for (int index = 0; index < ticks; ++index)
                this.WaitOneTick();
        }
    }
}