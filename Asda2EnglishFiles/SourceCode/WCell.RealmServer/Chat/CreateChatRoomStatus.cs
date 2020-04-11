namespace WCell.RealmServer.Chat
{
    public enum CreateChatRoomStatus
    {
        Error,
        Ok,
        UnableToOpen,
        UnableOpenOnBattle,
        YouAreAlreadyInChatRoom,
        CapacityError,
        SetPassword,
        YouCanOnlyOpenChatRoomInTown,
        YouCantOpenChatRoomWhileInHideMode,
    }
}