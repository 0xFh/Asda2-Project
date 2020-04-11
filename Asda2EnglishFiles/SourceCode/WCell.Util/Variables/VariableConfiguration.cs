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

        [XmlIgnore] public readonly Dictionary<string, V> ByFullName =
            new Dictionary<string, V>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);

        [XmlIgnore] public Action<V> VariableDefinintionInitializor =
            new Action<V>(VariableConfiguration<V>.DefaultDefinitionInitializor);

        private const string SettingsNodeName = "Settings";
        public readonly StringTree<TypeVariableDefinition> Tree;

        /// <summary>Holds an array of static variable fields</summary>
        [XmlIgnore] public readonly Dictionary<string, V> Definitions;

        public VariableConfiguration()
            : this((Action<string>) null)
        {
        }

        public VariableConfiguration(Action<string> onError)
        {
            this.Tree = new StringTree<TypeVariableDefinition>(onError, "\t", new char[1]
            {
                '.'
            });
            this.Definitions =
                new Dictionary<string, V>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
            this.AutoSave = true;
        }

        public Action<string> ErrorHandler
        {
            get { return this.Tree.ErrorHandler; }
            set { this.Tree.ErrorHandler = value; }
        }

        public virtual string FilePath { get; set; }

        public virtual bool AutoSave { get; set; }

        public virtual bool Load()
        {
            if (!File.Exists(this.FilePath))
                return false;
            this.Deserialize();
            return true;
        }

        public void Deserialize()
        {
            XmlUtil.EnsureCulture();
            using (XmlReader reader = XmlReader.Create(this.FilePath))
            {
                reader.ReadStartElement();
                reader.SkipEmptyNodes();
                try
                {
                    this.Tree.ReadXml(reader);
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to load Configuration from: " + this.FilePath, ex);
                }
                finally
                {
                    XmlUtil.ResetCulture();
                }
            }
        }

        public bool Contains(string name)
        {
            return this.Definitions.ContainsKey(name);
        }

        public bool IsReadOnly(string name)
        {
            return this.GetDefinition(name).IsReadOnly;
        }

        public void Save()
        {
            this.Save(true, false);
        }

        public virtual void Save(bool backupFirst, bool auto)
        {
            try
            {
                if (backupFirst && File.Exists(this.FilePath) && new FileInfo(this.FilePath).Length > 0L)
                    this.Backup(".bak");
                this.DoSave();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to save Configuration to: " + this.FilePath, ex);
            }

            XmlUtil.EnsureCulture();
            try
            {
                foreach (IConfiguration childConfiguration in this.ChildConfigurations)
                    childConfiguration.Save(backupFirst, auto);
            }
            finally
            {
                XmlUtil.ResetCulture();
            }
        }

        private void Backup(string suffix)
        {
            string destFileName = this.FilePath + suffix;
            try
            {
                if (new FileInfo(this.FilePath).Length <= 0L)
                    return;
                File.Copy(this.FilePath, destFileName, true);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to create backup of Configuration \"" + destFileName + "\"", ex);
            }
        }

        private void DoSave()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter((Stream) memoryStream, Encoding.UTF8))
                {
                    XmlUtil.EnsureCulture();
                    try
                    {
                        xmlTextWriter.Formatting = Formatting.Indented;
                        xmlTextWriter.WriteStartElement(this.RootNodeName);
                        xmlTextWriter.WriteStartElement("Settings");
                        this.Tree.WriteXml((XmlWriter) xmlTextWriter);
                        xmlTextWriter.WriteEndElement();
                        xmlTextWriter.WriteEndElement();
                    }
                    finally
                    {
                        XmlUtil.ResetCulture();
                    }
                }

                File.WriteAllBytes(this.FilePath, memoryStream.ToArray());
            }
        }

        public static void DefaultDefinitionInitializor(V def)
        {
        }

        public object Get(string name)
        {
            V v;
            if (this.Definitions.TryGetValue(name, out v))
                return v.Value;
            return (object) null;
        }

        public V GetDefinition(string name)
        {
            V v;
            this.Definitions.TryGetValue(name, out v);
            return v;
        }

        public bool Set(string name, object value)
        {
            V v;
            if (!this.Definitions.TryGetValue(name, out v))
                return false;
            v.Value = value;
            return true;
        }

        public bool Set(string name, string value)
        {
            V v;
            if (this.Definitions.TryGetValue(name, out v))
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
            this.VariableDefinintionInitializor(v);
            return v;
        }

        public void AddVariablesOfAsm<A>(Assembly asm) where A : VariableAttribute
        {
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex,
                    "Could not initialize assembly \"{0}\". You can probably fix this issue by making sure that the target platform of the assembly and all it's dependencies are equal.",
                    new object[1]
                    {
                        (object) asm.FullName
                    });
                return;
            }

            foreach (Type type in types)
            {
                this.InitMembers<A>(type.GetMembers(BindingFlags.Static | BindingFlags.Public));
                VariableClassAttribute variableClassAttribute =
                    ((IEnumerable<object>) type.GetCustomAttributes(typeof(VariableClassAttribute), true))
                    .FirstOrDefault<object>() as VariableClassAttribute;
                if (variableClassAttribute != null && variableClassAttribute.Inherit)
                {
                    Type baseType = type.BaseType;
                    while (baseType != (Type) null &&
                           (baseType.Namespace == null || !baseType.Namespace.StartsWith("System")))
                    {
                        this.InitMembers<A>(baseType.GetMembers(BindingFlags.Static | BindingFlags.Public));
                        if (baseType == type.BaseType)
                            break;
                    }
                }
            }
        }

        public void Foreach(Action<IVariableDefinition> callback)
        {
            foreach (V v in this.Definitions.Values)
                callback((IVariableDefinition) v);
        }

        private void InitMembers<A>(MemberInfo[] members) where A : VariableAttribute
        {
            foreach (MemberInfo member in members)
            {
                if (((IEnumerable<NotVariableAttribute>) member.GetCustomAttributes<NotVariableAttribute>())
                    .FirstOrDefault<NotVariableAttribute>() == null)
                {
                    A a = ((IEnumerable<object>) member.GetCustomAttributes(typeof(A), true))
                        .FirstOrDefault<object>() as A;
                    bool readOnly = member.IsReadonly() || (object) a != null && a.IsReadOnly;
                    bool fileOnly = (object) a != null && a.IsFileOnly;
                    Type variableType;
                    if (member.IsFieldOrProp() && (!readOnly || (object) a != null) &&
                        ((variableType = member.GetVariableType()).IsSimpleType() || readOnly ||
                         (variableType.IsArray ||
                          variableType.GetInterface(TypeVariableDefinition.GenericListType.Name) != (Type) null) ||
                         variableType.GetInterface(typeof(IXmlSerializable).Name) != (Type) null))
                    {
                        bool serialized = !readOnly;
                        string name;
                        if ((object) a != null)
                        {
                            name = a.Name ?? member.Name;
                            serialized = !readOnly && a.Serialized;
                        }
                        else
                            name = member.Name;

                        this.Add(name, member, serialized, readOnly, fileOnly);
                    }
                    else if ((object) a != null)
                        throw new Exception(string.Format(
                            "public static member \"{0}\" has VariableAttribute but invalid type.",
                            (object) member.GetFullMemberName()));
                }
            }
        }

        public V Add(string name, MemberInfo member, bool serialized, bool readOnly, bool fileOnly)
        {
            V v;
            if (this.Definitions.TryGetValue(name, out v))
                throw new AmbiguousMatchException("Found Variable with name \"" + name + "\" twice (" + (object) v +
                                                  "). Either rename the Variable or add a VariableAttribute to it to specify a different name in the Configuration file. (public static variables that are not read-only, are automatically added to the global variable collection)");
            V definition = this.CreateDefinition(name, member, serialized, readOnly, fileOnly);
            if ((object) definition != null)
                this.Add(definition, serialized);
            return definition;
        }

        public void Add(V def, bool serialize)
        {
            this.Definitions.Add(def.Name, def);
            this.ByFullName.Add(def.FullName, def);
            if (!serialize)
                return;
            this.Tree.AddChildInChain(def.FullName, (TypeVariableDefinition) def);
        }
    }
}