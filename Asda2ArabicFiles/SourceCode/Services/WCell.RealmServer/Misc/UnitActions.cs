using System;
using NLog;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;

namespace WCell.RealmServer.Misc
{
	#region IUnitAction
	/// <summary>
	/// Any kind of Action a Unit can perform
	/// </summary>
	public interface IUnitAction
	{
		/// <summary>
		/// The Attacker or Caster
		/// </summary>
		Unit Attacker { get; }

		/// <summary>
		/// Victim or Target or Receiver
		/// </summary>
		Unit Victim { get; }

		/// <summary>
		/// Whether this was a critical action (might be meaningless for some actions)
		/// </summary>
		bool IsCritical { get; }

		Spell Spell { get; }

		/// <summary>
		/// Reference count is used to support pooling
		/// </summary>
		int ReferenceCount
		{
			get;
			set;
		}
	}
	#endregion

	#region IDamageAction
	public interface IDamageAction : IUnitAction
	{
		SpellEffect SpellEffect
		{
			get;
		}

		int ActualDamage
		{
			get;
		}

		int Damage
		{
			get;
			set;
		}

		bool IsDot
		{
			get;
		}

		DamageSchool UsedSchool
		{
			get;
		}

		IAsda2Weapon Weapon
		{
			get;
		}
	}
	#endregion

	#region SimpleUnitAction
	public class SimpleUnitAction : IUnitAction
	{
		public Unit Attacker
		{
			get;
			set;
		}

		public Unit Victim
		{
			get;
			set;
		}

		public bool IsCritical
		{
			get;
			set;
		}

		public Spell Spell
		{
			get;
			set;
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		public int ReferenceCount
		{
			get { return 0; }
			set { }
		}
	}
	#endregion

	#region HealAction
	public class HealAction : SimpleUnitAction
	{
		public int Value
		{
			get;
			set;
		}

		/// <summary>
		/// Heal over time
		/// </summary>
		public bool IsHot
		{
			get;
			set;
		}
	}
	#endregion

	#region TrapTriggerAction
	public class TrapTriggerAction : SimpleUnitAction
	{
	}
	#endregion

	#region AuraRemovedAction
	public class AuraAction : IUnitAction
	{
		public Unit Attacker
		{
			get;
			set;
		}

		public Unit Victim
		{
			get;
			set;
		}

		public bool IsCritical
		{
			get { return false; }
		}

		public Aura Aura
		{
			get;
			set;
		}

		public Spell Spell
		{
			get { return Aura.Spell; }
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		public int ReferenceCount
		{
			get { return 0; }
			set { }
		}
	}
	#endregion

	#region SimpleDamageAction
	public class SimpleDamageAction : IDamageAction
	{
		public Unit Attacker
		{
			get { return null; }
		}

		public Unit Victim
		{
			get;
			set;
		}

		public SpellEffect SpellEffect
		{
			get { return null; }
		}

		public int Damage
		{
			get;
			set;
		}

		public int ActualDamage
		{
			get { return Damage; }
		}

		public bool IsDot
		{
			get { return false; }
		}

		public bool IsCritical
		{
			get { return false; }
		}

		public DamageSchool UsedSchool
		{
			get { return DamageSchool.Physical; }
		}

		public IAsda2Weapon Weapon
		{
			get { return null; }
		}

		public Spell Spell
		{
			get { return null; }
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		public int ReferenceCount
		{
			get { return 0; }
			set { }
		}
	}
	#endregion

	/// <summary>
	/// Contains all information related to any direct attack that deals positive damage
	/// </summary>
	public class DamageAction : IDamageAction
	{
		/// <summary>
		/// During Combat: The default delay in milliseconds between CombatTicks
		/// </summary>
		public static int DefaultCombatTickDelay = 600;

		public DamageAction(Unit attacker)
		{
			Attacker = attacker;
		}

		/// <summary>
		/// The Attacker or null if this is not an actual Attack
		/// </summary>
		public Unit Attacker
		{
			get;
			internal set;
		}

		/// <summary>
		/// The Unit that is being attacked
		/// </summary>
		public Unit Victim
		{
			get;
			set;
		}

