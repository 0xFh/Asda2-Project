namespace WCell.RealmServer.Asda2PetSystem
{
    public enum PetSynethisResult
    {
        Ok = 1,
        AvalibleOnlyFor40LvlAndHigher = 2,
        AbnormalPetInfo = 3,
        LowPetLevel = 4,
        CantUseCurrentlySummonedPet = 5,
        SuplimentInfoAbnormal = 6,
        NotEnoghtSystSupl = 7,
        IncorrectSuplimentLevel = 8,
        RandomSuplientError = 9,
        NotEnoghtRandowmSupliment = 10, // 0x0000000A
    }
}