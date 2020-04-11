using System;
using System.Collections.Generic;
using System.Globalization;

namespace WCell.Util
{
    public static class EnumUtil
    {
        public static T Parse<T>(string input)
        {
            return (T) Enum.Parse(typeof(T), input);
        }

        /// <summary>TODO: Put big enums in dictionaries</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse<T>(string input, out T result)
        {
            if (input.Length > 0)
            {
                if (char.IsDigit(input[0]) || input[0] == '-' || input[0] == '+')
                {
                    Type underlyingType = Enum.GetUnderlyingType(typeof(T));
                    try
                    {
                        object obj = Convert.ChangeType((object) input, underlyingType,
                            (IFormatProvider) CultureInfo.InvariantCulture);
                        result = (T) Enum.ToObject(typeof(T), obj);
                        return true;
                    }
                    catch (FormatException ex)
                    {
                    }
                }
                else
                {
                    Dictionary<string, object> dictionary;
                    object obj;
                    if (Utility.EnumValueMap.TryGetValue(typeof(T), out dictionary) &&
                        dictionary.TryGetValue(input.Trim(), out obj))
                    {
                        result = (T) obj;
                        return true;
                    }
                }
            }

            result = default(T);
            return false;
        }
    }
}