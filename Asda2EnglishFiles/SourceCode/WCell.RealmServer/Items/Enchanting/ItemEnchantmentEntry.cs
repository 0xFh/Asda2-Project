using WCell.Constants.Skills;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items.Enchanting
{
    /// <summary>
    /// 
    /// </summary>
    public class ItemEnchantmentEntry
    {
        public uint Id;
        public uint Charges;
        public ItemEnchantmentEffect[] Effects;
        public string Description;
        public uint Visual;
        public uint Flags;
        public uint SourceItemId;
        public uint ConditionId;
        public int RequiredSkillAmount;
        public ItemTemplate GemTemplate;

        /// <summary>
        /// 
        /// </summary>
        public ItemEnchantmentCondition Condition;

        public SkillId RequiredSkillId;

        public override string ToString()
        {
            return string.Format("{0} (Id: {1})", (object) this.Description, (object) this.Id);
        }

        public bool CheckRequirements(Unit enchanter)
        {
            if (enchanter is Character)
                return ((Character) enchanter).Skills.CheckSkill(this.RequiredSkillId, this.RequiredSkillAmount);
            return true;
        }
    }
}