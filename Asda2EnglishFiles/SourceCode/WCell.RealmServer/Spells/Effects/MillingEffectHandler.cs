using System;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Effects
{
    public class MillingEffectHandler : ItemConvertEffectHandler
    {
        public MillingEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            if (!this.m_cast.TargetItem.Template.Flags.HasFlag((Enum) ItemFlags.Millable))
                return SpellFailedReason.CantBeMilled;
            return base.Initialize();
        }

        public override LootEntryType LootEntryType
        {
            get { return LootEntryType.Milling; }
        }
    }
}