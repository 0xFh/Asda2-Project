using Castle.ActiveRecord;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
    public class PetitionCharter : Item
    {
        private PetitionRecord m_Petition;

        protected override void OnLoad()
        {
            this.m_Petition = ActiveRecordBase<PetitionRecord>.Find((object) (int) this.Owner.EntityId.Low);
        }

        protected internal override void DoDestroy()
        {
            this.Petition.Delete();
            base.DoDestroy();
        }

        public PetitionRecord Petition
        {
            get { return this.m_Petition; }
            set { this.m_Petition = value; }
        }
    }
}