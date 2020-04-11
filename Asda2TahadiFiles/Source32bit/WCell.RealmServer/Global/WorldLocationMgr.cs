using System;
using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Lang;

namespace WCell.RealmServer.Global
{
  /// <summary>
  /// 
  /// </summary>
  public static class WorldLocationMgr
  {
    /// <summary>All world locations definition</summary>
    public static readonly Dictionary<string, INamedWorldZoneLocation> WorldLocations =
      new Dictionary<string, INamedWorldZoneLocation>(
        StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// For faster iteration (Do we even need the dictionary?)
    /// </summary>
    private static INamedWorldZoneLocation[] LocationCache;

    public static INamedWorldZoneLocation Stormwind;
    public static INamedWorldZoneLocation Orgrimmar;

    public static List<INamedWorldZoneLocation> WorldLocationList
    {
      get { return WorldLocations.Values.ToList(); }
    }

    /// <summary>Depends on Table-Creation (Third)</summary>
    public static void Initialize()
    {
      ContentMgr.Load<WorldZoneLocation>();
      LocationCache = WorldLocations.Values.ToArray();
      Stormwind = GetFirstMatch("Stormwind");
      Orgrimmar = GetFirstMatch("Orgrimmar");
    }

    /// <summary>
    /// Searches in loaded world locations for a specific world location name
    /// </summary>
    /// <param name="name">Name of the location to search</param>
    /// <returns>WorldLocation for the selected location. Returns null if not found</returns>
    public static INamedWorldZoneLocation Get(string name)
    {
      INamedWorldZoneLocation worldZoneLocation;
      WorldLocations.TryGetValue(name, out worldZoneLocation);
      return worldZoneLocation;
    }

    /// <summary>
    /// Gets the first <see cref="T:WCell.RealmServer.Global.WorldZoneLocation" /> matching the given name parts
    /// </summary>
    /// <param name="partialName"></param>
    /// <returns></returns>
    public static INamedWorldZoneLocation GetFirstMatch(string partialName)
    {
      string[] strArray = partialName.Split(new char[1]
      {
        ' '
      }, StringSplitOptions.RemoveEmptyEntries);
      for(int index1 = 0; index1 < LocationCache.Length; ++index1)
      {
        INamedWorldZoneLocation worldZoneLocation = LocationCache[index1];
        bool flag = true;
        for(int index2 = 0; index2 < strArray.Length; ++index2)
        {
          string str = strArray[index2];
          if(worldZoneLocation.DefaultName.IndexOf(str, StringComparison.InvariantCultureIgnoreCase) == -1)
          {
            flag = false;
            break;
          }
        }

        if(flag)
          return worldZoneLocation;
      }

      return null;
    }

    public static List<INamedWorldZoneLocation> GetMatches(string partialName)
    {
      List<INamedWorldZoneLocation> worldZoneLocationList = new List<INamedWorldZoneLocation>(3);
      string[] strArray = partialName.Split(new char[1]
      {
        ' '
      }, StringSplitOptions.RemoveEmptyEntries);
      for(int index1 = 0; index1 < LocationCache.Length; ++index1)
      {
        INamedWorldZoneLocation worldZoneLocation = LocationCache[index1];
        bool flag = true;
        for(int index2 = 0; index2 < strArray.Length; ++index2)
        {
          string str = strArray[index2];
          if(worldZoneLocation.DefaultName.IndexOf(str, StringComparison.InvariantCultureIgnoreCase) == -1)
          {
            flag = false;
            break;
          }
        }

        if(flag)
          worldZoneLocationList.Add(worldZoneLocation);
      }

      return worldZoneLocationList;
    }

    /// <summary>Creates a GossipMenu of all locations</summary>
    /// <returns></returns>
    public static GossipMenu CreateTeleMenu()
    {
      return CreateTeleMenu(WorldLocationList);
    }

    public static GossipMenu CreateTeleMenu(List<INamedWorldZoneLocation> locations)
    {
      return CreateTeleMenu(locations,
        (convo, loc) =>
          convo.Character.TeleportTo(loc));
    }

    public static GossipMenu CreateTeleMenu(List<INamedWorldZoneLocation> locations,
      Action<GossipConversation, INamedWorldZoneLocation> callback)
    {
      GossipMenu gossipMenu = new GossipMenu();
      foreach(INamedWorldZoneLocation location in locations)
      {
        INamedWorldZoneLocation loc = location;
        gossipMenu.AddItem(new GossipMenuItem(loc.Names.LocalizeWithDefaultLocale(),
          convo => callback(convo, loc)));
      }

      return gossipMenu;
    }
  }
}