using Castle.ActiveRecord;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Talents;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.ObjectPools;
using WCell.Util.Threading;

namespace WCell.RealmServer.Spells
{
  public class PlayerSpellCollection : SpellCollection
  {
    private static readonly ObjectPool<PlayerSpellCollection> PlayerSpellCollectionPool =
      new ObjectPool<PlayerSpellCollection>(() => new PlayerSpellCollection());

    /// <summary>Whether to send Update Packets</summary>
    protected bool m_sendPackets;

    /// <summary>
    /// All current Spell-cooldowns.
    /// Each SpellId has an expiry time associated with it
    /// </summary>
    protected List<ISpellIdCooldown> m_idCooldowns;

    /// <summary>
    /// All current category-cooldowns.
    /// Each category has an expiry time associated with it
    /// </summary>
    protected List<ISpellCategoryCooldown> m_categoryCooldowns;

    /// <summary>The runes of this Player (if any)</summary>
    private RuneSet m_runes;

    public static PlayerSpellCollection Obtain(Character chr)
    {
      PlayerSpellCollection playerSpellCollection = PlayerSpellCollectionPool.Obtain();
      playerSpellCollection.Initialize(chr);
      if(playerSpellCollection.Runes != null)
        playerSpellCollection.Runes.InitRunes(chr);
      return playerSpellCollection;
    }

    private PlayerSpellCollection()
    {
      m_idCooldowns = new List<ISpellIdCooldown>(5);
      m_categoryCooldowns = new List<ISpellCategoryCooldown>(5);
    }

    protected override void Initialize(Unit owner)
    {
      base.Initialize(owner);
      Character owner1 = (Character) owner;
      m_sendPackets = false;
      if(owner.Class != ClassId.Balista)
        return;
      m_runes = new RuneSet(owner1);
    }

    protected internal override void Recycle()
    {
      base.Recycle();
      m_idCooldowns.Clear();
      m_categoryCooldowns.Clear();
      if(m_runes != null)
      {
        m_runes.Dispose();
        m_runes = null;
      }

      PlayerSpellCollectionPool.Recycle(this);
    }

    public IEnumerable<ISpellIdCooldown> IdCooldowns
    {
      get { return m_idCooldowns; }
    }

    public IEnumerable<ISpellCategoryCooldown> CategoryCooldowns
    {
      get { return m_categoryCooldowns; }
    }

    public int IdCooldownCount
    {
      get { return m_idCooldowns.Count; }
    }

    public int CategoryCooldownCount
    {
      get { return m_categoryCooldowns.Count; }
    }

    /// <summary>Owner as Character</summary>
    public Character OwnerChar
    {
      get { return (Character) Owner; }
    }

    /// <summary>The set of runes of this Character (if any)</summary>
    public RuneSet Runes
    {
      get { return m_runes; }
    }

    public void AddNew(Spell spell)
    {
      AddSpell(spell, false);
    }

    public override void AddSpell(Spell spell)
    {
      AddSpell(spell, true);
    }

    /// <summary>
    /// Adds the spell without doing any further checks nor adding any spell-related skills or showing animations (after load)
    /// </summary>
    internal void OnlyAdd(SpellRecord record)
    {
      SpellId spellId = record.SpellId;
      Spell spell = SpellHandler.Get(spellId);
      m_byId[spellId] = spell;
    }

    /// <summary>
    /// Teaches a new spell to the unit. Also sends the spell learning animation, if applicable.
    /// </summary>
    private void AddSpell(Spell spell, bool sendPacket)
    {
      if(spell.Ability != null)
      {
        Skill skill = OwnerChar.Skills[spell.Ability.Skill.Id] ??
                      OwnerChar.Skills.Add(spell.Ability.Skill, true);
        if(skill.CurrentTierSpell == null || skill.CurrentTierSpell.SkillTier < spell.SkillTier)
          skill.CurrentTierSpell = spell;
      }

      if(!m_byId.ContainsKey(spell.SpellId))
      {
        Character ownerChar = OwnerChar;
        int specIndex = GetSpecIndex(spell);
        List<SpellRecord> spellList = GetSpellList(spell);
        SpellRecord record = new SpellRecord(spell.SpellId, ownerChar.EntityId.Low, specIndex);
        record.SaveLater();
        spellList.Add(record);
        base.AddSpell(spell);
      }

      if(!m_sendPackets || !sendPacket)
        return;
      if(spell.Level == 1)
        Asda2SpellHandler.SendSkillLearnedFirstTimeResponse(OwnerChar.Client, spell.RealId,
          spell.CooldownTime);
      Asda2SpellHandler.SendSkillLearnedResponse(SkillLearnStatus.Ok, OwnerChar, (uint) spell.RealId,
        spell.Level);
    }

