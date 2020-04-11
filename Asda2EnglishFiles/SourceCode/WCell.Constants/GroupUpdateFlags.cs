using System;

namespace WCell.Constants
{
    /// <summary>
    /// Group Update flags used when sending group member stats
    /// </summary>
    [Flags]
    public enum GroupUpdateFlags : uint
    {
        None = 0,
        Status = 1,
        Health = 2,
        MaxHealth = 4,
        PowerType = 8,
        Power = 16, // 0x00000010
        MaxPower = 32, // 0x00000020
        Level = 64, // 0x00000040
        ZoneId = 128, // 0x00000080
        Position = 256, // 0x00000100
        Auras = 512, // 0x00000200
        PetGuid = 1024, // 0x00000400
        PetName = 2048, // 0x00000800
        PetDisplayId = 4096, // 0x00001000
        PetHealth = 8192, // 0x00002000
        PetMaxHealth = 16384, // 0x00004000
        PetPowerType = 32768, // 0x00008000
        PetPower = 65536, // 0x00010000
        PetMaxPower = 131072, // 0x00020000
        PetAuras = 262144, // 0x00040000
        Vehicle = 524288, // 0x00080000
        Unused2 = 1048576, // 0x00100000
        Unused3 = 2097152, // 0x00200000
        Unused4 = 4194304, // 0x00400000
        Unused5 = 8388608, // 0x00800000
        Unused6 = 16777216, // 0x01000000
        Unused7 = 33554432, // 0x02000000
        Unused8 = 67108864, // 0x04000000
        Unused9 = 134217728, // 0x08000000
        Unused10 = 268435456, // 0x10000000
        Unused11 = 536870912, // 0x20000000
        Unused12 = 1073741824, // 0x40000000
        Unused13 = 2147483648, // 0x80000000

        UpdatePlayer =
            Auras | Position | ZoneId | Level | MaxPower | Power | PowerType | MaxHealth | Health |
            Status, // 0x000003FF

        UpdatePet = PetAuras | PetMaxPower | PetPower | PetPowerType | PetMaxHealth | PetHealth | PetDisplayId |
                    PetName | PetGuid, // 0x0007FC00
        UpdateFull = UpdatePet | UpdatePlayer, // 0x0007FFFF
    }
}