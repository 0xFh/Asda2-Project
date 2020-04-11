using WCell.Constants.Misc;

namespace WCell.Constants
{
    public static class WCellInfo
    {
        /// <summary>
        /// The version of the WoW Client that is currently supported.
        /// </summary>
        public static readonly ClientVersion RequiredVersion =
            new ClientVersion((byte) 3, (byte) 3, (byte) 5, (ushort) 12340);

        /// <summary>The official codename of the current WCell build</summary>
        public const string Codename = "Amethyst";

        /// <summary>The color of the current WCell codename</summary>
        public const ChatColor CodenameColor = ChatColor.Purple;
    }
}