using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
    /// <summary>Non-combat pets</summary>
    public class SpellSummonCritterHandler : SpellSummonHandler
    {
        public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
        {
            return base.Summon(cast, ref targetLoc, entry);
        }
    }
}