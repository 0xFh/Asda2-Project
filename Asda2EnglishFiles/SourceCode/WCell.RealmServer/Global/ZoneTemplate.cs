using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Login;
using WCell.Constants.World;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Global
{
    /// <summary>Holds information about a zone, an area within a map.</summary>
    public class ZoneTemplate
    {
        public readonly List<ZoneTemplate> ChildZones = new List<ZoneTemplate>(1);
        public readonly List<WorldMapOverlayId> WorldMapOverlays = new List<WorldMapOverlayId>();
        internal ZoneTemplate m_ParentZone;
        internal ZoneId m_parentZoneId;
        internal MapTemplate m_MapTemplate;
        internal MapId m_MapId;
        public ZoneId Id;

        /// <summary>All WorldStates that are active in this Zone</summary>
        public WorldState[] WorldStates;

        /// <summary>The location of a significant site within this Zone.</summary>
        public IWorldLocation Site;

        public int ExplorationBit;

        /// <summary>The flags for the zone.</summary>
        public ZoneFlags Flags;

        /// <summary>Who does this Zone belong to (if anyone)</summary>
        public FactionGroupMask Ownership;

        public int AreaLevel;
        public string Name;
        public ZoneCreator Creator;

        public event ZoneTemplate.ZonePlayerEnteredHandler PlayerEntered;

        public event ZoneTemplate.ZonePlayerLeftHandler PlayerLeft;

        public MapId MapId
        {
            get { return this.m_MapId; }
            set
            {
                this.m_MapId = value;
                this.m_MapTemplate = WCell.RealmServer.Global.World.GetMapTemplate(value);
            }
        }

        public MapTemplate MapTemplate
        {
            get { return this.m_MapTemplate; }
            set
            {
                this.m_MapTemplate = value;
                this.m_MapId = value != null ? value.Id : MapId.End;
            }
        }

        public ZoneId ParentZoneId
        {
            get { return this.m_parentZoneId; }
        }

        public ZoneTemplate ParentZone
        {
            get { return this.m_ParentZone; }
            internal set
            {
                this.m_ParentZone = value;
                this.m_parentZoneId = value != null ? value.Id : ZoneId.None;
                if (value == null)
                    return;
                this.m_ParentZone.ChildZones.Add(this);
            }
        }

        /// <summary>
        /// Whether this is a PvP zone.
        /// Improve: http://www.wowwiki.com/PvP_flag
        /// </summary>
        public bool IsPvP
        {
            get { return this.Ownership != (FactionGroupMask.Alliance | FactionGroupMask.Horde); }
        }

        /// <summary>Whether or not the zone is an arena.</summary>
        public bool IsArena
        {
            get { return this.Flags.HasFlag((Enum) ZoneFlags.Arena); }
        }

        /// <summary>Whether or not the zone is a sanctuary.</summary>
        public bool IsSanctuary
        {
            get { return this.Flags.HasFlag((Enum) ZoneFlags.Sanctuary); }
        }

        /// <summary>Whether or not the zone is a city.</summary>
        public bool IsCity
        {
            get { return this.Flags.HasFlag((Enum) ZoneFlags.CapitalCity); }
        }

        /// <summary>
        /// Whether this Zone is hostile towards the given Character
        /// </summary>
        /// <param name="chr">The Character in question.</param>
        /// <returns>Whether or not to set the PvP flag.</returns>
        public bool IsHostileTo(Character chr)
        {
            FactionGroupMask ownership = this.Ownership;
            if (this.Ownership == FactionGroupMask.None && this.ParentZoneId != ZoneId.None &&
                this.ParentZone.Ownership != FactionGroupMask.None)
                ownership = this.ParentZone.Ownership;
            switch (ownership)
            {
                case FactionGroupMask.None:
                    return RealmServerConfiguration.ServerType.HasAnyFlag(RealmServerType.RPPVP);
                case FactionGroupMask.Alliance:
                    if (chr.FactionGroup == FactionGroup.Alliance)
                        return false;
                    if (!RealmServerConfiguration.ServerType.HasAnyFlag(RealmServerType.RPPVP) && !this.IsCity)
                        return this.IsArena;
                    return true;
                case FactionGroupMask.Horde:
                    if (chr.FactionGroup == FactionGroup.Horde)
                        return false;
                    if (!RealmServerConfiguration.ServerType.HasAnyFlag(RealmServerType.RPPVP) && !this.IsCity)
                        return this.IsArena;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Called when a player enters the zone.</summary>
        /// <param name="chr">the character entering the zone</param>
        /// <param name="oldZone">the zone the character came from</param>
        internal void OnPlayerEntered(Character chr, Zone oldZone)
        {
            ZoneTemplate.ZonePlayerEnteredHandler playerEntered = this.PlayerEntered;
            if (playerEntered == null)
                return;
            playerEntered(chr, oldZone);
        }

        /// <summary>Called when a player leaves the zone.</summary>
        /// <param name="chr">the character leaving the zone</param>
        /// <param name="oldZone">the zone the character just left</param>
        internal void OnPlayerLeft(Character chr, Zone oldZone)
        {
            ZoneTemplate.ZonePlayerLeftHandler playerLeft = this.PlayerLeft;
            if (playerLeft == null)
                return;
            playerLeft(chr, oldZone);
        }

        public virtual void OnHonorableKill(Character victor, Character victim)
        {
        }

        /// <summary>Called after ZoneInfo was created</summary>
        internal void FinalizeZone()
        {
            if (this.Creator == null)
                this.Creator = new ZoneCreator(this.DefaultCreator);
            if (this.ParentZoneId != ZoneId.None)
                return;
            this.WorldStates = WCell.Constants.World.WorldStates.GetStates(this.Id);
        }

        public Zone DefaultCreator(Map map, ZoneTemplate templ)
        {
            return new Zone(map, templ);
        }

        public delegate void ZonePlayerEnteredHandler(Character targetChr, Zone oldZone);

        public delegate void ZonePlayerLeftHandler(Character targetChr, Zone newZone);
    }
}