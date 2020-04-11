using System;
using System.Reflection;

namespace WCell.Util
{
    /// <summary>Reflection utilities used in the network layer</summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Checks if a method's parameters match a given array of types
        /// </summary>
        /// <param name="generic_method_definition">the method to check</param>
        /// <param name="types">the types to check for</param>
        /// <returns>true if the method has the required types for its parameters</returns>
        public static bool SatisfiesGenericConstraints(MethodInfo generic_method_definition, params Type[] types)
        {
            Type[] genericArguments = generic_method_definition.GetGenericArguments();
            if (genericArguments.Length != types.Length)
                return false;
            for (int index = 0; index < types.Length; ++index)
            {
                Type type1 = types[index];
                Type type2 = genericArguments[index];
                if ((type2.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) !=
                    GenericParameterAttributes.None && !type1.IsValueType &&
                    type1.GetConstructor(Type.EmptyTypes) == (ConstructorInfo) null ||
                    (type2.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) !=
                    GenericParameterAttributes.None && !type1.IsValueType ||
                    (type2.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) !=
                    GenericParameterAttributes.None && type1.IsValueType)
                    return false;
                foreach (Type parameterConstraint in type2.GetGenericParameterConstraints())
                {
                    if (parameterConstraint != type1.BaseType &&
                        type1.GetInterface(parameterConstraint.Name) == (Type) null)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if a delegate can be made from this given method for the given delegate type
        /// </summary>
        /// <typeparam name="DelegateType">the type of delegate to be created</typeparam>
        /// <param name="method">the method to be transformed into a delegate</param>
        /// <returns>true if the given method will be able to be of the given delegate type; false otherwise</returns>
        public static bool CanCreateDelegate<DelegateType>(MethodInfo method)
        {
            MethodInfo method1 = typeof(DelegateType).GetMethod("Invoke");
            if (method1.ReturnType != method.ReturnType)
                return false;
            ParameterInfo[] parameters1 = method.GetParameters();
            ParameterInfo[] parameters2 = method1.GetParameters();
            if (parameters1.Length != parameters2.Length)
                return false;
            for (int index = 0; index < parameters1.Length; ++index)
            {
                if (parameters1[index].GetType() != parameters2[index].GetType() ||
                    parameters1[index].GetType().IsInterface &&
                    parameters1[index].GetType().IsAssignableFrom(parameters2[index].GetType()) &&
                    !parameters2[index].GetType().IsAssignableFrom(parameters1[index].GetType()))
                    return false;
            }

            return ReflectionUtils.SatisfiesGenericConstraints(method, method1.GetGenericArguments());
        }
    }
}