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
      Name = name;
      m_timers = new List<SimpleTimerEntry>();
      m_updatables = new List<IUpdatable>();
      m_messageQueue = new LockfreeQueue<IMessage>();
      m_queueTimer = Stopwatch.StartNew();
      m_updateInterval = updateInterval;
      m_lastUpdate = 0;
      IsRunning = running;
    }

    public string Name { get; set; }

    public bool IsRunning
    {
      get { return _running; }
      set
      {
        if(_running == value)
          return;
        _running = value;
        if(value)
          _updateTask = Task.Factory.StartNewDelayed(m_updateInterval,
            QueueUpdateCallback, this);
      }
    }

    /// <summary>Update interval in milliseconds</summary>
    public int UpdateInterval
    {
      get { return m_updateInterval; }
      set { m_updateInterval = value; }
    }

    public long LastUpdateTime
    {
      get { return m_lastUpdate; }
    }

    /// <summary>
    /// Registers an updatable object in the server timer pool.
    /// </summary>
    /// <param name="updatable">the object to register</param>
    public void RegisterUpdatable(IUpdatable updatable)
    {
      AddMessage(() => m_updatables.Add(updatable));
    }

    /// <summary>
    /// Unregisters an updatable object from the server timer pool.
    /// </summary>
    /// <param name="updatable">the object to unregister</param>
    public void UnregisterUpdatable(IUpdatable updatable)
    {
      AddMessage(() => m_updatables.Remove(updatable));
    }

    /// <summary>
    /// Registers the given Updatable during the next Map Tick
    /// </summary>
    public void RegisterUpdatableLater(IUpdatable updatable)
    {
      m_messageQueue.Enqueue(new Message(() => RegisterUpdatable(updatable)));
    }

    /// <summary>
    /// Unregisters the given Updatable during the next Map Update
    /// </summary>
    public void UnregisterUpdatableLater(IUpdatable updatable)
    {
      m_messageQueue.Enqueue(new Message(() => UnregisterUpdatable(updatable)));
    }

    public SimpleTimerEntry CallPeriodically(int delayMillis, Action callback)
    {
      SimpleTimerEntry simpleTimerEntry =
        new SimpleTimerEntry(delayMillis, callback, m_lastUpdate, false);
      m_timers.Add(simpleTimerEntry);
      return simpleTimerEntry;
    }

    public SimpleTimerEntry CallDelayed(int delayMillis, Action callback)
    {
      SimpleTimerEntry simpleTimerEntry =
        new SimpleTimerEntry(delayMillis, callback, m_lastUpdate, true);
      m_timers.Add(simpleTimerEntry);
      return simpleTimerEntry;
    }

    /// <summary>Stops running the given timer</summary>
    public void CancelTimer(SimpleTimerEntry entry)
    {
      m_timers.Remove(entry);
    }

    internal int GetDelayUntilNextExecution(SimpleTimerEntry timer)
    {
      return timer.Delay - (int) (LastUpdateTime - timer.LastCallTime);
    }

    /// <summary>Queues a task for execution in the server task pool.</summary>
    public void AddMessage(Action action)
    {
      m_messageQueue.Enqueue(new Message(action));
    }

    /// <summary>removes all messages from queue</summary>
    public void Clear()
    {
      m_messageQueue.Clear();
    }

    public bool ExecuteInContext(Action action)
    {
      if(!IsInContext)
      {
        AddMessage(new Message(action));
        return false;
      }

      try
      {
        action();
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException(ex);
      }

      return true;
    }

    public void EnsureContext()
    {
      if(Thread.CurrentThread.ManagedThreadId != _currentUpdateThreadId)
        throw new InvalidOperationException("Not in context");
    }

    /// <summary>
    /// Indicates whether the current Thread is the processor of the MessageQueue
    /// </summary>
    public bool IsInContext
    {
      get { return Thread.CurrentThread.ManagedThreadId == _currentUpdateThreadId; }
    }

    /// <summary>Queues a task for execution in the server task pool.</summary>
    /// <param name="msg"></param>
    public void AddMessage(IMessage msg)
    {
      m_messageQueue.Enqueue(msg);
    }

    protected void QueueUpdateCallback(object state)
    {
      try
      {
        if(!_running || Interlocked.CompareExchange(ref _currentUpdateThreadId,
             Thread.CurrentThread.ManagedThreadId, 0) != 0)
          return;
        long elapsedMilliseconds1 = m_queueTimer.ElapsedMilliseconds;
        int dt = (int) (elapsedMilliseconds1 - m_lastUpdate);
        m_lastUpdate = (int) elapsedMilliseconds1;
        foreach(IUpdatable updatable in m_updatables)
        {
          try
          {
            updatable.Update(dt);
          }
          catch(Exception ex)
          {
            LogUtil.ErrorException(ex, "Failed to update: " + updatable);
          }
        }

        for(int index = m_timers.Count - 1; index >= 0; --index)
        {
          SimpleTimerEntry timer = m_timers[index];
          if(GetDelayUntilNextExecution(timer) <= 0)
          {
            try
            {
              timer.Execute(this);
            }
            catch(Exception ex)
            {
              LogUtil.ErrorException(ex, "Failed to execute timer: " + timer);
            }
          }
        }

        IMessage message;
        while(m_messageQueue.TryDequeue(out message))
        {
          try
          {
            message.Execute();
          }
          catch(Exception ex)
          {
            LogUtil.ErrorException(ex, "Failed to execute message: " + message);
          }

          if(!_running)
            return;
        }

        long elapsedMilliseconds2 = m_queueTimer.ElapsedMilliseconds;
        long num = elapsedMilliseconds2 - elapsedMilliseconds1 > (long) m_updateInterval
          ? 0L
          : elapsedMilliseconds1 + m_updateInterval - elapsedMilliseconds2;
        Interlocked.Exchange(ref _currentUpdateThreadId, 0);
        if(_running)
          _updateTask = Task.Factory.StartNewDelayed((int) num,
            QueueUpdateCallback, this);
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException(ex, "Failed to run TaskQueue callback for \"{0}\"", (object) Name);
      }
    }

    /// <summary>Ensures execution outside the Map-context.</summary>
    /// <exception cref="T:System.InvalidOperationException">thrown if the calling thread is the map thread</exception>
    public void EnsureNoContext()
    {
      if(Thread.CurrentThread.ManagedThreadId == _currentUpdateThreadId)
        throw new InvalidOperationException("Application Queue context prohibited.");
    }

    /// <summary>
    /// Adds the given message to the map's message queue and does not return
    /// until the message is processed.
    /// </summary>
    /// <remarks>Make sure that the map is running before calling this method.</remarks>
    /// <remarks>Must not be called from the map context.</remarks>
    public void AddMessageAndWait(bool allowInstantExecution, Action action)
    {
      AddMessageAndWait(allowInstantExecution, new Message(action));
    }

    /// <summary>
    /// Adds the given message to the map's message queue and does not return
    /// until the message is processed.
    /// </summary>
    /// <remarks>Make sure that the map is running before calling this method.</remarks>
    /// <remarks>Must not be called from the map context.</remarks>
    public void AddMessageAndWait(bool allowInstantExecution, IMessage msg)
    {
      if(allowInstantExecution && IsInContext)
      {
        msg.Execute();
      }
      else
      {
        EnsureNoContext();
        SimpleUpdatable updatable = new SimpleUpdatable();
        updatable.Callback = () => AddMessage(new Message(() =>
        {
          msg.Execute();
          lock(msg)
            Monitor.PulseAll(msg);
          UnregisterUpdatable(updatable);
        }));
        lock(msg)
        {
          RegisterUpdatableLater(updatable);
          Monitor.Wait(msg);
        }
      }
    }

    /// <summary>Waits for one map tick before returning.</summary>
    /// <remarks>Must not be called from the map context.</remarks>
    public void WaitOneTick()
    {
      AddMessageAndWait(false, new Message(() => { }));
    }

    /// <summary>
    /// Waits for the given amount of ticks.
    /// One tick might take 0 until Map.UpdateSpeed milliseconds.
    /// </summary>
    /// <remarks>Make sure that the map is running before calling this method.</remarks>
    /// <remarks>Must not be called from the map context.</remarks>
    public void WaitTicks(int ticks)
    {
      EnsureNoContext();
      for(int index = 0; index < ticks; ++index)
        WaitOneTick();
    }
  }
}