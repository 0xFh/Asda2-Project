using System;

namespace WCell.Util.Conversion
{
    public class ToIntConverter : IConverter
    {
        public object Convert(object input)
        {
            if (input is DBNull)
                return (object) 0;
            if (input is uint)
                return (object) (int) (uint) input;
            if (input is long)
                return (object) (int) (long) input;
            if (input is byte)
                return (object) (int) (byte) input;
            if (input is ushort)
                return (object) (int) (ushort) input;
            if (input is ulong)
                return (object) (int) (ulong) input;
            if (!(input is string))
                return input;
            long result;
            if (long.TryParse((string) input, out result))
                return (object) (int) result;
            throw new Exception("Could not convert value to Int: " + input);
        }
    }
}