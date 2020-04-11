using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs.Vehicles;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    public class VehicleAuraHandler : AuraEffectHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private Unit Caster;
        private Vehicle Vehicle;
        private VehicleSeat Seat;

        protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
            Unit target, ref SpellFailedReason failReason)
        {
            this.Caster = casterReference.Object as Unit;
            if (this.Caster == null || this.Caster is Vehicle)
            {
                VehicleAuraHandler.log.Warn("Invalid SpellCaster \"{0}\" for Spell: {1}", (object) this.Caster,
                    (object) this.SpellEffect.Spell);
                failReason = SpellFailedReason.Error;
            }
            else
            {
                this.Vehicle = target as Vehicle;
                if (this.Vehicle == null)
                {
                    failReason = SpellFailedReason.BadTargets;
                }
                else
                {
                    this.Seat = this.Vehicle.GetSeatFor(this.Caster);
                    if (this.Seat != null)
                        return;
                    failReason = SpellFailedReason.BadTargets;
                }
            }
        }

        protected override void Apply()
        {
            this.Seat.Enter(this.Caster);
        }

        protected override void Remove(bool cancelled)
        {
            if (!this.Caster.IsInWorld || this.Seat.Passenger != this.Caster)
                return;
            this.Seat.ClearSeat();
        }
    }
}