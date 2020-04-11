using System;
using WCell.Constants.NPCs;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs.Trainers
{
  /// <summary>Represents something that a trainer can teach</summary>
  [Serializable]
  public class TrainerSpellEntry : IDataHolder
  {
    public NPCId TrainerId;
    public SpellId SpellId;

    /// <summary>The base cost of the spell at the trainer.</summary>
    public uint Cost;

    /// <summary>
    /// 
    /// </summary>
    public SpellId RequiredSpellId;

    /// <summary>
    /// The minimum level a character must have acheived in order to purchase this spell.
    /// </summary>
    public int RequiredLevel;

    /// <summary>
    /// The required profession or secondary skill that this character must be trained in in order to purchase this spell.
    /// </summary>
    public SkillId RequiredSkillId;

    /// <summary>
    /// The required level of skill that a character must have obtained in the RequiredSkill in order to purchase this spell.
    /// </summary>
    public uint RequiredSkillAmount;

    /// <summary>
    /// The spell that the character will learn upon purchase of this TrainerSpellEntry from a trainer.
    /// </summary>
    [NotPersistent]public Spell Spell;

    /// <summary>The index of this Entry within the Trainer list</summary>
    [NotPersistent]public int Index;

    /// <summary>
    /// The price of the spell after Reputation discounts are applied.
    /// </summary>
    public uint GetDiscountedCost(Character character, NPC trainer)
    {
      if(character == null || trainer == null)
        return Cost;
      return character.Reputations.GetDiscountedCost(trainer.Faction.ReputationIndex, Cost);
    }

    /// <summary>
    /// The availability of the spell for the spell list filter.
    /// </summary>
    /// <returns>Available, Unavailable, AlreadyKnown</returns>
    public TrainerSpellState GetTrainerSpellState(Character character)
    {
      Spell spell = Spell;
      if(spell.IsTeachSpell)
        spell = spell.LearnSpell;
      if(character.Spells.Contains(spell.Id))
        return TrainerSpellState.AlreadyLearned;
      return spell.PreviousRank != null && !character.Spells.Contains(spell.PreviousRank.Id) ||
             spell.Ability == null ||
             (RequiredLevel > 0 && character.Level < RequiredLevel ||
              RequiredSpellId != SpellId.None) ||
             (RequiredSkillId != SkillId.None &&
              !character.Skills.CheckSkill(RequiredSkillId, (int) RequiredSkillAmount) ||
              Spell.IsProfession && Spell.TeachesApprenticeAbility &&
              character.Skills.FreeProfessions == 0U)
        ? TrainerSpellState.Unavailable
        : TrainerSpellState.Available;
    }

    public void FinalizeDataHolder()
    {
      if((Spell = SpellHandler.Get(SpellId)) == null)
        ContentMgr.OnInvalidDBData("SpellId is invalid in " + this);
      else if(RequiredSpellId != SpellId.None && SpellHandler.Get(RequiredSpellId) == null)
        ContentMgr.OnInvalidDBData("RequiredSpellId is invalid in " + this);
      else if(RequiredSkillId != SkillId.None && SkillHandler.Get(RequiredSkillId) == null)
      {
        ContentMgr.OnInvalidDBData("RequiredSkillId is invalid in " + this);
      }
      else
      {
        NPCEntry entry = NPCMgr.GetEntry(TrainerId);
        if(entry == null)
        {
          ContentMgr.OnInvalidDBData("TrainerId is invalid in " + this);
        }
        else
        {
          if(RequiredLevel == 0)
            RequiredLevel = Spell.Level;
          if(entry.TrainerEntry == null)
            entry.TrainerEntry = new TrainerEntry();
          entry.TrainerEntry.AddSpell(this);
        }
      }
    }

    public override string ToString()
    {
      return string.Format(
        "TrainerSpellEntry (Trainer: {0}, Spell: {1}, RequiredSpell: {2}, Required Skill: {3} ({4}))",
        (object) TrainerId, (object) SpellId, (object) RequiredSpellId,
        (object) RequiredSkillId, (object) RequiredSkillAmount);
    }
  }
}