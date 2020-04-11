namespace WCell.Core.Timers
{
    /// <summary>
    /// Defines the interface of an object that can be updated with respect to time.
    /// </summary>
    public interface IUpdatable
    {
        /// <summary>Updates the object.</summary>
        /// <param name="dt">the time since the last update in millis</param>
        void Update(int dt);
    }
}