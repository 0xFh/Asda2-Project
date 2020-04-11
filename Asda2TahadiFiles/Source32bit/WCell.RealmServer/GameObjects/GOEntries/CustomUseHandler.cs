using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.GOEntries
{
  internal class CustomUseHandler : GameObjectHandler
  {
    private readonly GOEntry.GOUseHandler Handler;

    public CustomUseHandler(GOEntry.GOUseHandler handler)
    {
      Handler = handler;
    }

    public override bool Use(Character user)
    {
      return Handler(m_go, user);
    }
  }
}