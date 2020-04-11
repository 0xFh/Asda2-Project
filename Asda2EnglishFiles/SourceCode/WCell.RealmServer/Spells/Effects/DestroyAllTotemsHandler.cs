using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
    public class DestroyAllTotemsHandler : SpellEffectHandler
    {
        public DestroyAllTotemsHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }

        public override void Apply()
        {
            Character casterObject = this.m_cast.CasterObject as Character;
            int num = 0;
            if (casterObject == null || casterObject.Totems == null)
                return;
            foreach (NPC totem in casterObject.Totems)
            {
                if (totem != null)
                {
                    Spell spell = SpellHandler.Get(totem.CreationSpellId);
                    if (spell != null)
                    {
                        num += casterObject.BasePower * spell.PowerCostPercentage / 100 / 4;
                        totem.Delete();
                    }
                }
            }

            casterObject.Energize(num, (Unit) casterObject, this.Effect);
        }
    }
}