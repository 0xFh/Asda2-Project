using WCell.Constants.Looting;
using WCell.Constants.Misc;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Disenchant Visual (Id: 61335)</summary>
    public class DisenchantEffectHandler : SpellEffectHandler
    {
        public DisenchantEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            if (this.m_cast.TargetItem == null)
                return SpellFailedReason.ItemNotReady;
            ItemTemplate template = this.m_cast.TargetItem.Template;
            if (template.RequiredDisenchantingLevel == -1)
                return SpellFailedReason.CantBeDisenchanted;
            return (long) template.RequiredDisenchantingLevel >
                   (long) this.m_cast.CasterChar.Skills.GetValue(SkillId.Enchanting)
                ? SpellFailedReason.CantBeDisenchantedSkill
                : SpellFailedReason.Ok;
        }

        public override void Apply()
        {
            Character casterChar = this.m_cast.CasterChar;
            casterChar.Emote(EmoteType.SimpleTalk);
            LootMgr.CreateAndSendObjectLoot((ILootable) this.m_cast.TargetItem, casterChar, LootEntryType.Disenchanting,
                false);
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Player; }
        }
    }
}