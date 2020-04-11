using NLog;
using System;
using System.Globalization;
using System.IO;
using WCell.Core.Variables;
using WCell.Util.Variables;

namespace WCell.Core.Addons
{
    public abstract class WCellAddonBase : IWCellAddon
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected IConfiguration config;

        /// <summary>
        /// The <see cref="T:WCell.Core.Addons.WCellAddonContext" /> that was used to load this Addon.
        /// </summary>
        public WCellAddonContext Context { get; private set; }

        private static void OnError(string msg)
        {
            WCellAddonBase.log.Warn("<Config>" + msg);
        }

        public virtual bool UseConfig
        {
            get { return false; }
        }

        public virtual IConfiguration CreateConfig()
        {
            VariableConfiguration<WCellVariableDefinition> variableConfiguration =
                new VariableConfiguration<WCellVariableDefinition>(new Action<string>(WCellAddonBase.OnError));
            variableConfiguration.FilePath =
                Path.Combine(this.Context.File.DirectoryName, this.GetType().Name + "Config.xml");
            variableConfiguration.AddVariablesOfAsm<VariableAttribute>(this.GetType().Assembly);
            return (IConfiguration) variableConfiguration;
        }

        public abstract string Name { get; }

        public abstract string ShortName { get; }

        public abstract string Author { get; }

        public abstract string Website { get; }

        public abstract void TearDown();

        public IConfiguration Config
        {
            get { return this.config; }
        }

        public abstract string GetLocalizedName(CultureInfo culture);

        public override string ToString()
        {
            return this.Name + " (" + this.ShortName + ") by " + this.Author;
        }

        public void InitAddon(WCellAddonContext context)
        {
            this.Context = context;
            if (!this.UseConfig)
                return;
            this.config = this.CreateConfig();
            if (!this.config.Load())
            {
                try
                {
                    this.config.Save(true, true);
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to save " + this.config.GetType().Name + " of addon: " + (object) this,
                        ex);
                }
            }
        }
    }
}