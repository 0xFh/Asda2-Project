using Castle.ActiveRecord;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Guilds;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.NLog;
using WCell.Util.Synchronization;
using WCell.Util.Threading;

namespace WCell.RealmServer.Guilds
{
    [Castle.ActiveRecord.ActiveRecord("Guild", Access = PropertyAccess.Property)]
    public class Guild : ActiveRecordBase<Guild>, INamed, IEnumerable<GuildMember>, IEnumerable, IChatTarget,
        IGenericChatTarget
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(Guild), nameof(_id), 1L);
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public readonly IDictionary<uint, GuildMember> Members =
            (IDictionary<uint, GuildMember>) new Dictionary<uint, GuildMember>();

        private Dictionary<GuildMember, GuildMember> _acceptedMembers = new Dictionary<GuildMember, GuildMember>();

        [Field("Name", NotNull = true, Unique = true)]
        private string _name;

        [Field("MOTD", NotNull = true)] private string _MOTD;
        [Field("Info", NotNull = true)] private string _info;
        [Field("Created", NotNull = true)] private DateTime _created;
        [Field("LeaderLowId", NotNull = true)] private int _leaderLowId;
        private byte _level;
        private uint _points;
        private byte[] _clanCrest;
        private string _noticeWriter;
        private DateTime _noticeDateTime;
        internal SimpleLockWrapper syncRoot;
        private GuildMember m_leader;
        private ImmutableList<GuildRank> m_ranks;
        private List<HistoryRecord> _history;
        private GuildSkill[] _skills;
        private GuildMember _newLeader;
        private DateTime _impeachmentStartTime;

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        private long _id { get; set; }

        public uint LeaderLowId
        {
            get { return (uint) this._leaderLowId; }
        }

        [Nested("Tabard")] private GuildTabard _tabard { get; set; }

        [Property] public int PurchasedBankTabCount { get; internal set; }

        [Property] public long Money { get; set; }

        [Property]
        public byte Level
        {
            get { return this._level; }
            set
            {
                this._level = value;
                this.UpdateLater();
            }
        }

        [Property] public byte MaxMembersCount { get; set; }

        [Property]
        public uint Points
        {
            get { return this._points; }
            set { this._points = value; }
        }

        [Property]
        public DateTime NoticeDateTime
        {
            get { return this._noticeDateTime; }
            set
            {
                this._noticeDateTime = value;
                this.UpdateLater();
            }
        }

        [Property(Length = 20)]
        public string NoticeWriter
        {
            get { return this._noticeWriter; }
            set
            {
                this._noticeWriter = value;
                this.UpdateLater();
            }
        }

        [Property(Length = 40)]
        public byte[] ClanCrest
        {
            get { return this._clanCrest; }
            set
            {
                this._clanCrest = value;
                this.UpdateLater();
                foreach (Character character in this.GetCharacters())
                    GlobalHandler.SendCharactrerInfoClanNameToAllNearbyCharacters(character);
            }
        }

        /// <summary>Id of this guild</summary>
        /// <remarks>UpdateField's GuildId is equal to it</remarks>
        public uint Id
        {
            get { return (uint) this._id; }
        }

        /// <summary>Guild's name</summary>
        /// <remarks>length is limited with MAX_GUILDNAME_LENGTH</remarks>
        public string Name
        {
            get { return this._name; }
            set
            {
                this._name = value;
                Asda2GuildHandler.SendUpdateGuildInfoResponse(this, GuildInfoMode.GuildNameChanged, (Character) null);
            }
        }

        /// <summary>Guild's message of the day</summary>
        /// <remarks>length is limited with MAX_GUILDMOTD_LENGTH</remarks>
        public string MOTD
        {
            get { return this._MOTD; }
            set
            {
                if (value != null && value.Length > GuildMgr.MaxGuildMotdLength)
                    return;
                this._MOTD = value;
                this.UpdateLater();
            }
        }

        /// <summary>Guild's information</summary>
        /// <remarks>length is limited with MAX_GUILDINFO_LENGTH</remarks>
        public string Info
        {
            get { return this._info; }
            set
            {
                if (value != null && value.Length > GuildMgr.MaxGuildInfoLength)
                    return;
                this._info = value;
                GuildHandler.SendGuildRosterToGuildMembers(this);
                this.UpdateLater();
            }
        }

        /// <summary>Date and time of guild creation</summary>
        public DateTime Created
        {
            get { return this._created; }
        }

        /// <summary>
        /// Guild leader's GuildMember
        /// Setting it does not send event to the guild. Use Guild.SendEvent
        /// </summary>
        public GuildMember Leader
        {
            get { return this.m_leader; }
            set
            {
                if (value == null || value.Guild != this)
                    return;
                this.m_leader = value;
                this._leaderLowId = (int) value.Id;
                this.UpdateLater();
            }
        }

        /// <summary>Guild's tabard</summary>
        public GuildTabard Tabard
        {
            get { return this._tabard; }
            set
            {
                this._tabard = value;
                this.UpdateLater();
            }
        }

        /// <summary>Number of guild's members</summary>
        public int MemberCount
        {
            get { return this.Members.Count; }
        }

        public GuildEventLog EventLog { get; private set; }

        public GuildBank Bank { get; private set; }

        /// <summary>
        /// Constructor is implicitely called when Guild is loaded from DB
        /// </summary>
        public Guild()
            : this(false)
        {
        }

        protected Guild(bool isNew)
        {
            this.syncRoot = new SimpleLockWrapper();
            this.EventLog = new GuildEventLog(this, isNew);
            this.Bank = new GuildBank(this, isNew);
        }

        /// <summary>
        /// Creates a new GuildRecord row in the database with the given information.
        /// </summary>
        /// <param name="leader">leader's character record</param>
        /// <param name="name">the name of the new character</param>
        /// <returns>the <seealso cref="T:WCell.RealmServer.Guilds.Guild" /> object</returns>
        public Guild(CharacterRecord leader, string name)
            : this(true)
        {
            this._created = DateTime.Now;
            this._id = Guild._idGenerator.Next();
            this._leaderLowId = (int) leader.EntityLowId;
            this._name = name;
            this._tabard = new GuildTabard();
            this._MOTD = "Default MOTD";
            this._info = "Default info";
            this._level = (byte) 1;
            this.m_ranks = GuildMgr.CreateDefaultRanks(this);
            foreach (ActiveRecordBase rank in this.m_ranks)
                rank.CreateLater();
            this.Register();
            this.m_leader = this.AddMember(leader);
            this.Leader.Character = World.GetCharacter(leader.Name, true);
            this.m_leader.RankId = 0;
            this._clanCrest = new byte[40];
            this.CreateLater();
        }

        /// <summary>Initialize the Guild after loading from DB</summary>
        internal void InitAfterLoad()
        {
            GuildRank[] all = GuildRank.FindAll(this);
            if (all.Length == 0)
            {
                Guild.log.Warn(string.Format("Guild {0} did not have ranks - Recreating default Ranks.",
                    (object) this));
                this.m_ranks = GuildMgr.CreateDefaultRanks(this);
            }
            else
                this.m_ranks = new ImmutableList<GuildRank>(
                    (IEnumerable<GuildRank>) ((IEnumerable<GuildRank>) all).OrderBy<GuildRank, int>(
                        (Func<GuildRank, int>) (rank => rank.RankIndex)));

            foreach (GuildRank rank in this.m_ranks)
                rank.InitRank();
            foreach (GuildMember guildMember in GuildMember.FindAll(this.Id))
            {
                guildMember.Init(this, World.GetCharacter((uint) guildMember.CharacterLowId));
                this.Members.Add(guildMember.Id, guildMember);
            }

            foreach (GuildSkill record in GuildSkill.FindAll(this))
            {
                record.InitAfterLoad(this);
                if (this.Skills[(int) record.Id] == null)
                    this.Skills[(int) record.Id] = record;
                else
                    record.DeleteLater();
            }

            this.m_leader = this[this.LeaderLowId];
            if (this.m_leader == null)
                this.OnNoLeaderFound();
            if (this.m_leader == null)
                return;
            this.Register();
        }

        /// <summary>
        /// Initializes guild after its creation or restoration from DB
        /// </summary>
        internal void Register()
        {
            Singleton<GuildMgr>.Instance.RegisterGuild(this);
        }

        public GuildMember AddMember(Character chr)
        {
            GuildMember guildMember = this.AddMember(chr.Record);
            if (guildMember != null)
                guildMember.Character = chr;
            return guildMember;
        }

        /// <summary>
        /// Adds a new guild member
        /// Calls GuildMgr.OnJoinGuild
        /// </summary>
        /// <param name="chr">character to add</param>
        /// <param name="update">if true, sends event to the guild</param>
        /// <returns>GuildMember of new member</returns>
        public GuildMember AddMember(CharacterRecord chr)
        {
            GuildMember guildMember;
            lock (this)
            {
                if (this.Members.TryGetValue(chr.EntityLowId, out guildMember))
                    return guildMember;
                guildMember = new GuildMember(chr, this, this.m_ranks.Last<GuildRank>());
                this.Members.Add(guildMember.Id, guildMember);
                guildMember.Create();
            }

            Singleton<GuildMgr>.Instance.RegisterGuildMember(guildMember);
            this.EventLog.AddJoinEvent(guildMember.Id);
            GuildHandler.SendEventToGuild(this, GuildEvents.JOINED, guildMember);
            Character characterByAccId = World.GetCharacterByAccId((uint) chr.AccountId);
            foreach (GuildSkill activeSkill in this.ActiveSkills)
                activeSkill.ApplyToCharacter(characterByAccId);
            characterByAccId.GuildMember = guildMember;
            Asda2GuildHandler.SendGuildNotificationResponse(this, GuildNotificationType.Joined, guildMember);
            Asda2GuildHandler.SendGuildMembersInfoResponse(characterByAccId.Client, this);
            this.AddHistoryMessage(Asda2GuildHistoryType.Joined, 0, chr.Name, DateTime.Now.ToLongTimeString());
            return guildMember;
        }

        public IEnumerable<GuildSkill> ActiveSkills
        {
            get
            {
                return ((IEnumerable<GuildSkill>) this.Skills).Where<GuildSkill>((Func<GuildSkill, bool>) (s =>
                {
                    if (s != null)
                        return s.IsActivated;
                    return false;
                }));
            }
        }

        public bool RemoveMember(uint memberId)
        {
            GuildMember member = this[memberId];
            if (member != null)
                return this.RemoveMember(member, true, false);
            return false;
        }

        public bool RemoveMember(string name, bool kicked = false)
        {
            GuildMember member = this[name];
            if (member != null)
                return this.RemoveMember(member, true, kicked);
            return false;
        }

        /// <summary>Removes GuildMember from the guild</summary>
        /// <param name="member">member to remove</param>
        /// <param name="update">if true, sends event to the guild</param>
        public bool RemoveMember(GuildMember member)
        {
            return this.RemoveMember(member, true, false);
        }

        /// <summary>Removes GuildMember from the guild</summary>
        /// <param name="member">member to remove</param>
        /// <param name="update">if false, changes to the guild will not be promoted anymore (used when the Guild is being disbanded)</param>
        public bool RemoveMember(GuildMember member, bool update, bool kicked = false)
        {
            this.AddHistoryMessage(kicked ? Asda2GuildHistoryType.Kicked : Asda2GuildHistoryType.Left, 0, member.Name,
                DateTime.Now.ToLongTimeString());
            if (member.Character != null)
            {
                foreach (GuildSkill activeSkill in this.ActiveSkills)
                    activeSkill.RemoveFromCharacter(member.Character);
            }

            if (update)
            {
                this.EventLog.AddLeaveEvent(member.Id);
                Asda2GuildHandler.SendGuildNotificationResponse(this,
                    kicked ? GuildNotificationType.Kicked : GuildNotificationType.Left, member);
            }

            this.OnRemoveMember(member);
            if (update && member == this.m_leader)
            {
                this.Disband();
                return true;
            }

            lock (this)
            {
                if (!this.Members.Remove(member.Id))
                    return false;
            }

            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
            {
                member.Delete();
                if (!update)
                    return;
                this.Update();
            }));
            return true;
        }

        /// <summary>
        /// Called before the given member is removed to clean up everything related to the given member
        /// </summary>
        protected void OnRemoveMember(GuildMember member)
        {
            Singleton<GuildMgr>.Instance.UnregisterGuildMember(member);
            Character character = member.Character;
            if (character == null)
                return;
            character.GuildMember = (GuildMember) null;
        }

        private void OnNoLeaderFound()
        {
            int num = int.MaxValue;
            GuildMember guildMember1 = (GuildMember) null;
            foreach (GuildMember guildMember2 in (IEnumerable<GuildMember>) this.Members.Values)
            {
                if (guildMember2.RankId < num)
                {
                    num = guildMember2.RankId;
                    guildMember1 = guildMember2;
                }
            }

            if (guildMember1 != null)
                return;
            this.Disband();
        }

        public GuildRank HighestRank
        {
            get { return this.m_ranks[0]; }
        }

        public GuildRank LowestRank
        {
            get
            {
                GuildRank[] ranks = this.Ranks;
                return ranks[ranks.Length - 1];
            }
        }

        /// <summary>Guild ranks as an array</summary>
        public GuildRank[] Ranks
        {
            get
            {
                if (this.m_ranks != null)
                    return this.m_ranks.ToArray<GuildRank>();
                return (GuildRank[]) null;
            }
        }

        /// <summary>Adds rank to the tail of ranks list</summary>
        /// <param name="name">name of new rank</param>
        /// <param name="privileges">privileges of new rank</param>
        /// <param name="update">if true, sends event to the guild</param>
        /// <returns>new rank</returns>
        public GuildRank AddRank(string name, GuildPrivileges privileges, bool update)
        {
            GuildRank guildRank;
            lock (this)
            {
                foreach (GuildRank rank in this.m_ranks)
                {
                    if (rank.Name == name)
                        return (GuildRank) null;
                }

                if (this.m_ranks.Count >= 10)
                    return (GuildRank) null;
                guildRank = new GuildRank(this, name, privileges, this.m_ranks.Count);
                this.m_ranks.Add(guildRank);
            }

            guildRank.SaveLater();
            if (update)
            {
                GuildHandler.SendGuildQueryToGuildMembers(this);
                GuildHandler.SendGuildRosterToGuildMembers(this);
            }

            return guildRank;
        }

        /// <summary>Deletes last rank from ranks list</summary>
        /// <param name="update">if true, sends event to the guild</param>
        public void RemoveRank(bool update)
        {
            try
            {
                if (this.m_ranks.Count <= 5)
                    return;
                int lastRankId = this.m_ranks.Count - 1;
                foreach (GuildMember guildMember in (IEnumerable<GuildMember>) this.Members.Values)
                {
                    if (guildMember.RankId == lastRankId)
                        guildMember.RankId = lastRankId - 1;
                }

                this.m_ranks.RemoveAt(lastRankId);
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                    this.m_ranks[lastRankId].Delete()));
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex, string.Format("Could not delete rank from guild {0}", (object) this),
                    new object[0]);
            }

            if (!update)
                return;
            GuildHandler.SendGuildQueryToGuildMembers(this);
            GuildHandler.SendGuildRosterToGuildMembers(this);
        }

        /// <summary>Changes priviliges and name of a rank</summary>
        /// <param name="rankId">Id of rank to modify</param>
        /// <param name="newName">new name of rank</param>
        /// <param name="newPrivileges">new priviliges of rank</param>
        /// <param name="update">if true, sends event to the guild</param>
        public void ChangeRank(int rankId, string newName, GuildPrivileges newPrivileges, bool update)
        {
            foreach (GuildRank rank in this.m_ranks)
            {
                if (rank.Name == newName && rank.RankIndex != rankId)
                    return;
            }

            try
            {
                if (this.m_ranks.Count <= rankId)
                    return;
                this.m_ranks[rankId].Name = newName;
                this.m_ranks[rankId].Privileges = newPrivileges;
                this.m_ranks[rankId].SaveLater();
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex, string.Format("Could not modify rank in guild {0}", (object) this),
                    new object[0]);
            }

            if (!update)
                return;
            GuildHandler.SendGuildQueryToGuildMembers(this);
            GuildHandler.SendGuildRosterToGuildMembers(this);
        }

        /// <summary>
        /// Promotes GuildMember. It's impossible to promote to guild leader
        /// </summary>
        /// <param name="member">member to promote</param>
        /// <returns>true, if success</returns>
        public bool Promote(GuildMember member)
        {
            if (member.Rank.RankIndex <= 1)
                return false;
            --member.RankId;
            member.UpdateLater();
            return true;
        }

        /// <summary>Demotes GuildMember</summary>
        /// <param name="member">member to promote</param>
        /// <returns>true, if success</returns>
        public bool Demote(GuildMember member)
        {
            if (member.Rank.RankIndex >= this.m_ranks.Count - 1)
                return false;
            ++member.RankId;
            member.UpdateLater();
            return true;
        }

        /// <summary>Requests member by his low id</summary>
        /// <param name="lowMemberId">low id of member's character</param>
        /// <returns>requested member or null</returns>
        public GuildMember this[uint lowMemberId]
        {
            get
            {
                foreach (GuildMember guildMember in (IEnumerable<GuildMember>) this.Members.Values)
                {
                    if ((int) guildMember.Id == (int) lowMemberId)
                        return guildMember;
                }

                return (GuildMember) null;
            }
        }

        /// <summary>Requests member by his name</summary>
        /// <param name="name">name of member's character (not case-sensitive)</param>
        /// <returns>requested member</returns>
        public GuildMember this[string name]
        {
            get
            {
                name = name.ToLower();
                foreach (GuildMember guildMember in (IEnumerable<GuildMember>) this.Members.Values)
                {
                    if (guildMember.Name.ToLower() == name)
                        return guildMember;
                }

                return (GuildMember) null;
            }
        }

        /// <summary>Disbands the guild</summary>
        /// <param name="update">if true, sends event to the guild</param>
        public void Disband()
        {
            lock (this)
            {
                GuildHandler.SendEventToGuild(this, GuildEvents.DISBANDED);
                foreach (GuildMember member in this.Members.Values.ToArray<GuildMember>())
                    this.RemoveMember(member, false, false);
                Singleton<GuildMgr>.Instance.UnregisterGuild(this);
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() => this.Delete()));
            }
        }

        /// <summary>Changes leader of the guild</summary>
        /// <param name="newLeader">GuildMember of new leader</param>
        /// <param name="update">if true, sends event to the guild</param>
        public void ChangeLeader(GuildMember newLeader)
        {
            if (newLeader.Guild != this)
                return;
            GuildMember currentLeader;
            lock (this)
            {
                currentLeader = this.Leader;
                if (currentLeader != null)
                    currentLeader.RankId = 1;
                newLeader.RankId = 0;
                this.Leader = newLeader;
            }

            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((IMessage) new Message((Action) (() =>
            {
                if (currentLeader != null)
                    currentLeader.Update();
                newLeader.Update();
                this.Update();
            })));
            if (currentLeader == null)
                return;
            GuildHandler.SendEventToGuild(this, GuildEvents.LEADER_CHANGED, newLeader, currentLeader);
        }

        public void TrySetTabard(GuildMember member, NPC vendor, GuildTabard tabard)
        {
            Character character = member.Character;
            if (character == null)
                return;
            if (!vendor.IsTabardVendor || !vendor.CheckVendorInteraction(character))
                GuildHandler.SendTabardResult((IPacketReceiver) character, GuildTabardResult.InvalidVendor);
            else if (!member.IsLeader)
                GuildHandler.SendTabardResult((IPacketReceiver) character, GuildTabardResult.NotGuildMaster);
            else if (character.Money < GuildMgr.GuildTabardCost)
            {
                GuildHandler.SendTabardResult((IPacketReceiver) character, GuildTabardResult.NotEnoughMoney);
            }
            else
            {
                character.SubtractMoney(GuildMgr.GuildTabardCost);
                this.Tabard = tabard;
                GuildHandler.SendTabardResult((IPacketReceiver) character, GuildTabardResult.Success);
                GuildHandler.SendGuildQueryResponse((IPacketReceiver) character, this);
            }
        }

        /// <summary>
        /// Check whether the given inviter may invite the given target
        /// Sends result to the inviter
        /// </summary>
        /// <param name="inviter">inviter's character, can be null. If null, sending result is suppressed</param>
        /// <param name="target">invitee's character, can be null</param>
        /// <param name="targetName">invitee character's name</param>
        /// <returs>result of invite</returs>
        public static GuildResult CheckInvite(Character inviter, Character target, string targetName = null)
        {
            GuildMember guildMember = inviter.GuildMember;
            GuildResult guildResult;
            if (guildMember == null)
                guildResult = GuildResult.PLAYER_NOT_IN_GUILD;
            else if (target == null)
                guildResult = GuildResult.PLAYER_NOT_FOUND;
            else if (inviter == target)
                guildResult = GuildResult.PERMISSIONS;
            else if (target.GuildMember != null)
                guildResult = GuildResult.ALREADY_IN_GUILD;
            else if (target.IsInvitedToGuild)
                guildResult = GuildResult.ALREADY_INVITED_TO_GUILD;
            else if ((int) inviter.Asda2FactionId != (int) target.Asda2FactionId)
            {
                guildResult = GuildResult.NOT_ALLIED;
            }
            else
            {
                if (inviter.Role.IsStaff)
                    return GuildResult.SUCCESS;
                if (!guildMember.HasRight(GuildPrivileges.InviteMembers))
                {
                    guildResult = GuildResult.PERMISSIONS;
                }
                else
                {
                    if (!target.IsIgnoring((IUser) inviter))
                        return GuildResult.SUCCESS;
                    guildResult = GuildResult.PLAYER_IGNORING_YOU;
                }
            }

            inviter.SendSystemMessage(string.Format("Unable to invite {1} cause : {0}.", (object) guildResult,
                (object) targetName));
            return guildResult;
        }

        /// <summary>
        /// Checks whether the given target exists in requester's guild and whether the given requestMember has needed privs
        /// Sends result of action to the requester
        /// </summary>
        /// <param name="reqChar">requester's character, can be null. If null, sending result is suppressed</param>
        /// <param name="targetChar">target's character, can be null</param>
        /// <param name="targetName">target character's name</param>
        /// <param name="commandId">executed command. Used for sending result</param>
        /// <param name="reqPrivs">priviliges required for executing this action</param>
        /// <param name="canAffectSelf">can this action be executed on self?</param>
        /// <returns>result of operation</returns>
        public static GuildResult CheckAction(Character reqChar, Character targetChar, string targetName,
            GuildCommandId commandId, GuildPrivileges reqPrivs, bool canAffectSelf)
        {
            GuildMember guildMember = reqChar.GuildMember;
            GuildResult resultCode;
            if (guildMember == null)
            {
                resultCode = GuildResult.PLAYER_NOT_IN_GUILD;
                targetName = string.Empty;
            }
            else if (targetChar == null)
                resultCode = GuildResult.PLAYER_NOT_FOUND;
            else if (!canAffectSelf && reqChar == targetChar)
                resultCode = GuildResult.PERMISSIONS;
            else if (reqChar.Guild != targetChar.Guild)
            {
                resultCode = GuildResult.PLAYER_NOT_IN_GUILD;
            }
            else
            {
                if (guildMember.HasRight(reqPrivs))
                    return GuildResult.SUCCESS;
                resultCode = GuildResult.PERMISSIONS;
                targetName = string.Empty;
            }

            GuildHandler.SendResult((IPacketReceiver) reqChar.Client, commandId, targetName, resultCode);
            return resultCode;
        }

        /// <summary>
        /// Checks whether the given target exists in requester's guild and whether the given requestMember has needed privs
        /// Sends result of action to the requester
        /// </summary>
        /// <param name="reqChar">requester's character, can be null. If null, sending result is suppressed</param>
        /// <param name="targetName">target character's name</param>
        /// <param name="targetGM">target's GuildMember entry is returned through this</param>
        /// <param name="commandId">executed command. Used for sending result</param>
        /// <param name="reqPrivs">priviliges required for executing this action</param>
        /// <param name="canAffectSelf">can this action be executed on self?</param>
        /// <returns>result of operation</returns>
        public static GuildResult CheckAction(Character reqChar, string targetName, out GuildMember targetGM,
            GuildCommandId commandId, GuildPrivileges reqPrivs, bool canAffectSelf)
        {
            targetGM = (GuildMember) null;
            GuildMember guildMember = reqChar.GuildMember;
            GuildResult resultCode;
            if (guildMember == null)
            {
                resultCode = GuildResult.PLAYER_NOT_IN_GUILD;
                targetName = string.Empty;
            }
            else if ((targetGM = guildMember.Guild[targetName]) == null)
                resultCode = GuildResult.PLAYER_NOT_FOUND;
            else if (!canAffectSelf && guildMember == targetGM)
            {
                resultCode = GuildResult.PERMISSIONS;
            }
            else
            {
                if (guildMember.HasRight(reqPrivs))
                    return GuildResult.SUCCESS;
                resultCode = GuildResult.PERMISSIONS;
                targetName = string.Empty;
            }

            GuildHandler.SendResult((IPacketReceiver) reqChar.Client, commandId, targetName, resultCode);
            return resultCode;
        }

        /// <summary>
        /// Checks if given character has necessary priviliges
        /// CheckInGuild call is done automatically
        /// </summary>
        /// <param name="character">character to check. May be null</param>
        /// <param name="commandId">executed command (used for sending result)</param>
        /// <param name="reqPrivs">required privileges</param>
        /// <returns>The Character's guild if the character has required privileges within the guild, otherwise null</returns>
        public static Guild CheckPrivs(Character character, GuildCommandId commandId, GuildPrivileges reqPrivs)
        {
            GuildMember guildMember = character.GuildMember;
            if (guildMember == null)
            {
                GuildHandler.SendResult((IPacketReceiver) character, commandId, GuildResult.PLAYER_NOT_IN_GUILD);
                return (Guild) null;
            }

            if (guildMember.HasRight(reqPrivs))
                return guildMember.Guild;
            Character character1 = guildMember.Character;
            if (character1 != null)
                GuildHandler.SendResult((IPacketReceiver) character1, commandId, GuildResult.PERMISSIONS);
            return (Guild) null;
        }

        /// <summary>
        /// Checks whether a guild member may make another guild member the guild leader
        /// </summary>
        /// <param name="reqChar">requester's character, can be null. If null, sending result is suppressed</param>
        /// <param name="targetChar">target's character, can be null</param>
        /// <param name="targetName">target character's name</param>
        /// <returns>result of operation</returns>
        public static GuildResult CheckIsLeader(Character reqChar, Character targetChar, GuildCommandId cmd,
            string targetName)
        {
            GuildMember guildMember1 = reqChar.GuildMember;
            GuildResult resultCode;
            if (guildMember1 == null)
                resultCode = GuildResult.PLAYER_NOT_IN_GUILD;
            else if (targetChar == null)
            {
                resultCode = GuildResult.PLAYER_NOT_FOUND;
            }
            else
            {
                GuildMember guildMember2;
                if ((guildMember2 = targetChar.GuildMember) == null || guildMember2.Guild != guildMember1.Guild)
                    resultCode = GuildResult.PLAYER_NOT_IN_GUILD;
                else if (guildMember1 == guildMember2)
                {
                    resultCode = GuildResult.PERMISSIONS;
                    targetName = string.Empty;
                }
                else
                {
                    if (guildMember1.IsLeader)
                        return GuildResult.SUCCESS;
                    resultCode = GuildResult.PERMISSIONS;
                    targetName = string.Empty;
                }
            }

            GuildHandler.SendResult((IPacketReceiver) reqChar, cmd, targetName, resultCode);
            return resultCode;
        }

        public IEnumerator<GuildMember> GetEnumerator()
        {
            return this.GetMembers().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetMembers().GetEnumerator();
        }

        /// <summary>Send a packet to every guild member</summary>
        /// <param name="packet">the packet to send</param>
        public void Broadcast(RealmPacketOut packet)
        {
            this.Broadcast(packet, (Character) null);
        }

        /// <summary>
        /// Send a packet to every guild member except for the one specified.
        /// </summary>
        /// <param name="packet">the packet to send</param>
        /// <param name="ignoredCharacter">the <see cref="T:WCell.RealmServer.Entities.Character" /> that won't receive the packet</param>
        public void Broadcast(RealmPacketOut packet, Character ignoredCharacter)
        {
            foreach (GuildMember member in this.GetMembers())
            {
                Character character = member.Character;
                if (character != null && character != ignoredCharacter)
                    character.Client.Send(packet, false);
            }
        }

        public void ForeachMember(Action<GuildMember> callback)
        {
            lock (this)
            {
                foreach (GuildMember guildMember in (IEnumerable<GuildMember>) this.Members.Values)
                    callback(guildMember);
            }
        }

        /// <summary>The EntityId (only set for Character)</summary>
        public EntityId EntityId
        {
            get { return EntityId.Zero; }
        }

        public byte OnlineMembersCount { get; set; }

        public List<HistoryRecord> History
        {
            get { return this._history ?? (this._history = new List<HistoryRecord>(12)); }
        }

        public GuildSkill[] Skills
        {
            get { return this._skills ?? (this._skills = new GuildSkill[10]); }
        }

        public void AddHistoryMessage(Asda2GuildHistoryType type, int value, string trigerName, string time)
        {
            if (this.History.Count == 12)
                this.History.RemoveAt(11);
            this.History.Insert(0, new HistoryRecord((byte) type, value, trigerName, time));
        }

        public void SendSystemMsg(string msg)
        {
            foreach (Character character in this.GetCharacters())
                character.SendSystemMessage(msg);
        }

        public void SendMessage(string message)
        {
            LogManager.GetCurrentClassLogger()
                .Warn("Tried to send message to guild {0} but Guild.SendMessage(string) is not implemented yet: {1}",
                    (object) this.Name, (object) message);
        }

        /// <summary>Say something to this target</summary>
        public void SendMessage(IChatter sender, string message)
        {
            ChatMgr.SendGuildMessage(sender, this, message);
        }

        /// <summary>
        /// All members.
        /// Looping over this enumeration is synchronized.
        /// </summary>
        public IEnumerable<GuildMember> GetMembers()
        {
            lock (this)
            {
                foreach (GuildMember guildMember in (IEnumerable<GuildMember>) this.Members.Values)
                    yield return guildMember;
            }
        }

        /// <summary>
        /// All online characters.
        /// Looping over this enumeration is synchronized.
        /// </summary>
        public IEnumerable<Character> GetCharacters()
        {
            foreach (GuildMember member in this.GetMembers())
            {
                Character chr = member.Character;
                if (chr != null)
                    yield return chr;
            }
        }

        /// <summary>Sends the given packet to all online characters</summary>
        public void Send(RealmPacketOut packet, bool addEnd = false, Locale locale = Locale.Any)
        {
            foreach (Character character in this.GetCharacters())
            {
                if (locale == Locale.Any || locale == character.Client.Locale)
                    character.Client.Send(packet, addEnd);
            }
        }

        /// <summary>All online chat listeners</summary>
        public IEnumerable<Character> GetCharacters(GuildPrivileges requiredPrivs)
        {
            foreach (GuildMember member in this.GetMembers())
            {
                Character chr = member.Character;
                if (chr != null && member.HasRight(requiredPrivs))
                    yield return chr;
            }
        }

        /// <summary>Sends the given packet to all online chat listeners</summary>
        public void SendToChatListeners(RealmPacketOut packet)
        {
        }

        /// <summary>Sends the given packet to all online officers</summary>
        public void SendToOfficers(RealmPacketOut packet)
        {
        }

        public override string ToString()
        {
            return this.Name + string.Format(" (Id: {0}, Members:{1}) - {2}", (object) this.Id,
                       (object) this.MemberCount, (object) this.Info);
        }

        public LearnGuildSkillResult TryLearnSkill(GuildSkillId skillId, out GuildSkill skill)
        {
            skill = this.Skills[(int) skillId];
            if (skill != null)
            {
                if (skill.IsMaxLevel)
                    return LearnGuildSkillResult.ThisIsTheMaxLevelOfSkill;
                if (!this.SubstractGuildPoints(skill.NextLearnCost))
                    return LearnGuildSkillResult.IncifitientPoints;
                ++this.Skills[(int) skillId].Level;
                Asda2GuildHandler.SendGuildSkillStatusChangedResponse(skill, ClanSkillStatus.Learned);
            }
            else
            {
                if (!this.SubstractGuildPoints(GuildSkillTemplate.Templates[(int) skillId].LearnCosts[1]))
                    return LearnGuildSkillResult.IncifitientPoints;
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                {
                    GuildSkill guildSkill = new GuildSkill(this, skillId);
                    this.Skills[(int) skillId] = guildSkill;
                    guildSkill.CreateLater();
                    Asda2GuildHandler.SendGuildSkillStatusChangedResponse(guildSkill, ClanSkillStatus.Learned);
                }));
            }

            return LearnGuildSkillResult.Ok;
        }

        public bool SubstractGuildPoints(int points)
        {
            if ((long) this.Points < (long) points)
                return false;
            this.Points -= (uint) points;
            this.AddHistoryMessage(Asda2GuildHistoryType.UsedPoints, points, points.ToString(),
                DateTime.Now.ToLongTimeString());
            return true;
        }

        public bool AddGuildPoints(int points)
        {
            this.Points += (uint) points;
            Asda2GuildHandler.SendUpdateGuildInfoResponse(this, GuildInfoMode.Silent, (Character) null);
            return true;
        }

        public Asda2GuildHandler.CreateImpeachmentResult CreateImpeachment(GuildMember guildMember)
        {
            if (DateTime.Now - this._impeachmentStartTime < TimeSpan.FromMinutes(4.0))
                return Asda2GuildHandler.CreateImpeachmentResult.AlreadyInProgress;
            this._impeachmentStartTime = DateTime.Now;
            this._acceptedMembers.Clear();
            this._newLeader = guildMember;
            Asda2GuildHandler.SendImpeachmentAnswerResponse(this, this._newLeader.Name);
            guildMember.Character.Map.CallDelayed(180000, new Action(this.ImpeachmentCallback));
            return Asda2GuildHandler.CreateImpeachmentResult.Success;
        }

        public void AddImpeachmentVote(GuildMember member)
        {
            if (this._acceptedMembers.ContainsKey(member) || member == this._newLeader || member.IsLeader)
                return;
            this._acceptedMembers.Add(member, member);
        }

        private void ImpeachmentCallback()
        {
            float num = (float) ((double) this._acceptedMembers.Count / (double) (this.MemberCount - 2) * 100.0);
            this.SendSystemMsg(string.Format("{0}% members accepted new leader.", (object) num));
            if ((double) num > 70.0)
            {
                this.Leader.Asda2RankId = (byte) 3;
                Asda2GuildHandler.SendGuildNotificationResponse(this, GuildNotificationType.RankChanged, this.Leader);
                this.Leader = this._newLeader;
                this.Leader.Asda2RankId = (byte) 4;
                Asda2GuildHandler.SendGuildNotificationResponse(this, GuildNotificationType.ApointedAsNewGuildLeader,
                    this.Leader);
                Asda2GuildHandler.SendImpeachmentResultResponse(this, Asda2GuildHandler.ImpeachmentResult.Success);
                Asda2GuildHandler.SendUpdateGuildInfoResponse(this, GuildInfoMode.Silent, (Character) null);
                this.AddHistoryMessage(Asda2GuildHistoryType.ApointedAsGuildLeaderThorowVote, 0, this.Leader.Name,
                    DateTime.Now.ToLongTimeString());
            }
            else
                Asda2GuildHandler.SendImpeachmentResultResponse(this, Asda2GuildHandler.ImpeachmentResult.Failed);

            this._acceptedMembers.Clear();
            this._newLeader = (GuildMember) null;
            this._impeachmentStartTime = DateTime.MinValue;
        }

        public bool LevelUp()
        {
            if (!this.SubstractGuildPoints(CharacterFormulas.GuildLevelUpCost[(int) this.Level]))
                return false;
            ++this.Level;
            this.AddHistoryMessage(Asda2GuildHistoryType.GuildLevelNowIs, (int) this.Level, "system",
                DateTime.Now.ToLongTimeString());
            Asda2GuildHandler.SendUpdateGuildInfoResponse(this, GuildInfoMode.GuildLevelChanged, (Character) null);
            foreach (Character character in this.GetCharacters())
                GlobalHandler.SendCharactrerInfoClanNameToAllNearbyCharacters(character);
            return true;
        }
    }
}