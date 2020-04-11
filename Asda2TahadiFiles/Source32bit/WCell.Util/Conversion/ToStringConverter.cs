namespace WCell.Util.Conversion
{
  public class ToStringConverter : IConverter
  {
    public object Convert(object input)
    {
      if(input == null)
        return "";
      return input.ToString();
    }
  }
}