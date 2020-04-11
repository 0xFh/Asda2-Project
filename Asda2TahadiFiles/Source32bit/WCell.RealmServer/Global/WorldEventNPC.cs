using WCell.Util.Data;

namespace WCell.RealmServer.Global
{
  /// <summary>
  /// Holds all information regarding a spawn
  /// involved in a WorldEvent
  /// </summary>
  public class WorldEventNPC
  {
    /// <summary>Spawn id of the object</summary>
    public uint Guid;

    /// <summary>
    /// ID of the world event relating to this entry
    /// as found in the database, negative values mean
    /// we should despawn
    /// </summary>
    public int _eventId;

    /// <summary>ID of the world event relating to this entry</summary>
    [NotPersistent]public uint EventId;

    /// <summary>
    /// True if we should spawn this entry
    /// False if we should despawn it
    /// </summary>
    [NotPersistent]public bool Spawn;

    public void FinalizeDataHolder()
    {
      Spawn = _eventId > 0;
      EventId = (uint) _eventId;
      WorldEvent worldEvent = WorldEventMgr.GetEvent(EventId);
      if(worldEvent == null)
        return;
      worldEvent.NPCSpawns.Add(this);
    }
  }
}