		public void ModDamagePercent(int pct)
		{
			m_Damage += (m_Damage * pct + 50) / 100;
		}

		/// <summary>
		/// Returns the given percentage of the applied damage
		/// </summary>
		public int GetDamagePercent(int percent)
		{
			return (m_Damage * percent + 50) / 100;
		}

		private int m_Damage;

		public int Damage
		{
			get { return m_Damage; }
			set
			{
				if (value < 0)
				{
					// no negative damage
					value = 0;
				}
				m_Damage = value;
			}
		}

		public bool IsCritical
		{
			get;
			set;
		}

		/// <summary>
		/// Damage over time has different implications than normal damage
		/// </summary>
		public bool IsDot
		{
			get;
			set;
		}

		public IAsda2Weapon Weapon
		{
			get;
			set;
		}

		public SpellEffect SpellEffect
		{
			get;
			set;
		}

		public Spell Spell
		{
			get { return SpellEffect != null ? SpellEffect.Spell : null; }
		}

		public DamageSchoolMask Schools;

		/// <summary>
		/// Can only be modified in AddDamageMods, not later.
		/// Value between 0 and 100.
		/// </summary>
		public float ResistPct;

		public int Absorbed, Resisted, Blocked;

		public VictimState VictimState;

		public HitFlags HitFlags;

		private ProcHitFlags ProcHitFlags;

		/// <summary>
		/// Actions that are marked in use, will not be recycled
		/// </summary>
		public int ReferenceCount
		{
			get;
			set;
		}

		#region Situational Properties
		public DamageSchool UsedSchool
		{
			get;
			set;
		}

		/// <summary>
		/// White damage or strike ability (Heroic Strike, Ranged, Throw etc)
		/// </summary>
		public bool IsWeaponAttack
		{
			get
			{
				return Weapon != null &&
					   (SpellEffect == null || SpellEffect.Spell.IsPhysicalAbility);
			}
		}

		/// <summary>
		/// Any attack that involves a spell
		/// </summary>
		public bool IsSpellCast
		{
			get { return SpellEffect != null; }
		}

		/// <summary>
		/// Pure spell attack, no weapon involved
		/// </summary>
		public bool IsMagic
		{
			get { return !IsWeaponAttack && SpellEffect != null; }
		}

		public bool IsRangedAttack
		{
			get
			{
				return Weapon != null ? Weapon.IsRanged : false;
			}
		}

		public bool IsAutoshot
		{
			get
			{
				return Weapon != null && SpellEffect != null &&
					SpellEffect.Spell.IsAutoRepeating;
			}
		}

		public bool IsMeleeAttack
		{
			get
			{
				return Weapon != null ? !Weapon.IsRanged : false;
			}
		}

		public bool CanDodge
		{
			get { return SpellEffect == null || !SpellEffect.Spell.Attributes.HasFlag(SpellAttributes.CannotDodgeBlockParry); }
		}

		public bool CanBlockParry
		{
			get
			{
				return CanDodge && !Victim.IsStunned && Attacker.IsInFrontOf(Victim);
			}
		}

		/// <summary>
		/// An action can crit if there is no spell involved,
		/// the given spell is allowed to crit by default,
		/// or the Attacker has a modifier that allows the spell to crit.
		/// </summary>
		public bool CanCrit
		{
			get
			{
				return (SpellEffect == null ||
						((!Spell.AttributesExB.HasFlag(SpellAttributesExB.CannotCrit) && !IsDot)) ||
						(Attacker is Character && ((Character)Attacker).PlayerAuras.CanSpellCrit(SpellEffect.Spell)));
			}
		}

