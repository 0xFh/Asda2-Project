using System;
using System.Collections.Generic;

namespace WCell.Util
{
    public static class ArrayUtil
    {
        /// <summary>
        /// At least ensure a size of highestIndex * this, if index is not valid within an array.
        /// </summary>
        public const float LoadConstant = 1.5f;

        /// <summary>
        /// Ensures that the given array has at least the given size and resizes if its too small
        /// </summary>
        public static void EnsureSize<T>(ref T[] arr, int size)
        {
            if (arr.Length >= size)
                return;
            Array.Resize<T>(ref arr, size);
        }

        /// <summary>
        /// Returns the entry in this array at the given index, or null if the index is out of bounds
        /// </summary>
        public static T Get<T>(this T[] arr, int index)
        {
            if (index >= arr.Length || index < 0)
                return default(T);
            return arr[index];
        }

        /// <summary>
        /// Returns the entry in this array at the given index, or null if the index is out of bounds
        /// </summary>
        public static T Get<T>(this T[] arr, uint index)
        {
            if ((long) index >= (long) arr.Length)
                return default(T);
            return arr[index];
        }

        /// <summary>
        /// Returns arr[index] or, if index is out of bounds, arr.Last()
        /// </summary>
        public static T GetMax<T>(this T[] arr, uint index)
        {
            if ((long) index >= (long) arr.Length)
                index = (uint) (arr.Length - 1);
            return arr[index];
        }

        /// <summary>
        /// Cuts away everything after and including the first null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        public static void Trunc<T>(ref T[] arr) where T : class
        {
            int num = arr.Length - 1;
            for (int newSize = 0; newSize <= num; ++newSize)
            {
                if ((object) arr[newSize] != null && newSize != num)
                {
                    Array.Resize<T>(ref arr, newSize);
                    break;
                }
            }
        }

        public static void TruncVals<T>(ref T[] arr) where T : struct
        {
            int num = arr.Length - 1;
            for (int newSize = 0; newSize <= num; ++newSize)
            {
                if (arr[newSize].Equals((object) default(T)) && newSize != num)
                {
                    Array.Resize<T>(ref arr, newSize);
                    break;
                }
            }
        }

        /// <summary>Cuts away all null values</summary>
        public static void Prune<T>(ref T[] arr) where T : class
        {
            List<T> objList = new List<T>(arr.Length);
            foreach (T obj in arr)
            {
                if ((object) obj != null)
                    objList.Add(obj);
            }

            arr = objList.ToArray();
        }

        /// <summary>Cuts away all null values</summary>
        public static void PruneStrings(ref string[] arr)
        {
            List<string> stringList = new List<string>(arr.Length);
            foreach (string str in arr)
            {
                if (!string.IsNullOrEmpty(str))
                    stringList.Add(str);
            }

            arr = stringList.ToArray();
        }

        /// <summary>Cuts away all null values</summary>
        public static void PruneVals<T>(ref T[] arr) where T : struct
        {
            List<T> objList = new List<T>(arr.Length);
            foreach (T obj in arr)
            {
                if (!obj.Equals((object) default(T)))
                    objList.Add(obj);
            }

            arr = objList.ToArray();
        }

        public static void Set<T>(ref T[] arr, uint index, T val)
        {
            if ((long) index >= (long) arr.Length)
                ArrayUtil.EnsureSize<T>(ref arr, (int) ((double) index * 1.5) + 1);
            arr[index] = val;
        }

        public static void Set<T>(ref T[] arr, uint index, T val, int maxSize)
        {
            if ((long) index >= (long) arr.Length)
                ArrayUtil.EnsureSize<T>(ref arr, maxSize);
            arr[index] = val;
        }

        public static List<T> GetOrCreate<T>(ref List<T>[] arr, uint index)
        {
            if ((long) index >= (long) arr.Length)
                ArrayUtil.EnsureSize<List<T>>(ref arr, (int) ((double) index * 1.5) + 1);
            return arr[index] ?? (arr[index] = new List<T>());
        }

