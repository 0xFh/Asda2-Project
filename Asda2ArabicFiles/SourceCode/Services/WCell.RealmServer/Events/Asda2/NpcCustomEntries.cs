using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Events.Asda2
{
  public static class NpcCustomEntries
  {
    public static void Init(int maxLevel, float healthMod, float otherStatsMod, float speedMod)
    {
      CreateCustomEntry(NPCId.EventDeckronTroops814, NpcCustomEntryId.Type1FieldWolf, healthMod, otherStatsMod, maxLevel, speedMod,
        new NpcCustomEntrySettings { Health = 10000, Damage = 1000, MDef = 50, PDef = 50, Speed = 0.7f });
      CreateCustomEntry(NPCId.EventViologer809, NpcCustomEntryId.Type2LeafEdgeOfFaith, healthMod, otherStatsMod, maxLevel, speedMod,
        new NpcCustomEntrySettings { Health = 20000, Damage = 2000, MDef = 100, PDef = 100, Speed = 0.7f });
      CreateCustomEntry(NPCId.EventWikiBlow810, NpcCustomEntryId.Type3PatchCatOfCourage, healthMod, otherStatsMod, maxLevel, speedMod,
        new NpcCustomEntrySettings { Health = 30000, Damage = 3000, MDef = 150, PDef = 150, Speed = 0.7f });
      CreateCustomEntry(NPCId.EventLoaderwalk811, NpcCustomEntryId.Type4DecronTroops, healthMod, otherStatsMod, maxLevel, speedMod,
        new NpcCustomEntrySettings { Health = 45000, Damage = 4500, MDef = 250, PDef = 250, Speed = 1f });
      CreateCustomEntry(NPCId.EventBudran812, NpcCustomEntryId.BossViter, healthMod, otherStatsMod, maxLevel, speedMod,
        new NpcCustomEntrySettings { Health = 300000, Damage = 8000, MDef = 400, PDef = 400, Speed = 0.8f });
      CreateCustomEntry(NPCId.EventKaiya813, NpcCustomEntryId.BossCommanderGeurantion, healthMod, otherStatsMod, maxLevel, speedMod,
        new NpcCustomEntrySettings { Health = 350000, Damage = 1000, MDef = 500, PDef = 500, Speed = 0.7f });
      CreateCustomEntry(NPCId.QueenKaiya510, NpcCustomEntryId.BossQueenKaiya, healthMod, otherStatsMod, maxLevel, speedMod,
        new NpcCustomEntrySettings { Health = 350000, Damage = 2000, MDef = 1500, PDef = 1500, Speed = 0.7f });
      CreateCustomEntry(NPCId.BlackEagle515, NpcCustomEntryId.BossBlackEagle, healthMod, otherStatsMod, maxLevel, speedMod,
        new NpcCustomEntrySettings { Health = 700000, Damage = 3000, MDef = 3000, PDef = 3000, Speed = 0.7f });

    }

    private static void CreateCustomEntry(NPCId npcId, NpcCustomEntryId customEntryId, float healthMod, float otherStatsMod, int maxLevel, float speedMod, NpcCustomEntrySettings npcCustomEntrySettings, bool isAggressive = true)
    {
      var entry = NPCMgr.GetEntry(npcId).Clone();
      entry.SetLevel(maxLevel - 5);
      if (npcCustomEntrySettings.Health != null)
        entry.SetHealth((int)(npcCustomEntrySettings.Health.Value * healthMod));
      if (npcCustomEntrySettings.PDef != null)
        entry.Resistances[(int)DamageSchool.Physical] = (int)(npcCustomEntrySettings.PDef.Value * otherStatsMod);
      if (npcCustomEntrySettings.MDef != null)
        entry.Resistances[(int)DamageSchool.Magical] = (int)(npcCustomEntrySettings.MDef.Value * otherStatsMod);
      if (npcCustomEntrySettings.Damage != null)
      {
        entry.MinDamage = npcCustomEntrySettings.Damage.Value * 0.9f * otherStatsMod;
        entry.MaxDamage = npcCustomEntrySettings.Damage.Value * 1.1f * otherStatsMod;
      }
      if (npcCustomEntrySettings.Speed != null)
      {
        entry.RunSpeed = npcCustomEntrySettings.Speed.Value * speedMod;
        entry.WalkSpeed = npcCustomEntrySettings.Speed.Value * speedMod;
      }
      entry.IsAgressive = isAggressive;
      NPCMgr.AddEntry((uint)customEntryId, entry);
      entry.NPCId = npcId;
    }

    class NpcCustomEntrySettings
    {
      public int? Health { get; set; }
      public int? PDef { get; set; }
      public int? MDef { get; set; }
      public int? Damage { get; set; }
      public float? Speed { get; set; }
    }
  }
}