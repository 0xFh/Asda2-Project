using System;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
    [Serializable]
    public class SpellTargetLocation : IWorldLocation, IHasPosition
    {
        private Vector3 m_Position;

        public SpellTargetLocation()
        {
            this.Phase = 1U;
        }

        public SpellTargetLocation(MapId map, Vector3 pos, uint phase = 1)
        {
            this.Position = pos;
            this.MapId = map;
            this.Phase = phase;
        }

        public Vector3 Position
        {
            get { return this.m_Position; }
            set { this.m_Position = value; }
        }

        [Persistent] public MapId MapId { get; set; }

        [Persistent]
        public float X
        {
            get { return this.m_Position.X; }
            set { this.m_Position.X = value; }
        }

        [Persistent]
        public float Y
        {
            get { return this.m_Position.Y; }
            set { this.m_Position.Y = value; }
        }

        [Persistent]
        public float Z
        {
            get { return this.m_Position.Z; }
            set { this.m_Position.Z = value; }
        }

        public Map Map
        {
            get { return WCell.RealmServer.Global.World.GetNonInstancedMap(this.MapId); }
        }

        public uint Phase { get; set; }
    }
}