        /// <summary>
        /// Adds the given value to the first slot that is not occupied in the given array
        /// </summary>
        /// <returns>The index at which it was added</returns>
        public static uint Add<T>(ref T[] arr, T val)
        {
            uint freeIndex = arr.GetFreeIndex<T>();
            if ((long) freeIndex >= (long) arr.Length)
                ArrayUtil.EnsureSize<T>(ref arr, (int) ((double) freeIndex * 1.5) + 1);
            arr[freeIndex] = val;
            return freeIndex;
        }

        /// <summary>Appends the given values to the end of arr</summary>
        /// <returns>The index at which it was added</returns>
        public static void Concat<T>(ref T[] arr, T[] values)
        {
            int length = arr.Length;
            Array.Resize<T>(ref arr, length + values.Length);
            Array.Copy((Array) values, 0, (Array) arr, length, values.Length);
        }

        /// <summary>
        /// Adds the given value to the first slot that is not occupied in the given array
        /// </summary>
        /// <returns>The index at which it was added</returns>
        public static uint AddOnlyOne<T>(ref T[] arr, T val)
        {
            uint freeIndex = arr.GetFreeIndex<T>();
            if ((long) freeIndex >= (long) arr.Length)
                ArrayUtil.EnsureSize<T>(ref arr, (int) freeIndex + 1);
            arr[freeIndex] = val;
            return freeIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The index at which it was added</returns>
        public static int AddElement<T>(this T[] arr, T val)
        {
            uint freeIndex = arr.GetFreeIndex<T>();
            if ((long) freeIndex >= (long) arr.Length)
                return -1;
            arr[freeIndex] = val;
            return (int) freeIndex;
        }

        public static bool ContainsIndex<T>(this T[] arr, uint index) where T : class
        {
            return (long) index < (long) arr.Length && (object) arr[index] != null;
        }

        public static uint GetFreeIndex<T>(this T[] arr)
        {
            uint num = 0;
            while ((long) num < (long) arr.Length &&
                   ((object) arr[num] != null && !arr[num].Equals((object) default(T))))
                ++num;
            return num;
        }

        /// <summary>
        /// Believe it or not, .NET has no such method by default.
        /// Array.Equals is not overridden properly.
        /// </summary>
        public static bool EqualValues<T>(this T[] arr, T[] arr2) where T : struct
        {
            for (int index = 0; index < arr.Length; ++index)
            {
                if (!arr[index].Equals((object) arr2[index]))
                    return false;
            }

            return true;
        }

        public static uint GetFreeValueIndex<T>(this T[] arr) where T : struct
        {
            uint num;
            for (num = 0U; (long) num < (long) arr.Length; ++num)
            {
                if (arr[num].Equals((object) default(T)))
                    return num;
            }

            return num + 1U;
        }

        /// <summary>Removes all empty entries from an array</summary>
        public static T GetWhere<T>(this T[] arr, Func<T, bool> predicate)
        {
            for (int index = 0; index < arr.Length; ++index)
            {
                T obj = arr[index];
                if (predicate(obj))
                    return obj;
            }

            return default(T);
        }

        public static void SetValue(Array arr, int index, object value)
        {
            Type elementType = arr.GetType().GetElementType();
            if (elementType.IsEnum && elementType != value.GetType())
                value = Enum.Parse(elementType, value.ToString());
            arr.SetValue(value, index);
        }

        public static void Shuffle<T>(this T[] arr)
        {
        }

        public static void Reverse<T>(this T[] arr)
        {
            int num = arr.Length - 1;
            for (int index = 0; index < arr.Length / 2; ++index)
            {
                T obj1 = arr[index];
                T obj2 = arr[num - index];
                arr[index] = obj2;
                arr[num - index] = obj1;
            }
        }

        /// <summary>
        /// Sets all values of the given array between offset and length to the given obj
        /// </summary>
        public static void Fill<T>(this T[] arr, T obj, int offset, int until)
        {
            for (int index = offset; index <= until; ++index)
                arr[index] = obj;
        }

        public static void Fill(this int[] arr, int offset, int until, int startVal)
        {
            for (int index = offset; index <= until; ++index)
                arr[index] = startVal++;
        }
    }
}