using WCell.Constants;

namespace WCell.RealmServer.Battlegrounds
{
    public class InstanceBGTeamQueue : BattlegroundTeamQueue
    {
        public InstanceBGTeamQueue(InstanceBattlegroundQueue parentQueue, BattlegroundSide side)
            : base((BattlegroundQueue) parentQueue, side)
        {
        }

        public InstanceBattlegroundQueue InstanceQueue
        {
            get { return (InstanceBattlegroundQueue) this._parentQueue; }
        }
    }
}