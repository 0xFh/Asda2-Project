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
      CreatureFamily creatureFamily = new CreatureFamily
      {
        Id = (CreatureFamilyId) (id = (int) GetUInt32(rawData, 0)),
        MinScale = GetFloat(rawData, 1),
        MaxScale = GetFloat(rawData, 3),
        MaxScaleLevel = GetInt32(rawData, 4),
        SkillLine = SkillHandler.Get(GetUInt32(rawData, 5)),
        PetFoodMask = (PetFoodMask) GetUInt32(rawData, 7),
        PetTalentType = (PetTalentType) GetUInt32(rawData, 8),
        Name = GetString(rawData, 10)
      };
      creatureFamily.ScaleStep = (creatureFamily.MaxScale - creatureFamily.MinScale) /
                                 creatureFamily.MaxScaleLevel;
      return creatureFamily;
    }
  }
}