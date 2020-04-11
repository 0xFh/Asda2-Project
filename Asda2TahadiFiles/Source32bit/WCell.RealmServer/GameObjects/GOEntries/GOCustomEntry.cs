using System;
using WCell.Util.Data;

namespace WCell.RealmServer.GameObjects.GOEntries
{
  public class GOCustomEntry : GOEntry
  {
    private GOUseHandler m_UseHandler;

    [NotPersistent]
    public GOUseHandler UseHandler
    {
      get { return m_UseHandler; }
      set
      {
        m_UseHandler = value;
        HandlerCreator = () => (GameObjectHandler) new CustomUseHandler(value);
      }
    }

    protected internal override void InitEntry()
    {
      if(Fields != null)
        return;
      Fields = new int[24];
    }
  }
}