using System;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Spells;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;
using WCell.Util.Variables;

namespace WCell.RealmServer.Battlegrounds
{
    public abstract class Battleground : InstancedMap, IBattlegroundRange
    {
        /// <summary>
        /// Whether to add to a Team even if it already has more players
        /// </summary>
        [Variable("BGAddPlayersToBiggerTeam")] public static bool AddPlayersToBiggerTeamDefault = true;

        /// <summary>
        /// Whether to buff <see cref="T:WCell.RealmServer.Entities.Character" />s of smaller Teams if they have less players
        /// </summary>
        [Variable("BGBuffSmallerTeam")] public static bool BuffSmallerBGTeam = true;

        /// <summary>
        /// Default delay until shutdown when player-count drops
        /// below minimum in seconds
        /// </summary>
        [Variable("BGDefaultShutdownDelay")] public static int DefaultShutdownDelayMillis = 200000;

        /// <summary>Start the BG once this many % of players joined</summary>
        [Variable("BGStartPlayerPct")] public static uint StartPlayerPct = 80;

        [Variable("BGUpdateQueueSeconds")] public static int UpdateQueueMillis = 10000;
        protected readonly BattlegroundTeam[] _teams;

        /// <summary>
        /// All currently pending requests of Characters who want to join this particular Battleground.
        /// </summary>
        protected InstanceBattlegroundQueue _instanceQueue;

        protected GlobalBattlegroundQueue _parentQueue;
        private Spell _preparationSpell;
        private Spell _healingReductionSpell;
        protected TimerEntry _queueTimer;
        protected TimerEntry _shutdownTimer;
        protected DateTime _startTime;
        protected BattlegroundStatus _status;
        protected BattlegroundTeam _winner;
        protected BattlegroundTemplate _template;
        protected int _minLevel;
        protected int _maxLevel;

        public Battleground(int minLevel, int maxlevel, BattlegroundTemplate template)
        {
            this._minLevel = minLevel;
            this._maxLevel = maxlevel;
            this._template = template;
        }

        public Battleground()
        {
            if (this.HasQueue)
            {
                this._queueTimer = new TimerEntry((Action<int>) (dt => this.ProcessPendingPlayers()));
                this.RegisterUpdatable((IUpdatable) this._queueTimer);
            }

            this._status = BattlegroundStatus.None;
            this.AddPlayersToBiggerTeam = Battleground.AddPlayersToBiggerTeamDefault;
            this._teams = new BattlegroundTeam[2];
            this._shutdownTimer = new TimerEntry((Action<int>) (dt => this.Delete()));
            this.RegisterUpdatable((IUpdatable) this._shutdownTimer);
        }

        /// <summary>
        /// The current <see cref="T:WCell.Constants.BattlegroundStatus" />
        /// </summary>
        public BattlegroundStatus Status
        {
            get { return this._status; }
            protected set { this._status = value; }
        }

        /// <summary>
        /// The <see cref="T:WCell.RealmServer.Battlegrounds.BattlegroundQueue" /> that manages requests for this particular Instance
        /// </summary>
        public InstanceBattlegroundQueue InstanceQueue
        {
            get { return this._instanceQueue; }
        }

        /// <summary>
        /// The <see cref="T:WCell.RealmServer.Battlegrounds.BattlegroundQueue" /> that manages general requests (not for a particular Instance)
        /// </summary>
        public GlobalBattlegroundQueue ParentQueue
        {
            get { return this._parentQueue; }
            protected internal set
            {
                this._parentQueue = value;
                this._minLevel = this._parentQueue.MinLevel;
                this._maxLevel = this._parentQueue.MaxLevel;
                this._template = this._parentQueue.Template;
            }
        }

        /// <summary>The team that won (or null if still in progress)</summary>
        public BattlegroundTeam Winner
        {
            get { return this._winner; }
            protected set
            {
                this._winner = value;
                value.ForeachCharacter((Action<Character>) (chr =>
                    chr.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.CompleteBattleground,
                        (uint) this.MapId, 1U, (Unit) null)));
            }
        }

