using System;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
    public struct WorldLocationStruct : IWorldLocation, IHasPosition
    {
        private Vector3 m_Position;
        private Map m_Map;
        private uint m_Phase;

        public WorldLocationStruct(MapId map, Vector3 pos, uint phase = 1)
        {
            this.m_Position = pos;
            this.m_Map = WCell.RealmServer.Global.World.GetNonInstancedMap(map);
            if (this.m_Map == null)
                throw new Exception("Invalid Map in WorldLocationStruct: " + (object) map);
            this.m_Phase = phase;
        }

        public WorldLocationStruct(Map map, Vector3 pos, uint phase = 1)
        {
            this.m_Position = pos;
            this.m_Map = map;
            this.m_Phase = phase;
        }

        public Vector3 Position
        {
            get { return this.m_Position; }
            set { this.m_Position = value; }
        }

        public Map Map
        {
            get { return this.m_Map; }
            set { this.m_Map = value; }
        }

        public uint Phase
        {
            get { return this.m_Phase; }
            set { this.m_Phase = value; }
        }

        public MapId MapId
        {
            get { return this.Map.Id; }
        }
    }
}