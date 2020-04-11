using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Handlers
{
    public class Asda2SoulmateSkillEmpower : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillEmpower()
            : base(Asda2SoulmateSkillId.Empower, (byte) 5, 60)
        {
        }

        public override bool TryCast(Character caster, Character friend)
        {
            if ((double) caster.GetDistance((WorldObject) friend) > 40.0 || friend.IsDead)
                return false;
            return base.TryCast(caster, friend);
        }

        public override void Action(Character caster, Character friend)
        {
            caster.AddFriendEmpower(false);
            friend.AddFriendEmpower(true);
            base.Action(caster, friend);
        }
    }
}