        public virtual SpellId PreparationSpellId
        {
            get { return SpellId.Preparation; }
        }

        public virtual SpellId HealingReductionSpellId
        {
            get { return SpellId.BattlegroundDampening; }
        }

        /// <summary>Get/sets whether this Battleground will shutdown soon</summary>
        public bool IsShuttingDown
        {
            get { return this._shutdownTimer.IsRunning; }
            set
            {
                if (value)
                    this.RemainingShutdownDelay = Battleground.DefaultShutdownDelayMillis;
                else
                    this.RemainingShutdownDelay = -1;
            }
        }

        /// <summary>
        /// The time when this BG started. default(DateTime) before it actually started.
        /// </summary>
        public DateTime StartTime
        {
            get { return this._startTime; }
        }

        /// <summary>
        /// Returns the time since the Battleground started in millis or 0 while still preparing.
        /// </summary>
        public int RuntimeMillis
        {
            get
            {
                if (this._status == BattlegroundStatus.Active || this._status == BattlegroundStatus.Finished)
                    return (int) (DateTime.Now - this.StartTime).TotalMilliseconds;
                return 0;
            }
        }

        /// <summary>Preparation time of this BG</summary>
        public virtual int PreparationTimeMillis
        {
            get { return 120000; }
        }

        /// <summary>
        /// Time until shutdown.
        /// Non-positive value cancels shutdown.
        /// </summary>
        public int RemainingShutdownDelay
        {
            get { return this._shutdownTimer.RemainingInitialDelayMillis; }
            set
            {
                this.EnsureContext();
                if (value > 0 == this._shutdownTimer.IsRunning)
                    return;
                if (value <= 0)
                    this._shutdownTimer.Stop();
                else
                    this.StartShutdown(value);
            }
        }

        /// <summary>
        /// Whether or not this battleground is currently active. (in progress)
        /// </summary>
        public new bool IsActive
        {
            get { return this._status == BattlegroundStatus.Active; }
        }

        /// <summary>Whether to allow people to join this Battleground</summary>
        public virtual bool IsOpen
        {
            get { return this._status != BattlegroundStatus.Finished; }
        }

        /// <summary>Whether Characters can still join</summary>
        public virtual bool IsAddingPlayers
        {
            get { return this.IsActive; }
        }

        /// <summary>
        /// Whether to start the mode "Call To Arms" and change timers
        /// </summary>
        public virtual bool IsHolidayBG
        {
            get { return WorldEventMgr.IsHolidayActive(BattlegroundMgr.GetHolidayIdByBGId(this.Template.Id)); }
        }

        /// <summary>
        /// Whether to start the Shutdown timer when <see cref="P:WCell.RealmServer.Global.Map.PlayerCount" /> drops below the minimum
        /// </summary>
        public bool CanShutdown { get; set; }

        /// <summary>
        /// Whether to add Players from the Queue even if the Team
        /// already has more Players
        /// </summary>
        public bool AddPlayersToBiggerTeam { get; set; }

        public override int MinLevel
        {
            get { return this._minLevel; }
        }

        public override int MaxLevel
        {
            get { return this._maxLevel; }
        }

        /// <summary>
        /// 
        /// </summary>
        public BattlegroundTemplate Template
        {
            get { return this._template; }
        }

        /// <summary>Starts the shutdown timer with the given delay</summary>
        protected virtual void StartShutdown(int millis)
        {
            this._shutdownTimer.Start(millis);
            foreach (Unit character in this.m_characters)
                character.Auras.Remove(this._preparationSpell);
        }

        public override bool CanEnter(Character chr)
        {
            if (this.IsOpen)
                return base.CanEnter(chr);
            return false;
        }

