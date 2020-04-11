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

        [WCell.Core.Initialization.Initialization(InitializationPass.Last, "Teleport manager.")]
        public static void Init()
        {
            Asda2TeleportMgr.Teleports.Add(1, new Asda2TeleportCristalVector()
            {
                To = new Vector3(1295f, 1235f),
                Price = 0,
                ToMap = MapId.RainRiver
            });
            Asda2TeleportMgr.Teleports.Add(3, new Asda2TeleportCristalVector()
            {
                To = new Vector3(3117f, 3389f),
                Price = 0,
                ToMap = MapId.Alpia
            });
            Asda2TeleportMgr.Teleports.Add(0, new Asda2TeleportCristalVector()
            {
                To = new Vector3(393f, 397f),
                Price = 3000,
                ToMap = MapId.Silaris
            });
            Asda2TeleportMgr.Teleports.Add(7, new Asda2TeleportCristalVector()
            {
                To = new Vector3(7135f, 7188f),
                Price = 10000,
                ToMap = MapId.Flamio
            });
            Asda2TeleportMgr.Teleports.Add(5, new Asda2TeleportCristalVector()
            {
                To = new Vector3(5394f, 5342f),
                Price = 15000,
                ToMap = MapId.Aquaton
            });
            Asda2TeleportMgr.Teleports.Add(25, new Asda2TeleportCristalVector()
            {
                To = new Vector3(25274f, 25326f),
                Price = 5000,
                ToMap = MapId.DesolatedMarsh
            });
            Asda2TeleportMgr.Teleports.Add(23, new Asda2TeleportCristalVector()
            {
                To = new Vector3(23253f, 23304f),
                Price = 15000,
                ToMap = MapId.IceQuarry
            });
            Asda2TeleportMgr.Teleports.Add(2, new Asda2TeleportCristalVector()
            {
                To = new Vector3(2303f, 2309f),
                Price = 1500,
                ToMap = MapId.ConquestLand
            });
            Asda2TeleportMgr.Teleports.Add(6, new Asda2TeleportCristalVector()
            {
                To = new Vector3(6365f, 6110f),
                Price = 1500,
                ToMap = MapId.SunnyCoast
            });
            Asda2TeleportMgr.Teleports.Add(13, new Asda2TeleportCristalVector()
            {
                To = new Vector3(13208f, 13388f),
                Price = 5000,
                ToMap = MapId.Flabis
            });
            Asda2TeleportMgr.Teleports.Add(24, new Asda2TeleportCristalVector()
            {
                To = new Vector3(24318f, 24310f),
                Price = 5000,
                ToMap = MapId.BurnedoutForest
            });
        }
    }
}