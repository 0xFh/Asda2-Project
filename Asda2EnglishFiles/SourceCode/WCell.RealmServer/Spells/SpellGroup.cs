using System;
using System.Collections;
using System.Collections.Generic;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells
{
    [Serializable]
    public class SpellGroup : List<Spell>, ISpellGroup, IEnumerable<Spell>, IEnumerable
    {
        public void Add(ISpellGroup group)
        {
            foreach (Spell spell in (IEnumerable<Spell>) group)
                this.Add(spell);
        }

        public void Add(params SpellLineId[] ids)
        {
            foreach (SpellLineId id in ids)
                this.Add((ISpellGroup) id.GetLine());
        }

        public void Add(params SpellId[] ids)
        {
            foreach (SpellId id in ids)
                this.Add((ISpellGroup) SpellHandler.Get(id));
        }
    }
}