using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	/// <summary>
	/// Do flat damage to any attacker
	/// </summary>
	public class DamageShieldEffectHandler : AttackEventEffectHandler
    {
        protected override void Apply()
        {
            if (SpellEffect.MiscValueB == 100)//from Kings rage
            {
                var targets = Owner.GetObjectsInRadius(8, ObjectTypes.Unit, false);
                foreach (var worldObject in targets)
                {
                    var unit = worldObject as Unit;
                    if (unit == null) continue;
                    if (!unit.IsHostileWith(Owner))
                        continue;
                    var spell = SpellHandler.Get(74);//stun 3sec
                    spell.Duration = 6000;
                    unit.Auras.CreateAndStartAura(Owner.SharedReference, spell, false);
                    spell.Duration = 3000;
                }
            }
            base.Apply();
        }

		public override void OnBeforeAttack(DamageAction action)
		{
			// do nothing
		}

		public override void OnAttack(DamageAction action)
		{
			// do nothing
		}

		public override void OnDefend(DamageAction action)
		{
			action.Victim.AddMessage(() =>
			{
				if (action.Victim.MayAttack(action.Attacker))
				{
					action.Attacker.DealSpellDamage(action.Victim, SpellEffect, action.Damage*(SpellEffect.MiscValue/100));
                    WCell.RealmServer.Handlers.Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(m_aura.CasterUnit as Character, Owner as Character, Owner as NPC, action.ActualDamage);
				}
			});
            if (action.Spell.SpellId == SpellId.MasterofSurvivalRank1)
            {
                var chr = m_aura.CasterUnit as Character;
                action.Victim.Heal(action.Damage * SpellEffect.MiscValue / 100);
            }
            //if(SpellEffect.MiscValueB == 0)
            //{
            //    action.Attacker.DealSpellDamage(action.Victim, SpellEffect, action.Damage * (SpellEffect.MiscValue / 100));
            //    WCell.RealmServer.Handlers.Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(m_aura.CasterUnit as Character, Owner as Character, Owner as NPC, action.ActualDamage);

            //action.Resisted = action.Damage;

            //}


            else if(SpellEffect.MiscValueB == 20)
            {
                var chr = m_aura.CasterUnit as Character;
                if(chr!=null && chr.IsInGroup)
                {
                    foreach (var member in chr.Group)
                    {
                        member.Character.Heal(action.Damage*SpellEffect.MiscValue/100);
                    }
                }
            }
           
		}
	}

    public class DragonSlayerEffectHandler : AttackEventEffectHandler
    {
        public override void OnBeforeAttack(DamageAction action)
        {
            
        }
        public override void OnAttack(DamageAction action)
        {
            if (action.Spell == null)
                return;
            action.Damage = (int)(action.Damage * 1.0f );
            var spell = SpellHandler.Get(74);//stun 3sec
            action.Victim.Auras.CreateAndStartAura(Owner.SharedReference, spell, false);
            m_aura.Cancel();
        }
    }
    public class ExplosiveArrowEffectHandler : AttackEventEffectHandler
    {
        public override void OnBeforeAttack(DamageAction action)
        {

        }
        public override void OnAttack(DamageAction action)
        {
            if (action.Spell == null ||action.SpellEffect.AuraType==AuraType.ExplosiveArrow)
                return;
            var targets = action.Victim.GetObjectsInRadius(6, ObjectTypes.Unit, false);
            foreach (var worldObject in targets)
            {
                if(!worldObject.IsHostileWith(action.Attacker))
                    continue;
                var unit = worldObject as Unit;
                if (unit == null) continue;
                var a = unit.DealSpellDamage(action.Attacker, SpellEffect, (int)(action.Attacker.RandomDamage * SpellEffect.MiscValue / 190), true, true, false, false);
                if (a == null)
                    continue;
                WCell.RealmServer.Handlers.Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(m_aura.CasterUnit as Character, Owner as Character, unit as NPC, a.ActualDamage);
                action.OnFinished();
            }
        }
    }
    public class ExploitBloodEffectHandler : AttackEventEffectHandler
    {
        public override void OnBeforeAttack(DamageAction action)
        {

        }
        public override void OnDefend(DamageAction action)
        {
            if (action.Spell == null)
                return;
            action.Attacker.Heal((int)(action.ActualDamage * 0.3f));
            base.OnDefend(action);
        }
    }
    public class AbsorbMagicEffectHandler : AttackEventEffectHandler
    {
        public override void OnDefend(DamageAction action)
        {
            if (action.Spell == null || action.Schools.HasFlag(DamageSchoolMask.Physical))
                return;
            action.Victim.Heal(action.ActualDamage);
            action.Resisted = 100;
            base.OnDefend(action);
        }
    }
    public class FlashLightEffectHandler : AttackEventEffectHandler
    {
        public override void OnBeforeAttack(DamageAction action)
        {
            
        }
        public override void OnAttack(DamageAction action)
        {
            if (action.Spell == null)
                return;
            if (Util.Utility.Random(0, 100000) < 2000)
            {
                var spell = SpellHandler.Get(SpellId.Silence10Rank7FromWindSlasher); //silens
                action.Victim.Auras.CreateAndStartAura(Owner.SharedReference, spell, false);
            }
        }
    }
    public class SurpriseEffectHandler : AttackEventEffectHandler
    {
        public override void OnBeforeAttack(DamageAction action)
        {

        }
        public override void OnAttack(DamageAction action)
        {
            if (action.Spell == null)
                return;
            action.Damage = (int)(action.Damage * 1.0f);
            var spell = SpellHandler.Get(SpellId.Silence10Rank7FromWindSlasher); //silens
            action.Victim.Auras.CreateAndStartAura(Owner.SharedReference, spell, false);
            Aura.Cancel();
        }
    }
}