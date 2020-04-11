using System.Collections.Generic;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Handlers
{
  public static class Asda2TeleportMgr
  {
    public static Dictionary<int, Asda2TeleportCristalVector> Teleports =
      new Dictionary<int, Asda2TeleportCristalVector>();

    [Initialization(InitializationPass.Last, "Teleport manager.")]
    public static void Init()
    {
      Teleports.Add(1, new Asda2TeleportCristalVector
      {
        To = new Vector3(1295f, 1235f),
        Price = 0,
        ToMap = MapId.RainRiver
      });
      Teleports.Add(3, new Asda2TeleportCristalVector
      {
        To = new Vector3(3117f, 3389f),
        Price = 0,
        ToMap = MapId.Alpia
      });
      Teleports.Add(0, new Asda2TeleportCristalVector
      {
        To = new Vector3(393f, 397f),
        Price = 3000,
        ToMap = MapId.Silaris
      });
      Teleports.Add(7, new Asda2TeleportCristalVector
      {
        To = new Vector3(7135f, 7188f),
        Price = 10000,
        ToMap = MapId.Flamio
      });
      Teleports.Add(5, new Asda2TeleportCristalVector
      {
        To = new Vector3(5394f, 5342f),
        Price = 15000,
        ToMap = MapId.Aquaton
      });
      Teleports.Add(25, new Asda2TeleportCristalVector
      {
        To = new Vector3(25274f, 25326f),
        Price = 5000,
        ToMap = MapId.DesolatedMarsh
      });
      Teleports.Add(23, new Asda2TeleportCristalVector
      {
        To = new Vector3(23253f, 23304f),
        Price = 15000,
        ToMap = MapId.IceQuarry
      });
      Teleports.Add(2, new Asda2TeleportCristalVector
      {
        To = new Vector3(2303f, 2309f),
        Price = 1500,
        ToMap = MapId.ConquestLand
      });
      Teleports.Add(6, new Asda2TeleportCristalVector
      {
        To = new Vector3(6365f, 6110f),
        Price = 1500,
        ToMap = MapId.SunnyCoast
      });
      Teleports.Add(13, new Asda2TeleportCristalVector
      {
        To = new Vector3(13208f, 13388f),
        Price = 5000,
        ToMap = MapId.Flabis
      });
      Teleports.Add(24, new Asda2TeleportCristalVector
      {
        To = new Vector3(24318f, 24310f),
        Price = 5000,
        ToMap = MapId.BurnedoutForest
      });
    }
  }
}