using Castle.ActiveRecord;
using WCell.Core;

namespace WCell.RealmServer.Database
{
  [ActiveRecord("EquipmentSetItemMappings", Access = PropertyAccess.Property)]
  public class EquipmentSetItemMapping : ActiveRecordBase<EquipmentSetItemMapping>
  {
    [Field("Item_LowId")]private int itemLowId;

    [PrimaryKey(PrimaryKeyType.Counter)]
    private long Id { get; set; }

    /// <summary>
    /// Cannot be named "Set" because
    /// NHibernate doesn't quote table names right now.
    /// </summary>
    [BelongsTo]
    public EquipmentSet ParentSet { get; set; }

    public EntityId ItemEntityId
    {
      get { return EntityId.GetItemId((uint) itemLowId); }
      set { itemLowId = (int) value.Low; }
    }
  }
}