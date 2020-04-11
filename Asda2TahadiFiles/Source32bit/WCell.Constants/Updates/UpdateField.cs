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
      get { return ((int) Group) + "Fields." + Name; }
    }

    public string FullTypeName
    {
      get { return "UpdateFieldType." + Type; }
    }

    public override string ToString()
    {
      return FullName + string.Format(" (Offset: {0}, Size: {1}, Type: {2}, Flags: {3})",
               (object) Offset, (object) Size, (object) Type, (object) Flags);
    }

    public override bool Equals(object obj)
    {
      if(!(obj is UpdateField))
        return false;
      UpdateField updateField = (UpdateField) obj;
      return updateField.Group == Group && (int) updateField.Offset == (int) Offset;
    }

    public override int GetHashCode()
    {
      return (int) (Group | (ObjectTypeId) ((int) Offset << 3));
    }
  }
}