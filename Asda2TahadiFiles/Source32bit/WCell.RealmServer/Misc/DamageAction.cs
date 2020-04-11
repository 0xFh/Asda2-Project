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
      Attacker = attacker;
    }

    /// <summary>The Attacker or null if this is not an actual Attack</summary>
    public Unit Attacker { get; internal set; }

    /// <summary>The Unit that is being attacked</summary>
    public Unit Victim { get; set; }

    public void ModDamagePercent(int pct)
    {
      m_Damage += (m_Damage * pct + 50) / 100;
    }

    /// <summary>Returns the given percentage of the applied damage</summary>
    public int GetDamagePercent(int percent)
    {
      return (m_Damage * percent + 50) / 100;
    }

    public int Damage
    {
      get { return m_Damage; }
      set
      {
        if(value < 0)
          value = 0;
        m_Damage = value;
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
        if(SpellEffect == null)
          return null;
        return SpellEffect.Spell;
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
        if(Weapon == null)
          return false;
        if(SpellEffect != null)
          return SpellEffect.Spell.IsPhysicalAbility;
        return true;
      }
    }

    /// <summary>Any attack that involves a spell</summary>
    public bool IsSpellCast
    {
      get { return SpellEffect != null; }
    }

    /// <summary>Pure spell attack, no weapon involved</summary>
    public bool IsMagic
    {
      get
      {
        if(!IsWeaponAttack)
          return SpellEffect != null;
        return false;
      }
    }

    public bool IsRangedAttack
    {
      get
      {
        if(Weapon == null)
          return false;
        return Weapon.IsRanged;
      }
    }

    public bool IsAutoshot
    {
      get
      {
        if(Weapon != null && SpellEffect != null)
          return SpellEffect.Spell.IsAutoRepeating;
        return false;
      }
    }

    public bool IsMeleeAttack
    {
      get
      {
        if(Weapon == null)
          return false;
        return !Weapon.IsRanged;
      }
    }

    public bool CanDodge
    {
      get
      {
        if(SpellEffect != null)
          return !SpellEffect.Spell.Attributes.HasFlag(SpellAttributes.CannotDodgeBlockParry);
        return true;
      }
    }

    public bool CanBlockParry
    {
      get
      {
        if(CanDodge && !Victim.IsStunned)
          return Attacker.IsInFrontOf(Victim);
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
        if(SpellEffect == null ||
           !Spell.AttributesExB.HasFlag(SpellAttributesExB.CannotCrit) && !IsDot)
          return true;
        if(Attacker is Character)
          return ((Character) Attacker).PlayerAuras.CanSpellCrit(SpellEffect.Spell);
        return false;
      }
    }

    public int ActualDamage
    {
      get
      {
        Character attacker = Attacker as Character;
        NPC victim = Victim as NPC;
        if(attacker != null && victim != null &&
           attacker.Level > Victim.Level + CharacterFormulas.MaxLvlMobCharDiff)
          return 1;
        int num = Damage - Absorbed - Resisted - Blocked;
        if(num > 0)
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
      if(Victim == null)
      {
        LogManager.GetCurrentClassLogger()
          .Error("{0} tried to attack with no Target selected.", Attacker);
        return ProcHitFlags;
      }

      if(Victim.IsEvading)
      {
        Evade();
        return ProcHitFlags;
      }

      if(Victim.IsImmune(UsedSchool) || Victim.IsInvulnerable)
      {
        MissImmune();
        return ProcHitFlags;
      }

      if(CanCrit && Victim.IsSitting)
      {
        StrikeCritical();
        return ProcHitFlags;
      }

      int num1 = CalcHitChance();
      int num2 = Utility.Random(1, 10000);
      if(num2 > num1)
      {
        Miss();
        return ProcHitFlags;
      }

      int num3 = CalcBlockChance();
      if(num2 > num1 - num3)
      {
        Block();
        return ProcHitFlags;
      }

      int num4 = CalcCritChance();
      if(num2 > num1 - num4 - num3)
      {
        StrikeCritical();
        return ProcHitFlags;
      }

      StrikeNormal();
      return ProcHitFlags;
    }

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
      if(Damage == Blocked)
        ProcHitFlags |= ProcHitFlags.FullBlock;
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
      HitFlags = HitFlags.PlayWoundAnimation | HitFlags.ResistType1 | HitFlags.ResistType2 |
                 HitFlags.CriticalStrike;
      VictimState = VictimState.Wound;
      ProcHitFlags |= ProcHitFlags.CriticalHit;
      Blocked = 0;
      DoStrike();
    }

    public void SetCriticalDamage()
    {
      Damage =
        MathUtil.RoundInt(Attacker.CalcCritDamage(Damage, Victim, SpellEffect));
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

    /// <summary>Strikes the target</summary>
    public void DoStrike()
    {
      if(Damage > 0)
      {
        ResistPct = CalcResistPrc(Victim.Asda2Defence, Damage, 0.0f);
        if(ResistPct > 95.0)
          ResistPct = 95f;
        if(ResistPct < 0.0)
          ResistPct = 0.0f;
        ++Victim.DeathPrevention;
        ++Attacker.DeathPrevention;
        try
        {
          AddDamageMods();
          Victim.OnDefend(this);
          Attacker.OnAttack(this);
          Resisted = MathUtil.RoundInt((float) (ResistPct * (double) Damage / 100.0));
          Victim.DoRawDamage(this);
        }
        finally
        {
          --Victim.DeathPrevention;
          --Attacker.DeathPrevention;
        }
      }

      if(SpellEffect == null)
        Asda2CombatHandler.SendAttackerStateUpdate(this);
      TriggerProcOnStrike();
    }

    public static float CalcResistPrc(float def, int srcDamage, float schoolResist)
    {
      float num1 = srcDamage / 2f;
      float num2 = num1 - def / 2f;
      if(num2 < 0.0)
        num2 = 0.0f;
      float num3 = (float) ((num1 + (double) num2) * CharacterFormulas.DeffenceRow /
                            (CharacterFormulas.DeffenceRow + (double) def));
      float num4 = ((double) num2 < 0.0 ? 0.0f : num2) + num3;
      return (float) ((100.0 - schoolResist) *
                      ((srcDamage - (double) num4) / srcDamage));
    }

    private void TriggerProcOnStrike()
    {
      if(Weapon == null || SpellEffect != null)
        return;
      ProcTriggerFlags flags1 = ProcTriggerFlags.None;
      ProcTriggerFlags flags2 = ProcTriggerFlags.None;
      if(Weapon.IsMelee)
      {
        flags1 |= ProcTriggerFlags.DoneMeleeAutoAttack;
        flags2 |= ProcTriggerFlags.ReceivedMeleeAutoAttack;
      }
      else if(Weapon.IsRanged)
      {
        flags1 |= ProcTriggerFlags.DoneRangedAutoAttack;
        flags2 |= ProcTriggerFlags.ReceivedRangedAutoAttack;
      }

      if(Attacker != null && Attacker.IsAlive)
        Attacker.Proc(flags1, Victim, this, true, ProcHitFlags);
      if(Victim == null || !Victim.IsAlive)
        return;
      Victim.Proc(flags2, Attacker, this, true, ProcHitFlags);
    }

    /// <summary>Calculated in UnitUpdates.UpdateBlockChance</summary>
    public int CalcBlockDamage()
    {
      if(!(Victim is Character))
        return 0;
      Asda2Item asda2Item = ((Character) Victim).Asda2Inventory.Equipment[8];
      if(asda2Item != null)
        return (int) (Damage *
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
      if(!CanCrit)
        return 0;
      if(Attacker is NPC)
        return 1000;
      return (int) Attacker.GetBaseCritChance(UsedSchool, Spell, Weapon) * 100;
    }

    /// <summary>Calculates the block chance between 1-10000</summary>
    public int CalcBlockChance()
    {
      return !(Victim is Character) || ((Character) Victim).Asda2Inventory.Equipment[8] == null
        ? 0
        : 3000;
    }

    /// <summary>Adds all damage boni and mali</summary>
    internal void AddDamageMods()
    {
      if(Attacker == null)
        return;
      if(!IsDot)
      {
        Damage = Attacker.GetFinalDamage(UsedSchool, Damage, Spell);
      }
      else
      {
        if(SpellEffect == null)
          return;
        Damage =
          Attacker.Auras.GetModifiedInt(SpellModifierType.PeriodicEffectValue, Spell, Damage);
      }
    }

    public int Absorb(int absorbAmount, DamageSchoolMask schools)
    {
      if(absorbAmount <= 0 || SpellEffect != null &&
         Spell.AttributesExD.HasFlag(SpellAttributesExD.CannotBeAbsorbed))
        return 0;
      if(schools.HasAnyFlag(UsedSchool))
      {
        int num = Math.Min(Damage, absorbAmount);
        absorbAmount -= num;
        Absorbed += num;
      }

      return absorbAmount;
    }

    internal void Reset(Unit attacker, Unit target, IAsda2Weapon weapon)
    {
      Attacker = attacker;
      Victim = target;
      Weapon = weapon;
      ProcHitFlags = ProcHitFlags.None;
    }

    internal void OnFinished()
    {
      if(Attacker != null && Victim is NPC)
        ((NPC) Victim).ThreatCollection.AddNewIfNotExisted(Attacker);
      --ReferenceCount;
      SpellEffect = null;
    }

    public override string ToString()
    {
      return string.Format("Attacker: {0}, Target: {1}, Spell: {2}, Damage: {3}", (object) Attacker,
        (object) Victim,
        SpellEffect != null ? (object) SpellEffect.Spell : (object) (Spell) null,
        (object) Damage);
    }
  }
}