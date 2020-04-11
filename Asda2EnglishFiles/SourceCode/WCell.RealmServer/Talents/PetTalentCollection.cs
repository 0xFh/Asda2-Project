using System;
using WCell.Constants.Pets;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs.Pets;

namespace WCell.RealmServer.Talents
{
    /// <summary>
    /// A TalentCollection for pets, which *must* have:
    /// 1. A Master of type <see cref="T:WCell.RealmServer.Entities.Character" />
    /// 2. A Record of type <see cref="T:WCell.RealmServer.NPCs.Pets.PermanentPetRecord" />
    /// </summary>
    public class PetTalentCollection : TalentCollection
    {
        /// <summary>Every 2h, the reset tier drops by one</summary>
        public static int PetResetTierDecayHours = 2;

        public PetTalentCollection(NPC owner)
            : base((Unit) owner)
        {
        }

        public PermanentPetRecord Record
        {
            get { return ((NPC) this.Owner).PermanentPetRecord; }
        }

        /// <summary>
        /// Need to make sure that this collection is not used if pet is not owned by character
        /// </summary>
        public override Character OwnerCharacter
        {
            get { return (Character) this.Owner.Master; }
        }

        public override bool CanLearn(TalentEntry entry, int rank)
        {
            if (entry.Tree.Id == ((NPC) this.Owner).PetTalentType.GetTalentTreeId())
                return base.CanLearn(entry, rank);
            return false;
        }

        public override int FreeTalentPoints
        {
            get { return this.Record.FreeTalentPoints; }
            set { this.Record.FreeTalentPoints = value; }
        }

        public override int CurrentSpecIndex
        {
            get { return 0; }
        }

        public override uint[] ResetPricesPerTier
        {
            get { return TalentMgr.PetTalentResetPricesPerTier; }
        }

        protected override int CurrentResetTier
        {
            get { return this.Record.TalentResetPriceTier; }
            set { this.Record.TalentResetPriceTier = value; }
        }

        public override DateTime? LastResetTime
        {
            get { return this.Record.LastTalentResetTime; }
            set { this.Record.LastTalentResetTime = value; }
        }

        /// <summary>Every 2 hours count down one tier</summary>
        public override int ResetTierDecayHours
        {
            get { return PetTalentCollection.PetResetTierDecayHours; }
        }

        public override int GetFreeTalentPointsForLevel(int level)
        {
            if (level < 20)
                return -this.TotalPointsSpent;
            return (level - 19) / 4 - this.TotalPointsSpent;
        }

        public override void UpdateFreeTalentPointsSilently(int delta)
        {
            throw new NotImplementedException();
        }
    }
}