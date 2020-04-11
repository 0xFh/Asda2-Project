namespace WCell.RealmServer.AI.Actions
{
    /// <summary>
    /// All possible outcomes of AI Action.
    /// Some actions will never succeed (eg following a moving Target),
    /// some cannot even be executed (eg follow Master without a Master)
    /// </summary>
    public enum AIActionResult
    {
        Success,
        Executing,
        Failure,
    }
}