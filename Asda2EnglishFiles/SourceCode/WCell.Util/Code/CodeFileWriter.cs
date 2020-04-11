using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using WCell.Util.NLog;

namespace WCell.Util.Code
{
    public class CodeFileWriter : IDisposable
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();
        public const string PublicVisibility = "public";
        public const string ReadonlyTag = "readonly";
        public const string StaticTag = "static";
        public const string VoidType = "void";
        public const string Class = "class";
        public const string Enum = "enum";
        private IndentTextWriter writer;
        private string namespce;
        private string className;
        private string typeNamePrefix;
        private string typeNameSuffix;
        private int bracketCount;
        private string m_fileName;
        private bool raisedException;
        private string backup;

        public CodeFileWriter(string fileName, string namespce, string className, string typeNamePrefix,
            string typeNameSuffix, params string[] usedNamespaces)
        {
            if (File.Exists(fileName))
                this.backup = File.ReadAllText(fileName);
            try
            {
                this.writer = new IndentTextWriter((TextWriter) new StreamWriter(this.m_fileName = fileName));
                this.namespce = namespce;
                this.className = className;
                this.typeNamePrefix = typeNamePrefix;
                this.typeNameSuffix = typeNameSuffix;
                this.WriteHeader(usedNamespaces);
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        public CodeFileWriter(string fileName, string namespce, string className, string[] usedNamespaces)
            : this(fileName, namespce, className, "class", "", usedNamespaces)
        {
        }

        /// <summary>
        /// Whether an Exception was raised during writing of the file.
        /// </summary>
        public bool RaisedException
        {
            get { return this.raisedException; }
        }

        /// <summary>
        /// The content of the file before (or null if there was none).
        /// </summary>
        public string Backup
        {
            get { return this.backup; }
        }

        public string Name
        {
            get { return this.className; }
        }

        public string Namespace
        {
            get { return this.namespce; }
        }

        public int IndentLevel
        {
            get { return this.writer.IndentLevel; }
            set { this.writer.IndentLevel = value; }
        }

        public IndentTextWriter BaseWriter
        {
            get { return this.writer; }
        }

        /// <summary>
        /// Executes the given action.
        /// If an Exception is raised, the Exception handler will be called and the file will be reverted.
        /// </summary>
        /// <param name="action"></param>
        public void ExecuteSafely(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        public void Revert()
        {
            try
            {
                this.Dispose();
            }
            finally
            {
                if (this.backup != null)
                    File.WriteAllText(this.m_fileName, this.backup);
                else
                    File.Delete(this.m_fileName);
            }
        }

        private void WriteHeader(string[] usedNamespaces)
        {
            foreach (string usedNamespace in usedNamespaces)
                this.WriteLine("using " + usedNamespace + ";");
            this.WriteLine();
            this.WriteLine("///");
            this.WriteLine("/// This file was automatically created, using WCell's CodeFileWriter");
            this.WriteLine("/// Date: " + DateTime.Now.Date.ToShortDateString());
            this.WriteLine("///");
            this.WriteLine();
            this.WriteLine("namespace " + this.namespce);
            this.OpenBracket();
            if (string.IsNullOrEmpty(this.typeNamePrefix))
                throw new Exception("No modifiers");
            this.WriteLine("public " + this.typeNamePrefix + " " + this.className + " " + this.typeNameSuffix);
            this.OpenBracket();
        }

        /// <summary>Public Default CTor</summary>
        /// <param name="bodyWriter"></param>
        public void WriteCTor(Action bodyWriter)
        {
            this.WriteCTor("public", "", bodyWriter);
        }

        /// <summary>Static CTor</summary>
        /// <param name="bodyWriter"></param>
        public void WriteStaticCTor(Action bodyWriter)
        {
            this.WriteCTor("static", "", bodyWriter);
        }

        public void WriteCTor(string args, Action bodyWriter)
        {
            this.WriteCTor("public", args, bodyWriter);
        }

        public void WriteCTor(string visibility, string args, Action bodyWriter)
        {
            this.WriteMethod(visibility, this.className, (string) null, args, bodyWriter);
        }

        public void WriteDefaultCTor(string visibility, Action bodyWriter)
        {
            this.WriteMethod(visibility, this.className, (string) null, "", bodyWriter);
        }

        private void WriteMethodHeader(string visibility, string returnType, string name, string args)
        {
            if (!string.IsNullOrEmpty(visibility))
                visibility += " ";
            if (!string.IsNullOrEmpty(name))
                name = " " + name;
            this.WriteLine("{0}{1}{2}({3})", (object) visibility, (object) returnType, (object) name, (object) args);
        }

        public void WritePublicMethod(string returnType, string name, string args, Action bodyWriter)
        {
            this.WriteMethod("public", returnType, name, args, bodyWriter);
        }

        public void WriteStaticMethod(string name, Action bodyWriter)
        {
            this.WriteMethod("public static", "void", name, "", bodyWriter);
        }

        public void WriteMethod(string name, Action bodyWriter)
        {
            this.WriteMethod("public", "void", name, "", bodyWriter);
        }

        public void WriteMethod(string returnType, string name, Action bodyWriter)
        {
            this.WriteMethod("public", returnType, name, "", bodyWriter);
        }

        public void WriteMethod(string visibility, string returnType, string name, Action bodyWriter)
        {
            this.WriteMethod(visibility, returnType, name, "", bodyWriter);
        }

        public void WriteMethod(string visibility, string returnType, string name, string args, Action bodyWriter)
        {
            this.WriteMethodHeader(visibility, returnType, name, args);
            this.OpenBracket();
            try
            {
                bodyWriter();
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }

            this.CloseBracket();
        }

        private void WritePropHeader(string visibility, string returnType, string name)
        {
            if (!string.IsNullOrEmpty(visibility))
                visibility += " ";
            else if (!string.IsNullOrEmpty(name))
                name = " " + name;
            this.Write("{0}{1}{2}", (object) visibility, (object) returnType, (object) name);
        }

        public void WriteAnonymousProperty(string returnType, string name, bool get, bool set)
        {
            this.WriteAnonymousProperty("public", returnType, name, get, set);
        }

        public void WriteAnonymousProperty(string visibility, string returnType, string name, bool get, bool set)
        {
            this.WritePropHeader(visibility, returnType, name);
            this.WriteLine();
            this.OpenBracket();
            if (get)
                this.WriteLine("get;");
            if (set)
                this.WriteLine("set;");
            this.CloseBracket();
        }

        public void WriteField(string returnType, string name, bool isReadOnly, bool isStatic)
        {
            this.WriteField("public", returnType, name, isReadOnly, isStatic);
        }

        public void WriteField(string visibility, string returnType, string name, bool isReadOnly, bool isStatic)
        {
            if (isReadOnly)
                visibility = ((IEnumerable<string>) new string[2]
                {
                    visibility,
                    "readonly"
                }).ToString<string>(" ");
            if (isStatic)
                visibility = ((IEnumerable<string>) new string[2]
                {
                    visibility,
                    "static"
                }).ToString<string>(" ");
            this.WritePropHeader(visibility, returnType, name);
            this.WriteLine(";");
        }

        public void Call(string methodName)
        {
            this.Call(methodName, "");
        }

        public void Call(string methodName, string args)
        {
            this.WriteLine(methodName + "(" + args + ");");
        }

        public void Assign(string key, string value)
        {
            this.WriteLine(key + "=" + value + ";");
        }

        public void Assign(string type, string varName, string value)
        {
            this.WriteLine(type + " " + varName + " = " + value + ";");
        }

        public void Assign(string type, string varName, string[] args)
        {
            this.WriteIndent(type + " " + varName + " = ");
            this.New(type, (IEnumerable<string>) args);
        }

        public void Inc(string fieldOrProp)
        {
            this.WriteLine(fieldOrProp + "++;");
        }

        public void Dec(string fieldOrProp)
        {
            this.WriteLine(fieldOrProp + "--;");
        }

        public void PublicArray(string type, string name)
        {
            this.WriteLine("public " + type + "[] " + name + ";");
        }

        public void Array(string type, string name)
        {
            this.WriteLine(type + "[] " + name + ";");
        }

        public void Array(string type, string name, int length)
        {
            this.Assign(type + "[]", name, "new " + type + "[" + (object) length + "]");
        }

        public void NewArray(string type, string seperator, Action valueWriter)
        {
            this.WriteIndent("new " + type + "[]");
            this.OpenBracket();
            try
            {
                valueWriter();
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }

            this.CloseBracket(seperator);
        }

        public void NewArray<T>(string type, IEnumerable<T> values)
        {
            this.WriteIndent("new " + type + "[]");
            this.OpenBracket();
            this.WriteLine(values.ToString<T>(",\n" + this.writer.Indent));
            this.CloseBracket();
        }

        public void Array<T>(string type, string name, IEnumerable<T> values)
        {
            this.WriteIndent(type + "[] " + name + " = ");
            this.NewArray<T>(type, values);
        }

        public void Array(string type, string name, Action valueWriter)
        {
            this.Array(type, name, 1, ";", valueWriter);
        }

        public void Array(string type, string name, int dimensions, string seperator, Action valueWriter)
        {
            string str = "";
            while (dimensions-- > 0)
                str += "[]";
            this.WriteIndent(type + str + " " + name + " = new " + type + str + " ");
            this.OpenBracket();
            try
            {
                valueWriter();
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }

            this.CloseBracket(seperator);
        }

        public void Array<T>(string type, string name, string seperator, T[][] arr)
        {
            this.Array(type, name, 2, seperator, (Action) (() =>
            {
                foreach (IEnumerable<T> values in arr)
                    this.NewArray<T>(type, values);
            }));
        }

        public void NewKomma(string type)
        {
            this.WriteLine("new " + type + "(),");
        }

        public void NewKomma(string type, IEnumerable<string> args)
        {
            this.WriteLine("new " + type + "(" + args.ToString<string>(", ") + "),");
        }

        public void NewInit<K, V>(string type, IEnumerable<KeyValuePair<K, V>> args, string seperator)
        {
            this.WriteIndent("new " + type + " ");
            this.OpenBracket();
            this.Write(args.ToString<K, V>(this.writer.Indent, ",\n"));
            this.WriteLine();
            this.CloseBracket(seperator);
        }

        public void New(string type, IEnumerable<string> args)
        {
            this.WriteLine("new " + type + "(" + args.ToString<string>(", ") + ");");
        }

        public void WriteCommentLine(object comment)
        {
            this.WriteLine("// " + comment);
        }

        public void WriteXmlCommentLine(object comment)
        {
            this.WriteLine("/// " + comment);
        }

        public void WriteXmlCommentLine(object comment, params object[] args)
        {
            this.WriteLine("/// " + string.Format(comment.ToString(), args));
        }

        public void WriteSummary(string summary)
        {
            this.WriteLine("/// <summary>");
            this.WriteLine("/// " + summary);
            this.WriteLine("/// </summary>");
        }

        public void StartSummary()
        {
            this.WriteLine("/// <summary>");
        }

        public void EndSummary()
        {
            this.WriteLine("/// </summary>");
        }

        public void WriteMap(string mapName)
        {
            this.WriteLine("#region " + mapName);
        }

        public void WriteEndMap()
        {
            this.WriteLine("#endregion");
        }

        public void OpenBracket()
        {
            this.WriteLine("{");
            ++this.writer.IndentLevel;
            ++this.bracketCount;
        }

        public void CloseBracket(bool semicolon)
        {
            --this.writer.IndentLevel;
            this.WriteLine("}" + (semicolon ? ";" : ""));
            this.WriteLine();
            --this.bracketCount;
        }

        public void CloseBracket(string end)
        {
            --this.writer.IndentLevel;
            this.WriteLine("}" + end);
            --this.bracketCount;
        }

        public void CloseBracket()
        {
            this.CloseBracket(false);
        }

        /// <summary>Closes all remaining brackets</summary>
        public void Finish()
        {
            while (this.bracketCount > 0)
                this.CloseBracket();
        }

        public void WriteLine(string content)
        {
            try
            {
                this.writer.WriteLine(content);
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        public void WriteLine(string content, params object[] args)
        {
            try
            {
                this.writer.WriteLine(string.Format(content, args));
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        public void WriteLine()
        {
            try
            {
                this.writer.WriteLine();
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        public void Write(string text, params object[] args)
        {
            this.Write(string.Format(text, args));
        }

        public void Write(string text)
        {
            try
            {
                this.writer.Write(text);
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        public void WriteIndent(string text)
        {
            try
            {
                this.writer.Write(this.writer.Indent);
                this.writer.Write(text);
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
        }

        public void OnException(Exception ex)
        {
            if (this.raisedException)
                return;
            this.raisedException = true;
            try
            {
                LogUtil.ErrorException(ex, false, "Failed to write code-file \"{0}\" - Reverting...",
                    (object) this.m_fileName);
                this.Revert();
            }
            catch (Exception ex1)
            {
                throw new Exception("Failed to revert code-file \"" + this.m_fileName + "\" after Exception was risen!",
                    ex1);
            }
        }

        public void Dispose()
        {
            try
            {
                this.Finish();
            }
            finally
            {
                this.writer.Close();
            }
        }
    }
}