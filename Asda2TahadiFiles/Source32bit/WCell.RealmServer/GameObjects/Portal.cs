using System;
using WCell.Constants.GameObjects;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.GameObjects
{
  public class Portal : GameObject
  {
    private IWorldLocation m_Target;

    public static Portal Create(IWorldLocation where, IWorldLocation target)
    {
      GOEntry entry = GOMgr.GetEntry(GOEntryId.Portal, true);
      if(entry == null)
        return null;
      Portal portal = (Portal) GameObject.Create(entry, where, null, null);
      portal.Target = target;
      return portal;
    }

    public static Portal Create(MapId mapId, Vector3 pos, MapId targetMap, Vector3 targetPos)
    {
      GOEntry entry = GOMgr.GetEntry(GOEntryId.Portal, true);
      if(entry == null)
        return null;
      Map nonInstancedMap = World.GetNonInstancedMap(mapId);
      if(nonInstancedMap == null)
        throw new ArgumentException("Invalid MapId (not a Continent): " + mapId);
      Portal portal = (Portal) GameObject.Create(entry, new WorldLocationStruct(mapId, pos, 1U),
        null, null);
      portal.Target = new WorldLocation(targetMap, targetPos, 1U);
      nonInstancedMap.AddObject(portal);
      return portal;
    }

    public Portal()
    {
    }

    protected Portal(IWorldLocation target)
    {
      Target = target;
    }

    /// <summary>
    /// Can be used to set the <see cref="P:WCell.RealmServer.GameObjects.Portal.Target" />
    /// </summary>
    public ZoneId TargetZoneId
    {
      get
      {
        if(Target is IWorldZoneLocation && ((IWorldZoneLocation) Target).ZoneTemplate != null)
          return ((IWorldZoneLocation) Target).ZoneTemplate.Id;
        return ZoneId.None;
      }
      set { Target = World.GetSite(value); }
    }

    /// <summary>
    /// Can be used to set the <see cref="P:WCell.RealmServer.GameObjects.Portal.Target" />
    /// </summary>
    public ZoneTemplate TargetZone
    {
      get
      {
        if(Target is IWorldZoneLocation)
          return ((IWorldZoneLocation) Target).ZoneTemplate;
        return null;
      }
      set { Target = value.Site; }
    }

    /// <summary>The target to which everyone should be teleported.</summary>
    public IWorldLocation Target
    {
      get { return m_Target; }
      set
      {
        m_Target = value;
        if(m_Target == null)
          throw new Exception("Target for GOPortalEntry must not be null.");
      }
    }
  }
}