using System;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Effects
{
    public class ProspectingEffectHandler : ItemConvertEffectHandler
    {
        public ProspectingEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            if (!this.m_cast.TargetItem.Template.Flags.HasFlag((Enum) ItemFlags.Prospectable))
                return SpellFailedReason.CantBeProspected;
            return base.Initialize();
        }

        public override LootEntryType LootEntryType
        {
            get { return LootEntryType.Prospecting; }
        }
    }
}