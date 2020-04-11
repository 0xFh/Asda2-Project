using System;
using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.Util;
using WCell.Util.ObjectPools;
using WCell.Util.Threading;

namespace WCell.RealmServer.Spells
{
  /// <summary>NPC spell collection</summary>
  public class NPCSpellCollection : SpellCollection
  {
    /// <summary>Cooldown of NPC spells, if they don't have one</summary>
    public static int DefaultNPCSpellCooldownMillis = 1500;

    private static readonly ObjectPool<NPCSpellCollection> NPCSpellCollectionPool =
      new ObjectPool<NPCSpellCollection>(() => new NPCSpellCollection());

    protected HashSet<Spell> m_readySpells;
    protected List<CooldownRemoveTimer> m_cooldowns;

    public static NPCSpellCollection Obtain(NPC npc)
    {
      NPCSpellCollection npcSpellCollection = NPCSpellCollectionPool.Obtain();
      npcSpellCollection.Initialize(npc);
      return npcSpellCollection;
    }

    private NPCSpellCollection()
    {
      m_readySpells = new HashSet<Spell>();
    }

    protected internal override void Recycle()
    {
      base.Recycle();
      m_readySpells.Clear();
      if(m_cooldowns != null)
        m_cooldowns.Clear();
      NPCSpellCollectionPool.Recycle(this);
    }

    public NPC OwnerNPC
    {
      get { return Owner as NPC; }
    }

    public IEnumerable<Spell> ReadySpells
    {
      get { return m_readySpells; }
    }

    public int ReadyCount
    {
      get { return m_readySpells.Count; }
    }

    /// <summary>The max combat of any 1vs1 combat spell</summary>
    public float MaxCombatSpellRange { get; private set; }

    /// <summary>Shuffles all currently ready Spells</summary>
    public void ShuffleReadySpells()
    {
      Utility.Shuffle(m_readySpells);
    }

    public Spell GetReadySpell(SpellId spellId)
    {
      foreach(Spell readySpell in m_readySpells)
      {
        if(readySpell.SpellId == spellId)
          return readySpell;
      }

      return null;
    }

    public override void AddSpell(Spell spell)
    {
      if(m_byId.ContainsKey(spell.SpellId))
        return;
      base.AddSpell(spell);
      OnNewSpell(spell);
    }

    private void OnNewSpell(Spell spell)
    {
      if(!spell.IsAreaSpell && !spell.IsAura && spell.HasHarmfulEffects)
        MaxCombatSpellRange = Math.Max(MaxCombatSpellRange,
          Owner.GetSpellMaxRange(spell, null));
      AddReadySpell(spell);
    }

    /// <summary>
    /// Adds the given spell as ready. Once casted, the spell will be removed.
    /// This can be used to signal a one-time cast of a spell whose priority is to be
    /// compared to the other spells.
    /// </summary>
    public void AddReadySpell(Spell spell)
    {
      if(spell.IsPassive || !Contains(spell.Id))
        return;
      m_readySpells.Add(spell);
    }

    public override void Clear()
    {
      base.Clear();
      m_readySpells.Clear();
    }

    public override bool Remove(Spell spell)
    {
      if(!base.Remove(spell))
        return false;
      m_readySpells.Remove(spell);
      if(Owner.GetSpellMaxRange(spell, null) >= (double) MaxCombatSpellRange)
      {
        MaxCombatSpellRange = 0.0f;
        foreach(Spell spell1 in m_byId.Values)
        {
          if(spell1.Range.MaxDist > (double) MaxCombatSpellRange)
            MaxCombatSpellRange = Owner.GetSpellMaxRange(spell1, null);
        }
      }

      return true;
    }

    /// <summary>
    /// When NPC Spells cooldown, they get removed from the list of
    /// ready Spells.
    /// </summary>
    public override void AddCooldown(Spell spell, Item item)
    {
      int num = Math.Max(spell.GetCooldown(Owner), spell.CategoryCooldownTime);
      if(num <= 0)
      {
        if(spell.CastDelay != 0U || spell.Durations.Max != 0)
          return;
        int modifiedInt = Owner.Auras.GetModifiedInt(SpellModifierType.CooldownTime, spell,
          DefaultNPCSpellCooldownMillis);
        AddCooldown(spell, modifiedInt);
      }
      else
      {
        int modifiedInt = Owner.Auras.GetModifiedInt(SpellModifierType.CooldownTime, spell, num);
        AddCooldown(spell, modifiedInt);
      }
    }

    public void AddCooldown(Spell spell, DateTime cdTime)
    {
      int milliSecondsInt = (cdTime - DateTime.Now).ToMilliSecondsInt();
      AddCooldown(spell, milliSecondsInt);
    }

    private void AddCooldown(Spell spell, int millis)
    {
      if(millis <= 0)
        return;
      m_readySpells.Remove(spell);
      CooldownRemoveTimer cooldownRemoveTimer =
        new CooldownRemoveTimer(millis, spell);
      Owner.CallDelayed(millis,
        o => ((NPCSpellCollection) Owner.Spells).AddReadySpell(spell));
      if(m_cooldowns == null)
        m_cooldowns = new List<CooldownRemoveTimer>();
      m_cooldowns.Add(cooldownRemoveTimer);
    }

    public override void ClearCooldowns()
    {
      IContextHandler contextHandler = Owner.ContextHandler;
      if(contextHandler == null)
        return;
      contextHandler.AddMessage(() =>
      {
        if(m_cooldowns == null)
          return;
        foreach(CooldownRemoveTimer cooldown in m_cooldowns)
        {
          Owner.RemoveUpdateAction(cooldown);
          AddReadySpell(cooldown.Spell);
        }
      });
    }

    public override bool IsReady(Spell spell)
    {
      return m_readySpells.Contains(spell);
    }

    public override void ClearCooldown(Spell spell, bool alsoCategory = true)
    {
      if(m_cooldowns == null)
        return;
      for(int index = 0; index < m_cooldowns.Count; ++index)
      {
        CooldownRemoveTimer cooldown = m_cooldowns[index];
        if((int) cooldown.Spell.Id == (int) spell.Id)
        {
          m_cooldowns.Remove(cooldown);
          AddReadySpell(cooldown.Spell);
          break;
        }
      }
    }

    /// <summary>
    /// Returns the delay until the given spell has cooled down in milliseconds
    /// </summary>
    public int GetRemainingCooldownMillis(Spell spell)
    {
      if(m_cooldowns == null)
        return 0;
      CooldownRemoveTimer cooldownRemoveTimer =
        m_cooldowns.Find(
          cd => (int) cd.Spell.Id == (int) spell.Id);
      if(cooldownRemoveTimer != null)
        return cooldownRemoveTimer.GetDelayUntilNextExecution(Owner);
      return 0;
    }

    protected class CooldownRemoveTimer : OneShotObjectUpdateTimer
    {
      public CooldownRemoveTimer(int millis, Spell spell)
        : base(millis, null)
      {
        Spell = spell;
        Callback = DoRemoveCooldown;
      }

      public Spell Spell { get; set; }

      private void DoRemoveCooldown(WorldObject owner)
      {
        ((NPCSpellCollection) ((Unit) owner).Spells).AddReadySpell(Spell);
      }
    }
  }
}