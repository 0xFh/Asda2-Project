using System;

namespace WCell.Constants.GameObjects
{
    [Flags]
    public enum GameObjectFlags
    {
        None = 0,
        InUse = 1,
        Locked = 2,
        ConditionalInteraction = 4,
        Transport = 8,
        GOFlag_0x10 = 16, // 0x00000010
        DoesNotDespawn = 32, // 0x00000020
        Triggered = 64, // 0x00000040
        GOFlag_0x80 = 128, // 0x00000080
        GOFlag_0x100 = 256, // 0x00000100
        Damaged = 512, // 0x00000200
        Destroyed = 1024, // 0x00000400
        GOFlag_0x800 = 2048, // 0x00000800
        GOFlag_0x1000 = 4096, // 0x00001000
        GOFlag_0x2000 = 8192, // 0x00002000
        GOFlag_0x4000 = 16384, // 0x00004000
        GOFlag_0x8000 = 32768, // 0x00008000
        Flag_0x10000 = 65536, // 0x00010000
        Flag_0x20000 = 131072, // 0x00020000
        Flag_0x40000 = 262144, // 0x00040000
    }
}