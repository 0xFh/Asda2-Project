using System;
using WCell.Core.Network;

namespace WCell.RealmServer.UpdateFields
{
    public class UpdateMask
    {
        private int m_maxBlockCount;
        private uint[] m_blocks;
        protected internal int m_lowestIndex;
        protected internal int m_highestIndex;

        public byte[] DataByOne { get; set; }

        public UpdateMask(int highestField)
        {
            this.m_maxBlockCount = (highestField >> 5) + 1;
            this.Clear();
        }

        public UpdateMask(uint[] data)
        {
            this.m_blocks = data;
            this.DataByOne = new byte[data.Length * 4 * 8];
            int maxIndex = this.MaxIndex;
            for (int index = 0; index < maxIndex; ++index)
                this.DataByOne[index] = this.GetBit(index) ? (byte) 1 : (byte) 0;
        }

        public int MaxBlockCount
        {
            get { return this.m_maxBlockCount; }
        }

        public uint[] Blocks
        {
            get { return this.m_blocks; }
        }

        public int LowestIndex
        {
            get { return this.m_lowestIndex; }
        }

        public int HighestIndex
        {
            get { return this.m_highestIndex; }
            set
            {
                this.m_maxBlockCount = (value >> 5) + 1;
                if (this.m_maxBlockCount <= this.m_blocks.Length)
                    return;
                Array.Resize<uint>(ref this.m_blocks, this.m_maxBlockCount);
            }
        }

        public bool HasBitsSet
        {
            get { return this.m_highestIndex >= this.m_lowestIndex; }
        }

        public void Clear()
        {
            this.m_highestIndex = 0;
            this.m_lowestIndex = int.MaxValue;
            this.m_blocks = new uint[this.m_maxBlockCount];
        }

        /// <summary>Writes all the blocks to the packet</summary>
        /// <param name="packet">The packet to write to</param>
        public void WriteFull(PrimitiveWriter packet)
        {
            packet.Write((byte) this.m_maxBlockCount);
            for (int index = 0; index < this.m_maxBlockCount; ++index)
                packet.Write(this.m_blocks[index]);
        }

        public int MaxIndex
        {
            get { return this.m_blocks.Length * 8 * 4; }
        }

        /// <summary>Writes the bit mask of all required fields</summary>
        /// <param name="packet">The packet to write to</param>
        public void WriteTo(PrimitiveWriter packet)
        {
            int num = (this.m_highestIndex >> 5) + 1;
            packet.Write((byte) num);
            for (int index = 0; index < num; ++index)
                packet.Write(this.m_blocks[index]);
        }

        /// <summary>Writes the bit mask of all required fields</summary>
        /// <param name="packet">The packet to write to</param>
        public void WriteToAsda2Packet(PrimitiveWriter packet)
        {
            foreach (uint block in this.m_blocks)
                packet.Write(block);
        }

        public void UnsetBit(int index)
        {
            this.m_blocks[index >> 5] &= (uint) ~(1 << index);
            this.DataByOne[index] = (byte) 0;
        }

        public void SetAll()
        {
            for (int index = 0; index < this.m_maxBlockCount; ++index)
                this.m_blocks[index] = uint.MaxValue;
        }

        public void SetBit(int index)
        {
            this.m_blocks[index >> 5] |= (uint) (1 << index);
            if (index > this.m_highestIndex)
                this.m_highestIndex = index;
            if (index < this.m_lowestIndex)
                this.m_lowestIndex = index;
            if (this.DataByOne == null)
                return;
            this.DataByOne[index] = (byte) 1;
        }

        public bool GetBit(int index)
        {
            return ((int) this.m_blocks[index >> 5] & 1 << index) != 0;
        }
    }
}