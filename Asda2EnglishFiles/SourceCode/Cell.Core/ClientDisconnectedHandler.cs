namespace Cell.Core
{
    /// <summary>Handler used for client disconnected event</summary>
    /// <param name="client">The client connection</param>
    /// <param name="forced">Indicates if the client disconnection was forced</param>
    public delegate void ClientDisconnectedHandler(IClient client, bool forced);
}