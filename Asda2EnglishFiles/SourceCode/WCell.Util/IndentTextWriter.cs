using System;
using System.IO;
using System.Runtime.Remoting;
using System.Text;

namespace WCell.Util
{
    public class IndentTextWriter : TextWriter
    {
        private TextWriter m_writer;
        private string indent;
        private char m_indentChar;
        private bool m_autoFlush;
        private bool m_lastWasNewLine;

        public IndentTextWriter(string file)
            : this(file, '\t')
        {
        }

        public IndentTextWriter(string file, char indentChar)
        {
            this.m_writer = (TextWriter) new StreamWriter(file);
            this.IndentLevel = 0;
            this.m_indentChar = indentChar;
        }

        public IndentTextWriter(TextWriter writer)
            : this(writer, '\t')
        {
        }

        public IndentTextWriter(TextWriter writer, char indentChar)
        {
            this.m_writer = writer;
            this.IndentLevel = 0;
            this.m_indentChar = indentChar;
        }

        public int IndentLevel
        {
            get { return this.indent.Length; }
            set
            {
                this.indent = "";
                for (int index = 0; index < value; ++index)
                    this.indent += (string) (object) this.m_indentChar;
            }
        }

        public char IndentChar
        {
            get { return this.m_indentChar; }
            set
            {
                this.m_indentChar = value;
                this.IndentLevel = this.IndentLevel;
            }
        }

        public string Indent
        {
            get { return this.indent; }
        }

        public bool LastWasNewLine
        {
            get { return this.m_lastWasNewLine; }
            set { this.m_lastWasNewLine = value; }
        }

        public override Encoding Encoding
        {
            get { return this.m_writer.Encoding; }
        }

        public override IFormatProvider FormatProvider
        {
            get { return this.m_writer.FormatProvider; }
        }

        public override string NewLine
        {
            get { return this.m_writer.NewLine; }
            set { this.m_writer.NewLine = value; }
        }

        public bool AutoFlush
        {
            get { return this.m_autoFlush; }
            set { this.m_autoFlush = value; }
        }

        public override void Write(bool value)
        {
            this.m_writer.Write(value);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(char value)
        {
            this.m_writer.Write(value);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(char[] buffer)
        {
            this.m_writer.Write(buffer);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(char[] buffer, int index, int count)
        {
            this.m_writer.Write(buffer, index, count);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(Decimal value)
        {
            this.m_writer.Write(value);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(double value)
        {
            this.m_writer.Write(value);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(float value)
        {
            this.m_writer.Write(value);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(int value)
        {
            this.m_writer.Write(value);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(long value)
        {
            this.m_writer.Write(value);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(object value)
        {
            this.m_writer.Write(value);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(string format, object arg0)
        {
            this.m_writer.Write(format, arg0);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(string format, object arg0, object arg1)
        {
            this.m_writer.Write(format, arg0, arg1);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            this.m_writer.Write(format, arg0, arg1, arg2);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(string format, params object[] arg)
        {
            this.m_writer.Write(format, arg);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(string value)
        {
            this.m_writer.Write(value);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(uint value)
        {
            this.m_writer.Write(value);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void Write(ulong value)
        {
            this.m_writer.Write(value);
            this.m_lastWasNewLine = false;
            if (!this.m_autoFlush)
                return;
            this.m_writer.Flush();
        }

        public override void WriteLine()
        {
            this.m_writer.WriteLine();
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(bool value)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(value);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(char value)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(value);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(char[] buffer)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(buffer);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(buffer, index, count);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(Decimal value)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(value);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(double value)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(value);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(float value)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(value);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(int value)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(value);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(long value)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(value);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(object value)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(value);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(string format, object arg0)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(format, arg0);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(format, arg0, arg1);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(format, arg0, arg1, arg2);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(string format, params object[] arg)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(format, arg);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public void WriteLineNotNull<T>(T obj, string format, params object[] args) where T : class
        {
            if ((object) obj == null)
                return;
            this.WriteLine(format, args);
        }

        public void WriteLineNotDefault<T>(T obj, string format, params object[] args) where T : struct
        {
            if (obj.Equals((object) default(T)))
                return;
            this.WriteLine(format, args);
        }

        public override void WriteLine(string value)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(value);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(uint value)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(value);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override void WriteLine(ulong value)
        {
            if (this.m_lastWasNewLine)
                this.m_writer.Write(this.indent);
            this.m_writer.WriteLine(value);
            if (this.m_autoFlush)
                this.m_writer.Flush();
            this.m_lastWasNewLine = true;
        }

        public override ObjRef CreateObjRef(Type requestedType)
        {
            return this.m_writer.CreateObjRef(requestedType);
        }

        public override bool Equals(object obj)
        {
            return this.m_writer.Equals(obj);
        }

        public override void Close()
        {
            this.m_writer.Close();
        }

        public override void Flush()
        {
            this.m_writer.Flush();
        }

        public override int GetHashCode()
        {
            return this.m_writer.GetHashCode();
        }

        public override object InitializeLifetimeService()
        {
            return this.m_writer.InitializeLifetimeService();
        }

        public override string ToString()
        {
            return this.m_writer.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            this.m_writer.Dispose();
        }
    }
}