using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace WCell.Core.Localization
{
  /// <summary>
  ///   A strongly-typed resource class, for looking up localized strings, etc.
  /// </summary>
  [DebuggerNonUserCode]
  [CompilerGenerated]
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  internal class WCell_Core
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
            new ResourceManager("WCell.Core.Localization.WCell.Core", typeof(WCell_Core).Assembly);
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
    ///   Looks up a localized string similar to Client attempting login sent AUTH_LOGON_CHALLENGE remaining length as {0}, however {1} bytes are remaining.
    /// </summary>
    internal static string Auth_Logon_with_invalid_length
    {
      get
      {
        return ResourceManager.GetString("Auth Logon with invalid length",
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Checking for {0} database.
    /// </summary>
    internal static string CheckingForDatabase
    {
      get { return ResourceManager.GetString(nameof(CheckingForDatabase), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to ProtocolVersion: {0} ClientType: {1} Version: {2} Architecture: {3} OS: {4} Locale: {5} TimeZone: {6} IP: {7}.
    /// </summary>
    internal static string ClientInformationFourCCs
    {
      get
      {
        return ResourceManager.GetString(nameof(ClientInformationFourCCs),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Couldn't connect to database server.
    /// </summary>
    internal static string DatabaseConnectionFailure
    {
      get
      {
        return ResourceManager.GetString(nameof(DatabaseConnectionFailure),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to {0} database does not exist, creating.
    /// </summary>
    internal static string DatabaseDoesNotExist
    {
      get { return ResourceManager.GetString(nameof(DatabaseDoesNotExist), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Failed to create the neccessary database!.
    /// </summary>
    internal static string DBCreateFailed
    {
      get { return ResourceManager.GetString(nameof(DBCreateFailed), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Couldn't load the DB script file!.
    /// </summary>
    internal static string DBScriptNotFound
    {
      get { return ResourceManager.GetString(nameof(DBScriptNotFound), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to A fatal, unhandled exception was encountered!.
    /// </summary>
    internal static string FatalUnhandledException
    {
      get
      {
        return ResourceManager.GetString(nameof(FatalUnhandledException),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to The PacketHandler for Packet {0} '{1}' has been overridden with '{2}'!.
    /// </summary>
    internal static string HandlerAlreadyRegistered
    {
      get
      {
        return ResourceManager.GetString(nameof(HandlerAlreadyRegistered),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Initialization complete! {0} successful steps, {1} non-fatal failures..
    /// </summary>
    internal static string InitComplete
    {
      get { return ResourceManager.GetString(nameof(InitComplete), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Failed to fully process all required initialization steps!.
    /// </summary>
    internal static string InitFailed
    {
      get { return ResourceManager.GetString(nameof(InitFailed), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Initializing database.
    /// </summary>
    internal static string InitializingDatabase
    {
      get { return ResourceManager.GetString(nameof(InitializingDatabase), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Initialization Pass #{0}.
    /// </summary>
    internal static string InitPass
    {
      get { return ResourceManager.GetString(nameof(InitPass), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Step '{0}' ({1}) failed to finish{2}.
    /// </summary>
    internal static string InitStepFailed
    {
      get { return ResourceManager.GetString(nameof(InitStepFailed), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Found and loaded {0} {1}!.
    /// </summary>
    internal static string InitStepsLoaded
    {
      get { return ResourceManager.GetString(nameof(InitStepsLoaded), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Step:  '{0}' ({1}).
    /// </summary>
    internal static string InitStepSucceeded
    {
      get { return ResourceManager.GetString(nameof(InitStepSucceeded), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Step '{0}' ({1}) was required to proceed; stopping!.
    /// </summary>
    internal static string InitStepWasRequired
    {
      get { return ResourceManager.GetString(nameof(InitStepWasRequired), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Cannot create packet handler delegate from method '{0}': invalid method signature!.
    /// </summary>
    internal static string InvalidHandlerMethodSignature
    {
      get
      {
        return ResourceManager.GetString(nameof(InvalidHandlerMethodSignature),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to IO task pool experiencing slowdowns..
    /// </summary>
    internal static string IOPoolSlowdown
    {
      get { return ResourceManager.GetString(nameof(IOPoolSlowdown), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to The key '{0}' was not found in the configuration file!.
    /// </summary>
    internal static string KeyNotFound
    {
      get { return ResourceManager.GetString(nameof(KeyNotFound), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Manager: {0}, internal restart failed..
    /// </summary>
    internal static string ManagerInternalRestartFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(ManagerInternalRestartFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Manager: {0}, internal start failed..
    /// </summary>
    internal static string ManagerInternalStartFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(ManagerInternalStartFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Manager: {0}, internal stop failed..
    /// </summary>
    internal static string ManagerInternalStopFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(ManagerInternalStopFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Manager: {0}, has succesfully restarted..
    /// </summary>
    internal static string ManagerRestarted
    {
      get { return ResourceManager.GetString(nameof(ManagerRestarted), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Manager: {0}, restart failed..
    /// </summary>
    internal static string ManagerRestartFailed
    {
      get { return ResourceManager.GetString(nameof(ManagerRestartFailed), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Restarting Manager: {0}.
    /// </summary>
    internal static string ManagerRestarting
    {
      get { return ResourceManager.GetString(nameof(ManagerRestarting), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Manager: {0}, has succesfully started..
    /// </summary>
    internal static string ManagerStarted
    {
      get { return ResourceManager.GetString(nameof(ManagerStarted), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Manager: {0}, start failed..
    /// </summary>
    internal static string ManagerStartFailed
    {
      get { return ResourceManager.GetString(nameof(ManagerStartFailed), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Starting Manager: {0}.
    /// </summary>
    internal static string ManagerStarting
    {
      get { return ResourceManager.GetString(nameof(ManagerStarting), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Manager: {0}, stop failed..
    /// </summary>
    internal static string ManagerStopFailed
    {
      get { return ResourceManager.GetString(nameof(ManagerStopFailed), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Manager: {0}, has succesfully stopped..
    /// </summary>
    internal static string ManagerStopped
    {
      get { return ResourceManager.GetString(nameof(ManagerStopped), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Stopping Manager: {0}.
    /// </summary>
    internal static string ManagerStopping
    {
      get { return ResourceManager.GetString(nameof(ManagerStopping), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Manager: {0}, has thrown an error: {1}.
    /// </summary>
    internal static string ManagerThrownError
    {
      get { return ResourceManager.GetString(nameof(ManagerThrownError), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Missing database schema file, ensure you have a {0} file in your server folder.
    /// </summary>
    internal static string MissingSqlScript
    {
      get { return ResourceManager.GetString(nameof(MissingSqlScript), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Network task pool experiencing slowdowns..
    /// </summary>
    internal static string NetworkPoolSlowdown
    {
      get { return ResourceManager.GetString(nameof(NetworkPoolSlowdown), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to GetResourceStream returned a null stream (file not found).
    /// </summary>
    internal static string NullStream
    {
      get { return ResourceManager.GetString(nameof(NullStream), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Performing next step: '{0}'.
    /// </summary>
    internal static string PerformingNextInitStep
    {
      get { return ResourceManager.GetString(nameof(PerformingNextInitStep), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Server has been shutdown..
    /// </summary>
    internal static string ProcessExited
    {
      get { return ResourceManager.GetString(nameof(ProcessExited), resourceCulture); }
    }

    /// <summary>Looks up a localized string similar to Running {0}.</summary>
    internal static string RunningSqlScript
    {
      get { return ResourceManager.GetString(nameof(RunningSqlScript), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Starting server....
    /// </summary>
    internal static string ServerStarting
    {
      get { return ResourceManager.GetString(nameof(ServerStarting), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Stopping server....
    /// </summary>
    internal static string ServerStopping
    {
      get { return ResourceManager.GetString(nameof(ServerStopping), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unhandled packet {0} ({1}), Size: {2} bytes.
    /// </summary>
    internal static string UnhandledPacket
    {
      get { return ResourceManager.GetString(nameof(UnhandledPacket), resourceCulture); }
    }
  }
}