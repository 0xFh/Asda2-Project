using System;
using System.Collections.Generic;
using WCell.Constants;

namespace WCell.Core
{
    /// <summary>TODO: Update summary.</summary>
    public static class ClientLocaleUtility
    {
        private static readonly Dictionary<string, ClientLocale> LocaleMap =
            new Dictionary<string, ClientLocale>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase)
            {
                {
                    "enUS",
                    ClientLocale.English
                },
                {
                    "enGB",
                    ClientLocale.English
                },
                {
                    "koKR",
                    ClientLocale.Korean
                },
                {
                    "frFR",
                    ClientLocale.French
                },
                {
                    "deDE",
                    ClientLocale.German
                },
                {
                    "zhCN",
                    ClientLocale.ChineseSimplified
                },
                {
                    "zhTW",
                    ClientLocale.ChineseTraditional
                },
                {
                    "esES",
                    ClientLocale.Spanish
                },
                {
                    "esMX",
                    ClientLocale.Spanish
                },
                {
                    "ruRU",
                    ClientLocale.Russian
                }
            };

        /// <summary>Looks up an enumeration for the given string.</summary>
        /// <param name="locale">string representation to lookup</param>
        /// <returns>Returns the matching enum member or the <see cref="F:WCell.Core.WCellConstants.DefaultLocale" /></returns>
        public static ClientLocale Lookup(string locale)
        {
            if (string.IsNullOrWhiteSpace(locale))
                return WCellConstants.DefaultLocale;
            try
            {
                locale = locale.Substring(0, 4);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return WCellConstants.DefaultLocale;
            }

            ClientLocale clientLocale;
            if (!ClientLocaleUtility.LocaleMap.TryGetValue(locale, out clientLocale))
                return WCellConstants.DefaultLocale;
            return clientLocale;
        }
    }
}