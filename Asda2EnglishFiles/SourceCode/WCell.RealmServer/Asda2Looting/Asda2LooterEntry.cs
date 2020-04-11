using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Looting;

namespace WCell.RealmServer.Asda2Looting
{
    /// <summary>
    /// TODO: Keep track of roll-results etc
    /// Every Character has a LooterEntry which represents the interface between a Character and its current Loot
    /// </summary>
    public class Asda2LooterEntry
    {
        internal Character m_owner;
        private Asda2Loot m_loot;

        public Asda2LooterEntry(Character chr)
        {
            this.m_owner = chr;
        }

        /// <summary>The Looter</summary>
        public Character Owner
        {
            get { return this.m_owner; }
        }

        /// <summary>The Loot that the Character is currently looking at</summary>
        public Asda2Loot Loot
        {
            get { return this.m_loot; }
            internal set
            {
                if (this.m_owner == null)
                {
                    this.m_loot = (Asda2Loot) null;
                }
                else
                {
                    if (this.m_loot == value)
                        return;
                    Asda2Loot loot = this.m_loot;
                    this.m_loot = value;
                    if (value == null)
                    {
                        this.m_owner.UnitFlags &= UnitFlags.CanPerformAction_Mask1 | UnitFlags.Flag_0_0x1 |
                                                  UnitFlags.SelectableNotAttackable | UnitFlags.Influenced |
                                                  UnitFlags.PlayerControlled | UnitFlags.Flag_0x10 |
                                                  UnitFlags.Preparation | UnitFlags.PlusMob |
                                                  UnitFlags.SelectableNotAttackable_2 | UnitFlags.NotAttackable |
                                                  UnitFlags.Passive | UnitFlags.PetInCombat | UnitFlags.Flag_12_0x1000 |
                                                  UnitFlags.Silenced | UnitFlags.Flag_14_0x4000 |
                                                  UnitFlags.Flag_15_0x8000 | UnitFlags.SelectableNotAttackable_3 |
                                                  UnitFlags.Combat | UnitFlags.TaxiFlight | UnitFlags.Disarmed |
                                                  UnitFlags.Confused | UnitFlags.Feared | UnitFlags.Possessed |
                                                  UnitFlags.NotSelectable | UnitFlags.Skinnable | UnitFlags.Mounted |
                                                  UnitFlags.Flag_28_0x10000000 | UnitFlags.Flag_29_0x20000000 |
                                                  UnitFlags.Flag_30_0x40000000 | UnitFlags.Flag_31_0x80000000;
                        if (!loot.MustKneelWhileLooting)
                            return;
                        this.m_owner.StandState = StandState.Stand;
                    }
                    else
                    {
                        this.m_owner.UnitFlags |= UnitFlags.Looting;
                        if (!value.MustKneelWhileLooting)
                            return;
                        this.m_owner.StandState = StandState.Kneeling;
                    }
                }
            }
        }

        /// <summary>Requires loot to already be generated</summary>
        /// <param name="lootable"></param>
        public void TryLoot(IAsda2Lootable lootable)
        {
            this.Release();
            Asda2Loot loot = lootable.Loot;
            if (loot == null || !this.MayLoot(loot))
                return;
            this.m_owner.CancelAllActions();
            this.Loot = loot;
        }

        /// <summary>
        /// Returns whether this Looter is entitled to loot anything from the given loot
        /// </summary>
        public bool MayLoot(Asda2Loot loot)
        {
            return this.m_owner != null && (loot.Looters.Count == 0 || loot.Looters.Contains(this) ||
                                            this.m_owner.GodMode || loot.Group != null &&
                                            this.m_owner.Group == loot.Group &&
                                            (loot.FreelyAvailableCount > 0 ||
                                             this.m_owner.GroupMember == loot.Group.MasterLooter));
        }

        /// <summary>
        /// Releases the current loot and (maybe) makes it available to everyone else.
        /// </summary>
        public void Release()
        {
            if (this.m_loot == null)
                return;
            Character owner = this.m_owner;
            this.m_loot.RemoveLooter(this);
            if (this.m_loot.Looters.Count == 0)
                this.m_loot.IsReleased = true;
            this.Loot = (Asda2Loot) null;
        }
    }
}