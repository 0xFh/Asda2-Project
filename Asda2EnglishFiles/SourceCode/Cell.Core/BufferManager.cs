using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using WCell.Util.Collections;

namespace Cell.Core
{
    /// <summary>
    /// Manages a pool of small buffers allocated from large, contiguous chunks of memory.
    /// </summary>
    /// <remarks>
    /// When used in an async network call, a buffer is pinned. Large numbers of pinned buffers
    /// cause problem with the GC (in particular it causes heap fragmentation).
    /// 
    /// This class maintains a set of large segments and gives clients pieces of these
    /// segments that they can use for their buffers. The alternative to this would be to
    /// create many small arrays which it then maintained. This methodology should be slightly
    /// better than the many small array methodology because in creating only a few very
    /// large objects it will force these objects to be placed on the LOH. Since the
    /// objects are on the LOH they are at this time not subject to compacting which would
    /// require an update of all GC roots as would be the case with lots of smaller arrays
    /// that were in the normal heap.
    /// </remarks>
    public class BufferManager
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();
        public static readonly List<BufferManager> Managers = new List<BufferManager>();

        /// <summary>
        /// Default BufferManager for small buffers with up to 128 bytes length
        /// </summary>
        public static readonly BufferManager Tiny = new BufferManager(1024, 128);

        /// <summary>
        /// Default BufferManager for small buffers with up to 1kb length
        /// </summary>
        public static readonly BufferManager Small = new BufferManager(1024, 1024);

        /// <summary>
        /// Default BufferManager for default-sized buffers (usually up to 8kb)
        /// </summary>
        public static readonly BufferManager Default = new BufferManager(512, 8192);

        /// <summary>Large BufferManager for buffers up to 64kb size</summary>
        public static readonly BufferManager Large = new BufferManager(128, 65536);

        /// <summary>Extra Large BufferManager holding 512kb buffers</summary>
        public static readonly BufferManager ExtraLarge = new BufferManager(32, 524288);

        /// <summary>Super Large BufferManager holding 1MB buffers</summary>
        public static readonly BufferManager SuperSized = new BufferManager(16, 1048576);

        /// <summary>
        /// Holds the total amount of memory allocated by all buffer managers.
        /// </summary>
        public static long GlobalAllocatedMemory = 0;

        /// <summary>Count of segments per Buffer</summary>
        private readonly int _segmentCount;

        /// <summary>Segment size</summary>
        private readonly int _segmentSize;

        /// <summary>Total count of segments in all buffers</summary>
        private int _totalSegmentCount;

        private static volatile int _segmentId;
        private readonly List<ArrayBuffer> _buffers;
        private readonly LockfreeQueue<BufferSegment> _availableSegments;

        /// <summary>The number of currently available segments</summary>
        public int AvailableSegmentsCount
        {
            get { return this._availableSegments.Count; }
        }

        public bool InUse
        {
            get { return this._availableSegments.Count < this._totalSegmentCount; }
        }

        public int UsedSegmentCount
        {
            get { return this._totalSegmentCount - this._availableSegments.Count; }
        }

        /// <summary>The total number of currently allocated buffers.</summary>
        public int TotalBufferCount
        {
            get { return this._buffers.Count; }
        }

        /// <summary>The total number of currently allocated segments.</summary>
        public int TotalSegmentCount
        {
            get { return this._totalSegmentCount; }
        }

        /// <summary>The total amount of all currently allocated buffers.</summary>
        public int TotalAllocatedMemory
        {
            get { return this._buffers.Count * (this._segmentCount * this._segmentSize); }
        }

        /// <summary>The size of a single segment</summary>
        public int SegmentSize
        {
            get { return this._segmentSize; }
        }

        /// <summary>
        /// Constructs a new <see cref="F:Cell.Core.BufferManager.Default"></see> object
        /// </summary>
        /// <param name="segmentCount">The number of chunks tocreate per segment</param>
        /// <param name="segmentSize">The size of a chunk in bytes</param>
        public BufferManager(int segmentCount, int segmentSize)
        {
            this._segmentCount = segmentCount;
            this._segmentSize = segmentSize;
            this._buffers = new List<ArrayBuffer>();
            this._availableSegments = new LockfreeQueue<BufferSegment>();
            BufferManager.Managers.Add(this);
        }

        /// <summary>
        /// Checks out a segment, creating more if the pool is empty.
        /// </summary>
        /// <returns>a BufferSegment object from the pool</returns>
        public BufferSegment CheckOut()
        {
            BufferSegment bufferSegment;
            if (!this._availableSegments.TryDequeue(out bufferSegment))
            {
                lock (this._buffers)
                {
                    while (!this._availableSegments.TryDequeue(out bufferSegment))
                        this.CreateBuffer();
                }
            }

            bufferSegment.m_uses = 1;
            return bufferSegment;
        }

        /// <summary>
        /// Checks out a segment, and wraps it with a SegmentStream, creating more if the pool is empty.
        /// </summary>
        /// <returns>a SegmentStream object wrapping the BufferSegment taken from the pool</returns>
        public SegmentStream CheckOutStream()
        {
            return new SegmentStream(this.CheckOut());
        }

        /// <summary>Requeues a segment into the buffer pool.</summary>
        /// <param name="segment">the segment to requeue</param>
        public void CheckIn(BufferSegment segment)
        {
            this._availableSegments.Enqueue(segment);
        }

        /// <summary>
        /// Creates a new buffer and adds the segments to the buffer pool.
        /// </summary>
        private void CreateBuffer()
        {
            ArrayBuffer buffer = new ArrayBuffer(this, this._segmentCount * this._segmentSize);
            for (int index = 0; index < this._segmentCount; ++index)
                this._availableSegments.Enqueue(new BufferSegment(buffer, index * this._segmentSize, this._segmentSize,
                    BufferManager._segmentId++));
            this._totalSegmentCount += this._segmentCount;
            this._buffers.Add(buffer);
            Interlocked.Add(ref BufferManager.GlobalAllocatedMemory, (long) (this._segmentCount * this._segmentSize));
        }

        /// <summary>
        /// Returns a BufferSegment that is at least of the given size.
        /// </summary>
        /// <param name="payloadSize"></param>
        /// <returns></returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">In case that the payload exceeds the SegmentSize of the largest buffer available.</exception>
        public static BufferSegment GetSegment(int payloadSize)
        {
            if (payloadSize <= BufferManager.Tiny.SegmentSize)
                return BufferManager.Tiny.CheckOut();
            if (payloadSize <= BufferManager.Small.SegmentSize)
                return BufferManager.Small.CheckOut();
            if (payloadSize <= BufferManager.Default.SegmentSize)
                return BufferManager.Default.CheckOut();
            if (payloadSize <= BufferManager.Large.SegmentSize)
                return BufferManager.Large.CheckOut();
            if (payloadSize <= BufferManager.ExtraLarge.SegmentSize)
                return BufferManager.ExtraLarge.CheckOut();
            throw new ArgumentOutOfRangeException("Required buffer is way too big: " + (object) payloadSize);
        }

        /// <summary>
        /// Returns a SegmentStream that is at least of the given size.
        /// </summary>
        /// <param name="payloadSize"></param>
        /// <returns></returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">In case that the payload exceeds the SegmentSize of the largest buffer available.</exception>
        public static SegmentStream GetSegmentStream(int payloadSize)
        {
            return new SegmentStream(BufferManager.GetSegment(payloadSize));
        }

        ~BufferManager()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize((object) this);
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            BufferSegment bufferSegment;
            do
                ;
            while (this._availableSegments.TryDequeue(out bufferSegment));
            this._buffers.Clear();
        }
    }
}