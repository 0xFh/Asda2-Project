using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIMoveIntoRangeThenExecAction : AIMoveToThenExecAction
    {
        private SimpleRange m_Range;

        public AIMoveIntoRangeThenExecAction(Unit owner, SimpleRange range, UnitActionCallback actionCallback)
            : base(owner, actionCallback)
        {
            this.m_Range = range;
        }

        public override float DistanceMin
        {
            get { return this.m_Range.MinDist; }
        }

        public override float DistanceMax
        {
            get { return this.m_Range.MaxDist; }
        }

        public override float DesiredDistance
        {
            get { return this.m_Range.Average; }
        }
    }
}