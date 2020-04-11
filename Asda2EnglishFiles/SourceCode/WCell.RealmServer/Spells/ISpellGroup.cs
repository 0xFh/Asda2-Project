using System.Collections;
using System.Collections.Generic;

namespace WCell.RealmServer.Spells
{
    public interface ISpellGroup : IEnumerable<Spell>, IEnumerable
    {
    }
}