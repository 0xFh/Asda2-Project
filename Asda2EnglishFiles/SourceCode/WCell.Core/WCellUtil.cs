using System.Text;
using WCell.Core.Network;

namespace WCell.Core
{
    public static class WCellUtil
    {
        /// <summary>
        /// Dumps the array to string form, using hexadecimal as the formatter
        /// </summary>
        /// <returns>hexadecimal representation of the data parsed</returns>
        public static string ToHex(PacketId packetId, byte[] arr, int start, int count)
        {
            StringBuilder stringBuilder1 = new StringBuilder();
            stringBuilder1.Append('\n');
            stringBuilder1.Append("{SERVER} " + string.Format(
                                      "Packet: ({0}) {1} PacketSize = {2}\n|------------------------------------------------|----------------|\n|00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F |0123456789ABCDEF|\n|------------------------------------------------|----------------|\n",
                                      (object) ("0x" + ((short) packetId.RawId).ToString("X4")), (object) packetId,
                                      (object) count));
            int num1 = start + count;
            int num2 = start;
            while (num2 < num1)
            {
                StringBuilder stringBuilder2 = new StringBuilder();
                StringBuilder stringBuilder3 = new StringBuilder();
                stringBuilder3.Append("|");
                for (int index = 0; index < 16; ++index)
                {
                    if (index + num2 < num1)
                    {
                        byte num3 = arr[index + num2];
                        stringBuilder3.Append(arr[index + num2].ToString("X2"));
                        stringBuilder3.Append(" ");
                        if (num3 >= (byte) 32 && num3 <= (byte) 127)
                            stringBuilder2.Append((char) num3);
                        else
                            stringBuilder2.Append(".");
                    }
                    else
                    {
                        stringBuilder3.Append("   ");
                        stringBuilder2.Append(" ");
                    }
                }

                stringBuilder3.Append("|");
                stringBuilder3.Append(stringBuilder2.ToString() + "|");
                stringBuilder3.Append('\n');
                stringBuilder1.Append(stringBuilder3.ToString());
                num2 += 16;
            }

            stringBuilder1.Append("-------------------------------------------------------------------");
            return stringBuilder1.ToString();
        }

        public static string FormatBytes(float num)
        {
            string str = "B";
            if ((double) num >= 1024.0)
            {
                num /= 1024f;
                str = "kb";
            }

            if ((double) num >= 1024.0)
            {
                num /= 1024f;
                str = "MB";
            }

            if ((double) num >= 1024.0)
            {
                num /= 1024f;
                str = "GB";
            }

            return string.Format("{0,6:f}{1}", (object) num, (object) str);
        }

        public static string FormatBytes(double num)
        {
            string str = "B";
            if (num >= 1024.0)
            {
                num /= 1024.0;
                str = "kb";
            }

            if (num >= 1024.0)
            {
                num /= 1024.0;
                str = "MB";
            }

            if (num >= 1024.0)
            {
                num /= 1024.0;
                str = "GB";
            }

            return string.Format("{0,6:f}{1}", (object) num, (object) str);
        }
    }
}