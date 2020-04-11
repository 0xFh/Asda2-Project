using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells.Effects
{
  /// <summary>Is used to feed the currently ActivePet of the Caster</summary>
  public class FeedPetEffectHandler : SpellEffectHandler
  {
    public FeedPetEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override bool HasOwnTargets
    {
      get { return false; }
    }

    public override ObjectTypes CasterType
    {
      get { return ObjectTypes.Player; }
    }

    public override SpellFailedReason Initialize()
    {
      NPC activePet = Cast.CasterChar.ActivePet;
      if(activePet == null)
        return SpellFailedReason.BadImplicitTargets;
      if(Cast.TargetItem == null)
        return SpellFailedReason.ItemNotFound;
      ItemTemplate template = Cast.TargetItem.Template;
      if(!activePet.CanEat(template.m_PetFood))
        return SpellFailedReason.WrongPetFood;
      long num = activePet.Level - template.Level;
      if(num > 35L)
        return SpellFailedReason.FoodLowlevel;
      return num < -15L ? SpellFailedReason.Highlevel : SpellFailedReason.Ok;
    }

    public override void Apply()
    {
      NPC activePet = Cast.CasterChar.ActivePet;
      ItemTemplate template = Cast.TargetItem.Template;
      if(activePet == null || template == null)
        return;
      Cast.Trigger(Effect.TriggerSpell, (WorldObject) activePet);
      Aura aura = activePet.Auras[Effect.TriggerSpellId];
      if(aura == null)
        return;
      AuraEffectHandler handler = aura.GetHandler(AuraType.PeriodicEnergize);
      if(handler == null)
        return;
      handler.BaseEffectValue = activePet.GetHappinessGain(template);
    }
  }
}