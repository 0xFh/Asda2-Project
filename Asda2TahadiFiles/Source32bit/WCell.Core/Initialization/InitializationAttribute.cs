using System;

namespace WCell.Core.Initialization
{
  [AttributeUsage(AttributeTargets.Method)]
  public class InitializationAttribute : Attribute, IInitializationInfo
  {
    public InitializationAttribute()
    {
      IsRequired = true;
      Name = "";
      Pass = InitializationPass.Any;
    }

    public InitializationAttribute(string name)
    {
      IsRequired = true;
      Name = name;
      Pass = InitializationPass.Any;
    }

    public InitializationAttribute(InitializationPass pass)
    {
      IsRequired = true;
      Name = "";
      Pass = pass;
    }

    public InitializationAttribute(InitializationPass pass, string name)
    {
      IsRequired = true;
      Pass = pass;
      Name = name;
    }

    public InitializationPass Pass { get; private set; }

    public string Name { get; set; }

    public bool IsRequired { get; set; }
  }
}