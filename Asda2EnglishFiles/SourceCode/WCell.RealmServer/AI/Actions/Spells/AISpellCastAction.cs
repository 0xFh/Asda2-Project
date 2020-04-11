using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.AI.Actions.Spells
{
    /// <summary>AI Action for casting a spell</summary>
    public class AISpellCastAction : AITargetedAction
    {
        protected Spell m_spell;

        public AISpellCastAction(Unit owner, Spell spell)
            : base(owner)
        {
            this.m_spell = spell;
        }

        public override void Start()
        {
            int num = (int) this.m_owner.SpellCast.Start(this.m_spell, false);
        }

        public override void Update()
        {
            if (this.m_owner.SpellCast.IsCasting)
                return;
            this.m_owner.Brain.StopCurrentAction();
        }

        public override void Stop()
        {
            if (!this.m_owner.IsUsingSpell || this.m_owner.SpellCast.Spell != this.m_spell)
                return;
            this.m_owner.CancelSpellCast();
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.Active; }
        }
    }
}