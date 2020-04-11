using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Handlers
{
    public class Asda2SoulmateSkillSoulSave : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillSoulSave()
            : base(Asda2SoulmateSkillId.SoulSave, (byte) 10, 1200)
        {
        }

        public override bool TryCast(Character caster, Character friend)
        {
            if ((double) caster.GetDistance((WorldObject) friend) > 40.0 || friend.IsDead ||
                (caster.IsSoulmateSoulSaved || friend.IsSoulmateSoulSaved))
                return false;
            return base.TryCast(caster, friend);
        }

        public override void Action(Character caster, Character friend)
        {
            friend.IsSoulmateSoulSaved = true;
            friend.SendInfoMsg("Your friend saved your soul.");
            base.Action(caster, friend);
        }
    }
}