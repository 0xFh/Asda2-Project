using System.Reflection;

namespace WCell.Core.Initialization
{
    public class InitializationStep
    {
        public readonly string InitStepName = "";
        public readonly InitializationPass Pass;
        public readonly bool IsRequired;
        public object[] InitContext;
        public readonly MethodInfo InitMethod;
        internal bool Executed;

        public InitializationStep(InitializationPass pass, string initStepName, bool isRequired, MethodInfo initMethod)
            : this(pass, initStepName, isRequired, (object[]) null, initMethod)
        {
        }

        public InitializationStep(InitializationPass pass, string initStepName, bool isRequired, object[] initContext,
            MethodInfo initMethod)
        {
            this.Pass = pass;
            this.InitStepName = initStepName;
            this.IsRequired = isRequired;
            this.InitContext = initContext;
            this.InitMethod = initMethod;
        }

        public object[] GetArgs(InitMgr mgr)
        {
            ParameterInfo[] parameters = this.InitMethod.GetParameters();
            object[] objArray = (object[]) null;
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(InitMgr))
                objArray = new object[1] {(object) mgr};
            return objArray;
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(this.InitStepName)
                ? this.InitStepName
                : this.InitMethod.DeclaringType.FullName + "." + this.InitMethod.Name;
        }
    }
}