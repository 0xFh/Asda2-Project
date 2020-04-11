using Castle.ActiveRecord;
using WCell.Constants.Items;
using WCell.Core.Database;
using WCell.RealmServer.Asda2PetSystem;
using WCell.RealmServer.Database;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Entities
{
  [ActiveRecord("Asda2PetRecord", Access = PropertyAccess.Property)]
  public class Asda2PetRecord : WCellRecord<Asda2PetRecord>
  {
    private static readonly NHIdGenerator
      _idGenerator = new NHIdGenerator(typeof(Asda2PetRecord), nameof(Guid), 1L);

    private byte _hungerPrc;
    private byte _level;

    public PetTemplate Template { get; set; }

    public Character Owner { get; set; }

    [Property]
    public uint OwnerId { get; set; }

    [Property]
    public short Id { get; set; }

    [Property]
    public string Name { get; set; }

    [PrimaryKey]
    public int Guid { get; set; }

    [Property]
    public short Expirience { get; set; }

    [Property]
    public byte HungerPrc
    {
      get { return _hungerPrc; }
      set
      {
        _hungerPrc = value;
        if(Owner == null)
          return;
        Asda2PetHandler.SendUpdatePetHungerResponse(Owner.Client, this);
      }
    }

    public Asda2PetStatType Stat1Type
    {
      get { return (Asda2PetStatType) Template.Bonus1Type; }
    }

    public int Stat1Value
    {
      get
      {
        if(Template.Bonus1Type != 0)
          return Asda2PetMgr.PetOptionValues[Template.Bonus1Type][Template.Rank][
            Template.Rarity][Level];
        return 0;
      }
    }

    public Asda2PetStatType Stat2Type
    {
      get { return (Asda2PetStatType) Template.Bonus2Type; }
    }

    public int Stat2Value
    {
      get
      {
        if(Template.Bonus2Type != 0)
          return Asda2PetMgr.PetOptionValues[Template.Bonus2Type][Template.Rank][
            Template.Rarity][Level];
        return 0;
      }
    }

    public Asda2PetStatType Stat3Type
    {
      get { return (Asda2PetStatType) Template.Bonus3Type; }
    }

    public int Stat3Value
    {
      get
      {
        if(Template.Bonus3Type != 0)
          return Asda2PetMgr.PetOptionValues[Template.Bonus3Type][Template.Rank][
            Template.Rarity][Level];
        return 0;
      }
    }

    [Property]
    public byte Level
    {
      get { return _level; }
      set { _level = value; }
    }

    public void AddStatsToOwner()
    {
      if(Stat1Type != Asda2PetStatType.None)
        ApplyStat(Stat1Type, Stat1Value);
      if(Stat2Type != Asda2PetStatType.None)
        ApplyStat(Stat2Type, Stat2Value);
      if(Stat3Type == Asda2PetStatType.None)
        return;
      ApplyStat(Stat3Type, Stat3Value);
    }

    private void ApplyStat(Asda2PetStatType type, int value)
    {
      switch(type)
      {
        case Asda2PetStatType.Strength:
          Owner.ApplyStatMod(ItemModType.Strength, value);
          break;
        case Asda2PetStatType.Stamina:
          Owner.ApplyStatMod(ItemModType.Stamina, value);
          break;
        case Asda2PetStatType.Intellect:
          Owner.ApplyStatMod(ItemModType.Intellect, value);
          break;
        case Asda2PetStatType.Energy:
          Owner.ApplyStatMod(ItemModType.Spirit, value);
          break;
        case Asda2PetStatType.Dexterity:
          Owner.ApplyStatMod(ItemModType.Agility, value);
          break;
        case Asda2PetStatType.Luck:
          Owner.ApplyStatMod(ItemModType.Luck, value);
          break;
        case Asda2PetStatType.AllCapabilities:
          Owner.ApplyStatMod(ItemModType.Strength, value);
          Owner.ApplyStatMod(ItemModType.Agility, value);
          Owner.ApplyStatMod(ItemModType.Intellect, value);
          Owner.ApplyStatMod(ItemModType.Stamina, value);
          Owner.ApplyStatMod(ItemModType.Luck, value);
          Owner.ApplyStatMod(ItemModType.Spirit, value);
          break;
        case Asda2PetStatType.MinAtack:
          Owner.ApplyStatMod(ItemModType.Damage, value);
          break;
        case Asda2PetStatType.MaxAtack:
          Owner.ApplyStatMod(ItemModType.Damage, value);
          break;
        case Asda2PetStatType.MinMaxAtack:
          Owner.ApplyStatMod(ItemModType.Damage, value);
          break;
        case Asda2PetStatType.MinMagicAtack:
          Owner.ApplyStatMod(ItemModType.MagicDamage, value);
          break;
        case Asda2PetStatType.MaxMagicAtack:
          Owner.ApplyStatMod(ItemModType.MagicDamage, value);
          break;
        case Asda2PetStatType.MinMaxMagicAtack:
          Owner.ApplyStatMod(ItemModType.MagicDamage, value);
          break;
        case Asda2PetStatType.MagicDeffence:
          Owner.ApplyStatMod(ItemModType.Asda2MagicDefence,
            (int) (value * (double) CharacterFormulas.PetMagicDeffenceMultiplier));
          break;
        case Asda2PetStatType.MinDeffence:
          Owner.ApplyStatMod(ItemModType.Asda2Defence,
            (int) (value * (double) CharacterFormulas.PetDeffenceMultiplier));
          break;
        case Asda2PetStatType.MaxDeffence:
          Owner.ApplyStatMod(ItemModType.Asda2Defence,
            (int) (value * (double) CharacterFormulas.PetDeffenceMultiplier));
          break;
        case Asda2PetStatType.MinMaxDeffence:
          Owner.ApplyStatMod(ItemModType.Asda2Defence,
            (int) (value * (double) CharacterFormulas.PetMagicDeffenceMultiplier));
          break;
        case Asda2PetStatType.DodgePrc:
          Owner.ApplyStatMod(ItemModType.DodgeRating, value);
          break;
        case Asda2PetStatType.CriticalPrc:
          Owner.ApplyStatMod(ItemModType.MeleeCriticalStrikeRating, value);
          Owner.ApplyStatMod(ItemModType.CriticalStrikeRating, value);
          Owner.ApplyStatMod(ItemModType.SpellCriticalStrikeRating, value);
          break;
        case Asda2PetStatType.ItemSellingPricePrc:
          Owner.ApplyStatMod(ItemModType.SellingCost, value);
          break;
        case Asda2PetStatType.StrengthPrc:
          Owner.ApplyStatMod(ItemModType.StrengthPrc, value);
          break;
        case Asda2PetStatType.StaminaPrc:
          Owner.ApplyStatMod(ItemModType.StaminaPrc, value);
          break;
        case Asda2PetStatType.IntellectPrc:
          Owner.ApplyStatMod(ItemModType.IntelectPrc, value);
          break;
        case Asda2PetStatType.DexterityPrc:
          Owner.ApplyStatMod(ItemModType.AgilityPrc, value);
          break;
        case Asda2PetStatType.LuckPrc:
          Owner.ApplyStatMod(ItemModType.LuckPrc, value);
          break;
        case Asda2PetStatType.AllCapabilitiesPrc:
          Owner.ApplyStatMod(ItemModType.StrengthPrc, value);
          Owner.ApplyStatMod(ItemModType.AgilityPrc, value);
          Owner.ApplyStatMod(ItemModType.IntelectPrc, value);
          Owner.ApplyStatMod(ItemModType.StaminaPrc, value);
          Owner.ApplyStatMod(ItemModType.LuckPrc, value);
          Owner.ApplyStatMod(ItemModType.EnergyPrc, value);
          break;
        case Asda2PetStatType.MinAtackPrc:
          Owner.ApplyStatMod(ItemModType.DamagePrc, value);
          break;
        case Asda2PetStatType.MaxAtackPrc:
          Owner.ApplyStatMod(ItemModType.DamagePrc, value);
          break;
        case Asda2PetStatType.MinMaxAtackPrc:
          Owner.ApplyStatMod(ItemModType.DamagePrc, value);
          break;
        case Asda2PetStatType.MinMagicAtackPrc:
          Owner.ApplyStatMod(ItemModType.MagicDamagePrc, value);
          break;
        case Asda2PetStatType.MaxMagicAtackPrc:
          Owner.ApplyStatMod(ItemModType.MagicDamagePrc, value);
          break;
        case Asda2PetStatType.MinMaxMagicAtackPrc:
          Owner.ApplyStatMod(ItemModType.MagicDamagePrc, value);
          break;
        case Asda2PetStatType.MagicDeffencePrc:
          Owner.ApplyStatMod(ItemModType.Asda2MagicDefencePrc, value);
          break;
        case Asda2PetStatType.MinDeffencePrc:
          Owner.ApplyStatMod(ItemModType.Asda2DefencePrc, value);
          break;
        case Asda2PetStatType.MaxDeffencePrc:
          Owner.ApplyStatMod(ItemModType.Asda2DefencePrc, value);
          break;
        case Asda2PetStatType.MinMaxDeffencePrc:
          Owner.ApplyStatMod(ItemModType.Asda2DefencePrc, value);
          break;
      }
    }

    public void RemoveStatsFromOwner()
    {
      if(Stat1Type != Asda2PetStatType.None)
        ApplyStat(Stat1Type, -Stat1Value);
      if(Stat2Type != Asda2PetStatType.None)
        ApplyStat(Stat2Type, -Stat2Value);
      if(Stat3Type == Asda2PetStatType.None)
        return;
      ApplyStat(Stat3Type, -Stat3Value);
    }

    [Property]
    public byte MaxLevel { get; set; }

    [Property]
    public bool CanChangeName { get; set; }

    public bool IsMaxExpirience
    {
      get
      {
        return Expirience >=
               Asda2PetMgr.ExpTable[Template.Rank][Template.Rarity][Level - 1];
      }
    }

    public Asda2PetRecord(PetTemplate template, Character owner)
    {
      Guid = (int) _idGenerator.Next();
      Id = (short) template.Id;
      OwnerId = owner.EntityId.Low;
      Name = template.Name;
      Owner = owner;
      MaxLevel = (byte) template.MaxLevel;
      _level = 1;
      Template = template;
      CanChangeName = true;
      HungerPrc = 100;
    }

    public Asda2PetRecord()
    {
    }

    public void Init(Character owner)
    {
      Template = Asda2PetMgr.PetTemplates[Id];
      Owner = owner;
    }

    public static Asda2PetRecord[] LoadAll(Character owner)
    {
      Asda2PetRecord[] allByProperty =
        FindAllByProperty("OwnerId", owner.EntityId.Low);
      foreach(Asda2PetRecord asda2PetRecord in allByProperty)
        asda2PetRecord.Init(owner);
      return allByProperty;
    }

    public bool GainXp(int i)
    {
      if(Level == 10)
        return false;
      int num = Asda2PetMgr.ExpTable[Template.Rank][Template.Rarity][Level - 1];
      if(Level == MaxLevel && Expirience >= num)
      {
        Expirience = (short) num;
        return false;
      }

      Expirience += (short) i;
      if(Level == MaxLevel)
      {
        if(Expirience > num)
          Expirience = (short) num;
        Asda2PetHandler.SendUpdatePetExpResponse(Owner.Client, this, false);
        return true;
      }

      if(Expirience > num)
      {
        RemoveStatsFromOwner();
        ++Level;
        AddStatsToOwner();
        Asda2CharacterHandler.SendUpdateStatsResponse(Owner.Client);
        Asda2CharacterHandler.SendUpdateStatsOneResponse(Owner.Client);
        GlobalHandler.UpdateCharacterPetInfoToArea(Owner);
        Asda2PetHandler.SendUpdatePetHungerResponse(Owner.Client, this);
      }

      Asda2PetHandler.SendUpdatePetExpResponse(Owner.Client, this, true);
      return true;
    }

    public void Feed(int i)
    {
      HungerPrc += (byte) i;
      if(HungerPrc > 100)
        HungerPrc = 100;
      Asda2PetHandler.SendUpdatePetHungerResponse(Owner.Client, this);
    }

    public void RemovePrcExp(int prc)
    {
      int num = Expirience -
                Asda2PetMgr.ExpTable[Template.Rank][Template.Rarity][Level - 2];
      Expirience -= (short) (num - num * prc / 100);
    }
  }
}