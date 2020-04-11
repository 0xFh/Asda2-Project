using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Core.DBC;
using WCell.RealmServer.Skills;

namespace WCell.RealmServer.NPCs
{
    public class DBCCreatureFamilyConverter : AdvancedDBCRecordConverter<CreatureFamily>
    {
        public override CreatureFamily ConvertTo(byte[] rawData, ref int id)
        {
            CreatureFamily creatureFamily = new CreatureFamily()
            {
                Id = (CreatureFamilyId) (id = (int) DBCRecordConverter.GetUInt32(rawData, 0)),
                MinScale = DBCRecordConverter.GetFloat(rawData, 1),
                MaxScale = DBCRecordConverter.GetFloat(rawData, 3),
                MaxScaleLevel = DBCRecordConverter.GetInt32(rawData, 4),
                SkillLine = SkillHandler.Get(DBCRecordConverter.GetUInt32(rawData, 5)),
                PetFoodMask = (PetFoodMask) DBCRecordConverter.GetUInt32(rawData, 7),
                PetTalentType = (PetTalentType) DBCRecordConverter.GetUInt32(rawData, 8),
                Name = this.GetString(rawData, 10)
            };
            creatureFamily.ScaleStep = (creatureFamily.MaxScale - creatureFamily.MinScale) /
                                       (float) creatureFamily.MaxScaleLevel;
            return creatureFamily;
        }
    }
}