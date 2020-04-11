using WCell.Constants.NPCs;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Events.Asda2
{
    public static class NpcCustomEntries
    {
        public static void Init(int maxLevel, float healthMod, float otherStatsMod, float speedMod)
        {
            NpcCustomEntries.CreateCustomEntry(NPCId.FieldWolf4, NpcCustomEntryId.Type1FieldWolf, healthMod,
                otherStatsMod, maxLevel, speedMod, new NpcCustomEntries.NpcCustomEntrySettings()
                {
                    Health = new int?(1000),
                    Damage = new int?(100),
                    MDef = new int?(50),
                    PDef = new int?(50),
                    Speed = new float?(0.6f)
                }, 1 != 0);
            NpcCustomEntries.CreateCustomEntry(NPCId.LeafEdgeofFaith562, NpcCustomEntryId.Type2LeafEdgeOfFaith,
                healthMod, otherStatsMod, maxLevel, speedMod, new NpcCustomEntries.NpcCustomEntrySettings()
                {
                    Health = new int?(2000),
                    Damage = new int?(200),
                    MDef = new int?(100),
                    PDef = new int?(100),
                    Speed = new float?(0.6f)
                }, 1 != 0);
            NpcCustomEntries.CreateCustomEntry(NPCId.PatchCatofCourage561, NpcCustomEntryId.Type3PatchCatOfCourage,
                healthMod, otherStatsMod, maxLevel, speedMod, new NpcCustomEntries.NpcCustomEntrySettings()
                {
                    Health = new int?(3000),
                    Damage = new int?(300),
                    MDef = new int?(150),
                    PDef = new int?(150),
                    Speed = new float?(0.6f)
                }, 1 != 0);
            NpcCustomEntries.CreateCustomEntry(NPCId.DeckronTroops635, NpcCustomEntryId.Type4DecronTroops, healthMod,
                otherStatsMod, maxLevel, speedMod, new NpcCustomEntries.NpcCustomEntrySettings()
                {
                    Health = new int?(4500),
                    Damage = new int?(450),
                    MDef = new int?(250),
                    PDef = new int?(250),
                    Speed = new float?(1f)
                }, 1 != 0);
            NpcCustomEntries.CreateCustomEntry(NPCId.Viter640, NpcCustomEntryId.BossViter, healthMod, otherStatsMod,
                maxLevel, speedMod, new NpcCustomEntries.NpcCustomEntrySettings()
                {
                    Health = new int?(30000),
                    Damage = new int?(800),
                    MDef = new int?(400),
                    PDef = new int?(400),
                    Speed = new float?(0.8f)
                }, 1 != 0);
            NpcCustomEntries.CreateCustomEntry(NPCId.CommanderGeurantion630, NpcCustomEntryId.BossCommanderGeurantion,
                healthMod, otherStatsMod, maxLevel, speedMod, new NpcCustomEntries.NpcCustomEntrySettings()
                {
                    Health = new int?(40000),
                    Damage = new int?(1000),
                    MDef = new int?(500),
                    PDef = new int?(500),
                    Speed = new float?(0.7f)
                }, 1 != 0);
            NpcCustomEntries.CreateCustomEntry(NPCId.QueenKaiya510, NpcCustomEntryId.BossQueenKaiya, healthMod,
                otherStatsMod, maxLevel, speedMod, new NpcCustomEntries.NpcCustomEntrySettings()
                {
                    Health = new int?(350000),
                    Damage = new int?(2000),
                    MDef = new int?(1500),
                    PDef = new int?(1500),
                    Speed = new float?(0.7f)
                }, 1 != 0);
            NpcCustomEntries.CreateCustomEntry(NPCId.BlackEagle515, NpcCustomEntryId.BossBlackEagle, healthMod,
                otherStatsMod, maxLevel, speedMod, new NpcCustomEntries.NpcCustomEntrySettings()
                {
                    Health = new int?(700000),
                    Damage = new int?(3000),
                    MDef = new int?(3000),
                    PDef = new int?(3000),
                    Speed = new float?(0.7f)
                }, 1 != 0);
        }

        private static void CreateCustomEntry(NPCId npcId, NpcCustomEntryId customEntryId, float healthMod,
            float otherStatsMod, int maxLevel, float speedMod,
            NpcCustomEntries.NpcCustomEntrySettings npcCustomEntrySettings, bool isAggressive = true)
        {
            NPCEntry entry = NPCMgr.GetEntry(npcId).Clone<NPCEntry>();
            entry.SetLevel(maxLevel - 5);
            if (npcCustomEntrySettings.Health.HasValue)
                entry.SetHealth((int) ((double) npcCustomEntrySettings.Health.Value * (double) healthMod));
            if (npcCustomEntrySettings.PDef.HasValue)
                entry.Resistances[0] = (int) ((double) npcCustomEntrySettings.PDef.Value * (double) otherStatsMod);
            if (npcCustomEntrySettings.MDef.HasValue)
                entry.Resistances[1] = (int) ((double) npcCustomEntrySettings.MDef.Value * (double) otherStatsMod);
            if (npcCustomEntrySettings.Damage.HasValue)
            {
                entry.MinDamage = (float) npcCustomEntrySettings.Damage.Value * 0.9f * otherStatsMod;
                entry.MaxDamage = (float) npcCustomEntrySettings.Damage.Value * 1.1f * otherStatsMod;
            }

            if (npcCustomEntrySettings.Speed.HasValue)
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