        public virtual void StartPreparation()
        {
            this.ExecuteInContext((Action) (() =>
            {
                if (!this.IsOpen)
                    return;
                this._status = BattlegroundStatus.Preparing;
                if (this._preparationSpell != null)
                {
                    foreach (WorldObject character in this.m_characters)
                        character.SpellCast.TriggerSelf(this._preparationSpell);
                }

                this.CallDelayed(this.PreparationTimeMillis / 2, new Action(this.OnPrepareHalftime));
                this.OnPrepareBegin();
            }));
        }

        public virtual void StartFight()
        {
            this.ExecuteInContext((Action) (() =>
            {
                if (this.IsDisposed || this._status == BattlegroundStatus.Active)
                    return;
                this._startTime = DateTime.Now;
                this._status = BattlegroundStatus.Active;
                foreach (Unit character in this.m_characters)
                    character.Auras.Remove(this._preparationSpell);
                this.OnStart();
            }));
        }

        /// <summary>Enter the final state</summary>
        public virtual void FinishFight()
        {
            this._status = BattlegroundStatus.Finished;
            this.RewardPlayers();
            this.SendPvpData();
            this.ExecuteInContext((Action) (() => this.FinalizeBattleground(false)));
        }

        /// <summary>
        /// Toggle <see cref="P:WCell.RealmServer.Battlegrounds.Battleground.IsShuttingDown" /> or <see cref="P:WCell.RealmServer.Battlegrounds.Battleground.CanShutdown" /> if required
        /// </summary>
        protected void CheckShutdown()
        {
            if (this.PlayerCount < this.MaxPlayerCount && this.CanShutdown)
            {
                if (this.IsShuttingDown)
                    return;
                this.IsShuttingDown = true;
            }
            else
            {
                if (!this.IsShuttingDown)
                    return;
                this.IsShuttingDown = false;
            }
        }

        /// <summary>
        /// Returns the <see cref="T:WCell.RealmServer.Battlegrounds.BattlegroundTeam" /> of the given side
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public BattlegroundTeam GetTeam(BattlegroundSide side)
        {
            return this._teams[(int) side];
        }

        public BattlegroundTeam AllianceTeam
        {
            get { return this.GetTeam(BattlegroundSide.Alliance); }
        }

        public BattlegroundTeam HordeTeam
        {
            get { return this.GetTeam(BattlegroundSide.Horde); }
        }

        /// <summary>
        /// Adds the given Character (or optionally his/her Group) to the Queue of this Battleground if possible.
        /// Make sure that HasQueue is true before calling this method.
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="asGroup"></param>
        public void TryJoin(Character chr, bool asGroup, BattlegroundSide side)
        {
            this.ExecuteInContext((Action) (() => this.DoTryJoin(chr, asGroup, side)));
        }

        protected void DoTryJoin(Character chr, bool asGroup, BattlegroundSide side)
        {
            this.EnsureContext();
            if (!chr.IsInWorld || !this.IsOpen)
                return;
            BattlegroundTeam team = this.GetTeam(side);
            ICharacterSet characterSet = this._instanceQueue.GetTeamQueue(side).GetCharacterSet(chr, asGroup);
            if (characterSet == null)
                return;
            team.Enqueue(characterSet);
        }

        /// <summary>Add as many players possible to both Teams</summary>
        public void ProcessPendingPlayers()
        {
            this.ProcessPendingPlayers(this._teams[0]);
            this.ProcessPendingPlayers(this._teams[1]);
        }

        /// <summary>
        /// Removes up to the max possible amount of players from the Queues
        /// and adds them to the Battleground.
        /// </summary>
        public int ProcessPendingPlayers(BattlegroundTeam team)
        {
            if (!this.IsAddingPlayers)
                return 0;
            int openPlayerSlotCount = team.OpenPlayerSlotCount;
            if (openPlayerSlotCount > 0)
                return this.ProcessPendingPlayers(team.Side, openPlayerSlotCount);
            return 0;
        }