    /// <summary>Replaces or (if newSpell == null) removes oldSpell.</summary>
    public override bool Replace(Spell oldSpell, Spell newSpell)
    {
      bool flag = oldSpell != null && m_byId.Remove(oldSpell.SpellId);
      if(flag)
      {
        OnRemove(oldSpell);
        if(newSpell == null)
        {
          int num = m_sendPackets ? 1 : 0;
          return true;
        }
      }

      if(newSpell != null)
      {
        if(m_sendPackets)
        {
          int num = flag ? 1 : 0;
        }

        AddSpell(newSpell, true);
      }

      return flag;
    }

    /// <summary>Enqueues a new task to remove that spell from DB</summary>
    private void OnRemove(Spell spell)
    {
      Character ownerChar = OwnerChar;
      if(spell.RepresentsSkillTier)
        ownerChar.Skills.Remove(spell.Ability.Skill.Id);
      if(spell.IsAura)
        ownerChar.Auras.Remove(spell);
      List<SpellRecord> spellList = GetSpellList(spell);
      for(int index = 0; index < spellList.Count; ++index)
      {
        SpellRecord spellRecord = spellList[index];
        if(spellRecord.SpellId == spell.SpellId)
        {
          ServerApp<RealmServer>.IOQueue.AddMessage(
            new Message(spellRecord.Delete));
          spellList.RemoveAt(index);
          break;
        }
      }
    }

    private int GetSpecIndex(Spell spell)
    {
      Character ownerChar = OwnerChar;
      if(!spell.IsTalent)
        return -1;
      return ownerChar.Talents.CurrentSpecIndex;
    }

    private List<SpellRecord> GetSpellList(Spell spell)
    {
      Character ownerChar = OwnerChar;
      if(spell.IsTalent)
        return ownerChar.CurrentSpecProfile.TalentSpells;
      return ownerChar.Record.AbilitySpells;
    }

    public override void Clear()
    {
      foreach(Spell spell in m_byId.Values.ToArray())
        OnRemove(spell);
      base.Clear();
    }

    internal void PlayerInitialize()
    {
      Character ownerChar = OwnerChar;
      foreach(Spell spell in m_byId.Values)
      {
        if(spell.Talent != null)
          ownerChar.Talents.AddExisting(spell.Talent, spell.Rank);
        else if(spell.IsPassive && !spell.HasHarmfulEffects)
        {
          int num = (int) ownerChar.SpellCast.Start(spell, true, Owner);
        }
      }

      foreach(Talent talent in ownerChar.Talents)
      {
        Spell spell = talent.Spell;
        if(spell.IsPassive)
        {
          int num = (int) ownerChar.SpellCast.Start(spell, true, Owner);
        }
      }

      m_sendPackets = true;
    }

    public override void AddDefaultSpells()
    {
      AddNew(SpellHandler.Get(SpellId.BashRank1));
      AddNew(SpellHandler.Get(SpellId.ArrowStrikeRank1));
    }

    /// <summary>Add everything to the caster that this spell requires</summary>
    public void AddSpellRequirements(Spell spell)
    {
    }

    /// <summary>
    /// Returns true if spell is currently cooling down.
    /// Removes expired cooldowns of that spell.
    /// </summary>
    public override bool IsReady(Spell spell)
    {
      if(spell.CooldownTime > 0)
      {
        for(int index = 0; index < m_idCooldowns.Count; ++index)
        {
          ISpellIdCooldown idCooldown = m_idCooldowns[index];
          if((int) idCooldown.SpellId == (int) spell.Id)
          {
            if(idCooldown.Until > DateTime.Now)
              return false;
            m_idCooldowns.RemoveAt(index);
            break;
          }
        }
      }

      return true;
    }

    public override void AddCooldown(Spell spell, Item casterItem)
    {
      bool flag = casterItem != null && casterItem.Template.UseSpell != null;
      int millis = 0;
      if(flag)
        millis = casterItem.Template.UseSpell.Cooldown;
      if(millis == 0)
        millis = spell.GetCooldown(Owner);
      if(millis <= 0)
        return;
      SpellIdCooldown spellIdCooldown = new SpellIdCooldown
      {
        SpellId = spell.Id,
        Until = DateTime.Now + TimeSpan.FromMilliseconds(millis)
      };
      if(flag)
        spellIdCooldown.ItemId = casterItem.Template.Id;
      m_idCooldowns.Add(spellIdCooldown);
      OwnerChar.Map.CallDelayed(500,
        () => Asda2SpellHandler.SendSetSkillCooldownResponse(OwnerChar, spell));
      OwnerChar.Map.CallDelayed(millis,
        () => Asda2SpellHandler.SendClearCoolDown(OwnerChar, spell.RealId));
    }

