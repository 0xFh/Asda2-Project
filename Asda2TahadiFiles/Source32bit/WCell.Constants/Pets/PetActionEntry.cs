using System.Runtime.InteropServices;
using WCell.Constants.Spells;

namespace WCell.Constants.Pets
{
  [StructLayout(LayoutKind.Explicit, Size = 4)]
  public struct PetActionEntry
  {
    private const uint ActionMask = 16777215;
    private const uint TypeMask = 4278190080;
    [FieldOffset(0)]public uint Raw;
    [FieldOffset(0)]private uint m_ActionId;
    [FieldOffset(3)]public PetActionType Type;

    public PetActionEntry(uint raw)
    {
      Type = 0;
      m_ActionId = 0U;
      Raw = raw;
    }

    private int ActionId
    {
      get { return (int) m_ActionId & 16777215; }
    }

    public PetAction Action
    {
      get { return (PetAction) ActionId; }
      set { Raw = (uint) value; }
    }

    public SpellId SpellId
    {
      get { return (SpellId) ActionId; }
    }

    public void SetSpell(SpellId id, PetActionType type)
    {
      Raw = (uint) id;
      Type = type;
    }

    public PetAttackMode AttackMode
    {
      get { return (PetAttackMode) ActionId; }
      set { Raw = (uint) value; }
    }

    public bool IsAutoCastEnabled
    {
      get { return (Type & PetActionType.IsAutoCastEnabled) != 0; }
      set
      {
        if(value)
          Type |= PetActionType.IsAutoCastEnabled;
        else
          Type &= ~PetActionType.IsAutoCastEnabled;
      }
    }

    public bool IsAutoCastAllowed
    {
      get { return (Type & PetActionType.IsAutoCastAllowed) != 0; }
      set
      {
        if(value)
          Type |= PetActionType.IsAutoCastAllowed;
        else
          Type &= ~PetActionType.IsAutoCastAllowed;
      }
    }

    public override string ToString()
    {
      return ((int) Type) + ": " + m_ActionId;
    }

    public static implicit operator PetActionEntry(uint data)
    {
      return new PetActionEntry { Raw = data };
    }

    public static implicit operator uint(PetActionEntry entry)
    {
      return entry.Raw;
    }

    public static uint GetActionId(uint data)
    {
      return data & 16777215U;
    }

    public static PetActionType GetType(uint data)
    {
      return (PetActionType) ((data & 4278190080U) >> 24);
    }
  }
}