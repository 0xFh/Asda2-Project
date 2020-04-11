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
      if(File.Exists(fileName))
        backup = File.ReadAllText(fileName);
      try
      {
        writer = new IndentTextWriter(new StreamWriter(m_fileName = fileName));
        this.namespce = namespce;
        this.className = className;
        this.typeNamePrefix = typeNamePrefix;
        this.typeNameSuffix = typeNameSuffix;
        WriteHeader(usedNamespaces);
      }
      catch(Exception ex)
      {
        OnException(ex);
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
      get { return raisedException; }
    }

    /// <summary>
    /// The content of the file before (or null if there was none).
    /// </summary>
    public string Backup
    {
      get { return backup; }
    }

    public string Name
    {
      get { return className; }
    }

    public string Namespace
    {
      get { return namespce; }
    }

    public int IndentLevel
    {
      get { return writer.IndentLevel; }
      set { writer.IndentLevel = value; }
    }

    public IndentTextWriter BaseWriter
    {
      get { return writer; }
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
      catch(Exception ex)
      {
        OnException(ex);
      }
    }

    public void Revert()
    {
      try
      {
        Dispose();
      }
      finally
      {
        if(backup != null)
          File.WriteAllText(m_fileName, backup);
        else
          File.Delete(m_fileName);
      }
    }

    private void WriteHeader(string[] usedNamespaces)
    {
      foreach(string usedNamespace in usedNamespaces)
        WriteLine("using " + usedNamespace + ";");
      WriteLine();
      WriteLine("///");
      WriteLine("/// This file was automatically created, using WCell's CodeFileWriter");
      WriteLine("/// Date: " + DateTime.Now.Date.ToShortDateString());
      WriteLine("///");
      WriteLine();
      WriteLine("namespace " + namespce);
      OpenBracket();
      if(string.IsNullOrEmpty(typeNamePrefix))
        throw new Exception("No modifiers");
      WriteLine("public " + typeNamePrefix + " " + className + " " + typeNameSuffix);
      OpenBracket();
    }

    /// <summary>Public Default CTor</summary>
    /// <param name="bodyWriter"></param>
    public void WriteCTor(Action bodyWriter)
    {
      WriteCTor("public", "", bodyWriter);
    }

    /// <summary>Static CTor</summary>
    /// <param name="bodyWriter"></param>
    public void WriteStaticCTor(Action bodyWriter)
    {
      WriteCTor("static", "", bodyWriter);
    }

    public void WriteCTor(string args, Action bodyWriter)
    {
      WriteCTor("public", args, bodyWriter);
    }

    public void WriteCTor(string visibility, string args, Action bodyWriter)
    {
      WriteMethod(visibility, className, null, args, bodyWriter);
    }

    public void WriteDefaultCTor(string visibility, Action bodyWriter)
    {
      WriteMethod(visibility, className, null, "", bodyWriter);
    }

    private void WriteMethodHeader(string visibility, string returnType, string name, string args)
    {
      if(!string.IsNullOrEmpty(visibility))
        visibility += " ";
      if(!string.IsNullOrEmpty(name))
        name = " " + name;
      WriteLine("{0}{1}{2}({3})", (object) visibility, (object) returnType, (object) name, (object) args);
    }

    public void WritePublicMethod(string returnType, string name, string args, Action bodyWriter)
    {
      WriteMethod("public", returnType, name, args, bodyWriter);
    }

    public void WriteStaticMethod(string name, Action bodyWriter)
    {
      WriteMethod("public static", "void", name, "", bodyWriter);
    }

    public void WriteMethod(string name, Action bodyWriter)
    {
      WriteMethod("public", "void", name, "", bodyWriter);
    }

    public void WriteMethod(string returnType, string name, Action bodyWriter)
    {
      WriteMethod("public", returnType, name, "", bodyWriter);
    }

    public void WriteMethod(string visibility, string returnType, string name, Action bodyWriter)
    {
      WriteMethod(visibility, returnType, name, "", bodyWriter);
    }

    public void WriteMethod(string visibility, string returnType, string name, string args, Action bodyWriter)
    {
      WriteMethodHeader(visibility, returnType, name, args);
      OpenBracket();
      try
      {
        bodyWriter();
      }
      catch(Exception ex)
      {
        OnException(ex);
      }

      CloseBracket();
    }

    private void WritePropHeader(string visibility, string returnType, string name)
    {
      if(!string.IsNullOrEmpty(visibility))
        visibility += " ";
      else if(!string.IsNullOrEmpty(name))
        name = " " + name;
      Write("{0}{1}{2}", (object) visibility, (object) returnType, (object) name);
    }

    public void WriteAnonymousProperty(string returnType, string name, bool get, bool set)
    {
      WriteAnonymousProperty("public", returnType, name, get, set);
    }

    public void WriteAnonymousProperty(string visibility, string returnType, string name, bool get, bool set)
    {
      WritePropHeader(visibility, returnType, name);
      WriteLine();
      OpenBracket();
      if(get)
        WriteLine("get;");
      if(set)
        WriteLine("set;");
      CloseBracket();
    }

    public void WriteField(string returnType, string name, bool isReadOnly, bool isStatic)
    {
      WriteField("public", returnType, name, isReadOnly, isStatic);
    }

    public void WriteField(string visibility, string returnType, string name, bool isReadOnly, bool isStatic)
    {
      if(isReadOnly)
        visibility = new string[2]
        {
          visibility,
          "readonly"
        }.ToString(" ");
      if(isStatic)
        visibility = new string[2]
        {
          visibility,
          "static"
        }.ToString(" ");
      WritePropHeader(visibility, returnType, name);
      WriteLine(";");
    }

    public void Call(string methodName)
    {
      Call(methodName, "");
    }

    public void Call(string methodName, string args)
    {
      WriteLine(methodName + "(" + args + ");");
    }

    public void Assign(string key, string value)
    {
      WriteLine(key + "=" + value + ";");
    }

    public void Assign(string type, string varName, string value)
    {
      WriteLine(type + " " + varName + " = " + value + ";");
    }

    public void Assign(string type, string varName, string[] args)
    {
      WriteIndent(type + " " + varName + " = ");
      New(type, args);
    }

    public void Inc(string fieldOrProp)
    {
      WriteLine(fieldOrProp + "++;");
    }

    public void Dec(string fieldOrProp)
    {
      WriteLine(fieldOrProp + "--;");
    }

    public void PublicArray(string type, string name)
    {
      WriteLine("public " + type + "[] " + name + ";");
    }

    public void Array(string type, string name)
    {
      WriteLine(type + "[] " + name + ";");
    }

    public void Array(string type, string name, int length)
    {
      Assign(type + "[]", name, "new " + type + "[" + length + "]");
    }

    public void NewArray(string type, string seperator, Action valueWriter)
    {
      WriteIndent("new " + type + "[]");
      OpenBracket();
      try
      {
        valueWriter();
      }
      catch(Exception ex)
      {
        OnException(ex);
      }

      CloseBracket(seperator);
    }

    public void NewArray<T>(string type, IEnumerable<T> values)
    {
      WriteIndent("new " + type + "[]");
      OpenBracket();
      WriteLine(values.ToString(",\n" + writer.Indent));
      CloseBracket();
    }

    public void Array<T>(string type, string name, IEnumerable<T> values)
    {
      WriteIndent(type + "[] " + name + " = ");
      NewArray(type, values);
    }

    public void Array(string type, string name, Action valueWriter)
    {
      Array(type, name, 1, ";", valueWriter);
    }

    public void Array(string type, string name, int dimensions, string seperator, Action valueWriter)
    {
      string str = "";
      while(dimensions-- > 0)
        str += "[]";
      WriteIndent(type + str + " " + name + " = new " + type + str + " ");
      OpenBracket();
      try
      {
        valueWriter();
      }
      catch(Exception ex)
      {
        OnException(ex);
      }

      CloseBracket(seperator);
    }

    public void Array<T>(string type, string name, string seperator, T[][] arr)
    {
      Array(type, name, 2, seperator, () =>
      {
        foreach(IEnumerable<T> values in arr)
          NewArray(type, values);
      });
    }

    public void NewKomma(string type)
    {
      WriteLine("new " + type + "(),");
    }

    public void NewKomma(string type, IEnumerable<string> args)
    {
      WriteLine("new " + type + "(" + args.ToString(", ") + "),");
    }

    public void NewInit<K, V>(string type, IEnumerable<KeyValuePair<K, V>> args, string seperator)
    {
      WriteIndent("new " + type + " ");
      OpenBracket();
      Write(args.ToString(writer.Indent, ",\n"));
      WriteLine();
      CloseBracket(seperator);
    }

    public void New(string type, IEnumerable<string> args)
    {
      WriteLine("new " + type + "(" + args.ToString(", ") + ");");
    }

    public void WriteCommentLine(object comment)
    {
      WriteLine("// " + comment);
    }

    public void WriteXmlCommentLine(object comment)
    {
      WriteLine("/// " + comment);
    }

    public void WriteXmlCommentLine(object comment, params object[] args)
    {
      WriteLine("/// " + string.Format(comment.ToString(), args));
    }

    public void WriteSummary(string summary)
    {
      WriteLine("/// <summary>");
      WriteLine("/// " + summary);
      WriteLine("/// </summary>");
    }

    public void StartSummary()
    {
      WriteLine("/// <summary>");
    }

    public void EndSummary()
    {
      WriteLine("/// </summary>");
    }

    public void WriteMap(string mapName)
    {
      WriteLine("#region " + mapName);
    }

    public void WriteEndMap()
    {
      WriteLine("#endregion");
    }

    public void OpenBracket()
    {
      WriteLine("{");
      ++writer.IndentLevel;
      ++bracketCount;
    }

    public void CloseBracket(bool semicolon)
    {
      --writer.IndentLevel;
      WriteLine("}" + (semicolon ? ";" : ""));
      WriteLine();
      --bracketCount;
    }

    public void CloseBracket(string end)
    {
      --writer.IndentLevel;
      WriteLine("}" + end);
      --bracketCount;
    }

    public void CloseBracket()
    {
      CloseBracket(false);
    }

    /// <summary>Closes all remaining brackets</summary>
    public void Finish()
    {
      while(bracketCount > 0)
        CloseBracket();
    }

    public void WriteLine(string content)
    {
      try
      {
        writer.WriteLine(content);
      }
      catch(Exception ex)
      {
        OnException(ex);
      }
    }

    public void WriteLine(string content, params object[] args)
    {
      try
      {
        writer.WriteLine(content, args);
      }
      catch(Exception ex)
      {
        OnException(ex);
      }
    }

    public void WriteLine()
    {
      try
      {
        writer.WriteLine();
      }
      catch(Exception ex)
      {
        OnException(ex);
      }
    }

    public void Write(string text, params object[] args)
    {
      Write(string.Format(text, args));
    }

    public void Write(string text)
    {
      try
      {
        writer.Write(text);
      }
      catch(Exception ex)
      {
        OnException(ex);
      }
    }

    public void WriteIndent(string text)
    {
      try
      {
        writer.Write(writer.Indent);
        writer.Write(text);
      }
      catch(Exception ex)
      {
        OnException(ex);
      }
    }

    public void OnException(Exception ex)
    {
      if(raisedException)
        return;
      raisedException = true;
      try
      {
        LogUtil.ErrorException(ex, false, "Failed to write code-file \"{0}\" - Reverting...",
          (object) m_fileName);
        Revert();
      }
      catch(Exception ex1)
      {
        throw new Exception("Failed to revert code-file \"" + m_fileName + "\" after Exception was risen!",
          ex1);
      }
    }

    public void Dispose()
    {
      try
      {
        Finish();
      }
      finally
      {
        writer.Close();
      }
    }
  }
}