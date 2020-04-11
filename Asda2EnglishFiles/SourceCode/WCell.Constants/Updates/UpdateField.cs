namespace WCell.Constants.Updates
{
    public class UpdateField
    {
        public const int ObjectTypeCount = 9;
        public ObjectTypeId Group;
        public uint Offset;
        public uint Size;
        public UpdateFieldType Type;
        public UpdateFieldFlags Flags;
        public string Name;

        /// <summary>
        /// Indicates whether this UpdateField should be sent to everyone around (or only to the owner)
        /// </summary>
        public bool IsPublic;

        public string FullName
        {
            get { return ((int) this.Group).ToString() + "Fields." + this.Name; }
        }

        public string FullTypeName
        {
            get { return "UpdateFieldType." + (object) this.Type; }
        }

        public override string ToString()
        {
            return this.FullName + string.Format(" (Offset: {0}, Size: {1}, Type: {2}, Flags: {3})",
                       (object) this.Offset, (object) this.Size, (object) this.Type, (object) this.Flags);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UpdateField))
                return false;
            UpdateField updateField = (UpdateField) obj;
            return updateField.Group == this.Group && (int) updateField.Offset == (int) this.Offset;
        }

        public override int GetHashCode()
        {
            return (int) (this.Group | (ObjectTypeId) ((int) this.Offset << 3));
        }
    }
}