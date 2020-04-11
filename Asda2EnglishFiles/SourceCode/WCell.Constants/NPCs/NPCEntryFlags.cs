using System;

namespace WCell.Constants.NPCs
{
    [Flags]
    public enum NPCEntryFlags : uint
    {
        Tamable = 1,
        SpiritHealer = 2,
        CanGatherHerbs = 256, // 0x00000100
        CanMine = 512, // 0x00000200
        NPCFlag0x400 = 1024, // 0x00000400
        ExoticCreature = 65536, // 0x00010000
        Flag_0x4000 = ExoticCreature | Tamable, // 0x00010001
        CanSalvage = 32768, // 0x00008000
        CanWalk = 262144, // 0x00040000
        CanSwim = 268435456, // 0x10000000
    }
}