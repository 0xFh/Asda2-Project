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
      NPC casterUnit = Cast.CasterUnit as NPC;
      if(casterUnit == null)
        return;
      IWorldLocation dest = casterUnit.Target;
      if(casterUnit.Target == null)
        dest = casterUnit;
      switch(casterUnit.EntryId)
      {
        case 508:
          if(casterUnit.MapId != MapId.QueenPalace)
            break;
          BurdanSummon(casterUnit, dest);
          break;
        case 509:
          if(casterUnit.MapId != MapId.QueenPalace)
            break;
          BurdanSummon(casterUnit, dest);
          break;
        case 510:
          if(casterUnit.MapId != MapId.QueenPalace)
            break;
          KaiyaSummon(casterUnit, dest);
          break;
        case 512:
          if(casterUnit.MapId != MapId.NightValey)
            break;
          NemesisSummon(casterUnit, dest);
          break;
        case 513:
          if(casterUnit.MapId != MapId.NightValey)
            break;
          NemesisSummon(casterUnit, dest);
          break;
        case 514:
          if(casterUnit.MapId != MapId.NightValey)
            break;
          NemesisSummon(casterUnit, dest);
          break;
        case 515:
          if(casterUnit.MapId != MapId.NightValey)
            break;
          BlackEagleSummon(casterUnit, dest);
          break;
        case 752:
          if(casterUnit.MapId != MapId.DecaronLab)
            break;
          BladeBossStarkSummon(casterUnit, dest);
          break;
        case 753:
          if(casterUnit.MapId != MapId.DecaronLab)
            break;
          ParasolBossSmartsuSummon(casterUnit, dest);
          break;
        case 754:
          if(casterUnit.MapId != MapId.DecaronLab)
            break;
          DuckWrenchBossSummon(casterUnit, dest);
          break;
        case 755:
          if(casterUnit.MapId != MapId.DecaronLab)
            break;
          IbrakoSummon(casterUnit, dest);
          break;
      }
    }

    private void IbrakoSummon(NPC npc, IWorldLocation dest)
    {
      if(npc.HealthPct < 40 && !npc.HelperBossSummoned)
      {
        npc.HelperBossSummoned = true;
        SpawnMob(2, 2, 752U, dest);
      }
      else
      {
        if(npc.HealthPct < 40)
          return;
        SpawnMob(-2, -2, 751U, npc);
        SpawnMob(0, -2, 747U, dest);
      }
    }

    private void DuckWrenchBossSummon(NPC npc, IWorldLocation dest)
    {
      SpawnMob(-2, -2, 749U, npc);
    }

    private void ParasolBossSmartsuSummon(NPC npc, IWorldLocation dest)
    {
      SpawnMob(-2, -2, 750U, npc);
    }

    private void BladeBossStarkSummon(NPC npc, IWorldLocation dest)
    {
      SpawnMob(-2, -2, 749U, npc);
    }

    private void BurdanSummon(NPC npc, IWorldLocation dest)
    {
      SpawnMob(-2, -2, 313U, npc);
      SpawnMob(-1, -2, 313U, npc);
      SpawnMob(0, -2, 316U, dest);
    }

    private void BlackEagleSummon(NPC npc, IWorldLocation dest)
    {
      if(npc.HealthPct < 40 && !npc.HelperBossSummoned)
      {
        npc.HelperBossSummoned = true;
        SpawnMob(2, 2, 512U, dest);
      }
      else
      {
        if(npc.HealthPct < 40)
          return;
        SpawnMob(-2, -2, 392U, npc);
        SpawnMob(-3, -2, 392U, npc);
        SpawnMob(0, -2, 392U, npc);
        SpawnMob(-1, -2, 392U, npc);
        SpawnMob(0, -2, 377U, dest);
        SpawnMob(1, -2, 377U, dest);
      }
    }

    private void NemesisSummon(NPC npc, IWorldLocation dest)
    {
      SpawnMob(-2, -2, 392U, npc);
      SpawnMob(-1, -2, 392U, npc);
      SpawnMob(0, -2, 377U, dest);
    }

    private void KaiyaSummon(NPC npc, IWorldLocation dest)
    {
      if(npc.HealthPct < 40 && !npc.HelperBossSummoned)
      {
        npc.HelperBossSummoned = true;
        SpawnMob(2, 2, 511U, dest);
      }
      else
      {
        if(npc.HealthPct < 40)
          return;
        SpawnMob(-2, -2, 324U, dest);
        SpawnMob(-1, -2, 325U, dest);
        SpawnMob(0, -2, 326U, dest);
        SpawnMob(1, -2, 327U, dest);
      }
    }

    private void SpawnMob(int x, int y, uint npcId, IWorldLocation dest)
    {
      Vector3 pos = new Vector3(dest.Position.X + x, dest.Position.Y + y);
      WorldLocation worldLocation = new WorldLocation(dest.Map, pos, 1U);
      NPCMgr.GetEntry(npcId).SpawnAt(worldLocation, false).Brain.State = BrainState.Roam;
    }
  }
}