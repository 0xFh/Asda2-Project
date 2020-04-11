using System;

namespace WCell.Util.Variables
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
  public class VariableAttribute : Attribute
  {
    public bool Serialized = true;
    public const bool DefaultSerialized = true;
    public string Name;
    public bool IsReadOnly;

    /// <summary>
    /// If set to false, cannot get or set this variable through any command
    /// </summary>
    public bool IsFileOnly;

    public VariableAttribute()
    {
    }

    public VariableAttribute(string name)
    {
      Name = name;
    }

    public VariableAttribute(bool serialized)
    {
      Serialized = serialized;
    }

    public VariableAttribute(string name, bool serialized)
    {
      Name = name;
      Serialized = serialized;
    }

    public VariableAttribute(string name, bool serialized, bool readOnly)
    {
      Name = name;
      Serialized = serialized;
      IsReadOnly = readOnly;
    }
  }
}