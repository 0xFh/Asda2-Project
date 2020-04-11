using System;
using System.Collections.Generic;
using WCell.Util.Strings;

namespace WCell.Util
{
  public class StringParser
  {
    public static Dictionary<Type, Func<string, object>> TypeParsers =
      ((Func<Dictionary<Type, Func<string, object>>>) (() => new Dictionary<Type, Func<string, object>>
      {
        {
          typeof(int),
          strVal => (object) int.Parse(strVal)
        },
        {
          typeof(float),
          strVal => (object) float.Parse(strVal)
        },
        {
          typeof(long),
          strVal => (object) long.Parse(strVal)
        },
        {
          typeof(ulong),
          strVal => (object) ulong.Parse(strVal)
        },
        {
          typeof(bool),
          strVal =>
            strVal.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
            strVal.Equals("1", StringComparison.InvariantCultureIgnoreCase)
              ? (object) true
              : (strVal.Equals("yes", StringComparison.InvariantCultureIgnoreCase)
                ? (object) true
                : (object) false)
        },
        {
          typeof(double),
          strVal => (object) double.Parse(strVal)
        },
        {
          typeof(uint),
          strVal => (object) uint.Parse(strVal)
        },
        {
          typeof(short),
          strVal => (object) short.Parse(strVal)
        },
        {
          typeof(ushort),
          strVal => (object) short.Parse(strVal)
        },
        {
          typeof(byte),
          strVal => (object) byte.Parse(strVal)
        },
        {
          typeof(char),
          strVal => (object) strVal[0]
        }
      }))();

    public static readonly OperatorHandler<long> BinaryOrHandler =
      (x, y) => x | y;

    public static readonly OperatorHandler<long> BinaryXOrHandler =
      (x, y) => x & ~y;

    public static readonly OperatorHandler<long> BinaryAndHandler =
      (x, y) => x & y;

    public static readonly OperatorHandler<long> PlusHandler =
      (x, y) => x + y;

    public static readonly OperatorHandler<long> MinusHandler =
      (x, y) => x - y;

    public static readonly OperatorHandler<long> DivideHandler =
      (x, y) => x / y;

    public static readonly OperatorHandler<long> MultiHandler =
      (x, y) => x * y;

    public static readonly Dictionary<string, OperatorHandler<long>> IntOperators =
      new Dictionary<string, OperatorHandler<long>>();

    static StringParser()
    {
      IntOperators["||"] = BinaryOrHandler;
      IntOperators["|"] = BinaryOrHandler;
      IntOperators["^"] = BinaryXOrHandler;
      IntOperators["&"] = BinaryAndHandler;
      IntOperators["+"] = PlusHandler;
      IntOperators["-"] = MinusHandler;
      IntOperators["*"] = DivideHandler;
      IntOperators["/"] = MultiHandler;
    }

    public static object Parse(string stringVal, Type type)
    {
      object obj = null;
      if(!Parse(stringVal, type, ref obj))
        throw new Exception(string.Format("Unable to parse string-Value \"{0}\" as Type \"{1}\"",
          stringVal, type));
      return obj;
    }

    public static bool Parse(string str, Type type, ref object obj)
    {
      if(!type.IsArray)
        return ParseSingleValue(str, type, ref obj);
      StringStream stringStream = new StringStream(str);
      int length = 0;
      while(stringStream.HasNext)
      {
        ++length;
        string result;
        if(!stringStream.NextString(out result, ","))
          break;
      }

      stringStream.Position = 0;
      Type elementType = type.GetElementType();
      Array instance = Array.CreateInstance(elementType, length);
      for(int index = 0; index < length; ++index)
      {
        object obj1 = null;
        string result;
        stringStream.NextString(out result, ",");
        if(!ParseSingleValue(result, elementType, ref obj1))
          return false;
        instance.SetValue(obj1, index);
      }

      obj = instance;
      return true;
    }

    public static bool ParseSingleValue(string str, Type type, ref object obj)
    {
      if(type == typeof(string))
        obj = str;
      else if(type.IsEnum)
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
        if(!TypeParsers.TryGetValue(type, out func))
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
      string[] strArray = expr.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
      bool flag = startsWithOperator;
      OperatorHandler<long> operatorHandler = null;
      foreach(string str1 in strArray)
      {
        string str2 = str1.Trim();
        if(flag)
        {
          if(!IntOperators.TryGetValue(str2, out operatorHandler))
          {
            error = "Invalid operator: " + str2;
            return false;
          }
        }
        else
        {
          object obj = null;
          if(!Parse(str2, valType, ref obj))
          {
            error = "Could not convert value \"" + str2 + "\" to Type \"" + valType +
                    "\"";
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