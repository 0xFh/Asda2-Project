using System.Collections.Concurrent;
using System.Threading;

namespace WCell.Util.Threading.ActorModel
{
  public abstract class Channel<TActor, TMessage> : IChannel where TActor : Actor
  {
    private readonly ConcurrentQueue<TMessage> _queue = new ConcurrentQueue<TMessage>();

    protected Channel(TActor actor)
    {
      Actor = actor;
      actor.AddChannel(this);
    }

    public TActor Actor { get; private set; }

    void IChannel.Wait()
    {
      while(!_queue.IsEmpty)
        Thread.SpinWait(1);
    }

    public void Send(TMessage msg)
    {
      if(Actor.Exited)
        return;
      _queue.Enqueue(msg);
      Execute();
    }

    public T Receive<T>(TMessage msg)
    {
      if(Actor.Exited)
        throw new ActorException("Actor has exited.");
      bool lockTaken = false;
      object obj = null;
      try
      {
        TActor actor = Actor;
        Monitor.Enter(obj = actor.Lock, ref lockTaken);
        return (T) OnTwoWayMessage(msg);
      }
      finally
      {
        if(lockTaken)
          Monitor.Exit(obj);
      }
    }

    private void Execute()
    {
      if(Actor.Exited || Interlocked.CompareExchange(ref Actor.Status, 1, 0) != 0)
        return;
      ThreadPool.QueueUserWorkItem(PoolCallback, Actor);
    }

    private void PoolCallback(object state)
    {
      bool lockTaken = false;
      object obj = null;
      try
      {
        TActor actor = Actor;
        Monitor.Enter(obj = actor.Lock, ref lockTaken);
        TMessage result;
        while(_queue.TryDequeue(out result))
          OnOneWayMessage(result);
      }
      finally
      {
        if(lockTaken)
          Monitor.Exit(obj);
      }

      if(!Actor.Exited)
      {
        Interlocked.Exchange(ref Actor.Status, 0);
        if(_queue.IsEmpty)
          return;
        Execute();
      }
      else
        Interlocked.Exchange(ref Actor.Status, 2);
    }

    protected virtual void OnOneWayMessage(TMessage msg)
    {
      throw new ActorException("This actor does not support one-way messages.");
    }

    protected virtual object OnTwoWayMessage(TMessage msg)
    {
      throw new ActorException("This actor does not support two-way messages.");
    }
  }
}