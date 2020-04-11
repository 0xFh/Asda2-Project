using System;
using WCell.Constants.Talents;

namespace WCell.Constants.Pets
{
    public static class PetExtensions
    {
        private static readonly TalentTreeId[] TreesByPetTalentType = new TalentTreeId[4];

        static PetExtensions()
        {
            PetExtensions.TreesByPetTalentType[2] = TalentTreeId.PetTalentsCunning;
            PetExtensions.TreesByPetTalentType[0] = TalentTreeId.PetTalentsFerocity;
            PetExtensions.TreesByPetTalentType[1] = TalentTreeId.PetTalentsTenacity;
            PetExtensions.TreesByPetTalentType[3] = TalentTreeId.None;
        }

        public static bool HasAnyFlag(this PetFoodMask flags, PetFoodMask otherFlags)
        {
            return (flags & otherFlags) != (PetFoodMask) 0;
        }

        public static bool HasAnyFlag(this PetFoodMask flags, PetFoodType foodType)
        {
            return (flags & (PetFoodMask) (1 << (int) (foodType - 1 & (PetFoodType) 31))) != (PetFoodMask) 0;
        }

        public static TalentTreeId GetTalentTreeId(this PetTalentType type)
        {
            return PetExtensions.TreesByPetTalentType[(int) type];
        }
    }
}