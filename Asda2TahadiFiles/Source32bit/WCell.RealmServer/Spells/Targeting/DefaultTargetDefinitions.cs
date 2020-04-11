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
      return DefaultTargetHandlers[(int) target];
    }

    public static TargetFilter GetTargetFilter(ImplicitSpellTargetType target)
    {
      return DefaultTargetHandlers[(int) target]?.Filter;
    }

    static DefaultTargetDefinitions()
    {
      InitTargetHandlers();
    }

    private static void InitTargetHandlers()
    {
      DefaultTargetHandlers[8] =
        new TargetDefinition(DefaultTargetAdders.AddAreaDest, null);
      DefaultTargetHandlers[22] = new TargetDefinition(
        DefaultTargetAdders.AddAreaSource, DefaultTargetFilters.IsHostile);
      DefaultTargetHandlers[15] = new TargetDefinition(
        DefaultTargetAdders.AddAreaDest, DefaultTargetFilters.IsHostile);
      DefaultTargetHandlers[16] = new TargetDefinition(
        DefaultTargetAdders.AddAreaDest, DefaultTargetFilters.IsHostile);
      DefaultTargetHandlers[37] =
        new TargetDefinition(DefaultTargetAdders.AddAllParty, null);
      DefaultTargetHandlers[20] = new TargetDefinition(
        DefaultTargetAdders.AddAreaSource, DefaultTargetFilters.IsAllied);
      DefaultTargetHandlers[33] = new TargetDefinition(
        DefaultTargetAdders.AddAreaDest, DefaultTargetFilters.IsAllied);
      DefaultTargetHandlers[29] = new TargetDefinition(
        DefaultTargetAdders.AddAreaDest, DefaultTargetFilters.IsAllied);
      DefaultTargetHandlers[31] = new TargetDefinition(
        DefaultTargetAdders.AddAreaSource, DefaultTargetFilters.IsFriendly);
      DefaultTargetHandlers[61] = new TargetDefinition(
        DefaultTargetAdders.AddAreaSource, DefaultTargetFilters.IsSamePartyAndClass);
      DefaultTargetHandlers[65] =
        new TargetDefinition(DefaultTargetAdders.AddSelection, null);
      DefaultTargetHandlers[45] =
        new TargetDefinition(DefaultTargetAdders.AddChain, null);
      DefaultTargetHandlers[53] =
        new TargetDefinition(DefaultTargetAdders.AddSelection, null);
      DefaultTargetHandlers[25] = new TargetDefinition(
        DefaultTargetAdders.AddSelection, DefaultTargetFilters.IsHostileOrHealable);
      DefaultTargetHandlers[47] =
        new TargetDefinition(DefaultTargetAdders.AddSelf, null);
      DefaultTargetHandlers[23] =
        new TargetDefinition(DefaultTargetAdders.AddObject, null);
      DefaultTargetHandlers[26] =
        new TargetDefinition(DefaultTargetAdders.AddItemOrObject, null);
      DefaultTargetHandlers[24] = new TargetDefinition(
        DefaultTargetAdders.AddAreaSource, DefaultTargetFilters.IsInFrontAndHostile);
      DefaultTargetHandlers[104] = new TargetDefinition(
        DefaultTargetAdders.AddAreaSource, DefaultTargetFilters.IsInFrontAndHostile);
      DefaultTargetHandlers[3] =
        new TargetDefinition(DefaultTargetAdders.AddAreaSource, null);
      DefaultTargetHandlers[52] =
        new TargetDefinition(DefaultTargetAdders.AddSelf, null);
      DefaultTargetHandlers[32] =
        new TargetDefinition(DefaultTargetAdders.AddSelf, null);
      DefaultTargetHandlers[72] =
        new TargetDefinition(DefaultTargetAdders.AddSelf, null);
      DefaultTargetHandlers[48] =
        new TargetDefinition(DefaultTargetAdders.AddSelf, null);
      DefaultTargetHandlers[63] = new TargetDefinition(
        DefaultTargetAdders.AddAreaDest, DefaultTargetFilters.IsHostile);
      DefaultTargetHandlers[73] =
        new TargetDefinition(DefaultTargetAdders.AddSelf, null);
      DefaultTargetHandlers[5] =
        new TargetDefinition(DefaultTargetAdders.AddPet, null);
      DefaultTargetHandlers[0] =
        new TargetDefinition(DefaultTargetAdders.AddSelf, null);
      DefaultTargetHandlers[57] = new TargetDefinition(
        DefaultTargetAdders.AddSelection, DefaultTargetFilters.IsFriendly);
      DefaultTargetHandlers[56] = new TargetDefinition(
        DefaultTargetAdders.AddAreaSource, DefaultTargetFilters.IsAllied);
      DefaultTargetHandlers[38] = new TargetDefinition(
        DefaultTargetAdders.AddSelection, DefaultTargetFilters.IsFriendly);
      DefaultTargetHandlers[77] = new TargetDefinition(
        DefaultTargetAdders.AddChannelObject, DefaultTargetFilters.IsHostile);
      DefaultTargetHandlers[1] =
        new TargetDefinition(DefaultTargetAdders.AddSelf, null);
      DefaultTargetHandlers[6] = new TargetDefinition(
        DefaultTargetAdders.AddSelection, DefaultTargetFilters.IsHostile);
      DefaultTargetHandlers[21] = new TargetDefinition(
        DefaultTargetAdders.AddSelection, DefaultTargetFilters.IsFriendly);
      DefaultTargetHandlers[35] = new TargetDefinition(
        DefaultTargetAdders.AddSelection, DefaultTargetFilters.IsAllied);
      DefaultTargetHandlers[4] = new TargetDefinition(
        DefaultTargetAdders.AddSelection, DefaultTargetFilters.IsHostile);
      DefaultTargetHandlers[50] =
        new TargetDefinition(DefaultTargetAdders.AddSelf, null);
      DefaultTargetHandlers[54] = new TargetDefinition(
        DefaultTargetAdders.AddAreaSource, DefaultTargetFilters.IsInFrontAndHostile);
      DefaultTargetHandlers[43] =
        new TargetDefinition(DefaultTargetAdders.AddSelf, null);
      DefaultTargetHandlers[41] =
        new TargetDefinition(DefaultTargetAdders.AddSelf, null);
      DefaultTargetHandlers[44] =
        new TargetDefinition(DefaultTargetAdders.AddSelf, null);
      DefaultTargetHandlers[42] =
        new TargetDefinition(DefaultTargetAdders.AddSelf, null);
      DefaultTargetHandlers[34] = new TargetDefinition(
        DefaultTargetAdders.AddAreaSource, DefaultTargetFilters.IsAllied);
      DefaultTargetHandlers[30] = new TargetDefinition(
        DefaultTargetAdders.AddAreaSource, DefaultTargetFilters.IsAllied);
    }
  }
}