using System.Text.RegularExpressions;
using WCell.Core;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Chat
{
    /// <summary>
    /// Utility class for creating and filtering chat messages.
    /// </summary>
    public static class ChatUtility
    {
        public static readonly Regex ControlCodeRegex =
            new Regex("\\|(cff[0-9a-fA-F]{6})|(\\|[rh])", RegexOptions.Compiled);

        public static readonly Regex AllowedControlRegex =
            new Regex(
                "\\|cff[0-9a-fA-F]{6}\\|H(item|quest|spell|achievement|trade)(\\:\\d+)+\\|h\\[[\\w\\d]([^\\|\\t\\r\\n\\0\\]]*)\\]\\|h\\|r",
                RegexOptions.Compiled);

        /// <summary>Strips all color controls from a string.</summary>
        /// <param name="msg">the message to strip colors from</param>
        /// <returns>a string with any color controls removed from it</returns>
        public static string Strip(string msg)
        {
            return ChatUtility.ControlCodeRegex.Replace(msg, "");
        }

        /// <summary>Colorizes the given message with the given color.</summary>
        /// <param name="msg">the message to colorize</param>
        /// <param name="color">the color value</param>
        /// <returns>a colorized string</returns>
        public static string Colorize(string msg, Color color)
        {
            return ChatUtility.Colorize(msg, color, true);
        }

        /// <summary>Colorizes the given message with the given color.</summary>
        /// <param name="msg">the message to colorize</param>
        /// <param name="color">the color value</param>
        /// <param name="enclose">whether to only have this string in the given color or to let the color code open for following text</param>
        /// <returns>a colorized string</returns>
        public static string Colorize(string msg, Color color, bool enclose)
        {
            return "|cff" + color.GetHexCode() + msg + (enclose ? "|r" : "");
        }

        /// <summary>Colorizes the given message with the given color.</summary>
        /// <param name="msg">the message to colorize</param>
        /// <param name="colorRgb">the color value</param>
        /// <returns>a colorized string</returns>
        public static string Colorize(string msg, string colorRgb)
        {
            return ChatUtility.Colorize(msg, colorRgb, true);
        }

        /// <summary>Colorizes the given message with the given color.</summary>
        /// <param name="msg">the message to colorize</param>
        /// <param name="colorRgb">the color value</param>
        /// <param name="enclose">whether to only have this string in the given color or to let the color code open for following text</param>
        /// <returns>a colorized string</returns>
        public static string Colorize(string msg, string colorRgb, bool enclose)
        {
            return "|cff" + colorRgb + msg + (enclose ? "|r" : "");
        }

        /// <summary>Colorizes the given message with the given color.</summary>
        /// <param name="msg">the message to colorize</param>
        /// <param name="red">the red color value</param>
        /// <param name="green">the green color value</param>
        /// <param name="blue">the blue color value</param>
        /// <returns>a colorized string</returns>
        public static string Colorize(string msg, string red, string green, string blue)
        {
            return ChatUtility.Colorize(msg, red, green, blue, true);
        }

        /// <summary>Colorizes the given message with the given color.</summary>
        /// <param name="msg">the message to colorize</param>
        /// <param name="red">the red color value</param>
        /// <param name="green">the green color value</param>
        /// <param name="blue">the blue color value</param>
        /// <param name="enclose">whether to only have this string in the given color or to let the color code open for following text</param>
        /// <returns>a colorized string</returns>
        public static string Colorize(string msg, string red, string green, string blue, bool enclose)
        {
            return "|cff" + red + green + blue + msg + (enclose ? "|r" : "");
        }

        /// <summary>
        /// Filters a string, removing any illegal control characters or character sequences.
        /// </summary>
        public static void Purify(ref string msg)
        {
            int num = 0;
            for (int startat = 0; startat < msg.Length; ++startat)
            {
                char ch = msg[startat];
                if (ch < ' ')
                {
                    msg = "";
                    break;
                }

                if (ch != '|')
                {
                    if (num % 2 != 0)
                    {
                        msg = "";
                        break;
                    }

                    num = 0;
                }
                else if (num % 2 == 0)
                {
                    bool flag = false;
                    for (Match match = ChatUtility.AllowedControlRegex.Match(msg, startat);
                        match.Success;
                        match = match.NextMatch())
                    {
                        flag = true;
                        startat += match.Length;
                    }

                    if (!flag)
                        ++num;
                    else
                        num = 0;
                }
                else
                    ++num;
            }
        }
    }
}