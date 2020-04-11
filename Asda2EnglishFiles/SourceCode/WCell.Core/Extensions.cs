using System.IO;
using System.Net;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Core
{
    public static class Extensions
    {
        /// <summary>
        /// Reads a C-style null-terminated string from the current stream.
        /// </summary>
        /// <param name="binReader">the extended <see cref="T:System.IO.BinaryReader" /> instance</param>
        /// <returns>the string being reader</returns>
        public static string ReadCString(this BinaryReader binReader)
        {
            StringBuilder stringBuilder = new StringBuilder();
            byte num;
            while (binReader.BaseStream.Position < binReader.BaseStream.Length &&
                   (num = binReader.ReadByte()) != (byte) 0)
                stringBuilder.Append((char) num);
            return stringBuilder.ToString();
        }

        public static IPAddress ToIPAddress(this string szValue)
        {
            IPAddress address;
            IPAddress.TryParse(szValue, out address);
            return address ?? IPAddress.Any;
        }

        public static string GetHexCode(this Color color)
        {
            return color.R.ToString("X2") + color.B.ToString("X2") + color.G.ToString("X2");
        }

        public static bool IsBetween(this int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        public static bool IsBetween(this uint value, int min, int max)
        {
            return (long) value >= (long) min && (long) value <= (long) max;
        }
    }
}