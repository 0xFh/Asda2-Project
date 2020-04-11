namespace WCell.Util.Graphics
{
    public static class HalfUtils
    {
        private const uint BiasDiffo = 3355443200;
        private const int cExpBias = 15;
        private const int cExpBits = 5;
        private const int cFracBits = 10;
        private const int cFracBitsDiff = 13;
        private const uint cFracMask = 1023;
        private const uint cRoundBit = 4096;
        private const int cSignBit = 15;
        private const uint cSignMask = 32768;
        private const uint eMax = 16;
        private const int eMin = -14;
        private const uint wMaxNormal = 1207955455;
        private const uint wMinNormal = 947912704;

        public static unsafe ushort Pack(float value)
        {
            uint num1 = *(uint*) &value;
            uint num2 = (uint) (((long) num1 & (long) int.MinValue) >> 16);
            uint num3 = num1 & (uint) int.MaxValue;
            if (num3 > 1207955455U)
                return (ushort) (num2 | (uint) short.MaxValue);
            if (num3 >= 947912704U)
                return (ushort) ((ulong) num2 |
                                 (ulong) ((long) num3 + -939524096L + 4095L + (long) (num3 >> 13 & 1U) >> 13));
            uint num4 = (uint) ((int) num3 & 8388607 | 8388608);
            int num5 = 113 - (int) (num3 >> 23);
            uint num6 = num5 > 31 ? 0U : num4 >> num5;
            return (ushort) (num2 | (uint) ((int) num6 + 4095 + ((int) (num6 >> 13) & 1)) >> 13);
        }

        public static unsafe float Unpack(ushort value)
        {
            uint num1;
            if (((int) value & -33792) == 0)
            {
                if (((int) value & 1023) != 0)
                {
                    uint num2 = 4294967282;
                    uint num3 = (uint) value & 1023U;
                    while (((int) num3 & 1024) == 0)
                    {
                        --num2;
                        num3 <<= 1;
                    }

                    uint num4 = num3 & 4294966271U;
                    num1 = (uint) (((int) value & 32768) << 16 | (int) num2 + (int) sbyte.MaxValue << 23 |
                                   (int) num4 << 13);
                }
                else
                    num1 = (uint) (((int) value & 32768) << 16);
            }
            else
                num1 = (uint) (((int) value & 32768) << 16 |
                               ((int) value >> 10 & 31) - 15 + (int) sbyte.MaxValue << 23 | ((int) value & 1023) << 13);

            return *(float*) &num1;
        }
    }
}