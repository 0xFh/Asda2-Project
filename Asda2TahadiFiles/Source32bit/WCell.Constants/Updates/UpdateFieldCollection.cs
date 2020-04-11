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
      TypeId = id;
      Fields = fields;
      FieldFlags = new UpdateFieldFlags[fields.Length];
      List<int> intList1 = new List<int>(25);
      List<int> intList2 = new List<int>(25);
      List<int> intList3 = new List<int>(25);
      for(int index = 0; index < Fields.Length; ++index)
      {
        UpdateField field = Fields[index];
        FieldFlags[index] = field.Flags;
        if((field.Flags & UpdateFieldFlags.Dynamic) != UpdateFieldFlags.None)
        {
          intList3.Add(index);
        }
        else
        {
          if((field.Flags & UpdateFieldFlags.OwnerOnly) != UpdateFieldFlags.None)
            intList1.Add(index);
          if((field.Flags & UpdateFieldFlags.GroupOnly) != UpdateFieldFlags.None)
            intList2.Add(index);
        }
      }

      OwnerIndices = intList1.ToArray();
      GroupIndices = intList2.ToArray();
      DynamicIndices = intList3.ToArray();
      BaseCollection = baseCollection;
      Offset = offset;
      HasPrivateFields = hasPrivateFields;
    }

    public int Length
    {
      get { return Fields.Length - Offset; }
    }

    public int TotalLength
    {
      get { return Fields.Length; }
    }
  }
}