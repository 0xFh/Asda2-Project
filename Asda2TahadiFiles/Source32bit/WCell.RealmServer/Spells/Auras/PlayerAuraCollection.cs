using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras.Handlers;

namespace WCell.RealmServer.Spells.Auras
{
  /// <summary>
  /// AuraCollection for Character objects.
  /// Contains a lot of modifications and bookkeeping that is not required for NPCs.
  /// </summary>
  public class PlayerAuraCollection : AuraCollection
  {
    /// <summary>Flat modifiers of spells</summary>
    internal readonly List<AddModifierEffectHandler> SpellModifiersFlat = new List<AddModifierEffectHandler>(5);

    /// <summary>Percent modifiers of spells</summary>
    internal readonly List<AddModifierEffectHandler> SpellModifiersPct = new List<AddModifierEffectHandler>(5);

    /// <summary>
    /// Mask of spells that are allowed to crit hit, although they are not allowed to, by default
    /// </summary>
    internal readonly uint[] CriticalStrikeEnabledMask = new uint[3];

    /// <summary>
    /// Set of Auras that are only applied when certain items are equipped
    /// </summary>
    private List<Aura> itemRestrictedAuras;

    /// <summary>
    /// Set of Auras that are only applied in certain shapeshift forms
    /// </summary>
    private List<Aura> shapeshiftRestrictedAuras;

    /// <summary>
    /// Set of Auras that are only applied in certain AuraStates
    /// </summary>
    private List<Aura> auraStateRestrictedAuras;

    /// <summary>
    /// Set of Auras which have effects that depend on other Auras
    /// </summary>
    private List<Aura> aurasWithAuraDependentEffects;

    /// <summary>
    /// Amount of currently added modifiers that require charges.
    /// If &gt; 0, will iterate over modifiers and remove charges after SpellCasts.
    /// </summary>
    public int ModifierWithChargesCount { get; protected set; }

    public PlayerAuraCollection(Character owner)
      : base(owner)
    {
    }

    public override void AddAura(Aura aura, bool start)
    {
      base.AddAura(aura, start);
      OnAuraAddedOrRemoved();
      if(aura.Spell.IsPassive)
      {
        if(aura.Spell.HasItemRequirements)
          ItemRestrictedAuras.Add(aura);
        if(aura.Spell.IsModalShapeshiftDependentAura)
          ShapeshiftRestrictedAuras.Add(aura);
        if(aura.Spell.RequiredCasterAuraState != AuraState.None)
          AuraStateRestrictedAuras.Add(aura);
      }

      if(!aura.Spell.HasAuraDependentEffects)
        return;
      AurasWithAuraDependentEffects.Add(aura);
    }

    protected internal override void Remove(Aura aura)
    {
      base.Remove(aura);
      OnAuraAddedOrRemoved();
      if(aura.Spell.IsPassive)
      {
        if(aura.Spell.HasItemRequirements)
          ItemRestrictedAuras.Remove(aura);
        if(aura.Spell.IsModalShapeshiftDependentAura)
          ShapeshiftRestrictedAuras.Remove(aura);
        if(aura.Spell.RequiredCasterAuraState != AuraState.None)
          AuraStateRestrictedAuras.Remove(aura);
      }

      if(!aura.Spell.HasAuraDependentEffects)
        return;
      AurasWithAuraDependentEffects.Remove(aura);
    }

    public void AddSpellModifierPercent(AddModifierEffectHandler modifier)
    {
      if(modifier.Charges > 0)
        ++ModifierWithChargesCount;
      SpellModifiersPct.Add(modifier);
      OnModifierChange(modifier);
      AuraHandler.SendModifierUpdate((Character) m_owner, modifier.SpellEffect, true);
    }

    public void AddSpellModifierFlat(AddModifierEffectHandler modifier)
    {
      if(modifier.Charges > 0)
        ++ModifierWithChargesCount;
      SpellModifiersFlat.Add(modifier);
      OnModifierChange(modifier);
      AuraHandler.SendModifierUpdate((Character) m_owner, modifier.SpellEffect, false);
    }

    public void RemoveSpellModifierPercent(AddModifierEffectHandler modifier)
    {
      if(modifier.Charges > 0)
        --ModifierWithChargesCount;
      OnModifierChange(modifier);
      AuraHandler.SendModifierUpdate((Character) m_owner, modifier.SpellEffect, true);
      SpellModifiersPct.Remove(modifier);
    }

