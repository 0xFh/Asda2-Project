using System;
using System.Runtime.InteropServices;

namespace WCell.RealmServer.UpdateFields
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct CompoundType
    {
        [FieldOffset(0)] public byte Byte1;
        [FieldOffset(1)] public byte Byte2;
        [FieldOffset(2)] public byte Byte3;
        [FieldOffset(3)] public byte Byte4;
        [FieldOffset(0)] public float Float;
        [FieldOffset(0)] public short Int16Low;
        [FieldOffset(2)] public short Int16High;
        [FieldOffset(0)] public int Int32;
        [FieldOffset(0)] public ushort UInt16Low;
        [FieldOffset(2)] public ushort UInt16High;
        [FieldOffset(0)] public uint UInt32;

        public unsafe byte[] ByteArray
        {
            get
            {
                byte[] numArray = new byte[4];
                fixed (byte* numPtr = numArray)
                    *(int*) numPtr = (int) this.UInt32;
                return numArray;
            }
            set
            {
                fixed (byte* numPtr = &value[0])
                    this.UInt32 = *(uint*) numPtr;
            }
        }

        public void SetByte(int index, byte value)
        {
            this.UInt32 &= (uint) ~((int) byte.MaxValue << index * 8);
            this.UInt32 |= (uint) value << index * 8;
        }

        public unsafe void SetByteUnsafe(int index, byte value)
        {
            *(sbyte*) ((IntPtr) this.UInt32 + index) = (sbyte) value;
        }

        public byte GetByte(int index)
        {
            return (byte) (this.UInt32 >> index * 8);
        }

        public unsafe byte GetByteUnsafe(int index)
        {
            return *(byte*) ((IntPtr) this.UInt32 + index);
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is CompoundType)
                return this.Equals((CompoundType) obj);
            return false;
        }

        private bool Equals(CompoundType obj)
        {
            return (int) this.UInt32 == (int) obj.UInt32;
        }

        public override int GetHashCode()
        {
            return this.Int32;
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