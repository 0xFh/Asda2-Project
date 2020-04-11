using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
    /// <summary>
    /// 
    /// </summary>
    public class SpellSummonPetHandler : SpellSummonHandler
    {
        public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
        {
            Unit casterUnit = cast.CasterUnit;
            if (casterUnit is Character)
                return ((Character) casterUnit).SpawnPet(entry, ref targetLoc,
                    cast.Spell.GetDuration(casterUnit.SharedReference));
            return base.Summon(cast, ref targetLoc, entry);
        }
    }
}