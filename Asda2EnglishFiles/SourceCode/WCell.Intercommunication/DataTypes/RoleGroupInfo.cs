using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WCell.Intercommunication.DataTypes
{
    [DataContract]
    [Serializable]
    public class RoleGroupInfo : IRoleGroup
    {
        /// <summary>
        /// Represents the command name that allows all commands for a Role.
        /// </summary>
        public const string AllCommands = "*";

        /// <summary>
        /// Represents the command name that allows all Commands that belong to the Role's status by default.
        /// </summary>
        public const string StatusCommands = "#";

        /// <summary>
        /// Represents the highest role that has been loaded (usually: Owner).
        /// </summary>
        public static RoleGroupInfo HighestRole;

        /// <summary>
        /// Represents the lowest role that has been loaded (usually: Guest).
        /// </summary>
        public static RoleGroupInfo LowestRole;

        private int m_rank;

        public static List<RoleGroupInfo> CreateDefaultGroups()
        {
            List<RoleGroupInfo> roleGroupInfoList = new List<RoleGroupInfo>();
            string[] commands = new string[1] {"*"};
            string[] strArray = new string[1] {"#"};
            roleGroupInfoList.Add(new RoleGroupInfo("Guest", 0, RoleStatus.Player, false, false, false, false, false,
                true, strArray, strArray));
            roleGroupInfoList.Add(new RoleGroupInfo("Player", 1, RoleStatus.Player, false, false, false, false, false,
                true, new string[1]
                {
                    "Guest"
                }, strArray));
            roleGroupInfoList.Add(new RoleGroupInfo("Vip", 5, RoleStatus.Player, false, false, false, false, true, true,
                new string[1]
                {
                    "Player"
                }, strArray));
            roleGroupInfoList.Add(new RoleGroupInfo("EM", 4, RoleStatus.EventManager, false, false, false, false, true,
                true, new string[1]
                {
                    "Player"
                }, strArray));
            roleGroupInfoList.Add(new RoleGroupInfo("QA", 100, RoleStatus.Staff, false, true, false, true, true, false,
                new string[1]
                {
                    "Vip"
                }, strArray));
            roleGroupInfoList.Add(new RoleGroupInfo("GM", 500, RoleStatus.Staff, true, false, true, true, true, false,
                new string[1]
                {
                    "QA"
                }, strArray));
            roleGroupInfoList.Add(new RoleGroupInfo("Developer", 1000, RoleStatus.Admin, true, false, true, true, true,
                false, new string[1]
                {
                    "GM"
                }, strArray));
            roleGroupInfoList.Add(new RoleGroupInfo("Admin", 5000, RoleStatus.Admin, true, false, true, true, true,
                false, new string[1]
                {
                    "GM"
                }, strArray));
            roleGroupInfoList.Add(new RoleGroupInfo("Owner", 10000, RoleStatus.Admin, true, false, true, true, true,
                false, new string[1]
                {
                    "Admin"
                }, commands));
            return roleGroupInfoList;
        }

        /// <summary>Default constructor.</summary>
        public RoleGroupInfo()
        {
            this.Name = "";
            this.Rank = 0;
            this.InheritanceList = new string[0];
            this.CommandNames = new string[0];
        }

        /// <summary>Default constructor.</summary>
        /// <param name="roleName">the name of the role</param>
        /// <param name="gm">whether or not this role makes you a GM</param>
        /// <param name="qa">whether or not this role makes you a QA</param>
        /// <param name="inherits">the other roles this role inherits from</param>
        public RoleGroupInfo(string roleName, int rank, RoleStatus status, bool gm, bool qa, bool canCommandOthers,
            bool canHandleTickets, bool maySkipAuthQueue, bool scrambleChat, string[] inherits)
            : this(roleName, rank, status, gm, qa, canCommandOthers, canHandleTickets, maySkipAuthQueue, scrambleChat,
                inherits, (string[]) null)
        {
        }

        /// <summary>Default constructor.</summary>
        /// <param name="roleName">the name of the role</param>
        /// <param name="gm">whether or not this role makes you a GM</param>
        /// <param name="qa">whether or not this role makes you a QA</param>
        /// <param name="inherits">the other roles this role inherits from</param>
        public RoleGroupInfo(string roleName, int rank, RoleStatus status, bool gm, bool qa, bool canCommandOthers,
            bool canHandleTickets, bool maySkipAuthQueue, bool scrambleChat, string[] inherits, string[] commands)
        {
            this.Name = roleName;
            this.Rank = rank;
            this.Status = status;
            this.AppearAsGM = gm;
            this.AppearAsQA = qa;
            this.InheritanceList = inherits;
            this.CommandNames = commands;
            this.CanUseCommandsOnOthers = canCommandOthers;
            this.CanHandleTickets = canHandleTickets;
            this.MaySkipAuthQueue = maySkipAuthQueue;
            this.ScrambleChat = scrambleChat;
            if (RoleGroupInfo.HighestRole == null || RoleGroupInfo.HighestRole.Rank < rank)
                RoleGroupInfo.HighestRole = this;
            if (RoleGroupInfo.LowestRole != null && RoleGroupInfo.LowestRole.Rank <= this.m_rank)
                return;
            RoleGroupInfo.LowestRole = this;
        }

        /// <summary>The name of the role.</summary>
        [XmlAttribute]
        [DataMember]
        public string Name { get; set; }

        /// <summary>What kind of status this roll represents</summary>
        [XmlAttribute]
        [DataMember]
        public RoleStatus Status { get; set; }

        /// <summary>
        /// Whether the User may login, even if the server is full.
        /// </summary>
        [XmlAttribute]
        [DataMember]
        public bool MaySkipAuthQueue { get; set; }

        /// <summary>Whether the player's chat will be scrambled</summary>
        [DataMember]
        [XmlAttribute]
        public bool ScrambleChat { get; set; }

        public bool IsStaff
        {
            get { return this.Status >= RoleStatus.Staff; }
        }

        /// <summary>Whether or not the role makes the player a GM.</summary>
        [DataMember]
        [XmlAttribute]
        public bool AppearAsGM { get; set; }

        /// <summary>Whether or not the role makes the player a QA.</summary>
        [XmlAttribute]
        [DataMember]
        public bool AppearAsQA { get; set; }

        /// <summary>The actual Rank of this Role</summary>
        [DataMember]
        [XmlAttribute]
        public int Rank
        {
            get { return this.m_rank; }
            set
            {
                this.m_rank = value;
                if (RoleGroupInfo.HighestRole == null || RoleGroupInfo.HighestRole.Rank < this.m_rank)
                    RoleGroupInfo.HighestRole = this;
                if (RoleGroupInfo.LowestRole != null && RoleGroupInfo.LowestRole.Rank <= this.m_rank)
                    return;
                RoleGroupInfo.LowestRole = this;
            }
        }

        /// <summary>
        /// Whether this Role is allowed to call commands on others (eg. using double prefix)
        /// </summary>
        [XmlAttribute]
        [DataMember]
        public bool CanUseCommandsOnOthers { get; set; }

        /// <summary>
        /// Whether this Role sees ticket information and can handle tickets
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public bool CanHandleTickets { get; set; }

        /// <summary>
        /// A list of the other roles the role inherits from, permissions-wise.
        /// </summary>
        [XmlArray("Inheritance")]
        [DataMember]
        [XmlArrayItem("InheritsFrom")]
        public string[] InheritanceList { get; set; }

        /// <summary>A list of the names of all allowed Commands.</summary>
        [XmlArray("Commands")]
        [XmlArrayItem("Command")]
        [DataMember]
        public string[] CommandNames { get; set; }

        public override string ToString()
        {
            return this.Name + " (Rank: " + (object) this.Rank + ")";
        }
    }
}