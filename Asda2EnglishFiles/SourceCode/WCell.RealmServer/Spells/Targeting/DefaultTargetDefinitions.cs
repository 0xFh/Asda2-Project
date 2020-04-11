using WCell.Constants.Spells;
using WCell.Util;

namespace WCell.RealmServer.Spells.Targeting
{
    public static class DefaultTargetDefinitions
    {
        public static readonly TargetDefinition[] DefaultTargetHandlers =
            new TargetDefinition[(int) (Utility.GetMaxEnum<ImplicitSpellTargetType>() + 1)];

        /// <summary>
        /// Returns the handler and filter for the given target type
        /// </summary>
        public static TargetDefinition GetTargetDefinition(ImplicitSpellTargetType target)
        {
            return DefaultTargetDefinitions.DefaultTargetHandlers[(int) target];
        }

        public static TargetFilter GetTargetFilter(ImplicitSpellTargetType target)
        {
            return DefaultTargetDefinitions.DefaultTargetHandlers[(int) target]?.Filter;
        }

        static DefaultTargetDefinitions()
        {
            DefaultTargetDefinitions.InitTargetHandlers();
        }

        private static void InitTargetHandlers()
        {
            DefaultTargetDefinitions.DefaultTargetHandlers[8] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddAreaDest), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[22] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaSource), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsHostile)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[15] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaDest), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsHostile)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[16] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaDest), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsHostile)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[37] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddAllParty), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[20] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaSource), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsAllied)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[33] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaDest), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsAllied)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[29] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaDest), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsAllied)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[31] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaSource), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsFriendly)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[61] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaSource), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsSamePartyAndClass)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[65] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelection), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[45] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddChain), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[53] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelection), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[25] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddSelection), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsHostileOrHealable)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[47] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelf), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[23] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddObject), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[26] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddItemOrObject), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[24] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaSource), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsInFrontAndHostile)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[104] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaSource), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsInFrontAndHostile)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[3] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddAreaSource), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[52] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelf), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[32] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelf), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[72] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelf), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[48] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelf), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[63] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaDest), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsHostile)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[73] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelf), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[5] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddPet), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[0] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelf), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[57] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddSelection), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsFriendly)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[56] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaSource), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsAllied)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[38] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddSelection), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsFriendly)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[77] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddChannelObject), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsHostile)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[1] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelf), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[6] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddSelection), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsHostile)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[21] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddSelection), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsFriendly)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[35] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddSelection), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsAllied)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[4] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddSelection), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsHostile)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[50] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelf), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[54] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaSource), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsInFrontAndHostile)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[43] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelf), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[41] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelf), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[44] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelf), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[42] =
                new TargetDefinition(new TargetAdder(DefaultTargetAdders.AddSelf), (TargetFilter[]) null);
            DefaultTargetDefinitions.DefaultTargetHandlers[34] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaSource), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsAllied)
                });
            DefaultTargetDefinitions.DefaultTargetHandlers[30] = new TargetDefinition(
                new TargetAdder(DefaultTargetAdders.AddAreaSource), new TargetFilter[1]
                {
                    new TargetFilter(DefaultTargetFilters.IsAllied)
                });
        }
    }
}