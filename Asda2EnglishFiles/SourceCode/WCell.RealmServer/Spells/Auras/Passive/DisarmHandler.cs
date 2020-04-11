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
            this.Owner.IncMechanicCount(SpellMechanic.Disarmed, false);
            this.Owner.SetDisarmed(this.DisarmType);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.DecMechanicCount(SpellMechanic.Disarmed, false);
            this.Owner.UnsetDisarmed(this.DisarmType);
        }
    }
}