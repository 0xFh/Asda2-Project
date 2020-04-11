using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Util;

namespace WCell.RealmServer.Guilds
{
    /// <summary>
    /// Represents the relationship between a Character and its Guild.
    /// </summary>
    [Castle.ActiveRecord.ActiveRecord("GuildMember", Access = PropertyAccess.Property)]
    public class GuildMember : WCellRecord<GuildMember>, INamed
    {
        [Field("Name", NotNull = true)] private string _name;
        [Field("LastLvl", NotNull = true)] private int _lastLevel;
        [Field("LastLogin", NotNull = true)] private DateTime _lastLogin;
        [Field("LastZone", NotNull = true)] private int _lastZoneId;
        [Field("Class", NotNull = true)] private int _class;
        [Field("Rank", NotNull = true)] private int _rankId;
        [Field("GuildId", NotNull = true)] private int m_GuildId;
        [Field("PublicNote")] private string _publicNote;
        [Field("OfficerNote")] private string _officerNote;
        [Field("BankRemainingMoneyAllowance")] private int _remainingMoneyAllowance;

        [Field("BankMoneyAllowanceResetTime", NotNull = true)]
        private DateTime _moneyAllowanceResetTime;

        private Character m_chr;
        private Guild m_Guild;

        [PrimaryKey(PrimaryKeyType.Assigned)] public long CharacterLowId { get; private set; }

        public uint GuildId
        {
            get { return (uint) this.m_GuildId; }
            set { this.m_GuildId = (int) value; }
        }

        [Property] public byte CharNum { get; set; }

        [Property] public int AccId { get; set; }

        [Property] public byte ProffessionLevel { get; set; }

        /// <summary>Loads all members of the given guild from the DB</summary>
        public static GuildMember[] FindAll(uint guildId)
        {
            return ActiveRecordBase<GuildMember>.FindAllByProperty("m_GuildId", (object) (int) guildId);
        }

        public GuildMember(CharacterRecord chr, Guild guild, GuildRank rank)
            : this()
        {
            int zone = (int) chr.Zone;
            this.Guild = guild;
            this.CharacterLowId = (long) (int) chr.EntityLowId;
            this._rankId = rank.RankIndex;
            this._name = chr.Name;
            this._lastLevel = chr.Level;
            this._lastLogin = DateTime.Now;
            this._lastZoneId = zone;
            this._class = (int) chr.Class;
            this._publicNote = string.Empty;
            this._officerNote = string.Empty;
            this.AccId = chr.AccountId;
            this.ProffessionLevel = chr.ProfessionLevel;
            this.CharNum = chr.CharNum;
        }

        /// <summary>
        /// The low part of the Character's EntityId. Use EntityId.GetPlayerId(Id) to get a full EntityId
        /// </summary>
        public uint Id
        {
            get { return (uint) this.CharacterLowId; }
        }

        /// <summary>The name of this GuildMember's character</summary>
        public string Name
        {
            get { return this._name; }
        }

        public Guild Guild
        {
            get { return this.m_Guild; }
            private set
            {
                this.m_Guild = value;
                this.GuildId = value.Id;
            }
        }

        public string PublicNote
        {
            get { return this._publicNote; }
            set
            {
                this._publicNote = value;
                Asda2GuildHandler.SendGuildNotificationResponse(this.Guild, GuildNotificationType.EditPublicMessage,
                    this);
                this.SaveLater();
            }
        }

        public string OfficerNote
        {
            get { return this._officerNote; }
            set
            {
                this._officerNote = value;
                this.SaveLater();
            }
        }

        /// <summary>
        /// Current level of this GuildMember or his saved level if member already logged out
        /// </summary>
        public int Level
        {
            get
            {
                if (this.m_chr != null)
                    return this.m_chr.Level;
                return this._lastLevel;
            }
            internal set { this._lastLevel = value; }
        }

