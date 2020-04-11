using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Handlers
{
  public class Asda2SoulmateSkillHeal : Asda2SoulmateSkill
  {
    public Asda2SoulmateSkillHeal()
      : base(Asda2SoulmateSkillId.Heal, 5, 60)
    {
    }

    public override bool TryCast(Character caster, Character friend)
    {
      if(caster.GetDistance(friend) > 40.0 || friend.HealthPct >= 90 || friend.IsDead)
        return false;
      return base.TryCast(caster, friend);
    }

    public override void Action(Character caster, Character friend)
    {
      friend.HealPercent(50, null, null);
      base.Action(caster, friend);
    }
  }
}