		public int ActualDamage
		{
			get
			{
			    var chr = Attacker as Character;
			    var victim = Victim as NPC;
                var victim2 = Victim as Character;
                if (chr != null && victim != null)
                {
                    if (chr.Level > Victim.Level + CharacterFormulas.MaxLvlMobCharDiff)
                    {
                        return 1;
                    }
                }
                //if (chr != null && victim2 != null)
                //{
                //   // var dmg1 = Damage & Absorbed & Resisted & Blocked;
                //    if (chr.IsAsda2BattlegroundInProgress)
                //    {
                //        Damage = Damage / 3 * 2;
                //        Absorbed = Absorbed * 3 / 2;
                //        Resisted = Resisted * 3 / 2 ;
                //        Blocked = Blocked * 3 / 2 ;

                //    }
                //    if (chr.IsAsda2Dueling)
                //    {
                //        Damage = Damage / 3 * 2;
                //        Absorbed = Absorbed * 3 / 2 ;
                //        Resisted = Resisted * 3 / 2;
                //        Blocked = Blocked * 3 / 2;
                        
                //    }
                //}
                /*if (chr.IsAsda2BattlegroundInProgress)
                {
                    Damage = Damage / 2;

                }
                if (chr.IsAsda2Dueling)
                {
                    Damage = Damage / 2;

                }*/
                var dmg = Damage - Absorbed - Resisted - Blocked;

              /* if (chr.IsAsda2BattlegroundInProgress)
                {
                    dmg = dmg / 2;
                    return dmg <= 0 ? 1 : dmg;
                }
                if (chr.IsAsda2Dueling)
                {
                    dmg = dmg / 2;
                    return dmg <= 0 ? 1 : dmg;
                }*/
                return dmg <= 0 ? 1 : dmg;
                
                
			}
		}
		#endregion

		#region Attack

	    /// <summary>
	    /// Does a melee/ranged/wand physical attack.
	    /// Calculates resistances/attributes (resilience, hit chance) and takes them into account.
	    /// </summary>
	    /// <returns>ProcHitFlags containing hit result</returns>
	    public ProcHitFlags DoAttack()
	    {
	        if (Victim == null)
	        {
	            LogManager.GetCurrentClassLogger().Error("{0} tried to attack with no Target selected.", Attacker);
	            return ProcHitFlags;
	        }

	        if (Victim.IsEvading)
	        {
	            Evade();
	            return ProcHitFlags;
	        }
	        if (Victim.IsImmune(UsedSchool) || Victim.IsInvulnerable)
	        {
	            MissImmune();
	            return ProcHitFlags;
	        }

	        if (CanCrit && Victim.IsSitting)
	        {
	            StrikeCritical();
	            return ProcHitFlags;
	        }

	        //hitinfo declarations
	        var hitChance = CalcHitChance();

	        var random = Utility.Random(1, 10000);
	        if (random > hitChance)
	        {
	            // missed the target
	            Miss();
	            return ProcHitFlags;
	        }
	        var block = CalcBlockChance();
	        if (random > (hitChance - block))
	        {
	            // block
	            Block();
	            return ProcHitFlags;
	        }
	        var critical = CalcCritChance();
	        if (random > (hitChance - critical - block))
	        {
	            // critical hit
	            StrikeCritical();
	            return ProcHitFlags;
	        }
	        // normal attack
	        StrikeNormal();
	        return ProcHitFlags;
	    }

	    #endregion

		#region Miss & Strike
		public void MissImmune()
		{
			Damage = 0;
			VictimState = VictimState.Immune;
			ProcHitFlags |= ProcHitFlags.Immune;
			DoStrike();
		}

		public void Miss()
		{
			Damage = 0;
			ProcHitFlags |= ProcHitFlags.Miss;
			DoStrike();
		}

		public void Dodge()
		{
			Damage = 0;
			VictimState = VictimState.Dodge;
			HitFlags = HitFlags.Miss;
			ProcHitFlags |= ProcHitFlags.Dodge;
			Blocked = 0;
			IsCritical = false;
			DoStrike();
		}

		public void Block()
		{
			HitFlags = HitFlags.PlayWoundAnimation | HitFlags.Block;
			VictimState = VictimState.Block;
			ProcHitFlags |= ProcHitFlags.Block;
			Blocked = CalcBlockDamage();
			if (Damage == Blocked)
			{
				ProcHitFlags |= ProcHitFlags.FullBlock;
			}
			IsCritical = false;
			DoStrike();
		}

