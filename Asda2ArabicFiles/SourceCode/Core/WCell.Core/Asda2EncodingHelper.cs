using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Network;

namespace WCell.Core
{
    public static class Asda2EncodingHelper
    {
        private static readonly char[] RuChars =
            "йцукенгшщзхъфывапролджэячсмитьбюЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮёЁ".ToArray();

        private static readonly char[] EngTranslitChars =
            "ycukeng#%zh'f@vaproldj394smit[bwYCUKENG#%ZH]F@VAPROLDJ394SMIT'BW<<".ToArray();
        private static readonly byte[] RuEncode =
            "E9 F6 F3 EA E5 ED E3 F8 F9 E7 F5 FA F4 FB E2 E0 EF F0 EE EB E4 E6 FD FF F7 F1 EC E8 F2 FC E1 FE C9 D6 D3 CA C5 CD C3 D8 D9 C7 D5 DA D4 DB C2 C0 CF D0 CE CB C4 C6 DD DF D7 D1 CC C8 D2 DC C1 DE B8 A8"
                .AsBytes();

        private static readonly byte[] RuEncodeTranslit =
            Encoding.ASCII.GetBytes(EngTranslitChars);
        public static char[] RuCharacters = new char[256];
        public static byte[] RuCharactersReversed = new byte[ushort.MaxValue];
        public static byte[] RuCharactersReversedTranslit = new byte[ushort.MaxValue];
        public static char[] ForReverseTranslit = new char[ushort.MaxValue];
        static Asda2EncodingHelper()
        {
            for (int i = 0; i < 256; i++)
            {
                RuCharacters[i] = (char)i;
            }
            for (int i = 0; i < RuEncode.Length; i++)
            {
                RuCharacters[RuEncode[i]] = RuChars[i];
            }
            for (int i = 0; i < RuCharactersReversed.Length; i++)
            {
                if (i >= 256)
                {
                    RuCharactersReversed[i] = 63;
                    RuCharactersReversedTranslit[i] = 63;
                    ForReverseTranslit[i] = (char)63;
                    continue;
                }
                RuCharactersReversed[i] = (byte)i;
                RuCharactersReversedTranslit[i] = (byte)i;
                ForReverseTranslit[i] = (char)i;
            }
            for (int i = 0; i < RuChars.Length; i++)
            {
                RuCharactersReversed[RuChars[i]] = RuEncode[i];
                RuCharactersReversedTranslit[RuChars[i]] = RuEncodeTranslit[i];
            }
            for (int i = 0; i < EngTranslitChars.Length; i++)
            {
                ForReverseTranslit[EngTranslitChars[i]] = RuChars[i];
            }
            InitAllowedEnglishSymbols();
            InitAllowedArabicSymbols();
        }
        public static string Decode(byte[] data, Locale locale)
        {
            //todo encode messages to correct locale
            return Encoding.Default.GetString(data);
            var r = new char[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                r[i] = RuCharacters[data[i]];
            }
            return new string(r);
        }
        public static byte[] Encode(string s, Locale locale)
        {
            //todo encode messages to correct locale
            return Encoding.Default.GetBytes(s);
            var r = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                r[i] = RuCharactersReversed[s[i]];
            }
            return r;
        }
        public static byte[] EncodeTranslit(string s)
        {
            var r = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                r[i] = RuCharactersReversedTranslit[s[i]];
            }
            return r;
        }

        public static string Translit(string name)
        {
            var res = new char[name.Length];
            for (int i = 0; i < name.Length; i++)
            {
                res[i] = (char)RuCharactersReversedTranslit[name[i]];
            }
            return new string(res);
        }
        public static string ReverseTranslit(string name)
        {
            var res = new char[name.Length];
            for (int i = 0; i < name.Length; i++)
            {
                res[i] = ForReverseTranslit[name[i]];
            }
            return new string(res);
        }

        public static bool[] AllowedEnglishSymbols = new bool[ushort.MaxValue];
        public static bool[] AllowedArabicNameSymbols = new bool[ushort.MaxValue];
        public static bool[] AllowedArabicSymbols = new bool[ushort.MaxValue];
        public static bool[] AllowArabicNameSymbols = new bool[ushort.MaxValue];

        public static string AllowedEnglishSymbolsStr =
            "`1234567890-=qwertyuiop[]\asdfghjkl;'zxcvbnm,./~!@#$%^&*()_+QWERTYUIOP{}ASDFGHJKL:\"ZXCVBNM<>?; ";
        public static string AllowedEnglishNameSymbolsStr = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";
        public static string AllowedArabicSymbolsStr =
           "`1234567890-=ضصثقفغعهخحجدذشسيبلاتنمكطظزوةىلارؤءئ[];',./~!@#$%^&*()_+{}:\"<>?; ";
        public static string AllowedArabicNameSymbolsStr = " ضصثقفغعهخحجدشسيبلاتنمكطذئءؤرلاىةوزظ.123456789";


        private static void InitAllowedEnglishSymbols()
        {
            foreach (var b in AllowedEnglishSymbolsStr)
            {
                AllowedEnglishSymbols[b] = true;
            }
            foreach (var b in AllowedEnglishNameSymbolsStr)
            {
                AllowedArabicNameSymbols[b] = true;
            }
        }
        private static void InitAllowedArabicSymbols()
        {
            foreach (var b in AllowedArabicSymbolsStr)
            {
                AllowedArabicSymbols[b] = true;
            }
            foreach (var b in AllowedArabicNameSymbolsStr)
            {
                AllowedArabicNameSymbols[b] = true;
            }
        }
        public static bool IsPrueEnglish(string s)
        {
            return s.All(c => AllowedEnglishSymbols[c]);
        }
        public static bool IsPrueEnglishName(string s)
        {
            return s.All(c => AllowedArabicNameSymbols[c]);
        }
        public static Locale MinimumAvailableLocale(Locale clientLocale, string message)
        {
            var isPrueEnglish = IsPrueEnglish(message);
            var locale = Locale.En;
            if (!isPrueEnglish)
                locale = clientLocale;
            return locale;
        }

        public static bool IsPureArabicName(string characterName)
        {
            return characterName.All(c => AllowArabicNameSymbols[c]);
        }
    }
}
