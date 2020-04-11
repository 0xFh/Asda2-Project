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
      GO = go;
    }

    public GameObject GO
    {
      get
      {
        if(m_GO != null && !m_GO.IsInWorld)
          return null;
        return m_GO;
      }
      set { m_GO = value; }
    }

    public DynamicObject Marker
    {
      get
      {
        if(m_Marker != null && !m_Marker.IsInWorld)
          return null;
        return m_Marker;
      }
      set { m_Marker = value; }
    }

    public void Dispose()
    {
      GO = null;
      DynamicObject marker = Marker;
      if(marker == null)
        return;
      marker.Delete();
      Marker = null;
    }
  }
}