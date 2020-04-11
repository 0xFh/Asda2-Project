using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WCell.Util
{
    public static class CollectionExtensions
    {
        public static V GetOrCreate<K, V>(this IDictionary<K, V> map, K key) where V : new()
        {
            V v;
            if (!map.TryGetValue(key, out v))
                map.Add(key, v = new V());
            return v;
        }

        public static List<V> GetOrCreate<K, V>(this IDictionary<K, List<V>> map, K key)
        {
            List<V> vList;
            if (!map.TryGetValue(key, out vList))
                map.Add(key, vList = new List<V>());
            return vList;
        }

        public static HashSet<V> GetOrCreate<K, V>(this IDictionary<K, HashSet<V>> map, K key)
        {
            HashSet<V> vSet;
            if (!map.TryGetValue(key, out vSet))
                map.Add(key, vSet = new HashSet<V>());
            return vSet;
        }

        public static IDictionary<K2, V> GetOrCreate<K, K2, V>(this IDictionary<K, IDictionary<K2, V>> map, K key)
        {
            IDictionary<K2, V> dictionary;
            if (!map.TryGetValue(key, out dictionary))
                map.Add(key, dictionary = (IDictionary<K2, V>) new Dictionary<K2, V>());
            return dictionary;
        }

        public static Dictionary<K2, V> GetOrCreate<K, K2, V>(this IDictionary<K, Dictionary<K2, V>> map, K key)
        {
            Dictionary<K2, V> dictionary;
            if (!map.TryGetValue(key, out dictionary))
                map.Add(key, dictionary = new Dictionary<K2, V>());
            return dictionary;
        }

        public static V GetValue<K, V>(this IDictionary<K, V> map, K key)
        {
            V v;
            map.TryGetValue(key, out v);
            return v;
        }

        public static List<TOutput> TransformList<TInput, TOutput>(this IEnumerable<TInput> enumerable,
            Func<TInput, TOutput> transformer)
        {
            List<TOutput> outputList = new List<TOutput>(enumerable.Count<TInput>());
            foreach (TInput input in enumerable)
                outputList.Add(transformer(input));
            return outputList;
        }

        public static TOutput[] TransformArray<TInput, TOutput>(this IEnumerable<TInput> enumerable,
            Func<TInput, TOutput> transformer)
        {
            TOutput[] outputArray = new TOutput[enumerable.Count<TInput>()];
            IEnumerator<TInput> enumerator = enumerable.GetEnumerator();
            for (int index = 0; index < outputArray.Length; ++index)
            {
                enumerator.MoveNext();
                outputArray[index] = transformer(enumerator.Current);
            }

            return outputArray;
        }

        public static void AddUnique<T>(this ICollection<T> items, T item)
        {
            if (items.Contains(item))
                return;
            items.Add(item);
        }

        /// <summary>
        /// The predicate returns false to stop the iteration (and to indicate that it found the item).
        /// Iterate returns true once the predicate returned false the first time.
        /// </summary>
        public static bool Iterate<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            foreach (T obj in items)
            {
                if (!predicate(obj))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// For unexplainable reasons, HashSet's ToArray method is internal
        /// </summary>
        public static T[] MakeArray<T>(this HashSet<T> set)
        {
            T[] objArray = new T[set.Count];
            int num = 0;
            foreach (T obj in set)
                objArray[num++] = obj;
            return objArray;
        }

        public static T RemoveFirst<T>(this IList<T> items, Func<T, bool> filter)
        {
            for (int index = 0; index < items.Count; ++index)
            {
                T obj = items[index];
                if (filter(obj))
                {
                    items.RemoveAt(index);
                    return obj;
                }
            }

            return default(T);
        }

        public static bool Contains<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            foreach (T obj in list)
            {
                if (predicate(obj))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns either the list or a new List if list is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> NotNull<T>(this List<T> list)
        {
            if (list == null)
                list = new List<T>();
            return list;
        }

        public static void SetUnindexedValue(this MemberInfo member, object obj, object value)
        {
            if (member is FieldInfo)
            {
                ((FieldInfo) member).SetValue(obj, value);
            }
            else
            {
                if (!(member is PropertyInfo))
                    throw new Exception("Can only get Values of Fields and Properties");
                ((PropertyInfo) member).SetValue(obj, value, (object[]) null);
            }
        }

        public static object GetUnindexedValue(this MemberInfo member, object obj)
        {
            if (member is FieldInfo)
                return ((FieldInfo) member).GetValue(obj);
            if (member is PropertyInfo)
                return ((PropertyInfo) member).GetValue(obj, (object[]) null);
            throw new Exception("Can only get Values of Fields and Properties");
        }

        public static Type GetVariableType(this MemberInfo member)
        {
            if (member is FieldInfo)
                return ((FieldInfo) member).FieldType;
            if (member is PropertyInfo)
                return ((PropertyInfo) member).PropertyType;
            throw new Exception("Can only get VariableType of Fields and Properties");
        }

        public static string GetFullMemberName(this MemberInfo member)
        {
            string str = member.DeclaringType.FullName + "." + member.Name;
            if (member is MethodInfo)
                str = str + "(" +
                      ((IEnumerable<ParameterInfo>) ((MethodBase) member).GetParameters()).ToString<ParameterInfo>(", ",
                          (Func<ParameterInfo, object>) (param => (object) param.ParameterType.Name)) + ")";
            return str;
        }

        public static Type GetActualType(this MemberInfo member)
        {
            Type type1 = member is Type ? (Type) member : member.GetVariableType();
            Type type2;
            if (type1.IsArray)
            {
                type2 = type1.GetElementType();
                if (type2 == (Type) null)
                    throw new Exception(string.Format("Unable to get Type of Array {0} ({1}).", (object) type1,
                        (object) member.GetFullMemberName()));
            }
            else
                type2 = type1;

            return type2;
        }

        /// <summary>Simple types are primitive-types and strings</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSimpleType(this Type type)
        {
            return type.IsEnum || type.IsPrimitive || type == typeof(string);
        }

        public static bool IsFieldOrProp(this MemberInfo member)
        {
            return (object) (member as FieldInfo) != null || member is PropertyInfo;
        }

        public static bool IsReadonly(this MemberInfo member)
        {
            if (member is FieldInfo)
                return ((FieldInfo) member).IsInitOnly || ((FieldInfo) member).IsLiteral;
            if (member is PropertyInfo)
                return !((PropertyInfo) member).CanWrite ||
                       ((PropertyInfo) member).GetSetMethod() == (MethodInfo) null ||
                       !((PropertyInfo) member).GetSetMethod().IsPublic;
            return true;
        }

        public static bool IsNumericType(this Type type)
        {
            return type.IsInteger() || type.IsFloatingPoint();
        }

        public static bool IsFloatingPoint(this Type type)
        {
            return type == typeof(float) || type == typeof(double);
        }

        public static bool IsInteger(this Type type)
        {
            return type.IsEnum || type == typeof(int) || (type == typeof(uint) || type == typeof(short)) ||
                   (type == typeof(ushort) || type == typeof(byte) ||
                    (type == typeof(sbyte) || type == typeof(long))) || type == typeof(ulong);
        }

        public static unsafe ushort GetUInt16(this byte[] data, uint field)
        {
            uint num = field * 4U;
            if ((long) (num + 2U) > (long) data.Length)
                return ushort.MaxValue;
            fixed (byte* numPtr = &data[num])
                return *(ushort*) numPtr;
        }

        public static unsafe ushort GetUInt16AtByte(this byte[] data, uint startIndex)
        {
            if ((long) (startIndex + 1U) >= (long) data.Length)
                return ushort.MaxValue;
            fixed (byte* numPtr = &data[startIndex])
                return *(ushort*) numPtr;
        }

        public static unsafe uint GetUInt32(this byte[] data, uint field)
        {
            uint num = field * 4U;
            if ((long) (num + 4U) > (long) data.Length)
                return uint.MaxValue;
            fixed (byte* numPtr = &data[num])
                return *(uint*) numPtr;
        }

        public static unsafe int GetInt32(this byte[] data, uint field)
        {
            uint num = field * 4U;
            if ((long) (num + 4U) > (long) data.Length)
                return int.MaxValue;
            fixed (byte* numPtr = &data[num])
                return *(int*) numPtr;
        }

        public static unsafe float GetFloat(this byte[] data, uint field)
        {
            uint num = field * 4U;
            if ((long) (num + 4U) > (long) data.Length)
                return float.NaN;
            fixed (byte* numPtr = &data[num])
                return *(float*) numPtr;
        }

        public static unsafe ulong GetUInt64(this byte[] data, uint startingField)
        {
            uint num = startingField * 4U;
            if ((long) (num + 8U) > (long) data.Length)
                return ulong.MaxValue;
            fixed (byte* numPtr = &data[num])
                return (ulong) *(long*) numPtr;
        }

        public static byte[] GetBytes(this byte[] data, uint startingField, int amount)
        {
            byte[] numArray = new byte[amount];
            uint num = startingField * 4U;
            if ((long) num + (long) amount > (long) data.Length)
                return numArray;
            for (int index = 0; index < amount; ++index)
                numArray[index] = data[(long) num + (long) index];
            return numArray;
        }
    }
}