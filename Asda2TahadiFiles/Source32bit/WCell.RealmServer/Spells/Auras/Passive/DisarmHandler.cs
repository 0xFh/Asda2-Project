using WCell.Constants.Items;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Simply disarm melee and ranged for now</summary>
  public abstract class DisarmHandler : AuraEffectHandler
  {
    public abstract InventorySlotType DisarmType { get; }

    protected override void Apply()
    {
      Owner.IncMechanicCount(SpellMechanic.Disarmed, false);
      Owner.SetDisarmed(DisarmType);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.DecMechanicCount(SpellMechanic.Disarmed, false);
      Owner.UnsetDisarmed(DisarmType);
    }
  }
}