        /// <summary>Time of last login of member</summary>
        public DateTime LastLogin
        {
            get
            {
                if (this.m_chr != null)
                    return DateTime.Now;
                return this._lastLogin;
            }
            internal set { this._lastLogin = value; }
        }

        /// <summary>
        /// The current or last Id of zone in which this GuildMember was
        /// </summary>
        public int ZoneId
        {
            get
            {
                if (this.m_chr == null)
                    return this._lastZoneId;
                if (this.m_chr.Zone == null)
                    return 0;
                return (int) this.m_chr.Zone.Id;
            }
            internal set { this._lastZoneId = value; }
        }

        /// <summary>Class of GuildMember</summary>
        public ClassId Class
        {
            get
            {
                if (this.m_chr != null)
                    return this.m_chr.Class;
                return (ClassId) this._class;
            }
            internal set { this._class = (int) value; }
        }

        public GuildRank Rank
        {
            get { return this.Guild.Ranks[this.RankId]; }
        }

        public int RankId
        {
            get { return this._rankId; }
            set
            {
                this._rankId = value;
                if (this.m_chr != null)
                {
                    this.m_chr.GuildRank = (uint) value;
                    foreach (GuildBankTabRights guildBankTabRights in
                        ((IEnumerable<GuildBankTabRights>) this.m_chr.GuildMember.Rank.BankTabRights)
                        .Where<GuildBankTabRights>((Func<GuildBankTabRights, bool>) (right => right != null)))
                        guildBankTabRights.GuildRankId = (uint) value;
                }

                this.SaveLater();
            }
        }

        /// <summary>
        /// Returns the Character or null, if this member is offline
        /// </summary>
        public Character Character
        {
            get { return this.m_chr; }
            internal set
            {
                this.m_chr = value;
                if (this.m_chr == null)
                    return;
                this._name = this.m_chr.Name;
                this.m_chr.GuildMember = this;
            }
        }

        public uint BankMoneyWithdrawlAllowance
        {
            get
            {
                if (this.IsLeader)
                    return uint.MaxValue;
                if (DateTime.Now >= this.BankMoneyAllowanceResetTime)
                {
                    this.BankMoneyAllowanceResetTime = DateTime.Now.AddHours(24.0);
                    this._remainingMoneyAllowance = (int) this.Rank.DailyBankMoneyAllowance;
                }

                return (uint) this._remainingMoneyAllowance;
            }
            set { this._remainingMoneyAllowance = (int) value; }
        }

        public DateTime BankMoneyAllowanceResetTime
        {
            get { return this._moneyAllowanceResetTime; }
            set { this._moneyAllowanceResetTime = value; }
        }

        public bool IsLeader
        {
            get { return this.Guild.Leader == this; }
        }

        public byte Asda2RankId
        {
            get { return (byte) (4 - this.RankId); }
            set { this.RankId = 4 - (int) value; }
        }

        internal GuildMember()
        {
        }

        internal void Init(Guild guild, Character chr)
        {
            this.Guild = guild;
            this.Character = chr;
        }

        /// <summary>Removes this member from its Guild</summary>
        public void LeaveGuild(bool kicked = false)
        {
            this.Guild.RemoveMember(this, true, kicked);
        }

        public bool HasBankTabRight(int tabId, GuildBankTabPrivileges privilege)
        {
            if (this.Rank == null || this.Rank.BankTabRights[tabId] == null)
                return false;
            return this.Rank.BankTabRights[tabId].Privileges.HasAnyFlag(privilege);
        }

        public override string ToString()
        {
            return "GuildMember: " + this.Name;
        }

        public bool HasRight(GuildPrivileges privilege)
        {
            if (!this.IsLeader)
                return this.Rank.Privileges.HasAnyFlag(privilege);
            return true;
        }

        public void SaveLater()
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage(
                new Action(((ActiveRecordBase) this).SaveAndFlush));
        }
    }
}