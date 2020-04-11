namespace WCell.RealmServer.Handlers
{
    public enum SelectFactionStatus
    {
        Failed,
        Ok,
        YouAlreadyHaveFaction,
        AllowedOnlyFor2JobCharacters,
        FactionIsFull,
        AnotherFactionHasAlreadySelectThisBattleArea,
    }
}