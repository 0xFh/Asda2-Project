using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Handlers
{
    public class Asda2SoulmateSkillResurect : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillResurect()
            : base(Asda2SoulmateSkillId.SoulSave, (byte) 18, 600)
        {
        }

        public override bool TryCast(Character caster, Character friend)
        {
            if ((double) caster.GetDistance((WorldObject) friend) > 40.0 || friend.IsAlive)
                return false;
            return base.TryCast(caster, friend);
        }

        public override void Action(Character caster, Character friend)
        {
            friend.Resurrect();
            base.Action(caster, friend);
        }
    }
}