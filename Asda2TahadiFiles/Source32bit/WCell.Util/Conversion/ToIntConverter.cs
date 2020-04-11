using System;

namespace WCell.Util.Conversion
{
  public class ToIntConverter : IConverter
  {
    public object Convert(object input)
    {
      if(input is DBNull)
        return 0;
      if(input is uint)
        return (int) (uint) input;
      if(input is long)
        return (int) (long) input;
      if(input is byte)
        return (int) (byte) input;
      if(input is ushort)
        return (int) (ushort) input;
      if(input is ulong)
        return (int) (ulong) input;
      if(!(input is string))
        return input;
      long result;
      if(long.TryParse((string) input, out result))
        return (int) result;
      throw new Exception("Could not convert value to Int: " + input);
    }
  }
}