namespace WCell.Constants.Updates
{
    /// <summary>
    /// Update Types used in SMSG_UPDATE_OBJECT inside realm server
    /// </summary>
    public enum UpdateType : byte
    {
        Values,
        Movement,
        Create,
        CreateSelf,
        OutOfRange,
        Near,
    }
}