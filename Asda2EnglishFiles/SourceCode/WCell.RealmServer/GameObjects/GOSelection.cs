using System;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects
{
    public class GOSelection : IDisposable
    {
        private GameObject m_GO;
        private DynamicObject m_Marker;

        public GOSelection(GameObject go)
        {
            this.GO = go;
        }

        public GameObject GO
        {
            get
            {
                if (this.m_GO != null && !this.m_GO.IsInWorld)
                    return (GameObject) null;
                return this.m_GO;
            }
            set { this.m_GO = value; }
        }

        public DynamicObject Marker
        {
            get
            {
                if (this.m_Marker != null && !this.m_Marker.IsInWorld)
                    return (DynamicObject) null;
                return this.m_Marker;
            }
            set { this.m_Marker = value; }
        }

        public void Dispose()
        {
            this.GO = (GameObject) null;
            DynamicObject marker = this.Marker;
            if (marker == null)
                return;
            marker.Delete();
            this.Marker = (DynamicObject) null;
        }
    }
}