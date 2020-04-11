using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Achievements;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.ObjectPools;

namespace WCell.RealmServer.Spells
{
  /// <summary>
  /// 
  /// </summary>
  public abstract class SpellCollection : IEnumerable<Spell>, IEnumerable
  {
    public static readonly ObjectPool<List<Spell>> SpellListPool =
      ObjectPoolMgr.CreatePool(() => new List<Spell>(), true);

    /// <summary>All spells by id</summary>
    protected Dictionary<SpellId, Spell> m_byId;

    /// <summary>All spells by id</summary>
    protected Dictionary<short, Spell> m_byRealId;

    /// <summary>
    /// Additional effects to be triggered when casting certain Spells
    /// </summary>
    private List<AddTargetTriggerHandler> m_TargetTriggers;

    protected SpellCollection()
    {
      m_byId = new Dictionary<SpellId, Spell>();
      m_byRealId = new Dictionary<short, Spell>();
    }

    protected virtual void Initialize(Unit owner)
    {
      Owner = owner;
    }

    protected internal virtual void Recycle()
    {
      Owner = null;
      m_byId.Clear();
      m_byRealId.Clear();
      if(m_TargetTriggers == null)
        return;
      m_TargetTriggers.Clear();
    }

    /// <summary>Required by SpellCollection</summary>
    public Unit Owner { get; protected internal set; }

    /// <summary>The amount of Spells in this Collection</summary>
    public int Count
    {
      get { return m_byId.Count; }
    }

    public bool HasSpells
    {
      get { return m_byId.Count > 0; }
    }

    public IEnumerable<Spell> AllSpells
    {
      get { return m_byId.Values; }
    }

    /// <summary>
    /// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
    /// </summary>
    public void AddSpell(uint spellId)
    {
      AddSpell(SpellHandler.ById[spellId]);
    }

    /// <summary>
    /// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
    /// </summary>
    public void AddSpell(SpellId spellId)
    {
      if(Contains(spellId))
        return;
      AddSpell(SpellHandler.Get(spellId));
    }

    /// <summary>
    /// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
    /// </summary>
    public virtual void AddSpell(Spell spell)
    {
      m_byId[spell.SpellId] = spell;
      m_byRealId[spell.RealId] = spell;
      OnAdd(spell);
    }

    protected void OnAdd(Spell spell)
    {
      if(spell.IsPassive)
        Owner.SpellCast.TriggerSelf(spell);
      if(spell.AdditionallyTaughtSpells.Count > 0)
      {
        foreach(Spell additionallyTaughtSpell in spell.AdditionallyTaughtSpells)
          AddSpell(additionallyTaughtSpell);
      }

      if(!(Owner is Character))
        return;
      ((Character) Owner).Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.LearnSpell,
        spell.Id, 0U, null);
    }

    /// <summary>
    /// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
    /// </summary>
    public void AddSpell(IEnumerable<SpellId> spells)
    {
      foreach(SpellId spell in spells)
        AddSpell(spell);
    }

    /// <summary>
    /// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
    /// </summary>
    public void AddSpell(params SpellId[] spells)
    {
      foreach(SpellId spell in spells)
        AddSpell(spell);
    }

    /// <summary>
    /// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
    /// </summary>
    public void AddSpell(IEnumerable<Spell> spells)
    {
      foreach(Spell spell in spells)
        AddSpell(spell);
    }

    /// <summary>
    /// Adds the spell without doing any further checks or adding any spell-related skills or showing animations
    /// </summary>
    public void OnlyAdd(SpellId id)
    {
      Spell spell = SpellHandler.ById.Get((uint) id);
      m_byId.Add(id, spell);
      m_byRealId.Add(spell.RealId, spell);
    }

    public void OnlyAdd(Spell spell)
    {
      if(!m_byId.ContainsKey(spell.SpellId))
        m_byId.Add(spell.SpellId, spell);
      if(m_byRealId.ContainsKey(spell.RealId))
        return;
      m_byRealId.Add(spell.RealId, spell);
    }

    public bool Contains(uint id)
    {
      return m_byId.ContainsKey((SpellId) id);
    }

    public bool Contains(SpellId id)
    {
      return m_byId.ContainsKey(id);
    }

    public Spell this[SpellId id]
    {
      get
      {
        Spell spell;
        m_byId.TryGetValue(id, out spell);
        return spell;
      }
    }

    public Spell this[uint id]
    {
      get
      {
        Spell spell;
        m_byId.TryGetValue((SpellId) id, out spell);
        return spell;
      }
    }

    /// <summary>
    /// Gets the highest rank of the line that this SpellCollection contains
    /// </summary>
    public Spell GetHighestRankOf(SpellLineId lineId)
    {
      return GetHighestRankOf(lineId.GetLine());
    }

