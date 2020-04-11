using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Editor.Figurines
{
    /// <summary>A decorative Unit without a brain</summary>
    public class FigurineBase : Unit
    {
        private static AuraCollection _sharedAuras;

        protected FigurineBase(NPCId id)
        {
            if (FigurineBase._sharedAuras == null)
                FigurineBase._sharedAuras = new AuraCollection((Unit) this);
            this.m_auras = FigurineBase._sharedAuras;
            this.GenerateId((uint) id);
            NPCEntry entry = NPCMgr.GetEntry(id);
            this.UnitFlags = UnitFlags.SelectableNotAttackable | UnitFlags.Possessed;
            this.DynamicFlags = UnitDynamicFlags.TrackUnit;
            this.EmoteState = EmoteType.StateDead;
            this.NPCFlags |= NPCFlags.Gossip;
            this.Model = entry.GetRandomModel();
            this.EntryId = entry.Id;
            this.m_runSpeed = 1f;
            this.m_swimSpeed = 1f;
            this.m_swimBackSpeed = 1f;
            this.m_walkSpeed = 1f;
            this.m_walkBackSpeed = 1f;
            this.m_flightSpeed = 1f;
            this.m_flightBackSpeed = 1f;
            this.SetInt32((UpdateFieldId) UnitFields.MAXHEALTH, int.MaxValue);
            this.SetInt32((UpdateFieldId) UnitFields.BASE_HEALTH, int.MaxValue);
            this.SetInt32((UpdateFieldId) UnitFields.HEALTH, int.MaxValue);
            this.SetFloat((UpdateFieldId) ObjectFields.SCALE_X, entry.Scale * this.DefaultScale);
            this.m_evades = true;
        }

        /// <summary>Editor is only visible to staff members</summary>
        public override VisibilityStatus DetermineVisibilityFor(Unit observer)
        {
            return !(observer is Character) || !((Character) observer).Role.IsStaff
                ? VisibilityStatus.Invisible
                : VisibilityStatus.Visible;
        }

        public override LinkedList<WaypointEntry> Waypoints
        {
            get { return WaypointEntry.EmptyList; }
        }

        public override NPCSpawnPoint SpawnPoint
        {
            get { return (NPCSpawnPoint) null; }
        }

        public virtual float DefaultScale
        {
            get { return 1f; }
        }

        public override string Name
        {
            get { return "Dummy"; }
            set { }
        }

        protected override bool OnBeforeDeath()
        {
            return true;
        }

        protected override void OnDeath()
        {
            this.Delete();
        }

        public override Faction Faction
        {
            get { return this.DefaultFaction; }
            set { }
        }

        public override Faction DefaultFaction
        {
            get { return FactionMgr.Get(FactionId.Friendly); }
        }

        public override FactionId FactionId
        {
            get { return this.Faction.Id; }
            set { }
        }

        public override void Dispose(bool disposing)
        {
            this.m_Map = (Map) null;
        }
    }
}