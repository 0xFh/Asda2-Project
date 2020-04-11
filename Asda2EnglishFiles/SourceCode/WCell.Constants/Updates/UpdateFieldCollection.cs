using System.Collections.Generic;

namespace WCell.Constants.Updates
{
    public class UpdateFieldCollection
    {
        public readonly UpdateField[] Fields;
        public readonly UpdateFieldFlags[] FieldFlags;
        public readonly int[] OwnerIndices;
        public readonly int[] GroupIndices;
        public readonly int[] DynamicIndices;
        public readonly ObjectTypeId TypeId;
        public readonly UpdateFieldCollection BaseCollection;
        public readonly int Offset;
        public readonly bool HasPrivateFields;

        internal UpdateFieldCollection(ObjectTypeId id, UpdateField[] fields, UpdateFieldCollection baseCollection,
            int offset, bool hasPrivateFields)
        {
            this.TypeId = id;
            this.Fields = fields;
            this.FieldFlags = new UpdateFieldFlags[fields.Length];
            List<int> intList1 = new List<int>(25);
            List<int> intList2 = new List<int>(25);
            List<int> intList3 = new List<int>(25);
            for (int index = 0; index < this.Fields.Length; ++index)
            {
                UpdateField field = this.Fields[index];
                this.FieldFlags[index] = field.Flags;
                if ((field.Flags & UpdateFieldFlags.Dynamic) != UpdateFieldFlags.None)
                {
                    intList3.Add(index);
                }
                else
                {
                    if ((field.Flags & UpdateFieldFlags.OwnerOnly) != UpdateFieldFlags.None)
                        intList1.Add(index);
                    if ((field.Flags & UpdateFieldFlags.GroupOnly) != UpdateFieldFlags.None)
                        intList2.Add(index);
                }
            }

            this.OwnerIndices = intList1.ToArray();
            this.GroupIndices = intList2.ToArray();
            this.DynamicIndices = intList3.ToArray();
            this.BaseCollection = baseCollection;
            this.Offset = offset;
            this.HasPrivateFields = hasPrivateFields;
        }

        public int Length
        {
            get { return this.Fields.Length - this.Offset; }
        }

        public int TotalLength
        {
            get { return this.Fields.Length; }
        }
    }
}