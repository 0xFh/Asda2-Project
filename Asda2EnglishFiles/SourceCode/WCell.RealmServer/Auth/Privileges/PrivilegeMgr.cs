using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer;

namespace WCell.AuthServer.Privileges
{
    /// <summary>
    /// Handles the management of role groups, and their permissions.
    /// </summary>
    public class PrivilegeMgr : Manager<PrivilegeMgr>
    {
        /// <summary>The location of the configuration file within the</summary>
        public static string RoleGroupFile = "RoleGroups.xml";

        private FileSystemWatcher m_configWatcher;
        private Dictionary<string, RoleGroupInfo> m_roleGroups;

        /// <summary>Default constructor</summary>
        private PrivilegeMgr()
        {
            string configDir = RealmServerConfiguration.ConfigDir;
            if (!File.Exists(configDir))
                Directory.CreateDirectory(configDir);
            this.m_configWatcher = new FileSystemWatcher(configDir);
            this.m_configWatcher.Changed += new FileSystemEventHandler(this.ConfigChanged);
        }

        public IDictionary<string, RoleGroupInfo> RoleGroups
        {
            get { return (IDictionary<string, RoleGroupInfo>) this.m_roleGroups; }
        }

        protected void LoadConfig()
        {
            this.LoadConfiguration();
            string defaultRole = RealmServerConfiguration.DefaultRole;
            if (!Singleton<PrivilegeMgr>.Instance.Exists(defaultRole))
                throw new Exception("Default Role (Config: DefaultRole) does not exist: " + defaultRole);
        }

        public void LoadConfiguration()
        {
            RoleGroupConfig roleGroupConfig = RoleGroupConfig.LoadConfigOrDefault(PrivilegeMgr.RoleGroupFile);
            this.m_roleGroups =
                new Dictionary<string, RoleGroupInfo>(
                    (IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
            foreach (RoleGroupInfo roleGroup in roleGroupConfig.RoleGroups)
                this.m_roleGroups.Add(roleGroup.Name, roleGroup);
        }

        private void ConfigChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed || !(e.Name == PrivilegeMgr.RoleGroupFile))
                return;
            LogManager.GetCurrentClassLogger().Info("Privilege config changed");
            try
            {
                this.LoadConfig();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to reload Configuration.", ex);
            }
        }

        /// <summary>Clears all currently-loaded commands and roles.</summary>
        public void ClearConfiguration()
        {
            this.m_roleGroups.Clear();
        }

        public bool Exists(string privLevelName)
        {
            return this.m_roleGroups.ContainsKey(privLevelName);
        }

        /// <summary>Gets a role group by name.</summary>
        /// <returns>the RoleGroup if it exists; null otherwise</returns>
        public RoleGroupInfo GetRoleGroup(string roleGroupName)
        {
            RoleGroupInfo roleGroupInfo;
            if (this.m_roleGroups.TryGetValue(roleGroupName, out roleGroupInfo))
                return roleGroupInfo;
            return (RoleGroupInfo) null;
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Fourth, "Privilege manager")]
        public static void Initialize()
        {
            Singleton<PrivilegeMgr>.Instance.LoadConfig();
        }

        /// <summary>
        /// Returns the given PrivLevel or the default one, if role is invalid.
        /// </summary>
        public string GetRoleOrDefault(string role)
        {
            RoleGroupInfo roleGroupInfo;
            if (this.m_roleGroups.TryGetValue(role, out roleGroupInfo))
                return roleGroupInfo.Name;
            return RealmServerConfiguration.DefaultRole;
        }
    }
}