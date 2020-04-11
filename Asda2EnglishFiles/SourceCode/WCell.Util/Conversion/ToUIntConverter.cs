using System;

namespace WCell.Util.Conversion
{
    public class ToUIntConverter : IConverter
    {
        public virtual object Convert(object input)
        {
            if (input is DBNull)
                return (object) 0U;
            if (input is uint)
                return input;
            if (input is int)
                return (object) (uint) (int) input;
            if (input is sbyte)
                return (object) (uint) (sbyte) input;
            if (input is short)
                return (object) (uint) (short) input;
            if (input is long)
                return (object) (uint) (long) input;
            if (input is byte)
                return (object) (uint) (byte) input;
            if (input is ushort)
                return (object) (uint) (ushort) input;
            if (input is ulong)
                return (object) (uint) (ulong) input;
            long result;
            if (input is string && long.TryParse((string) input, out result))
                return (object) (uint) result;
            throw new Exception("Could not convert value to UInt: " + input);
        }
    }
}