using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GGemu
{
    class Program
    {
        static TcpListener listener = new TcpListener(IPAddress.Any, 80);
        static byte[] data = new byte[10000];
       
        static void Main(string[] args)
        {
            File.WriteAllText("en.txt",_data.AsString());
            File.WriteAllText("ru.txt", _dataRus.AsString());
        }

        private static byte[] _data = File.ReadAllBytes("response.dat");
        private static byte[] _dataRus = File.ReadAllBytes("rusresponse.dat");

    }
    public static class DataConvertionHelpers
    {
        private static StringBuilder _sb = new StringBuilder();

        /// <summary>
        /// Converts hex string to byte sequence.
        /// </summary>
        /// <exception cref="ArgumentException">String contains wrong Symbol.</exception>
        /// <param name="data">Hex string.</param>
        /// <returns>Byte sequence.</returns>
        public static byte[] AsBytes(this string data)
        {

            if (data == null) throw new ArgumentNullException("data");
            var bytesResult = new List<byte>();
            var index = 0;
            var seq = data.Where(IsHexDight).ToArray();
            while (index < seq.Length)
            {
                try
                {
                    char[] twoDights = seq.Skip(index).Take(2).ToArray();
                    string curByteStr = twoDights[0].ToString() + twoDights[1].ToString();
                    byte curByte;
                    if (byte.TryParse(curByteStr, NumberStyles.AllowHexSpecifier, null, out curByte))
                        bytesResult.Add(curByte);
                }
                catch {}
                index += 2;
            }
            return bytesResult.ToArray();
        }

        private static bool IsHexDight(char c)
        {

            if ("0123456789ABCDEF".Contains(c))
                return true;
            if (c == ' ' || c == '\n')
                return false;
            throw new Exception("String contains wrong symbol : " + c);
        }

        public static string AsString(this IEnumerable<Byte> data)
        {
            if (data == null || data.Count() == 0)
                return "";
            var resultStr = data.Aggregate("", (current, b) => current + (b.ToString("X2") + " "));
            return resultStr.Substring(0, resultStr.Length - 1);
        }

        public static UInt32 GetUInt32FromByteArrayInversion(this IList<byte> data, int index)
        {
            return
                UInt32.Parse(
                    data[index].ToString("X2") + data[index + 1].ToString("X2") + data[index + 2].ToString("X2") +
                    data[index + 3].ToString("X2"), NumberStyles.AllowHexSpecifier);
        }
    }
}
