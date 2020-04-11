using System;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.RealmServer.Skills;

namespace WCell.RealmServer.NPCs
{
    [Serializable]
    public class CreatureFamily
    {
        public CreatureFamilyId Id;
        public string Name;

        /// <summary>Pets of this Level will have their max Scale</summary>
        public int MaxScaleLevel;

        public float MinScale;
        public float MaxScale;

        /// <summary>Scale step per level</summary>
        public float ScaleStep;

        public PetFoodMask PetFoodMask;
        public PetTalentType PetTalentType;
        public SkillLine SkillLine;

        public override string ToString()
        {
            return string.Format("{0} ({1})", (object) this.Name, (object) this.Id);
        }
    }
}