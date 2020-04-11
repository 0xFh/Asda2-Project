using Castle.ActiveRecord;
using Cell.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.ArenaTeams;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.Battlegrounds.Arenas
{
    [Castle.ActiveRecord.ActiveRecord("ArenaTeam", Access = PropertyAccess.Property)]
    public class ArenaTeam : WCellRecord<ArenaTeam>, INamed, IEnumerable<ArenaTeamMember>, IEnumerable, IChatTarget,
        IGenericChatTarget
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(ArenaTeam), nameof(_id), 1L);

        public readonly ImmutableDictionary<uint, ArenaTeamMember> Members =
            new ImmutableDictionary<uint, ArenaTeamMember>();

        [Field("Name", NotNull = true, Unique = true)]
        private string _name;

        [Field("LeaderLowId", NotNull = true)] private int _leaderLowId;
        [Field("Type", NotNull = true)] private int _type;
        private SpinWaitLock m_syncRoot;
        private ArenaTeamMember m_leader;
        private ArenaTeamStats m_stats;
        private ArenaTeamSlot m_slot;

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        private long _id { get; set; }

        public uint LeaderLowId
        {
            get { return (uint) this._leaderLowId; }
        }

        /// <summary>
        /// The SyncRoot against which to synchronize this arena team(when iterating over it or making certain changes)
        /// </summary>
        public SpinWaitLock SyncRoot
        {
            get { return this.m_syncRoot; }
        }

        /// <summary>Id of this team</summary>
        public uint Id
        {
            get { return (uint) this._id; }
        }

        /// <summary>Arena team's name</summary>
        /// <remarks>length is limited with MAX_ARENATEAM_LENGTH</remarks>
        public string Name
        {
            get { return this._name; }
        }

        /// <summary>Type of this arena team</summary>
        public uint Type
        {
            get { return (uint) this._type; }
        }

        public ArenaTeamSlot Slot
        {
            get { return this.m_slot; }
            set { this.m_slot = value; }
        }

        /// <summary>
        /// Arena team leader's ArenaTeamMember
        /// Setting it does not send event to the team. Use ArenaTeam.SendEvent
        /// </summary>
        public ArenaTeamMember Leader
        {
            get { return this.m_leader; }
            set
            {
                if (value == null || value.ArenaTeam != this)
                    return;
                this.m_leader = value;
                this._leaderLowId = (int) value.Id;
            }
        }

        /// <summary>Stats of the arena team</summary>
        public ArenaTeamStats Stats
        {
            set { this.m_stats = value; }
            get { return this.m_stats; }
        }

        /// <summary>Number of arena team's members</summary>
        public int MemberCount
        {
            get { return this.Members.Count; }
        }

        public ArenaTeam()
        {
        }

        /// <summary>
        /// Creates a new ArenaTeamRecord row in the database with the given information.
        /// </summary>
        /// <param name="leader">leader's character record</param>
        /// <param name="name">the name of the new character</param>
        /// <returns>the <seealso cref="T:WCell.RealmServer.Battlegrounds.Arenas.ArenaTeam" /> object</returns>
        public ArenaTeam(CharacterRecord leader, string name, uint type)
            : this()
        {
            this._id = ArenaTeam._idGenerator.Next();
            this._leaderLowId = (int) leader.EntityLowId;
            this._name = name;
            this._type = (int) type;
            this.m_slot = ArenaMgr.GetSlotByType(type);
            this.m_leader = new ArenaTeamMember(leader, this, true);
            this.m_stats = new ArenaTeamStats(this);
            this.Members.Add(this.m_leader.Id, this.m_leader);
            this.m_leader.Create();
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage(new Action(((ActiveRecordBase) this).Create));
            this.Register();
        }

        internal void InitAfterLoad()
        {
            foreach (ArenaTeamMember arenaTeamMember in ArenaTeamMember.FindAll(this))
            {
                arenaTeamMember.Init(this);
                this.Members.Add(arenaTeamMember.Id, arenaTeamMember);
            }

            this.m_stats = ActiveRecordBase<ArenaTeamStats>.FindByPrimaryKey((object) this.Id);
            this.m_slot = ArenaMgr.GetSlotByType(this.Type);
            this.m_leader = this[this.LeaderLowId];
            if (this.m_leader == null)
                this.OnLeaderDeleted();
            if (this.m_leader == null)
                return;
            this.Register();
        }

        /// <summary>
        /// Initializes arena team after its creation or restoration from DB
        /// </summary>
        internal void Register()
        {
            ArenaMgr.RegisterArenaTeam(this);
        }

        public ArenaTeamMember AddMember(Character chr)
        {
            ArenaTeamMember arenaTeamMember = this.AddMember(chr.Record);
            if (arenaTeamMember != null)
                arenaTeamMember.Character = chr;
            return arenaTeamMember;
        }

        /// <summary>
        /// Adds a new arena team member
        /// Calls ArenaTeamMgr.OnJoinTeam
        /// </summary>
        /// <param name="chr">character to add</param>
        /// <returns>ArenaTeamMember of new member</returns>
        public ArenaTeamMember AddMember(CharacterRecord chr)
        {
            if ((long) this.Members.Count >= (long) (this.Type * 2U))
                return (ArenaTeamMember) null;
            this.SyncRoot.Enter();
            ArenaTeamMember atm;
            try
            {
                if (this.Members.TryGetValue(chr.EntityLowId, out atm))
                    return atm;
                atm = new ArenaTeamMember(chr, this, false);
                atm.Character.SetArenaTeamInfoField(this.Slot, ArenaTeamInfoType.ARENA_TEAM_ID, this.Id);
                atm.Character.SetArenaTeamInfoField(this.Slot, ArenaTeamInfoType.ARENA_TEAM_MEMBER, 1U);
                this.Members.Add(atm.Id, atm);
                atm.Create();
                this.Update();
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex,
                    string.Format("Could not add member {0} to arena team {1}", (object) chr, (object) this),
                    new object[0]);
                return (ArenaTeamMember) null;
            }
            finally
            {
                this.SyncRoot.Exit();
            }

            ArenaMgr.RegisterArenaTeamMember(atm);
            return atm;
        }

        public bool RemoveMember(uint memberId)
        {
            ArenaTeamMember member = this[memberId];
            if (member != null)
                return this.RemoveMember(member, true);
            return false;
        }

        public bool RemoveMember(string name)
        {
            ArenaTeamMember member = this[name];
            if (member != null)
                return this.RemoveMember(member, true);
            return false;
        }

        /// <summary>Removes ArenaTeamMember from the arena team</summary>
        /// <param name="member">member to remove</param>
        /// <param name="update">if true, sends event to the team</param>
        public bool RemoveMember(ArenaTeamMember member)
        {
            return this.RemoveMember(member, true);
        }

        /// <summary>Removes ArenaTeamMember from the arena team</summary>
        /// <param name="member">member to remove</param>
        /// <param name="update">if false, changes to the team will not be promoted anymore (used when the team is being disbanded)</param>
        public bool RemoveMember(ArenaTeamMember member, bool update)
        {
            this.OnRemoveMember(member);
            if (update && member == this.m_leader)
                this.OnLeaderDeleted();
            if (this.m_leader == null)
                return true;
            this.m_syncRoot.Enter();
            try
            {
                if (!this.Members.Remove(member.Id))
                    return false;
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex,
                    string.Format("Could not delete member {0} from arena team {1}", (object) member.Name,
                        (object) this), new object[0]);
                return false;
            }
            finally
            {
                this.m_syncRoot.Exit();
            }

            int num = update ? 1 : 0;
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
        protected void OnRemoveMember(ArenaTeamMember member)
        {
            ArenaMgr.UnregisterArenaTeamMember(member);
            Character character = member.Character;
            if (character == null)
                return;
            character.ArenaTeamMember[(int) this.Slot] = (ArenaTeamMember) null;
            character.SetArenaTeamInfoField(this.Slot, ArenaTeamInfoType.ARENA_TEAM_ID, 0U);
            character.SetArenaTeamInfoField(this.Slot, ArenaTeamInfoType.ARENA_TEAM_TYPE, 0U);
            character.SetArenaTeamInfoField(this.Slot, ArenaTeamInfoType.ARENA_TEAM_MEMBER, 0U);
            character.SetArenaTeamInfoField(this.Slot, ArenaTeamInfoType.ARENA_TEAM_GAMES_WEEK, 0U);
            character.SetArenaTeamInfoField(this.Slot, ArenaTeamInfoType.ARENA_TEAM_GAMES_SEASON, 0U);
            character.SetArenaTeamInfoField(this.Slot, ArenaTeamInfoType.ARENA_TEAM_WINS_SEASON, 0U);
            character.SetArenaTeamInfoField(this.Slot, ArenaTeamInfoType.ARENA_TEAM_PERSONAL_RATING, 0U);
        }

        private void OnLeaderDeleted()
        {
            ArenaTeamMember newLeader = (ArenaTeamMember) null;
            foreach (ArenaTeamMember arenaTeamMember in (IEnumerable<ArenaTeamMember>) this.Members.Values)
                newLeader = arenaTeamMember;
            if (newLeader == null)
                this.Disband();
            else
                this.ChangeLeader(newLeader);
        }

        public IEnumerator<ArenaTeamMember> GetEnumerator()
        {
            foreach (ArenaTeamMember arenaTeamMember in (IEnumerable<ArenaTeamMember>) this.Members.Values)
                yield return arenaTeamMember;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (ArenaTeamMember arenaTeamMember in (IEnumerable<ArenaTeamMember>) this.Members.Values)
                yield return (object) arenaTeamMember;
        }

        public void SendSystemMsg(string msg)
        {
            foreach (ArenaTeamMember arenaTeamMember in (IEnumerable<ArenaTeamMember>) this.Members.Values)
            {
                if (arenaTeamMember.Character != null)
                    arenaTeamMember.Character.SendSystemMessage(msg);
            }
        }

        public void SendMessage(string message)
        {
            throw new NotImplementedException();
        }

        /// <summary>Say something to this target</summary>
        public void SendMessage(IChatter sender, string message)
        {
        }

        /// <summary>Requests member by his low id</summary>
        /// <param name="lowMemberId">low id of member's character</param>
        /// <returns>requested member or null</returns>
        public ArenaTeamMember this[uint lowMemberId]
        {
            get
            {
                foreach (ArenaTeamMember arenaTeamMember in (IEnumerable<ArenaTeamMember>) this.Members.Values)
                {
                    if ((int) arenaTeamMember.Id == (int) lowMemberId)
                        return arenaTeamMember;
                }

                return (ArenaTeamMember) null;
            }
        }

        /// <summary>Requests member by his name</summary>
        /// <param name="name">name of member's character (not case-sensitive)</param>
        /// <returns>requested member</returns>
        public ArenaTeamMember this[string name]
        {
            get
            {
                name = name.ToLower();
                foreach (ArenaTeamMember arenaTeamMember in (IEnumerable<ArenaTeamMember>) this.Members.Values)
                {
                    if (arenaTeamMember.Name.ToLower() == name)
                        return arenaTeamMember;
                }

                return (ArenaTeamMember) null;
            }
        }

        /// <summary>Disbands the arena team</summary>
        /// <param name="update">if true, sends event to the team</param>
        public void Disband()
        {
            this.m_syncRoot.Enter();
            try
            {
                foreach (ArenaTeamMember member in this.Members.Values.ToArray<ArenaTeamMember>())
                    this.RemoveMember(member, false);
                ArenaMgr.UnregisterArenaTeam(this);
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() => this.Delete()));
            }
            finally
            {
                this.m_syncRoot.Exit();
            }
        }

        /// <summary>Changes leader of the arena team</summary>
        /// <param name="newLeader">ArenaTeamMember of new leader</param>
        /// <param name="update">if true, sends event to the team</param>
        public void ChangeLeader(ArenaTeamMember newLeader)
        {
            if (newLeader.ArenaTeam != this)
                return;
            ArenaTeamMember currentLeader = this.Leader;
            currentLeader.Character.SetArenaTeamInfoField(this.Slot, ArenaTeamInfoType.ARENA_TEAM_MEMBER, 1U);
            this.Leader = newLeader;
            newLeader.Character.SetArenaTeamInfoField(this.Slot, ArenaTeamInfoType.ARENA_TEAM_MEMBER, 0U);
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((IMessage) new Message((Action) (() =>
            {
                if (currentLeader != null)
                    currentLeader.Update();
                newLeader.Update();
                this.Update();
            })));
            ArenaTeamMember arenaTeamMember = currentLeader;
        }
    }
}