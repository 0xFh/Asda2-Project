using System;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
    internal class CastAnotherSpellHandler : SpellEffectHandler
    {
        public CastAnotherSpellHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }
        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            Random s_Random = new Random();
            var spell = Cast.Spell;
            var chance = s_Random.Next(0,100);
          
                if (spell.Id == 1916 || spell.Id == 2916 || spell.Id == 3916 || spell.Id == 4916 || spell.Id == 5916 || spell.Id == 6916 || spell.Id == 7916)
                {
                        chance = 8;
                        RunSpell((Unit)target, (uint)Effect.MiscValueB);
                        return;
                   

                }
                if (spell.Id == 1917 || spell.Id == 2917 || spell.Id == 3917 || spell.Id == 4917 || spell.Id == 5917 || spell.Id == 6917 || spell.Id == 7917)
                {
                    
                        RunSpell((Unit)target, (uint)Effect.MiscValueB);
                        return;
                    


                }
                if (spell.Id == 1909 || spell.Id == 2909 || spell.Id == 3909 || spell.Id == 4909 || spell.Id == 5909 || spell.Id == 6909 || spell.Id == 7909)
                {
                   
                        RunSpell((Unit)target, (uint)Effect.MiscValueB);
                        return;
                   
                }
            
            if (Effect.MiscValue != 0)
            {
                RunSpell((Unit)target, (uint)Effect.MiscValue);
            }
            if (Effect.MiscValueB != 0)
            {
               // chance = 8;
                RunSpell((Unit)target, (uint)Effect.MiscValueB);
            }
                
            if (Effect.MiscValueC != 0)
            {
                RunSpell((Unit)target, (uint)Effect.MiscValueC);
            }
                
        }

        void RunSpell(Unit target, uint spellId)
        {
            if(Util.Utility.Random(0,101)>Cast.Spell.ProcChance)
                return;
           
            Spell spell = SpellHandler.Get(spellId);
            
            if (spell != null)
            {
                Vector3 loc = target.Position;
                SpellCast.Trigger(m_cast.CasterUnit, spell, ref loc, target);
            }
        }
    }
}