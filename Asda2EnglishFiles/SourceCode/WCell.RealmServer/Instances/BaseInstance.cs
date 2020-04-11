using System;
using WCell.Constants.Factions;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Instances
{
    /// <summary>
    /// The base class for all WoW-style "Instances" (Dungeon, Heroic, Raid etc)
    /// 
    /// TODO:
    /// - SMSG_INSTANCE_RESET_FAILURE: The party leader has attempted to reset the instance you are in. Please zone out to allow the instance to reset.
    /// </summary>
    public abstract class BaseInstance : InstancedMap, IUpdatable
    {
        /// <summary>The timeout for normal Dungeon instances</summary>
        public static int DefaultInstanceTimeoutMillis = 1800000;

        private DateTime m_expiryTime = new DateTime();
        internal FactionGroup m_OwningFaction = FactionGroup.Invalid;
        private IInstanceHolderSet m_owner;
        internal MapDifficultyEntry difficulty;
        private DateTime m_lastReset;
        private TimerEntry m_timeoutTimer;
        private InstanceProgress progress;
        private InstanceSettings settings;

        protected internal override void InitMap()
        {
            base.InitMap();
            int resetTime = this.difficulty.ResetTime;
            this.m_lastReset = DateTime.Now;
            this.m_timeoutTimer = new TimerEntry(new Action<int>(this.OnTimeout));
            base.RegisterUpdatableLater(this);
            this.settings = this.CreateSettings();
        }

        protected virtual InstanceSettings CreateSettings()
        {
            if (!this.Difficulty.IsDungeon)
                return (InstanceSettings) new RaidInstanceSettings(this);
            return (InstanceSettings) new DungeonInstanceSettings(this);
        }

        /// <summary>Whether this instance will ever expire</summary>
        public bool CanExpire
        {
            get { return this.m_expiryTime != new DateTime(); }
        }

        public DateTime ExpiryTime
        {
            get { return this.m_expiryTime; }
        }

        /// <summary>Difficulty of the instance</summary>
        public override MapDifficultyEntry Difficulty
        {
            get { return this.difficulty; }
        }

        public InstanceSettings Settings
        {
            get { return this.settings; }
        }

        public InstanceProgress Progress
        {
            get { return this.progress; }
        }

        public IInstanceHolderSet Owner
        {
            get { return this.m_owner; }
            set { this.m_owner = value; }
        }

        public override FactionGroup OwningFaction
        {
            get { return this.m_OwningFaction; }
        }

        public int TimeoutDelay
        {
            get { return BaseInstance.DefaultInstanceTimeoutMillis; }
        }

        /// <summary>The last time this Instance was reset</summary>
        public DateTime LastReset
        {
            get { return this.m_lastReset; }
        }

        /// <summary>Whether this instance can be reset</summary>
        public override bool CanReset(Character chr)
        {
            if (chr.Role.IsStaff || chr == this.m_owner.InstanceLeader)
                return this.PlayerCount == 0;
            return false;
        }

        public override void Reset()
        {
            base.Reset();
            this.m_lastReset = DateTime.Now;
        }

        protected override void OnEnter(Character chr)
        {
            base.OnEnter(chr);
            if (this.m_timeoutTimer.IsRunning)
            {
                this.m_timeoutTimer.Stop();
                Map.s_log.Debug("{0} #{1} timeout timer stopped by: {2}", (object) this.Name,
                    (object) this.m_InstanceId, (object) chr.Name);
            }

            if (chr.GodMode || this.Difficulty.BindingType != BindingType.Soft)
                return;
            this.Bind((IInstanceHolderSet) chr);
        }

        protected void Bind(IInstanceHolderSet holder)
        {
            if (holder.InstanceLeader.Group != null)
                holder.InstanceLeader.Group.ForeachCharacter((Action<Character>) (chr =>
                {
                    InstanceCollection instances = chr.Instances;
                    if (instances == null)
                        return;
                    instances.BindTo(this);
                }));
            else
                holder.InstanceLeaderCollection.BindTo(this);
        }

        protected override void OnLeave(Character chr)
        {
            if (this.PlayerCount > 1)
                return;
            if (this.TimeoutDelay > 0)
                this.m_timeoutTimer.Start(this.TimeoutDelay, 0);
            Map.s_log.Debug("{0} #{1} timeout timer started.", (object) this.Name, (object) this.m_InstanceId);
        }

        public void Update(int dt)
        {
            this.m_timeoutTimer.Update(dt);
        }

        public override bool CanEnter(Character chr)
        {
            DateTime? lastLogout = chr.LastLogout;
            DateTime lastReset = this.m_lastReset;
            int num = lastLogout.HasValue ? (lastLogout.GetValueOrDefault() > lastReset ? 1 : 0) : 0;
            if (!base.CanEnter(chr))
                return false;
            if (this.Owner == null)
                return true;
            Character instanceLeader = this.Owner.InstanceLeader;
            if (instanceLeader == null || !chr.IsAlliedWith(instanceLeader))
                return chr.GodMode;
            return true;
        }

        public override void TeleportOutside(Character chr)
        {
            chr.TeleportToNearestGraveyard();
        }

        public override void DeleteNow()
        {
            InstanceMgr.Instances.RemoveInstance(this.MapId, this.InstanceId);
            base.DeleteNow();
        }

        protected override void Dispose()
        {
            base.Dispose();
            this.m_owner = (IInstanceHolderSet) null;
        }

        public override string ToString()
        {
            string str = "";
            if (this.Owner != null && this.Owner.InstanceLeader != null)
                str = " - Owned by: " + this.Owner.InstanceLeader.Name;
            return base.ToString() + (this.difficulty.IsHeroic ? " [Heroic]" : "") + str;
        }

        public override sealed void Save()
        {
            if (this.progress == null)
                this.progress = new InstanceProgress(this.MapId, this.InstanceId);
            this.progress.ResetTime =
                DateTime.Now.AddMilliseconds((double) this.m_timeoutTimer.RemainingInitialDelayMillis);
            this.progress.DifficultyIndex = this.DifficultyIndex;
            this.PerformSave();
            this.progress.Save();
        }

        /// <summary>Method is to be overridden by instance implementation</summary>
        protected virtual void PerformSave()
        {
        }
    }
}