using System.Collections.Concurrent;
using System.Threading;

namespace WCell.Util.Threading.ActorModel
{
    public abstract class Channel<TActor, TMessage> : IChannel where TActor : WCell.Util.Threading.ActorModel.Actor
    {
        private readonly ConcurrentQueue<TMessage> _queue = new ConcurrentQueue<TMessage>();

        protected Channel(TActor actor)
        {
            this.Actor = actor;
            actor.AddChannel((IChannel) this);
        }

        public TActor Actor { get; private set; }

        void IChannel.Wait()
        {
            while (!this._queue.IsEmpty)
                Thread.SpinWait(1);
        }

        public void Send(TMessage msg)
        {
            if (this.Actor.Exited)
                return;
            this._queue.Enqueue(msg);
            this.Execute();
        }

        public T Receive<T>(TMessage msg)
        {
            if (this.Actor.Exited)
                throw new ActorException("Actor has exited.");
            bool lockTaken = false;
            object obj = null;
            try
            {
                TActor actor = this.Actor;
                Monitor.Enter(obj = actor.Lock, ref lockTaken);
                return (T) this.OnTwoWayMessage(msg);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(obj);
            }
        }

        private void Execute()
        {
            if (this.Actor.Exited || Interlocked.CompareExchange(ref this.Actor.Status, 1, 0) != 0)
                return;
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.PoolCallback), (object) this.Actor);
        }

        private void PoolCallback(object state)
        {
            bool lockTaken = false;
            object obj = null;
            try
            {
                TActor actor = this.Actor;
                Monitor.Enter(obj = actor.Lock, ref lockTaken);
                TMessage result;
                while (this._queue.TryDequeue(out result))
                    this.OnOneWayMessage(result);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(obj);
            }

            if (!this.Actor.Exited)
            {
                Interlocked.Exchange(ref this.Actor.Status, 0);
                if (this._queue.IsEmpty)
                    return;
                this.Execute();
            }
            else
                Interlocked.Exchange(ref this.Actor.Status, 2);
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