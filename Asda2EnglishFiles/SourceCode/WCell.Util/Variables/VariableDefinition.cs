using System;
using System.Xml.Serialization;

namespace WCell.Util.Variables
{
    public class VariableDefinition
    {
        public static readonly VariableDefinition[] EmptyArray = new VariableDefinition[0];

        public VariableDefinition()
        {
        }

        public VariableDefinition(string name)
        {
            this.Name = name;
        }

        [XmlAttribute] public string Name { get; set; }

        [XmlAttribute("Value")] public string StringValue { get; set; }

        /// <summary>Copies the actual Value</summary>
        public object Eval(Type type)
        {
            if (this.Name == null)
                throw new Exception("Variable's Name was not set - Value: " + this.StringValue);
            if (this.StringValue == null)
                throw new Exception("Variable's StringValue was not set - Name: " + this.Name);
            object obj = (object) null;
            if (!StringParser.Parse(this.StringValue, type, ref obj))
                throw new Exception(string.Format("Unable to parse Variable Value \"{0}\" as Type \"{1}\"",
                    (object) this.StringValue, (object) type.Name));
            return obj;
        }

        public override string ToString()
        {
            return this.Name + " (Value: " + this.StringValue + ")";
        }
    }
}