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
            Unit owner = this.m_aura.Auras.Owner;
            owner.Dismount();
            this.displayId = owner.DisplayId;
            if (this.m_spellEffect.MiscValue > 0)
            {
                if (!NPCMgr.EntriesLoaded)
                    return;
                NPCEntry entry = NPCMgr.GetEntry((uint) this.m_spellEffect.MiscValue);
                if (entry == null)
                    TransformHandler.log.Warn("Transform spell {0} has invalid creature-id {1}",
                        (object) this.m_aura.Spell, (object) this.m_spellEffect.MiscValue);
                else
                    owner.Model = entry.GetRandomModel();
            }
            else
                TransformHandler.log.Warn("Transform spell {0} has no creature-id set", (object) this.m_aura.Spell);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.DisplayId = this.displayId;
        }
    }
}