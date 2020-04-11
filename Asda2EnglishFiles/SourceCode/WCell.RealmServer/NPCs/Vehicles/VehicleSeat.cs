using System;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs.Vehicles
{
    public class VehicleSeat
    {
        public readonly Vehicle Vehicle;
        public readonly VehicleSeatEntry Entry;
        public readonly byte Index;
        private Unit _passenger;

        public bool IsDriverSeat
        {
            get { return this.Entry.Flags.HasAnyFlag(VehicleSeatFlags.VehicleControlSeat); }
        }

        public bool CharacterCanEnterOrExit
        {
            get { return this.Entry.Flags.HasAnyFlag(VehicleSeatFlags.CanEnterorExit); }
            internal set
            {
                if (!value)
                    this.Entry.Flags &= VehicleSeatFlags.HasLowerAnimForEnter | VehicleSeatFlags.HasLowerAnimForRide |
                                        VehicleSeatFlags.Flagx4 |
                                        VehicleSeatFlags.ShouldUseVehicleSeatExitAnimationOnVoluntaryExit |
                                        VehicleSeatFlags.Flagx10 | VehicleSeatFlags.Flagx20 | VehicleSeatFlags.Flagx40 |
                                        VehicleSeatFlags.Flagx80 | VehicleSeatFlags.Flagx100 |
                                        VehicleSeatFlags.HidePassenger | VehicleSeatFlags.Flagx400 |
                                        VehicleSeatFlags.VehicleControlSeat | VehicleSeatFlags.Flagx1000 |
                                        VehicleSeatFlags.Uncontrolled | VehicleSeatFlags.CanAttack |
                                        VehicleSeatFlags.ShouldUseVehicleSeatExitAnimationOnForcedExit |
                                        VehicleSeatFlags.Flagx10000 | VehicleSeatFlags.Flagx20000 |
                                        VehicleSeatFlags.HasVehicleExitAnimForVoluntaryExit |
                                        VehicleSeatFlags.HasVehicleExitAnimForForcedExit |
                                        VehicleSeatFlags.Flagx100000 | VehicleSeatFlags.Flagx200000 |
                                        VehicleSeatFlags.RecHasVehicleEnterAnim | VehicleSeatFlags.Flagx800000 |
                                        VehicleSeatFlags.EnableVehicleZoom | VehicleSeatFlags.CanSwitchSeats |
                                        VehicleSeatFlags.HasStartWaitingForVehicleTransitionAnim_Enter |
                                        VehicleSeatFlags.HasStartWaitingForVehicleTransitionAnim_Exit |
                                        VehicleSeatFlags.CanCast | VehicleSeatFlags.Flagx40000000 |
                                        VehicleSeatFlags.AllowsInteraction;
                else
                    this.Entry.Flags |= VehicleSeatFlags.CanEnterorExit;
            }
        }

        public bool HasUnitAttachment
        {
            get { return this.Entry.PassengerNPCId != 0U; }
        }

        public VehicleSeat(Vehicle vehicle, VehicleSeatEntry entry, byte index)
        {
            this.Vehicle = vehicle;
            this.Entry = entry;
            this.Index = index;
        }

        public bool IsOccupied
        {
            get { return this._passenger != null; }
        }

        public Unit Passenger
        {
            get { return this._passenger; }
            internal set { this._passenger = value; }
        }

        /// <summary>Add Passenger</summary>
        public void Enter(Unit passenger)
        {
            this._passenger = passenger;
            passenger.m_vehicleSeat = this;
            passenger.MovementFlags |= MovementFlags.OnTransport;
            passenger.TransportPosition = this.Entry.AttachmentOffset;
            passenger.TransportOrientation = this.Entry.PassengerYaw;
            ++this.Vehicle._passengerCount;
            if (this.IsDriverSeat)
            {
                this.Vehicle.Charmer = passenger;
                passenger.Charm = (Unit) this.Vehicle;
                this.Vehicle.UnitFlags |= UnitFlags.Possessed;
            }

            Character chr = passenger as Character;
            Vector3 position = this.Vehicle.Position;
            if (chr != null)
            {
                VehicleHandler.Send_SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA(chr);
                VehicleHandler.SendBreakTarget((IPacketReceiver) chr, (IEntity) this.Vehicle);
            }

            MovementHandler.SendEnterTransport(passenger);
            if (chr != null)
            {
                MiscHandler.SendCancelAutoRepeat((IPacketReceiver) chr, (IEntity) this.Vehicle);
                PetHandler.SendVehicleSpells((IPacketReceiver) chr, (NPC) this.Vehicle);
            }

            passenger.IncMechanicCount(SpellMechanic.Rooted, true);
            passenger.HasPermissionToMove = false;
            passenger.MovementFlags |= MovementFlags.Root;
            if (chr == null)
                return;
            chr.SetMover((WorldObject) this.Vehicle, this.IsDriverSeat);
            chr.FarSight = this.Vehicle.EntityId;
        }

        /// <summary>Remove Passenger</summary>
        public void ClearSeat()
        {
            if (this._passenger == null)
                return;
            if (this.IsDriverSeat)
            {
                this.Vehicle.Charmer = (Unit) null;
                this._passenger.Charm = (Unit) null;
                this.Vehicle.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 |
                                          UnitFlags.SelectableNotAttackable | UnitFlags.Influenced |
                                          UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 | UnitFlags.Preparation |
                                          UnitFlags.PlusMob | UnitFlags.SelectableNotAttackable_2 |
                                          UnitFlags.NotAttackable | UnitFlags.Passive | UnitFlags.Looting |
                                          UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 | UnitFlags.Silenced |
                                          UnitFlags.Flag_14_0x4000 | UnitFlags.Flag_15_0x8000 |
                                          UnitFlags.SelectableNotAttackable_3 | UnitFlags.Combat |
                                          UnitFlags.TaxiFlight | UnitFlags.Disarmed | UnitFlags.Confused |
                                          UnitFlags.Feared | UnitFlags.NotSelectable | UnitFlags.Skinnable |
                                          UnitFlags.Mounted | UnitFlags.Flag_28_0x10000000 |
                                          UnitFlags.Flag_29_0x20000000 | UnitFlags.Flag_30_0x40000000 |
                                          UnitFlags.Flag_31_0x80000000;
            }

            --this.Vehicle._passengerCount;
            if (this._passenger.MovementFlags.HasFlag((Enum) MovementFlags.Flying))
            {
                SpellCast spellCast = this.Vehicle.SpellCast;
                if (spellCast != null)
                    spellCast.Trigger(SpellId.EffectParachute);
            }

            this._passenger.MovementFlags &= MovementFlags.MaskMoving | MovementFlags.PitchUp |
                                             MovementFlags.PitchDown | MovementFlags.WalkMode |
                                             MovementFlags.DisableGravity | MovementFlags.Root |
                                             MovementFlags.PendingStop | MovementFlags.PendingStrafeStop |
                                             MovementFlags.PendingForward | MovementFlags.PendingBackward |
                                             MovementFlags.PendingStrafeLeft | MovementFlags.PendingStrafeRight |
                                             MovementFlags.PendingRoot | MovementFlags.Swimming | MovementFlags.CanFly |
                                             MovementFlags.Flying | MovementFlags.SplineElevation |
                                             MovementFlags.SplineEnabled | MovementFlags.Waterwalking |
                                             MovementFlags.CanSafeFall | MovementFlags.Hover | MovementFlags.LocalDirty;
            this._passenger.Auras.RemoveFirstVisibleAura((Predicate<Aura>) (aura => aura.Spell.IsVehicle));
            if (this._passenger is Character)
            {
                Character passenger = (Character) this._passenger;
                VehicleHandler.Send_SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA(passenger);
                MovementHandler.SendMoved(passenger);
                MiscHandler.SendCancelAutoRepeat((IPacketReceiver) passenger, (IEntity) this.Vehicle);
                PetHandler.SendEmptySpells((IPacketReceiver) passenger);
                passenger.ResetMover();
                passenger.FarSight = EntityId.Zero;
            }

            this._passenger.m_vehicleSeat = (VehicleSeat) null;
            MovementHandler.SendHeartbeat(this._passenger, this._passenger.Position, this._passenger.Orientation);
            this._passenger.DecMechanicCount(SpellMechanic.Rooted, true);
            this._passenger.HasPermissionToMove = true;
            this._passenger.MovementFlags &= MovementFlags.MaskMoving | MovementFlags.PitchUp |
                                             MovementFlags.PitchDown | MovementFlags.WalkMode |
                                             MovementFlags.OnTransport | MovementFlags.DisableGravity |
                                             MovementFlags.PendingStop | MovementFlags.PendingStrafeStop |
                                             MovementFlags.PendingForward | MovementFlags.PendingBackward |
                                             MovementFlags.PendingStrafeLeft | MovementFlags.PendingStrafeRight |
                                             MovementFlags.PendingRoot | MovementFlags.Swimming | MovementFlags.CanFly |
                                             MovementFlags.Flying | MovementFlags.SplineElevation |
                                             MovementFlags.SplineEnabled | MovementFlags.Waterwalking |
                                             MovementFlags.CanSafeFall | MovementFlags.Hover | MovementFlags.LocalDirty;
            this._passenger = (Unit) null;
        }
    }
}