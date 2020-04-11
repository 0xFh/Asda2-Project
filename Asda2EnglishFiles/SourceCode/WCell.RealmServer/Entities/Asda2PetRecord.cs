using Castle.ActiveRecord;
using WCell.Constants.Items;
using WCell.Core.Database;
using WCell.RealmServer.Asda2PetSystem;
using WCell.RealmServer.Database;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Entities
{
    [Castle.ActiveRecord.ActiveRecord("Asda2PetRecord", Access = PropertyAccess.Property)]
    public class Asda2PetRecord : WCellRecord<Asda2PetRecord>
    {
        private static readonly NHIdGenerator
            _idGenerator = new NHIdGenerator(typeof(Asda2PetRecord), nameof(Guid), 1L);

        private byte _hungerPrc;
        private byte _level;

        public PetTemplate Template { get; set; }

        public Character Owner { get; set; }

        [Property] public uint OwnerId { get; set; }

        [Property] public short Id { get; set; }

        [Property] public string Name { get; set; }

        [PrimaryKey] public int Guid { get; set; }

        [Property] public short Expirience { get; set; }

        [Property]
        public byte HungerPrc
        {
            get { return this._hungerPrc; }
            set
            {
                this._hungerPrc = value;
                if (this.Owner == null)
                    return;
                Asda2PetHandler.SendUpdatePetHungerResponse(this.Owner.Client, this);
            }
        }

        public Asda2PetStatType Stat1Type
        {
            get { return (Asda2PetStatType) this.Template.Bonus1Type; }
        }

        public int Stat1Value
        {
            get
            {
                if (this.Template.Bonus1Type != 0)
                    return Asda2PetMgr.PetOptionValues[this.Template.Bonus1Type][this.Template.Rank][
                        this.Template.Rarity][(int) this.Level];
                return 0;
            }
        }

        public Asda2PetStatType Stat2Type
        {
            get { return (Asda2PetStatType) this.Template.Bonus2Type; }
        }

        public int Stat2Value
        {
            get
            {
                if (this.Template.Bonus2Type != 0)
                    return Asda2PetMgr.PetOptionValues[this.Template.Bonus2Type][this.Template.Rank][
                        this.Template.Rarity][(int) this.Level];
                return 0;
            }
        }

        public Asda2PetStatType Stat3Type
        {
            get { return (Asda2PetStatType) this.Template.Bonus3Type; }
        }

        public int Stat3Value
        {
            get
            {
                if (this.Template.Bonus3Type != 0)
                    return Asda2PetMgr.PetOptionValues[this.Template.Bonus3Type][this.Template.Rank][
                        this.Template.Rarity][(int) this.Level];
                return 0;
            }
        }

        [Property]
        public byte Level
        {
            get { return this._level; }
            set { this._level = value; }
        }

        public void AddStatsToOwner()
        {
            if (this.Stat1Type != Asda2PetStatType.None)
                this.ApplyStat(this.Stat1Type, this.Stat1Value);
            if (this.Stat2Type != Asda2PetStatType.None)
                this.ApplyStat(this.Stat2Type, this.Stat2Value);
            if (this.Stat3Type == Asda2PetStatType.None)
                return;
            this.ApplyStat(this.Stat3Type, this.Stat3Value);
        }

        private void ApplyStat(Asda2PetStatType type, int value)
        {
            switch (type)
            {
                case Asda2PetStatType.Strength:
                    this.Owner.ApplyStatMod(ItemModType.Strength, value);
                    break;
                case Asda2PetStatType.Stamina:
                    this.Owner.ApplyStatMod(ItemModType.Stamina, value);
                    break;
                case Asda2PetStatType.Intellect:
                    this.Owner.ApplyStatMod(ItemModType.Intellect, value);
                    break;
                case Asda2PetStatType.Energy:
                    this.Owner.ApplyStatMod(ItemModType.Spirit, value);
                    break;
                case Asda2PetStatType.Dexterity:
                    this.Owner.ApplyStatMod(ItemModType.Agility, value);
                    break;
                case Asda2PetStatType.Luck:
                    this.Owner.ApplyStatMod(ItemModType.Luck, value);
                    break;
                case Asda2PetStatType.AllCapabilities:
                    this.Owner.ApplyStatMod(ItemModType.Strength, value);
                    this.Owner.ApplyStatMod(ItemModType.Agility, value);
                    this.Owner.ApplyStatMod(ItemModType.Intellect, value);
                    this.Owner.ApplyStatMod(ItemModType.Stamina, value);
                    this.Owner.ApplyStatMod(ItemModType.Luck, value);
                    this.Owner.ApplyStatMod(ItemModType.Spirit, value);
                    break;
                case Asda2PetStatType.MinAtack:
                    this.Owner.ApplyStatMod(ItemModType.Damage, value);
                    break;
                case Asda2PetStatType.MaxAtack:
                    this.Owner.ApplyStatMod(ItemModType.Damage, value);
                    break;
                case Asda2PetStatType.MinMaxAtack:
                    this.Owner.ApplyStatMod(ItemModType.Damage, value);
                    break;
                case Asda2PetStatType.MinMagicAtack:
                    this.Owner.ApplyStatMod(ItemModType.MagicDamage, value);
                    break;
                case Asda2PetStatType.MaxMagicAtack:
                    this.Owner.ApplyStatMod(ItemModType.MagicDamage, value);
                    break;
                case Asda2PetStatType.MinMaxMagicAtack:
                    this.Owner.ApplyStatMod(ItemModType.MagicDamage, value);
                    break;
                case Asda2PetStatType.MagicDeffence:
                    this.Owner.ApplyStatMod(ItemModType.Asda2MagicDefence,
                        (int) ((double) value * (double) CharacterFormulas.PetMagicDeffenceMultiplier));
                    break;
                case Asda2PetStatType.MinDeffence:
                    this.Owner.ApplyStatMod(ItemModType.Asda2Defence,
                        (int) ((double) value * (double) CharacterFormulas.PetDeffenceMultiplier));
                    break;
                case Asda2PetStatType.MaxDeffence:
                    this.Owner.ApplyStatMod(ItemModType.Asda2Defence,
                        (int) ((double) value * (double) CharacterFormulas.PetDeffenceMultiplier));
                    break;
                case Asda2PetStatType.MinMaxDeffence:
                    this.Owner.ApplyStatMod(ItemModType.Asda2Defence,
                        (int) ((double) value * (double) CharacterFormulas.PetMagicDeffenceMultiplier));
                    break;
                case Asda2PetStatType.DodgePrc:
                    this.Owner.ApplyStatMod(ItemModType.DodgeRating, value);
                    break;
                case Asda2PetStatType.CriticalPrc:
                    this.Owner.ApplyStatMod(ItemModType.MeleeCriticalStrikeRating, value);
                    this.Owner.ApplyStatMod(ItemModType.CriticalStrikeRating, value);
                    this.Owner.ApplyStatMod(ItemModType.SpellCriticalStrikeRating, value);
                    break;
                case Asda2PetStatType.ItemSellingPricePrc:
                    this.Owner.ApplyStatMod(ItemModType.SellingCost, value);
                    break;
                case Asda2PetStatType.StrengthPrc:
                    this.Owner.ApplyStatMod(ItemModType.StrengthPrc, value);
                    break;
                case Asda2PetStatType.StaminaPrc:
                    this.Owner.ApplyStatMod(ItemModType.StaminaPrc, value);
                    break;
                case Asda2PetStatType.IntellectPrc:
                    this.Owner.ApplyStatMod(ItemModType.IntelectPrc, value);
                    break;
                case Asda2PetStatType.DexterityPrc:
                    this.Owner.ApplyStatMod(ItemModType.AgilityPrc, value);
                    break;
                case Asda2PetStatType.LuckPrc:
                    this.Owner.ApplyStatMod(ItemModType.LuckPrc, value);
                    break;
                case Asda2PetStatType.AllCapabilitiesPrc:
                    this.Owner.ApplyStatMod(ItemModType.StrengthPrc, value);
                    this.Owner.ApplyStatMod(ItemModType.AgilityPrc, value);
                    this.Owner.ApplyStatMod(ItemModType.IntelectPrc, value);
                    this.Owner.ApplyStatMod(ItemModType.StaminaPrc, value);
                    this.Owner.ApplyStatMod(ItemModType.LuckPrc, value);
                    this.Owner.ApplyStatMod(ItemModType.EnergyPrc, value);
                    break;
                case Asda2PetStatType.MinAtackPrc:
                    this.Owner.ApplyStatMod(ItemModType.DamagePrc, value);
                    break;
                case Asda2PetStatType.MaxAtackPrc:
                    this.Owner.ApplyStatMod(ItemModType.DamagePrc, value);
                    break;
                case Asda2PetStatType.MinMaxAtackPrc:
                    this.Owner.ApplyStatMod(ItemModType.DamagePrc, value);
                    break;
                case Asda2PetStatType.MinMagicAtackPrc:
                    this.Owner.ApplyStatMod(ItemModType.MagicDamagePrc, value);
                    break;
                case Asda2PetStatType.MaxMagicAtackPrc:
                    this.Owner.ApplyStatMod(ItemModType.MagicDamagePrc, value);
                    break;
                case Asda2PetStatType.MinMaxMagicAtackPrc:
                    this.Owner.ApplyStatMod(ItemModType.MagicDamagePrc, value);
                    break;
                case Asda2PetStatType.MagicDeffencePrc:
                    this.Owner.ApplyStatMod(ItemModType.Asda2MagicDefencePrc, value);
                    break;
                case Asda2PetStatType.MinDeffencePrc:
                    this.Owner.ApplyStatMod(ItemModType.Asda2DefencePrc, value);
                    break;
                case Asda2PetStatType.MaxDeffencePrc:
                    this.Owner.ApplyStatMod(ItemModType.Asda2DefencePrc, value);
                    break;
                case Asda2PetStatType.MinMaxDeffencePrc:
                    this.Owner.ApplyStatMod(ItemModType.Asda2DefencePrc, value);
                    break;
            }
        }

        public void RemoveStatsFromOwner()
        {
            if (this.Stat1Type != Asda2PetStatType.None)
                this.ApplyStat(this.Stat1Type, -this.Stat1Value);
            if (this.Stat2Type != Asda2PetStatType.None)
                this.ApplyStat(this.Stat2Type, -this.Stat2Value);
            if (this.Stat3Type == Asda2PetStatType.None)
                return;
            this.ApplyStat(this.Stat3Type, -this.Stat3Value);
        }

        [Property] public byte MaxLevel { get; set; }

        [Property] public bool CanChangeName { get; set; }

        public bool IsMaxExpirience
        {
            get
            {
                return (int) this.Expirience >=
                       Asda2PetMgr.ExpTable[this.Template.Rank][this.Template.Rarity][(int) this.Level - 1];
            }
        }

        public Asda2PetRecord(PetTemplate template, Character owner)
        {
            this.Guid = (int) Asda2PetRecord._idGenerator.Next();
            this.Id = (short) template.Id;
            this.OwnerId = owner.EntityId.Low;
            this.Name = template.Name;
            this.Owner = owner;
            this.MaxLevel = (byte) template.MaxLevel;
            this._level = (byte) 1;
            this.Template = template;
            this.CanChangeName = true;
            this.HungerPrc = (byte) 100;
        }

        public Asda2PetRecord()
        {
        }

        public void Init(Character owner)
        {
            this.Template = Asda2PetMgr.PetTemplates[(int) this.Id];
            this.Owner = owner;
        }

        public static Asda2PetRecord[] LoadAll(Character owner)
        {
            Asda2PetRecord[] allByProperty =
                ActiveRecordBase<Asda2PetRecord>.FindAllByProperty("OwnerId", (object) owner.EntityId.Low);
            foreach (Asda2PetRecord asda2PetRecord in allByProperty)
                asda2PetRecord.Init(owner);
            return allByProperty;
        }

        public bool GainXp(int i)
        {
            if (this.Level == (byte) 10)
                return false;
            int num = Asda2PetMgr.ExpTable[this.Template.Rank][this.Template.Rarity][(int) this.Level - 1];
            if ((int) this.Level == (int) this.MaxLevel && (int) this.Expirience >= num)
            {
                this.Expirience = (short) num;
                return false;
            }

            this.Expirience += (short) i;
            if ((int) this.Level == (int) this.MaxLevel)
            {
                if ((int) this.Expirience > num)
                    this.Expirience = (short) num;
                Asda2PetHandler.SendUpdatePetExpResponse(this.Owner.Client, this, false);
                return true;
            }

            if ((int) this.Expirience > num)
            {
                this.RemoveStatsFromOwner();
                ++this.Level;
                this.AddStatsToOwner();
                Asda2CharacterHandler.SendUpdateStatsResponse(this.Owner.Client);
                Asda2CharacterHandler.SendUpdateStatsOneResponse(this.Owner.Client);
                GlobalHandler.UpdateCharacterPetInfoToArea(this.Owner);
                Asda2PetHandler.SendUpdatePetHungerResponse(this.Owner.Client, this);
            }

            Asda2PetHandler.SendUpdatePetExpResponse(this.Owner.Client, this, true);
            return true;
        }

        public void Feed(int i)
        {
            this.HungerPrc += (byte) i;
            if (this.HungerPrc > (byte) 100)
                this.HungerPrc = (byte) 100;
            Asda2PetHandler.SendUpdatePetHungerResponse(this.Owner.Client, this);
        }

        public void RemovePrcExp(int prc)
        {
            int num = (int) this.Expirience -
                      Asda2PetMgr.ExpTable[this.Template.Rank][this.Template.Rarity][(int) this.Level - 2];
            this.Expirience -= (short) (num - num * prc / 100);
        }
    }
}