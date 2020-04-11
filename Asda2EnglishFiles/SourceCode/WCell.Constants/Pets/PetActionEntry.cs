using System.Runtime.InteropServices;
using WCell.Constants.Spells;

namespace WCell.Constants.Pets
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct PetActionEntry
    {
        private const uint ActionMask = 16777215;
        private const uint TypeMask = 4278190080;
        [FieldOffset(0)] public uint Raw;
        [FieldOffset(0)] private uint m_ActionId;
        [FieldOffset(3)] public PetActionType Type;

        public PetActionEntry(uint raw)
        {
            this.Type = (PetActionType) 0;
            this.m_ActionId = 0U;
            this.Raw = raw;
        }

        private int ActionId
        {
            get { return (int) this.m_ActionId & 16777215; }
        }

        public PetAction Action
        {
            get { return (PetAction) this.ActionId; }
            set { this.Raw = (uint) value; }
        }

        public SpellId SpellId
        {
            get { return (SpellId) this.ActionId; }
        }

        public void SetSpell(SpellId id, PetActionType type)
        {
            this.Raw = (uint) id;
            this.Type = type;
        }

        public PetAttackMode AttackMode
        {
            get { return (PetAttackMode) this.ActionId; }
            set { this.Raw = (uint) value; }
        }

        public bool IsAutoCastEnabled
        {
            get { return (this.Type & PetActionType.IsAutoCastEnabled) != (PetActionType) 0; }
            set
            {
                if (value)
                    this.Type |= PetActionType.IsAutoCastEnabled;
                else
                    this.Type &= ~PetActionType.IsAutoCastEnabled;
            }
        }

        public bool IsAutoCastAllowed
        {
            get { return (this.Type & PetActionType.IsAutoCastAllowed) != (PetActionType) 0; }
            set
            {
                if (value)
                    this.Type |= PetActionType.IsAutoCastAllowed;
                else
                    this.Type &= ~PetActionType.IsAutoCastAllowed;
            }
        }

        public override string ToString()
        {
            return ((int) this.Type).ToString() + ": " + (object) this.m_ActionId;
        }

        public static implicit operator PetActionEntry(uint data)
        {
            return new PetActionEntry() {Raw = data};
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