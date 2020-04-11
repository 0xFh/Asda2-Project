using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Cell.Core.Localization
{
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [DebuggerNonUserCode]
    [CompilerGenerated]
    internal class Cell_Core
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        internal Cell_Core()
        {
        }

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals((object) Cell_Core.resourceMan, (object) null))
                    Cell_Core.resourceMan =
                        new ResourceManager("Cell.Core.Localization.Cell.Core", typeof(Cell_Core).Assembly);
                return Cell_Core.resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get { return Cell_Core.resourceCulture; }
            set { Cell_Core.resourceCulture = value; }
        }

        /// <summary>
        ///   Looks up a localized string similar to Starting the network layer!.
        /// </summary>
        internal static string BaseStart
        {
            get { return Cell_Core.ResourceManager.GetString(nameof(BaseStart), Cell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Stopping the network layer!.
        /// </summary>
        internal static string BaseStop
        {
            get { return Cell_Core.ResourceManager.GetString(nameof(BaseStop), Cell_Core.resourceCulture); }
        }

        /// <summary>Looks up a localized string similar to Connected.</summary>
        internal static string ClientConnected
        {
            get { return Cell_Core.ResourceManager.GetString(nameof(ClientConnected), Cell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Disconnected.
        /// </summary>
        internal static string ClientDisconnected
        {
            get { return Cell_Core.ResourceManager.GetString(nameof(ClientDisconnected), Cell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Encountered a fatal error while trying to accept a connection. You might have to restart the server..
        /// </summary>
        internal static string FatalAsyncAccept
        {
            get { return Cell_Core.ResourceManager.GetString(nameof(FatalAsyncAccept), Cell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Listening endpoint is invalid! Check your configuration and make sure the address and port are valid! Endpoint: {0}.
        /// </summary>
        internal static string InvalidEndpoint
        {
            get { return Cell_Core.ResourceManager.GetString(nameof(InvalidEndpoint), Cell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Listening to TCP socket on {0}.
        /// </summary>
        internal static string ListeningTCPSocket
        {
            get { return Cell_Core.ResourceManager.GetString(nameof(ListeningTCPSocket), Cell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to TCP Listen socket for port {0} closed.
        /// </summary>
        internal static string ListeningTCPSocketStopped
        {
            get
            {
                return Cell_Core.ResourceManager.GetString(nameof(ListeningTCPSocketStopped),
                    Cell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Listening to UDP socket on {0}.
        /// </summary>
        internal static string ListeningUDPSocket
        {
            get { return Cell_Core.ResourceManager.GetString(nameof(ListeningUDPSocket), Cell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to No network adapters are available on the system!.
        /// </summary>
        internal static string NoNetworkAdapters
        {
            get { return Cell_Core.ResourceManager.GetString(nameof(NoNetworkAdapters), Cell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Path cannot be be null or empty.
        /// </summary>
        internal static string PathCannotBeNull
        {
            get { return Cell_Core.ResourceManager.GetString(nameof(PathCannotBeNull), Cell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Server is ready for connections!.
        /// </summary>
        internal static string ReadyForConnections
        {
            get { return Cell_Core.ResourceManager.GetString(nameof(ReadyForConnections), Cell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Server is no longer accepting connections!.
        /// </summary>
        internal static string ServerNotRunning
        {
            get { return Cell_Core.ResourceManager.GetString(nameof(ServerNotRunning), Cell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Encountered a socket exception while trying to accept a connection!.
        /// </summary>
        internal static string SocketExceptionAsyncAccept
        {
            get
            {
                return Cell_Core.ResourceManager.GetString(nameof(SocketExceptionAsyncAccept),
                    Cell_Core.resourceCulture);
            }
        }
    }
}