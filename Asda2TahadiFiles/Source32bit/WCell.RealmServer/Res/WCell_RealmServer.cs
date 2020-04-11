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

    /// <summary>
    ///   Returns the cached ResourceManager instance used by this class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if(ReferenceEquals(resourceMan, null))
          resourceMan = new ResourceManager("WCell.RealmServer.Res.WCell.RealmServer",
            typeof(WCell_RealmServer).Assembly);
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
    ///   Looks up a localized string similar to An attempt was made to recycle an EntityId that is already recycled! EntityId: {0} EntityIdType:{1}.
    /// </summary>
    internal static string AlreadyRecycledEntityId
    {
      get
      {
        return ResourceManager.GetString(nameof(AlreadyRecycledEntityId),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Tried to authenticate the client before their account was loaded/initialized!.
    /// </summary>
    internal static string BadAuthenticationProcedure
    {
      get
      {
        return ResourceManager.GetString(nameof(BadAuthenticationProcedure),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Character EntityId {0} Account {1} tried to connect while already logged in.
    /// </summary>
    internal static string CharacterAlreadyConnected
    {
      get
      {
        return ResourceManager.GetString(nameof(CharacterAlreadyConnected),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Character EntityId {0} not found in Account {1}.
    /// </summary>
    internal static string CharacterNotFound
    {
      get
      {
        return ResourceManager.GetString(nameof(CharacterNotFound),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Character null, could not register.
    /// </summary>
    internal static string CharacterNullRegister
    {
      get
      {
        return ResourceManager.GetString(nameof(CharacterNullRegister),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Character null, could not unregister.
    /// </summary>
    internal static string CharacterNullUnregister
    {
      get
      {
        return ResourceManager.GetString(nameof(CharacterNullUnregister),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Relation added successfully: Character='{0}'({1}), RelatedCharacter='{2}'({3}), RelationType={4}, RelationResult={5}.
    /// </summary>
    internal static string CharacterRelationAdded
    {
      get
      {
        return ResourceManager.GetString(nameof(CharacterRelationAdded),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Relation add failed: Character='{0}'({1}), RelatedCharacter='{2}'({3}), RelationType={4}, RelationResult={5}.
    /// </summary>
    internal static string CharacterRelationAddedFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(CharacterRelationAddedFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Relation removing succeeded: Character={0}, RelatedCharacter={1}, RelationType={2}, RelationResult={3}.
    /// </summary>
    internal static string CharacterRelationRemoved
    {
      get
      {
        return ResourceManager.GetString(nameof(CharacterRelationRemoved),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Relation removing failed: Character={0}, RelatedCharacter={1}, RelationType={2}, RelationResult={3}.
    /// </summary>
    internal static string CharacterRelationRemoveFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(CharacterRelationRemoveFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Loaded {0} character relations of character '{1}' from the DB..
    /// </summary>
    internal static string CharacterRelationsLoaded
    {
      get
      {
        return ResourceManager.GetString(nameof(CharacterRelationsLoaded),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Error loading the character relations for player '{0}' from the DB ({1}).
    /// </summary>
    internal static string CharacterRelationsLoadFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(CharacterRelationsLoadFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unloaded {0} character relations of character '{1}'..
    /// </summary>
    internal static string CharacterRelationsUnloaded
    {
      get
      {
        return ResourceManager.GetString(nameof(CharacterRelationsUnloaded),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Relation validation failed: Character='{0}'({1}), RelatedCharacter='{2}'({3}), RelationType={4}, RelationResult={5}.
    /// </summary>
    internal static string CharacterRelationValidationFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(CharacterRelationValidationFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Player '{0}' already logged in! Removing them from the online list!.
    /// </summary>
    internal static string CharacterStillLoggedIn
    {
      get
      {
        return ResourceManager.GetString(nameof(CharacterStillLoggedIn),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Command configuration changed; reloading!.
    /// </summary>
    internal static string CommandConfigChanged
    {
      get
      {
        return ResourceManager.GetString(nameof(CommandConfigChanged),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to The command manager failed to (re)start! Reason: failed to create the script environment!.
    /// </summary>
    internal static string CommandMgrCEFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(CommandMgrCEFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to The command manager failed to (re)start/stop! Reason: failed to destroy the existing script environment!.
    /// </summary>
    internal static string CommandMgrDEFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(CommandMgrDEFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to The command manager failed to (re)start! Reason: failed to load command script files!.
    /// </summary>
    internal static string CommandMgrLoadFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(CommandMgrLoadFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to The command manager (re)started successfully!.
    /// </summary>
    internal static string CommandMgrStarted
    {
      get
      {
        return ResourceManager.GetString(nameof(CommandMgrStarted),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to The command manager stopped successfully!.
    /// </summary>
    internal static string CommandMgrStopped
    {
      get
      {
        return ResourceManager.GetString(nameof(CommandMgrStopped),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to The specified command module doesn't exist! Module: {0}.
    /// </summary>
    internal static string CommandModuleDoesntExist
    {
      get
      {
        return ResourceManager.GetString(nameof(CommandModuleDoesntExist),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to An exception occured within the IPC device when trying to communicate with the AuthServer..
    /// </summary>
    internal static string CommunicationException
    {
      get
      {
        return ResourceManager.GetString(nameof(CommunicationException),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to There was a catastrophic database failure, server not started.
    /// </summary>
    internal static string DatabaseFailure
    {
      get
      {
        return ResourceManager.GetString(nameof(DatabaseFailure),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to The required DBC file, '{0}', does not exist!.
    /// </summary>
    internal static string DBCFileDoesntExist
    {
      get
      {
        return ResourceManager.GetString(nameof(DBCFileDoesntExist),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Failed to load and parse the DBC file, '{0}'!.
    /// </summary>
    internal static string DBCLoadFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(DBCLoadFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Packet received/parsed with erroneous length! Packet ID: {0:X4} Length given: {1}.
    /// </summary>
    internal static string ErroneousPacketReceived
    {
      get
      {
        return ResourceManager.GetString(nameof(ErroneousPacketReceived),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Failed to retrieve account '{0}' from the AuthServer!.
    /// </summary>
    internal static string FailedToRetrieveAccount
    {
      get
      {
        return ResourceManager.GetString(nameof(FailedToRetrieveAccount),
          resourceCulture);
      }
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
    ///   Looks up a localized string similar to GameTables loaded in {0}ms.
    /// </summary>
    internal static string GameTablesLoaded
    {
      get
      {
        return ResourceManager.GetString(nameof(GameTablesLoaded),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Loaded data for Guild '{0}' from the DB..
    /// </summary>
    internal static string GuildLoaded
    {
      get
      {
        return ResourceManager.GetString(nameof(GuildLoaded),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Error loading the data for Guild with ID '{0}' from the DB..
    /// </summary>
    internal static string GuildLoadFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(GuildLoadFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Unloaded Guild '{0}'..
    /// </summary>
    internal static string GuildUnload
    {
      get
      {
        return ResourceManager.GetString(nameof(GuildUnload),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Handling packet '{0}' with size of {1} bytes..
    /// </summary>
    internal static string HandlingPacket
    {
      get
      {
        return ResourceManager.GetString(nameof(HandlingPacket),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Attempt to rename a character not belonging to the client's account! GUID: '{0}' ({1}).
    /// </summary>
    internal static string IllegalRenameAttempt
    {
      get
      {
        return ResourceManager.GetString(nameof(IllegalRenameAttempt),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to An entity attempted to move to an invalid/non-existent map! Target map: {0} Entity: {1}.
    /// </summary>
    internal static string InvalidMapMove
    {
      get
      {
        return ResourceManager.GetString(nameof(InvalidMapMove),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to IPC device failed to reconnect to the AuthServer. Is the AuthServer running?.
    /// </summary>
    internal static string IPCProxyCouldntReconnect
    {
      get
      {
        return ResourceManager.GetString(nameof(IPCProxyCouldntReconnect),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to IPC device disconnected from AuthServer!.
    /// </summary>
    internal static string IPCProxyDisconnected
    {
      get
      {
        return ResourceManager.GetString(nameof(IPCProxyDisconnected),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to IPC device failed to connect to the AuthServer because the AuthServer is probably not running. Retrying in {0} seconds....
    /// </summary>
    internal static string IPCProxyFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(IPCProxyFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to IPC device failed to connect to the AuthServer. Retrying in {0} seconds....
    /// </summary>
    internal static string IPCProxyFailedException
    {
      get
      {
        return ResourceManager.GetString(nameof(IPCProxyFailedException),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to IPC device successfully reconnected to the AuthServer!.
    /// </summary>
    internal static string IPCProxyReconnected
    {
      get
      {
        return ResourceManager.GetString(nameof(IPCProxyReconnected),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to IPC device successfully connected to the AuthServer!.
    /// </summary>
    internal static string IPCProxySucceeded
    {
      get
      {
        return ResourceManager.GetString(nameof(IPCProxySucceeded),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Player tried to delete a mail message that is not theirs; possible error or exploit attempt! Player name: {0}.
    /// </summary>
    internal static string MailNotOwnerDelete
    {
      get
      {
        return ResourceManager.GetString(nameof(MailNotOwnerDelete),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Mail System Halted.
    /// </summary>
    internal static string MailSystemHalted
    {
      get
      {
        return ResourceManager.GetString(nameof(MailSystemHalted),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Mail System Started.
    /// </summary>
    internal static string MailSystemLoaded
    {
      get
      {
        return ResourceManager.GetString(nameof(MailSystemLoaded),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Starting Mail System....
    /// </summary>
    internal static string MailSystemStart
    {
      get
      {
        return ResourceManager.GetString(nameof(MailSystemStart),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Halting Mail System....
    /// </summary>
    internal static string MailSystemStop
    {
      get
      {
        return ResourceManager.GetString(nameof(MailSystemStop),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to {0} maps loaded in {1}ms.
    /// </summary>
    internal static string MapsLoaded
    {
      get
      {
        return ResourceManager.GetString(nameof(MapsLoaded),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Could not find {0}: {1}.
    /// </summary>
    internal static string NotFound
    {
      get { return ResourceManager.GetString(nameof(NotFound), resourceCulture); }
    }

    /// <summary>
    ///   Looks up a localized string similar to An exception occured while trying to handle packet! Packet ID: '{0}'.
    /// </summary>
    internal static string PacketHandleException
    {
      get
      {
        return ResourceManager.GetString(nameof(PacketHandleException),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to The packet ID was out of range. Possible encryption/decryption corruption..
    /// </summary>
    internal static string PacketIDOutOfRange
    {
      get
      {
        return ResourceManager.GetString(nameof(PacketIDOutOfRange),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Packet reports a length bigger than the amount of data available! Packet ID: {0}, reported length: {1}, available length: {2}.
    /// </summary>
    internal static string PacketLengthMismatch
    {
      get
      {
        return ResourceManager.GetString(nameof(PacketLengthMismatch),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Failed to parse packet properly! Given opcode: {0:X4}.
    /// </summary>
    internal static string PacketParseFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(PacketParseFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Partitioned and started {0} maps in {1}ms.
    /// </summary>
    internal static string PartitionStartComplete
    {
      get
      {
        return ResourceManager.GetString(nameof(PartitionStartComplete),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Map context required. Add a message to the map to ensure the action is performed within the Map's context: {0}.
    /// </summary>
    internal static string MapContextNeeded
    {
      get
      {
        return ResourceManager.GetString(nameof(MapContextNeeded),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Map context prohibited. Make sure to NOT call this method from within the map context due to deadlock risks or blocking code: {0}.
    /// </summary>
    internal static string MapContextProhibited
    {
      get
      {
        return ResourceManager.GetString(nameof(MapContextProhibited),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Map {0} now running!.
    /// </summary>
    internal static string MapStarted
    {
      get
      {
        return ResourceManager.GetString(nameof(MapStarted),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Map {0} stopped!.
    /// </summary>
    internal static string MapStopped
    {
      get
      {
        return ResourceManager.GetString(nameof(MapStopped),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Map is calling method while updating. Make sure to NOT call this and similar methods while a Map is updating: {0}.
    /// </summary>
    internal static string MapUpdating
    {
      get
      {
        return ResourceManager.GetString(nameof(MapUpdating),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Registered all packet handlers!.
    /// </summary>
    internal static string RegisteredAllHandlers
    {
      get
      {
        return ResourceManager.GetString(nameof(RegisteredAllHandlers),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Registered handler for {0}() for '{1}'.
    /// </summary>
    internal static string RegisteredHandler
    {
      get
      {
        return ResourceManager.GetString(nameof(RegisteredHandler),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Trying to register RealmServer while RealmServer is not running..
    /// </summary>
    internal static string RegisterNotRunning
    {
      get
      {
        return ResourceManager.GetString(nameof(RegisterNotRunning),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Relation Manager started.
    /// </summary>
    internal static string RelationManagerStarted
    {
      get
      {
        return ResourceManager.GetString(nameof(RelationManagerStarted),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Relation Manager stopped.
    /// </summary>
    internal static string RelationManagerStopped
    {
      get
      {
        return ResourceManager.GetString(nameof(RelationManagerStopped),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Renaming character - original name: '{0}' new name: '{1}'.
    /// </summary>
    internal static string RenamingCharacter
    {
      get
      {
        return ResourceManager.GetString(nameof(RenamingCharacter),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Resetting the IPC connection to the AuthServer!.
    /// </summary>
    internal static string ConnectingToAuthServer
    {
      get
      {
        return ResourceManager.GetString(nameof(ConnectingToAuthServer),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Error while getting script engine instance! Script engine: '{0}'.
    /// </summary>
    internal static string ScriptEngineRetrieveError
    {
      get
      {
        return ResourceManager.GetString(nameof(ScriptEngineRetrieveError),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Exception encountered during script execution! Script engine: '{0}' Module name: '{1}' Function name: '{2}' {3}Exception: {4}.
    /// </summary>
    internal static string ScriptExecutionError
    {
      get
      {
        return ResourceManager.GetString(nameof(ScriptExecutionError),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Failed to load script: {0}!.
    /// </summary>
    internal static string ScriptLoadFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(ScriptLoadFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Sending packet '{0}' with a length of {1}.
    /// </summary>
    internal static string SendingPacket
    {
      get
      {
        return ResourceManager.GetString(nameof(SendingPacket),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to An exception was caught when trying to bind to the listening address! Make sure to check your configuration and verify that the host address and port are valid for this computer..
    /// </summary>
    internal static string ServerBaseStartExCaught
    {
      get
      {
        return ResourceManager.GetString(nameof(ServerBaseStartExCaught),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Failed to load the server ruleset for the given server type: {0}.
    /// </summary>
    internal static string ServerRulesetLoadFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(ServerRulesetLoadFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Starting up realm server..
    /// </summary>
    internal static string StartingServer
    {
      get
      {
        return ResourceManager.GetString(nameof(StartingServer),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Starting/initializing: {0}.
    /// </summary>
    internal static string StartInit
    {
      get
      {
        return ResourceManager.GetString(nameof(StartInit),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Exception occured when starting/initializing: {0}.
    /// </summary>
    internal static string StartInitException
    {
      get
      {
        return ResourceManager.GetString(nameof(StartInitException),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Stopping/cleaning up: {0}.
    /// </summary>
    internal static string StopCleanup
    {
      get
      {
        return ResourceManager.GetString(nameof(StopCleanup),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to Exception occured when stopping/cleaning up: {0}.
    /// </summary>
    internal static string StopCleanupException
    {
      get
      {
        return ResourceManager.GetString(nameof(StopCleanupException),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to The world manager failed to start!.
    /// </summary>
    internal static string WorldMgrFailed
    {
      get
      {
        return ResourceManager.GetString(nameof(WorldMgrFailed),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to The world manager started successfully!.
    /// </summary>
    internal static string WorldMgrSucceeded
    {
      get
      {
        return ResourceManager.GetString(nameof(WorldMgrSucceeded),
          resourceCulture);
      }
    }

    /// <summary>
    ///   Looks up a localized string similar to {0} zones loaded in {1}ms.
    /// </summary>
    internal static string ZonesLoaded
    {
      get
      {
        return ResourceManager.GetString(nameof(ZonesLoaded),
          resourceCulture);
      }
    }
  }
}