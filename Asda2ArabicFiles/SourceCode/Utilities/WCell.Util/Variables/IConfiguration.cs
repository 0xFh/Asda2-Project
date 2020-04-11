using System;

namespace WCell.Util.Variables
{
    public interface IConfiguration
    {
        bool Load();

        void Save(bool backup = true, bool auto = true);

        bool Contains(string name);

        bool IsReadOnly(string name);

        object Get(string name);

        bool Set(string name, string value);

        bool Set(string name, object value);

        void Foreach(Action<IVariableDefinition> callback);
    }
}