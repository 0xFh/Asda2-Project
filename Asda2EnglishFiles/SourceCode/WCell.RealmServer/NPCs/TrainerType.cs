using System;

namespace WCell.RealmServer.NPCs
{
    [Serializable]
    public enum TrainerType : byte
    {
        Class = 0,
        Mounts = 1,
        Professions = 2,
        Pets = 3,
        NotATrainer = 255, // 0xFF
    }
}