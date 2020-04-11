using System;
using System.Linq;
using System.Text;

namespace WCell.Core
{
    public class AsdaStoryTextDecoder
    {
        private static readonly char[] RuChars =
            "йцукенгшщзхъфывапролджэячсмитьбюЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮ".ToArray();

        private static readonly byte[] RuEncode =
            "E9 F6 F3 EA E5 ED E3 F8 F9 E7 F5 FA F4 FB E2 E0 EF F0 EE EB E4 E6 FD FF F7 F1 EC E8 F2 FC E1 FE C9 D6 D3 CA C5 CD C3 D8 D9 C7 D5 DA D4 DB C2 C0 CF D0 CE CB C4 C6 DD DF D7 D1 CC C8 D2 DC C1 DE"
                .AsBytes();

        private static readonly ByteBuffer EncodeData = new ByteBuffer(1024);

        public static string GetString(byte[] data,int startIndex, int count)
        {
            var d = new byte[count];
            Array.Copy(data, startIndex, d, 0, count);
            return Decode(d);
        }
        public static byte[] GetBytes(string msg)
        {
            return Encode(msg);
        }
        public static byte[] Encode(string msg)
        {
            if(msg == null)
                return new byte[0];
            EncodeData.SetIndex(0);
            foreach (char c in msg)
            {
                switch (c)
                {
                    case '1':
                        EncodeData.WriteByte(0x31);
                        break;
                    case '2':
                        EncodeData.WriteByte(0x32);
                        break;
                    case '3':
                        EncodeData.WriteByte(0x33);
                        break;
                    case '4':
                        EncodeData.WriteByte(0x34);
                        break;
                    case '5':
                        EncodeData.WriteByte(0x35);
                        break;
                    case '6':
                        EncodeData.WriteByte(0x36);
                        break;
                    case '7':
                        EncodeData.WriteByte(0x37);
                        break;
                    case '8':
                        EncodeData.WriteByte(0x38);
                        break;
                    case '9':
                        EncodeData.WriteByte(0x39);
                        break;
                    case '0':
                        EncodeData.WriteByte(0x30);
                        break;
                    case ':':
                        EncodeData.WriteByte(0x3A);
                        break;
                    default:
                        if (RuChars.Contains(c))
                        {
                            int indexOfChar = Array.IndexOf(RuChars, c);
                            EncodeData.WriteByte(RuEncode[indexOfChar]);
                        }
                        else EncodeData.WriteByte(Encoding.ASCII.GetBytes(new[] {c})[0]);
                        break;
                }
            }
            return EncodeData.Get_UsefullDataByteArray();
        }

