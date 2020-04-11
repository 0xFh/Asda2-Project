using System;

namespace WCell.Constants.NPCs
{
    [Serializable]
    public enum AIMotionGenerationType
    {
        IdleMotion,
        RandomMotion,
        WaypointMotion,
        MaxDBMotion,
        ConfusedMotion,
        ChaseMotion,
        HomeMotion,
        FlightMotion,
        PointMotion,
        FleeingMotion,
        DistractMotion,
        AssistanceMotion,
        AssistanceDistractMotion,
        TimedFleeingMotion,
        FollowMotion,
    }
}