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

        internal WCell_Core()
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
                if (object.ReferenceEquals((object) WCell_Core.resourceMan, (object) null))
                    WCell_Core.resourceMan =
                        new ResourceManager("WCell.Core.Localization.WCell.Core", typeof(WCell_Core).Assembly);
                return WCell_Core.resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get { return WCell_Core.resourceCulture; }
            set { WCell_Core.resourceCulture = value; }
        }

        /// <summary>
        ///   Looks up a localized string similar to Client attempting login sent AUTH_LOGON_CHALLENGE remaining length as {0}, however {1} bytes are remaining.
        /// </summary>
        internal static string Auth_Logon_with_invalid_length
        {
            get
            {
                return WCell_Core.ResourceManager.GetString("Auth Logon with invalid length",
                    WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Checking for {0} database.
        /// </summary>
        internal static string CheckingForDatabase
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(CheckingForDatabase), WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to ProtocolVersion: {0} ClientType: {1} Version: {2} Architecture: {3} OS: {4} Locale: {5} TimeZone: {6} IP: {7}.
        /// </summary>
        internal static string ClientInformationFourCCs
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(ClientInformationFourCCs),
                    WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Couldn't connect to database server.
        /// </summary>
        internal static string DatabaseConnectionFailure
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(DatabaseConnectionFailure),
                    WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to {0} database does not exist, creating.
        /// </summary>
        internal static string DatabaseDoesNotExist
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(DatabaseDoesNotExist), WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Failed to create the neccessary database!.
        /// </summary>
        internal static string DBCreateFailed
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(DBCreateFailed), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Couldn't load the DB script file!.
        /// </summary>
        internal static string DBScriptNotFound
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(DBScriptNotFound), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to A fatal, unhandled exception was encountered!.
        /// </summary>
        internal static string FatalUnhandledException
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(FatalUnhandledException),
                    WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The PacketHandler for Packet {0} '{1}' has been overridden with '{2}'!.
        /// </summary>
        internal static string HandlerAlreadyRegistered
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(HandlerAlreadyRegistered),
                    WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Initialization complete! {0} successful steps, {1} non-fatal failures..
        /// </summary>
        internal static string InitComplete
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(InitComplete), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Failed to fully process all required initialization steps!.
        /// </summary>
        internal static string InitFailed
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(InitFailed), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Initializing database.
        /// </summary>
        internal static string InitializingDatabase
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(InitializingDatabase), WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Initialization Pass #{0}.
        /// </summary>
        internal static string InitPass
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(InitPass), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Step '{0}' ({1}) failed to finish{2}.
        /// </summary>
        internal static string InitStepFailed
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(InitStepFailed), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Found and loaded {0} {1}!.
        /// </summary>
        internal static string InitStepsLoaded
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(InitStepsLoaded), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Step:  '{0}' ({1}).
        /// </summary>
        internal static string InitStepSucceeded
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(InitStepSucceeded), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Step '{0}' ({1}) was required to proceed; stopping!.
        /// </summary>
        internal static string InitStepWasRequired
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(InitStepWasRequired), WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Cannot create packet handler delegate from method '{0}': invalid method signature!.
        /// </summary>
        internal static string InvalidHandlerMethodSignature
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(InvalidHandlerMethodSignature),
                    WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to IO task pool experiencing slowdowns..
        /// </summary>
        internal static string IOPoolSlowdown
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(IOPoolSlowdown), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to The key '{0}' was not found in the configuration file!.
        /// </summary>
        internal static string KeyNotFound
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(KeyNotFound), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Manager: {0}, internal restart failed..
        /// </summary>
        internal static string ManagerInternalRestartFailed
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(ManagerInternalRestartFailed),
                    WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Manager: {0}, internal start failed..
        /// </summary>
        internal static string ManagerInternalStartFailed
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(ManagerInternalStartFailed),
                    WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Manager: {0}, internal stop failed..
        /// </summary>
        internal static string ManagerInternalStopFailed
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(ManagerInternalStopFailed),
                    WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Manager: {0}, has succesfully restarted..
        /// </summary>
        internal static string ManagerRestarted
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(ManagerRestarted), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Manager: {0}, restart failed..
        /// </summary>
        internal static string ManagerRestartFailed
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(ManagerRestartFailed), WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Restarting Manager: {0}.
        /// </summary>
        internal static string ManagerRestarting
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(ManagerRestarting), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Manager: {0}, has succesfully started..
        /// </summary>
        internal static string ManagerStarted
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(ManagerStarted), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Manager: {0}, start failed..
        /// </summary>
        internal static string ManagerStartFailed
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(ManagerStartFailed), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Starting Manager: {0}.
        /// </summary>
        internal static string ManagerStarting
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(ManagerStarting), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Manager: {0}, stop failed..
        /// </summary>
        internal static string ManagerStopFailed
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(ManagerStopFailed), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Manager: {0}, has succesfully stopped..
        /// </summary>
        internal static string ManagerStopped
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(ManagerStopped), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Stopping Manager: {0}.
        /// </summary>
        internal static string ManagerStopping
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(ManagerStopping), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Manager: {0}, has thrown an error: {1}.
        /// </summary>
        internal static string ManagerThrownError
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(ManagerThrownError), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Missing database schema file, ensure you have a {0} file in your server folder.
        /// </summary>
        internal static string MissingSqlScript
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(MissingSqlScript), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Network task pool experiencing slowdowns..
        /// </summary>
        internal static string NetworkPoolSlowdown
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(NetworkPoolSlowdown), WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to GetResourceStream returned a null stream (file not found).
        /// </summary>
        internal static string NullStream
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(NullStream), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Performing next step: '{0}'.
        /// </summary>
        internal static string PerformingNextInitStep
        {
            get
            {
                return WCell_Core.ResourceManager.GetString(nameof(PerformingNextInitStep), WCell_Core.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Server has been shutdown..
        /// </summary>
        internal static string ProcessExited
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(ProcessExited), WCell_Core.resourceCulture); }
        }

        /// <summary>Looks up a localized string similar to Running {0}.</summary>
        internal static string RunningSqlScript
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(RunningSqlScript), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Starting server....
        /// </summary>
        internal static string ServerStarting
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(ServerStarting), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Stopping server....
        /// </summary>
        internal static string ServerStopping
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(ServerStopping), WCell_Core.resourceCulture); }
        }

        /// <summary>
        ///   Looks up a localized string similar to Unhandled packet {0} ({1}), Size: {2} bytes.
        /// </summary>
        internal static string UnhandledPacket
        {
            get { return WCell_Core.ResourceManager.GetString(nameof(UnhandledPacket), WCell_Core.resourceCulture); }
        }
    }
}