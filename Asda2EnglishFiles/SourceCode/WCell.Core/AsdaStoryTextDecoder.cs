using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Core
{
    public class AsdaStoryTextDecoder
    {
        private static readonly char[] RuChars =
            "йцукенгшщзхъфывапролджэячсмитьбюЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮ".ToArray<char>();

        private static readonly byte[] RuEncode =
            "E9 F6 F3 EA E5 ED E3 F8 F9 E7 F5 FA F4 FB E2 E0 EF F0 EE EB E4 E6 FD FF F7 F1 EC E8 F2 FC E1 FE C9 D6 D3 CA C5 CD C3 D8 D9 C7 D5 DA D4 DB C2 C0 CF D0 CE CB C4 C6 DD DF D7 D1 CC C8 D2 DC C1 DE"
                .AsBytes();

        private static readonly ByteBuffer EncodeData = new ByteBuffer(1024);

        public static string GetString(byte[] data, int startIndex, int count)
        {
            byte[] msg = new byte[count];
            Array.Copy((Array) data, startIndex, (Array) msg, 0, count);
            return AsdaStoryTextDecoder.Decode(msg);
        }

        public static byte[] GetBytes(string msg)
        {
            return AsdaStoryTextDecoder.Encode(msg);
        }

        public static byte[] Encode(string msg)
        {
            if (msg == null)
                return new byte[0];
            AsdaStoryTextDecoder.EncodeData.SetIndex(0);
            foreach (char ch in msg)
            {
                switch (ch)
                {
                    case '0':
                        AsdaStoryTextDecoder.EncodeData.WriteByte((byte) 48);
                        break;
                    case '1':
                        AsdaStoryTextDecoder.EncodeData.WriteByte((byte) 49);
                        break;
                    case '2':
                        AsdaStoryTextDecoder.EncodeData.WriteByte((byte) 50);
                        break;
                    case '3':
                        AsdaStoryTextDecoder.EncodeData.WriteByte((byte) 51);
                        break;
                    case '4':
                        AsdaStoryTextDecoder.EncodeData.WriteByte((byte) 52);
                        break;
                    case '5':
                        AsdaStoryTextDecoder.EncodeData.WriteByte((byte) 53);
                        break;
                    case '6':
                        AsdaStoryTextDecoder.EncodeData.WriteByte((byte) 54);
                        break;
                    case '7':
                        AsdaStoryTextDecoder.EncodeData.WriteByte((byte) 55);
                        break;
                    case '8':
                        AsdaStoryTextDecoder.EncodeData.WriteByte((byte) 56);
                        break;
                    case '9':
                        AsdaStoryTextDecoder.EncodeData.WriteByte((byte) 57);
                        break;
                    case ':':
                        AsdaStoryTextDecoder.EncodeData.WriteByte((byte) 58);
                        break;
                    default:
                        if (((IEnumerable<char>) AsdaStoryTextDecoder.RuChars).Contains<char>(ch))
                        {
                            int index = Array.IndexOf<char>(AsdaStoryTextDecoder.RuChars, ch);
                            AsdaStoryTextDecoder.EncodeData.WriteByte(AsdaStoryTextDecoder.RuEncode[index]);
                            break;
                        }

                        AsdaStoryTextDecoder.EncodeData.WriteByte(Encoding.ASCII.GetBytes(new char[1]
                        {
                            ch
                        })[0]);
                        break;
                }
            }

            return AsdaStoryTextDecoder.EncodeData.Get_UsefullDataByteArray();
        }

        public static string Decode(byte[] msg)
        {
            string str = "";
            foreach (byte num in msg)
            {
                switch (num)
                {
                    case 48:
                        str += "0";
                        break;
                    case 49:
                        str += "1";
                        break;
                    case 50:
                        str += "2";
                        break;
                    case 51:
                        str += "3";
                        break;
                    case 52:
                        str += "4";
                        break;
                    case 53:
                        str += "5";
                        break;
                    case 54:
                        str += "6";
                        break;
                    case 55:
                        str += "7";
                        break;
                    case 56:
                        str += "8";
                        break;
                    case 57:
                        str += "9";
                        break;
                    case 58:
                        str += ":";
                        break;
                    case 65:
                        str += "A";
                        break;
                    case 66:
                        str += "B";
                        break;
                    case 67:
                        str += "C";
                        break;
                    case 68:
                        str += "D";
                        break;
                    case 69:
                        str += "E";
                        break;
                    case 70:
                        str += "F";
                        break;
                    case 71:
                        str += "G";
                        break;
                    case 72:
                        str += "H";
                        break;
                    case 73:
                        str += "I";
                        break;
                    case 74:
                        str += "J";
                        break;
                    case 75:
                        str += "K";
                        break;
                    case 76:
                        str += "L";
                        break;
                    case 77:
                        str += "M";
                        break;
                    case 78:
                        str += "N";
                        break;
                    case 79:
                        str += "O";
                        break;
                    case 80:
                        str += "P";
                        break;
                    case 81:
                        str += "Q";
                        break;
                    case 82:
                        str += "R";
                        break;
                    case 83:
                        str += "S";
                        break;
                    case 84:
                        str += "T";
                        break;
                    case 85:
                        str += "U";
                        break;
                    case 86:
                        str += "V";
                        break;
                    case 87:
                        str += "W";
                        break;
                    case 88:
                        str += "X";
                        break;
                    case 89:
                        str += "Y";
                        break;
                    case 90:
                        str += "Z";
                        break;
                    case 97:
                        str += "a";
                        break;
                    case 98:
                        str += "b";
                        break;
                    case 99:
                        str += "c";
                        break;
                    case 100:
                        str += "d";
                        break;
                    case 101:
                        str += "e";
                        break;
                    case 102:
                        str += "f";
                        break;
                    case 103:
                        str += "g";
                        break;
                    case 104:
                        str += "h";
                        break;
                    case 105:
                        str += "i";
                        break;
                    case 106:
                        str += "j";
                        break;
                    case 107:
                        str += "k";
                        break;
                    case 108:
                        str += "l";
                        break;
                    case 109:
                        str += "m";
                        break;
                    case 110:
                        str += "n";
                        break;
                    case 111:
                        str += "o";
                        break;
                    case 112:
                        str += "p";
                        break;
                    case 113:
                        str += "q";
                        break;
                    case 114:
                        str += "r";
                        break;
                    case 115:
                        str += "s";
                        break;
                    case 116:
                        str += "t";
                        break;
                    case 117:
                        str += "u";
                        break;
                    case 118:
                        str += "v";
                        break;
                    case 119:
                        str += "w";
                        break;
                    case 120:
                        str += "x";
                        break;
                    case 121:
                        str += "y";
                        break;
                    case 122:
                        str += "z";
                        break;
                    case 168:
                        str += "Ё";
                        break;
                    case 184:
                        str += "ё";
                        break;
                    case 192:
                        str += "А";
                        break;
                    case 193:
                        str += "Б";
                        break;
                    case 194:
                        str += "В";
                        break;
                    case 195:
                        str += "Г";
                        break;
                    case 196:
                        str += "Д";
                        break;
                    case 197:
                        str += "Е";
                        break;
                    case 198:
                        str += "Ж";
                        break;
                    case 199:
                        str += "З";
                        break;
                    case 200:
                        str += "И";
                        break;
                    case 201:
                        str += "Й";
                        break;
                    case 202:
                        str += "К";
                        break;
                    case 203:
                        str += "Л";
                        break;
                    case 204:
                        str += "М";
                        break;
                    case 205:
                        str += "Н";
                        break;
                    case 206:
                        str += "О";
                        break;
                    case 207:
                        str += "П";
                        break;
                    case 208:
                        str += "Р";
                        break;
                    case 209:
                        str += "С";
                        break;
                    case 210:
                        str += "Т";
                        break;
                    case 211:
                        str += "У";
                        break;
                    case 212:
                        str += "Ф";
                        break;
                    case 213:
                        str += "Х";
                        break;
                    case 214:
                        str += "Ц";
                        break;
                    case 215:
                        str += "Ч";
                        break;
                    case 216:
                        str += "Ш";
                        break;
                    case 217:
                        str += "Щ";
                        break;
                    case 218:
                        str += "Ъ";
                        break;
                    case 219:
                        str += "Ы";
                        break;
                    case 220:
                        str += "Ь";
                        break;
                    case 221:
                        str += "Э";
                        break;
                    case 222:
                        str += "Ю";
                        break;
                    case 223:
                        str += "Я";
                        break;
                    case 224:
                        str += "а";
                        break;
                    case 225:
                        str += "б";
                        break;
                    case 226:
                        str += "в";
                        break;
                    case 227:
                        str += "г";
                        break;
                    case 228:
                        str += "д";
                        break;
                    case 229:
                        str += "е";
                        break;
                    case 230:
                        str += "ж";
                        break;
                    case 231:
                        str += "з";
                        break;
                    case 232:
                        str += "и";
                        break;
                    case 233:
                        str += "й";
                        break;
                    case 234:
                        str += "к";
                        break;
                    case 235:
                        str += "л";
                        break;
                    case 236:
                        str += "м";
                        break;
                    case 237:
                        str += "н";
                        break;
                    case 238:
                        str += "о";
                        break;
                    case 239:
                        str += "п";
                        break;
                    case 240:
                        str += "р";
                        break;
                    case 241:
                        str += "с";
                        break;
                    case 242:
                        str += "т";
                        break;
                    case 243:
                        str += "у";
                        break;
                    case 244:
                        str += "ф";
                        break;
                    case 245:
                        str += "х";
                        break;
                    case 246:
                        str += "ц";
                        break;
                    case 247:
                        str += "ч";
                        break;
                    case 248:
                        str += "ш";
                        break;
                    case 249:
                        str += "щ";
                        break;
                    case 250:
                        str += "ъ";
                        break;
                    case 251:
                        str += "ы";
                        break;
                    case 252:
                        str += "ь";
                        break;
                    case 253:
                        str += "э";
                        break;
                    case 254:
                        str += "ю";
                        break;
                    case byte.MaxValue:
                        str += "я";
                        break;
                    default:
                        str += "?";
                        break;
                }
            }

            return str;
        }
    }
}