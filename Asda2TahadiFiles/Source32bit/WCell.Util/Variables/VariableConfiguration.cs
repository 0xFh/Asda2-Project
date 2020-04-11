using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using WCell.Util.NLog;
using WCell.Util.Strings;
using WCell.Util.Xml;

namespace WCell.Util.Variables
{
  public class VariableConfiguration<V> : IConfiguration where V : TypeVariableDefinition, new()
  {
    protected string RootNodeName = "Config";
    public readonly List<IConfiguration> ChildConfigurations = new List<IConfiguration>();

    [XmlIgnore]public readonly Dictionary<string, V> ByFullName =
      new Dictionary<string, V>(StringComparer.InvariantCultureIgnoreCase);

    [XmlIgnore]public Action<V> VariableDefinintionInitializor =
      DefaultDefinitionInitializor;

    private const string SettingsNodeName = "Settings";
    public readonly StringTree<TypeVariableDefinition> Tree;

    /// <summary>Holds an array of static variable fields</summary>
    [XmlIgnore]public readonly Dictionary<string, V> Definitions;

    public VariableConfiguration()
      : this(null)
    {
    }

    public VariableConfiguration(Action<string> onError)
    {
      Tree = new StringTree<TypeVariableDefinition>(onError, "\t", '.');
      Definitions =
        new Dictionary<string, V>(StringComparer.InvariantCultureIgnoreCase);
      AutoSave = true;
    }

    public Action<string> ErrorHandler
    {
      get { return Tree.ErrorHandler; }
      set { Tree.ErrorHandler = value; }
    }

    public virtual string FilePath { get; set; }

    public virtual bool AutoSave { get; set; }

    public virtual bool Load()
    {
      if(!File.Exists(FilePath))
        return false;
      Deserialize();
      return true;
    }

    public void Deserialize()
    {
      XmlUtil.EnsureCulture();
      using(XmlReader reader = XmlReader.Create(FilePath))
      {
        reader.ReadStartElement();
        reader.SkipEmptyNodes();
        try
        {
          Tree.ReadXml(reader);
        }
        catch(Exception ex)
        {
          throw new Exception("Unable to load Configuration from: " + FilePath, ex);
        }
        finally
        {
          XmlUtil.ResetCulture();
        }
      }
    }

    public bool Contains(string name)
    {
      return Definitions.ContainsKey(name);
    }

    public bool IsReadOnly(string name)
    {
      return GetDefinition(name).IsReadOnly;
    }

    public void Save()
    {
      Save(true, false);
    }

    public virtual void Save(bool backupFirst, bool auto)
    {
      try
      {
        if(backupFirst && File.Exists(FilePath) && new FileInfo(FilePath).Length > 0L)
          Backup(".bak");
        DoSave();
      }
      catch(Exception ex)
      {
        throw new Exception("Unable to save Configuration to: " + FilePath, ex);
      }

      XmlUtil.EnsureCulture();
      try
      {
        foreach(IConfiguration childConfiguration in ChildConfigurations)
          childConfiguration.Save(backupFirst, auto);
      }
      finally
      {
        XmlUtil.ResetCulture();
      }
    }

    private void Backup(string suffix)
    {
      string destFileName = FilePath + suffix;
      try
      {
        if(new FileInfo(FilePath).Length <= 0L)
          return;
        File.Copy(FilePath, destFileName, true);
      }
      catch(Exception ex)
      {
        throw new Exception("Unable to create backup of Configuration \"" + destFileName + "\"", ex);
      }
    }

