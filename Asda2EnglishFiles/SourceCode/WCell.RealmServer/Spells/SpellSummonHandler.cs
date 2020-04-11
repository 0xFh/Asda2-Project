using System;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
    [Serializable]
    public class SpellSummonHandler
    {
        public virtual bool CanSummon(SpellCast cast, NPCEntry entry)
        {
            return true;
        }

        public virtual NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
        {
            Unit casterUnit = cast.CasterUnit;
            int duration = cast.Spell.GetDuration(cast.CasterReference);
            NPC npc;
            if (casterUnit != null)
            {
                npc = casterUnit.SpawnMinion(entry, ref targetLoc, duration);
            }
            else
            {
                npc = entry.Create(cast.TargetMap.DifficultyIndex);
                npc.Position = targetLoc;
                npc.Brain.IsRunning = true;
                npc.Phase = cast.Phase;
                cast.Map.AddObject((WorldObject) npc);
            }

            if (casterUnit is Character)
                npc.Level = casterUnit.Level;
            npc.Summoner = casterUnit;
            npc.Creator = cast.CasterReference.EntityId;
            if (casterUnit != null)
            {
                casterUnit.Summon = npc.EntityId;
                if (casterUnit.HasMaster)
                    npc.Master = casterUnit.Master;
            }

            return npc;
        }
    }
}