using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.GOEntries;

namespace WCell.RealmServer.GameObjects.Handlers
{
  /// <summary>GO Type 22</summary>
  public class SpellCasterHandler : GameObjectHandler
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    private int chargesLeft;

    protected internal override void Initialize(GameObject go)
    {
      base.Initialize(go);
      chargesLeft = ((GOSpellCasterEntry) m_go.Entry).Charges;
    }

    public override bool Use(Character user)
    {
      GOSpellCasterEntry entry = (GOSpellCasterEntry) m_go.Entry;
      if(entry.Spell == null)
        return false;
      m_go.SpellCast.Trigger(entry.Spell, user);
      if(chargesLeft == 1)
        m_go.Delete();
      else if(chargesLeft > 0)
        --chargesLeft;
      return true;
    }
  }
}