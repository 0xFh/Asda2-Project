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
            GOSummoningRitualEntry entry = (GOSummoningRitualEntry) this.m_go.Entry;
            if (user == null)
                return false;
            Character character = user;
            Unit owner = this.m_go.Owner;
            if (entry.CastersGrouped && !owner.IsAlliedWith((IFactionMember) character) ||
                this.Users.Contains(character))
                return false;
            this.Users.Add(character);
            if (this.Users.Count >= entry.CasterCount - 1)
            {
                this.TriggerSpell(owner, this.Target);
                for (int index = 0; index < this.Users.Count; ++index)
                    this.Users[index].m_currentRitual = (SummoningRitualHandler) null;
                this.Users.Clear();
            }

            return true;
        }

        public void TriggerSpell(Unit caster, Unit target)
        {
            caster.SpellCast.TriggerSingle(SpellHandler.Get(((GOSummoningRitualEntry) this.m_go.Entry).SpellId),
                (WorldObject) target);
        }

        public void Remove(Character chr)
        {
            this.Users.Remove(chr);
            chr.m_currentRitual = (SummoningRitualHandler) null;
        }
    }
}