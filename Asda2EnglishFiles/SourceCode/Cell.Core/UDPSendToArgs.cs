using System.Net;

namespace Cell.Core
{
    /// <summary>
    /// Container class for the server object and the client IP.
    /// </summary>
    public class UDPSendToArgs
    {
        private ServerBase _server;
        private IPEndPoint _client;

        /// <summary>The server object receiving the UDP communications.</summary>
        public ServerBase Server
        {
            get { return this._server; }
        }

        /// <summary>The IP address the data was received from.</summary>
        public IPEndPoint ClientIP
        {
            get { return this._client; }
        }

        /// <summary>Default constructor.</summary>
        /// <param name="srvr">The server object receiving the UP communications.</param>
        /// <param name="client">The IP address the data was received from.</param>
        public UDPSendToArgs(ServerBase srvr, IPEndPoint client)
        {
            this._server = srvr;
            this._client = client;
        }
    }
}