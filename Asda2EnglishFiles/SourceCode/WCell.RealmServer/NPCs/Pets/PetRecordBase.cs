using Castle.ActiveRecord;
using System;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.NPCs.Pets
{
    public abstract class PetRecordBase<R> : ActiveRecordBase<R>, IPetRecord where R : IPetRecord
    {
        [Field("OwnerLowId", NotNull = true)] protected int _OwnerLowId;
        [Field("NameTimeStamp")] protected int _NameTimeStamp;
        [Field("PetState", NotNull = true)] protected int _PetState;

        [Field("PetAttackMode", NotNull = true)]
        protected int _petAttackMode;

        [Field("PetFlags", NotNull = true)] protected int _petFlags;
        private uint[] m_ActionButtons;

        [PrimaryKey(PrimaryKeyType.Assigned, "EntryId")]
        private int _EntryId { get; set; }

        public uint OwnerId
        {
            get { return (uint) this._OwnerLowId; }
            set { this._OwnerLowId = (int) value; }
        }

        public NPCId EntryId
        {
            get { return (NPCId) this._EntryId; }
            set { this._EntryId = (int) value; }
        }

        public virtual uint PetNumber
        {
            get { return 0; }
            set { throw new InvalidOperationException("Cannot set PetNumber"); }
        }

        public NPCEntry Entry
        {
            get { return NPCMgr.GetEntry(this.EntryId); }
            set { this.EntryId = value.NPCId; }
        }

        [Property] public bool IsActivePet { get; set; }

        [Property] public string Name { get; set; }

        public uint NameTimeStamp
        {
            get { return (uint) this._NameTimeStamp; }
            set { this._NameTimeStamp = (int) value; }
        }

        public PetState PetState
        {
            get { return (PetState) this._PetState; }
            set { this._PetState = (int) value; }
        }

        public PetAttackMode AttackMode
        {
            get { return (PetAttackMode) this._petAttackMode; }
            set { this._petAttackMode = (int) value; }
        }

        public PetFlags Flags
        {
            get { return (PetFlags) this._petFlags; }
            set { this._petFlags = (int) value; }
        }

        public bool IsStabled
        {
            get { return this.Flags.HasFlag((Enum) PetFlags.Stabled); }
            set
            {
                if (value)
                {
                    this.Flags |= PetFlags.Stabled;
                    this.IsActivePet = false;
                }
                else
                {
                    this.Flags &= ~PetFlags.Stabled;
                    this.IsActivePet = true;
                }
            }
        }

        /// <summary>Dirty records have uncommitted changes</summary>
        public bool IsDirty { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        [Property(NotNull = true)]
        public uint[] ActionButtons
        {
            get { return this.m_ActionButtons; }
            set { this.m_ActionButtons = value; }
        }

        public override void Create()
        {
            base.Create();
            this.IsDirty = false;
        }

        public override void Update()
        {
            base.Update();
            this.IsDirty = false;
        }

        public virtual void SetupPet(NPC pet)
        {
            if (!string.IsNullOrEmpty(this.Name))
                pet.SetName(this.Name, this.NameTimeStamp);
            pet.PetState = this.PetState;
        }

        public virtual void UpdateRecord(NPC pet)
        {
            if (pet.PetNameTimestamp != 0U)
            {
                this.Name = pet.Name;
                this.NameTimeStamp = pet.PetNameTimestamp;
            }

            this.PetState = pet.PetState;
            this.EntryId = (NPCId) pet.EntryId;
            this.IsDirty = true;
        }
    }
}