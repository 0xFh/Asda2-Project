namespace WCell.Constants.Updates
{
    /// <summary>
    /// UpdatePriority determines required frequency of Updates for an Action
    /// </summary>
    public enum UpdatePriority
    {
        Inactive,
        Background,
        VeryLowPriority,
        LowPriority,
        Active,
        HighPriority,
        End,
    }
}