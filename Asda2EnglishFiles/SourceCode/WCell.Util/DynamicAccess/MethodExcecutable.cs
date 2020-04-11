using System;
using System.Collections.Generic;
using System.Reflection;

namespace WCell.Util.DynamicAccess
{
    public class MethodExcecutable : IExecutable
    {
        private string m_name;
        private readonly object m_TargetObj;
        private readonly MethodInfo m_method;
        private readonly Type[] m_parameterTypes;

        public MethodExcecutable(string name, object targetObj, MethodInfo method)
        {
            if (method.IsStatic && targetObj != null)
                throw new ArgumentException("Invalid Executable - Static method \"" + method.Name +
                                            "\" cannot have a targetObj (\"" + targetObj + "\")");
            if (!method.IsStatic && targetObj == null)
                throw new ArgumentException("Invalid Executable - Instance method \"" + method.Name +
                                            "\" must have a targetObj (null).");
            this.m_name = name;
            this.m_TargetObj = targetObj;
            this.m_method = method;
            this.m_parameterTypes =
                ((IEnumerable<ParameterInfo>) this.m_method.GetParameters()).TransformArray<ParameterInfo, Type>(
                    (Func<ParameterInfo, Type>) (info => info.ParameterType));
        }

        public MethodInfo Method
        {
            get { return this.m_method; }
        }

        public string Name
        {
            get { return this.m_name; }
            set { this.m_name = value; }
        }

        public Type[] ParameterTypes
        {
            get { return this.m_parameterTypes; }
        }

        public void Exec(params object[] args)
        {
            this.m_method.Invoke(this.m_TargetObj, args);
        }

        public override string ToString()
        {
            return string.Format("Method {0}.{1}({2})", (object) this.m_method.DeclaringType.Name, (object) this.m_name,
                (object) ((IEnumerable<ParameterInfo>) this.Method.GetParameters()).ToString<ParameterInfo>(", "));
        }
    }
}