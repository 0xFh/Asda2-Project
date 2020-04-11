using System;
using System.Collections.Concurrent;

namespace WCell.Util.Threading.ActorModel
{
    public abstract class Actor : IDisposable
    {
        private readonly ConcurrentBag<IChannel> _channels = new ConcurrentBag<IChannel>();
        internal int Status;

        protected Actor()
        {
            this.Lock = new object();
        }

        internal object Lock { get; private set; }

        public bool Exited { get; private set; }

        internal void AddChannel(IChannel channel)
        {
            this._channels.Add(channel);
        }

        public void Dispose()
        {
            if (this.Exited)
                throw new ObjectDisposedException("Actor has already been disposed.");
            foreach (IChannel channel in this._channels)
                channel.Wait();
            this.Exited = true;
        }
    }
}