using System;
using System.Collections.Generic;
using System.Text;

namespace WCell.Util.Strings
{
    /// <summary>
    /// Wraps a string for convinient string parsing.
    /// It is using an internal position for the given string so you can read
    /// continuesly the next part.
    /// 
    /// TODO: Make it an actual stream
    /// </summary>
    public class StringStream : ICloneable
    {
        public static bool[] Whitespaces = new bool[256];
        private readonly string str;
        private int pos;

        static StringStream()
        {
            StringStream.Whitespaces[32] = true;
            StringStream.Whitespaces[9] = true;
            StringStream.Whitespaces[13] = true;
            StringStream.Whitespaces[10] = true;
        }

        public StringStream(string s)
            : this(s, 0)
        {
        }

        public StringStream(string s, int initialPos)
        {
            this.str = s;
            this.pos = initialPos;
        }

        public StringStream(StringStream stream)
            : this(stream.str, stream.pos)
        {
        }

        /// <summary>Indicates whether we did not reach the end yet.</summary>
        public bool HasNext
        {
            get { return this.pos < this.str.Length; }
        }

        /// <summary>The position within the initial string.</summary>
        public int Position
        {
            get { return this.pos; }
            set { this.pos = value; }
        }

        /// <summary>
        /// The remaining length (from the current position until the end).
        /// </summary>
        public int Length
        {
            get { return this.str.Length - this.pos; }
        }

        /// <summary>
        /// The remaining string (from the current position until the end).
        /// </summary>
        public string Remainder
        {
            get
            {
                if (!this.HasNext)
                    return "";
                return this.str.Substring(this.pos, this.Length);
            }
        }

        /// <summary>The wrapped string.</summary>
        public string String
        {
            get { return this.str; }
        }

        /// <summary>[Not implemented]</summary>
        public string this[int index]
        {
            get { return ""; }
        }

        /// <summary>Resets the position to the beginning.</summary>
        public void Reset()
        {
            this.pos = 0;
        }

        /// <summary>Increases the position by the given count.</summary>
        public void Skip(int charCount)
        {
            this.pos += charCount;
        }

        /// <returns><code>NextLong(-1, \" \")</code></returns>
        public long NextLong()
        {
            return this.NextLong(-1L, " ");
        }

        /// <returns><code>NextLong(defaultVal, \" \")</code></returns>
        public long NextLong(long defaultVal)
        {
            return this.NextLong(defaultVal, " ");
        }

        /// <returns>The next word as long.</returns>
        /// <param name="defaultVal">What should be returned if the next word cannot be converted into a long.</param>
        /// <param name="separator">What the next word should be seperated by.</param>
        public long NextLong(long defaultVal, string separator)
        {
            try
            {
                return long.Parse(this.NextWord(separator));
            }
            catch
            {
                return defaultVal;
            }
        }

        /// <returns><code>NextInt(-1, \" \")</code></returns>
        public int NextInt()
        {
            return this.NextInt(-1, " ");
        }

        /// <returns><code>NextInt(defaultVal, \" \")</code></returns>
        public int NextInt(int defaultVal)
        {
            return this.NextInt(defaultVal, " ");
        }

        /// <returns>The next word as int.</returns>
        /// <param name="defaultVal">What should be returned if the next word cannot be converted into an int.</param>
        /// <param name="separator">What the next word should be seperated by.</param>
        public int NextInt(int defaultVal, string separator)
        {
            int result;
            if (int.TryParse(this.NextWord(separator), out result))
                return result;
            return defaultVal;
        }

        /// <returns><code>NextUInt(-1, \" \")</code></returns>
        public uint NextUInt()
        {
            return this.NextUInt(0U, " ");
        }

        /// <returns><code>NextUInt(defaultVal, \" \")</code></returns>
        public uint NextUInt(uint defaultVal)
        {
            return this.NextUInt(defaultVal, " ");
        }

        /// <returns>The next word as uint.</returns>
        /// <param name="defaultVal">What should be returned if the next word cannot be converted into an int.</param>
        /// <param name="separator">What the next word should be seperated by.</param>
        public uint NextUInt(uint defaultVal, string separator)
        {
            uint result;
            if (uint.TryParse(this.NextWord(separator), out result))
                return result;
            return defaultVal;
        }

        /// <returns><code>NextInt(-1, \" \")</code></returns>
        public float NextFloat()
        {
            return this.NextFloat(0.0f, " ");
        }

        /// <returns><code>NextInt(defaultVal, \" \")</code></returns>
        public float NextFloat(float defaultVal)
        {
            return this.NextFloat(defaultVal, " ");
        }

