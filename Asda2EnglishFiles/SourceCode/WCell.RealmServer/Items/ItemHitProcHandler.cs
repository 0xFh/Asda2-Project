using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Items
{
    /// <summary>
    /// Handles Item OnHit-procs which are applied to the wearer of the Item
    /// </summary>
    public class ItemHitProcHandler : IProcHandler, IDisposable
    {
        private Item m_Item;
        private readonly Spell m_Spell;

        public ItemHitProcHandler(Item item, Spell spell)
        {
            this.m_Item = item;
            this.m_Spell = spell;
        }

        public Unit Owner
        {
            get { return this.m_Item.Owner; }
        }

        /// <summary>ItemHitProcs always trigger</summary>
        public ProcTriggerFlags ProcTriggerFlags
        {
            get { return ProcTriggerFlags.DoneMeleeAutoAttack | ProcTriggerFlags.DoneRangedAutoAttack; }
        }

        public ProcHitFlags ProcHitFlags
        {
            get { return ProcHitFlags.Hit; }
        }

        /// <summary>
        /// Chance to Proc from 0 to 100
        /// Yet to implement: http://www.wowwiki.com/Procs_per_minute
        /// </summary>
        public uint ProcChance
        {
            get { return 100; }
        }

        public Spell ProcSpell
        {
            get { return this.m_Spell; }
        }

        /// <summary>ItemHitProcs dont have charges</summary>
        public int StackCount
        {
            get { return 0; }
            set { throw new NotImplementedException("Items do not have proc charges."); }
        }

        public int MinProcDelay
        {
            get { return 0; }
        }

        public DateTime NextProcTime { get; set; }

        public bool CanBeTriggeredBy(Unit triggerer, IUnitAction action, bool active)
        {
            return this.m_Spell.CanProcBeTriggeredBy(this.m_Item.Owner, action, active);
        }

        public void TriggerProc(Unit triggerer, IUnitAction action)
        {
            this.m_Item.Owner.SpellCast.ValidateAndTriggerNew(this.m_Spell, this.Owner, (WorldObject) triggerer, action,
                (SpellEffect) null);
        }

        public void Dispose()
        {
            this.m_Item = (Item) null;
        }
    }
}