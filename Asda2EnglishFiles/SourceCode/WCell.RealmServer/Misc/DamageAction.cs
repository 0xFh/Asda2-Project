using NLog;
using System;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.RealmServer.Misc
{
    /// <summary>
    /// Contains all information related to any direct attack that deals positive damage
    /// </summary>
    public class DamageAction : IDamageAction, IUnitAction
    {
        /// <summary>
        /// During Combat: The default delay in milliseconds between CombatTicks
        /// </summary>
        public static int DefaultCombatTickDelay = 600;

        private int m_Damage;
        public DamageSchoolMask Schools;

        /// <summary>
        /// Can only be modified in AddDamageMods, not later.
        /// Value between 0 and 100.
        /// </summary>
        public float ResistPct;

        public int Absorbed;
        public int Resisted;
        public int Blocked;
        public VictimState VictimState;
        public HitFlags HitFlags;
        private ProcHitFlags ProcHitFlags;

        public DamageAction(Unit attacker)
        {
            this.Attacker = attacker;
        }

        /// <summary>The Attacker or null if this is not an actual Attack</summary>
        public Unit Attacker { get; internal set; }

        /// <summary>The Unit that is being attacked</summary>
        public Unit Victim { get; set; }

        public void ModDamagePercent(int pct)
        {
            this.m_Damage += (this.m_Damage * pct + 50) / 100;
        }

        /// <summary>Returns the given percentage of the applied damage</summary>
        public int GetDamagePercent(int percent)
        {
            return (this.m_Damage * percent + 50) / 100;
        }

        public int Damage
        {
            get { return this.m_Damage; }
            set
            {
                if (value < 0)
                    value = 0;
                this.m_Damage = value;
            }
        }

        public bool IsCritical { get; set; }

        /// <summary>
        /// Damage over time has different implications than normal damage
        /// </summary>
        public bool IsDot { get; set; }

        public IAsda2Weapon Weapon { get; set; }

        public SpellEffect SpellEffect { get; set; }

        public Spell Spell
        {
            get
            {
                if (this.SpellEffect == null)
                    return (Spell) null;
                return this.SpellEffect.Spell;
            }
        }

        /// <summary>Actions that are marked in use, will not be recycled</summary>
        public int ReferenceCount { get; set; }

        public DamageSchool UsedSchool { get; set; }

        /// <summary>
        /// White damage or strike ability (Heroic Strike, Ranged, Throw etc)
        /// </summary>
        public bool IsWeaponAttack
        {
            get
            {
                if (this.Weapon == null)
                    return false;
                if (this.SpellEffect != null)
                    return this.SpellEffect.Spell.IsPhysicalAbility;
                return true;
            }
        }

        /// <summary>Any attack that involves a spell</summary>
        public bool IsSpellCast
        {
            get { return this.SpellEffect != null; }
        }

        /// <summary>Pure spell attack, no weapon involved</summary>
        public bool IsMagic
        {
            get
            {
                if (!this.IsWeaponAttack)
                    return this.SpellEffect != null;
                return false;
            }
        }

        public bool IsRangedAttack
        {
            get
            {
                if (this.Weapon == null)
                    return false;
                return this.Weapon.IsRanged;
            }
        }

        public bool IsAutoshot
        {
            get
            {
                if (this.Weapon != null && this.SpellEffect != null)
                    return this.SpellEffect.Spell.IsAutoRepeating;
                return false;
            }
        }

        public bool IsMeleeAttack
        {
            get
            {
                if (this.Weapon == null)
                    return false;
                return !this.Weapon.IsRanged;
            }
        }

        public bool CanDodge
        {
            get
            {
                if (this.SpellEffect != null)
                    return !this.SpellEffect.Spell.Attributes.HasFlag((Enum) SpellAttributes.CannotDodgeBlockParry);
                return true;
            }
        }

        public bool CanBlockParry
        {
            get
            {
                if (this.CanDodge && !this.Victim.IsStunned)
                    return this.Attacker.IsInFrontOf((WorldObject) this.Victim);
                return false;
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
                if (this.SpellEffect == null ||
                    !this.Spell.AttributesExB.HasFlag((Enum) SpellAttributesExB.CannotCrit) && !this.IsDot)
                    return true;
                if (this.Attacker is Character)
                    return ((Character) this.Attacker).PlayerAuras.CanSpellCrit(this.SpellEffect.Spell);
                return false;
            }
        }

        public int ActualDamage
        {
            get
            {
                Character attacker = this.Attacker as Character;
                NPC victim = this.Victim as NPC;
                if (attacker != null && victim != null &&
                    attacker.Level > this.Victim.Level + CharacterFormulas.MaxLvlMobCharDiff)
                    return 1;
                int num = this.Damage - this.Absorbed - this.Resisted - this.Blocked;
                if (num > 0)
                    return num;
                return 1;
            }
        }

        /// <summary>
        /// Does a melee/ranged/wand physical attack.
        /// Calculates resistances/attributes (resilience, hit chance) and takes them into account.
        /// </summary>
        /// <returns>ProcHitFlags containing hit result</returns>
        public ProcHitFlags DoAttack()
        {
            if (this.Victim == null)
            {
                LogManager.GetCurrentClassLogger()
                    .Error("{0} tried to attack with no Target selected.", (object) this.Attacker);
                return this.ProcHitFlags;
            }

            if (this.Victim.IsEvading)
            {
                this.Evade();
                return this.ProcHitFlags;
            }

            if (this.Victim.IsImmune(this.UsedSchool) || this.Victim.IsInvulnerable)
            {
                this.MissImmune();
                return this.ProcHitFlags;
            }

            if (this.CanCrit && this.Victim.IsSitting)
            {
                this.StrikeCritical();
                return this.ProcHitFlags;
            }

            int num1 = this.CalcHitChance();
            int num2 = Utility.Random(1, 10000);
            if (num2 > num1)
            {
                this.Miss();
                return this.ProcHitFlags;
            }

            int num3 = this.CalcBlockChance();
            if (num2 > num1 - num3)
            {
                this.Block();
                return this.ProcHitFlags;
            }

            int num4 = this.CalcCritChance();
            if (num2 > num1 - num4 - num3)
            {
                this.StrikeCritical();
                return this.ProcHitFlags;
            }

            this.StrikeNormal();
            return this.ProcHitFlags;
        }

        public void MissImmune()
        {
            this.Damage = 0;
            this.VictimState = VictimState.Immune;
            this.ProcHitFlags |= ProcHitFlags.Immune;
            this.DoStrike();
        }

        public void Miss()
        {
            this.Damage = 0;
            this.ProcHitFlags |= ProcHitFlags.Miss;
            this.DoStrike();
        }

        public void Dodge()
        {
            this.Damage = 0;
            this.VictimState = VictimState.Dodge;
            this.HitFlags = HitFlags.Miss;
            this.ProcHitFlags |= ProcHitFlags.Dodge;
            this.Blocked = 0;
            this.IsCritical = false;
            this.DoStrike();
        }

        public void Block()
        {
            this.HitFlags = HitFlags.PlayWoundAnimation | HitFlags.Block;
            this.VictimState = VictimState.Block;
            this.ProcHitFlags |= ProcHitFlags.Block;
            this.Blocked = this.CalcBlockDamage();
            if (this.Damage == this.Blocked)
                this.ProcHitFlags |= ProcHitFlags.FullBlock;
            this.IsCritical = false;
            this.DoStrike();
        }

        public void Evade()
        {
            this.Damage = 0;
            this.VictimState = VictimState.Evade;
            this.ProcHitFlags |= ProcHitFlags.Evade;
            this.DoStrike();
        }

        public void StrikeCritical()
        {
            this.IsCritical = this.Victim.StandState == StandState.Stand;
            this.SetCriticalDamage();
            this.HitFlags = HitFlags.PlayWoundAnimation | HitFlags.ResistType1 | HitFlags.ResistType2 |
                            HitFlags.CriticalStrike;
            this.VictimState = VictimState.Wound;
            this.ProcHitFlags |= ProcHitFlags.CriticalHit;
            this.Blocked = 0;
            this.DoStrike();
        }

        public void SetCriticalDamage()
        {
            this.Damage =
                MathUtil.RoundInt(this.Attacker.CalcCritDamage((float) this.Damage, this.Victim, this.SpellEffect));
        }

        public void StrikeNormal()
        {
            this.HitFlags = HitFlags.PlayWoundAnimation;
            this.VictimState = VictimState.Wound;
            this.ProcHitFlags |= ProcHitFlags.NormalHit;
            this.Blocked = 0;
            this.IsCritical = false;
            this.DoStrike();
        }

        /// <summary>Strikes the target</summary>
        public void DoStrike()
        {
            if (this.Damage > 0)
            {
                this.ResistPct = DamageAction.CalcResistPrc(this.Victim.Asda2Defence, this.Damage, 0.0f);
                if ((double) this.ResistPct > 95.0)
                    this.ResistPct = 95f;
                if ((double) this.ResistPct < 0.0)
                    this.ResistPct = 0.0f;
                ++this.Victim.DeathPrevention;
                ++this.Attacker.DeathPrevention;
                try
                {
                    this.AddDamageMods();
                    this.Victim.OnDefend(this);
                    this.Attacker.OnAttack(this);
                    this.Resisted = MathUtil.RoundInt((float) ((double) this.ResistPct * (double) this.Damage / 100.0));
                    this.Victim.DoRawDamage((IDamageAction) this);
                }
                finally
                {
                    --this.Victim.DeathPrevention;
                    --this.Attacker.DeathPrevention;
                }
            }

            if (this.SpellEffect == null)
                Asda2CombatHandler.SendAttackerStateUpdate(this);
            this.TriggerProcOnStrike();
        }

        public static float CalcResistPrc(float def, int srcDamage, float schoolResist)
        {
            float num1 = (float) srcDamage / 2f;
            float num2 = num1 - def / 2f;
            if ((double) num2 < 0.0)
                num2 = 0.0f;
            float num3 = (float) (((double) num1 + (double) num2) * (double) CharacterFormulas.DeffenceRow /
                                  ((double) CharacterFormulas.DeffenceRow + (double) def));
            float num4 = ((double) num2 < 0.0 ? 0.0f : num2) + num3;
            return (float) ((100.0 - (double) schoolResist) *
                            (((double) srcDamage - (double) num4) / (double) srcDamage));
        }

        private void TriggerProcOnStrike()
        {
            if (this.Weapon == null || this.SpellEffect != null)
                return;
            ProcTriggerFlags flags1 = ProcTriggerFlags.None;
            ProcTriggerFlags flags2 = ProcTriggerFlags.None;
            if (this.Weapon.IsMelee)
            {
                flags1 |= ProcTriggerFlags.DoneMeleeAutoAttack;
                flags2 |= ProcTriggerFlags.ReceivedMeleeAutoAttack;
            }
            else if (this.Weapon.IsRanged)
            {
                flags1 |= ProcTriggerFlags.DoneRangedAutoAttack;
                flags2 |= ProcTriggerFlags.ReceivedRangedAutoAttack;
            }

            if (this.Attacker != null && this.Attacker.IsAlive)
                this.Attacker.Proc(flags1, this.Victim, (IUnitAction) this, true, this.ProcHitFlags);
            if (this.Victim == null || !this.Victim.IsAlive)
                return;
            this.Victim.Proc(flags2, this.Attacker, (IUnitAction) this, true, this.ProcHitFlags);
        }

        /// <summary>Calculated in UnitUpdates.UpdateBlockChance</summary>
        public int CalcBlockDamage()
        {
            if (!(this.Victim is Character))
                return 0;
            Asda2Item asda2Item = ((Character) this.Victim).Asda2Inventory.Equipment[8];
            if (asda2Item != null)
                return (int) ((double) this.Damage *
                              CharacterFormulas.CalcShieldBlockPrc(asda2Item.Template.Quality,
                                  asda2Item.Template.RequiredLevel));
            return 0;
        }

        /// <summary>Gives the chance to hit between 0-10000</summary>
        public int CalcHitChance()
        {
            return 10000;
        }

        /// <summary>Calculates the crit chance between 0-10000</summary>
        /// <returns>The crit chance after taking into account defense, weapon skills, resilience etc</returns>
        public int CalcCritChance()
        {
            if (!this.CanCrit)
                return 0;
            if (this.Attacker is NPC)
                return 1000;
            return (int) this.Attacker.GetBaseCritChance(this.UsedSchool, this.Spell, this.Weapon) * 100;
        }

        /// <summary>Calculates the block chance between 1-10000</summary>
        public int CalcBlockChance()
        {
            return !(this.Victim is Character) || ((Character) this.Victim).Asda2Inventory.Equipment[8] == null
                ? 0
                : 3000;
        }

        /// <summary>Adds all damage boni and mali</summary>
        internal void AddDamageMods()
        {
            if (this.Attacker == null)
                return;
            if (!this.IsDot)
            {
                this.Damage = this.Attacker.GetFinalDamage(this.UsedSchool, this.Damage, this.Spell);
            }
            else
            {
                if (this.SpellEffect == null)
                    return;
                this.Damage =
                    this.Attacker.Auras.GetModifiedInt(SpellModifierType.PeriodicEffectValue, this.Spell, this.Damage);
            }
        }

        public int Absorb(int absorbAmount, DamageSchoolMask schools)
        {
            if (absorbAmount <= 0 || this.SpellEffect != null &&
                this.Spell.AttributesExD.HasFlag((Enum) SpellAttributesExD.CannotBeAbsorbed))
                return 0;
            if (schools.HasAnyFlag(this.UsedSchool))
            {
                int num = Math.Min(this.Damage, absorbAmount);
                absorbAmount -= num;
                this.Absorbed += num;
            }

            return absorbAmount;
        }

        internal void Reset(Unit attacker, Unit target, IAsda2Weapon weapon)
        {
            this.Attacker = attacker;
            this.Victim = target;
            this.Weapon = weapon;
            this.ProcHitFlags = ProcHitFlags.None;
        }

        internal void OnFinished()
        {
            if (this.Attacker != null && this.Victim is NPC)
                ((NPC) this.Victim).ThreatCollection.AddNewIfNotExisted(this.Attacker);
            --this.ReferenceCount;
            this.SpellEffect = (SpellEffect) null;
        }

        public override string ToString()
        {
            return string.Format("Attacker: {0}, Target: {1}, Spell: {2}, Damage: {3}", (object) this.Attacker,
                (object) this.Victim,
                this.SpellEffect != null ? (object) this.SpellEffect.Spell : (object) (Spell) null,
                (object) this.Damage);
        }
    }
}