        /// <returns>The next word as int.</returns>
        /// <param name="defaultVal">What should be returned if the next word cannot be converted into an int.</param>
        /// <param name="separator">What the next word should be seperated by.</param>
        public float NextFloat(float defaultVal, string separator)
        {
            float result;
            if (float.TryParse(this.NextWord(separator), out result))
                return result;
            return defaultVal;
        }

        public char PeekChar()
        {
            if (!this.HasNext)
                return char.MinValue;
            return this.str[this.pos];
        }

        public char NextChar()
        {
            if (!this.HasNext)
                return char.MinValue;
            return this.str[this.pos++];
        }

        public bool NextBool()
        {
            return this.NextBool(" ");
        }

        public bool NextBool(bool dflt)
        {
            if (this.HasNext)
                return this.NextBool(" ");
            return dflt;
        }

        public bool NextBool(string separator)
        {
            return StringStream.GetBool(this.NextWord(separator));
        }

        public static bool GetBool(string word)
        {
            return word.Equals("1") || word.StartsWith("y", StringComparison.InvariantCultureIgnoreCase) ||
                   word.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Calls <code>NextEnum(" ")</code>.
        /// </summary>
        public T NextEnum<T>(T defaultVal)
        {
            if (!this.HasNext)
                return defaultVal;
            return this.NextEnum<T>(" ", defaultVal);
        }

        public T NextEnum<T>(string separator)
        {
            return this.NextEnum<T>((T) Enum.GetValues(typeof(T)).GetValue(0));
        }

        public T NextEnum<T>(string separator, T defaultVal)
        {
            T result;
            if (!EnumUtil.TryParse<T>(this.NextWord(separator), out result))
                return defaultVal;
            return result;
        }

        /// <summary>
        /// Calls <code>NextWord(" ")</code>.
        /// </summary>
        public string NextWord()
        {
            return this.NextWord(" ");
        }

        /// <summary>
        /// Moves the position behind the next word in the string, seperated by <code>seperator</code> and returns the word.
        /// </summary>
        public string NextWord(string separator)
        {
            int length = this.str.Length;
            if (this.pos >= length)
                return "";
            int num;
            if ((num = this.str.IndexOf(separator, this.pos)) == this.pos)
            {
                this.pos += separator.Length;
                return "";
            }

            if (num < 0)
            {
                if (this.pos == length)
                    return "";
                num = length;
            }

            string str = this.str.Substring(this.pos, num - this.pos);
            this.pos = num + separator.Length;
            if (this.pos > length)
                this.pos = length;
            return str;
        }

        /// <returns><code>NextWords(count, \" \")</code></returns>
        public string NextWords(int count)
        {
            return this.NextWords(count, " ");
        }

        /// <summary>
        /// Read the next quoted string or a single word, separated by the given separator
        /// </summary>
        public bool NextString(out string result, string separator = ",")
        {
            this.SkipWhitespace();
            if (this.PeekChar() == '"')
            {
                result = this.NextQuotedString();
                if (this.str.IndexOf(separator, this.pos) != this.pos)
                    return false;
                this.pos += separator.Length;
                return true;
            }

            result = this.NextWord(separator);
            return true;
        }

        /// <summary>
        /// Reads a quoted string.
        /// Returns an empty string if the next character is not a quotation mark.
        /// </summary>
        public string NextQuotedString()
        {
            if (this.PeekChar() != '"')
                return "";
            StringBuilder stringBuilder = new StringBuilder(5);
            bool flag = false;
            while (true)
            {
                ++this.Position;
                char ch = this.PeekChar();
                if (flag)
                {
                    flag = false;
                    stringBuilder.Append(ch);
                }
                else
                {
                    switch (ch)
                    {
                        case '"':
                            goto label_7;
                        case '\\':
                            flag = true;
                            break;
                        default:
                            stringBuilder.Append(ch);
                            break;
                    }
                }
            }

            label_7:
            ++this.Position;
            return stringBuilder.ToString();
        }

        /// <summary>-[smhdw] [seconds] [minutes] [hours] [days] [weeks]</summary>
        /// <returns></returns>
        public TimeSpan? NextTimeSpan()
        {
            string str = this.NextModifiers();
            int seconds = 0;
            int minutes = 0;
            int hours = 0;
            int days = 0;
            if (str.Contains("s"))
                seconds = this.NextInt(0);
            if (str.Contains("m"))
                minutes = this.NextInt(0);
            if (str.Contains("h"))
                hours = this.NextInt(0);
            if (str.Contains("d"))
                days = this.NextInt(0);
            if (str.Contains("w"))
                days += this.NextInt(0) * 7;
            if (seconds > 0 || minutes > 0 || hours > 0 || days > 0)
                return new TimeSpan?(new TimeSpan(days, hours, minutes, seconds, 0));
            return new TimeSpan?();
        }

        /// <returns>The next <code>count</code> word seperated by <code>seperator</code> as a string.</returns>
        public string NextWords(int count, string separator)
        {
            string str = "";
            for (int index = 0; index < count && this.HasNext; ++index)
            {
                if (index > 0)
                    str += separator;
                str += this.NextWord(separator);
            }

            return str;
        }

        /// <returns>The next <code>count</code> word seperated by <code>seperator</code> as an array of strings.</returns>
        public string[] NextWordsArray(int maxCount = 2147483647, string separator = " ")
        {
            string[] strArray = new string[maxCount];
            for (int index = 0; index < maxCount && this.HasNext; ++index)
                strArray[index] = this.NextWord(separator);
            return strArray;
        }

        /// <summary>
        /// Calls <code>RemainingWords(" ")</code>
        /// </summary>
        public string[] RemainingWords()
        {
            return this.RemainingWords(" ");
        }

        public string[] RemainingWords(string separator)
        {
            List<string> stringList = new List<string>();
            while (this.HasNext)
                stringList.Add(this.NextWord(separator));
            return stringList.ToArray();
        }

        /// <returns><code>Consume(' ')</code></returns>
        public void ConsumeSpace()
        {
            this.Consume(' ');
        }

        public void SkipWhitespace()
        {
            char ch;
            while ((ch = this.PeekChar()) < 'Ā' && StringStream.Whitespaces[(int) ch])
                ++this.Position;
        }

        /// <summary>
        /// Calls <code>SkipWord(" ")</code>.
        /// </summary>
        public void SkipWord()
        {
            this.SkipWord(" ");
        }

        /// <summary>
        /// Skips the next word, seperated by the given seperator.
        /// </summary>
        public void SkipWord(string separator)
        {
            this.SkipWords(1, separator);
        }

        /// <summary>
        /// Calls <code>SkipWords(count, " ")</code>.
        /// </summary>
        /// <param name="count">The amount of words to be skipped.</param>
        public void SkipWords(int count)
        {
            this.SkipWords(count, " ");
        }

        /// <summary>
        /// Skips <code>count</code> words, seperated by the given seperator.
        /// </summary>
        /// <param name="count">The amount of words to be skipped.</param>
        public void SkipWords(int count, string separator)
        {
            this.NextWords(count, separator);
        }

        /// <summary>Consume a whole string, as often as it occurs.</summary>
        public void Consume(string rs)
        {
            while (this.HasNext)
            {
                int index;
                for (index = 0; index < rs.Length; ++index)
                {
                    if ((int) this.str[this.pos + index] != (int) rs[index])
                        return;
                }

                this.pos += index;
            }
        }

        /// <summary>
        /// Ignores all directly following characters that are equal to <code>c</code>.
        /// </summary>
        public void Consume(char c)
        {
            while (this.HasNext && (int) this.str[this.pos] == (int) c)
                ++this.pos;
        }

        /// <summary>
        /// Ignores a maximum of <code>amount</code> characters that are equal to <code>c</code>.
        /// </summary>
        public void Consume(char c, int amount)
        {
            for (int index = 0; index < amount && this.HasNext && (int) this.str[this.pos] == (int) c; ++index)
                ++this.pos;
        }

        /// <summary>
        /// Consumes the next character, if it equals <code>c</code>.
        /// </summary>
        /// <returns>whether the character was equal to <code>c</code> (and thus has been deleted)</returns>
        public bool ConsumeNext(char c)
        {
            if (!this.HasNext || (int) this.str[this.pos] != (int) c)
                return false;
            ++this.pos;
            return true;
        }

        /// <returns>whether or not the remainder contains the given string.</returns>
        public bool Contains(string s)
        {
            return s.IndexOf(s, this.pos) > -1;
        }

        /// <returns>whether or not the remainder contains the given char.</returns>
        public bool Contains(char c)
        {
            return this.str.IndexOf(c, this.pos) > -1;
        }

        /// <summary>
        /// Reads the next word as string of modifiers.
        /// Modifiers are a string (usually representing a set of different modifiers per char), preceeded by a -.
        /// </summary>
        /// <remarks>Doesn't do anything if the next word does not start with a -.</remarks>
        /// <returns>The set of flags without the - or "" if none found</returns>
        public string NextModifiers()
        {
            int pos = this.pos;
            string str = this.NextWord();
            if (str.StartsWith("-") && str.Length > 1)
                return str.Substring(1);
            this.pos = pos;
            return "";
        }

        public StringStream CloneStream()
        {
            return (StringStream) this.Clone();
        }

        public object Clone()
        {
            return (object) new StringStream(this.str, this.pos);
        }

        public override string ToString()
        {
            return this.Remainder.Trim();
        }
    }
}