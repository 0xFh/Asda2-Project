using WCell.Constants.NPCs;
using WCell.Core.DBC;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs
{
  public class DBCVehicleSeatConverter : AdvancedDBCRecordConverter<VehicleSeatEntry>
  {
    public override VehicleSeatEntry ConvertTo(byte[] rawData, ref int id)
    {
      id = GetInt32(rawData, 0);
      return new VehicleSeatEntry
      {
        Id = GetUInt32(rawData, 0),
        Flags = (VehicleSeatFlags) GetUInt32(rawData, 1),
        AttachmentId = GetInt32(rawData, 2),
        AttachmentOffset = new Vector3(GetFloat(rawData, 3),
          GetFloat(rawData, 4), GetFloat(rawData, 5)),
        EnterPreDelay = GetFloat(rawData, 6),
        EnterSpeed = GetFloat(rawData, 7),
        EnterGravity = GetFloat(rawData, 8),
        EnterMinDuration = GetFloat(rawData, 9),
        EnterMaxDuration = GetFloat(rawData, 10),
        EnterMinArcHeight = GetFloat(rawData, 11),
        EnterMaxArcHeight = GetFloat(rawData, 12),
        EnterAnimStart = GetInt32(rawData, 13),
        EnterAnimLoop = GetInt32(rawData, 14),
        RideAnimStart = GetInt32(rawData, 15),
        RideAnimLoop = GetInt32(rawData, 16),
        RideUpperAnimStart = GetInt32(rawData, 17),
        RideUpperAnimLoop = GetInt32(rawData, 18),
        ExitPreDelay = GetFloat(rawData, 19),
        ExitSpeed = GetFloat(rawData, 20),
        ExitGravity = GetFloat(rawData, 21),
        ExitMinDuration = GetFloat(rawData, 22),
        ExitMaxDuration = GetFloat(rawData, 23),
        ExitMinArcHeight = GetFloat(rawData, 24),
        ExitMaxArcHeight = GetFloat(rawData, 25),
        ExitAnimStart = GetInt32(rawData, 26),
        ExitAnimLoop = GetInt32(rawData, 27),
        ExitAnimEnd = GetInt32(rawData, 28),
        PassengerYaw = GetFloat(rawData, 29),
        PassengerPitch = GetFloat(rawData, 30),
        PassengerRoll = GetFloat(rawData, 31),
        PassengerAttachmentId = GetInt32(rawData, 32),
        FlagsB = (VehicleSeatFlagsB) GetUInt32(rawData, 45)
      };
    }
  }
}