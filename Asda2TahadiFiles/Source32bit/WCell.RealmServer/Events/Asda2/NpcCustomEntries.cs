using WCell.Constants.NPCs;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Events.Asda2
{
  public static class NpcCustomEntries
  {
    public static void Init(int maxLevel, float healthMod, float otherStatsMod, float speedMod)
    {
      CreateCustomEntry(NPCId.FieldWolf4, NpcCustomEntryId.Type1FieldWolf, healthMod,
        otherStatsMod, maxLevel, speedMod, new NpcCustomEntrySettings
        {
          Health = 1000,
          Damage = 100,
          MDef = 50,
          PDef = 50,
          Speed = 0.6f
        }, 1 != 0);
      CreateCustomEntry(NPCId.LeafEdgeofFaith562, NpcCustomEntryId.Type2LeafEdgeOfFaith,
        healthMod, otherStatsMod, maxLevel, speedMod, new NpcCustomEntrySettings
        {
          Health = 2000,
          Damage = 200,
          MDef = 100,
          PDef = 100,
          Speed = 0.6f
        }, 1 != 0);
      CreateCustomEntry(NPCId.PatchCatofCourage561, NpcCustomEntryId.Type3PatchCatOfCourage,
        healthMod, otherStatsMod, maxLevel, speedMod, new NpcCustomEntrySettings
        {
          Health = 3000,
          Damage = 300,
          MDef = 150,
          PDef = 150,
          Speed = 0.6f
        }, 1 != 0);
      CreateCustomEntry(NPCId.DeckronTroops635, NpcCustomEntryId.Type4DecronTroops, healthMod,
        otherStatsMod, maxLevel, speedMod, new NpcCustomEntrySettings
        {
          Health = 4500,
          Damage = 450,
          MDef = 250,
          PDef = 250,
          Speed = 1f
        }, 1 != 0);
      CreateCustomEntry(NPCId.Viter640, NpcCustomEntryId.BossViter, healthMod, otherStatsMod,
        maxLevel, speedMod, new NpcCustomEntrySettings
        {
          Health = 30000,
          Damage = 800,
          MDef = 400,
          PDef = 400,
          Speed = 0.8f
        }, 1 != 0);
      CreateCustomEntry(NPCId.CommanderGeurantion630, NpcCustomEntryId.BossCommanderGeurantion,
        healthMod, otherStatsMod, maxLevel, speedMod, new NpcCustomEntrySettings
        {
          Health = 40000,
          Damage = 1000,
          MDef = 500,
          PDef = 500,
          Speed = 0.7f
        }, 1 != 0);
      CreateCustomEntry(NPCId.QueenKaiya510, NpcCustomEntryId.BossQueenKaiya, healthMod,
        otherStatsMod, maxLevel, speedMod, new NpcCustomEntrySettings
        {
          Health = 350000,
          Damage = 2000,
          MDef = 1500,
          PDef = 1500,
          Speed = 0.7f
        }, 1 != 0);
      CreateCustomEntry(NPCId.BlackEagle515, NpcCustomEntryId.BossBlackEagle, healthMod,
        otherStatsMod, maxLevel, speedMod, new NpcCustomEntrySettings
        {
          Health = 700000,
          Damage = 3000,
          MDef = 3000,
          PDef = 3000,
          Speed = 0.7f
        }, 1 != 0);
    }

    private static void CreateCustomEntry(NPCId npcId, NpcCustomEntryId customEntryId, float healthMod,
      float otherStatsMod, int maxLevel, float speedMod,
      NpcCustomEntrySettings npcCustomEntrySettings, bool isAggressive = true)
    {
      NPCEntry entry = NPCMgr.GetEntry(npcId).Clone();
      entry.SetLevel(maxLevel - 5);
      if(npcCustomEntrySettings.Health.HasValue)
        entry.SetHealth((int) (npcCustomEntrySettings.Health.Value * (double) healthMod));
      if(npcCustomEntrySettings.PDef.HasValue)
        entry.Resistances[0] = (int) (npcCustomEntrySettings.PDef.Value * (double) otherStatsMod);
      if(npcCustomEntrySettings.MDef.HasValue)
        entry.Resistances[1] = (int) (npcCustomEntrySettings.MDef.Value * (double) otherStatsMod);
      if(npcCustomEntrySettings.Damage.HasValue)
      {
        entry.MinDamage = npcCustomEntrySettings.Damage.Value * 0.9f * otherStatsMod;
        entry.MaxDamage = npcCustomEntrySettings.Damage.Value * 1.1f * otherStatsMod;
      }

      if(npcCustomEntrySettings.Speed.HasValue)
      {
        entry.RunSpeed = npcCustomEntrySettings.Speed.Value * speedMod;
        entry.WalkSpeed = npcCustomEntrySettings.Speed.Value * speedMod;
      }

      entry.IsAgressive = isAggressive;
      NPCMgr.AddEntry((uint) customEntryId, entry);
      entry.NPCId = npcId;
    }

    private class NpcCustomEntrySettings
    {
      public int? Health { get; set; }

      public int? PDef { get; set; }

      public int? MDef { get; set; }

      public int? Damage { get; set; }

      public float? Speed { get; set; }
    }
  }
}