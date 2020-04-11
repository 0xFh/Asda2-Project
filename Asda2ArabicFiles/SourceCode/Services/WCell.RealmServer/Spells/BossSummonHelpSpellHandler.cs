using WCell.Constants.World;
using WCell.RealmServer.AI;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
    internal class BossSummonHelpSpellHandler : SpellEffectHandler
    {
        public BossSummonHelpSpellHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }
        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            var npc = Cast.CasterUnit as NPC;
            if(npc == null)
                return;
            IWorldLocation dest = npc.Target;
            if (npc.Target == null)
                dest = npc;
            switch (npc.EntryId)
            {
                case 508:
                    if (npc.MapId != MapId.QueenPalace)
                        return;
                    BurdanSummon(npc, dest);
                    break;
                case 509:
                    if (npc.MapId != MapId.QueenPalace)
                        return;
                    BurdanSummon(npc, dest);
                    break;
                case 510://kaiya
                    if (npc.MapId != MapId.QueenPalace)
                        return;
                    KaiyaSummon(npc, dest);
                    break;
                case 512:
                    if (npc.MapId != MapId.NightValey)
                        return;
                    NemesisSummon(npc, dest);
                    break;
                case 513: 
                    if (npc.MapId != MapId.NightValey)
                        return;
                    NemesisSummon(npc, dest);
                    break;
                case 514:
                    if (npc.MapId != MapId.NightValey)
                        return;
                    NemesisSummon(npc, dest);
                    break;
                case 515:
                    if (npc.MapId != MapId.NightValey)
                        return;
                    BlackEagleSummon(npc, dest);
                    break;
                case 752:
                    if (npc.MapId != MapId.DecaronLab)
                        return;
                    BladeBossStarkSummon(npc, dest);
                    break;
                case 753:
                    if (npc.MapId != MapId.DecaronLab)
                        return;
                    ParasolBossSmartsuSummon(npc, dest);
                    break;
                case 754:
                    if (npc.MapId != MapId.DecaronLab)
                        return;
                    DuckWrenchBossSummon(npc, dest);
                    break;
                case 755:
                    if (npc.MapId != MapId.DecaronLab)
                        return;
                    IbrakoSummon(npc, dest);
                    break;
                    
            }
        }

        private void IbrakoSummon(NPC npc, IWorldLocation dest)
        {
            if (npc.HealthPct < 40 && !npc.HelperBossSummoned)
            {
                npc.HelperBossSummoned = true;
                SpawnMob(2, 2, 752, dest);
            }
            else if (npc.HealthPct >= 40)
            {
                SpawnMob(-2, -2, 751, npc);
                SpawnMob(0, -2, 747, dest); 
            }
        }

        private void DuckWrenchBossSummon(NPC npc, IWorldLocation dest)
        {
            SpawnMob(-2, -2, 749, npc);
        }

        private void ParasolBossSmartsuSummon(NPC npc, IWorldLocation dest)
        {
            SpawnMob(-2, -2, 750, npc);
        }

        private void BladeBossStarkSummon(NPC npc, IWorldLocation dest)
        {
            SpawnMob(-2, -2, 749, npc);
        }

        private void BurdanSummon(NPC npc, IWorldLocation dest)
        {
            SpawnMob(-2, -2, 313, npc);
            SpawnMob(-1, -2, 313, npc);
            SpawnMob(0, -2, 316, dest);
        }

        private void BlackEagleSummon(NPC npc, IWorldLocation dest)
        {

            if (npc.HealthPct < 40 && !npc.HelperBossSummoned)
            {
                npc.HelperBossSummoned = true;
                SpawnMob(2, 2, 512, dest); 
            }
            else if (npc.HealthPct >= 40)
            {
                SpawnMob(-2, -2, 392, npc);
                SpawnMob(-3, -2, 392, npc);
                SpawnMob(0, -2, 392, npc);
                SpawnMob(-1, -2, 392, npc);
                SpawnMob(0, -2, 377, dest);
                SpawnMob(1, -2, 377, dest);
            }
        }

        private void NemesisSummon(NPC npc, IWorldLocation dest)
        {
            SpawnMob(-2, -2, 392, npc);
            SpawnMob(-1, -2, 392, npc);
            SpawnMob(0, -2, 377, dest);
        }

        private void KaiyaSummon(NPC npc, IWorldLocation dest)
        {
            if (npc.HealthPct < 40 && !npc.HelperBossSummoned)
            {
                npc.HelperBossSummoned = true;
                SpawnMob(2, 2, 511, dest); //hysteric kaiya
            }
            else if (npc.HealthPct >= 40)
            {
                SpawnMob(-2, -2, 324, dest);
                SpawnMob(-1, -2, 325, dest);
                SpawnMob(0, -2, 326, dest);
                SpawnMob(1, -2, 327, dest);
            }
        }

        private void SpawnMob(int x, int y, uint npcId, IWorldLocation dest)
        {
            var pos = new Vector3(dest.Position.X + x, dest.Position.Y + y);
            var wl = new WorldLocation(dest.Map, pos);
            var newNpc = NPCMgr.GetEntry(npcId).SpawnAt(wl);
            newNpc.Brain.State = BrainState.Roam;
        }
    }
}