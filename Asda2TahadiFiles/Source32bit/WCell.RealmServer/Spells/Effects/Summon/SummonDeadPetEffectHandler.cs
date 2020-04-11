using WCell.Constants.Spells;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Spells.Effects
{
  public class SummonDeadPetEffectHandler : SummonEffectHandler
  {
    public SummonDeadPetEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override SpellFailedReason Initialize()
    {
      return m_cast.CasterChar.ActivePet == null ? SpellFailedReason.NoPet : SpellFailedReason.Ok;
    }

    public override void Apply()
    {
      m_cast.CasterChar.ActivePet.HealthPct = CalcEffectValue();
    }

    public override ObjectTypes CasterType
    {
      get { return ObjectTypes.Player; }
    }

    public override bool HasOwnTargets
    {
      get { return false; }
    }
  }
}