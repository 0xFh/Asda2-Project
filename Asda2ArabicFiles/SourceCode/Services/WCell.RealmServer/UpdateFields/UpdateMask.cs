/*************************************************************************
 *
 *   file		: UpdateMask.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-16 05:30:51 +0100 (ma, 16 feb 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 757 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Core.Network;

namespace WCell.RealmServer.UpdateFields
{
    public class UpdateMask
    {
        public byte[] DataByOne { get; set; }
        private int m_maxBlockCount;
		private uint[] m_blocks;

		protected internal int m_lowestIndex;
		protected internal int m_highestIndex;

        public UpdateMask(int highestField)
		{
			//m_maxBlockCount = (highestField + 31) >> 5;
			m_maxBlockCount = (highestField >> 5) + 1;
			Clear();
        }

        public UpdateMask(uint[] data)
        {
            m_blocks = data;
            DataByOne = new byte[data.Length*4*8];
            var len = MaxIndex;
            for (int i = 0; i < len; i++)
            {
                DataByOne[i] = (byte) (GetBit(i)?1:0);
            }
        }

        public int MaxBlockCount
        {
            get { return m_maxBlockCount; }
        }

        public uint[] Blocks
        {
            get { return m_blocks; }
        }

    	public int LowestIndex
    	{
			get { return m_lowestIndex; }
    	}

		public int HighestIndex
		{
			get
			{
				return m_highestIndex;
			}
			set
			{
				m_maxBlockCount = (value >> 5) + 1;

				if (m_maxBlockCount > m_blocks.Length)
				{
					Array.Resize(ref m_blocks, m_maxBlockCount);
				}

			}
		}

		public bool HasBitsSet
		{
			get
			{
				return m_highestIndex >= m_lowestIndex;
			}
		}

        public void Clear()
		{
			m_highestIndex = 0;
			m_lowestIndex = int.MaxValue;
            m_blocks = new uint[m_maxBlockCount];
        }

        /// <summary>
        /// Writes all the blocks to the packet
        /// </summary>
        /// <param name="packet">The packet to write to</param>
        public void WriteFull(PrimitiveWriter packet)
        {
            packet.Write((byte)m_maxBlockCount);

            for (int i = 0; i < m_maxBlockCount; i++)
            {
                packet.Write(m_blocks[i]);
            }
        }
        public int MaxIndex { get { return m_blocks.Length*8*4; } }
        /// <summary>
        /// Writes the bit mask of all required fields
        /// </summary>
		/// <param name="packet">The packet to write to</param>
		public void WriteTo(PrimitiveWriter packet)
		{
			//var valueCount = (m_highestIndex + 31) >> 5;
			var valueCount = (m_highestIndex >> 5) + 1;

			packet.Write((byte)valueCount);
			for (var i = 0; i < valueCount; i++)
			{
				packet.Write(m_blocks[i]);
			}
		}
        /// <summary>
        /// Writes the bit mask of all required fields
        /// </summary>
        /// <param name="packet">The packet to write to</param>
        public void WriteToAsda2Packet(PrimitiveWriter packet)
        {
            foreach (uint t in m_blocks)
            {
                packet.Write(t);
            }
        }

        public void UnsetBit(int index)
        {
            m_blocks[index >> 5] &= ~(uint)(1 << (index & 31));
            DataByOne[index] = 0;
        }

        public void SetAll()
        {
            for (int i = 0; i < m_maxBlockCount; i++)
            {
                m_blocks[i] = uint.MaxValue;
            }
        }

        public void SetBit(int index)
        {
            m_blocks[index >> 5] |= (uint)(1 << (index & 31));
			if (index > m_highestIndex)
			{
				m_highestIndex = index;
			}
			if (index < m_lowestIndex)
			{
				m_lowestIndex = index;
			}
            if(DataByOne!=null)
                DataByOne[index] = 1;
        }

        public bool GetBit(int index)
        {
            return (m_blocks[index >> 5] & (uint)(1 << (index & 31))) != 0;
        }
    }
}