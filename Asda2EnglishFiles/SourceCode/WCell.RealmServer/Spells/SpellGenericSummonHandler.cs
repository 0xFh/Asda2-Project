using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
    public class SpellGenericSummonHandler : SpellSummonHandler
    {
        public SpellGenericSummonHandler.Delegate Callback { get; set; }

        public SpellGenericSummonHandler(SpellGenericSummonHandler.Delegate callback)
        {
            this.Callback = callback;
        }

        public delegate NPC Delegate(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry);
    }
}