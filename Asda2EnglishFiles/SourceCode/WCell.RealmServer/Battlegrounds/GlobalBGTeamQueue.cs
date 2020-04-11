using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Battlegrounds
{
    public class GlobalBGTeamQueue : BattlegroundTeamQueue
    {
        public GlobalBGTeamQueue(GlobalBattlegroundQueue parentQueue, BattlegroundSide side)
            : base((BattlegroundQueue) parentQueue, side)
        {
        }

        public GlobalBattlegroundQueue GlobalQueue
        {
            get { return (GlobalBattlegroundQueue) this.ParentQueue; }
        }

        public override BattlegroundRelation Enqueue(ICharacterSet chrs)
        {
            BattlegroundRelation battlegroundRelation = base.Enqueue(chrs);
            ((GlobalBattlegroundQueue) this.ParentQueue).CheckBGCreation();
            return battlegroundRelation;
        }
    }
}