using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace WCell.RealmServer.Res
{
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [DebuggerNonUserCode]
    [CompilerGenerated]
    internal class WCell_RealmServer
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        internal WCell_RealmServer()
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
                if (object.ReferenceEquals((object) WCell_RealmServer.resourceMan, (object) null))
                    WCell_RealmServer.resourceMan = new ResourceManager("WCell.RealmServer.Res.WCell.RealmServer",
                        typeof(WCell_RealmServer).Assembly);
                return WCell_RealmServer.resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get { return WCell_RealmServer.resourceCulture; }
            set { WCell_RealmServer.resourceCulture = value; }
        }

        /// <summary>
        ///   Looks up a localized string similar to An attempt was made to recycle an EntityId that is already recycled! EntityId: {0} EntityIdType:{1}.
        /// </summary>
        internal static string AlreadyRecycledEntityId
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(AlreadyRecycledEntityId),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Tried to authenticate the client before their account was loaded/initialized!.
        /// </summary>
        internal static string BadAuthenticationProcedure
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(BadAuthenticationProcedure),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Character EntityId {0} Account {1} tried to connect while already logged in.
        /// </summary>
        internal static string CharacterAlreadyConnected
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CharacterAlreadyConnected),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Character EntityId {0} not found in Account {1}.
        /// </summary>
        internal static string CharacterNotFound
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CharacterNotFound),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Character null, could not register.
        /// </summary>
        internal static string CharacterNullRegister
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CharacterNullRegister),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Character null, could not unregister.
        /// </summary>
        internal static string CharacterNullUnregister
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CharacterNullUnregister),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Relation added successfully: Character='{0}'({1}), RelatedCharacter='{2}'({3}), RelationType={4}, RelationResult={5}.
        /// </summary>
        internal static string CharacterRelationAdded
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CharacterRelationAdded),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Relation add failed: Character='{0}'({1}), RelatedCharacter='{2}'({3}), RelationType={4}, RelationResult={5}.
        /// </summary>
        internal static string CharacterRelationAddedFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CharacterRelationAddedFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Relation removing succeeded: Character={0}, RelatedCharacter={1}, RelationType={2}, RelationResult={3}.
        /// </summary>
        internal static string CharacterRelationRemoved
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CharacterRelationRemoved),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Relation removing failed: Character={0}, RelatedCharacter={1}, RelationType={2}, RelationResult={3}.
        /// </summary>
        internal static string CharacterRelationRemoveFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CharacterRelationRemoveFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Loaded {0} character relations of character '{1}' from the DB..
        /// </summary>
        internal static string CharacterRelationsLoaded
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CharacterRelationsLoaded),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Error loading the character relations for player '{0}' from the DB ({1}).
        /// </summary>
        internal static string CharacterRelationsLoadFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CharacterRelationsLoadFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Unloaded {0} character relations of character '{1}'..
        /// </summary>
        internal static string CharacterRelationsUnloaded
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CharacterRelationsUnloaded),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Relation validation failed: Character='{0}'({1}), RelatedCharacter='{2}'({3}), RelationType={4}, RelationResult={5}.
        /// </summary>
        internal static string CharacterRelationValidationFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CharacterRelationValidationFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Player '{0}' already logged in! Removing them from the online list!.
        /// </summary>
        internal static string CharacterStillLoggedIn
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CharacterStillLoggedIn),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Command configuration changed; reloading!.
        /// </summary>
        internal static string CommandConfigChanged
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CommandConfigChanged),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The command manager failed to (re)start! Reason: failed to create the script environment!.
        /// </summary>
        internal static string CommandMgrCEFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CommandMgrCEFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The command manager failed to (re)start/stop! Reason: failed to destroy the existing script environment!.
        /// </summary>
        internal static string CommandMgrDEFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CommandMgrDEFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The command manager failed to (re)start! Reason: failed to load command script files!.
        /// </summary>
        internal static string CommandMgrLoadFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CommandMgrLoadFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The command manager (re)started successfully!.
        /// </summary>
        internal static string CommandMgrStarted
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CommandMgrStarted),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The command manager stopped successfully!.
        /// </summary>
        internal static string CommandMgrStopped
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CommandMgrStopped),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The specified command module doesn't exist! Module: {0}.
        /// </summary>
        internal static string CommandModuleDoesntExist
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CommandModuleDoesntExist),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to An exception occured within the IPC device when trying to communicate with the AuthServer..
        /// </summary>
        internal static string CommunicationException
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(CommunicationException),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to There was a catastrophic database failure, server not started.
        /// </summary>
        internal static string DatabaseFailure
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(DatabaseFailure),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The required DBC file, '{0}', does not exist!.
        /// </summary>
        internal static string DBCFileDoesntExist
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(DBCFileDoesntExist),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Failed to load and parse the DBC file, '{0}'!.
        /// </summary>
        internal static string DBCLoadFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(DBCLoadFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Packet received/parsed with erroneous length! Packet ID: {0:X4} Length given: {1}.
        /// </summary>
        internal static string ErroneousPacketReceived
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(ErroneousPacketReceived),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Failed to retrieve account '{0}' from the AuthServer!.
        /// </summary>
        internal static string FailedToRetrieveAccount
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(FailedToRetrieveAccount),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to A fatal, unhandled exception was encountered!.
        /// </summary>
        internal static string FatalUnhandledException
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(FatalUnhandledException),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to GameTables loaded in {0}ms.
        /// </summary>
        internal static string GameTablesLoaded
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(GameTablesLoaded),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Loaded data for Guild '{0}' from the DB..
        /// </summary>
        internal static string GuildLoaded
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(GuildLoaded),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Error loading the data for Guild with ID '{0}' from the DB..
        /// </summary>
        internal static string GuildLoadFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(GuildLoadFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Unloaded Guild '{0}'..
        /// </summary>
        internal static string GuildUnload
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(GuildUnload),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Handling packet '{0}' with size of {1} bytes..
        /// </summary>
        internal static string HandlingPacket
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(HandlingPacket),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Attempt to rename a character not belonging to the client's account! GUID: '{0}' ({1}).
        /// </summary>
        internal static string IllegalRenameAttempt
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(IllegalRenameAttempt),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to An entity attempted to move to an invalid/non-existent map! Target map: {0} Entity: {1}.
        /// </summary>
        internal static string InvalidMapMove
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(InvalidMapMove),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to IPC device failed to reconnect to the AuthServer. Is the AuthServer running?.
        /// </summary>
        internal static string IPCProxyCouldntReconnect
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(IPCProxyCouldntReconnect),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to IPC device disconnected from AuthServer!.
        /// </summary>
        internal static string IPCProxyDisconnected
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(IPCProxyDisconnected),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to IPC device failed to connect to the AuthServer because the AuthServer is probably not running. Retrying in {0} seconds....
        /// </summary>
        internal static string IPCProxyFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(IPCProxyFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to IPC device failed to connect to the AuthServer. Retrying in {0} seconds....
        /// </summary>
        internal static string IPCProxyFailedException
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(IPCProxyFailedException),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to IPC device successfully reconnected to the AuthServer!.
        /// </summary>
        internal static string IPCProxyReconnected
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(IPCProxyReconnected),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to IPC device successfully connected to the AuthServer!.
        /// </summary>
        internal static string IPCProxySucceeded
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(IPCProxySucceeded),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Player tried to delete a mail message that is not theirs; possible error or exploit attempt! Player name: {0}.
        /// </summary>
        internal static string MailNotOwnerDelete
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(MailNotOwnerDelete),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Mail System Halted.
        /// </summary>
        internal static string MailSystemHalted
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(MailSystemHalted),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Mail System Started.
        /// </summary>
        internal static string MailSystemLoaded
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(MailSystemLoaded),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Starting Mail System....
        /// </summary>
        internal static string MailSystemStart
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(MailSystemStart),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Halting Mail System....
        /// </summary>
        internal static string MailSystemStop
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(MailSystemStop),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to {0} maps loaded in {1}ms.
        /// </summary>
        internal static string MapsLoaded
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(MapsLoaded),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Could not find {0}: {1}.
        /// </summary>
        internal static string NotFound
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(NotFound), WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to An exception occured while trying to handle packet! Packet ID: '{0}'.
        /// </summary>
        internal static string PacketHandleException
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(PacketHandleException),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The packet ID was out of range. Possible encryption/decryption corruption..
        /// </summary>
        internal static string PacketIDOutOfRange
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(PacketIDOutOfRange),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Packet reports a length bigger than the amount of data available! Packet ID: {0}, reported length: {1}, available length: {2}.
        /// </summary>
        internal static string PacketLengthMismatch
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(PacketLengthMismatch),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Failed to parse packet properly! Given opcode: {0:X4}.
        /// </summary>
        internal static string PacketParseFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(PacketParseFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Partitioned and started {0} maps in {1}ms.
        /// </summary>
        internal static string PartitionStartComplete
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(PartitionStartComplete),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Map context required. Add a message to the map to ensure the action is performed within the Map's context: {0}.
        /// </summary>
        internal static string MapContextNeeded
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(MapContextNeeded),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Map context prohibited. Make sure to NOT call this method from within the map context due to deadlock risks or blocking code: {0}.
        /// </summary>
        internal static string MapContextProhibited
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(MapContextProhibited),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Map {0} now running!.
        /// </summary>
        internal static string MapStarted
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(MapStarted),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Map {0} stopped!.
        /// </summary>
        internal static string MapStopped
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(MapStopped),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Map is calling method while updating. Make sure to NOT call this and similar methods while a Map is updating: {0}.
        /// </summary>
        internal static string MapUpdating
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(MapUpdating),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Registered all packet handlers!.
        /// </summary>
        internal static string RegisteredAllHandlers
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(RegisteredAllHandlers),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Registered handler for {0}() for '{1}'.
        /// </summary>
        internal static string RegisteredHandler
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(RegisteredHandler),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Trying to register RealmServer while RealmServer is not running..
        /// </summary>
        internal static string RegisterNotRunning
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(RegisterNotRunning),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Relation Manager started.
        /// </summary>
        internal static string RelationManagerStarted
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(RelationManagerStarted),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Relation Manager stopped.
        /// </summary>
        internal static string RelationManagerStopped
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(RelationManagerStopped),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Renaming character - original name: '{0}' new name: '{1}'.
        /// </summary>
        internal static string RenamingCharacter
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(RenamingCharacter),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Resetting the IPC connection to the AuthServer!.
        /// </summary>
        internal static string ConnectingToAuthServer
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(ConnectingToAuthServer),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Error while getting script engine instance! Script engine: '{0}'.
        /// </summary>
        internal static string ScriptEngineRetrieveError
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(ScriptEngineRetrieveError),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Exception encountered during script execution! Script engine: '{0}' Module name: '{1}' Function name: '{2}' {3}Exception: {4}.
        /// </summary>
        internal static string ScriptExecutionError
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(ScriptExecutionError),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Failed to load script: {0}!.
        /// </summary>
        internal static string ScriptLoadFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(ScriptLoadFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Sending packet '{0}' with a length of {1}.
        /// </summary>
        internal static string SendingPacket
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(SendingPacket),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to An exception was caught when trying to bind to the listening address! Make sure to check your configuration and verify that the host address and port are valid for this computer..
        /// </summary>
        internal static string ServerBaseStartExCaught
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(ServerBaseStartExCaught),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Failed to load the server ruleset for the given server type: {0}.
        /// </summary>
        internal static string ServerRulesetLoadFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(ServerRulesetLoadFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Starting up realm server..
        /// </summary>
        internal static string StartingServer
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(StartingServer),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Starting/initializing: {0}.
        /// </summary>
        internal static string StartInit
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(StartInit),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Exception occured when starting/initializing: {0}.
        /// </summary>
        internal static string StartInitException
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(StartInitException),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Stopping/cleaning up: {0}.
        /// </summary>
        internal static string StopCleanup
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(StopCleanup),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Exception occured when stopping/cleaning up: {0}.
        /// </summary>
        internal static string StopCleanupException
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(StopCleanupException),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The world manager failed to start!.
        /// </summary>
        internal static string WorldMgrFailed
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(WorldMgrFailed),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The world manager started successfully!.
        /// </summary>
        internal static string WorldMgrSucceeded
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(WorldMgrSucceeded),
                    WCell_RealmServer.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to {0} zones loaded in {1}ms.
        /// </summary>
        internal static string ZonesLoaded
        {
            get
            {
                return WCell_RealmServer.ResourceManager.GetString(nameof(ZonesLoaded),
                    WCell_RealmServer.resourceCulture);
            }
        }
    }
}