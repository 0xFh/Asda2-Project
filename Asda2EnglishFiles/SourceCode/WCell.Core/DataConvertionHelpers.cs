using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace WCell.Core
{
    public static class DataConvertionHelpers
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static StringBuilder _sb = new StringBuilder();

        /// <summary>Converts hex string to byte sequence.</summary>
        /// <exception cref="T:System.ArgumentException">String contains wrong Symbol.</exception>
        /// <param name="data">Hex string.</param>
        /// <returns>Byte sequence.</returns>
        public static byte[] AsBytes(this string data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            List<byte> byteList = new List<byte>();
            int count = 0;
            char[] array1 = data.Where<char>(new Func<char, bool>(DataConvertionHelpers.IsHexDight)).ToArray<char>();
            while (count < array1.Length)
            {
                try
                {
                    char[] array2 = ((IEnumerable<char>) array1).Skip<char>(count).Take<char>(2).ToArray<char>();
                    byte result;
                    if (byte.TryParse(array2[0].ToString() + array2[1].ToString(), NumberStyles.AllowHexSpecifier,
                        (IFormatProvider) null, out result))
                        byteList.Add(result);
                }
                catch
                {
                    DataConvertionHelpers.Logger.Warn("Packet content is wrong. Cannot parse it to data. {0}", data);
                }

                count += 2;
            }

            return byteList.ToArray();
        }

        private static bool IsHexDight(char c)
        {
            if ("0123456789ABCDEF".Contains<char>(c))
                return true;
            if (c == ' ' || c == '\n')
                return false;
            throw new Exception("String contains wrong symbol : " + (object) c);
        }

        public static string AsString(this IEnumerable<byte> data)
        {
            if (data == null || data.Count<byte>() == 0)
                return "";
            string str = data.Aggregate<byte, string>("",
                (Func<string, byte, string>) ((current, b) => current + b.ToString("X2") + " "));
            return str.Substring(0, str.Length - 1);
        }

        public static uint GetUInt32FromByteArrayInversion(this IList<byte> data, int index)
        {
            byte num = data[index];
            string str1 = num.ToString("X2");
            num = data[index + 1];
            string str2 = num.ToString("X2");
            num = data[index + 2];
            string str3 = num.ToString("X2");
            num = data[index + 3];
            string str4 = num.ToString("X2");
            return uint.Parse(str1 + str2 + str3 + str4, NumberStyles.AllowHexSpecifier);
        }
    }
}