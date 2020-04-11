using System;
using WCell.Constants.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs
{
    [Serializable]
    public class VehicleSeatEntry
    {
        public uint Id;
        public VehicleSeatFlags Flags;
        public int AttachmentId;
        public Vector3 AttachmentOffset;
        public float EnterPreDelay;
        public float EnterSpeed;
        public float EnterGravity;
        public float EnterMinDuration;
        public float EnterMaxDuration;
        public float EnterMinArcHeight;
        public float EnterMaxArcHeight;
        public int EnterAnimStart;
        public int EnterAnimLoop;
        public int RideAnimStart;
        public int RideAnimLoop;
        public int RideUpperAnimStart;
        public int RideUpperAnimLoop;
        public float ExitPreDelay;
        public float ExitSpeed;
        public float ExitGravity;
        public float ExitMinDuration;
        public float ExitMaxDuration;
        public float ExitMinArcHeight;
        public float ExitMaxArcHeight;
        public int ExitAnimStart;
        public int ExitAnimLoop;
        public int ExitAnimEnd;
        public float PassengerYaw;
        public float PassengerPitch;
        public float PassengerRoll;
        public int PassengerAttachmentId;
        public int VehicleEnterAnim;
        public int VehicleExitAnim;
        public int VehicleRideAnimLoop;
        public int VehicleEnterAnimBone;
        public int VehicleExitAnimBone;
        public int VehicleRideAnimLoopBone;
        public float VehicleEnterAnimDelay;
        public float VehicleExitAnimDelay;
        public uint VehicleAbilityDisplay;
        public uint EnterUISoundId;
        public uint ExitUISoundId;
        public int SkinId;
        public VehicleSeatFlagsB FlagsB;
        public uint PassengerNPCId;
    }
}