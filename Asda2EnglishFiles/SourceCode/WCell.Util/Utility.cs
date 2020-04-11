using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.Util
{
    /// <summary>
    /// Contains miscellaneous utility method used throughout the project.
    /// </summary>
    /// <remarks>Things that can't be added as extension methods, or are too miscellaneous
    /// will most likely be in this class.</remarks>
    public static class Utility
    {
        public static readonly DateTime UnixTimeStart = new DateTime(1970, 1, 1, 0, 0, 0);
        public static readonly object[] EmptyObjectArray = new object[0];

        public static readonly Dictionary<string, Type> TypeMap =
            new Dictionary<string, Type>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);

        public static readonly Dictionary<Type, Dictionary<string, object>> EnumValueMap =
            new Dictionary<Type, Dictionary<string, object>>(300);

        [NotVariable] private static readonly Vector2[] PositionDiffs = new Vector2[10000];
        private static long holdrand = DateTime.Now.Ticks;
        [NotVariable] private static readonly Random R = new Random();
        private static readonly Random rnd = new Random();
        public const int TicksPerSecond = 10000;
        private const long TICKS_SINCE_1970 = 621355968000000000;

        /// <summary>
        /// One second has 10 million system ticks (DateTime.Ticks etc)
        /// </summary>
        private const string DefaultNameSpace = "WCell.Constants.";

        public const float Epsilon = 1E-06f;

        static Utility()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                Utility.InitEnums(assembly);
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(Utility.CurrentDomain_AssemblyLoad);
            Utility.TypeMap.Add("UInt32", typeof(uint));
            Utility.TypeMap.Add("UInt64", typeof(ulong));
            Utility.TypeMap.Add("Int32", typeof(int));
            Utility.TypeMap.Add("Int64", typeof(long));
            Utility.InitPosDiffs();
        }

        public static Vector2 GetPosDiff(int pointNum)
        {
            int index = pointNum % (Utility.PositionDiffs.Length - 1);
            return Utility.PositionDiffs[index];
        }

        public static void InitPosDiffs()
        {
            int num1 = 1;
            int num2 = -num1;
            int num3 = -num1;
            int num4 = Utility.PositionDiffs.Length + 8 - 1;
            int num5 = 0;
            do
            {
                for (; num3 <= num1 && num5++ < num4; ++num3)
                {
                    if (8 < num5)
                        Utility.PositionDiffs[num5 - 8] = new Vector2((float) num2, (float) num3);
                }

                int num6 = num1;
                int num7 = num2 + 1;
                if (num5 < num4)
                {
                    for (; num7 <= num1 && num5++ < num4; ++num7)
                    {
                        if (8 < num5)
                            Utility.PositionDiffs[num5 - 8] = new Vector2((float) num7, (float) num6);
                    }

                    int num8 = num1;
                    int num9 = num6 - 1;
                    if (num5 < num4)
                    {
                        for (; num9 >= -num1 && num5++ < num4; --num9)
                        {
                            if (8 < num5)
                                Utility.PositionDiffs[num5 - 8] = new Vector2((float) num8, (float) num9);
                        }

                        num3 = -num1;
                        num2 = num8 - 1;
                        if (num5 < num4)
                        {
                            for (; num2 >= -num1; --num2)
                            {
                                if (num2 == -num1)
                                {
                                    ++num1;
                                    num2 = -num1;
                                    num3 = -num1;
                                    break;
                                }

                                if (num5++ < num4)
                                {
                                    if (8 < num5)
                                        Utility.PositionDiffs[num5 - 8] = new Vector2((float) num2, (float) num3);
                                }
                                else
                                    break;
                            }
                        }
                        else
                            goto label_17;
                    }
                    else
                        goto label_21;
                }
                else
                    goto label_27;
            } while (num5 < num4);

            goto label_16;
            label_27:
            return;
            label_21:
            return;
            label_17:
            return;
            label_16:;
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Utility.InitEnums(args.LoadedAssembly);
        }

        private static void InitEnums(Assembly asm)
        {
            Utility.AddTypesToTypeMap(asm);
        }

        /// <summary>
        /// Adds all non-standard Enum-types of the given Assembly to the TypeMap.
        /// Also caches all big enums into a dictionary to improve Lookup speed.
        /// </summary>
        /// <param name="asm"></param>
        public static void AddTypesToTypeMap(Assembly asm)
        {
            if (asm.FullName == null || (asm.FullName.StartsWith("System.") || asm.FullName.StartsWith("Microsoft.") ||
                                         (asm.FullName.StartsWith("NHibernate") || asm.FullName.StartsWith("Castle")) ||
                                         (asm.FullName.StartsWith("msvc") || asm.FullName.StartsWith("NLog")) ||
                                         asm.FullName.StartsWith("mscorlib")))
                return;
            foreach (Type type in asm.GetTypes())
            {
                if (!type.FullName.StartsWith("System.") && !type.FullName.StartsWith("Microsoft.") && type.IsValueType)
                {
                    Utility.TypeMap[type.FullName] = type;
                    if (type.IsEnum && !type.IsNested)
                    {
                        Array values = Enum.GetValues(type);
                        Dictionary<string, object> dictionary = new Dictionary<string, object>(values.Length + 100,
                            (IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
                        string[] names = Enum.GetNames(type);
                        for (int index = 0; index < names.Length; ++index)
                            dictionary[names[index]] = values.GetValue(index);
                        Utility.EnumValueMap[type] = dictionary;
                    }
                }
            }
        }

        public static int ToMilliSecondsInt(this DateTime time)
        {
            return (int) (time.Ticks / 10000L);
        }

        public static int ToMilliSecondsInt(this TimeSpan time)
        {
            return (int) (time.Ticks / 10000L);
        }

        public static int ToMilliSecondsInt(int ticks)
        {
            return ticks / 10000;
        }

        /// <summary>Gets the system uptime.</summary>
        /// <returns>the system uptime in milliseconds</returns>
        public static uint GetSystemTime()
        {
            return (uint) Environment.TickCount;
        }

        /// <summary>Gets the time since the Unix epoch.</summary>
        /// <returns>the time since the unix epoch in seconds</returns>
        public static uint GetEpochTime()
        {
            return (uint) ((ulong) (DateTime.UtcNow.Ticks - 621355968000000000L) / 10000000UL);
        }

        public static DateTime GetDateTimeFromUnixTime(uint unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((double) unixTime);
        }

        public static DateTime GetUTCTimeSeconds(long seconds)
        {
            return Utility.UnixTimeStart.AddSeconds((double) seconds);
        }

        public static DateTime GetUTCTimeMillis(long millis)
        {
            return Utility.UnixTimeStart.AddMilliseconds((double) millis);
        }

        /// <summary>Gets the system uptime.</summary>
        /// <remarks>
        /// Even though this returns a long, the original value is a 32-bit integer,
        /// so it will wrap back to 0 after approximately 49 and half days of system uptime.
        /// </remarks>
        /// <returns>the system uptime in milliseconds</returns>
        public static long GetSystemTimeLong()
        {
            return (long) (uint) Environment.TickCount;
        }

        /// <summary>
        /// Converts the current time and date into the time and date format of the WoW client.
        /// </summary>
        /// <returns>a packed time and date</returns>
        public static uint GetDateTimeToGameTime(DateTime n)
        {
            uint num = n.DayOfWeek == DayOfWeek.Sunday ? 6U : (uint) (n.DayOfWeek - 1);
            return (uint) (n.Minute & 63) | (uint) (n.Hour << 6 & 1984) | (uint) ((int) num << 11 & 14336) |
                   (uint) (n.Day - 1 << 14 & 1032192) | (uint) (n.Month - 1 << 20 & 15728640) |
                   (uint) (n.Year - 2000 << 24 & 520093696);
        }

        public static DateTime GetGameTimeToDateTime(uint packedDate)
        {
            int minute = (int) packedDate & 63;
            int hour = (int) (packedDate >> 6) & 31;
            int num1 = (int) (packedDate >> 14) & 63;
            int num2 = (int) (packedDate >> 20) & 15;
            return new DateTime(((int) (packedDate >> 24) & 31) + 2000, num2 + 1, num1 + 1, hour, minute, 0);
        }

        /// <summary>
        /// Gets the time between the Unix epich and a specific <see cref="T:System.DateTime">time</see>.
        /// </summary>
        /// <param name="time">the end time</param>
        /// <returns>the time between the unix epoch and the supplied <see cref="T:System.DateTime">time</see> in seconds</returns>
        public static uint GetEpochTimeFromDT()
        {
            return Utility.GetEpochTimeFromDT(DateTime.Now);
        }

        /// <summary>
        /// Gets the time between the Unix epich and a specific <see cref="T:System.DateTime">time</see>.
        /// </summary>
        /// <param name="time">the end time</param>
        /// <returns>the time between the unix epoch and the supplied <see cref="T:System.DateTime">time</see> in seconds</returns>
        public static uint GetEpochTimeFromDT(DateTime time)
        {
            return (uint) ((ulong) (time.Ticks - 621355968000000000L) / 10000000UL);
        }

        /// <summary>Swaps one reference with another atomically.</summary>
        /// <typeparam name="T">the type of the reference</typeparam>
        /// <param name="originalRef">the original reference</param>
        /// <param name="newRef">the new reference</param>
        public static void SwapReference<T>(ref T originalRef, ref T newRef) where T : class
        {
            T comparand;
            do
            {
                comparand = originalRef;
            } while ((object) Interlocked.CompareExchange<T>(ref originalRef, newRef, comparand) != (object) comparand);
        }

        /// <summary>
        /// Swaps one reference with another atomically, and replaces the original with the given value
        /// </summary>
        /// <typeparam name="T">the type of the reference</typeparam>
        /// <param name="originalRef">the original reference</param>
        /// <param name="newRef">the new reference</param>
        /// <param name="replacement">the value to replace the original with</param>
        public static void SwapReference<T>(ref T originalRef, ref T newRef, T replacement) where T : class
        {
            do
            {
                newRef = originalRef;
            } while ((object) Interlocked.CompareExchange<T>(ref originalRef, replacement, newRef) != (object) newRef);
        }

        /// <summary>Moves memory from one array to another.</summary>
        /// <param name="src">the pointer to the source array</param>
        /// <param name="srcIndex">the index to read from in the source array</param>
        /// <param name="dest">the destination array</param>
        /// <param name="destIndex">the index to write to in the destination array</param>
        /// <param name="len">the number of bytes to move</param>
        public static unsafe void MoveMemory(byte* src, int srcIndex, byte[] dest, int destIndex, int len)
        {
            if (len == 0)
                return;
            src += srcIndex;
            fixed (byte* numPtr1 = &dest[destIndex])
            {
                byte* numPtr2 = numPtr1;
                while (len-- > 0)
                    *numPtr2++ = *src++;
            }
        }

        /// <summary>Moves memory from one array to another.</summary>
        /// <param name="src">the source array</param>
        /// <param name="srcIndex">the index to read from in the source array</param>
        /// <param name="dest">the pointer to the destination array</param>
        /// <param name="destIndex">the index to write to in the destination array</param>
        /// <param name="len">the number of bytes to move</param>
        public static unsafe void MoveMemory(byte[] src, int srcIndex, byte* dest, int destIndex, int len)
        {
            if (len == 0)
                return;
            dest += destIndex;
            fixed (byte* numPtr1 = &src[srcIndex])
            {
                byte* numPtr2 = numPtr1;
                while (len-- > 0)
                    *dest++ = *numPtr2++;
            }
        }

        /// <summary>Cast one thing into another</summary>
        public static T Cast<T>(object obj)
        {
            return (T) Convert.ChangeType(obj, typeof(T));
        }

        /// <summary>
        /// Returns the string representation of an IEnumerable (all elements, joined by comma)
        /// </summary>
        /// <param name="conj">The conjunction to be used between each elements of the collection</param>
        public static string ToString<T>(this IEnumerable<T> collection, string conj)
        {
            return collection == null ? "(null)" : string.Join(conj, Utility.ToStringArrT<T>(collection));
        }

        /// <summary>
        /// Returns the string representation of an IEnumerable (all elements, joined by comma)
        /// </summary>
        /// <param name="conj">The conjunction to be used between each elements of the collection</param>
        public static string ToString<T>(this IEnumerable<T> collection, string conj, Func<T, object> converter)
        {
            return collection == null ? "(null)" : string.Join(conj, Utility.ToStringArrT<T>(collection, converter));
        }

        /// <summary>
        /// Returns the string representation of an IEnumerable (all elements, joined by comma)
        /// </summary>
        /// <param name="conj">The conjunction to be used between each elements of the collection</param>
        public static string ToStringCol(this ICollection collection, string conj)
        {
            return collection == null ? "(null)" : string.Join(conj, Utility.ToStringArr((IEnumerable) collection));
        }

        public static string ToString(this IEnumerable collection, string conj)
        {
            return collection == null ? "(null)" : string.Join(conj, Utility.ToStringArr(collection));
        }

        public static string[] ToStringArrT<T>(IEnumerable<T> collection)
        {
            return Utility.ToStringArrT<T>(collection, (Func<T, object>) null);
        }

        public static string[] ToStringArr(IEnumerable collection)
        {
            List<string> stringList = new List<string>();
            foreach (object obj in collection)
            {
                if (obj != null)
                    stringList.Add(obj.ToString());
            }

            return stringList.ToArray();
        }

        public static string[] ToStringArrT<T>(IEnumerable<T> collection, Func<T, object> converter)
        {
            string[] strArray = new string[collection.Count<T>()];
            IEnumerator<T> enumerator = collection.GetEnumerator();
            int num = 0;
            while (enumerator.MoveNext())
            {
                T current = enumerator.Current;
                if (!object.Equals((object) current, (object) default(T)))
                    strArray[num++] = (converter != null ? converter(current) : (object) current).ToString();
            }

            return strArray;
        }

        public static string[] ToJoinedStringArr<T>(IEnumerable<T> col, int partCount, string conj)
        {
            string[] stringArrT = Utility.ToStringArrT<T>(col);
            List<string> stringList1 = new List<string>();
            List<string> stringList2 = new List<string>(partCount);
            int index = 0;
            int num = 0;
            for (; index < stringArrT.Length; ++index)
            {
                stringList2.Add(stringArrT[index]);
                if (num == partCount)
                {
                    num = 0;
                    stringList1.Add(string.Join(conj, stringList2.ToArray()));
                    stringList2.Clear();
                }

                ++num;
            }

            if (stringList2.Count > 0)
                stringList1.Add(string.Join(conj, stringList2.ToArray()));
            return stringList1.ToArray();
        }

        public static string ToString<K, V>(this IEnumerable<KeyValuePair<K, V>> args, string indent, string seperator)
        {
            string str = "";
            int num = 0;
            foreach (KeyValuePair<K, V> keyValuePair in args)
            {
                ++num;
                str = str + indent + (object) keyValuePair.Key + " = " + (object) keyValuePair.Value;
                if (num < args.Count<KeyValuePair<K, V>>())
                    str += seperator;
            }

            return str;
        }

        /// <summary>Random bool value</summary>
        public static bool HeadsOrTails()
        {
            return Utility.Random(2) == 0;
        }

        public static int Random()
        {
            return Utility.R.Next();
        }

        public static uint RandomUInt()
        {
            return (uint) Utility.R.Next();
        }

        public static bool Chance()
        {
            return Utility.Chance(Utility.RandomFloat());
        }

        public static bool Chance(double chance)
        {
            return chance > 1.0 || chance >= 0.0 && (double) Utility.Random() / (double) short.MaxValue <= chance;
        }

        public static bool Chance(float chance)
        {
            return (double) chance > 1.0 || (double) chance >= 0.0 && (double) Utility.RandomFloat() <= (double) chance;
        }

        public static float RandomFloat()
        {
            return (float) Utility.R.NextDouble();
        }

        /// <summary>Generates a pseudo-random number in range [from, to)</summary>
        public static int Random(int from, int to)
        {
            return from == to
                ? from
                : (from > to ? Utility.Random() % (from - to) + to : Utility.Random() % (to - from) + from);
        }

        public static uint Random(uint from, uint to)
        {
            return (int) from == (int) to
                ? from
                : (from > to ? Utility.RandomUInt() % (from - to) + to : Utility.RandomUInt() % (to - from) + from);
        }

        public static int Random(int max)
        {
            return Utility.Random() % max;
        }

        public static uint RandomUInt(uint max)
        {
            return Utility.RandomUInt() % max;
        }

        public static float Random(float from, float to)
        {
            return (double) from > (double) to
                ? Utility.RandomFloat() * (from - to) + to
                : Utility.RandomFloat() * (to - from) + from;
        }

        public static double Random(double from, double to)
        {
            return from > to
                ? (double) Utility.RandomFloat() * (from - to) + to
                : (double) Utility.RandomFloat() * (to - from) + from;
        }

        public static void Shuffle<T>(ICollection<T> col)
        {
            T[] array = col.ToArray<T>();
            byte[] numArray = new byte[array.Length];
            Utility.rnd.NextBytes(numArray);
            Array.Sort<byte, T>(numArray, array);
            col.Clear();
            for (int index = 0; index < array.Length; ++index)
            {
                T obj = array[index];
                col.Add(obj);
            }
        }

        public static O GetRandom<O>(this IList<O> os)
        {
            return os.Count == 0 ? default(O) : os[Utility.Random(0, os.Count)];
        }

        /// <summary>
        /// Measures how long the given func takes to be executed repeats times
        /// </summary>
        public static void Measure(string name, int repeats, Action action)
        {
            DateTime now = DateTime.Now;
            for (int index = 0; index < repeats; ++index)
                action();
            TimeSpan timeSpan = DateTime.Now - now;
            Console.WriteLine(name + " (" + (object) repeats + " time(s)) took: " + (object) timeSpan);
        }

        /// <summary>Gets the biggest value of a numeric enum</summary>
        public static T GetMaxEnum<T>()
        {
            return ((IEnumerable<T>) Enum.GetValues(typeof(T))).Max<T>();
        }

        /// <summary>
        /// Creates and returns an array of all indices that are set within the given flag field.
        /// eg. {11000011, 11000011} would result into an array containing: 0,1,6,7,8,9,14,15
        /// </summary>
        public static uint[] GetSetIndices(uint[] flagsArr)
        {
            List<uint> indices = new List<uint>();
            foreach (uint flags in flagsArr)
                Utility.GetSetIndices(indices, flags);
            return indices.ToArray();
        }

        public static uint Sum(this IEnumerable<uint> arr)
        {
            return arr.Aggregate<uint, uint>(0U, (Func<uint, uint, uint>) ((current, n) => current + n));
        }

        /// <summary>
        /// Creates and returns an array of all indices that are set within the given flag field.
        /// eg. 11000011 would result into an array containing: 0,1,6,7
        /// </summary>
        public static uint[] GetSetIndices(uint flags)
        {
            List<uint> indices = new List<uint>();
            Utility.GetSetIndices(indices, flags);
            return indices.ToArray();
        }

        public static T[] GetSetIndicesEnum<T>(T flags)
        {
            List<uint> indices = new List<uint>();
            uint flags1 = (uint) Convert.ChangeType((object) flags, typeof(uint));
            Utility.GetSetIndices(indices, flags1);
            if (indices.Count == 0)
                return new T[1] {(T) (object) 0U};
            T[] objArray = new T[indices.Count];
            for (int index = 0; index < indices.Count; ++index)
            {
                object obj = (object) (uint) (1 << (int) indices[index]);
                objArray[index] = (T) obj;
            }

            return objArray;
        }

        public static void GetSetIndices(List<uint> indices, uint flags)
        {
            for (uint index = 0; index < 32U; ++index)
            {
                if (((long) flags & (long) (1 << (int) index)) != 0L)
                    indices.Add(index);
            }
        }

        /// <summary>
        /// Creates and returns an array of all indices that are set within the given flag field.
        /// eg. 11000011 would result into an array containing: 0,1,6,7
        /// </summary>
        public static T[] GetSetIndices<T>(uint flags)
        {
            List<T> objList = new List<T>(5);
            for (uint index = 0; index < 32U; ++index)
            {
                if (((long) flags & (long) (1 << (int) index)) != 0L)
                {
                    if (typeof(T).IsEnum)
                        objList.Add((T) Enum.Parse(typeof(T), index.ToString()));
                    else
                        objList.Add((T) Convert.ChangeType((object) index, typeof(T)));
                }
            }

            return objList.ToArray();
        }

        public static A[] CreateEnumArray<E, A>()
        {
            return new A[(int) Convert.ChangeType((object) Utility.GetMaxEnum<E>(), typeof(int))];
        }

        /// <summary>
        /// Delays the given action by the given amount of milliseconds
        /// </summary>
        /// <returns>The timer that performs the delayed call (in case that you might want to cancel earlier)</returns>
        public static Timer Delay(uint millis, Action action)
        {
            Timer timer = (Timer) null;
            timer = new Timer((TimerCallback) (sender =>
            {
                action();
                timer.Dispose();
            }));
            timer.Change((long) millis, -1L);
            return timer;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool IsInRange(float sqDistance, float max)
        {
            return (double) sqDistance <= (double) max * (double) max;
        }

        public static string GetAbsolutePath(string file)
        {
            return new DirectoryInfo(file).FullName;
        }

        public static IPAddress ParseOrResolve(string input)
        {
            IPAddress address1;
            if (IPAddress.TryParse(input, out address1))
                return address1;
            return ((IEnumerable<IPAddress>) Dns.GetHostAddresses(input))
                   .Where<IPAddress>((Func<IPAddress, bool>) (address =>
                       address.AddressFamily == AddressFamily.InterNetwork)).FirstOrDefault<IPAddress>() ??
                   IPAddress.Loopback;
        }

        public static string FormatMoney(uint money)
        {
            string str = "";
            if (money >= 10000U)
            {
                str = str + (object) (money / 10000U) + "g ";
                money %= 10000U;
            }

            if (money >= 100U)
            {
                str = str + (object) (money / 100U) + "s ";
                money %= 100U;
            }

            if (money > 0U)
                str = str + (object) money + "c";
            return str;
        }

        public static string Format(this TimeSpan time)
        {
            return string.Format("{0}{1:00}h {2:00}m {3:00}s",
                time.TotalDays > 0.0 ? (object) (((int) time.TotalDays).ToString() + "d ") : (object) "",
                (object) time.Hours, (object) time.Minutes, (object) time.Seconds);
        }

        public static string FormatMillis(this DateTime time)
        {
            return string.Format("{0:00}h {1:00}m {2:00}s {3:00}ms", (object) time.Hour, (object) time.Minute,
                (object) time.Second, (object) time.Millisecond);
        }

        /// <summary>Checks whether the given mail-address is valid.</summary>
        public static bool IsValidEMailAddress(string mail)
        {
            return EmailAddressParser.Valid(mail, false);
        }

        public static bool IsStatic(this Type type)
        {
            return (type.Attributes & (TypeAttributes.Abstract | TypeAttributes.Sealed)) ==
                   (TypeAttributes.Abstract | TypeAttributes.Sealed);
        }

        /// <summary>
        /// When overridden in a derived class, returns an array of custom attributes identified by System.Type.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this type are returned.</typeparam>
        /// <param name="methodInfo"></param>
        /// <returns>An array of custom attributes applied to this member, or an array with zero (0) elements if no attributes have been applied.</returns>
        public static T[] GetCustomAttributes<T>(this MemberInfo methodInfo) where T : Attribute
        {
            return methodInfo.GetCustomAttributes(typeof(T), false) as T[];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrType"></param>
        /// <returns></returns>
        public static Type GetArrUnderlyingType(Type arrType)
        {
            string fullName = arrType.FullName;
            int length = fullName.IndexOf('[');
            if (length <= -1)
                return (Type) null;
            string name = fullName.Substring(0, length);
            return arrType.Assembly.GetType(name);
        }

        public static Type GetType(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == (Type) null && (!Utility.TypeMap.TryGetValue(typeName, out type) &&
                                        !Utility.TypeMap.TryGetValue("WCell.Constants." + typeName, out type)))
                throw new Exception("Invalid Type specified: " + typeName);
            return type;
        }

        /// <summary>
        /// Gets all assemblies that match the given fully qualified name without version checks etc.
        /// </summary>
        /// <param name="asmName"></param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetMatchingAssemblies(string asmName)
        {
            string[] strArray = asmName.Split(',');
            if (strArray.Length > 0)
                asmName = strArray[0];
            return ((IEnumerable<Assembly>) AppDomain.CurrentDomain.GetAssemblies()).Where<Assembly>(
                (Func<Assembly, bool>) (asm => asm.GetName().Name == asmName));
        }

        public static object ChangeType(object obj, Type type)
        {
            return Utility.ChangeType(obj, type, false);
        }

        public static object ChangeType(object obj, Type type, bool underlyingType)
        {
            if (type.IsEnum)
            {
                Type underlyingType1 = Enum.GetUnderlyingType(type);
                if (!underlyingType)
                    obj = Enum.ToObject(type, obj);
                else if (underlyingType1 != obj.GetType())
                    obj = Convert.ChangeType(obj, underlyingType1);
                return obj;
            }

            ConstructorInfo constructor = type.GetConstructor(new Type[1]
            {
                obj.GetType()
            });
            if (constructor == (ConstructorInfo) null)
            {
                try
                {
                    return Convert.ChangeType(obj, type);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Could not convert \"{0}\" from {1} to {2} - {2} has no public ctor with one argument of type \"{1}\".",
                            obj, (object) obj.GetType(), (object) type), ex);
                }
            }
            else
                return constructor.Invoke(new object[1] {obj});
        }

        /// <summary>
        /// Writes the content of all files in the given directory to the given output file
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="outputFile"></param>
        public static void MergeFiles(string directory, string outputFile)
        {
            Utility.MergeFiles(Directory.GetFiles(directory), outputFile);
        }

        public static void MergeFiles(string[] inputFiles, string outputFile)
        {
            using (FileStream fileStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter streamWriter = new StreamWriter((Stream) fileStream))
                {
                    foreach (string inputFile in inputFiles)
                    {
                        byte[] buffer = System.IO.File.ReadAllBytes(inputFile);
                        streamWriter.WriteLine();
                        streamWriter.WriteLine("# " + inputFile);
                        streamWriter.WriteLine();
                        streamWriter.Flush();
                        fileStream.Write(buffer, 0, buffer.Length);
                        fileStream.Flush();
                    }
                }
            }
        }

        public static DirectoryInfo GetDirectory(this FileSystemInfo file)
        {
            if (file is DirectoryInfo)
                return ((DirectoryInfo) file).Parent;
            if (file is FileInfo)
                return ((FileInfo) file).Directory;
            return (DirectoryInfo) null;
        }

        public static void MKDirs(this FileInfo file)
        {
            file.GetDirectory().MKDirs();
        }

        public static void MKDirs(this DirectoryInfo dir)
        {
            if (dir.Exists)
                return;
            DirectoryInfo parent = dir.Parent;
            if (parent != null && !parent.Exists)
                dir.Parent.MKDirs();
            dir.Create();
        }

        /// <summary>Returns up to the n first lines from the given file.</summary>
        /// <param name="fileName"></param>
        /// <param name="n"></param>
        /// <param name="ignoreEmpty"></param>
        /// <returns></returns>
        public static string[] ReadLines(string fileName, int n, bool ignoreEmpty)
        {
            string[] strArray = new string[n];
            using (StreamReader streamReader = new StreamReader(fileName))
            {
                for (int index = 0; index < n; ++index)
                {
                    if (!streamReader.EndOfStream)
                    {
                        strArray[index] = streamReader.ReadLine();
                        if (ignoreEmpty && strArray[index].Length == 0)
                            --index;
                    }
                    else
                        break;
                }
            }

            return strArray;
        }

        public static string GetStringRepresentation(object val)
        {
            if (val is string)
                return (string) val;
            if (val is ICollection)
                return ((ICollection) val).ToStringCol(", ");
            if (val is IEnumerable)
                return ((IEnumerable) val).ToString(", ");
            if (val is TimeSpan)
                return ((TimeSpan) val).Format();
            Type type = val.GetType();
            if (!type.IsEnum)
                return val.ToString();
            Type underlyingType = Enum.GetUnderlyingType(type);
            object obj = Convert.ChangeType(val, underlyingType);
            return val.ToString() + " (" + obj + ")";
        }

        public static bool ContainsIgnoreCase(this string str, string part)
        {
            return str.IndexOf(part, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public static bool IsWithinEpsilon(this float val, float otherVal)
        {
            return (double) val <= (double) otherVal + 9.99999997475243E-07 &&
                   (double) val >= (double) otherVal - 9.99999997475243E-07;
        }

        public static bool IsLessOrNearlyEqual(this float val, float otherVal)
        {
            return (double) val < (double) otherVal || val.IsWithinEpsilon(otherVal);
        }

        public static long MakeLong(int low, int high)
        {
            return (long) (uint) low | (long) high << 32;
        }

        /// <summary>
        /// Unpacks a long that was packed with <see cref="M:WCell.Util.Utility.MakeLong(System.Int32,System.Int32)"></see> into two ints
        /// </summary>
        /// <param name="val">The packed long</param>
        /// <param name="low">the low part to unpack into</param>
        /// <param name="high">the high part to unpack into</param>
        public static void UnpackLong(long val, ref int low, ref int high)
        {
            low = (int) val;
            high = (int) (val >> 32);
        }
    }
}