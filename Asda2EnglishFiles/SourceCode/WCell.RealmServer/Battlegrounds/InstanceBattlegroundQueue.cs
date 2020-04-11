using WCell.Constants;

namespace WCell.RealmServer.Battlegrounds
{
    public class InstanceBattlegroundQueue : BattlegroundQueue
    {
        private Battleground m_battleground;

        public InstanceBattlegroundQueue(Battleground bg)
        {
            this.m_battleground = bg;
            this.m_Template = bg.Template;
            this.m_MinLevel = bg.MinLevel;
            this.m_MaxLevel = bg.MaxLevel;
        }

        protected override BattlegroundTeamQueue CreateTeamQueue(BattlegroundSide side)
        {
            return (BattlegroundTeamQueue) new InstanceBGTeamQueue(this, side);
        }

        public override bool RequiresLocking
        {
            get { return false; }
        }

        public override Battleground Battleground
        {
            get { return this.m_battleground; }
        }

        protected internal override void Dispose()
        {
            base.Dispose();
            this.m_battleground = (Battleground) null;
        }
    }
}