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
      return SpellId.ToString();
    }

    public void FinalizeDataHolder()
    {
      Spell spell = SpellHandler.Get(SpellId);
      if(spell == null)
      {
        ContentMgr.OnInvalidDBData(GetType().Name + " for \"{0} {1}\" refers to invalid Spell: {2}.",
          (object) Race, (object) Class, (object) this);
      }
      else
      {
        List<Archetype> archetypes = ArchetypeMgr.GetArchetypes(Race, Class);
        if(archetypes == null)
        {
          ContentMgr.OnInvalidDBData(GetType().Name + " \"{0}\" refers to invalid Archetype: {1} {2}.",
            (object) this, (object) Race, (object) Class);
        }
        else
        {
          foreach(Archetype archetype in archetypes)
            archetype.Spells.Add(spell);
        }
      }
    }
  }
}