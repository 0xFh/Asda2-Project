using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells
{
  /// <summary>A set of Runes that Death Knights use</summary>
  public class RuneSet
  {
    public static float DefaultRuneCooldownPerSecond = 0.1f;
    public readonly RuneType[] ActiveRunes = new RuneType[6];

    public RuneSet(Character owner)
    {
      Owner = owner;
    }

    public Character Owner { get; internal set; }

    public float[] Cooldowns
    {
      get { return Owner.Record.RuneCooldowns; }
    }

    internal void InitRunes(Character owner)
    {
      Owner = owner;
      UnpackRuneSetMask(owner.Record.RuneSetMask);
      float[] cooldowns = Cooldowns;
      if(cooldowns == null || cooldowns.Length != 6)
        owner.Record.RuneCooldowns = new float[6];
      for(RuneType type = RuneType.Blood; type < RuneType.End; ++type)
        SetCooldown(type, DefaultRuneCooldownPerSecond);
    }

    internal void Dispose()
    {
      Owner = null;
    }

    public int GetIndexOfFirstRuneOfType(RuneType type, bool onlyIfNotOnCooldown = false)
    {
      for(int index = 0; index < 6; ++index)
      {
        if(ActiveRunes[index] == type && (!onlyIfNotOnCooldown || Cooldowns[index] <= 0.0))
          return index;
      }

      return -1;
    }

    public bool Convert(RuneType from, RuneType to, bool onlyIfNotOnCooldown = true)
    {
      for(uint index = 0; index < 6U; ++index)
      {
        if(ActiveRunes[index] == from && (!onlyIfNotOnCooldown || Cooldowns[index] <= 0.0))
        {
          Convert(index, to);
          return true;
        }
      }

      return false;
    }

    public void ConvertToDefault(uint index)
    {
      Convert(index, SpellConstants.DefaultRuneSet[index]);
    }

    public void Convert(uint index, RuneType to)
    {
      ActiveRunes[index] = to;
      SpellHandler.SendConvertRune(Owner.Client, index, to);
    }

    /// <summary>Returns how many runes of the given type are ready</summary>
    public int GetReadyRunes(RuneType type)
    {
      int num = 0;
      for(int index = 0; index < 6; ++index)
      {
        if(ActiveRunes[index] == type && Cooldowns[index] <= 0.0)
          ++num;
      }

      return num;
    }

    /// <summary>
    /// Whether there are enough runes in this set to satisfy the given cost requirements
    /// </summary>
    public bool HasEnoughRunes(Spell spell)
    {
      RuneCostEntry runeCostEntry = spell.RuneCostEntry;
      if(runeCostEntry == null || !runeCostEntry.CostsRunes ||
         Owner.Auras.GetModifiedInt(SpellModifierType.PowerCost, spell, 1) != 1)
        return true;
      for(RuneType runeType = RuneType.Blood; runeType < (RuneType) runeCostEntry.CostPerType.Length; ++runeType)
      {
        int num = runeCostEntry.CostPerType[(int) runeType];
        if(num > 0)
        {
          for(int index = 0; index < 6; ++index)
          {
            if((ActiveRunes[index] == runeType || ActiveRunes[index] == RuneType.Death) &&
               Cooldowns[index] <= 0.0)
              --num;
          }

          if(num > 0)
            return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Method is internal because we don't have a packet yet to signal the client spontaneous cooldown updates
    /// </summary>
    internal void ConsumeRunes(Spell spell)
    {
      RuneCostEntry runeCostEntry = spell.RuneCostEntry;
      if(runeCostEntry == null || !runeCostEntry.CostsRunes ||
         Owner.Auras.GetModifiedInt(SpellModifierType.PowerCost, spell, 1) != 1)
        return;
      for(RuneType runeType = RuneType.Blood; runeType < (RuneType) runeCostEntry.CostPerType.Length; ++runeType)
      {
        int num = runeCostEntry.CostPerType[(int) runeType];
        if(num > 0)
        {
          for(uint index = 0; index < 6U; ++index)
          {
            if(ActiveRunes[index] == runeType && Cooldowns[index] <= 0.0)
            {
              StartCooldown(index);
              --num;
              if(num == 0)
                return;
            }
          }

          for(uint index = 0; index < 6U; ++index)
          {
            if(ActiveRunes[index] == RuneType.Death && Cooldowns[index] <= 0.0)
            {
              ConvertToDefault(index);
              StartCooldown(index);
              --num;
              if(num == 0)
                return;
            }
          }
        }
      }
    }

    /// <summary>TODO: Send update to client, if necessary</summary>
    internal void StartCooldown(uint index)
    {
      Cooldowns[index] = 1f;
    }

    /// <summary>TODO: Send update to client, if necessary</summary>
    internal void UnsetCooldown(uint index)
    {
      Cooldowns[index] = 0.0f;
    }

    internal void UpdateCooldown(int dtMillis)
    {
      float[] cooldowns = Cooldowns;
      for(uint index = 0; index < 6U; ++index)
      {
        float num = cooldowns[index] -
                    (float) ((dtMillis * (double) GetCooldown(ActiveRunes[index]) + 500.0) /
                             1000.0);
        cooldowns[index] = (double) num <= 0.0 ? 0.0f : num;
      }
    }

    /// <summary>
    /// Gets the cooldown of the given RuneType in rune refreshment per second.
    /// For example:
    /// 1 = a rune refreshes in one second;
    /// 0.1 = a rune refrehes in 10 seconds.
    /// </summary>
    public float GetCooldown(RuneType type)
    {
      return Owner.GetFloat((PlayerFields) (25 + type));
    }

    public void SetCooldown(RuneType type, float cdPerSecond)
    {
      Owner.SetFloat((PlayerFields) (25 + type), cdPerSecond);
    }

    public void ModCooldown(RuneType type, float delta)
    {
      SetCooldown(type, GetCooldown(type) + delta);
    }

    /// <summary>Modifies all cooldowns by the given percentage</summary>
    /// <param name="percentDelta">If this value is 100, runes will cooldown in half the time</param>
    /// <returns>The delta of all rune types</returns>
    public float[] ModAllCooldownsPercent(int percentDelta)
    {
      float[] numArray = new float[4];
      for(RuneType type = RuneType.Blood; type < RuneType.End; ++type)
      {
        float cooldown = GetCooldown(type);
        float cdPerSecond = cooldown + (float) (cooldown * (double) percentDelta / 100.0);
        SetCooldown(type, cdPerSecond);
        numArray[(int) type] = cdPerSecond - cooldown;
      }

      return numArray;
    }

    public int PackRuneSetMask()
    {
      int num = 0;
      for(int index = 0; index < 6; ++index)
        num |= (int) (ActiveRunes[index] + 1) << SpellConstants.BitsPerRune * index;
      return num;
    }

    public void UnpackRuneSetMask(int runeSetMask)
    {
      if(runeSetMask == 0)
      {
        SpellConstants.DefaultRuneSet.CopyTo(ActiveRunes, 0);
      }
      else
      {
        for(int index = 0; index < 6; ++index)
        {
          RuneType runeType = (RuneType) ((runeSetMask & SpellConstants.SingleRuneFullBitMask) - 1);
          ActiveRunes[index] = runeType >= RuneType.End || runeType < RuneType.Blood
            ? SpellConstants.DefaultRuneSet[index]
            : runeType;
          runeSetMask >>= SpellConstants.BitsPerRune;
        }
      }
    }

    /// <summary>Used for packets</summary>
    internal byte GetActiveRuneMask()
    {
      int num = 0;
      float[] cooldowns = Cooldowns;
      for(int index = 0; index < 6; ++index)
      {
        if(cooldowns[index] == 0.0)
          num |= 1 << index;
      }

      return (byte) num;
    }
  }
}