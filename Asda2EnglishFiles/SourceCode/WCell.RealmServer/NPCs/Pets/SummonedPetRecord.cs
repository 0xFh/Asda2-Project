using Castle.ActiveRecord;
using System;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.NPCs.Pets
{
    /// <summary>
    /// Summoned pets for which we only store ActionBar (and maybe name) settings
    /// </summary>
    [Castle.ActiveRecord.ActiveRecord("Pets_Summoned", Access = PropertyAccess.Property)]
    public class SummonedPetRecord : PetRecordBase<SummonedPetRecord>
    {
        [Field("PetNumber", NotNull = true)] private int m_PetNumber;

        public override uint PetNumber
        {
            get { return (uint) this.m_PetNumber; }
            set { this.m_PetNumber = (int) value; }
        }

        public static SummonedPetRecord[] LoadSummonedPetRecords(uint ownerId)
        {
            try
            {
                return ActiveRecordBase<SummonedPetRecord>.FindAllByProperty("_OwnerLowId", (object) (int) ownerId);
            }
            catch (Exception ex)
            {
                RealmDBMgr.OnDBError(ex);
                return ActiveRecordBase<SummonedPetRecord>.FindAllByProperty("_OwnerLowId", (object) (int) ownerId);
            }
        }
    }
}