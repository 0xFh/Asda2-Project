namespace WCell.RealmServer.Handlers
{
    public enum PartyInviteStatusResponse
    {
        TheInvitionRequestHasBeenDeclined,
        TheInvitionRequestHasBeenAccepted,
        ThereIsNoOneToInvite,
        TheInvitionTimeHasPassed,
        YouAreAlreadyInParty,
        SomeOneRevicingYourInvation,
        SomeoneIsInvitingYou,
    }
}