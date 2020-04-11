using System;
using WCell.Constants.NPCs;

namespace WCell.RealmServer.NPCs
{
    [Serializable]
    public class VehicleEntry
    {
        public VehicleSeatEntry[] Seats = new VehicleSeatEntry[8];

        /// <summary>This is *NOT* the EntryId of the NPCEntry</summary>
        public uint Id;

        /// <summary>flag, position 1</summary>
        public VehicleFlags Flags;

        /// <summary>turn speed, position 2</summary>
        public float TurnSpeed;

        /// <summary>pitchspeed, position 3</summary>
        public float PitchSpeed;

        public float PitchMin;
        public float PitchMax;
        public float MouseLookOffsetPitch;
        public float CameraFadeDistScalarMin;
        public float CameraFadeDistScalarMax;
        public float CameraPitchOffset;
        public float FacingLimitRight;
        public float FacingLimitLeft;
        public float TurnLingering;
        public float PitchLingering;
        public float MouseLingering;
        public float EndOpacity;
        public float ArcSpeed;
        public float ArcRepeat;
        public float ArcWidth;
        public float[] ImpactRadius;
        public VehiclePowerType PowerType;
        public int SeatCount;
        public bool IsMinion;
    }
}