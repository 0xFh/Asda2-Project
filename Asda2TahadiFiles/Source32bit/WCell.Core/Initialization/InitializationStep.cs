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
      : this(pass, initStepName, isRequired, null, initMethod)
    {
    }

    public InitializationStep(InitializationPass pass, string initStepName, bool isRequired, object[] initContext,
      MethodInfo initMethod)
    {
      Pass = pass;
      InitStepName = initStepName;
      IsRequired = isRequired;
      InitContext = initContext;
      InitMethod = initMethod;
    }

    public object[] GetArgs(InitMgr mgr)
    {
      ParameterInfo[] parameters = InitMethod.GetParameters();
      object[] objArray = null;
      if(parameters.Length == 1 && parameters[0].ParameterType == typeof(InitMgr))
        objArray = new object[1] { mgr };
      return objArray;
    }

    public override string ToString()
    {
      return !string.IsNullOrEmpty(InitStepName)
        ? InitStepName
        : InitMethod.DeclaringType.FullName + "." + InitMethod.Name;
    }
  }
}