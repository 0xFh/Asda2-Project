using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.NPCs.Pets
{
    /// <summary>Record for Hunter pets with talents and everything</summary>
    [Castle.ActiveRecord.ActiveRecord("Pets_Permanent", Access = PropertyAccess.Property)]
    public class PermanentPetRecord : PetRecordBase<PermanentPetRecord>
    {
        [Field("PetNumber", NotNull = true)] private int m_PetNumber;

        public override uint PetNumber
        {
            get { return (uint) this.m_PetNumber; }
            set { this.m_PetNumber = (int) value; }
        }

        [Property] public int Experience { get; set; }

        [Property] public int Level { get; set; }

        [Property] public DateTime? StabledSince { get; set; }

        [Property] public DateTime? LastTalentResetTime { get; set; }

        [Property] public int TalentResetPriceTier { get; set; }

        [Property(NotNull = true)] public int FreeTalentPoints { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<PetTalentSpellRecord> Spells { get; set; }

        public override void SetupPet(NPC pet)
        {
            base.SetupPet(pet);
            pet.PetExperience = this.Experience;
            pet.Level = this.Level;
            pet.LastTalentResetTime = this.LastTalentResetTime;
            if (!pet.HasTalents)
                return;
            pet.FreeTalentPoints = this.FreeTalentPoints;
        }

        public override void UpdateRecord(NPC pet)
        {
            base.UpdateRecord(pet);
            this.Experience = pet.PetExperience;
            this.PetNumber = pet.PetNumber;
            this.Entry = pet.Entry;
            this.Level = pet.Level;
            this.LastTalentResetTime = pet.LastTalentResetTime;
            if (!pet.HasTalents)
                return;
            this.FreeTalentPoints = pet.FreeTalentPoints;
        }

        public static PermanentPetRecord[] LoadPermanentPetRecords(uint ownerId)
        {
            try
            {
                return ActiveRecordBase<PermanentPetRecord>.FindAllByProperty("_OwnerLowId", (object) (int) ownerId);
            }
            catch (Exception ex)
            {
                RealmDBMgr.OnDBError(ex);
                return ActiveRecordBase<PermanentPetRecord>.FindAllByProperty("_OwnerLowId", (object) (int) ownerId);
            }
        }
    }
}