        /// <summary>
        /// Removes up to the given amount of players from the Queues
        /// and adds them to the Battleground.
        /// </summary>
        /// <param name="amount"></param>
        /// <remarks>Map-Context required. Cannot be used once the Battleground is over.</remarks>
        /// <returns>The amount of remaining players</returns>
        public int ProcessPendingPlayers(BattlegroundSide side, int amount)
        {
            this.EnsureContext();
            if (this._instanceQueue == null)
                return 0;
            amount -= this._instanceQueue.GetTeamQueue(side).DequeueCharacters(amount, this);
            if (amount > 0)
                amount -= this._parentQueue.GetTeamQueue(side).DequeueCharacters(amount, this);
            return amount;
        }

        /// <summary>
        /// Starts to clean things up once the BG is over.
        /// Might be called right before the BG is disposed.
        /// </summary>
        protected void FinalizeBattleground(bool disposing)
        {
            this.EnsureContext();
            if (this._shutdownTimer == null)
                return;
            this._shutdownTimer = (TimerEntry) null;
            this.OnFinish(disposing);
            if (this._instanceQueue != null)
            {
                this._instanceQueue.Dispose();
                this._instanceQueue = (InstanceBattlegroundQueue) null;
            }

            if (this._parentQueue == null)
                return;
            this._parentQueue.OnRemove(this);
        }

        public override void DeleteNow()
        {
            BattlegroundMgr.Instances.RemoveInstance(this.Template.Id, this.InstanceId);
            this.FinalizeBattleground(true);
            base.DeleteNow();
        }

        public void SendPvpData()
        {
            foreach (Character character in this.m_characters)
                BattlegroundHandler.SendPvpData((IPacketReceiver) character, character.Battlegrounds.Team.Side, this);
        }

        protected override void Dispose()
        {
            base.Dispose();
            foreach (BattlegroundTeam team in this._teams)
                team.Dispose();
            this._parentQueue = (GlobalBattlegroundQueue) null;
        }

        protected virtual void OnPrepareBegin()
        {
        }

        protected virtual void OnPrepareHalftime()
        {
            this.CallDelayed(this.PreparationTimeMillis / 2, new Action(this.StartFight));
        }

        protected virtual void OnStart()
        {
            MiscHandler.SendPlaySoundToMap((Map) this, 3439U);
        }

        protected virtual void OnFinish(bool disposing)
        {
            MiscHandler.SendPlaySoundToMap((Map) this, this.Winner.Side == BattlegroundSide.Horde ? 8454U : 8455U);
        }

        public virtual void OnPlayerClickedOnflag(GameObject go, Character chr)
        {
        }

        public virtual bool HasQueue
        {
            get { return true; }
        }

        protected internal override void InitMap()
        {
            base.InitMap();
            BattlegroundMgr.Instances.AddInstance(this.Template.Id, this);
            this._preparationSpell = SpellHandler.Get(this.PreparationSpellId);
            this._healingReductionSpell = SpellHandler.Get(this.HealingReductionSpellId);
            if (this.HasQueue)
            {
                this._instanceQueue = new InstanceBattlegroundQueue(this);
                this._queueTimer.Start(0, Battleground.UpdateQueueMillis);
            }

            this._teams[0] = this.CreateAllianceTeam();
            this._teams[1] = this.CreateHordeTeam();
            this.ProcessPendingPlayers();
        }

        /// <summary>
        /// Creates a new BattlegroundTeam during initialization of the BG
        /// </summary>
        protected virtual BattlegroundTeam CreateHordeTeam()
        {
            return new BattlegroundTeam(
                this._instanceQueue != null
                    ? this._instanceQueue.GetTeamQueue(BattlegroundSide.Horde)
                    : (BattlegroundTeamQueue) null, BattlegroundSide.Horde, this);
        }

