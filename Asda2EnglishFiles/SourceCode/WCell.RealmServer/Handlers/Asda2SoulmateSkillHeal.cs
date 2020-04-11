using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Handlers
{
    public class Asda2SoulmateSkillHeal : Asda2SoulmateSkill
    {
        public Asda2SoulmateSkillHeal()
            : base(Asda2SoulmateSkillId.Heal, (byte) 5, 60)
        {
        }

        public override bool TryCast(Character caster, Character friend)
        {
            if ((double) caster.GetDistance((WorldObject) friend) > 40.0 || friend.HealthPct >= 90 || friend.IsDead)
                return false;
            return base.TryCast(caster, friend);
        }

        public override void Action(Character caster, Character friend)
        {
            friend.HealPercent(50, (Unit) null, (SpellEffect) null);
            base.Action(caster, friend);
        }
    }
}