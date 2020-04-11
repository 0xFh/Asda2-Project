using System;
using System.IO;

namespace WCell.Core.Network
{
    public static class IOExtensions
    {
        public static int WritePackedUInt64(this BinaryWriter binWriter, ulong number)
        {
            byte[] bytes = BitConverter.GetBytes(number);
            byte num = 0;
            long position1 = binWriter.BaseStream.Position;
            binWriter.Write(num);
            for (int index = 0; index < 8; ++index)
            {
                if (bytes[index] != (byte) 0)
                {
                    num |= (byte) (1 << index);
                    binWriter.Write(bytes[index]);
                }
            }

            long position2 = binWriter.BaseStream.Position;
            binWriter.BaseStream.Position = position1;
            binWriter.Write(num);
            binWriter.BaseStream.Position = position2;
            return (int) (position2 - position1);
        }
    }
}