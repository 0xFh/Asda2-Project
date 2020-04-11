using System;

namespace WCell.RealmServer.Spells
{
  [Serializable]
  public class AISpellSettings
  {
    public readonly CooldownRange Cooldown = new CooldownRange(-1, -1);

    /// <summary>Amount of time to idle after casting the spell</summary>
    public int IdleTimeAfterCastMillis = 1000;

    public static readonly CooldownRange[] DefaultCooldownsByCategory = new CooldownRange[4];

    public static CooldownRange GetDefaultCategoryCooldown(AISpellCooldownCategory cat)
    {
      return DefaultCooldownsByCategory[(int) cat];
    }

    public static void SetDefaultCategoryCooldown(AISpellCooldownCategory cat, int min, int max)
    {
      DefaultCooldownsByCategory[(int) cat] = new CooldownRange(min, max);
    }

    static AISpellSettings()
    {
      DefaultCooldownsByCategory[0] = new CooldownRange(30000, 60000);
      DefaultCooldownsByCategory[1] = new CooldownRange(30000, 60000);
      DefaultCooldownsByCategory[2] = new CooldownRange(30000, 60000);
      DefaultCooldownsByCategory[3] = new CooldownRange(5000, 10000);
    }

    public AISpellSettings(Spell spell)
    {
      Spell = spell;
    }

    public Spell Spell { get; private set; }

    public void SetCooldownRange(int cdMin, int cdMax)
    {
      SetCooldown(cdMin, cdMax);
    }

    public void SetCooldownRange(int cd)
    {
      SetCooldown(cd);
    }

    public void SetCooldown(int cd)
    {
      SetCooldown(cd, cd);
    }

    public void SetCooldown(int cdMin, int cdMax)
    {
      Cooldown.MinDelay = cdMin;
      Cooldown.MaxDelay = cdMax;
    }

    internal void InitializeAfterLoad()
    {
      CooldownRange categoryCooldown =
        GetDefaultCategoryCooldown(Spell.GetAISpellCooldownCategory());
      if(Cooldown.MinDelay < 0)
        Cooldown.MinDelay = categoryCooldown.MinDelay;
      if(Cooldown.MaxDelay >= 0)
        return;
      Cooldown.MaxDelay = categoryCooldown.MaxDelay;
    }
  }
}