		public void Evade()
		{
			Damage = 0;
			VictimState = VictimState.Evade;
			ProcHitFlags |= ProcHitFlags.Evade;
			DoStrike();
		}

		public void StrikeCritical()
		{
			IsCritical = Victim.StandState == StandState.Stand;
			SetCriticalDamage();
			HitFlags = HitFlags.PlayWoundAnimation | HitFlags.ResistType1 | HitFlags.ResistType2 | HitFlags.CriticalStrike;
			VictimState = VictimState.Wound;
			ProcHitFlags |= ProcHitFlags.CriticalHit;
			Blocked = 0;
			// Automatic double damage against sitting target - but doesn't proc crit abilities
			DoStrike();
		}

		public void SetCriticalDamage()
		{
			Damage = MathUtil.RoundInt(Attacker.CalcCritDamage(Damage, Victim, SpellEffect));
		}

		public void StrikeNormal()
		{
			HitFlags = HitFlags.PlayWoundAnimation;
			VictimState = VictimState.Wound;
			ProcHitFlags |= ProcHitFlags.NormalHit;
			Blocked = 0;
			IsCritical = false;
			DoStrike();
		}

		/// <summary>
		/// Strikes the target
		/// </summary>
		public void DoStrike()
		{
			if (Damage > 0)
			{
			    ResistPct = CalcResistPrc(Victim.Asda2Defence,Damage,0);
			    
			    if (ResistPct > 95)
				{
					ResistPct = 95;
				}
				if (ResistPct < 0)
				{
					ResistPct = 0;
				}

				Victim.DeathPrevention++;
				Attacker.DeathPrevention++;
				try
				{
					// add mods and call events
					AddDamageMods();
					Victim.OnDefend(this);
					Attacker.OnAttack(this);

					Resisted = MathUtil.RoundInt(ResistPct * Damage / 100f);
					


					Victim.DoRawDamage(this);
				}
				finally
				{
					Victim.DeathPrevention--;
					Attacker.DeathPrevention--;
				}
			}

			if (SpellEffect == null)
            {
                Asda2CombatHandler.SendAttackerStateUpdate(this);
			}

			TriggerProcOnStrike();
		}

	    public static float CalcResistPrc(float def, int srcDamage, float schoolResist)
	    {
	        var halfDmg = ((float) srcDamage)/2;
	        var fisrtPartDmg = halfDmg - def/2;
	        if (fisrtPartDmg < 0)
	            fisrtPartDmg = 0;
            var secPartDmg = (halfDmg + fisrtPartDmg) * CharacterFormulas.DeffenceRow / (CharacterFormulas.DeffenceRow + def);
	        var allDmg = (fisrtPartDmg < 0 ? 0 : fisrtPartDmg) + secPartDmg;
	        return (100-schoolResist)*((srcDamage -allDmg)/srcDamage);
	    }


	    private void TriggerProcOnStrike()
		{
			if (Weapon != null && SpellEffect == null)
			{
				var attackerProcTriggerFlags = ProcTriggerFlags.None;
				var victimProcTriggerFlags = ProcTriggerFlags.None;

				if (Weapon.IsMelee)
				{
					attackerProcTriggerFlags |= ProcTriggerFlags.DoneMeleeAutoAttack;
					victimProcTriggerFlags |= ProcTriggerFlags.ReceivedMeleeAutoAttack;
				}
				else if (Weapon.IsRanged)
				{
					attackerProcTriggerFlags |= ProcTriggerFlags.DoneRangedAutoAttack;
					victimProcTriggerFlags |= ProcTriggerFlags.ReceivedRangedAutoAttack;
				}

				if (Attacker != null && Attacker.IsAlive)
				{
					Attacker.Proc(attackerProcTriggerFlags, Victim, this, true, ProcHitFlags);
				}

				if (Victim != null && Victim.IsAlive)
				{
					Victim.Proc(victimProcTriggerFlags, Attacker, this, true, ProcHitFlags);
				}
			}
		}
		#endregion

