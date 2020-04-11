using System;
using System.Collections.Generic;
using WCell.Util.Strings;

namespace WCell.Util
{
    public class StringParser
    {
        public static Dictionary<Type, Func<string, object>> TypeParsers =
            ((Func<Dictionary<Type, Func<string, object>>>) (() => new Dictionary<Type, Func<string, object>>()
            {
                {
                    typeof(int),
                    (Func<string, object>) (strVal => (object) int.Parse(strVal))
                },
                {
                    typeof(float),
                    (Func<string, object>) (strVal => (object) float.Parse(strVal))
                },
                {
                    typeof(long),
                    (Func<string, object>) (strVal => (object) long.Parse(strVal))
                },
                {
                    typeof(ulong),
                    (Func<string, object>) (strVal => (object) ulong.Parse(strVal))
                },
                {
                    typeof(bool),
                    (Func<string, object>) (strVal =>
                        strVal.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
                        strVal.Equals("1", StringComparison.InvariantCultureIgnoreCase)
                            ? (object) true
                            : (strVal.Equals("yes", StringComparison.InvariantCultureIgnoreCase)
                                ? (object) true
                                : (object) false))
                },
                {
                    typeof(double),
                    (Func<string, object>) (strVal => (object) double.Parse(strVal))
                },
                {
                    typeof(uint),
                    (Func<string, object>) (strVal => (object) uint.Parse(strVal))
                },
                {
                    typeof(short),
                    (Func<string, object>) (strVal => (object) short.Parse(strVal))
                },
                {
                    typeof(ushort),
                    (Func<string, object>) (strVal => (object) short.Parse(strVal))
                },
                {
                    typeof(byte),
                    (Func<string, object>) (strVal => (object) byte.Parse(strVal))
                },
                {
                    typeof(char),
                    (Func<string, object>) (strVal => (object) strVal[0])
                }
            }))();

        public static readonly StringParser.OperatorHandler<long> BinaryOrHandler =
            (StringParser.OperatorHandler<long>) ((x, y) => x | y);

        public static readonly StringParser.OperatorHandler<long> BinaryXOrHandler =
            (StringParser.OperatorHandler<long>) ((x, y) => x & ~y);

        public static readonly StringParser.OperatorHandler<long> BinaryAndHandler =
            (StringParser.OperatorHandler<long>) ((x, y) => x & y);

        public static readonly StringParser.OperatorHandler<long> PlusHandler =
            (StringParser.OperatorHandler<long>) ((x, y) => x + y);

        public static readonly StringParser.OperatorHandler<long> MinusHandler =
            (StringParser.OperatorHandler<long>) ((x, y) => x - y);

        public static readonly StringParser.OperatorHandler<long> DivideHandler =
            (StringParser.OperatorHandler<long>) ((x, y) => x / y);

        public static readonly StringParser.OperatorHandler<long> MultiHandler =
            (StringParser.OperatorHandler<long>) ((x, y) => x * y);

        public static readonly Dictionary<string, StringParser.OperatorHandler<long>> IntOperators =
            new Dictionary<string, StringParser.OperatorHandler<long>>();

        static StringParser()
        {
            StringParser.IntOperators["||"] = StringParser.BinaryOrHandler;
            StringParser.IntOperators["|"] = StringParser.BinaryOrHandler;
            StringParser.IntOperators["^"] = StringParser.BinaryXOrHandler;
            StringParser.IntOperators["&"] = StringParser.BinaryAndHandler;
            StringParser.IntOperators["+"] = StringParser.PlusHandler;
            StringParser.IntOperators["-"] = StringParser.MinusHandler;
            StringParser.IntOperators["*"] = StringParser.DivideHandler;
            StringParser.IntOperators["/"] = StringParser.MultiHandler;
        }

        public static object Parse(string stringVal, Type type)
        {
            object obj = (object) null;
            if (!StringParser.Parse(stringVal, type, ref obj))
                throw new Exception(string.Format("Unable to parse string-Value \"{0}\" as Type \"{1}\"",
                    (object) stringVal, (object) type));
            return obj;
        }

        public static bool Parse(string str, Type type, ref object obj)
        {
            if (!type.IsArray)
                return StringParser.ParseSingleValue(str, type, ref obj);
            StringStream stringStream = new StringStream(str);
            int length = 0;
            while (stringStream.HasNext)
            {
                ++length;
                string result;
                if (!stringStream.NextString(out result, ","))
                    break;
            }

            stringStream.Position = 0;
            Type elementType = type.GetElementType();
            Array instance = Array.CreateInstance(elementType, length);
            for (int index = 0; index < length; ++index)
            {
                object obj1 = (object) null;
                string result;
                stringStream.NextString(out result, ",");
                if (!StringParser.ParseSingleValue(result, elementType, ref obj1))
                    return false;
                instance.SetValue(obj1, index);
            }

            obj = (object) instance;
            return true;
        }

        public static bool ParseSingleValue(string str, Type type, ref object obj)
        {
            if (type == typeof(string))
                obj = (object) str;
            else if (type.IsEnum)
            {
                try
                {
                    obj = Enum.Parse(type, str, true);
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                Func<string, object> func;
                if (!StringParser.TypeParsers.TryGetValue(type, out func))
                    return false;
                try
                {
                    obj = func(str);
                    return obj != null;
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Evaluates the given (simple) expression
        /// 
        /// TODO: Use Polish Notation to allow more efficiency and complexity
        /// TODO: Add operator priority
        /// </summary>
        public static bool Eval(Type valType, ref long val, string expr, ref object error, bool startsWithOperator)
        {
            string[] strArray = expr.Split(new char[1] {' '}, StringSplitOptions.RemoveEmptyEntries);
            bool flag = startsWithOperator;
            StringParser.OperatorHandler<long> operatorHandler = (StringParser.OperatorHandler<long>) null;
            foreach (string str1 in strArray)
            {
                string str2 = str1.Trim();
                if (flag)
                {
                    if (!StringParser.IntOperators.TryGetValue(str2, out operatorHandler))
                    {
                        error = (object) ("Invalid operator: " + str2);
                        return false;
                    }
                }
                else
                {
                    object obj = (object) null;
                    if (!StringParser.Parse(str2, valType, ref obj))
                    {
                        error = (object) ("Could not convert value \"" + str2 + "\" to Type \"" + (object) valType +
                                          "\"");
                        return false;
                    }

                    long y = (long) Convert.ChangeType(obj, typeof(long));
                    val = operatorHandler == null ? y : operatorHandler(val, y);
                }

                flag = !flag;
            }

            return true;
        }

        public delegate T OperatorHandler<T>(T x, T y);
    }
}