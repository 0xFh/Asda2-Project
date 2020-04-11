using System;

namespace WCell.Util.Variables
{
    public abstract class VariableConfiguration<C, V> : VariableConfiguration<V> where C : VariableConfiguration<V>
        where V : TypeVariableDefinition, new()
    {
        protected VariableConfiguration()
        {
        }

        protected VariableConfiguration(Action<string> onError)
            : base(onError)
        {
        }
    }
}