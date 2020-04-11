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
      Whitespaces[32] = true;
      Whitespaces[9] = true;
      Whitespaces[13] = true;
      Whitespaces[10] = true;
    }

    public StringStream(string s)
      : this(s, 0)
    {
    }

    public StringStream(string s, int initialPos)
    {
      str = s;
      pos = initialPos;
    }

    public StringStream(StringStream stream)
      : this(stream.str, stream.pos)
    {
    }

    /// <summary>Indicates whether we did not reach the end yet.</summary>
    public bool HasNext
    {
      get { return pos < str.Length; }
    }

    /// <summary>The position within the initial string.</summary>
    public int Position
    {
      get { return pos; }
      set { pos = value; }
    }

    /// <summary>
    /// The remaining length (from the current position until the end).
    /// </summary>
    public int Length
    {
      get { return str.Length - pos; }
    }

    /// <summary>
    /// The remaining string (from the current position until the end).
    /// </summary>
    public string Remainder
    {
      get
      {
        if(!HasNext)
          return "";
        return str.Substring(pos, Length);
      }
    }

    /// <summary>The wrapped string.</summary>
    public string String
    {
      get { return str; }
    }

    /// <summary>[Not implemented]</summary>
    public string this[int index]
    {
      get { return ""; }
    }

    /// <summary>Resets the position to the beginning.</summary>
    public void Reset()
    {
      pos = 0;
    }

    /// <summary>Increases the position by the given count.</summary>
    public void Skip(int charCount)
    {
      pos += charCount;
    }

    /// <returns><code>NextLong(-1, \" \")</code></returns>
    public long NextLong()
    {
      return NextLong(-1L, " ");
    }

    /// <returns><code>NextLong(defaultVal, \" \")</code></returns>
    public long NextLong(long defaultVal)
    {
      return NextLong(defaultVal, " ");
    }

    /// <returns>The next word as long.</returns>
    /// <param name="defaultVal">What should be returned if the next word cannot be converted into a long.</param>
    /// <param name="separator">What the next word should be seperated by.</param>
    public long NextLong(long defaultVal, string separator)
    {
      try
      {
        return long.Parse(NextWord(separator));
      }
      catch
      {
        return defaultVal;
      }
    }

    /// <returns><code>NextInt(-1, \" \")</code></returns>
    public int NextInt()
    {
      return NextInt(-1, " ");
    }

    /// <returns><code>NextInt(defaultVal, \" \")</code></returns>
    public int NextInt(int defaultVal)
    {
      return NextInt(defaultVal, " ");
    }

    /// <returns>The next word as int.</returns>
    /// <param name="defaultVal">What should be returned if the next word cannot be converted into an int.</param>
    /// <param name="separator">What the next word should be seperated by.</param>
    public int NextInt(int defaultVal, string separator)
    {
      int result;
      if(int.TryParse(NextWord(separator), out result))
        return result;
      return defaultVal;
    }

    /// <returns><code>NextUInt(-1, \" \")</code></returns>
    public uint NextUInt()
    {
      return NextUInt(0U, " ");
    }

    /// <returns><code>NextUInt(defaultVal, \" \")</code></returns>
    public uint NextUInt(uint defaultVal)
    {
      return NextUInt(defaultVal, " ");
    }

    /// <returns>The next word as uint.</returns>
    /// <param name="defaultVal">What should be returned if the next word cannot be converted into an int.</param>
    /// <param name="separator">What the next word should be seperated by.</param>
    public uint NextUInt(uint defaultVal, string separator)
    {
      uint result;
      if(uint.TryParse(NextWord(separator), out result))
        return result;
      return defaultVal;
    }

    /// <returns><code>NextInt(-1, \" \")</code></returns>
    public float NextFloat()
    {
      return NextFloat(0.0f, " ");
    }

    /// <returns><code>NextInt(defaultVal, \" \")</code></returns>
    public float NextFloat(float defaultVal)
    {
      return NextFloat(defaultVal, " ");
    }

    /// <returns>The next word as int.</returns>
    /// <param name="defaultVal">What should be returned if the next word cannot be converted into an int.</param>
    /// <param name="separator">What the next word should be seperated by.</param>
    public float NextFloat(float defaultVal, string separator)
    {
      float result;
      if(float.TryParse(NextWord(separator), out result))
        return result;
      return defaultVal;
    }

    public char PeekChar()
    {
      if(!HasNext)
        return char.MinValue;
      return str[pos];
    }

    public char NextChar()
    {
      if(!HasNext)
        return char.MinValue;
      return str[pos++];
    }

    public bool NextBool()
    {
      return NextBool(" ");
    }

    public bool NextBool(bool dflt)
    {
      if(HasNext)
        return NextBool(" ");
      return dflt;
    }

    public bool NextBool(string separator)
    {
      return GetBool(NextWord(separator));
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
      if(!HasNext)
        return defaultVal;
      return NextEnum(" ", defaultVal);
    }

    public T NextEnum<T>(string separator)
    {
      return NextEnum((T) Enum.GetValues(typeof(T)).GetValue(0));
    }

    public T NextEnum<T>(string separator, T defaultVal)
    {
      T result;
      if(!EnumUtil.TryParse(NextWord(separator), out result))
        return defaultVal;
      return result;
    }

    /// <summary>
    /// Calls <code>NextWord(" ")</code>.
    /// </summary>
    public string NextWord()
    {
      return NextWord(" ");
    }

    /// <summary>
    /// Moves the position behind the next word in the string, seperated by <code>seperator</code> and returns the word.
    /// </summary>
    public string NextWord(string separator)
    {
      int length = this.str.Length;
      if(pos >= length)
        return "";
      int num;
      if((num = this.str.IndexOf(separator, pos)) == pos)
      {
        pos += separator.Length;
        return "";
      }

      if(num < 0)
      {
        if(pos == length)
          return "";
        num = length;
      }

      string str = this.str.Substring(pos, num - pos);
      pos = num + separator.Length;
      if(pos > length)
        pos = length;
      return str;
    }

    /// <returns><code>NextWords(count, \" \")</code></returns>
    public string NextWords(int count)
    {
      return NextWords(count, " ");
    }

    /// <summary>
    /// Read the next quoted string or a single word, separated by the given separator
    /// </summary>
    public bool NextString(out string result, string separator = ",")
    {
      SkipWhitespace();
      if(PeekChar() == '"')
      {
        result = NextQuotedString();
        if(str.IndexOf(separator, pos) != pos)
          return false;
        pos += separator.Length;
        return true;
      }

      result = NextWord(separator);
      return true;
    }

    /// <summary>
    /// Reads a quoted string.
    /// Returns an empty string if the next character is not a quotation mark.
    /// </summary>
    public string NextQuotedString()
    {
      if(PeekChar() != '"')
        return "";
      StringBuilder stringBuilder = new StringBuilder(5);
      bool flag = false;
      while(true)
      {
        ++Position;
        char ch = PeekChar();
        if(flag)
        {
          flag = false;
          stringBuilder.Append(ch);
        }
        else
        {
          switch(ch)
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
      ++Position;
      return stringBuilder.ToString();
    }

    /// <summary>-[smhdw] [seconds] [minutes] [hours] [days] [weeks]</summary>
    /// <returns></returns>
    public TimeSpan? NextTimeSpan()
    {
      string str = NextModifiers();
      int seconds = 0;
      int minutes = 0;
      int hours = 0;
      int days = 0;
      if(str.Contains("s"))
        seconds = NextInt(0);
      if(str.Contains("m"))
        minutes = NextInt(0);
      if(str.Contains("h"))
        hours = NextInt(0);
      if(str.Contains("d"))
        days = NextInt(0);
      if(str.Contains("w"))
        days += NextInt(0) * 7;
      if(seconds > 0 || minutes > 0 || hours > 0 || days > 0)
        return new TimeSpan(days, hours, minutes, seconds, 0);
      return new TimeSpan?();
    }

    /// <returns>The next <code>count</code> word seperated by <code>seperator</code> as a string.</returns>
    public string NextWords(int count, string separator)
    {
      string str = "";
      for(int index = 0; index < count && HasNext; ++index)
      {
        if(index > 0)
          str += separator;
        str += NextWord(separator);
      }

      return str;
    }

    /// <returns>The next <code>count</code> word seperated by <code>seperator</code> as an array of strings.</returns>
    public string[] NextWordsArray(int maxCount = 2147483647, string separator = " ")
    {
      string[] strArray = new string[maxCount];
      for(int index = 0; index < maxCount && HasNext; ++index)
        strArray[index] = NextWord(separator);
      return strArray;
    }

    /// <summary>
    /// Calls <code>RemainingWords(" ")</code>
    /// </summary>
    public string[] RemainingWords()
    {
      return RemainingWords(" ");
    }

    public string[] RemainingWords(string separator)
    {
      List<string> stringList = new List<string>();
      while(HasNext)
        stringList.Add(NextWord(separator));
      return stringList.ToArray();
    }

    /// <returns><code>Consume(' ')</code></returns>
    public void ConsumeSpace()
    {
      Consume(' ');
    }

    public void SkipWhitespace()
    {
      char ch;
      while((ch = PeekChar()) < 'Ā' && Whitespaces[ch])
        ++Position;
    }

    /// <summary>
    /// Calls <code>SkipWord(" ")</code>.
    /// </summary>
    public void SkipWord()
    {
      SkipWord(" ");
    }

    /// <summary>
    /// Skips the next word, seperated by the given seperator.
    /// </summary>
    public void SkipWord(string separator)
    {
      SkipWords(1, separator);
    }

    /// <summary>
    /// Calls <code>SkipWords(count, " ")</code>.
    /// </summary>
    /// <param name="count">The amount of words to be skipped.</param>
    public void SkipWords(int count)
    {
      SkipWords(count, " ");
    }

    /// <summary>
    /// Skips <code>count</code> words, seperated by the given seperator.
    /// </summary>
    /// <param name="count">The amount of words to be skipped.</param>
    public void SkipWords(int count, string separator)
    {
      NextWords(count, separator);
    }

    /// <summary>Consume a whole string, as often as it occurs.</summary>
    public void Consume(string rs)
    {
      while(HasNext)
      {
        int index;
        for(index = 0; index < rs.Length; ++index)
        {
          if(str[pos + index] != rs[index])
            return;
        }

        pos += index;
      }
    }

    /// <summary>
    /// Ignores all directly following characters that are equal to <code>c</code>.
    /// </summary>
    public void Consume(char c)
    {
      while(HasNext && str[pos] == c)
        ++pos;
    }

    /// <summary>
    /// Ignores a maximum of <code>amount</code> characters that are equal to <code>c</code>.
    /// </summary>
    public void Consume(char c, int amount)
    {
      for(int index = 0; index < amount && HasNext && (int) str[pos] == (int) c; ++index)
        ++pos;
    }

    /// <summary>
    /// Consumes the next character, if it equals <code>c</code>.
    /// </summary>
    /// <returns>whether the character was equal to <code>c</code> (and thus has been deleted)</returns>
    public bool ConsumeNext(char c)
    {
      if(!HasNext || str[pos] != c)
        return false;
      ++pos;
      return true;
    }

    /// <returns>whether or not the remainder contains the given string.</returns>
    public bool Contains(string s)
    {
      return s.IndexOf(s, pos) > -1;
    }

    /// <returns>whether or not the remainder contains the given char.</returns>
    public bool Contains(char c)
    {
      return str.IndexOf(c, pos) > -1;
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
      string str = NextWord();
      if(str.StartsWith("-") && str.Length > 1)
        return str.Substring(1);
      this.pos = pos;
      return "";
    }

    public StringStream CloneStream()
    {
      return (StringStream) Clone();
    }

    public object Clone()
    {
      return new StringStream(str, pos);
    }

    public override string ToString()
    {
      return Remainder.Trim();
    }
  }
}