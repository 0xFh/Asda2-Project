using System;
using System.Runtime.InteropServices;

namespace WCell.RealmServer.UpdateFields
{
  [Serializable]
  [StructLayout(LayoutKind.Explicit)]
  public struct CompoundType
  {
    [FieldOffset(0)]public byte Byte1;
    [FieldOffset(1)]public byte Byte2;
    [FieldOffset(2)]public byte Byte3;
    [FieldOffset(3)]public byte Byte4;
    [FieldOffset(0)]public float Float;
    [FieldOffset(0)]public short Int16Low;
    [FieldOffset(2)]public short Int16High;
    [FieldOffset(0)]public int Int32;
    [FieldOffset(0)]public ushort UInt16Low;
    [FieldOffset(2)]public ushort UInt16High;
    [FieldOffset(0)]public uint UInt32;

    public unsafe byte[] ByteArray
    {
      get
      {
        byte[] numArray = new byte[4];
        fixed(byte* numPtr = numArray)
          *(int*) numPtr = (int) UInt32;
        return numArray;
      }
      set
      {
        fixed(byte* numPtr = &value[0])
          UInt32 = *(uint*) numPtr;
      }
    }

    public void SetByte(int index, byte value)
    {
      UInt32 &= (uint) ~(byte.MaxValue << index * 8);
      UInt32 |= (uint) value << index * 8;
    }

    public unsafe void SetByteUnsafe(int index, byte value)
    {
      *(sbyte*) ((IntPtr) UInt32 + index) = (sbyte) value;
    }

    public byte GetByte(int index)
    {
      return (byte) (UInt32 >> index * 8);
    }

    public unsafe byte GetByteUnsafe(int index)
    {
      return *(byte*) ((IntPtr) UInt32 + index);
    }

    public override bool Equals(object obj)
    {
      if(obj != null && obj is CompoundType)
        return Equals((CompoundType) obj);
      return false;
    }

    private bool Equals(CompoundType obj)
    {
      return (int) UInt32 == (int) obj.UInt32;
    }

    public override int GetHashCode()
    {
      return Int32;
    }

    public static bool operator ==(CompoundType lhs, CompoundType rhs)
    {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(CompoundType lhs, CompoundType rhs)
    {
      return !lhs.Equals(rhs);
    }
  }
}