    /// <summary>Clears all pending spell cooldowns.</summary>
    public override void ClearCooldowns()
    {
      foreach(ISpellIdCooldown idCooldown in m_idCooldowns)
        Asda2SpellHandler.SendClearCoolDown(OwnerChar, (SpellId) idCooldown.SpellId);
      foreach(Spell spell in m_byId.Values)
      {
        foreach(ISpellCategoryCooldown categoryCooldown in m_categoryCooldowns)
        {
          if((int) spell.Category == (int) categoryCooldown.CategoryId)
          {
            Asda2SpellHandler.SendClearCoolDown(OwnerChar, spell.SpellId);
            break;
          }
        }
      }

      ISpellIdCooldown[] cds = m_idCooldowns.ToArray();
      ISpellCategoryCooldown[] catCds = m_categoryCooldowns.ToArray();
      m_idCooldowns.Clear();
      m_categoryCooldowns.Clear();
      ServerApp<RealmServer>.IOQueue.AddMessage(new Message(() =>
      {
        foreach(ISpellIdCooldown spellIdCooldown in cds)
        {
          if(spellIdCooldown is ActiveRecordBase)
            ((ActiveRecordBase) spellIdCooldown).Delete();
        }

        foreach(ISpellCategoryCooldown categoryCooldown in catCds)
        {
          if(categoryCooldown is ActiveRecordBase)
            ((ActiveRecordBase) categoryCooldown).Delete();
        }
      }));
    }

    /// <summary>Clears the cooldown for this spell</summary>
    public override void ClearCooldown(Spell cooldownSpell, bool alsoCategory = true)
    {
      Character ownerChar = OwnerChar;
      Asda2SpellHandler.SendClearCoolDown(ownerChar, cooldownSpell.SpellId);
      if(alsoCategory && cooldownSpell.Category != 0U)
      {
        foreach(Spell spell in m_byId.Values)
        {
          if((int) spell.Category == (int) cooldownSpell.Category)
            Asda2SpellHandler.SendClearCoolDown(ownerChar, spell.SpellId);
        }
      }

      ISpellIdCooldown idCooldown =
        m_idCooldowns.RemoveFirst(
          cd => (int) cd.SpellId == (int) cooldownSpell.Id);
      ISpellCategoryCooldown catCooldown = m_categoryCooldowns.RemoveFirst(
        cd => (int) cd.CategoryId == (int) cooldownSpell.Category);
      if(!(idCooldown is ActiveRecordBase) && !(catCooldown is ActiveRecordBase))
        return;
      ServerApp<RealmServer>.IOQueue.AddMessage(new Message(() =>
      {
        if(idCooldown is ActiveRecordBase)
          ((ActiveRecordBase) idCooldown).Delete();
        if(!(catCooldown is ActiveRecordBase))
          return;
        ((ActiveRecordBase) catCooldown).Delete();
      }));
    }

    private void SaveCooldowns()
    {
      SaveCooldowns(m_idCooldowns);
    }

    private void SaveCooldowns<T>(List<T> cooldowns) where T : ICooldown
    {
      for(int index = cooldowns.Count - 1; index >= 0; --index)
      {
        ICooldown cooldown = cooldowns[index];
        if(cooldown.Until < DateTime.Now.AddMilliseconds(SpellHandler.MinCooldownSaveTimeMillis))
        {
          if(cooldown is ActiveRecordBase)
            ((ActiveRecordBase) cooldown).DeleteLater();
        }
        else
        {
          IConsistentCooldown consistentCooldown = cooldown.AsConsistent();
          consistentCooldown.CharId = Owner.EntityId.Low;
          consistentCooldown.Save();
          cooldowns.Add((T) consistentCooldown);
        }

        cooldowns.RemoveAt(index);
      }
    }

    internal void OnSave()
    {
      try
      {
        SaveCooldowns();
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException(ex,
          string.Format("Failed to save cooldowns for {0}.",
            OwnerChar == null ? "Not character" : OwnerChar.Name));
      }
    }

