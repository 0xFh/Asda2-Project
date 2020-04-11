using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class TransformHandler : AuraEffectHandler
  {
    protected static Logger log = LogManager.GetCurrentClassLogger();
    private uint displayId;

    protected override void Apply()
    {
      Unit owner = m_aura.Auras.Owner;
      owner.Dismount();
      displayId = owner.DisplayId;
      if(m_spellEffect.MiscValue > 0)
      {
        if(!NPCMgr.EntriesLoaded)
          return;
        NPCEntry entry = NPCMgr.GetEntry((uint) m_spellEffect.MiscValue);
        if(entry == null)
          log.Warn("Transform spell {0} has invalid creature-id {1}",
            m_aura.Spell, m_spellEffect.MiscValue);
        else
          owner.Model = entry.GetRandomModel();
      }
      else
        log.Warn("Transform spell {0} has no creature-id set", m_aura.Spell);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.DisplayId = displayId;
    }
  }
}