using Castle.ActiveRecord;
using System;
using WCell.Constants.Items;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Handlers
{
    [Castle.ActiveRecord.ActiveRecord("GuildSkill", Access = PropertyAccess.Property)]
    public class GuildSkill : WCellRecord<GuildSkill>
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(GuildSkill), nameof(Guid), 1L);
        private bool _isActivated;
        private byte _level;

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }

        [Property]
        public bool IsActivated
        {
            get { return this._isActivated; }
            private set
            {
                this._isActivated = value;
                this.SaveLater();
            }
        }

        [Property] public GuildSkillId Id { get; set; }

        [Property]
        public byte Level
        {
            get { return this._level; }
            set
            {
                this._level = value;
                this.SaveLater();
            }
        }

        [Property] public uint GuildId { get; set; }

        public Guild Guild { get; set; }

        [Property] public DateTime LastMaintance { get; set; }

        public bool IsMaxLevel
        {
            get { return (int) this.Level == this.Template.MaxLevel; }
        }

        public int NextLearnCost
        {
            get
            {
                if (this.Template.LearnCosts.Length > (int) this.Level + 1)
                    return this.Template.LearnCosts[(int) this.Level];
                return int.MaxValue;
            }
        }

        public int ActivationCost
        {
            get { return this.Template.ActivationCosts[(int) this.Level]; }
        }

        public int MaintanceCost
        {
            get { return this.Template.MaitenceCosts[(int) this.Level]; }
        }

        public int ValueOnUse
        {
            get
            {
                return (int) ((double) this.Template.BonusValuses[(int) this.Level] *
                              Math.Pow((double) this.Guild.Level, 0.5));
            }
        }

        public static GuildSkill[] FindAll(Guild guild)
        {
            return ActiveRecordBase<GuildSkill>.FindAllByProperty("GuildId", (object) guild.Id);
        }

        public void ToggleActivate(Character trigerer)
        {
            if (this.IsActivated)
            {
                this.IsActivated = false;
                foreach (Character character in this.Guild.GetCharacters())
                    this.RemoveFromCharacter(character);
            }
            else
            {
                if (!this.Guild.SubstractGuildPoints(this.ActivationCost))
                {
                    Asda2GuildHandler.SendGuildSkillActivatedResponse(trigerer,
                        GuildSkillActivationStatus.IncefitientPoints, this);
                    return;
                }

                this.IsActivated = true;
                this.LastMaintance = DateTime.Now;
                foreach (Character character in this.Guild.GetCharacters())
                    this.ApplyToCharacter(character);
            }

            Asda2GuildHandler.SendGuildSkillActivatedResponse(trigerer, GuildSkillActivationStatus.Ok, this);
            Asda2GuildHandler.SendGuildSkillStatusChangedResponse(this, ClanSkillStatus.Activation);
            Asda2GuildHandler.SendUpdateGuildInfoResponse(this.Guild, GuildInfoMode.Silent, (Character) null);
            Asda2GuildHandler.SendGuildSkillsInfoToGuild(this.Guild);
        }

        public void ApplyToCharacter(Character character)
        {
            switch (this.Id)
            {
                case GuildSkillId.AtackPrc:
                    character.ApplyStatMod(ItemModType.DamagePrc, this.ValueOnUse);
                    character.ApplyStatMod(ItemModType.MagicDamagePrc, this.ValueOnUse);
                    break;
                case GuildSkillId.DeffencePrc:
                    character.ApplyStatMod(ItemModType.Asda2DefencePrc, this.ValueOnUse);
                    character.ApplyStatMod(ItemModType.Asda2MagicDefencePrc, this.ValueOnUse);
                    break;
                case GuildSkillId.MovingSpeedPrc:
                    character.ApplyStatMod(ItemModType.Speed, this.ValueOnUse);
                    break;
                case GuildSkillId.AtackSpeedPrc:
                    character.ApplyStatMod(ItemModType.AtackTimePrc, -this.ValueOnUse);
                    break;
                case GuildSkillId.Expirience:
                    character.ApplyStatMod(ItemModType.Asda2Expirience, this.ValueOnUse);
                    break;
            }

            Asda2CharacterHandler.SendUpdateStatsOneResponse(character.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(character.Client);
        }

        public void RemoveFromCharacter(Character character)
        {
            switch (this.Id)
            {
                case GuildSkillId.AtackPrc:
                    character.RemoveStatMod(ItemModType.DamagePrc, this.ValueOnUse);
                    character.RemoveStatMod(ItemModType.MagicDamagePrc, this.ValueOnUse);
                    break;
                case GuildSkillId.DeffencePrc:
                    character.RemoveStatMod(ItemModType.Asda2DefencePrc, this.ValueOnUse);
                    character.RemoveStatMod(ItemModType.Asda2MagicDefencePrc, this.ValueOnUse);
                    break;
                case GuildSkillId.MovingSpeedPrc:
                    character.RemoveStatMod(ItemModType.Speed, this.ValueOnUse);
                    break;
                case GuildSkillId.AtackSpeedPrc:
                    character.RemoveStatMod(ItemModType.AtackTimePrc, -this.ValueOnUse);
                    break;
                case GuildSkillId.Expirience:
                    character.RemoveStatMod(ItemModType.Asda2Expirience, this.ValueOnUse);
                    break;
            }

            Asda2CharacterHandler.SendUpdateStatsOneResponse(character.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(character.Client);
        }

        public GuildSkill()
        {
        }

        public GuildSkill(Guild guild, GuildSkillId id)
        {
            this.Guid = GuildSkill._idGenerator.Next();
            this.Id = id;
            this.Guild = guild;
            this.GuildId = guild.Id;
            this._level = (byte) 1;
            this.InitAfterLoad(guild);
        }

        public GuildSkillTemplate Template { get; private set; }

        public void InitAfterLoad(Guild g)
        {
            this.Template = GuildSkillTemplate.Templates[(int) this.Id];
            this.Guild = g;
        }
    }
}