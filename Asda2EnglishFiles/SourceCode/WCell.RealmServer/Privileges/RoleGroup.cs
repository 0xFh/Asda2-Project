using NLog;
using System;
using System.Collections.Generic;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Commands;
using WCell.Util.Commands;

namespace WCell.RealmServer.Privileges
{
    /// <summary>Defines a group with specific traits and permissions.</summary>
    [Serializable]
    public class RoleGroup : IComparable<RoleGroup>, IComparable<int>, IRoleGroup
    {
        private readonly HashSet<Command<RealmServerCmdArgs>> m_commands = new HashSet<Command<RealmServerCmdArgs>>();

        /// <summary>Default constructor.</summary>
        public RoleGroup()
        {
            this.Name = "";
            this.Rank = 0;
            this.InheritanceList = new string[0];
            this.CommandNames = new string[0];
        }

        public RoleGroup(RoleGroupInfo info)
        {
            this.Name = info.Name;
            this.Rank = info.Rank;
            this.Status = info.Status;
            this.AppearAsGM = info.AppearAsGM;
            this.AppearAsQA = info.AppearAsQA;
            this.CanUseCommandsOnOthers = info.CanUseCommandsOnOthers;
            this.CanHandleTickets = info.CanHandleTickets;
            this.MaySkipAuthQueue = info.MaySkipAuthQueue;
            this.ScrambleChat = info.ScrambleChat;
            this.InheritanceList = info.InheritanceList == null ? new string[0] : info.InheritanceList;
            this.CommandNames = info.CommandNames;
            foreach (string commandName in info.CommandNames)
            {
                if (commandName == "*")
                {
                    using (IEnumerator<Command<RealmServerCmdArgs>> enumerator =
                        RealmCommandHandler.Instance.Commands.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Command<RealmServerCmdArgs> current = enumerator.Current;
                            if (!this.m_commands.Contains(current))
                                this.m_commands.Add(current);
                        }

                        break;
                    }
                }
                else if (commandName == "#")
                {
                    foreach (Command<RealmServerCmdArgs> command in (IEnumerable<Command<RealmServerCmdArgs>>)
                        RealmCommandHandler.Instance.Commands)
                    {
                        if (command is RealmServerCommand &&
                            info.Status >= ((RealmServerCommand) command).RequiredStatusDefault)
                            this.m_commands.Add(command);
                    }
                }
                else
                {
                    Command<RealmServerCmdArgs> command = RealmCommandHandler.Instance[commandName];
                    if (command != null)
                        this.m_commands.Add(command);
                    else
                        LogManager.GetCurrentClassLogger().Warn("Invalid Command \"{0}\" specified for Role \"{1}\"",
                            (object) commandName, (object) info);
                }
            }
        }

        /// <summary>The name of the role.</summary>
        public string Name { get; set; }

        /// <summary>RoleStatus indicates the relevance of a role</summary>
        public RoleStatus Status { get; set; }

        /// <summary>Whether the player is a staff-member</summary>
        public bool IsStaff
        {
            get { return this.Status >= RoleStatus.Staff; }
        }

        /// <summary>
        /// Whether the player can always login, even if the Realm is full
        /// </summary>
        public bool MaySkipAuthQueue { get; set; }

        /// <summary>Whether the player's chat will be scrambled</summary>
        public bool ScrambleChat { get; set; }

        /// <summary>Whether or not the role makes the player a GM.</summary>
        public bool AppearAsGM { get; set; }

        /// <summary>Whether or not the role makes the player a QA.</summary>
        public bool AppearAsQA { get; set; }

        /// <summary>The actual Rank of this Role</summary>
        public int Rank { get; set; }

        public bool CanUseCommandsOnOthers { get; set; }

        public bool CanHandleTickets { get; set; }

        /// <summary>
        /// A list of the other roles the role inherits from, permissions-wise.
        /// </summary>
        public string[] InheritanceList { get; set; }

        /// <summary>A list of the names of all allowed Commands.</summary>
        public string[] CommandNames { get; set; }

        /// <summary>A list of all allowed Commands.</summary>
        public HashSet<Command<RealmServerCmdArgs>> Commands
        {
            get { return this.m_commands; }
        }

        public override bool Equals(object obj)
        {
            if ((object) (obj as RoleGroup) != null)
                return ((RoleGroup) obj).Rank == this.Rank;
            return false;
        }

        public override int GetHashCode()
        {
            return this.Rank;
        }

        public int CompareTo(int other)
        {
            return this.Rank - other;
        }

        public int CompareTo(RoleGroup other)
        {
            return this.Rank - other.Rank;
        }

        public static bool operator >(RoleGroup left, RoleGroup right)
        {
            if ((object) right != null)
                return left.Rank > right.Rank;
            return true;
        }

        public static bool operator >=(RoleGroup left, RoleGroup right)
        {
            if ((object) right != null)
                return left.Rank >= right.Rank;
            return true;
        }

        public static bool operator <(RoleGroup left, RoleGroup right)
        {
            if ((object) right != null)
                return left.Rank < right.Rank;
            return false;
        }

        public static bool operator <=(RoleGroup left, RoleGroup right)
        {
            if ((object) right != null)
                return left.Rank <= right.Rank;
            return false;
        }

        public static bool operator >(RoleGroup left, int right)
        {
            return left.Rank > right;
        }

        public static bool operator <(RoleGroup left, int right)
        {
            return left.Rank < right;
        }

        public static bool operator >=(RoleGroup left, int right)
        {
            return left.Rank >= right;
        }

        public static bool operator <=(RoleGroup left, int right)
        {
            return left.Rank <= right;
        }

        public static bool operator ==(RoleGroup left, int right)
        {
            return left.Rank == right;
        }

        public static bool operator !=(RoleGroup left, int right)
        {
            return left.Rank != right;
        }

        public static bool operator >(RoleGroup left, RoleStatus right)
        {
            return left.Status > right;
        }

        public static bool operator <(RoleGroup left, RoleStatus right)
        {
            return left.Status < right;
        }

        public static bool operator >=(RoleGroup left, RoleStatus right)
        {
            return left.Status >= right;
        }

        public static bool operator <=(RoleGroup left, RoleStatus right)
        {
            return left.Status <= right;
        }

        public static bool operator ==(RoleGroup left, RoleStatus right)
        {
            return left.Status == right;
        }

        public static bool operator !=(RoleGroup left, RoleStatus right)
        {
            return left.Status != right;
        }

        public override string ToString()
        {
            return this.Name + " (Rank: " + (object) this.Rank + ")";
        }

        public bool MayUse(Command<RealmServerCmdArgs> cmd)
        {
            return this.Commands.Contains(cmd);
        }
    }
}