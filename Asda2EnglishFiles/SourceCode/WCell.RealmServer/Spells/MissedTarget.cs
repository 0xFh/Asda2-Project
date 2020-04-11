using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells
{
    /// <summary>
    /// Represents a target which didn't get hit by a SpellCast
    /// </summary>
    public struct MissedTarget
    {
        public readonly WorldObject Target;
        public readonly CastMissReason Reason;

        public MissedTarget(WorldObject target, CastMissReason reason)
        {
            this.Target = target;
            this.Reason = reason;
        }

        public override bool Equals(object obj)
        {
            if (obj is MissedTarget)
                return ((MissedTarget) obj).Target == this.Target;
            return false;
        }

        public override int GetHashCode()
        {
            return this.Target.GetHashCode();
        }
    }
}