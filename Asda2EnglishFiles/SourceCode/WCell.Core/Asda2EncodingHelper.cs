using System;
using System.Linq;
using System.Text;
using WCell.Core.Network;

namespace WCell.Core
{
    public static class Asda2EncodingHelper
    {
        private static readonly char[] RuChars =
            "йцукенгшщзхъфывапролджэячсмитьбюЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮёЁ".ToArray<char>();

        private static readonly char[] EngTranslitChars =
            "ycukeng#%zh'f@vaproldj394smit[bwYCUKENG#%ZH]F@VAPROLDJ394SMIT'BW<<".ToArray<char>();

        private static readonly byte[] RuEncode =
            "E9 F6 F3 EA E5 ED E3 F8 F9 E7 F5 FA F4 FB E2 E0 EF F0 EE EB E4 E6 FD FF F7 F1 EC E8 F2 FC E1 FE C9 D6 D3 CA C5 CD C3 D8 D9 C7 D5 DA D4 DB C2 C0 CF D0 CE CB C4 C6 DD DF D7 D1 CC C8 D2 DC C1 DE B8 A8"
                .AsBytes();

        private static readonly byte[] RuEncodeTranslit = Encoding.ASCII.GetBytes(Asda2EncodingHelper.EngTranslitChars);
        public static char[] RuCharacters = new char[256];
        public static byte[] RuCharactersReversed = new byte[(int) ushort.MaxValue];
        public static byte[] RuCharactersReversedTranslit = new byte[(int) ushort.MaxValue];
        public static char[] ForReverseTranslit = new char[(int) ushort.MaxValue];
        public static bool[] AllowedEnglishSymbols = new bool[(int) ushort.MaxValue];
        public static bool[] AllowedEnglishNameSymbols = new bool[(int) ushort.MaxValue];

        public static string AllowedEnglishSymbolsStr =
            "`1234567890-=qwertyuiop[]\asdfghjkl;'zxcvbnm,./~!@#$%^&*()_+QWERTYUIOP{}ASDFGHJKL:\"ZXCVBNM<>?; ";

        public static string AllowedEnglishNameSymbolsStr =
            "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";

        static Asda2EncodingHelper()
        {
            for (int index = 0; index < 256; ++index)
                Asda2EncodingHelper.RuCharacters[index] = (char) index;
            for (int index = 0; index < Asda2EncodingHelper.RuEncode.Length; ++index)
                Asda2EncodingHelper.RuCharacters[(int) Asda2EncodingHelper.RuEncode[index]] =
                    Asda2EncodingHelper.RuChars[index];
            for (int index = 0; index < Asda2EncodingHelper.RuCharactersReversed.Length; ++index)
            {
                if (index >= 256)
                {
                    Asda2EncodingHelper.RuCharactersReversed[index] = (byte) 63;
                    Asda2EncodingHelper.RuCharactersReversedTranslit[index] = (byte) 63;
                    Asda2EncodingHelper.ForReverseTranslit[index] = '?';
                }
                else
                {
                    Asda2EncodingHelper.RuCharactersReversed[index] = (byte) index;
                    Asda2EncodingHelper.RuCharactersReversedTranslit[index] = (byte) index;
                    Asda2EncodingHelper.ForReverseTranslit[index] = (char) index;
                }
            }

            for (int index = 0; index < Asda2EncodingHelper.RuChars.Length; ++index)
            {
                Asda2EncodingHelper.RuCharactersReversed[(int) Asda2EncodingHelper.RuChars[index]] =
                    Asda2EncodingHelper.RuEncode[index];
                Asda2EncodingHelper.RuCharactersReversedTranslit[(int) Asda2EncodingHelper.RuChars[index]] =
                    Asda2EncodingHelper.RuEncodeTranslit[index];
            }

            for (int index = 0; index < Asda2EncodingHelper.EngTranslitChars.Length; ++index)
                Asda2EncodingHelper.ForReverseTranslit[(int) Asda2EncodingHelper.EngTranslitChars[index]] =
                    Asda2EncodingHelper.RuChars[index];
            Asda2EncodingHelper.InitAllowedEnglishSymbols();
        }

        public static string Decode(byte[] data, Locale locale)
        {
            return Encoding.ASCII.GetString(data);
        }

        public static byte[] Encode(string s, Locale locale)
        {
            return Encoding.ASCII.GetBytes(s);
        }

        public static byte[] EncodeTranslit(string s)
        {
            byte[] numArray = new byte[s.Length];
            for (int index = 0; index < s.Length; ++index)
                numArray[index] = Asda2EncodingHelper.RuCharactersReversedTranslit[(int) s[index]];
            return numArray;
        }

        public static string Translit(string name)
        {
            char[] chArray = new char[name.Length];
            for (int index = 0; index < name.Length; ++index)
                chArray[index] = (char) Asda2EncodingHelper.RuCharactersReversedTranslit[(int) name[index]];
            return new string(chArray);
        }

        public static string ReverseTranslit(string name)
        {
            char[] chArray = new char[name.Length];
            for (int index = 0; index < name.Length; ++index)
                chArray[index] = Asda2EncodingHelper.ForReverseTranslit[(int) name[index]];
            return new string(chArray);
        }

        private static void InitAllowedEnglishSymbols()
        {
            foreach (char ch in Asda2EncodingHelper.AllowedEnglishSymbolsStr)
                Asda2EncodingHelper.AllowedEnglishSymbols[(int) ch] = true;
            foreach (char ch in Asda2EncodingHelper.AllowedEnglishNameSymbolsStr)
                Asda2EncodingHelper.AllowedEnglishNameSymbols[(int) ch] = true;
        }

        public static bool IsPrueEnglish(string s)
        {
            return s.All<char>((Func<char, bool>) (c => Asda2EncodingHelper.AllowedEnglishSymbols[(int) c]));
        }

        public static bool IsPrueEnglishName(string s)
        {
            return s.All<char>((Func<char, bool>) (c => Asda2EncodingHelper.AllowedEnglishNameSymbols[(int) c]));
        }

        public static Locale MinimumAvailableLocale(Locale clientLocale, string message)
        {
            bool flag = Asda2EncodingHelper.IsPrueEnglish(message);
            Locale locale = Locale.Start;
            if (!flag)
                locale = clientLocale;
            return locale;
        }
    }
}