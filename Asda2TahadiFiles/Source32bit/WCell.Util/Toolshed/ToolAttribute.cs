using System;

namespace WCell.Util.Toolshed
{
  public class ToolAttribute : Attribute
  {
    public string Name;

    public ToolAttribute()
      : this(null)
    {
    }

    public ToolAttribute(string name)
    {
      Name = name;
    }
  }
}