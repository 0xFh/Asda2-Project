namespace WCell.RealmServer.Handlers
{
    public enum UseSoulmateSkillStatus
    {
        Fail = 0,
        Ok = 1,
        NoFriend = 2,
        FriendNotInGame = 3,
        FriendNakazan = 4,
        FriendCantUseThisSkill = 5,
        CantUseOnThisMap = 6,
        TooFarFromFriend = 7,
        NotEnoughtMana = 8,
        WrongFriendGender = 9,
        ThisMonsterIsAlreadyDead = 11, // 0x0000000B
        CantUseWhileMove = 12, // 0x0000000C
        YouAreDead = 13, // 0x0000000D
        FriendDead = 14, // 0x0000000E
        Cooldown = 16, // 0x00000010
    }
}