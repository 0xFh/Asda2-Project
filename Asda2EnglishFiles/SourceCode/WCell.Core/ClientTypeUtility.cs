using System;
using System.Collections.Generic;
using WCell.Constants;

namespace WCell.Core
{
    /// <summary>TODO: Update summary.</summary>
    public static class ClientTypeUtility
    {
        private static readonly Dictionary<string, ClientType> TypeMap =
            new Dictionary<string, ClientType>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase)
            {
                {
                    "WoWT",
                    ClientType.Test
                },
                {
                    "WoWB",
                    ClientType.Beta
                },
                {
                    "WoW\0",
                    ClientType.Normal
                },
                {
                    "WoWI",
                    ClientType.Installing
                }
            };

        /// <summary>Looks up an enumeration for the given string.</summary>
        /// <param name="locale">string representation to lookup</param>
        /// <returns>Returns the matching enum member or the <see cref="F:WCell.Constants.ClientType.Invalid" /></returns>
        public static ClientType Lookup(string clientInstallationType)
        {
            if (string.IsNullOrWhiteSpace(clientInstallationType))
                return ClientType.Invalid;
            try
            {
                clientInstallationType = clientInstallationType.Substring(0, 4);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return ClientType.Invalid;
            }

            ClientType clientType;
            if (!ClientTypeUtility.TypeMap.TryGetValue(clientInstallationType, out clientType))
                return ClientType.Invalid;
            return clientType;
        }
    }
}