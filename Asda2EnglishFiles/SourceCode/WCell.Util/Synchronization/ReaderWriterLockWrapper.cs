using System;
using System.Threading;

namespace WCell.Util
{
    public class ReaderWriterLockWrapper
    {
        private readonly ReaderWriterLockSlim lck;
        private readonly ReaderWriterLockWrapper.ReaderWrapper reader;
        private readonly ReaderWriterLockWrapper.WriterWrapper writer;

        public ReaderWriterLockWrapper()
        {
            this.lck = new ReaderWriterLockSlim();
            this.reader = new ReaderWriterLockWrapper.ReaderWrapper(this.lck);
            this.writer = new ReaderWriterLockWrapper.WriterWrapper(this.lck);
        }

        public IDisposable EnterReadLock()
        {
            this.lck.EnterReadLock();
            return (IDisposable) this.reader;
        }

        public IDisposable EnterWriteLock()
        {
            this.lck.EnterWriteLock();
            return (IDisposable) this.writer;
        }

        private struct ReaderWrapper : IDisposable
        {
            private readonly ReaderWriterLockSlim lck;

            public ReaderWrapper(ReaderWriterLockSlim lck)
            {
                this.lck = lck;
            }

            public void Dispose()
            {
                this.lck.ExitReadLock();
            }
        }

        private struct WriterWrapper : IDisposable
        {
            private readonly ReaderWriterLockSlim lck;

            public WriterWrapper(ReaderWriterLockSlim lck)
            {
                this.lck = lck;
            }

            public void Dispose()
            {
                this.lck.ExitWriteLock();
            }
        }
    }
}