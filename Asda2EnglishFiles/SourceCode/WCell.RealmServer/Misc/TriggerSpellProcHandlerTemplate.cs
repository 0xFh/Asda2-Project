using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Misc
{
    /// <summary>Triggers a spell on proc</summary>
    public class TriggerSpellProcHandlerTemplate : ProcHandlerTemplate
    {
        public Spell Spell { get; set; }

        public TriggerSpellProcHandlerTemplate(Spell spell, ProcTriggerFlags triggerFlags,
            ProcHitFlags hitFlags = ProcHitFlags.None, uint procChance = 100, int stackCount = 0)
            : this(spell, triggerFlags, (ProcValidator) null, hitFlags, procChance, stackCount)
        {
        }

        public TriggerSpellProcHandlerTemplate(Spell spell, ProcTriggerFlags triggerFlags,
            ProcValidator validator = null, ProcHitFlags hitFlags = ProcHitFlags.None, uint procChance = 100,
            int stackCount = 0)
            : base(triggerFlags, hitFlags, (ProcCallback) null, validator, procChance, stackCount)
        {
            this.Spell = spell;
            this.ProcAction = new ProcCallback(this.ProcSpell);
        }

        public bool ProcSpell(Unit creator, Unit triggerer, IUnitAction action)
        {
            SpellCast.ValidateAndTriggerNew(this.Spell, creator, (WorldObject) triggerer, (SpellChannel) null,
                (Item) null, action, (SpellEffect) null);
            return false;
        }
    }
}