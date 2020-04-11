using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WCell.Util.DynamicAccess;

namespace WCell.Util.Toolshed
{
  public class ToolMgr
  {
    public readonly Dictionary<string, IExecutable> Executables =
      new Dictionary<string, IExecutable>(StringComparer.InvariantCultureIgnoreCase);

    public readonly List<IExecutable> ExecutableList = new List<IExecutable>();

    public static void GetAllPublicStaticClassesOfAsm(Assembly asm, Dictionary<string, Type> map)
    {
      foreach(Type type in asm.GetTypes())
      {
        if(type.IsStatic())
        {
          object[] customAttributes = type.GetCustomAttributes(true);
          ToolAttribute toolAttribute = (ToolAttribute) customAttributes
            .Where(attr => attr is ToolAttribute).First();
          if(toolAttribute != null || customAttributes
               .Where(attr => attr is NoToolAttribute).Count() <= 0)
          {
            string key = toolAttribute != null ? toolAttribute.Name : type.Name;
            if(map.ContainsKey(key))
              throw new Exception(string.Format(
                "Invalid Type name of static Tool class \"{0}\", used by {1} AND {2}", key,
                map[type.Name].FullName, type.FullName));
            map.Add(key, type);
          }
        }
      }
    }

    public void AddStaticMethodsOfAsm(Assembly asm)
    {
      foreach(Type type in asm.GetTypes())
      {
        HashSet<string> stringSet = new HashSet<string>();
        object[] customAttributes = type.GetCustomAttributes(true);
        bool flag1 = customAttributes
                       .Where(attr => attr is ToolAttribute).Count() > 0;
        if(flag1 || customAttributes
             .Where(attr => attr is NoToolAttribute).Count() <= 0)
        {
          foreach(MethodInfo method in type.GetMethods(
            BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod))
          {
            string name = method.Name;
            if(!name.StartsWith("get_") && !name.StartsWith("set_") &&
               method.GetCustomAttributes<NoToolAttribute>().Length <= 0)
            {
              ToolAttribute toolAttribute =
                method.GetCustomAttributes<ToolAttribute>()
                  .FirstOrDefault();
              if(toolAttribute != null)
                name = toolAttribute.Name ?? name;
              else if(!flag1)
                continue;
              bool flag2 = true;
              foreach(ParameterInfo parameter in method.GetParameters())
              {
                if(!parameter.ParameterType.IsSimpleType())
                {
                  flag2 = false;
                  break;
                }
              }

              if(flag2)
              {
                if(!stringSet.Contains(name))
                {
                  stringSet.Add(name);
                  Add(name, null, method);
                }
                else if(toolAttribute != null)
                  throw new ToolException(
                    "Found multiple static methods with ToolAttribute, called: {0}.- Make sure that the names are unique.",
                    (object) method.GetFullMemberName());
              }
              else if(toolAttribute != null)
                throw new ToolException(
                  "Static method {0} was marked with ToolAttribute but had non-simple Parameters. - Make sure to only give methods with simple parameters the ToolAttribute. You can exclude them with the NoToolAttribute.",
                  (object) method.GetFullMemberName());
            }
          }
        }
      }
    }

    public void Add(IExecutable executable)
    {
      EnsureUniqueName(executable.Name);
      Executables.Add(executable.Name, executable);
      ExecutableList.Add(executable);
    }

    public void Add(string name, object targetObj, MethodInfo method)
    {
      Add(new MethodExcecutable(name, targetObj, method));
    }

    public IExecutable Get(string name)
    {
      IExecutable executable;
      Executables.TryGetValue(name, out executable);
      return executable;
    }

    public bool Execute(string name, params object[] args)
    {
      IExecutable executable;
      if(!Executables.TryGetValue(name, out executable))
        return false;
      executable.Exec(args);
      return true;
    }

    private void EnsureUniqueName(string name)
    {
      if(Executables.ContainsKey(name))
        throw new ToolException(
          "Tried to add two Executables with same name (\"" + name +
          "\") to ToolMgr. - Make sure to use unique names.");
    }
  }
}