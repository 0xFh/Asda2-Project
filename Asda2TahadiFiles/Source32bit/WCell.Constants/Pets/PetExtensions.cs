using System;
using WCell.Constants.Talents;

namespace WCell.Constants.Pets
{
  public static class PetExtensions
  {
    private static readonly TalentTreeId[] TreesByPetTalentType = new TalentTreeId[4];

    static PetExtensions()
    {
      TreesByPetTalentType[2] = TalentTreeId.PetTalentsCunning;
      TreesByPetTalentType[0] = TalentTreeId.PetTalentsFerocity;
      TreesByPetTalentType[1] = TalentTreeId.PetTalentsTenacity;
      TreesByPetTalentType[3] = TalentTreeId.None;
    }

    public static bool HasAnyFlag(this PetFoodMask flags, PetFoodMask otherFlags)
    {
      return (flags & otherFlags) != 0;
    }

    public static bool HasAnyFlag(this PetFoodMask flags, PetFoodType foodType)
    {
      return (flags & (PetFoodMask) (1 << (int) (foodType - 1 & (PetFoodType) 31))) != 0;
    }

    public static TalentTreeId GetTalentTreeId(this PetTalentType type)
    {
      return TreesByPetTalentType[(int) type];
    }
  }
}