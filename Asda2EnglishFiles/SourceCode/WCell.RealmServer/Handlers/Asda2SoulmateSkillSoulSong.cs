using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Handlers
{
    public class Asda2SoulmateSkillSoulSong : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillSoulSong()
            : base(Asda2SoulmateSkillId.Empower, (byte) 5, 43200)
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
            caster.AddSoulmateSong();
            friend.AddSoulmateSong();
            base.Action(caster, friend);
        }
    }
}