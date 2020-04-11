using System;
using System.Reflection;

namespace WCell.Util.ReflectionUtil
{
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="U">The type of the User objects</typeparam>
  public abstract class MemberAccessor<U>
  {
    /// <summary>
    /// Returns all members of an object with the given name (if character can use it)
    /// </summary>
    public MemberInfo[] GetMembers(U user, object obj, string accessName, ref object propHolder)
    {
      return GetMembers(user, obj, accessName, obj?.GetType(), ref propHolder);
    }

    /// <summary>
    /// Returns all members of an object with the given name (if character can access all holders in the chain)
    /// </summary>
    public MemberInfo[] GetMembers(U user, object obj, string accessName, Type type, ref object memberHolder)
    {
      memberHolder = null;
      bool flag = accessName.StartsWith("#");
      if(type == null && !flag)
        return null;
      object obj1 = obj;
      MemberInfo member = null;
      MemberInfo[] memberInfoArray = null;
      int index = 0;
      string[] strArray1;
      if(flag)
      {
        string[] strArray2 = accessName.Split(new char[1]
        {
          '#'
        }, StringSplitOptions.RemoveEmptyEntries);
        if(strArray2.Length != 2)
          return null;
        string name = strArray2[0];
        if(name.StartsWith("."))
          name = "WCell.RealmServer" + name;
        accessName = strArray2[1];
        strArray1 = accessName.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
          type = assembly.GetType(name, false, true);
          if(type != null)
            break;
        }

        if(type == null)
          return null;
      }
      else
        strArray1 = accessName.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);

      while(index < strArray1.Length)
      {
        string name = strArray1[index];
        ++index;
        BindingFlags bindingAttr = (BindingFlags) (17 | (obj1 != null ? 4 : 8));
        memberInfoArray = type.GetMember(name, MemberTypes.Field | MemberTypes.Method | MemberTypes.Property,
          bindingAttr);
        if(memberInfoArray == null || memberInfoArray.Length == 0 ||
           user != null && !CanRead(member, user))
          return null;
        if(index < strArray1.Length)
        {
          member = memberInfoArray[0];
          obj1 = !(member is PropertyInfo)
            ? (!(member is FieldInfo)
              ? ((MethodBase) member).Invoke(obj1, null)
              : ((FieldInfo) member).GetValue(obj1))
            : ((PropertyInfo) member).GetValue(obj1, null);
          type = obj1.GetType();
        }
      }

      memberHolder = obj1;
      return memberInfoArray;
    }

    public MemberInfo GetProp(U user, object obj, string name, Type type, out object propHolder)
    {
      propHolder = null;
      MemberInfo[] members = GetMembers(user, obj, name, type, ref propHolder);
      if(members != null && members.Length > 0)
      {
        MemberInfo member = members[0];
        if(!member.IsReadonly() && (user == null || CanWrite(member, user)))
          return member;
      }

      return null;
    }

    /// <summary>Sets a property on the given object.</summary>
    public MemberInfo SetPropValue(U user, object obj, string name, string value)
    {
      return SetPropValue(user, obj, name, value, obj.GetType());
    }

    public MemberInfo SetPropValue(U user, object obj, string name, string value, Type type)
    {
      object propHolder;
      MemberInfo prop = GetProp(user, obj, name, type, out propHolder);
      if(prop != null)
      {
        object obj1 = null;
        if(StringParser.Parse(value, prop.GetVariableType(), ref obj1))
        {
          prop.SetUnindexedValue(propHolder, obj1);
          return prop;
        }
      }

      return null;
    }

    public MemberInfo ModPropValue<T>(U user, object obj, string name, Type type, string delta,
      StringParser.OperatorHandler<T> oper, ref T newValue)
    {
      object propHolder;
      MemberInfo prop = GetProp(user, obj, name, type, out propHolder);
      if(prop != null)
      {
        object obj1 = null;
        if(StringParser.Parse(delta, prop.GetVariableType(), ref obj1))
        {
          object unindexedValue = prop.GetUnindexedValue(obj);
          prop.SetUnindexedValue(propHolder, newValue = oper((T) unindexedValue, (T) obj1));
          return prop;
        }
      }

      return null;
    }

    /// <summary>
    /// Returns the value of a property-chain if user == null or user may read the given prop.
    /// </summary>
    public bool GetPropValue(U user, object obj, ref string accessName, out object value)
    {
      return GetPropValue(user, obj, ref accessName, obj?.GetType(), out value);
    }

    /// <summary>
    /// Returns the value of a property-chain if user == null or user may read the given prop.
    /// </summary>
    public bool GetPropValue(U user, object obj, ref string accessName, Type type, out object value)
    {
      object memberHolder = null;
      MemberInfo[] members = GetMembers(user, obj, accessName, type, ref memberHolder);
      if(members != null && members.Length > 0)
      {
        MemberInfo member = members[0];
        if(user == null || CanRead(member, user))
        {
          accessName = member.Name;
          value = member.GetUnindexedValue(memberHolder);
          return true;
        }
      }

      value = null;
      return false;
    }

    public bool CallMethod(U user, object obj, ref string accessName, string[] args, out object result)
    {
      object propHolder = null;
      MemberInfo[] members = GetMembers(user, obj, accessName, ref propHolder);
      if(members != null && members.Length > 0 && members[0] is MethodInfo)
      {
        foreach(MemberInfo member in members)
        {
          if(member is MethodInfo)
          {
            MethodInfo methodInfo = (MethodInfo) member;
            if(user != null && !CanWrite(member, user) ||
               methodInfo.ContainsGenericParameters)
            {
              result = null;
              return false;
            }

            ParameterInfo[] parameters1 = methodInfo.GetParameters();
            if(parameters1.Length == args.Length)
            {
              object[] parameters2 = new object[args.Length];
              bool flag = true;
              for(int index = 0; index < parameters2.Length; ++index)
              {
                object obj1 = null;
                Type parameterType = parameters1[index].ParameterType;
                if(!parameterType.IsSimpleType() ||
                   !StringParser.Parse(args[index], parameterType, ref obj1))
                {
                  flag = false;
                  break;
                }

                parameters2[index] = obj1;
              }

              if(flag)
              {
                accessName = methodInfo.Name;
                result = methodInfo.Invoke(propHolder, parameters2);
                return true;
              }
            }
          }
        }
      }

      result = null;
      return false;
    }

    public abstract bool CanRead(MemberInfo member, U user);

    public abstract bool CanWrite(MemberInfo member, U user);
  }
}