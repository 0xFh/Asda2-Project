namespace WCell.Constants.NPCs
{
    /// <summary>Same a NPCFlags, but as a simple (not flag-)field</summary>
    public enum NPCEntryType
    {
        None = 0,
        Gossip = 1,
        QuestGiver = 2,
        Trainer = 4,
        ClassTrainer = 5,
        ProfessionTrainer = 6,
        Vendor = 7,
        GeneralGoodsVendor = 8,
        FoodVendor = 9,
        PoisonVendor = 10, // 0x0000000A
        ReagentVender = 11, // 0x0000000B
        Armorer = 12, // 0x0000000C
        TaxiVendor = 13, // 0x0000000D
        SpiritHealer = 14, // 0x0000000E
        SpiritGuide = 15, // 0x0000000F
        InnKeeper = 16, // 0x00000010
        Banker = 17, // 0x00000011
        Petitioner = 18, // 0x00000012
        TabardVendor = 19, // 0x00000013
        BattleMaster = 20, // 0x00000014
        Auctioneer = 21, // 0x00000015
        Stable = 22, // 0x00000016
        GuildBanker = 23, // 0x00000017
    }
}