		#region Chances
		/// <summary>
		/// Calculated in UnitUpdates.UpdateBlockChance
		/// </summary>
		public int CalcBlockDamage()
		{
			// Mobs should be able to block as well, right?
			if (!(Victim is Character))
			{
				// mobs can't block
				return 0;
			}

			var target = (Character)Victim;
		    var shield = target.Asda2Inventory.Equipment[(int) Asda2EquipmentSlots.Shild];
		    if (shield!= null)
		        return (int) (Damage*CharacterFormulas.CalcShieldBlockPrc(shield.Template.Quality,shield.Template.RequiredLevel));
			return 0;
		}

		/// <summary>
		/// Gives the chance to hit between 0-10000
		/// </summary>
		public int CalcHitChance()
		{
            return 10000;
		}

		/// <summary>
		/// Calculates the crit chance between 0-10000
		/// </summary>
		/// <returns>The crit chance after taking into account defense, weapon skills, resilience etc</returns>
		public int CalcCritChance()
		{
			if (!CanCrit)
			{
				return 0;
			}
            if (Attacker is NPC)
                return 1000;
            var chance = (int)Attacker.GetBaseCritChance(UsedSchool, Spell, Weapon) * 100;
		    return chance;
		}

		/// <summary>
		/// Calculates the block chance between 1-10000
		/// </summary>
		public int CalcBlockChance()
        {
            if (!(Victim is Character))
            {
                // mobs can't block
                return 0;
            }

            var target = (Character)Victim;
            var shield = target.Asda2Inventory.Equipment[(int)Asda2EquipmentSlots.Shild];
		    return shield != null ? 3000 : 0;
        }

		
		#endregion

		#region Damages
		/// <summary>
		/// Adds all damage boni and mali
		/// </summary>
		internal void AddDamageMods()
		{
			if (Attacker != null)
			{
				if (!IsDot)
				{
					// does not add to dot
					Damage = Attacker.GetFinalDamage(UsedSchool, Damage, Spell);
				}
				else if (SpellEffect != null)
				{
					// periodic damage mod
					Damage = Attacker.Auras.GetModifiedInt(SpellModifierType.PeriodicEffectValue, Spell, Damage);
				}
			}
		}
		#endregion

		#region Absorb
		public int Absorb(int absorbAmount, DamageSchoolMask schools)
		{
			if (absorbAmount <= 0)
			{
				return 0;
			}

			if (SpellEffect != null && Spell.AttributesExD.HasFlag(SpellAttributesExD.CannotBeAbsorbed))
			{
				return 0;
			}

			if (schools.HasAnyFlag(UsedSchool))
			{
				var value = Math.Min(Damage, absorbAmount);
				absorbAmount -= value;
				Absorbed += value;
			}
			return absorbAmount;
		}
		#endregion

		internal void Reset(Unit attacker, Unit target, IAsda2Weapon weapon)
		{
			Attacker = attacker;
			Victim = target;
			Weapon = weapon;
			ProcHitFlags = ProcHitFlags.None;
		}

		internal void OnFinished()
		{
			if (Attacker != null && Victim is NPC)
			{
				((NPC)(Victim)).ThreatCollection.AddNewIfNotExisted(Attacker);
			}
			ReferenceCount--;
			SpellEffect = null;
		}

		public override string ToString()
		{
			return string.Format("Attacker: {0}, Target: {1}, Spell: {2}, Damage: {3}", Attacker, Victim,
				SpellEffect != null ? SpellEffect.Spell : null, Damage);
		}
	}

	public interface IAttackEventHandler
	{
		/// <summary>
		/// Called before hit chance, damage etc is determined.
		/// This is not used for Spell attacks, since those only have a single "stage".
		/// NOT CURRENTLY IMPLEMENTED
		/// </summary>
		void OnBeforeAttack(DamageAction action);

		/// <summary>
		/// Called on the attacker, right before resistance is subtracted and final damage is evaluated
		/// </summary>
		void OnAttack(DamageAction action);

		/// <summary>
		/// Called on the defender, right before resistance is subtracted and final damage is evaluated
		/// </summary>
		void OnDefend(DamageAction action);
	}
}