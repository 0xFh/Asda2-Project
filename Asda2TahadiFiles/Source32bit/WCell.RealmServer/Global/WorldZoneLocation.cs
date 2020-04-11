using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
  /// <summary>
  /// World locations are specific game locations that can be accessed with teleport command.
  /// </summary>
  public class WorldZoneLocation : IDataHolder, INamedWorldZoneLocation, IWorldZoneLocation, IWorldLocation,
    IHasPosition
  {
    private string[] m_Names = new string[8];
    public uint Id;
    private ZoneTemplate m_ZoneTemplate;
    private ZoneId m_ZoneId;

    [NotPersistent]
    public string[] Names
    {
      get { return m_Names; }
      set { m_Names = value; }
    }

    public uint Phase
    {
      get { return 1; }
    }

    public WorldZoneLocation()
    {
    }

    public WorldZoneLocation(string name, MapId mapId, Vector3 pos)
      : this(mapId, pos)
    {
      DefaultName = name;
    }

    public WorldZoneLocation(string[] localizedNames, MapId mapId, Vector3 pos)
      : this(mapId, pos)
    {
      Names = localizedNames;
    }

    private WorldZoneLocation(MapId mapId, Vector3 pos)
    {
      MapId = mapId;
      Position = pos;
    }

    public string DefaultName
    {
      get { return Names.LocalizeWithDefaultLocale(); }
      set { Names[(int) RealmServerConfiguration.DefaultLocale] = value; }
    }

    public string EnglishName
    {
      get { return Names.LocalizeWithDefaultLocale(); }
      set { Names[0] = value; }
    }

    public MapId MapId { get; set; }

    public Map Map
    {
      get { return World.GetNonInstancedMap(MapId); }
    }

    public Vector3 Position { get; set; }

    public ZoneId ZoneId
    {
      get { return m_ZoneId; }
      set { m_ZoneId = value; }
    }

    /// <summary>The Zone to which this Location belongs (if any)</summary>
    [NotPersistent]
    public ZoneTemplate ZoneTemplate
    {
      get { return m_ZoneTemplate; }
      set
      {
        m_ZoneTemplate = value;
        ZoneId = m_ZoneTemplate != null ? m_ZoneTemplate.Id : ZoneId.None;
      }
    }

    public uint GetId()
    {
      return Id;
    }

    public DataHolderState DataHolderState { get; set; }

    public void FinalizeDataHolder()
    {
      WorldLocationMgr.WorldLocations[DefaultName] = this;
      ZoneTemplate zoneInfo = World.GetZoneInfo(ZoneId);
      if(zoneInfo == null)
        return;
      if(zoneInfo.Site is WorldZoneLocation)
        ((WorldZoneLocation) zoneInfo.Site).ZoneTemplate = null;
      else if(zoneInfo.Site != null)
        return;
      zoneInfo.Site = this;
      ZoneTemplate = zoneInfo;
    }

    public override bool Equals(object obj)
    {
      if(obj is WorldZoneLocation && ((WorldZoneLocation) obj).Position.X == (double) Position.X &&
         (((WorldZoneLocation) obj).Position.Y == (double) Position.Y &&
          ((WorldZoneLocation) obj).Position.Z == (double) Position.Z))
        return ((WorldZoneLocation) obj).MapId == MapId;
      return false;
    }

    public override int GetHashCode()
    {
      return (int) ((double) MapId *
                    (Position.X * (double) Position.Y * Position.Z));
    }

    /// <summary>
    /// Overload the ToString method to return a formated text with world location name and id
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return string.Format("{0} (Id: {1})", DefaultName, Id);
    }
  }
}