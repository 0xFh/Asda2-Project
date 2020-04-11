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
      m_maxBlockCount = (highestField >> 5) + 1;
      Clear();
    }

    public UpdateMask(uint[] data)
    {
      m_blocks = data;
      DataByOne = new byte[data.Length * 4 * 8];
      int maxIndex = MaxIndex;
      for(int index = 0; index < maxIndex; ++index)
        DataByOne[index] = GetBit(index) ? (byte) 1 : (byte) 0;
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
      get { return m_highestIndex; }
      set
      {
        m_maxBlockCount = (value >> 5) + 1;
        if(m_maxBlockCount <= m_blocks.Length)
          return;
        Array.Resize(ref m_blocks, m_maxBlockCount);
      }
    }

    public bool HasBitsSet
    {
      get { return m_highestIndex >= m_lowestIndex; }
    }

    public void Clear()
    {
      m_highestIndex = 0;
      m_lowestIndex = int.MaxValue;
      m_blocks = new uint[m_maxBlockCount];
    }

    /// <summary>Writes all the blocks to the packet</summary>
    /// <param name="packet">The packet to write to</param>
    public void WriteFull(PrimitiveWriter packet)
    {
      packet.Write((byte) m_maxBlockCount);
      for(int index = 0; index < m_maxBlockCount; ++index)
        packet.Write(m_blocks[index]);
    }

    public int MaxIndex
    {
      get { return m_blocks.Length * 8 * 4; }
    }

    /// <summary>Writes the bit mask of all required fields</summary>
    /// <param name="packet">The packet to write to</param>
    public void WriteTo(PrimitiveWriter packet)
    {
      int num = (m_highestIndex >> 5) + 1;
      packet.Write((byte) num);
      for(int index = 0; index < num; ++index)
        packet.Write(m_blocks[index]);
    }

    /// <summary>Writes the bit mask of all required fields</summary>
    /// <param name="packet">The packet to write to</param>
    public void WriteToAsda2Packet(PrimitiveWriter packet)
    {
      foreach(uint block in m_blocks)
        packet.Write(block);
    }

    public void UnsetBit(int index)
    {
      m_blocks[index >> 5] &= (uint) ~(1 << index);
      DataByOne[index] = 0;
    }

    public void SetAll()
    {
      for(int index = 0; index < m_maxBlockCount; ++index)
        m_blocks[index] = uint.MaxValue;
    }

    public void SetBit(int index)
    {
      m_blocks[index >> 5] |= (uint) (1 << index);
      if(index > m_highestIndex)
        m_highestIndex = index;
      if(index < m_lowestIndex)
        m_lowestIndex = index;
      if(DataByOne == null)
        return;
      DataByOne[index] = 1;
    }

    public bool GetBit(int index)
    {
      return ((int) m_blocks[index >> 5] & 1 << index) != 0;
    }
  }
}