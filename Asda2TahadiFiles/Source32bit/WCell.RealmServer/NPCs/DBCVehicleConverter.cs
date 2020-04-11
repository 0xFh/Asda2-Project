using System;
using WCell.Constants.NPCs;
using WCell.Core.DBC;

namespace WCell.RealmServer.NPCs
{
  public class DBCVehicleConverter : AdvancedDBCRecordConverter<VehicleEntry>
  {
    public override VehicleEntry ConvertTo(byte[] rawData, ref int id)
    {
      id = GetInt32(rawData, 0);
      VehicleEntry vehicleEntry = new VehicleEntry
      {
        Id = GetUInt32(rawData, 0),
        Flags = (VehicleFlags) GetUInt32(rawData, 1),
        TurnSpeed = GetFloat(rawData, 2),
        PitchSpeed = GetFloat(rawData, 3),
        PitchMin = GetFloat(rawData, 4),
        PitchMax = GetFloat(rawData, 5)
      };
      int num1 = 0;
      int num2 = 0;
      for(int index = 0; index < vehicleEntry.Seats.Length; ++index)
      {
        uint uint32 = GetUInt32(rawData, 6 + index);
        if(uint32 > 0U)
        {
          VehicleSeatEntry vehicleSeatEntry = NPCMgr.GetVehicleSeatEntry(uint32);
          vehicleEntry.Seats[index] = vehicleSeatEntry;
          ++num2;
          num1 = index;
        }
      }

      vehicleEntry.SeatCount = num2;
      if(num1 < 7)
        Array.Resize(ref vehicleEntry.Seats, num1 + 1);
      vehicleEntry.PowerType = (VehiclePowerType) GetInt32(rawData, 37);
      return vehicleEntry;
    }
  }
}