    private void DoSave()
    {
      using(MemoryStream memoryStream = new MemoryStream())
      {
        using(XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
        {
          XmlUtil.EnsureCulture();
          try
          {
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlTextWriter.WriteStartElement(RootNodeName);
            xmlTextWriter.WriteStartElement("Settings");
            Tree.WriteXml(xmlTextWriter);
            xmlTextWriter.WriteEndElement();
            xmlTextWriter.WriteEndElement();
          }
          finally
          {
            XmlUtil.ResetCulture();
          }
        }

        File.WriteAllBytes(FilePath, memoryStream.ToArray());
      }
    }

    public static void DefaultDefinitionInitializor(V def)
    {
    }

    public object Get(string name)
    {
      V v;
      if(Definitions.TryGetValue(name, out v))
        return v.Value;
      return null;
    }

    public V GetDefinition(string name)
    {
      V v;
      Definitions.TryGetValue(name, out v);
      return v;
    }

    public bool Set(string name, object value)
    {
      V v;
      if(!Definitions.TryGetValue(name, out v))
        return false;
      v.Value = value;
      return true;
    }

    public bool Set(string name, string value)
    {
      V v;
      if(Definitions.TryGetValue(name, out v))
        return v.TrySet(value);
      return false;
    }

    public V CreateDefinition(string name, MemberInfo member, bool serialized, bool readOnly, bool fileOnly)
    {
      V instance = Activator.CreateInstance<V>();
      instance.Name = name;
      instance.Member = member;
      instance.Serialized = serialized;
      instance.IsReadOnly = readOnly;
      instance.IsFileOnly = fileOnly;
      V v = instance;
      VariableDefinintionInitializor(v);
      return v;
    }

    public void AddVariablesOfAsm<A>(Assembly asm) where A : VariableAttribute
    {
      Type[] types;
      try
      {
        types = asm.GetTypes();
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException(ex,
          "Could not initialize assembly \"{0}\". You can probably fix this issue by making sure that the target platform of the assembly and all it's dependencies are equal.",
          (object) asm.FullName);
        return;
      }

      foreach(Type type in types)
      {
        InitMembers<A>(type.GetMembers(BindingFlags.Static | BindingFlags.Public));
        VariableClassAttribute variableClassAttribute =
          type.GetCustomAttributes(typeof(VariableClassAttribute), true)
            .FirstOrDefault() as VariableClassAttribute;
        if(variableClassAttribute != null && variableClassAttribute.Inherit)
        {
          Type baseType = type.BaseType;
          while(baseType != null &&
                (baseType.Namespace == null || !baseType.Namespace.StartsWith("System")))
          {
            InitMembers<A>(baseType.GetMembers(BindingFlags.Static | BindingFlags.Public));
            if(baseType == type.BaseType)
              break;
          }
        }
      }
    }

    public void Foreach(Action<IVariableDefinition> callback)
    {
      foreach(V v in Definitions.Values)
        callback(v);
    }

    private void InitMembers<A>(MemberInfo[] members) where A : VariableAttribute
    {
      foreach(MemberInfo member in members)
      {
        if(member.GetCustomAttributes<NotVariableAttribute>()
             .FirstOrDefault() == null)
        {
          A a = member.GetCustomAttributes(typeof(A), true)
            .FirstOrDefault() as A;
          bool readOnly = member.IsReadonly() || a != null && a.IsReadOnly;
          bool fileOnly = a != null && a.IsFileOnly;
          Type variableType;
          if(member.IsFieldOrProp() && (!readOnly || a != null) &&
             ((variableType = member.GetVariableType()).IsSimpleType() || readOnly ||
              (variableType.IsArray ||
               variableType.GetInterface(TypeVariableDefinition.GenericListType.Name) != null) ||
              variableType.GetInterface(typeof(IXmlSerializable).Name) != null))
          {
            bool serialized = !readOnly;
            string name;
            if(a != null)
            {
              name = a.Name ?? member.Name;
              serialized = !readOnly && a.Serialized;
            }
            else
              name = member.Name;

            Add(name, member, serialized, readOnly, fileOnly);
          }
          else if(a != null)
            throw new Exception(string.Format(
              "public static member \"{0}\" has VariableAttribute but invalid type.",
              member.GetFullMemberName()));
        }
      }
    }

    public V Add(string name, MemberInfo member, bool serialized, bool readOnly, bool fileOnly)
    {
      V v;
      if(Definitions.TryGetValue(name, out v))
        throw new AmbiguousMatchException("Found Variable with name \"" + name + "\" twice (" + v +
                                          "). Either rename the Variable or add a VariableAttribute to it to specify a different name in the Configuration file. (public static variables that are not read-only, are automatically added to the global variable collection)");
      V definition = CreateDefinition(name, member, serialized, readOnly, fileOnly);
      if(definition != null)
        Add(definition, serialized);
      return definition;
    }

    public void Add(V def, bool serialize)
    {
      Definitions.Add(def.Name, def);
      ByFullName.Add(def.FullName, def);
      if(!serialize)
        return;
      Tree.AddChildInChain(def.FullName, def);
    }
  }
}