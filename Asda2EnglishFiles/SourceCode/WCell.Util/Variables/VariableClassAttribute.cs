using System;

namespace WCell.Util.Variables
{
    [AttributeUsage(AttributeTargets.Class)]
    public class VariableClassAttribute : Attribute
    {
        public bool Inherit;

        public VariableClassAttribute(bool inherit)
        {
            this.Inherit = inherit;
        }
    }
}