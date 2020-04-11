using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WCell.Util;

namespace WCell.Core.Addons
{
    /// <summary>Contains all information related to an Addon.</summary>
    public class WCellAddonContext
    {
        protected FileInfo m_file;
        protected Assembly m_assembly;
        protected IWCellAddon m_addon;
        protected WCellAddonAttribute m_attr;

        public WCellAddonContext(FileInfo file, Assembly asm)
        {
            this.m_file = file;
            this.m_assembly = asm;
        }

        public string ShortName
        {
            get { return this.m_addon != null ? this.m_addon.ShortName : ""; }
        }

        public FileInfo File
        {
            get { return this.m_file; }
        }

        /// <summary>
        /// The containing assembly (might be null if descriptor has not been loaded yet)
        /// </summary>
        public Assembly Assembly
        {
            get { return this.m_assembly; }
        }

        /// <summary>
        /// The created Addon (might be null if descriptor has not been loaded yet or if this a library which does not get initialized)
        /// </summary>
        public IWCellAddon Addon
        {
            get { return this.m_addon; }
        }

        public void InitAddon()
        {
            if (this.m_addon != null)
                return;
            Type[] types;
            try
            {
                types = this.m_assembly.GetTypes();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "Unable to load Addon {0} - please make sure that it and it's dependencies were built against the current build and all it's dependencies are available.",
                        (object) this), ex);
            }

            foreach (Type type in types)
            {
                if (((IEnumerable<Type>) type.GetInterfaces()).Contains<Type>(typeof(IWCellAddon)))
                {
                    WCellAddonAttribute[] customAttributes = type.GetCustomAttributes<WCellAddonAttribute>();
                    this.m_attr = customAttributes.Length > 0 ? customAttributes[0] : (WCellAddonAttribute) null;
                    this.m_addon = (IWCellAddon) Activator.CreateInstance(type);
                    if (this.m_addon != null)
                    {
                        WCellAddonMgr.RegisterAddon(this);
                        break;
                    }

                    break;
                }
            }
        }

        public override string ToString()
        {
            if (this.m_addon == null)
                return this.m_assembly.FullName;
            return this.m_addon.GetDefaultDescription();
        }
    }
}