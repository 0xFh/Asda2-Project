using System.Collections.Generic;
using System.Text;

namespace WCell.Util
{
    /// <summary>
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Combines the strings of a string array into one delimited string
        /// </summary>
        /// <param name="inputArray">The string array to combine</param>
        /// <param name="delimiter">The delimited</param>
        /// <returns>A string of the delimited strings</returns>
        public static string ToDelimitedString(this string[] inputArray, string delimiter)
        {
            if (inputArray.Length > 1)
                return string.Join(delimiter, inputArray);
            return inputArray[0];
        }

        /// <summary>
        /// Combines the strings of an List&lt;string&gt; into one delimited string
        /// </summary>
        /// <param name="inputArray">The List&lt;string&gt; to combine</param>
        /// <param name="delimiter">The delimited</param>
        /// <returns>A string of the delimited strings</returns>
        public static string ToDelimitedString(this List<string> inputArray, string delimiter)
        {
            if (inputArray.Count > 1)
                return string.Join(delimiter, inputArray.ToArray());
            return inputArray[0];
        }

        /// <summary>
        /// Combines the strings of a string array into a string, which resembles a list
        /// </summary>
        /// <param name="szArray">The string array to combine</param>
        /// <returns>A string which resembles a list, using commas, and follows the rules of English grammar</returns>
        public static string GetReadableList(this string[] szArray)
        {
            if (szArray.Length == 0)
                return "none";
            if (szArray.Length == 1)
                return szArray[0];
            string str = string.Join(";", szArray).Replace(";", ", ");
            int num = str.LastIndexOf(", ");
            return str.Insert(num + 2, "and ");
        }

        /// <summary>
        /// Safely splits a string without erroring if the delimiter is not present
        /// </summary>
        /// <param name="inputSz">The string to split</param>
        /// <param name="delimiter">The character to split on</param>
        /// <returns>A string array of the split string</returns>
        public static string[] Split(this string inputSz, char delimiter)
        {
            if (string.IsNullOrEmpty(inputSz))
                return new string[0];
            if (inputSz.IndexOf(delimiter) == -1)
                return new string[1] {inputSz};
            return inputSz.Split(delimiter);
        }

        /// <summary>Converts a byte array to a period-delimited string</summary>
        /// <param name="inputArray">the byte array to convert</param>
        /// <returns>a period-delimited string of the converted byte array</returns>
        public static string ToReadableIPAddress(this byte[] inputArray)
        {
            if (inputArray.Length != 4)
                return "not an IP address";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(inputArray[0].ToString()).Append('.');
            stringBuilder.Append(inputArray[1].ToString()).Append('.');
            stringBuilder.Append(inputArray[2].ToString()).Append('.');
            stringBuilder.Append(inputArray[3].ToString());
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Converts a random string (aBcDeFG) to a capitalized string (Abcdefg)
        /// </summary>
        public static string ToCapitalizedString(this string input)
        {
            if (input.Length == 0)
                return input;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(char.ToUpper(input[0]));
            for (int index = 1; index < input.Length; ++index)
                stringBuilder.Append(char.ToLower(input[index]));
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Capitalizes the string and also considers (and removes) special characters, such as "_"
        /// </summary>
        public static string ToFriendlyName(this string input)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = true;
            for (int index = 0; index < input.Length; ++index)
            {
                char c = input[index];
                if (c == '_')
                    c = ' ';
                char ch = flag ? char.ToUpper(c) : char.ToLower(c);
                flag = ch == ' ';
                stringBuilder.Append(ch);
            }

            return stringBuilder.ToString();
        }

        public static string ToCamelCase(this string input)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = true;
            for (int index = 0; index < input.Length; ++index)
            {
                char c = input[index];
                if (c == '_')
                    c = ' ';
                char ch = flag ? char.ToUpper(c) : char.ToLower(c);
                if (ch == ' ')
                {
                    flag = true;
                }
                else
                {
                    flag = false;
                    stringBuilder.Append(ch);
                }
            }

            return stringBuilder.ToString();
        }

        public static string Format(this string input, params object[] args)
        {
            return string.Format(input, args);
        }
    }
}