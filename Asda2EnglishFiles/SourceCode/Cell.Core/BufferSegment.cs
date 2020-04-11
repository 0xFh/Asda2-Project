﻿using System;
using System.Threading;

namespace Cell.Core
{
    public class BufferSegment
    {
        public readonly ArrayBuffer Buffer;
        public readonly int Offset;
        public readonly int Length;
        internal int m_uses;

        public BufferSegment(ArrayBuffer buffer, int offset, int length, int id)
        {
            this.Buffer = buffer;
            this.Offset = offset;
            this.Length = length;
            this.Number = id;
        }

        /// <summary>
        /// Returns the byte as the given index within this segment.
        /// </summary>
        /// <param name="i">the offset in this segment to go</param>
        /// <returns>the byte at the index, or 0 if the index is out-of-bounds</returns>
        public byte this[int i]
        {
            get { return this.Buffer.Array[this.Offset + i]; }
        }

        /// <summary>
        /// Returns a deep-copy of the data mapped to this segment.
        /// </summary>
        public byte[] SegmentData
        {
            get
            {
                byte[] numArray = new byte[this.Length];
                System.Buffer.BlockCopy((Array) this.Buffer.Array, this.Offset, (Array) numArray, 0, this.Length);
                return numArray;
            }
        }

        /// <summary>The number of users still using this segment.</summary>
        public int Uses
        {
            get { return this.m_uses; }
        }

        /// <summary>Unique segment identifier.</summary>
        public int Number { get; internal set; }

        /// <summary>
        /// Copies the contents of the given array into this segment at the given offset.
        /// </summary>
        /// <param name="bytes">the buffer to read from</param>
        /// <param name="offset">the offset to start reading from</param>
        /// <exception cref="T:System.ArgumentException">an ArgumentException will be thrown if offset is greater than
        /// the length of the buffer</exception>
        public void CopyFrom(byte[] bytes, int offset)
        {
            System.Buffer.BlockCopy((Array) bytes, offset, (Array) this.Buffer.Array, this.Offset + offset,
                bytes.Length - offset);
        }

        /// <summary>
        /// Copys the data in this segment to another <see cref="T:Cell.Core.BufferSegment" />.
        /// </summary>
        /// <param name="segment">the <see cref="T:Cell.Core.BufferSegment" /> instance to copy to</param>
        /// <param name="length">the amount of bytes to copy from this segment</param>
        /// <exception cref="T:System.ArgumentException">an ArgumentException will be thrown if length is greater than
        /// the length of the segment</exception>
        public void CopyTo(BufferSegment segment, int length)
        {
            System.Buffer.BlockCopy((Array) this.Buffer.Array, this.Offset, (Array) segment.Buffer.Array,
                segment.Offset, length);
        }

        /// <summary>Increments the usage counter of this segment.</summary>
        public void IncrementUsage()
        {
            Interlocked.Increment(ref this.m_uses);
        }

        /// <summary>Decrements the usage counter of this segment.</summary>
        /// <remarks>When the usage counter reaches 0, the segment will be
        /// returned to the buffer pool.</remarks>
        public void DecrementUsage()
        {
            if (Interlocked.Decrement(ref this.m_uses) != 0)
                return;
            this.Buffer.CheckIn(this);
        }

        /// <summary>Creates a new BufferSegment for the given buffer.</summary>
        /// <param name="bytes">the buffer to wrap</param>
        /// <returns>a new BufferSegment wrapping the given buffer</returns>
        /// <remarks>This will also create an underlying ArrayBuffer to pin the buffer
        /// for the BufferSegment.  The ArrayBuffer will be disposed when the segment
        /// is released.</remarks>
        public static BufferSegment CreateSegment(byte[] bytes)
        {
            return BufferSegment.CreateSegment(bytes, 0, bytes.Length);
        }

        /// <summary>Creates a new BufferSegment for the given buffer.</summary>
        /// <param name="bytes">the buffer to wrap</param>
        /// <param name="offset">the offset of the buffer to read from</param>
        /// <param name="length">the length of the data to read</param>
        /// <returns>a new BufferSegment wrapping the given buffer</returns>
        /// <remarks>This will also create an underlying ArrayBuffer to pin the buffer
        /// for the BufferSegment.  The ArrayBuffer will be disposed when the segment
        /// is released.</remarks>
        public static BufferSegment CreateSegment(byte[] bytes, int offset, int length)
        {
            return new BufferSegment(new ArrayBuffer(bytes), offset, length, -1);
        }
    }
}