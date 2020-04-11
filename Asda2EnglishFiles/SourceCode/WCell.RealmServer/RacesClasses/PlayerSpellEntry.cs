using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.RacesClasses
{
    public class PlayerSpellEntry : IDataHolder
    {
        public RaceId Race;
        public ClassId Class;
        public SpellId SpellId;

        public override string ToString()
        {
            return this.SpellId.ToString();
        }

        public void FinalizeDataHolder()
        {
            Spell spell = SpellHandler.Get(this.SpellId);
            if (spell == null)
            {
                ContentMgr.OnInvalidDBData(this.GetType().Name + " for \"{0} {1}\" refers to invalid Spell: {2}.",
                    (object) this.Race, (object) this.Class, (object) this);
            }
            else
            {
                List<Archetype> archetypes = ArchetypeMgr.GetArchetypes(this.Race, this.Class);
                if (archetypes == null)
                {
                    ContentMgr.OnInvalidDBData(this.GetType().Name + " \"{0}\" refers to invalid Archetype: {1} {2}.",
                        (object) this, (object) this.Race, (object) this.Class);
                }
                else
                {
                    foreach (Archetype archetype in archetypes)
                        archetype.Spells.Add(spell);
                }
            }
        }
    }
}