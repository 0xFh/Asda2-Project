using System;

namespace WCell.Constants.NPCs
{
    /// <summary>NPC Type Flags</summary>
    [Flags]
    public enum NPCFlags
    {
        None = 0,
        Gossip = 1,
        QuestGiver = 2,
        Flag_0x4 = 4,
        Flag_0x8 = 8,
        UnkTrainer = 16, // 0x00000010
        ClassTrainer = 32, // 0x00000020
        ProfessionTrainer = 64, // 0x00000040
        AnyTrainer = ProfessionTrainer | ClassTrainer | UnkTrainer, // 0x00000070
        Vendor = 128, // 0x00000080
        GeneralGoodsVendor = 256, // 0x00000100
        FoodVendor = 512, // 0x00000200
        PoisonVendor = 1024, // 0x00000400
        ReagentVendor = 2048, // 0x00000800
        Armorer = 4096, // 0x00001000
        AnyVendor = Armorer | PoisonVendor | FoodVendor | GeneralGoodsVendor | Vendor, // 0x00001780
        FlightMaster = 8192, // 0x00002000
        SpiritHealer = 16384, // 0x00004000
        SpiritGuide = 32768, // 0x00008000
        InnKeeper = 65536, // 0x00010000
        Banker = 131072, // 0x00020000
        Petitioner = 262144, // 0x00040000
        TabardDesigner = 524288, // 0x00080000
        BattleMaster = 1048576, // 0x00100000
        Auctioneer = 2097152, // 0x00200000
        StableMaster = 4194304, // 0x00400000
        GuildBanker = 8388608, // 0x00800000
        SpellClick = 16777216, // 0x01000000
    }
}