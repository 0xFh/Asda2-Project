using System;
using Castle.ActiveRecord;
using WCell.Constants.Items;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Handlers
{
    [ActiveRecord("GuildSkill", Access = PropertyAccess.Property)]
    public class GuildSkill : WCellRecord<GuildSkill>
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(GuildSkill), "Guid");

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid
        {
            get;
            set;
        }
        private bool _isActivated;
        private byte _level;

        [Property]
        public bool IsActivated
        {
            get { return _isActivated; }
            private set
            {
                _isActivated = value; this.SaveLater();              
            }
        }
        [Property]
        public GuildSkillId Id { get; set; }
        [Property]
        public byte Level
        {
            get { return _level; }
            set { _level = value;
                this.SaveLater();
            }
        }

        [Property]
        public uint GuildId { get; set; }

        public Guild Guild { get; set; }

        [Property]
        public DateTime LastMaintance { get; set; }

        public bool IsMaxLevel
        {
            get { return Level == Template.MaxLevel; }
        }
        public int NextLearnCost { get { return Template.LearnCosts.Length<=Level+1?int.MaxValue: Template.LearnCosts[Level]; } }
        public int ActivationCost { get { return Template.ActivationCosts[Level]; } }
        public int MaintanceCost { get { return Template.MaitenceCosts[Level]; } }
        public int ValueOnUse { get { return (int) (Template.BonusValuses[Level]*Math.Pow(Guild.Level,0.5)); } }
        public static GuildSkill[] FindAll(Guild guild)
        {
            return FindAllByProperty("GuildId", guild.Id);
        }
        public void ToggleActivate(Character trigerer)
        {
            if(IsActivated)
            {
                //deactivate
                IsActivated = false;
                //cancel skill efect from char
                foreach (var character in Guild.GetCharacters())
                {
                    RemoveFromCharacter(character);
                }
            }
            else
            {
                if (!Guild.SubstractGuildPoints(ActivationCost))
               {
                   Asda2GuildHandler.SendGuildSkillActivatedResponse(trigerer,GuildSkillActivationStatus.IncefitientPoints, this);
                   return;
               }
                //activate
                IsActivated = true;
                //appky skill efect to char
                LastMaintance = DateTime.Now;
                foreach (var character in Guild.GetCharacters())
                {
                    ApplyToCharacter(character);
                }
            }
            Asda2GuildHandler.SendGuildSkillActivatedResponse(trigerer, GuildSkillActivationStatus.Ok, this);
            Asda2GuildHandler.SendGuildSkillStatusChangedResponse(this,ClanSkillStatus.Activation);
            Asda2GuildHandler.SendUpdateGuildInfoResponse(Guild);
            Asda2GuildHandler.SendGuildSkillsInfoToGuild(Guild);
        }
        public void ApplyToCharacter (Character character)
        {
            switch (Id)
            {
                case GuildSkillId.AtackPrc:
                    character.ApplyStatMod(ItemModType.DamagePrc, ValueOnUse);
                    character.ApplyStatMod(ItemModType.MagicDamagePrc, ValueOnUse);
                    break;
                case GuildSkillId.DeffencePrc:
                    character.ApplyStatMod(ItemModType.Asda2DefencePrc, ValueOnUse);
                    character.ApplyStatMod(ItemModType.Asda2MagicDefencePrc, ValueOnUse);
                    break;
                case GuildSkillId.AtackSpeedPrc:
                    character.ApplyStatMod(ItemModType.AtackTimePrc, -ValueOnUse);
                    break;
                case GuildSkillId.MovingSpeedPrc:
                    character.ApplyStatMod(ItemModType.Speed, ValueOnUse);
                    break;
                case GuildSkillId.Expirience:
                    character.ApplyStatMod(ItemModType.Asda2Expirience, ValueOnUse);
                    break;
            }
            Asda2CharacterHandler.SendUpdateStatsOneResponse(character.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(character.Client);
        }
        public void RemoveFromCharacter (Character character)
        {
            switch (Id)
            {
                case GuildSkillId.AtackPrc:
                    character.RemoveStatMod(ItemModType.DamagePrc, ValueOnUse);
                    character.RemoveStatMod(ItemModType.MagicDamagePrc, ValueOnUse);
                    break;
                case GuildSkillId.DeffencePrc:
                    character.RemoveStatMod(ItemModType.Asda2DefencePrc, ValueOnUse);
                    character.RemoveStatMod(ItemModType.Asda2MagicDefencePrc, ValueOnUse);
                    break;
                case GuildSkillId.AtackSpeedPrc:
                    character.RemoveStatMod(ItemModType.AtackTimePrc, -ValueOnUse);
                    break;
                case GuildSkillId.MovingSpeedPrc:
                    character.RemoveStatMod(ItemModType.Speed, ValueOnUse);
                    break;
                case GuildSkillId.Expirience:
                    character.RemoveStatMod(ItemModType.Asda2Expirience, ValueOnUse);
                    break;
            }
            Asda2CharacterHandler.SendUpdateStatsOneResponse(character.Client);
            Asda2CharacterHandler.SendUpdateStatsResponse(character.Client);
        }
        public GuildSkill(){}
        public GuildSkill(Guild guild, GuildSkillId id)
        {
            Guid = _idGenerator.Next();
            Id = id;
            Guild = guild;
            GuildId = guild.Id;
            _level = 1;
            InitAfterLoad(guild);
        }

        public GuildSkillTemplate Template { get; private set; }
        public void InitAfterLoad(Guild g)
        {
            Template = GuildSkillTemplate.Templates[(int) Id];
            Guild = g;
        }
    }

    public enum GuildSkillId : short
    {
        AtackPrc =0,
        DeffencePrc = 1,
        MovingSpeedPrc = 2,
        AtackSpeedPrc = 3,
        Expirience = 4
    }
}