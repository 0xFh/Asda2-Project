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
      NPCFlags = NPCFlags.SpellClick;
      SetupSeats();
      SetupMoveFlags();
      AddMessage(() =>
      {
        Level = entry.GetRandomLevel();
        PowerType = PowerType.Energy;
        MaxPower = entry.VehicleEntry.PowerType == VehiclePowerType.Pyrite ? 50 : 100;
        Power = MaxPower;
        if(entry.Spells != null)
          return;
        PowerType = PowerType.End;
      });
    }

    private void SetupMoveFlags()
    {
      VehicleFlags flags = Entry.VehicleEntry.Flags;
      if(flags.HasAnyFlag(VehicleFlags.PreventJumping))
        MovementFlags2 |= MovementFlags2.PreventJumping;
      if(flags.HasAnyFlag(VehicleFlags.PreventStrafe))
        MovementFlags2 |= MovementFlags2.PreventStrafe;
      if(flags.HasAnyFlag(VehicleFlags.FullSpeedTurning))
        MovementFlags2 |= MovementFlags2.FullSpeedTurning;
      if(flags.HasAnyFlag(VehicleFlags.AlwaysAllowPitching))
        MovementFlags2 |= MovementFlags2.AlwaysAllowPitching;
      if(!flags.HasAnyFlag(VehicleFlags.FullSpeedPitching))
        return;
      MovementFlags2 |= MovementFlags2.FullSpeedPitching;
    }

    private void SetupSeats()
    {
      VehicleSeatEntry[] seats = m_entry.VehicleEntry.Seats;
      _seats = new VehicleSeat[seats.Length];
      for(int i = 0; i < seats.Length; i++)
      {
        VehicleSeatEntry entry = seats[i];
        if(entry != null)
        {
          _seats[i] = new VehicleSeat(this, entry, (byte) i);
          if(_seats[i].Entry.PassengerNPCId != 0u)
          {
            HasUnitAttachment = true;
            _seats[i].CharacterCanEnterOrExit = false;
            int seat = i;
            AddMessage(delegate
            {
              NPCEntry ent = NPCMgr.GetEntry(entry.PassengerNPCId);
              if(ent == null)
              {
                return;
              }

              NPC npc = ent.SpawnAt(this, false);
              npc.Brain.EnterDefaultState();
              _seats[seat].Enter(npc);
            });
          }
        }
      }
    }

    public int PassengerCount
    {
      get { return _passengerCount; }
    }

    public int SeatCount
    {
      get { return m_entry.VehicleEntry.SeatCount; }
    }

    public int FreeSeats
    {
      get { return m_entry.VehicleEntry.SeatCount - _passengerCount; }
    }

    public bool IsFull
    {
      get { return FreeSeats < 1; }
    }

    public VehicleSeat[] Seats
    {
      get { return _seats; }
    }

    public Unit Driver
    {
      get { return _seats[0].Passenger; }
    }

    public override bool SetPosition(Vector3 pt)
    {
      bool flag = m_Map.MoveObject(this, ref pt);
      foreach(VehicleSeat vehicleSeat in _seats.Where(
        seat =>
        {
          if(seat != null)
            return seat.Passenger != null;
          return false;
        }))
        flag = vehicleSeat.Passenger.SetPosition(pt + vehicleSeat.Entry.AttachmentOffset);
      return flag;
    }

    public override bool SetPosition(Vector3 pt, float orientation)
    {
      if(!m_Map.MoveObject(this, ref pt))
        return false;
      m_orientation = orientation;
      bool flag = true;
      foreach(VehicleSeat vehicleSeat in _seats.Where(
        seat =>
        {
          if(seat != null)
            return seat.Passenger != null;
          return false;
        }))
      {
        flag = vehicleSeat.Passenger.SetPosition(pt + vehicleSeat.Entry.AttachmentOffset);
        vehicleSeat.Passenger.Orientation = orientation + vehicleSeat.Entry.PassengerYaw;
      }

      return flag;
    }

    public bool CanEnter(Unit unit)
    {
      if(IsAtLeastNeutralWith(unit))
        return !IsFull;
      return false;
    }

    public VehicleSeat GetFirstFreeSeat(bool isCharacter)
    {
      for(int index = 0; index < _seats.Length; ++index)
      {
        VehicleSeat seat = _seats[index];
        if(seat != null && (!isCharacter || seat.CharacterCanEnterOrExit) && !seat.IsOccupied)
          return seat;
      }

      return null;
    }

    /// <summary>
    /// Returns null if unit may not enter or there is no free seat available
    /// </summary>
    public VehicleSeat GetSeatFor(Unit unit)
    {
      if(!CanEnter(unit))
        return null;
      return GetFirstFreeSeat(unit is Character);
    }

    public void ClearAllSeats(bool onlyClearUsableSeats = false)
    {
      foreach(VehicleSeat seat in _seats)
      {
        if(seat != null && (!onlyClearUsableSeats || seat.CharacterCanEnterOrExit))
          seat.ClearSeat();
      }

      Dismiss();
    }

    public void Dismiss()
    {
      if(!Entry.VehicleEntry.IsMinion)
        return;
      Delete();
    }

    /// <summary>Returns null if the passenger could not be found</summary>
    public VehicleSeat FindSeatOccupiedBy(EntityId entityId)
    {
      return Seats.Where(vehicleSeat =>
      {
        if(vehicleSeat != null && vehicleSeat.Passenger != null)
          return vehicleSeat.Passenger.EntityId == entityId;
        return false;
      }).FirstOrDefault();
    }

    /// <summary>Returns null if the unit could not be found</summary>
    public VehicleSeat FindSeatOccupiedBy(Unit passenger)
    {
      return Seats.Where(vehicleSeat =>
      {
        if(vehicleSeat != null && vehicleSeat.Passenger != null)
          return vehicleSeat.Passenger == passenger;
        return false;
      }).FirstOrDefault();
    }

    protected internal override void DeleteNow()
    {
      if(HasUnitAttachment)
      {
        foreach(VehicleSeat vehicleSeat in Seats.Where(
          seat => seat != null))
        {
          if(vehicleSeat.Passenger != null && vehicleSeat.HasUnitAttachment)
            vehicleSeat.Passenger.Delete();
        }
      }

      ClearAllSeats(false);
      base.DeleteNow();
    }

    public override UpdateFlags UpdateFlags
    {
      get { return UpdateFlags.Flag_0x10 | UpdateFlags.Living | UpdateFlags.StationaryObject | UpdateFlags.Vehicle; }
    }

    protected override void WriteTypeSpecificMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation,
      UpdateFlags updateFlags)
    {
      base.WriteTypeSpecificMovementUpdate(writer, relation, updateFlags);
      writer.Write(m_entry.VehicleId);
      writer.Write(m_entry.VehicleAimAdjustment);
    }
  }
}