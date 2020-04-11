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
            this.Name = name;
        }

        public VariableAttribute(bool serialized)
        {
            this.Serialized = serialized;
        }

        public VariableAttribute(string name, bool serialized)
        {
            this.Name = name;
            this.Serialized = serialized;
        }

        public VariableAttribute(string name, bool serialized, bool readOnly)
        {
            this.Name = name;
            this.Serialized = serialized;
            this.IsReadOnly = readOnly;
        }
    }
}