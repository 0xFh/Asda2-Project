using Castle.ActiveRecord;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
  public class PetitionCharter : Item
  {
    private PetitionRecord m_Petition;

    protected override void OnLoad()
    {
      m_Petition = ActiveRecordBase<PetitionRecord>.Find((int) Owner.EntityId.Low);
    }

    protected internal override void DoDestroy()
    {
      Petition.Delete();
      base.DoDestroy();
    }

    public PetitionRecord Petition
    {
      get { return m_Petition; }
      set { m_Petition = value; }
    }
  }
}