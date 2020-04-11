using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Events.Asda2.Managers
{
  public static class Asda2EventMgr
  {
    public static void SendMessageToWorld(string message, params object[] p)
    {
      World.BroadcastMsg("Event Manager", string.Format(message, p), Color.LightPink);
    }
  }
}