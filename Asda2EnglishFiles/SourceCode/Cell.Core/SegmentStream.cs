using System;
using System.IO;

namespace Cell.Core
{
    /// <summary>
    /// Similar to MemoryStream, but with an underlying BufferSegment.
    /// Will automatically free the old and get a new segment if its length was exceeded.
    /// </summary>
    public class SegmentStream : Stream
    {
        private int m_Position;
        private BufferSegment _segment;
        private int __length;

        private int _length
        {
            get { return this.__length; }
            set { this.__length = value; }
        }

        public SegmentStream(BufferSegment segment)
        {
            this._segment = segment;
            this.m_Position = this._segment.Offset;
        }

        public BufferSegment Segment
        {
            get { return this._segment; }
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.m_Position = (int) offset;
                    break;
                case SeekOrigin.Current:
                    this.m_Position += (int) offset;
                    break;
                case SeekOrigin.End:
                    this.m_Position = this._segment.Offset + this._segment.Length - (int) offset;
                    break;
            }

            if (this.m_Position > this._segment.Length)
                this.m_Position = this._segment.Length;
            return (long) this.m_Position;
        }

        public override void SetLength(long value)
        {
            this._length = (int) value;
            if (this.m_Position > this._length)
                this.m_Position = this._length + this._segment.Offset;
            if (this._length <= this._segment.Length)
                return;
            this.EnsureCapacity(this._length);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = Math.Min(count, this._segment.Offset + this._segment.Length - this.m_Position);
            Buffer.BlockCopy((Array) this._segment.Buffer.Array, this.m_Position, (Array) buffer, offset, count);
            this.m_Position += count;
            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.m_Position + count >= this._segment.Offset + this._segment.Length)
                this.EnsureCapacity(this.m_Position - this._segment.Offset + count);
            Buffer.BlockCopy((Array) buffer, offset, (Array) this._segment.Buffer.Array, this.m_Position, count);
            this.m_Position += count;
            this._length = Math.Max(this._length, this.m_Position - this._segment.Offset);
        }

        public override int ReadByte()
        {
            return (int) this._segment.Buffer.Array[this.m_Position++];
        }

        public override void WriteByte(byte value)
        {
            if (this.m_Position + 1 >= this._segment.Offset + this._segment.Length)
                this.EnsureCapacity(this.m_Position - this._segment.Offset + 1);
            this._segment.Buffer.Array[this.m_Position++] = value;
            this._length = Math.Max(this._length, this.m_Position - this._segment.Offset);
        }

        private void CheckOpcode(int count)
        {
        }

        private void EnsureCapacity(int size)
        {
            BufferSegment segment = BufferManager.GetSegment(size);
            this._segment.CopyTo(segment, this._length);
            this.m_Position = this.m_Position - this._segment.Offset + segment.Offset;
            this._segment.DecrementUsage();
            this._segment = segment;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return (long) this._length; }
        }

        public override long Position
        {
            get { return (long) (this.m_Position - this._segment.Offset); }
            set { this.m_Position = (int) value + this._segment.Offset; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this._segment.DecrementUsage();
        }
    }
}