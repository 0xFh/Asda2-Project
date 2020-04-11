using System;
using System.Net;
using System.Net.Sockets;

namespace Cell.Core
{
    public static class ByteArrHelper
    {
        /// <summary>
        /// Sets the given bytes in the given array at the given index
        /// </summary>
        public static void SetBytes(this byte[] arr, uint index, byte[] bytes)
        {
            for (int index1 = 0; index1 < bytes.Length; ++index1)
                arr[(long) index + (long) index1] = bytes[index1];
        }

        /// <summary>
        /// Sets the ushort in opposite byte-order in the given array at the given index
        /// </summary>
        public static void SetUShortBE(this byte[] arr, uint index, ushort val)
        {
            arr[index] = (byte) (((int) val & 65280) >> 8);
            arr[index + 1U] = (byte) ((uint) val & (uint) byte.MaxValue);
        }

        public static unsafe ushort GetUInt16(this byte[] data, uint field)
        {
            uint num = field * 4U;
            if ((long) (num + 2U) > (long) data.Length)
                return ushort.MaxValue;
            fixed (byte* numPtr = &data[num])
                return *(ushort*) numPtr;
        }

        public static unsafe ushort GetUInt16AtByte(this byte[] data, uint startIndex)
        {
            if ((long) (startIndex + 1U) >= (long) data.Length)
                return ushort.MaxValue;
            fixed (byte* numPtr = &data[startIndex])
                return *(ushort*) numPtr;
        }

        public static unsafe uint GetUInt32(this byte[] data, uint field)
        {
            uint num = field * 4U;
            if ((long) (num + 4U) > (long) data.Length)
                return uint.MaxValue;
            fixed (byte* numPtr = &data[num])
                return *(uint*) numPtr;
        }

        public static unsafe int GetInt32(this byte[] data, uint field)
        {
            uint num = field * 4U;
            if ((long) (num + 4U) > (long) data.Length)
                return int.MaxValue;
            fixed (byte* numPtr = &data[num])
                return *(int*) numPtr;
        }

        public static unsafe float GetFloat(this byte[] data, uint field)
        {
            uint num = field * 4U;
            if ((long) (num + 4U) > (long) data.Length)
                return float.NaN;
            fixed (byte* numPtr = &data[num])
                return *(float*) numPtr;
        }

        public static unsafe ulong GetUInt64(this byte[] data, uint startingField)
        {
            uint num = startingField * 4U;
            if ((long) (num + 8U) > (long) data.Length)
                return ulong.MaxValue;
            fixed (byte* numPtr = &data[num])
                return (ulong) *(long*) numPtr;
        }

        public static byte[] GetBytes(this byte[] data, uint startingField, int amount)
        {
            byte[] numArray = new byte[amount];
            uint num = startingField * 4U;
            if ((long) num + (long) amount > (long) data.Length)
                return numArray;
            for (int index = 0; index < amount; ++index)
                numArray[index] = data[(long) num + (long) index];
            return numArray;
        }

        public static bool AreAllZero(this byte[] data)
        {
            foreach (byte num in data)
            {
                if (num != (byte) 0)
                    return false;
            }

            return true;
        }

        public static bool IsIPV4(this IPAddress addr)
        {
            return addr.AddressFamily == AddressFamily.InterNetwork;
        }

        public static bool IsIPV6(this IPAddress addr)
        {
            return addr.AddressFamily == AddressFamily.InterNetworkV6;
        }

        public static int GetLength(this IPAddress addr)
        {
            return addr.GetAddressBytes().Length;
        }
    }
}