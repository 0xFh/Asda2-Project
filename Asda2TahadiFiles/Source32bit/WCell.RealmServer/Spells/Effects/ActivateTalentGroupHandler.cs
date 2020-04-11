using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
  public class ActivateTalentGroupHandler : SpellEffectHandler
  {
    public ActivateTalentGroupHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override void Apply()
    {
      int basePoints = Effect.BasePoints;
      Character casterObject = Cast.CasterObject as Character;
      if(casterObject == null)
        return;
      casterObject.ApplyTalentSpec(basePoints);
    }
  }
}