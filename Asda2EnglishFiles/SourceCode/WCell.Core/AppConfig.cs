using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using WCell.Core.Localization;

namespace WCell.Core
{
    /// <summary>Defines a configuration made up of key/value values</summary>
    public sealed class AppConfig : IEnumerable<string>, IEnumerable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public const string NullString = "None";
        private readonly System.Configuration.Configuration _cfg;
        private readonly FileInfo _executableFile;

        public FileInfo ExecutableFile
        {
            get { return this._executableFile; }
        }

        /// <summary>Whether to save after adding/changing values</summary>
        public bool SaveOnChange { get; set; }

        /// <summary>Default constructor</summary>
        /// <param name="executablePath">The path of the executable whose App-config to load</param>
        public AppConfig(string executablePath)
        {
            try
            {
                this._cfg = ConfigurationManager.OpenExeConfiguration(
                    (this._executableFile = new FileInfo(executablePath)).FullName);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format((IFormatProvider) CultureInfo.CurrentCulture,
                    "Cannot load AppConfig for {0}", new object[1]
                    {
                        (object) executablePath
                    }), ex);
            }

            AppConfig.LoadConfigDefaults();
        }

        /// <summary>
        /// Loads default values in the configuration if they don't already exist
        /// </summary>
        private static void LoadConfigDefaults()
        {
        }

        private static int GetNoneNesting(string val)
        {
            int indexB = 0;
            int length = val.Length;
            if (length > 1)
            {
                while (val[indexB] == '(' && val[length - indexB - 1] == ')')
                    ++indexB;
                if (indexB > 0 &&
                    string.Compare("None", 0, val, indexB, length - 2 * indexB, StringComparison.Ordinal) != 0)
                    indexB = 0;
            }

            return indexB;
        }

        public bool AddValue(string key, string value)
        {
            this._cfg.AppSettings.Settings.Add(key, value);
            return true;
        }

        public string GetValue(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            string val = this._cfg.AppSettings.Settings[key].Value;
            if (val == null)
            {
                AppConfig.Log.Error(string.Format((IFormatProvider) CultureInfo.CurrentCulture, WCell_Core.KeyNotFound,
                    new object[1]
                    {
                        (object) key
                    }));
                return "";
            }

            switch (AppConfig.GetNoneNesting(val))
            {
                case 0:
                    return val;
                case 1:
                    return (string) null;
                default:
                    return val.Substring(1, val.Length - 2);
            }
        }

        public bool GetBool(string key)
        {
            bool result;
            if (!bool.TryParse(this.GetValue(key), out result))
                return false;
            return result;
        }

        public bool SetValue(string key, object value)
        {
            if (this._cfg.AppSettings.Settings[key] == null)
                return false;
            this._cfg.AppSettings.Settings.Remove(key);
            this._cfg.AppSettings.Settings.Add(key, value.ToString());
            return true;
        }

        /// <summary>
        /// Creates a config entry with the supplied value if one doesn't already exist
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="value">the value</param>
        public void CreateValue(string key, object value)
        {
            if (this._cfg.AppSettings.Settings[key] != null)
                return;
            this._cfg.AppSettings.Settings.Add(key, value.ToString());
            if (this.SaveOnChange)
                this._cfg.Save(ConfigurationSaveMode.Full);
        }

        public void OverrideValue(string key, string value)
        {
            KeyValueConfigurationElement setting = this._cfg.AppSettings.Settings[key];
            if (setting == null)
                this._cfg.AppSettings.Settings.Add(key, value);
            else
                setting.Value = value;
            if (!this.SaveOnChange)
                return;
            this._cfg.Save(ConfigurationSaveMode.Full);
        }

        public void Save()
        {
            this._cfg.Save(ConfigurationSaveMode.Full);
        }

        public string GetFullPath(string file)
        {
            if (Path.IsPathRooted(file))
                return file;
            Debug.Assert(this._executableFile.Directory != null, "ExecutableFile.Directory != null");
            return Path.Combine(this._executableFile.Directory.FullName, file);
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            foreach (string allKey in this._cfg.AppSettings.Settings.AllKeys)
            {
                string configLine = allKey + ": " + this._cfg.AppSettings.Settings[allKey].Value;
                yield return configLine;
            }
        }

        /// <summary>
        /// Get an enumerator that represents the key/value pairs of this configuration
        /// </summary>
        /// <returns>an IEnumerator object to enumerate through this configuration</returns>
        public IEnumerator GetEnumerator()
        {
            StringCollection stringCollection = new StringCollection();
            foreach (string allKey in this._cfg.AppSettings.Settings.AllKeys)
            {
                string str = allKey + ": " + this._cfg.AppSettings.Settings[allKey].Value;
                stringCollection.Add(str);
            }

            return (IEnumerator) stringCollection.GetEnumerator();
        }
    }
}