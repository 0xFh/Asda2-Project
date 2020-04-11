using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
    public class SpellSummonDoomguardHandler : SpellSummonHandler
    {
        public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
        {
            NPC npc = entry.SpawnAt(cast.Map, targetLoc, false);
            npc.RemainingDecayDelayMillis = cast.Spell.GetDuration(cast.CasterReference);
            npc.Creator = cast.CasterReference.EntityId;
            return npc;
        }
    }
}