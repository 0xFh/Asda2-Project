using System;

namespace WCell.RealmServer.AI
{
    [Serializable]
    public enum BrainState
    {
        Idle,
        Roam,
        Combat,
        Evade,
        Follow,
        Guard,
        FormationMove,
        PatrolCircle,
        GmMove,
        DefenceTownEventMove,
        Fear,
        Dead,
        End,
    }
}