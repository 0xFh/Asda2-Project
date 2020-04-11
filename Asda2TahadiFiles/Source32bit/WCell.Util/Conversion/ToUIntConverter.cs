using System;

namespace WCell.Util.Conversion
{
  public class ToUIntConverter : IConverter
  {
    public virtual object Convert(object input)
    {
      if(input is DBNull)
        return 0U;
      if(input is uint)
        return input;
      if(input is int)
        return (uint) (int) input;
      if(input is sbyte)
        return (uint) (sbyte) input;
      if(input is short)
        return (uint) (short) input;
      if(input is long)
        return (uint) (long) input;
      if(input is byte)
        return (uint) (byte) input;
      if(input is ushort)
        return (uint) (ushort) input;
      if(input is ulong)
        return (uint) (ulong) input;
      long result;
      if(input is string && long.TryParse((string) input, out result))
        return (uint) result;
      throw new Exception("Could not convert value to UInt: " + input);
    }
  }
}