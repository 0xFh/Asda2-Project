﻿namespace WCell.Constants.Battlegrounds
{
    public enum ArathiBases
    {
        Farm = 0,
        Blacksmith = 1,
        Stables = 2,
        Lumbermill = 3,
        GoldMine = 4,
        End = 5
    }

    public enum ABSounds
    {
        NodeContested = 8192,
        NodeCapturedAlliance = 8173,
        NodeCapturedHorde = 8213,
        NodeAssaultedAlliance = 8212,
        NodeAssaultedHorde = 8174,
        NearVictory = 8456
    }

    public enum BaseState
    {
        Neutral = 0,
        CapturedAlliance = 1,
        CapturedHorde = 2,
        ContestedAlliance = 3,
        ContestedHorde = 4,
        End = 5,
    }
}