    public void RemoveSpellModifierFlat(AddModifierEffectHandler modifier)
    {
      if(modifier.Charges > 0)
        --ModifierWithChargesCount;
      OnModifierChange(modifier);
      AuraHandler.SendModifierUpdate((Character) m_owner, modifier.SpellEffect, false);
      SpellModifiersFlat.Remove(modifier);
    }

    private void OnModifierChange(AddModifierEffectHandler modifier)
    {
      foreach(Aura aura in Owner.Auras)
      {
        if(aura.IsActivated && !aura.Spell.IsEnhancer && modifier.SpellEffect.MatchesSpell(aura.Spell))
          aura.ReApplyNonPeriodicEffects();
      }
    }

    /// <summary>
    /// Returns the modified value (modified by certain talent bonusses) of the given type for the given spell (as int)
    /// </summary>
    public override int GetModifiedInt(SpellModifierType type, Spell spell, int value)
    {
      int modifierFlat = GetModifierFlat(type, spell);
      int modifierPercent = GetModifierPercent(type, spell);
      return ((value + modifierFlat) * (100 + modifierPercent) + 50) / 100;
    }

    /// <summary>
    /// Returns the given value minus bonuses through certain talents, of the given type for the given spell (as int)
    /// </summary>
    public override int GetModifiedIntNegative(SpellModifierType type, Spell spell, int value)
    {
      int modifierFlat = GetModifierFlat(type, spell);
      int modifierPercent = GetModifierPercent(type, spell);
      return ((value - modifierFlat) * (100 - modifierPercent) + 50) / 100;
    }

    /// <summary>
    /// Returns the modified value (modified by certain talents) of the given type for the given spell (as float)
    /// </summary>
    public override float GetModifiedFloat(SpellModifierType type, Spell spell, float value)
    {
      int modifierFlat = GetModifierFlat(type, spell);
      int modifierPercent = GetModifierPercent(type, spell);
      return (float) ((value + (double) modifierFlat) * (1.0 + modifierPercent / 100.0));
    }

    /// <summary>
    /// Returns the percent modifier (through certain talents) of the given type for the given spell
    /// </summary>
    public int GetModifierPercent(SpellModifierType type, Spell spell)
    {
      int num = 0;
      for(int index = 0; index < SpellModifiersPct.Count; ++index)
      {
        AddModifierEffectHandler modifierEffectHandler = SpellModifiersPct[index];
        if((SpellModifierType) modifierEffectHandler.SpellEffect.MiscValue == type &&
           modifierEffectHandler.SpellEffect.MatchesSpell(spell))
          num += modifierEffectHandler.SpellEffect.ValueMin;
      }

      return num;
    }

    /// <summary>
    /// Returns the flat modifier (through certain talents) of the given type for the given spell
    /// </summary>
    public int GetModifierFlat(SpellModifierType type, Spell spell)
    {
      int num = 0;
      for(int index = 0; index < SpellModifiersFlat.Count; ++index)
      {
        AddModifierEffectHandler modifierEffectHandler = SpellModifiersFlat[index];
        if((SpellModifierType) modifierEffectHandler.SpellEffect.MiscValue == type &&
           modifierEffectHandler.SpellEffect.MatchesSpell(spell))
          num += modifierEffectHandler.SpellEffect.ValueMin;
      }

      return num;
    }

    public override void OnCasted(SpellCast cast)
    {
      Spell spell = cast.Spell;
      if(ModifierWithChargesCount <= 0)
        return;
      List<IAura> auraList = null;
      foreach(AddModifierEffectHandler modifierEffectHandler in SpellModifiersFlat)
      {
        SpellEffect spellEffect = modifierEffectHandler.SpellEffect;
        if(spellEffect.MatchesSpell(spell) && cast.Spell != spellEffect.Spell &&
           (cast.TriggerEffect == null || cast.TriggerEffect.Spell != spellEffect.Spell) &&
           modifierEffectHandler.Charges > 0)
        {
          --modifierEffectHandler.Charges;
          if(modifierEffectHandler.Charges < 1)
          {
            if(auraList == null)
              auraList = SpellCast.AuraListPool.Obtain();
            auraList.Add(modifierEffectHandler.Aura);
          }
        }
      }

      foreach(AddModifierEffectHandler modifierEffectHandler in SpellModifiersPct)
      {
        SpellEffect spellEffect = modifierEffectHandler.SpellEffect;
        if(spellEffect.MatchesSpell(spell) && cast.Spell != spellEffect.Spell &&
           (cast.TriggerEffect == null || cast.TriggerEffect.Spell != spellEffect.Spell) &&
           modifierEffectHandler.Charges > 0)
        {
          --modifierEffectHandler.Charges;
          if(modifierEffectHandler.Charges < 1)
          {
            if(auraList == null)
              auraList = SpellCast.AuraListPool.Obtain();
            auraList.Add(modifierEffectHandler.Aura);
          }
        }
      }

      if(auraList == null)
        return;
      foreach(IAura aura in auraList)
        aura.Remove(false);
      auraList.Clear();
      SpellCast.AuraListPool.Recycle(auraList);
    }