    internal void LoadSpellsAndTalents()
    {
      Character ownerChar = OwnerChar;
      CharacterRecord record = ownerChar.Record;
      SpellRecord[] spellRecordArray = SpellRecord.LoadAllRecordsFor(ownerChar.EntityId.Low);
      SpecProfile[] specProfiles = ownerChar.SpecProfiles;
      foreach(SpellRecord spellRecord in spellRecordArray)
      {
        Spell spell = spellRecord.Spell;
        if(spell == null)
          LogManager.GetCurrentClassLogger().Warn("Character \"{0}\" had invalid spell: {1} ({2})",
            this, spellRecord.SpellId, spellRecord.SpellId);
        else if(spell.IsTalent)
        {
          if(spellRecord.SpecIndex < 0 || spellRecord.SpecIndex >= specProfiles.Length)
            LogManager.GetCurrentClassLogger().Warn(
              "Character \"{0}\" had Talent-Spell {1} ({2}) but with invalid SpecIndex: {3}",
              (object) this, (object) spellRecord.SpellId, (object) spellRecord.SpellId,
              (object) spellRecord.SpecIndex);
          else
            specProfiles[spellRecord.SpecIndex].TalentSpells.Add(spellRecord);
        }
        else
        {
          record.AbilitySpells.Add(spellRecord);
          OnlyAdd(spell);
        }
      }

      foreach(SpellRecord talentSpell in ownerChar.CurrentSpecProfile.TalentSpells)
        OnlyAdd(talentSpell);
    }

    internal void LoadCooldowns()
    {
      Character ownerChar = OwnerChar;
      DateTime now = DateTime.Now;
      foreach(PersistentSpellIdCooldown record in PersistentSpellIdCooldown.LoadIdCooldownsFor(ownerChar.EntityId
        .Low))
      {
        if(record.Until > now)
          m_idCooldowns.Add(record);
        else
          record.DeleteLater();
      }

      foreach(PersistentSpellCategoryCooldown record in PersistentSpellCategoryCooldown.LoadCategoryCooldownsFor(
        ownerChar.EntityId.Low))
      {
        if(record.Until > now)
          m_categoryCooldowns.Add(record);
        else
          record.DeleteLater();
      }
    }

    public SkillLearnStatus TryLearnSpell(short skillId, byte level)
    {
      Spell spell = SpellHandler.Get((uint) skillId + level * 1000U);
      if(spell == null || level <= 0)
        return SkillLearnStatus.Fail;
      if(spell.LearnLevel > OwnerChar.Level)
        return SkillLearnStatus.LowLevel;
      if(spell.ClassMask != Asda2ClassMask.All &&
         !spell.ClassMask.HasFlag(OwnerChar.Asda2ClassMask) ||
         spell.ProffNum > OwnerChar.RealProffLevel)
        return SkillLearnStatus.JoblevelIsNotHighEnought;
      if(AvalibleSkillPoints <= 0)
        return SkillLearnStatus.NotEnoghtSpellPoints;
      if(!OwnerChar.SubtractMoney((uint) spell.Cost))
        return SkillLearnStatus.NotEnoghtMoney;
      AchievementProgressRecord progressRecord = OwnerChar.Achievements.GetOrCreateProgressRecord(1U);
      ++progressRecord.Counter;
      if(progressRecord.Counter == 45U)
      {
        switch(OwnerChar.Profession)
        {
          case Asda2Profession.Warrior:
            OwnerChar.DiscoverTitle(Asda2TitleId.ofBattle24);
            break;
          case Asda2Profession.Archer:
            OwnerChar.DiscoverTitle(Asda2TitleId.ofArchery25);
            break;
          case Asda2Profession.Mage:
            OwnerChar.DiscoverTitle(Asda2TitleId.ofMagic26);
            break;
        }
      }

      if(progressRecord.Counter > 90U)
      {
        switch(OwnerChar.Profession)
        {
          case Asda2Profession.Warrior:
            OwnerChar.GetTitle(Asda2TitleId.ofBattle24);
            break;
          case Asda2Profession.Archer:
            OwnerChar.GetTitle(Asda2TitleId.ofArchery25);
            break;
          case Asda2Profession.Mage:
            OwnerChar.GetTitle(Asda2TitleId.ofMagic26);
            break;
        }
      }

      progressRecord.SaveAndFlush();
      if(level > 1)
      {
        Spell oldSpell = this.FirstOrDefault(s => (int) s.RealId == (int) skillId);
        if(oldSpell == null || oldSpell.Level != spell.Level - 1)
          return SkillLearnStatus.BadSpellLevel;
        Replace(oldSpell, spell);
        return SkillLearnStatus.Ok;
      }

      AddSpell(spell, true);
      OwnerChar.SendMoneyUpdate();
      return SkillLearnStatus.Ok;
    }
  }
}