        /// <summary>
        /// Creates a new BattlegroundTeam during initialization of the BG
        /// </summary>
        protected virtual BattlegroundTeam CreateAllianceTeam()
        {
            return new BattlegroundTeam(
                this._instanceQueue != null
                    ? this._instanceQueue.GetTeamQueue(BattlegroundSide.Alliance)
                    : (BattlegroundTeamQueue) null, BattlegroundSide.Alliance, this);
        }

        protected virtual BattlegroundStats CreateStats()
        {
            return new BattlegroundStats();
        }

        /// <summary>
        /// Override this to give the players mark of honors and whatnot.
        /// Note: Usually should trigger  a spell on the characters
        /// e.g. SpellId.CreateWarsongMarkOfHonorWinner
        /// </summary>
        protected virtual void RewardPlayers()
        {
        }

        public override void TeleportInside(Character chr)
        {
            BattlegroundInvitation invitation = chr.Battlegrounds.Invitation;
            BattlegroundTeam battlegroundTeam =
                invitation == null ? this.GetTeam(chr.FactionGroup.GetBattlegroundSide()) : invitation.Team;
            chr.TeleportTo((Map) this, battlegroundTeam.StartPosition, new float?(battlegroundTeam.StartOrientation));
        }

        public override void TeleportOutside(Character chr)
        {
            chr.Battlegrounds.TeleportBack();
        }

        protected override void OnEnter(Character chr)
        {
            base.OnEnter(chr);
            BattlegroundInvitation invitation = chr.Battlegrounds.Invitation;
            if (invitation == null)
                return;
            BattlegroundTeam team = invitation.Team;
            if (team.Battleground != this)
            {
                team.Battleground.TeleportInside(chr);
            }
            else
            {
                chr.RemoveUpdateAction((ObjectUpdateTimer) invitation.CancelTimer);
                chr.SpellCast.TriggerSelf(this._healingReductionSpell);
                this.JoinTeam(chr, team);
                BattlegroundHandler.SendStatusActive(chr, this, invitation.QueueIndex);
            }
        }

        protected void JoinTeam(Character chr, BattlegroundTeam team)
        {
            if (chr.Battlegrounds.Team != null)
                chr.Battlegrounds.Team.RemoveMember(chr);
            chr.Battlegrounds.Stats = this.CreateStats();
            team.AddMember(chr);
            if (this._status != BattlegroundStatus.None || (long) this.PlayerCount <
                (long) this.MaxPlayerCount * (long) Battleground.StartPlayerPct / 100L)
                return;
            this.StartPreparation();
        }

        /// <summary>Is called when a Character logs back in</summary>
        protected internal virtual bool LogBackIn(Character chr)
        {
            BattlegroundSide battlegroundTeam = chr.Record.BattlegroundTeam;
            BattlegroundTeam team = this.GetTeam(battlegroundTeam);
            if (battlegroundTeam != BattlegroundSide.End && !team.IsFull)
            {
                BattlegroundTeamQueue queue = team.Queue;
                int queueIndex =
                    chr.Battlegrounds.AddRelation(new BattlegroundRelation(queue, (ICharacterSet) chr, false));
                chr.Battlegrounds.Invitation = new BattlegroundInvitation(team, queueIndex);
            }
            else if (!chr.Role.IsStaff)
            {
                this.TeleportOutside(chr);
                return false;
            }

            return true;
        }

        protected override void OnLeave(Character chr)
        {
            base.OnLeave(chr);
            BattlegroundTeam team = chr.Battlegrounds.Team;
            if (team == null)
                return;
            if (chr.Battlegrounds.Invitation != null)
                chr.Battlegrounds.RemoveRelation(this.Template.Id);
            team.RemoveMember(chr);
            this.ProcessPendingPlayers(team);
            if (this.IsActive && !chr.Role.IsStaff)
                chr.Auras.CreateSelf(BattlegroundMgr.DeserterSpell, false);
            chr.Auras.Remove(this._healingReductionSpell);
            this.CheckShutdown();
        }
    }
}