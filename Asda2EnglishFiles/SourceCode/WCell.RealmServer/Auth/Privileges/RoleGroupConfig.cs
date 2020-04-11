using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer;
using WCell.Util;

namespace WCell.AuthServer.Privileges
{
    /// <summary>
    /// Provides storage/loading of role groups from an XML configuration file.
    /// </summary>
    [Serializable]
    public class RoleGroupConfig : XmlFile<RoleGroupConfig>
    {
        public static readonly string[] EmptyNameArr = new string[0];

        private static List<string> EmptyNameList =
            ((IEnumerable<string>) RoleGroupConfig.EmptyNameArr).ToList<string>();

        /// <summary>A list of all role groups.</summary>
        [XmlArray("Privileges")]
        [XmlArrayItem("Privilege")]
        public RoleGroupInfo[] RoleGroups { get; set; }

        /// <summary>
        /// Tries to load the specified configuration file, creating a default
        /// configuration file if the specified one does not exist.
        /// </summary>
        /// <param name="fname">the name of the configuration file to load</param>
        /// <returns>a <see cref="T:WCell.AuthServer.Privileges.RoleGroupConfig" /> object representing the loaded file</returns>
        public static RoleGroupConfig LoadConfigOrDefault(string fname)
        {
            string fullPath = RealmServerConfiguration.GetFullPath(RealmServerConfiguration.ConfigDir);
            string str = Path.Combine(fullPath, fname);
            RoleGroupConfig roleGroupConfig =
                File.Exists(str) ? XmlFile<RoleGroupConfig>.Load(str) : (RoleGroupConfig) null;
            if (RoleGroupInfo.HighestRole == null || RoleGroupInfo.HighestRole.IsStaff)
            {
                roleGroupConfig = new RoleGroupConfig()
                {
                    RoleGroups = RoleGroupInfo.CreateDefaultGroups().ToArray()
                };
                roleGroupConfig.SaveAs(fname, fullPath);
            }

            return roleGroupConfig;
        }
    }
}