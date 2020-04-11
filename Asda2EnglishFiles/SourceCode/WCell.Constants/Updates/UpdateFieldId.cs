namespace WCell.Constants.Updates
{
    public struct UpdateFieldId
    {
        public readonly int RawId;

        private UpdateFieldId(int rawId)
        {
            this.RawId = rawId;
        }

        public static implicit operator int(UpdateFieldId field)
        {
            return field.RawId;
        }

        public static implicit operator UpdateFieldId(ObjectFields val)
        {
            return new UpdateFieldId((int) val);
        }

        public static implicit operator UpdateFieldId(UnitFields val)
        {
            return new UpdateFieldId((int) val);
        }

        public static implicit operator UpdateFieldId(PlayerFields val)
        {
            return new UpdateFieldId((int) val);
        }

        public static implicit operator UpdateFieldId(CorpseFields val)
        {
            return new UpdateFieldId((int) val);
        }

        public static implicit operator UpdateFieldId(ItemFields val)
        {
            return new UpdateFieldId((int) val);
        }

        public static implicit operator UpdateFieldId(ContainerFields val)
        {
            return new UpdateFieldId((int) val);
        }

        public static implicit operator UpdateFieldId(DynamicObjectFields val)
        {
            return new UpdateFieldId((int) val);
        }

        public static implicit operator UpdateFieldId(GameObjectFields val)
        {
            return new UpdateFieldId((int) val);
        }
    }
}