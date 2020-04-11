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

    /// <summary>
    ///   Returns the cached ResourceManager instance used by this class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if(ReferenceEquals(resourceMan, null))
          resourceMan =
            new ResourceManager("Cell.Core.Localization.Cell.Core", typeof(Cell_Core).Assembly);
        return resourceMan;
      }
    }

    /// <summary>
    ///   Overrides the current thread's CurrentUICulture property for all
    ///   resource lookups using this strongly typed resource class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get { return resourceCulture; }
      set { resourceCulture = value; }
    }

    /// <summary>
    ///   Looks up a localized string similar to Starting the network layer!.
    /// </summary>
    internal static string BaseStart
    {
      get { return ResourceManager.GetString(nameof(BaseStart), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Stopping the network layer!.
    /// </summary>
    internal static string BaseStop
    {
      get { return ResourceManager.GetString(nameof(BaseStop), resourceCulture); }
    }

    /// <summary>Looks up a localized string similar to Connected.</summary>
    internal static string ClientConnected
    {
      get { return ResourceManager.GetString(nameof(ClientConnected), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Disconnected.
    /// </summary>
    internal static string ClientDisconnected
    {
      get { return ResourceManager.GetString(nameof(ClientDisconnected), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Encountered a fatal error while trying to accept a connection. You might have to restart the server..
    /// </summary>
    internal static string FatalAsyncAccept
    {
      get { return ResourceManager.GetString(nameof(FatalAsyncAccept), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Listening endpoint is invalid! Check your configuration and make sure the address and port are valid! Endpoint: {0}.
    /// </summary>
    internal static string InvalidEndpoint
    {
      get { return ResourceManager.GetString(nameof(InvalidEndpoint), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Listening to TCP socket on {0}.
    /// </summary>
    internal static string ListeningTCPSocket
    {
      get { return ResourceManager.GetString(nameof(ListeningTCPSocket), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to TCP Listen socket for port {0} closed.
    /// </summary>
    internal static string ListeningTCPSocketStopped
    {
      get
      {
        return ResourceManager.GetString(nameof(ListeningTCPSocketStopped),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Listening to UDP socket on {0}.
    /// </summary>
    internal static string ListeningUDPSocket
    {
      get { return ResourceManager.GetString(nameof(ListeningUDPSocket), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to No network adapters are available on the system!.
    /// </summary>
    internal static string NoNetworkAdapters
    {
      get { return ResourceManager.GetString(nameof(NoNetworkAdapters), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Path cannot be be null or empty.
    /// </summary>
    internal static string PathCannotBeNull
    {
      get { return ResourceManager.GetString(nameof(PathCannotBeNull), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Server is ready for connections!.
    /// </summary>
    internal static string ReadyForConnections
    {
      get { return ResourceManager.GetString(nameof(ReadyForConnections), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Server is no longer accepting connections!.
    /// </summary>
    internal static string ServerNotRunning
    {
      get { return ResourceManager.GetString(nameof(ServerNotRunning), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Encountered a socket exception while trying to accept a connection!.
    /// </summary>
    internal static string SocketExceptionAsyncAccept
    {
      get
      {
        return ResourceManager.GetString(nameof(SocketExceptionAsyncAccept),
          resourceCulture);
      }
    }
  }
}