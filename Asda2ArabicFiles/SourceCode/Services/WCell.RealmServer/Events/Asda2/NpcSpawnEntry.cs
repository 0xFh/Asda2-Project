using System.Collections.Generic;
using WCell.RealmServer.AI;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Events.Asda2
{
  [System.Serializable]
  public class NpcSpawnEntry
  {
    public NPCEntry NpcEntry { get; set; }
    public List<Vector3> MovingPoints = new List<Vector3>();
    public int TimeToSpawnMillis { get; set; }
    public BrainState BrainState { get; set; }
    public NpcSpawnEntry AddMovingPoint(int x, int y)
    {
      MovingPoints.Add(new Vector3(x, y));
      return this;
    }
  }
}