using System;

namespace WCell.Core.Initialization
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DependentInitializationAttribute : Attribute
    {
        public string Name { get; set; }

        public DependentInitializationAttribute(Type dependentType)
            : this(dependentType, "")
        {
        }

        public DependentInitializationAttribute(Type dependentType, string name)
        {
            this.DependentType = dependentType;
            this.Name = name;
        }

        public Type DependentType { get; set; }
    }
}