    private List<Aura> ItemRestrictedAuras
    {
      get
      {
        if(itemRestrictedAuras == null)
          itemRestrictedAuras = new List<Aura>(3);
        return itemRestrictedAuras;
      }
    }

    internal void OnEquip(Item item)
    {
    }

    internal void OnBeforeUnEquip(Item item)
    {
    }

    private List<Aura> ShapeshiftRestrictedAuras
    {
      get
      {
        if(shapeshiftRestrictedAuras == null)
          shapeshiftRestrictedAuras = new List<Aura>(3);
        return shapeshiftRestrictedAuras;
      }
    }

    internal void OnShapeshiftFormChanged()
    {
      if(shapeshiftRestrictedAuras == null)
        return;
      foreach(Aura shapeshiftRestrictedAura in shapeshiftRestrictedAuras)
      {
        if(shapeshiftRestrictedAura.Spell.RequiredShapeshiftMask != ShapeshiftMask.None)
          shapeshiftRestrictedAura.IsActivated = MayActivate(shapeshiftRestrictedAura);
        else if(shapeshiftRestrictedAura.Spell.HasShapeshiftDependentEffects)
          shapeshiftRestrictedAura.ReEvaluateNonPeriodicHandlerRequirements();
      }
    }

    private List<Aura> AuraStateRestrictedAuras
    {
      get
      {
        if(auraStateRestrictedAuras == null)
          auraStateRestrictedAuras = new List<Aura>(2);
        return auraStateRestrictedAuras;
      }
    }

    internal void OnAuraStateChanged()
    {
      if(auraStateRestrictedAuras == null)
        return;
      foreach(Aura stateRestrictedAura in auraStateRestrictedAuras)
        stateRestrictedAura.IsActivated = MayActivate(stateRestrictedAura);
    }

    private List<Aura> AurasWithAuraDependentEffects
    {
      get
      {
        if(aurasWithAuraDependentEffects == null)
          aurasWithAuraDependentEffects = new List<Aura>(2);
        return aurasWithAuraDependentEffects;
      }
    }

    internal void OnAuraAddedOrRemoved()
    {
      if(aurasWithAuraDependentEffects == null)
        return;
      foreach(Aura auraDependentEffect in aurasWithAuraDependentEffects)
      {
        foreach(AuraEffectHandler handler in auraDependentEffect.Handlers)
        {
          if(handler.SpellEffect.IsDependentOnOtherAuras)
            handler.IsActivated = MayActivate(handler);
        }
      }
    }

    /// <summary>
    /// Check all restrictions on the given Aura (optionally, exclude item check)
    /// </summary>
    private bool MayActivate(Aura aura, bool inclItemCheck)
    {
      return true;
    }

    protected internal override bool MayActivate(Aura aura)
    {
      if(MayActivate(aura, true))
        return true;
      return base.MayActivate(aura);
    }

    protected internal override bool MayActivate(AuraEffectHandler handler)
    {
      SpellEffect spellEffect = handler.SpellEffect;
      if((spellEffect.RequiredShapeshiftMask == ShapeshiftMask.None ||
          spellEffect.RequiredShapeshiftMask.HasAnyFlag(Owner.ShapeshiftMask)) &&
         (spellEffect.RequiredActivationAuras == null || ContainsAny(spellEffect.RequiredActivationAuras)))
        return true;
      return base.MayActivate(handler);
    }

    /// <summary>
    /// Returns wehther the given spell is allowed to crit, if it was not
    /// allowed to crit by default. (Due to Talents that override Spell behavior)
    /// </summary>
    public bool CanSpellCrit(Spell spell)
    {
      return spell.MatchesMask(CriticalStrikeEnabledMask);
    }
  }
}