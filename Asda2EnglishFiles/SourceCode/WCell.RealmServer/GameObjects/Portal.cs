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
            if (entry == null)
                return (Portal) null;
            Portal portal = (Portal) GameObject.Create(entry, where, (GOSpawnEntry) null, (GOSpawnPoint) null);
            portal.Target = target;
            return portal;
        }

        public static Portal Create(MapId mapId, Vector3 pos, MapId targetMap, Vector3 targetPos)
        {
            GOEntry entry = GOMgr.GetEntry(GOEntryId.Portal, true);
            if (entry == null)
                return (Portal) null;
            Map nonInstancedMap = WCell.RealmServer.Global.World.GetNonInstancedMap(mapId);
            if (nonInstancedMap == null)
                throw new ArgumentException("Invalid MapId (not a Continent): " + (object) mapId);
            Portal portal = (Portal) GameObject.Create(entry, (IWorldLocation) new WorldLocationStruct(mapId, pos, 1U),
                (GOSpawnEntry) null, (GOSpawnPoint) null);
            portal.Target = (IWorldLocation) new WorldLocation(targetMap, targetPos, 1U);
            nonInstancedMap.AddObject((WorldObject) portal);
            return portal;
        }

        public Portal()
        {
        }

        protected Portal(IWorldLocation target)
        {
            this.Target = target;
        }

        /// <summary>
        /// Can be used to set the <see cref="P:WCell.RealmServer.GameObjects.Portal.Target" />
        /// </summary>
        public ZoneId TargetZoneId
        {
            get
            {
                if (this.Target is IWorldZoneLocation && ((IWorldZoneLocation) this.Target).ZoneTemplate != null)
                    return ((IWorldZoneLocation) this.Target).ZoneTemplate.Id;
                return ZoneId.None;
            }
            set { this.Target = WCell.RealmServer.Global.World.GetSite(value); }
        }

        /// <summary>
        /// Can be used to set the <see cref="P:WCell.RealmServer.GameObjects.Portal.Target" />
        /// </summary>
        public ZoneTemplate TargetZone
        {
            get
            {
                if (this.Target is IWorldZoneLocation)
                    return ((IWorldZoneLocation) this.Target).ZoneTemplate;
                return (ZoneTemplate) null;
            }
            set { this.Target = value.Site; }
        }

        /// <summary>The target to which everyone should be teleported.</summary>
        public IWorldLocation Target
        {
            get { return this.m_Target; }
            set
            {
                this.m_Target = value;
                if (this.m_Target == null)
                    throw new Exception("Target for GOPortalEntry must not be null.");
            }
        }
    }
}