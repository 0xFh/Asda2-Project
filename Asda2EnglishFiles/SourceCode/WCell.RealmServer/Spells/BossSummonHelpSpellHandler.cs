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
            NPC casterUnit = this.Cast.CasterUnit as NPC;
            if (casterUnit == null)
                return;
            IWorldLocation dest = (IWorldLocation) casterUnit.Target;
            if (casterUnit.Target == null)
                dest = (IWorldLocation) casterUnit;
            switch (casterUnit.EntryId)
            {
                case 508:
                    if (casterUnit.MapId != MapId.QueenPalace)
                        break;
                    this.BurdanSummon(casterUnit, dest);
                    break;
                case 509:
                    if (casterUnit.MapId != MapId.QueenPalace)
                        break;
                    this.BurdanSummon(casterUnit, dest);
                    break;
                case 510:
                    if (casterUnit.MapId != MapId.QueenPalace)
                        break;
                    this.KaiyaSummon(casterUnit, dest);
                    break;
                case 512:
                    if (casterUnit.MapId != MapId.NightValey)
                        break;
                    this.NemesisSummon(casterUnit, dest);
                    break;
                case 513:
                    if (casterUnit.MapId != MapId.NightValey)
                        break;
                    this.NemesisSummon(casterUnit, dest);
                    break;
                case 514:
                    if (casterUnit.MapId != MapId.NightValey)
                        break;
                    this.NemesisSummon(casterUnit, dest);
                    break;
                case 515:
                    if (casterUnit.MapId != MapId.NightValey)
                        break;
                    this.BlackEagleSummon(casterUnit, dest);
                    break;
                case 752:
                    if (casterUnit.MapId != MapId.DecaronLab)
                        break;
                    this.BladeBossStarkSummon(casterUnit, dest);
                    break;
                case 753:
                    if (casterUnit.MapId != MapId.DecaronLab)
                        break;
                    this.ParasolBossSmartsuSummon(casterUnit, dest);
                    break;
                case 754:
                    if (casterUnit.MapId != MapId.DecaronLab)
                        break;
                    this.DuckWrenchBossSummon(casterUnit, dest);
                    break;
                case 755:
                    if (casterUnit.MapId != MapId.DecaronLab)
                        break;
                    this.IbrakoSummon(casterUnit, dest);
                    break;
            }
        }

        private void IbrakoSummon(NPC npc, IWorldLocation dest)
        {
            if (npc.HealthPct < 40 && !npc.HelperBossSummoned)
            {
                npc.HelperBossSummoned = true;
                this.SpawnMob(2, 2, 752U, dest);
            }
            else
            {
                if (npc.HealthPct < 40)
                    return;
                this.SpawnMob(-2, -2, 751U, (IWorldLocation) npc);
                this.SpawnMob(0, -2, 747U, dest);
            }
        }

        private void DuckWrenchBossSummon(NPC npc, IWorldLocation dest)
        {
            this.SpawnMob(-2, -2, 749U, (IWorldLocation) npc);
        }

        private void ParasolBossSmartsuSummon(NPC npc, IWorldLocation dest)
        {
            this.SpawnMob(-2, -2, 750U, (IWorldLocation) npc);
        }

        private void BladeBossStarkSummon(NPC npc, IWorldLocation dest)
        {
            this.SpawnMob(-2, -2, 749U, (IWorldLocation) npc);
        }

        private void BurdanSummon(NPC npc, IWorldLocation dest)
        {
            this.SpawnMob(-2, -2, 313U, (IWorldLocation) npc);
            this.SpawnMob(-1, -2, 313U, (IWorldLocation) npc);
            this.SpawnMob(0, -2, 316U, dest);
        }

        private void BlackEagleSummon(NPC npc, IWorldLocation dest)
        {
            if (npc.HealthPct < 40 && !npc.HelperBossSummoned)
            {
                npc.HelperBossSummoned = true;
                this.SpawnMob(2, 2, 512U, dest);
            }
            else
            {
                if (npc.HealthPct < 40)
                    return;
                this.SpawnMob(-2, -2, 392U, (IWorldLocation) npc);
                this.SpawnMob(-3, -2, 392U, (IWorldLocation) npc);
                this.SpawnMob(0, -2, 392U, (IWorldLocation) npc);
                this.SpawnMob(-1, -2, 392U, (IWorldLocation) npc);
                this.SpawnMob(0, -2, 377U, dest);
                this.SpawnMob(1, -2, 377U, dest);
            }
        }

        private void NemesisSummon(NPC npc, IWorldLocation dest)
        {
            this.SpawnMob(-2, -2, 392U, (IWorldLocation) npc);
            this.SpawnMob(-1, -2, 392U, (IWorldLocation) npc);
            this.SpawnMob(0, -2, 377U, dest);
        }

        private void KaiyaSummon(NPC npc, IWorldLocation dest)
        {
            if (npc.HealthPct < 40 && !npc.HelperBossSummoned)
            {
                npc.HelperBossSummoned = true;
                this.SpawnMob(2, 2, 511U, dest);
            }
            else
            {
                if (npc.HealthPct < 40)
                    return;
                this.SpawnMob(-2, -2, 324U, dest);
                this.SpawnMob(-1, -2, 325U, dest);
                this.SpawnMob(0, -2, 326U, dest);
                this.SpawnMob(1, -2, 327U, dest);
            }
        }

        private void SpawnMob(int x, int y, uint npcId, IWorldLocation dest)
        {
            Vector3 pos = new Vector3(dest.Position.X + (float) x, dest.Position.Y + (float) y);
            WorldLocation worldLocation = new WorldLocation(dest.Map, pos, 1U);
            NPCMgr.GetEntry(npcId).SpawnAt((IWorldLocation) worldLocation, false).Brain.State = BrainState.Roam;
        }
    }
}