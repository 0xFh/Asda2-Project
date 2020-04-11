namespace WCell.RealmServer.Global
{
    /// <summary>Defines the interface of a message.</summary>
    public interface IRealmMessage
    {
        /// <summary>
        /// Indicates where the message is valid.
        /// Mapal messages must be disposed when object is moved to a different Map.
        /// </summary>
        RealmMessageBoundary Boundary { get; }

        /// <summary>Executes the message.</summary>
        void Execute();
    }
}