using System;

namespace WCell.Core.Initialization
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class InitializationAttribute : Attribute, IInitializationInfo
    {
        public InitializationAttribute()
        {
            this.IsRequired = true;
            this.Name = "";
            this.Pass = InitializationPass.Any;
        }

        public InitializationAttribute(string name)
        {
            this.IsRequired = true;
            this.Name = name;
            this.Pass = InitializationPass.Any;
        }

        public InitializationAttribute(InitializationPass pass)
        {
            this.IsRequired = true;
            this.Name = "";
            this.Pass = pass;
        }

        public InitializationAttribute(InitializationPass pass, string name)
        {
            this.IsRequired = true;
            this.Pass = pass;
            this.Name = name;
        }

        public InitializationPass Pass { get; private set; }

        public string Name { get; set; }

        public bool IsRequired { get; set; }
    }
}