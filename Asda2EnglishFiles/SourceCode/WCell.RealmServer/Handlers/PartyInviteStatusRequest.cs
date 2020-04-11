namespace WCell.RealmServer.Handlers
{
    public enum PartyInviteStatusRequest
    {
        Invite = 0,
        Invited = 1,
        AlreadyBelongToAParty = 2,
        AlreadyLogout = 3,
        YouCantInvite2OrMorePeopleAtOneTime = 4,
        ADifferentPersonInvitedAtThisTime = 5,
        YouAlreadyInGroup = 8,
        Dicline = 9,
        TargetAlreadyInGroup = 10, // 0x0000000A
        YouCantInviteOtherFactionToAGroup = 11, // 0x0000000B
    }
}