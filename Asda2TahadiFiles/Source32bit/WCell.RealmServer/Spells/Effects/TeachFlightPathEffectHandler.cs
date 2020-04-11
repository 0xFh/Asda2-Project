using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Spells.Effects
{
  public class TeachFlightPathEffectHandler : SpellEffectHandler
  {
    public TeachFlightPathEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Player; }
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      Character character = target as Character;
      character.TaxiNodes.Activate((uint) Effect.MiscValue);
      TaxiHandler.SendTaxiPathActivated(character.Client);
      TaxiHandler.SendTaxiPathUpdate(character.Client, Cast.CasterUnit.EntityId, true);
    }
  }
}