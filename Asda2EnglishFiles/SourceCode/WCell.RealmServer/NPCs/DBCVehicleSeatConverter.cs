using WCell.Constants.NPCs;
using WCell.Core.DBC;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs
{
    public class DBCVehicleSeatConverter : AdvancedDBCRecordConverter<VehicleSeatEntry>
    {
        public override VehicleSeatEntry ConvertTo(byte[] rawData, ref int id)
        {
            id = DBCRecordConverter.GetInt32(rawData, 0);
            return new VehicleSeatEntry()
            {
                Id = DBCRecordConverter.GetUInt32(rawData, 0),
                Flags = (VehicleSeatFlags) DBCRecordConverter.GetUInt32(rawData, 1),
                AttachmentId = DBCRecordConverter.GetInt32(rawData, 2),
                AttachmentOffset = new Vector3(DBCRecordConverter.GetFloat(rawData, 3),
                    DBCRecordConverter.GetFloat(rawData, 4), DBCRecordConverter.GetFloat(rawData, 5)),
                EnterPreDelay = DBCRecordConverter.GetFloat(rawData, 6),
                EnterSpeed = DBCRecordConverter.GetFloat(rawData, 7),
                EnterGravity = DBCRecordConverter.GetFloat(rawData, 8),
                EnterMinDuration = DBCRecordConverter.GetFloat(rawData, 9),
                EnterMaxDuration = DBCRecordConverter.GetFloat(rawData, 10),
                EnterMinArcHeight = DBCRecordConverter.GetFloat(rawData, 11),
                EnterMaxArcHeight = DBCRecordConverter.GetFloat(rawData, 12),
                EnterAnimStart = DBCRecordConverter.GetInt32(rawData, 13),
                EnterAnimLoop = DBCRecordConverter.GetInt32(rawData, 14),
                RideAnimStart = DBCRecordConverter.GetInt32(rawData, 15),
                RideAnimLoop = DBCRecordConverter.GetInt32(rawData, 16),
                RideUpperAnimStart = DBCRecordConverter.GetInt32(rawData, 17),
                RideUpperAnimLoop = DBCRecordConverter.GetInt32(rawData, 18),
                ExitPreDelay = DBCRecordConverter.GetFloat(rawData, 19),
                ExitSpeed = DBCRecordConverter.GetFloat(rawData, 20),
                ExitGravity = DBCRecordConverter.GetFloat(rawData, 21),
                ExitMinDuration = DBCRecordConverter.GetFloat(rawData, 22),
                ExitMaxDuration = DBCRecordConverter.GetFloat(rawData, 23),
                ExitMinArcHeight = DBCRecordConverter.GetFloat(rawData, 24),
                ExitMaxArcHeight = DBCRecordConverter.GetFloat(rawData, 25),
                ExitAnimStart = DBCRecordConverter.GetInt32(rawData, 26),
                ExitAnimLoop = DBCRecordConverter.GetInt32(rawData, 27),
                ExitAnimEnd = DBCRecordConverter.GetInt32(rawData, 28),
                PassengerYaw = DBCRecordConverter.GetFloat(rawData, 29),
                PassengerPitch = DBCRecordConverter.GetFloat(rawData, 30),
                PassengerRoll = DBCRecordConverter.GetFloat(rawData, 31),
                PassengerAttachmentId = DBCRecordConverter.GetInt32(rawData, 32),
                FlagsB = (VehicleSeatFlagsB) DBCRecordConverter.GetUInt32(rawData, 45)
            };
        }
    }
}