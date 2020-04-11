using WCell.Constants.Updates;
using WCell.RealmServer.Items;
using WCell.RealmServer.UpdateFields;

namespace WCell.RealmServer.Entities
{
    /// <summary>An equippable container item, such as a bag etc</summary>
    public class Container : Item, IContainer, IEntity
    {
        public new static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Container);
        private ContainerInventory m_inventory;

        protected override UpdateFieldCollection _UpdateFieldInfos
        {
            get { return UpdateFieldInfos; }
        }

        public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
        {
            get { return UpdateFieldHandler.DynamicContainerFieldHandlers; }
        }

        protected internal Container()
        {
        }

        protected override void OnInit()
        {
            this.Type |= ObjectTypes.Container;
            this.ContainerSlots = this.m_template.ContainerSlots;
            this.m_inventory = new ContainerInventory(this, (UpdateFieldId) ContainerFields.SLOT_1,
                this.m_template.ContainerSlots);
        }

        protected override void OnLoad()
        {
            this.Type |= ObjectTypes.Container;
            this.SetInt32((UpdateFieldId) ContainerFields.NUM_SLOTS, this.m_record.ContSlots);
            this.m_inventory =
                new ContainerInventory(this, (UpdateFieldId) ContainerFields.SLOT_1, this.m_record.ContSlots);
        }

        public override ObjectTypeId ObjectTypeId
        {
            get { return ObjectTypeId.Container; }
        }

        /// <summary>NUM_SLOTS</summary>
        public int ContainerSlots
        {
            get { return this.GetInt32(ContainerFields.NUM_SLOTS); }
            set
            {
                this.SetInt32((UpdateFieldId) ContainerFields.NUM_SLOTS, value);
                this.m_record.ContSlots = value;
            }
        }

        public BaseInventory BaseInventory
        {
            get { return (BaseInventory) this.m_inventory; }
        }

        public override ObjectTypeCustom CustomType
        {
            get { return ObjectTypeCustom.Container | ObjectTypeCustom.Object; }
        }

        protected internal override void DoDestroy()
        {
            foreach (Item obj in (BaseInventory) this.m_inventory)
                obj.Destroy();
            base.DoDestroy();
        }
    }
}