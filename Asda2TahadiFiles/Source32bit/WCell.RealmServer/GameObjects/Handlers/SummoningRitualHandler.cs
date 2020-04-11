using NLog;
using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.GameObjects.Handlers
{
  /// <summary>GO Type 18</summary>
  public class SummoningRitualHandler : GameObjectHandler
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    public readonly List<Character> Users = new List<Character>(5);
    internal Unit Target;

    public override bool Use(Character user)
    {
      GOSummoningRitualEntry entry = (GOSummoningRitualEntry) m_go.Entry;
      if(user == null)
        return false;
      Character character = user;
      Unit owner = m_go.Owner;
      if(entry.CastersGrouped && !owner.IsAlliedWith(character) ||
         Users.Contains(character))
        return false;
      Users.Add(character);
      if(Users.Count >= entry.CasterCount - 1)
      {
        TriggerSpell(owner, Target);
        for(int index = 0; index < Users.Count; ++index)
          Users[index].m_currentRitual = null;
        Users.Clear();
      }

      return true;
    }

    public void TriggerSpell(Unit caster, Unit target)
    {
      caster.SpellCast.TriggerSingle(SpellHandler.Get(((GOSummoningRitualEntry) m_go.Entry).SpellId),
        target);
    }

    public void Remove(Character chr)
    {
      Users.Remove(chr);
      chr.m_currentRitual = null;
    }
  }
}