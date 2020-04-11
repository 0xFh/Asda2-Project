namespace WCell.RealmServer.Handlers
{
    public enum SkillLearnStatus
    {
        Ok = 1,
        BadSpellLevel = 2,
        SpellLevelIsMaximum = 3,
        BadProffession = 4,
        JoblevelIsNotHighEnought = 4,
        NotEnoghtMoney = 5,
        NotEnoghtSpellPoints = 6,
        YouHaveSpendAllAlowedForThisJobSpellPoints = 7,
        LowLevel = 8,
        YourInventoryHasBenExpanded = 9,
        CCHasBeedRecharged = 10, // 0x0000000A
        CannontOpenStatusWindow = 11, // 0x0000000B
        Fail = 12, // 0x0000000C
    }
}