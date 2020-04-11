using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.Threading;

namespace WCell.RealmServer.NPCs.Vehicles
{
    public class Vehicle : NPC, ITransportInfo, IFactionMember, IWorldLocation, IHasPosition, INamedEntity, IEntity,
        INamed, IContextHandler
    {
        private VehicleSeat[] _seats;
        protected internal int _passengerCount;

        protected override HighId HighId
        {
            get { return HighId.Vehicle; }
        }

        public bool HasUnitAttachment { get; set; }

        protected internal override void SetupNPC(NPCEntry entry, NPCSpawnPoint spawnPoint)
        {
            base.SetupNPC(entry, spawnPoint);
            this.NPCFlags = NPCFlags.SpellClick;
            this.SetupSeats();
            this.SetupMoveFlags();
            this.AddMessage((Action) (() =>
            {
                this.Level = entry.GetRandomLevel();
                this.PowerType = PowerType.Energy;
                this.MaxPower = entry.VehicleEntry.PowerType == VehiclePowerType.Pyrite ? 50 : 100;
                this.Power = this.MaxPower;
                if (entry.Spells != null)
                    return;
                this.PowerType = PowerType.End;
            }));
        }

        private void SetupMoveFlags()
        {
            VehicleFlags flags = this.Entry.VehicleEntry.Flags;
            if (flags.HasAnyFlag(VehicleFlags.PreventJumping))
                this.MovementFlags2 |= MovementFlags2.PreventJumping;
            if (flags.HasAnyFlag(VehicleFlags.PreventStrafe))
                this.MovementFlags2 |= MovementFlags2.PreventStrafe;
            if (flags.HasAnyFlag(VehicleFlags.FullSpeedTurning))
                this.MovementFlags2 |= MovementFlags2.FullSpeedTurning;
            if (flags.HasAnyFlag(VehicleFlags.AlwaysAllowPitching))
                this.MovementFlags2 |= MovementFlags2.AlwaysAllowPitching;
            if (!flags.HasAnyFlag(VehicleFlags.FullSpeedPitching))
                return;
            this.MovementFlags2 |= MovementFlags2.FullSpeedPitching;
        }

        private void SetupSeats()
        {
            VehicleSeatEntry[] seats = this.m_entry.VehicleEntry.Seats;
            this._seats = new VehicleSeat[seats.Length];
            for (int i = 0; i < seats.Length; i++)
            {
                VehicleSeatEntry entry = seats[i];
                if (entry != null)
                {
                    this._seats[i] = new VehicleSeat(this, entry, (byte) i);
                    if (this._seats[i].Entry.PassengerNPCId != 0u)
                    {
                        this.HasUnitAttachment = true;
                        this._seats[i].CharacterCanEnterOrExit = false;
                        int seat = i;
                        base.AddMessage(delegate()
                        {
                            NPCEntry ent = NPCMgr.GetEntry(entry.PassengerNPCId);
                            if (ent == null)
                            {
                                return;
                            }

                            NPC npc = ent.SpawnAt(this, false);
                            npc.Brain.EnterDefaultState();
                            this._seats[seat].Enter(npc);
                        });
                    }
                }
            }
        }

        public int PassengerCount
        {
            get { return this._passengerCount; }
        }

        public int SeatCount
        {
            get { return this.m_entry.VehicleEntry.SeatCount; }
        }

        public int FreeSeats
        {
            get { return this.m_entry.VehicleEntry.SeatCount - this._passengerCount; }
        }

        public bool IsFull
        {
            get { return this.FreeSeats < 1; }
        }

        public VehicleSeat[] Seats
        {
            get { return this._seats; }
        }

        public Unit Driver
        {
            get { return this._seats[0].Passenger; }
        }

        public override bool SetPosition(Vector3 pt)
        {
            bool flag = this.m_Map.MoveObject((WorldObject) this, ref pt);
            foreach (VehicleSeat vehicleSeat in ((IEnumerable<VehicleSeat>) this._seats).Where<VehicleSeat>(
                (Func<VehicleSeat, bool>) (seat =>
                {
                    if (seat != null)
                        return seat.Passenger != null;
                    return false;
                })))
                flag = vehicleSeat.Passenger.SetPosition(pt + vehicleSeat.Entry.AttachmentOffset);
            return flag;
        }

        public override bool SetPosition(Vector3 pt, float orientation)
        {
            if (!this.m_Map.MoveObject((WorldObject) this, ref pt))
                return false;
            this.m_orientation = orientation;
            bool flag = true;
            foreach (VehicleSeat vehicleSeat in ((IEnumerable<VehicleSeat>) this._seats).Where<VehicleSeat>(
                (Func<VehicleSeat, bool>) (seat =>
                {
                    if (seat != null)
                        return seat.Passenger != null;
                    return false;
                })))
            {
                flag = vehicleSeat.Passenger.SetPosition(pt + vehicleSeat.Entry.AttachmentOffset);
                vehicleSeat.Passenger.Orientation = orientation + vehicleSeat.Entry.PassengerYaw;
            }

            return flag;
        }

        public bool CanEnter(Unit unit)
        {
            if (this.IsAtLeastNeutralWith((IFactionMember) unit))
                return !this.IsFull;
            return false;
        }

        public VehicleSeat GetFirstFreeSeat(bool isCharacter)
        {
            for (int index = 0; index < this._seats.Length; ++index)
            {
                VehicleSeat seat = this._seats[index];
                if (seat != null && (!isCharacter || seat.CharacterCanEnterOrExit) && !seat.IsOccupied)
                    return seat;
            }

            return (VehicleSeat) null;
        }

        /// <summary>
        /// Returns null if unit may not enter or there is no free seat available
        /// </summary>
        public VehicleSeat GetSeatFor(Unit unit)
        {
            if (!this.CanEnter(unit))
                return (VehicleSeat) null;
            return this.GetFirstFreeSeat(unit is Character);
        }

        public void ClearAllSeats(bool onlyClearUsableSeats = false)
        {
            foreach (VehicleSeat seat in this._seats)
            {
                if (seat != null && (!onlyClearUsableSeats || seat.CharacterCanEnterOrExit))
                    seat.ClearSeat();
            }

            this.Dismiss();
        }

        public void Dismiss()
        {
            if (!this.Entry.VehicleEntry.IsMinion)
                return;
            this.Delete();
        }

        /// <summary>Returns null if the passenger could not be found</summary>
        public VehicleSeat FindSeatOccupiedBy(EntityId entityId)
        {
            return ((IEnumerable<VehicleSeat>) this.Seats).Where<VehicleSeat>((Func<VehicleSeat, bool>) (vehicleSeat =>
            {
                if (vehicleSeat != null && vehicleSeat.Passenger != null)
                    return vehicleSeat.Passenger.EntityId == entityId;
                return false;
            })).FirstOrDefault<VehicleSeat>();
        }

        /// <summary>Returns null if the unit could not be found</summary>
        public VehicleSeat FindSeatOccupiedBy(Unit passenger)
        {
            return ((IEnumerable<VehicleSeat>) this.Seats).Where<VehicleSeat>((Func<VehicleSeat, bool>) (vehicleSeat =>
            {
                if (vehicleSeat != null && vehicleSeat.Passenger != null)
                    return vehicleSeat.Passenger == passenger;
                return false;
            })).FirstOrDefault<VehicleSeat>();
        }

        protected internal override void DeleteNow()
        {
            if (this.HasUnitAttachment)
            {
                foreach (VehicleSeat vehicleSeat in ((IEnumerable<VehicleSeat>) this.Seats).Where<VehicleSeat>(
                    (Func<VehicleSeat, bool>) (seat => seat != null)))
                {
                    if (vehicleSeat.Passenger != null && vehicleSeat.HasUnitAttachment)
                        vehicleSeat.Passenger.Delete();
                }
            }

            this.ClearAllSeats(false);
            base.DeleteNow();
        }

        public override UpdateFlags UpdateFlags
        {
            get
            {
                return UpdateFlags.Flag_0x10 | UpdateFlags.Living | UpdateFlags.StationaryObject | UpdateFlags.Vehicle;
            }
        }

        protected override void WriteTypeSpecificMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation,
            UpdateFlags updateFlags)
        {
            base.WriteTypeSpecificMovementUpdate(writer, relation, updateFlags);
            writer.Write(this.m_entry.VehicleId);
            writer.Write(this.m_entry.VehicleAimAdjustment);
        }
    }
}