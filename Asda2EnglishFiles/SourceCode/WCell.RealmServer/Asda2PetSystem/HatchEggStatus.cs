namespace WCell.RealmServer.Asda2PetSystem
{
    public enum HatchEggStatus
    {
        Fail = 0,
        Ok = 1,
        YouAreNoLongerAllowedToUsePet = 2,
        InqubatorItemError = 3,
        NoEgg = 4,
        SuplimentError = 5,
        HatchingProbablilityFailed = 7,
        PetHatchingFailed = 9,
        LowLevel = 10, // 0x0000000A
    }
}