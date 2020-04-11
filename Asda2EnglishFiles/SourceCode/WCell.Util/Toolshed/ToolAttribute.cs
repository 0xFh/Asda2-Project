using System;

namespace WCell.Util.Toolshed
{
    public class ToolAttribute : Attribute
    {
        public string Name;

        public ToolAttribute()
            : this((string) null)
        {
        }

        public ToolAttribute(string name)
        {
            this.Name = name;
        }
    }
}