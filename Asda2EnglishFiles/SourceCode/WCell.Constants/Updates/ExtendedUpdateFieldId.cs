namespace WCell.Constants.Updates
{
    public struct ExtendedUpdateFieldId
    {
        public ObjectTypeId ObjectType;
        public int RawId;

        public ExtendedUpdateFieldId(int rawId, ObjectTypeId objectType)
        {
            this.RawId = rawId;
            this.ObjectType = objectType;
        }

        public ExtendedUpdateFieldId(ObjectFields val)
        {
            this.RawId = (int) val;
            this.ObjectType = ObjectTypeId.Object;
        }

        public ExtendedUpdateFieldId(UnitFields val)
        {
            this.RawId = (int) val;
            this.ObjectType = ObjectTypeId.Unit;
        }

        public ExtendedUpdateFieldId(PlayerFields val)
        {
            this.RawId = (int) val;
            this.ObjectType = ObjectTypeId.Player;
        }

        public ExtendedUpdateFieldId(CorpseFields val)
        {
            this.RawId = (int) val;
            this.ObjectType = ObjectTypeId.Corpse;
        }

        public ExtendedUpdateFieldId(ItemFields val)
        {
            this.RawId = (int) val;
            this.ObjectType = ObjectTypeId.Item;
        }

        public ExtendedUpdateFieldId(ContainerFields val)
        {
            this.RawId = (int) val;
            this.ObjectType = ObjectTypeId.Container;
        }

        public ExtendedUpdateFieldId(DynamicObjectFields val)
        {
            this.RawId = (int) val;
            this.ObjectType = ObjectTypeId.DynamicObject;
        }

        public ExtendedUpdateFieldId(GameObjectFields val)
        {
            this.RawId = (int) val;
            this.ObjectType = ObjectTypeId.GameObject;
        }

        public static implicit operator int(ExtendedUpdateFieldId field)
        {
            return field.RawId;
        }

        public static implicit operator ExtendedUpdateFieldId(ObjectFields val)
        {
            return new ExtendedUpdateFieldId((int) val, ObjectTypeId.Object);
        }

        public static implicit operator ExtendedUpdateFieldId(UnitFields val)
        {
            return new ExtendedUpdateFieldId((int) val, ObjectTypeId.Unit);
        }

        public static implicit operator ExtendedUpdateFieldId(PlayerFields val)
        {
            return new ExtendedUpdateFieldId((int) val, ObjectTypeId.Player);
        }

        public static implicit operator ExtendedUpdateFieldId(CorpseFields val)
        {
            return new ExtendedUpdateFieldId((int) val, ObjectTypeId.Corpse);
        }

        public static implicit operator ExtendedUpdateFieldId(ItemFields val)
        {
            return new ExtendedUpdateFieldId((int) val, ObjectTypeId.Item);
        }

        public static implicit operator ExtendedUpdateFieldId(ContainerFields val)
        {
            return new ExtendedUpdateFieldId((int) val, ObjectTypeId.Container);
        }

        public static implicit operator ExtendedUpdateFieldId(DynamicObjectFields val)
        {
            return new ExtendedUpdateFieldId((int) val, ObjectTypeId.DynamicObject);
        }

        public static implicit operator ExtendedUpdateFieldId(GameObjectFields val)
        {
            return new ExtendedUpdateFieldId((int) val, ObjectTypeId.GameObject);
        }
    }
}