    /// <summary>
    /// Gets the highest rank of the line that this SpellCollection contains
    /// </summary>
    public Spell GetHighestRankOf(SpellLine line)
    {
      Spell spell = line.HighestRank;
      while(!Contains(spell.SpellId))
      {
        if((spell = spell.PreviousRank) == null)
          return null;
      }

      return spell;
    }

    public void Remove(SpellId spellId)
    {
      Replace(SpellHandler.Get(spellId), null);
    }

    public bool Remove(uint spellId)
    {
      Remove((SpellId) spellId);
      return true;
    }

    public virtual bool Remove(Spell spell)
    {
      return Replace(spell, null);
    }

    public virtual void Clear()
    {
      foreach(Spell spell in m_byId.Values)
      {
        if(spell.IsPassive)
          Owner.Auras.Remove(spell);
      }

      m_byId.Clear();
      m_byRealId.Clear();
    }

    /// <summary>
    /// Only works if you have 2 valid spell ids and oldSpellId already exists.
    /// </summary>
    public void Replace(SpellId oldSpellId, SpellId newSpellId)
    {
      Spell newSpell = SpellHandler.Get(newSpellId);
      Spell oldSpell;
      if(!m_byId.TryGetValue(oldSpellId, out oldSpell))
        return;
      Replace(oldSpell, newSpell);
    }

    /// <summary>
    /// Replaces or (if newSpell == null) removes oldSpell; does nothing if oldSpell doesn't exist.
    /// </summary>
    public virtual bool Replace(Spell oldSpell, Spell newSpell)
    {
      if(oldSpell != null)
        m_byRealId.Remove(oldSpell.RealId);
      if(m_byId.Remove(oldSpell.SpellId))
      {
        if(oldSpell.IsPassive)
          Owner.Auras.Remove(oldSpell);
        if(newSpell != null)
          AddSpell(newSpell);
        return true;
      }

      if(newSpell != null)
        AddSpell(newSpell);
      return false;
    }

    public virtual void AddDefaultSpells()
    {
    }

    public abstract void AddCooldown(Spell spell, Item casterItem);

    public abstract void ClearCooldowns();

    public abstract bool IsReady(Spell spell);

    /// <summary>Clears the cooldown for the given spell</summary>
    public void ClearCooldown(SpellId spellId, bool alsoClearCategory = true)
    {
      Spell cooldownSpell = SpellHandler.Get(spellId);
      if(cooldownSpell == null)
      {
        try
        {
          throw new ArgumentException("No spell given for cooldown", nameof(spellId));
        }
        catch(Exception ex)
        {
          LogUtil.WarnException(ex);
        }
      }
      else
        ClearCooldown(cooldownSpell, alsoClearCategory);
    }

    public void ClearCooldown(SpellLineId id, bool alsoClearCategory = true)
    {
      SpellLine line = id.GetLine();
      if(line == null)
        return;
      foreach(Spell cooldownSpell in line)
        ClearCooldown(cooldownSpell, alsoClearCategory);
    }

    public abstract void ClearCooldown(Spell cooldownSpell, bool alsoClearCategory = true);

    public List<AddTargetTriggerHandler> TargetTriggers
    {
      get
      {
        if(m_TargetTriggers == null)
          m_TargetTriggers = new List<AddTargetTriggerHandler>(3);
        return m_TargetTriggers;
      }
    }

    public int AvalibleSkillPoints
    {
      get
      {
        int num = Owner.Level -
                  AllSpells.Sum(allSpell => allSpell.Level) + 13;
        if(num >= 0)
          return num;
        return 0;
      }
    }

    /// <summary>
    /// Trigger all spells that might be triggered by the given Spell
    /// </summary>
    /// <param name="spell"></param>
    internal void TriggerSpellsFor(SpellCast cast)
    {
      if(m_TargetTriggers == null)
        return;
      Spell spell = cast.Spell;
      for(int index = 0; index < m_TargetTriggers.Count; ++index)
      {
        AddTargetTriggerHandler targetTrigger = m_TargetTriggers[index];
        SpellEffect spellEffect = targetTrigger.SpellEffect;
        int num;
        if(spellEffect.EffectType != SpellEffectType.TriggerSpellFromTargetWithCasterAsTarget &&
           spell.SpellClassSet == spellEffect.Spell.SpellClassSet && spellEffect.MatchesSpell(spell) &&
           (((num = spellEffect.CalcEffectValue(Owner)) >= 100 || Utility.Random(0, 101) <= num) &&
            (spell != spellEffect.TriggerSpell && targetTrigger.Aura.CasterUnit != null)))
          cast.Trigger(spellEffect.TriggerSpell);
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<Spell> GetEnumerator()
    {
      foreach(Spell spell in m_byId.Values)
        yield return spell;
    }

    public Spell GetSpellByRealId(short skillId)
    {
      Spell spell;
      m_byRealId.TryGetValue(skillId, out spell);
      return spell;
    }
  }
}