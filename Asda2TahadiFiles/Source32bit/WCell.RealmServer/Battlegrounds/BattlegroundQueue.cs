using WCell.Constants;
using WCell.Core;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Battlegrounds
{
    /// <summary>
    /// A BattlegroundQueue manages access of Players to Battlegrounds
    /// </summary>
    public abstract class BattlegroundQueue
    {
        public readonly BattlegroundTeamQueue[] TeamQueues = new BattlegroundTeamQueue[2];
        protected BattlegroundTemplate m_Template;
        protected int m_BracketId;
        protected int m_MinLevel;
        protected int m_MaxLevel;

        protected BattlegroundQueue()
        {
            this.TeamQueues[0] = this.CreateTeamQueue(BattlegroundSide.Alliance);
            this.TeamQueues[1] = this.CreateTeamQueue(BattlegroundSide.Horde);
        }

        public BattlegroundQueue(BattlegroundTemplate template, int bracketId, int minLevel, int maxLevel)
            : this()
        {
            this.m_Template = template;
            this.m_BracketId = bracketId;
            this.m_MinLevel = minLevel;
            this.m_MaxLevel = maxLevel;
        }

        protected abstract BattlegroundTeamQueue CreateTeamQueue(BattlegroundSide side);

        public int BracketId
        {
            get { return this.m_BracketId; }
        }

        public int MinLevel
        {
            get { return this.m_MinLevel; }
        }

        public int MaxLevel
        {
            get { return this.m_MaxLevel; }
        }

        public BattlegroundTemplate Template
        {
            get { return this.m_Template; }
        }

        public abstract bool RequiresLocking { get; }

        public BattlegroundTeamQueue GetTeamQueue(Character chr)
        {
            return this.GetTeamQueue(chr.Faction.Group.GetBattlegroundSide());
        }

        public BattlegroundTeamQueue GetTeamQueue(BattlegroundSide side)
        {
            return this.TeamQueues[(int) side];
        }

        public uint InstanceId
        {
            get
            {
                if (this.Battleground == null)
                    return 0;
                return this.Battleground.InstanceId;
            }
        }

        public bool CanEnter(Character chr)
        {
            if (chr.Level.IsBetween(this.MinLevel, this.MaxLevel))
                return this.m_Template.MapTemplate.MayEnter(chr);
            return false;
        }

        public abstract Battleground Battleground { get; }

        public int AverageWaitTime
        {
            get { return 5000; }
        }

        protected internal virtual void Dispose()
        {
            foreach (BattlegroundTeamQueue teamQueue in this.TeamQueues)
            {
                foreach (BattlegroundRelation pendingRequest in teamQueue.PendingRequests)
                    pendingRequest.Cancel();
            }
        }
    }
}