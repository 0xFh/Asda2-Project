using System.Collections.Generic;
using WCell.Constants.GameObjects;
using WCell.Constants.World;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Spawns;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.GameObjects.Spawns
{
  /// <summary>Spawn-information for GameObjects</summary>
  [DataHolder]
  public class GOSpawnEntry : SpawnEntry<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>,
    IDataHolder
  {
    public float Scale = 1f;
    public uint Id;
    public GOEntryId EntryId;
    [NotPersistent]public uint EntryIdRaw;
    public GameObjectState State;
    [Persistent(4)]public float[] Rotations;
    public byte AnimProgress;
    [NotPersistent]public GOEntry Entry;
    [NotPersistent]public LootItemEntry LootEntry;

    public GOSpawnEntry()
    {
    }

    public GOSpawnEntry(GOEntry entry, GameObjectState state, MapId mapId, ref Vector3 pos, float orientation,
      float scale, float[] rotations, int respawnTimeSecs = 600)
    {
      Entry = entry;
      EntryId = entry.GOId;
      State = state;
      MapId = mapId;
      Position = pos;
      Orientation = orientation;
      Scale = scale;
      Rotations = rotations;
      RespawnSeconds = respawnTimeSecs;
    }

    public uint LootMoney
    {
      get { return 0; }
    }

    /// <summary>
    /// Spawns and returns a new GameObject from this template into the given map
    /// </summary>
    /// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
    public GameObject Spawn(Map map)
    {
      if(Entry == null)
        return null;
      return GameObject.Create(Entry, new WorldLocationStruct(map, Position, 1U), this,
        null);
    }

    /// <summary>
    /// Spawns and returns a new GameObject from this template at the given location
    /// </summary>
    /// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
    public GameObject Spawn(IWorldLocation pos)
    {
      return GameObject.Create(Entry, pos, this, null);
    }

    public override GameObject SpawnObject(GOSpawnPoint point)
    {
      return GameObject.Create(Entry, point, this, point);
    }

    public uint GetId()
    {
      return Id;
    }

    public DataHolderState DataHolderState { get; set; }

    /// <summary>Finalize this GOSpawnEntry</summary>
    /// <param name="addToPool">If set to false, will not try to add it to any pool (recommended for custom GOSpawnEntry that share a pool)</param>
    public void FinalizeDataHolder()
    {
      FinalizeDataHolder(true);
    }

    private void AddToPoolTemplate()
    {
      m_PoolTemplate = GOMgr.GetOrCreateSpawnPoolTemplate(PoolId);
      m_PoolTemplate.AddEntry(this);
    }

    /// <summary>Finalize this GOSpawnEntry</summary>
    /// <param name="addToPool">If set to false, will not try to add it to any pool (recommended for custom GOSpawnEntry that share a pool)</param>
    public override void FinalizeDataHolder(bool addToPool)
    {
      if(Entry == null)
      {
        Entry = GOMgr.GetEntry(EntryId, false);
        if(Entry == null)
        {
          ContentMgr.OnInvalidDBData("{0} had an invalid EntryId.", (object) this);
          return;
        }
      }

      if(Scale == 0.0)
        Scale = 1f;
      if(EntryId == GOEntryId.Loot)
        EntryId = (GOEntryId) EntryIdRaw;
      else
        EntryIdRaw = (uint) EntryId;
      if(Rotations == null)
        Rotations = new float[4];
      base.FinalizeDataHolder(addToPool);
      if(MapId == MapId.End || MapId == MapId.None)
        return;
      Entry.SpawnEntries.Add(this);
      ArrayUtil.Set(ref GOMgr.SpawnEntries, SpawnId, this);
      if(!addToPool)
        return;
      AddToPoolTemplate();
    }

    public override string ToString()
    {
      return (Entry != null ? Entry.DefaultName : "") + " (EntryId: " +
             EntryId + " (" + (int) EntryId + "))";
    }

    /// <summary>Do not remove: Used internally for caching</summary>
    public static IEnumerable<GOSpawnEntry> GetAllDataHolders()
    {
      List<GOSpawnEntry> goSpawnEntryList = new List<GOSpawnEntry>(10000);
      foreach(GOEntry goEntry in GOMgr.Entries.Values)
      {
        if(goEntry != null)
          goSpawnEntryList.AddRange(goEntry.SpawnEntries);
      }

      return goSpawnEntryList;
    }
  }
}