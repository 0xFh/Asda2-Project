using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
    public class SpellSummonImmovableHandler : SpellSummonHandler
    {
        public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
        {
            NPC npc = base.Summon(cast, ref targetLoc, entry);
            npc.HasPermissionToMove = false;
            return npc;
        }
    }
}