        public static string Decode(byte[] msg)
        {
            string decodedMsg = "";
            foreach (byte b in msg)
            {
                switch (b)
                {
                        #region parse

                        //й  ц  у  к  е  н  г  ш  щ  з  х  ъ  ф  ы  в  а  п  р  о  л  д  ж  э  я  ч  с  м  и  т  ь  б  ю
                        //E9 F6 F3 EA E5 ED E3 F8 F9 E7 F5 FA F4 FB E2 E0 EF F0 EE EB E4 E6 FD FF F7 F1 EC E8 F2 FC E1 FE
                        //q  w  e  r  t  y  u  i  o  p  a  s  d  f  g  h  j  k  l  z  x  c  v  b  n  m
                        //71 77 65 72 74 79 75 69 6F 70 61 73 64 66 67 68 6A 6B 6C 7A 78 63 76 62 6E 6D 
                        //1  2  3  4  5  6  7  8  9  0
                        //31 32 5A 34 35 36 37 38 39 30              
                        //Й  Ц  У  К  Е  Н  Г  Ш  Щ  З  Х  Ъ  Ф  Ы  В  А  П  Р  О  Л  Д  Ж  Э  Я  Ч  С  М  И  Т  Ь  Б  Ю
                        //C9 D6 D3 CA C5 CD C3 D8 D9 C7 D5 DA D4 DB C2 C0 CF D0 CE CB C4 C6 DD DF D7 D1 CC C8 D2 DC C1 DE
                        //Q  W  E  R  T  Y  U  I  O  P  A  S  D  F  G  H  J  K  L  Z  X  C  V  B  N  M
                        //51 57 45 52 54 59 55 49 4F 50 41 53 44 46 47 48 4A 4B 4C 5A 58 43 56 42 4E 4D 

                    default:
                        decodedMsg += "?";
                        break;
                    case 0xB8:
                        decodedMsg += "ё";
                        break;
                    case 0xA8:
                        decodedMsg += "Ё";
                        break;
                    case 0x3A:
                        decodedMsg += ":";
                        break;
                    case 0xE9:
                        decodedMsg += "й";
                        break;
                    case 0xF6:
                        decodedMsg += "ц";
                        break;
                    case 0xF3:
                        decodedMsg += "у";
                        break;
                    case 0xEA:
                        decodedMsg += "к";
                        break;
                    case 0xE5:
                        decodedMsg += "е";
                        break;
                    case 0xED:
                        decodedMsg += "н";
                        break;
                    case 0xE3:
                        decodedMsg += "г";
                        break;
                    case 0xF8:
                        decodedMsg += "ш";
                        break;
                    case 0xF9:
                        decodedMsg += "щ";
                        break;
                    case 0xE7:
                        decodedMsg += "з";
                        break;
                    case 0xF5:
                        decodedMsg += "х";
                        break;
                    case 0xFA:
                        decodedMsg += "ъ";
                        break;
                    case 0xF4:
                        decodedMsg += "ф";
                        break;
                    case 0xFB:
                        decodedMsg += "ы";
                        break;
                    case 0xE2:
                        decodedMsg += "в";
                        break;
                    case 0xE0:
                        decodedMsg += "а";
                        break;
                    case 0xEF:
                        decodedMsg += "п";
                        break;
                    case 0xF0:
                        decodedMsg += "р";
                        break;
                    case 0xEE:
                        decodedMsg += "о";
                        break;
                    case 0xEB:
                        decodedMsg += "л";
                        break;
                    case 0xE4:
                        decodedMsg += "д";
                        break;
                    case 0xE6:
                        decodedMsg += "ж";
                        break;
                    case 0xFD:
                        decodedMsg += "э";
                        break;
                    case 0xFF:
                        decodedMsg += "я";
                        break;
                    case 0xF7:
                        decodedMsg += "ч";
                        break;
                    case 0xF1:
                        decodedMsg += "с";
                        break;
                    case 0xEC:
                        decodedMsg += "м";
                        break;
                    case 0xE8:
                        decodedMsg += "и";
                        break;
                    case 0xF2:
                        decodedMsg += "т";
                        break;
                    case 0xFC:
                        decodedMsg += "ь";
                        break;
                    case 0xE1:
                        decodedMsg += "б";
                        break;
                    case 0xFE:
                        decodedMsg += "ю";
                        break;
                    case 0x71:
                        decodedMsg += "q";
                        break;
                    case 0x77:
                        decodedMsg += "w";
                        break;
                    case 0x65:
                        decodedMsg += "e";
                        break;
                    case 0x72:
                        decodedMsg += "r";
                        break;
                    case 0x74:
                        decodedMsg += "t";
                        break;
                    case 0x79:
                        decodedMsg += "y";
                        break;
                    case 0x75:
                        decodedMsg += "u";
                        break;
                    case 0x69:
                        decodedMsg += "i";
                        break;
                    case 0x6F:
                        decodedMsg += "o";
                        break;
                    case 0x70:
                        decodedMsg += "p";
                        break;
                    case 0x61:
                        decodedMsg += "a";
                        break;
                    case 0x73:
                        decodedMsg += "s";
                        break;
                    case 0x64:
                        decodedMsg += "d";
                        break;
                    case 0x66:
                        decodedMsg += "f";
                        break;
                    case 0x67:
                        decodedMsg += "g";
                        break;
                    case 0x68:
                        decodedMsg += "h";
                        break;
                    case 0x6A:
                        decodedMsg += "j";
                        break;
                    case 0x6B:
                        decodedMsg += "k";
                        break;
                    case 0x6C:
                        decodedMsg += "l";
                        break;
                    case 0x7A:
                        decodedMsg += "z";
                        break;
                    case 0x78:
                        decodedMsg += "x";
                        break;
                    case 0x63:
                        decodedMsg += "c";
                        break;
                    case 0x76:
                        decodedMsg += "v";
                        break;
                    case 0x62:
                        decodedMsg += "b";
                        break;
                    case 0x6E:
                        decodedMsg += "n";
                        break;
                    case 0x6D:
                        decodedMsg += "m";
                        break;
                    case 0x31:
                        decodedMsg += "1";
                        break;
                    case 0x32:
                        decodedMsg += "2";
                        break;
                    case 0x33:
                        decodedMsg += "3";
                        break;
                    case 0x34:
                        decodedMsg += "4";
                        break;
                    case 0x35:
                        decodedMsg += "5";
                        break;
                    case 0x36:
                        decodedMsg += "6";
                        break;
                    case 0x37:
                        decodedMsg += "7";
                        break;
                    case 0x38:
                        decodedMsg += "8";
                        break;
                    case 0x39:
                        decodedMsg += "9";
                        break;
                    case 0x30:
                        decodedMsg += "0";
                        break;
                    case 0xC9:
                        decodedMsg += "Й";
                        break;
                    case 0xD6:
                        decodedMsg += "Ц";
                        break;
                    case 0xD3:
                        decodedMsg += "У";
                        break;
                    case 0xCA:
                        decodedMsg += "К";
                        break;
                    case 0xC5:
                        decodedMsg += "Е";
                        break;
                    case 0xCD:
                        decodedMsg += "Н";
                        break;
                    case 0xC3:
                        decodedMsg += "Г";
                        break;
                    case 0xD8:
                        decodedMsg += "Ш";
                        break;
                    case 0xD9:
                        decodedMsg += "Щ";
                        break;
                    case 0xC7:
                        decodedMsg += "З";
                        break;
                    case 0xD5:
                        decodedMsg += "Х";
                        break;
                    case 0xDA:
                        decodedMsg += "Ъ";
                        break;
                    case 0xD4:
                        decodedMsg += "Ф";
                        break;
                    case 0xDB:
                        decodedMsg += "Ы";
                        break;
                    case 0xC2:
                        decodedMsg += "В";
                        break;
                    case 0xC0:
                        decodedMsg += "А";
                        break;
                    case 0xCF:
                        decodedMsg += "П";
                        break;
                    case 0xD0:
                        decodedMsg += "Р";
                        break;
                    case 0xCE:
                        decodedMsg += "О";
                        break;
                    case 0xCB:
                        decodedMsg += "Л";
                        break;
                    case 0xC4:
                        decodedMsg += "Д";
                        break;
                    case 0xC6:
                        decodedMsg += "Ж";
                        break;
                    case 0xDD:
                        decodedMsg += "Э";
                        break;
                    case 0xDF:
                        decodedMsg += "Я";
                        break;
                    case 0xD7:
                        decodedMsg += "Ч";
                        break;
                    case 0xD1:
                        decodedMsg += "С";
                        break;
                    case 0xCC:
                        decodedMsg += "М";
                        break;
                    case 0xC8:
                        decodedMsg += "И";
                        break;
                    case 0xD2:
                        decodedMsg += "Т";
                        break;
                    case 0xDC:
                        decodedMsg += "Ь";
                        break;
                    case 0xC1:
                        decodedMsg += "Б";
                        break;
                    case 0xDE:
                        decodedMsg += "Ю";
                        break;
                    case 0x51:
                        decodedMsg += "Q";
                        break;
                    case 0x57:
                        decodedMsg += "W";
                        break;
                    case 0x45:
                        decodedMsg += "E";
                        break;
                    case 0x52:
                        decodedMsg += "R";
                        break;
                    case 0x54:
                        decodedMsg += "T";
                        break;
                    case 0x59:
                        decodedMsg += "Y";
                        break;
                    case 0x55:
                        decodedMsg += "U";
                        break;
                    case 0x49:
                        decodedMsg += "I";
                        break;
                    case 0x4F:
                        decodedMsg += "O";
                        break;
                    case 0x50:
                        decodedMsg += "P";
                        break;
                    case 0x41:
                        decodedMsg += "A";
                        break;
                    case 0x53:
                        decodedMsg += "S";
                        break;
                    case 0x44:
                        decodedMsg += "D";
                        break;
                    case 0x46:
                        decodedMsg += "F";
                        break;
                    case 0x47:
                        decodedMsg += "G";
                        break;
                    case 0x48:
                        decodedMsg += "H";
                        break;
                    case 0x4A:
                        decodedMsg += "J";
                        break;
                    case 0x4B:
                        decodedMsg += "K";
                        break;
                    case 0x4C:
                        decodedMsg += "L";
                        break;
                    case 0x5A:
                        decodedMsg += "Z";
                        break;
                    case 0x58:
                        decodedMsg += "X";
                        break;
                    case 0x43:
                        decodedMsg += "C";
                        break;
                    case 0x56:
                        decodedMsg += "V";
                        break;
                    case 0x42:
                        decodedMsg += "B";
                        break;
                    case 0x4E:
                        decodedMsg += "N";
                        break;
                    case 0x4D:
                        decodedMsg += "M";
                        break;

                        #endregion
                }
            }
            return decodedMsg;
        }
    }
}