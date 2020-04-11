using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.RealmServer.Instances
{
    public abstract class InstanceConfigBase<T, E> : XmlFile<T>, IInstanceConfig
        where T : XmlFileBase, IInstanceConfig, new() where E : IComparable
    {
        [XmlIgnore] private Dictionary<E, InstanceConfigEntry<E>> m_Settings =
            new Dictionary<E, InstanceConfigEntry<E>>();

        private static string filename;
        [XmlIgnore] private InstanceConfigEntry<E>[] m_entries;

        public static string Filename
        {
            get { return InstanceConfigBase<T, E>.filename; }
        }

        protected static T LoadSettings(string fileName)
        {
            InstanceConfigBase<T, E>.filename = RealmServerConfiguration.GetContentPath(fileName);
            T obj = !File.Exists(InstanceConfigBase<T, E>.filename)
                ? Activator.CreateInstance<T>()
                : XmlFile<T>.Load(InstanceConfigBase<T, E>.filename);
            obj.Setup();
            try
            {
                obj.SaveAs(InstanceConfigBase<T, E>.filename);
            }
            catch (Exception ex)
            {
                LogUtil.WarnException(ex, "Unable to save Configuration file", new object[0]);
            }

            return obj;
        }

        [XmlElement("Setting")]
        public InstanceConfigEntry<E>[] Entries
        {
            get { return this.m_entries; }
            set
            {
                this.m_entries = value;
                this.SortSettings();
            }
        }

        [XmlIgnore]
        public Dictionary<E, InstanceConfigEntry<E>> Settings
        {
            get { return this.m_Settings; }
            set { this.m_Settings = value; }
        }

        [XmlIgnore] public abstract IEnumerable<E> SortedIds { get; }

        public InstanceConfigEntry<E> GetSetting(E id)
        {
            InstanceConfigEntry<E> instanceConfigEntry;
            this.m_Settings.TryGetValue(id, out instanceConfigEntry);
            return instanceConfigEntry;
        }

        protected abstract void InitSetting(InstanceConfigEntry<E> configEntry);

        public void Setup()
        {
            if (this.Entries == null)
            {
                this.SortSettings();
            }
            else
            {
                foreach (InstanceConfigEntry<E> configEntry in this.Settings.Values)
                {
                    if (configEntry != null && configEntry.TypeName.Trim().Length > 0)
                        this.InitSetting(configEntry);
                }
            }
        }

        private void SortSettings()
        {
            if (this.Entries != null)
            {
                foreach (InstanceConfigEntry<E> entry in this.Entries)
                {
                    if (entry != null)
                        this.Settings[entry.Name] = entry;
                }
            }

            this.CreateStubs();
            this.m_entries = this.m_Settings.Values.ToArray<InstanceConfigEntry<E>>();
            Array.Sort<InstanceConfigEntry<E>>(this.m_entries);
        }

        private void CreateStubs()
        {
            this.CreateStubs(this.SortedIds);
        }

        private void CreateStubs(IEnumerable<E> sortedIds)
        {
            foreach (E sortedId in sortedIds)
            {
                if (this.GetSetting(sortedId) == null)
                    this.Settings[sortedId] = new InstanceConfigEntry<E>(sortedId, " ");
            }
        }
    }
}