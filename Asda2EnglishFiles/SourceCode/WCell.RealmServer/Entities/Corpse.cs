using NLog;
using System;
using System.IO;
using System.Threading;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Items;
using WCell.RealmServer.UpdateFields;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Entities
{
    public class Corpse : WorldObject
    {
        [Variable("CorpseMinReclaimDelayMillis")]
        public static int MinReclaimDelay = 30000;

        [Variable("CorpseAutoReleaseDelayMillis")]
        public static int AutoReleaseDelay = 360000;

        [Variable("BonesDecayTimeMillis")] public static int DecayTimeMillis = 60000;
        public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Corpse);
        internal static readonly CompoundType[] EmptyItemFields = new CompoundType[19];
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static float s_reclaimRadius;
        internal static float ReclaimRadiusSq;
        private static float s_corpseVisRange;
        internal static float GhostVisibilityRadiusSq;
        private static int lastUID;
        private Character m_owner;

        public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
        {
            get { return UpdateFieldHandler.DynamicCorpseHandlers; }
        }

        /// <summary>
        /// The radius (in yards) in which a Character has to be in order to reclaim his/her corpse (Default: 40)
        /// </summary>
        public static float MinReclaimRadius
        {
            get { return Corpse.s_reclaimRadius; }
            set
            {
                Corpse.s_reclaimRadius = value;
                Corpse.ReclaimRadiusSq = Corpse.s_reclaimRadius * Corpse.s_reclaimRadius;
            }
        }

        /// <summary>
        /// The radius (in yards) around a corpse in which the dead owner can see
        /// living Units
        /// </summary>
        public static float GhostVisibilityRadius
        {
            get { return Corpse.s_corpseVisRange; }
            set
            {
                Corpse.s_corpseVisRange = value;
                Corpse.GhostVisibilityRadiusSq = Corpse.s_corpseVisRange * Corpse.s_corpseVisRange;
            }
        }

        static Corpse()
        {
            Corpse.MinReclaimRadius = 40f;
            Corpse.GhostVisibilityRadius = 60f;
        }

        protected override UpdateFieldCollection _UpdateFieldInfos
        {
            get { return Corpse.UpdateFieldInfos; }
        }

        /// <summary>Creates a new Corpse for the given owner</summary>
        /// <param name="owner">The owner of this Corpse</param>
        /// <param name="pos">The position where the Corpse should appear</param>
        /// <param name="orientation">Orientation of the corpse</param>
        /// <param name="displayId">The displayid of the corpse</param>
        /// <param name="face">Face value</param>
        /// <param name="skin">Skin value</param>
        /// <param name="hairStyle">Hairstyle</param>
        /// <param name="hairColor">Haircolor</param>
        /// <param name="facialHair">Facial hair (beard)</param>
        /// <param name="guildId">The guild to which the owner of the corpse belongs</param>
        /// <param name="gender">Gender of the owner</param>
        /// <param name="race">Race of the owner</param>
        /// <param name="flags">Flags (only skeleton or full corpse)</param>
        /// <param name="dynFlags">Dynamic flags (is it lootable?)</param>
        public Corpse(Character owner, Vector3 pos, float orientation, uint displayId, byte face, byte skin,
            byte hairStyle, byte hairColor, byte facialHair, uint guildId, GenderType gender, RaceId race,
            CorpseFlags flags, CorpseDynamicFlags dynFlags)
        {
            this.EntityId = EntityId.GetCorpseId((uint) Interlocked.Increment(ref Corpse.lastUID));
            this.DisplayId = displayId;
            this.Owner = owner;
            this.Type |= ObjectTypes.Corpse;
            this.ScaleX = 1f;
            this.m_position = pos;
            this.m_orientation = orientation;
            this.Face = face;
            this.Skin = skin;
            this.HairStyle = hairStyle;
            this.HairColor = hairColor;
            this.FacialHair = facialHair;
            this.GuildId = guildId;
            this.Gender = gender;
            this.Race = race;
            this.Flags = flags;
            this.DynamicFlags = dynFlags;
        }

        public override string Name
        {
            get
            {
                if (this.m_owner == null)
                    return "Unknown Corpse";
                return "Corpse of " + (object) this.m_owner;
            }
            set { }
        }

        public override ObjectTypeId ObjectTypeId
        {
            get { return ObjectTypeId.Corpse; }
        }

        public override UpdateFlags UpdateFlags
        {
            get
            {
                return UpdateFlags.Flag_0x10 | UpdateFlags.StationaryObject | UpdateFlags.StationaryObjectOnTransport;
            }
        }

        public override Faction Faction
        {
            get { return this.m_owner.Faction; }
            set { throw new Exception("Corpse' faction can't be changed"); }
        }

        public override FactionId FactionId
        {
            get { return this.m_owner.Faction.Id; }
            set { throw new Exception("Corpse' faction can't be changed"); }
        }

        public override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.m_owner = (Character) null;
        }

        protected override void WriteMovementUpdate(PrimitiveWriter packet, UpdateFieldFlags relation)
        {
            if (this.UpdateFlags.HasAnyFlag(UpdateFlags.StationaryObjectOnTransport))
            {
                EntityId.Zero.WritePacked((BinaryWriter) packet);
                packet.Write(this.Position);
                packet.Write(this.Position);
                packet.Write(this.Orientation);
                packet.Write(this.Orientation);
            }
            else
            {
                if (!this.UpdateFlags.HasAnyFlag(UpdateFlags.StationaryObject))
                    return;
                packet.Write(this.Position);
                packet.WriteFloat(this.Orientation);
            }
        }

        protected internal override void OnEnterMap()
        {
        }

        protected internal override void OnLeavingMap()
        {
            base.OnLeavingMap();
        }

        public override void OnFinishedLooting()
        {
            this.StartDecay();
        }

        /// <summary>Set the Item at the given slot on this corpse.</summary>
        public void SetItem(EquipmentSlot slot, ItemTemplate template)
        {
            uint num = template.DisplayId | (uint) template.InventorySlotType << 24;
            this.SetUInt32((int) (11 + slot), num);
        }

        /// <summary>Removes the Items from this Corpse</summary>
        public void RemoveItems()
        {
            Array.Copy((Array) Corpse.EmptyItemFields, 0, (Array) this.m_updateValues, 11,
                Corpse.EmptyItemFields.Length);
            if (this.m_requiresUpdate || !this.IsInWorld)
                return;
            this.RequestUpdate();
        }

        /// <summary>Starts the decay-timer</summary>
        public void StartDecay()
        {
            if (this.IsInWorld && this.Flags != CorpseFlags.Bones)
            {
                this.RemoveItems();
                this.DynamicFlags = CorpseDynamicFlags.None;
                this.m_Map.CallDelayed(Corpse.DecayTimeMillis, new Action(((WorldObject) this).Delete));
            }
            else
                this.Delete();
        }

        public Character Owner
        {
            get { return this.m_owner; }
            set
            {
                this.m_owner = value;
                if (this.m_owner != null)
                    this.SetEntityId((UpdateFieldId) CorpseFields.OWNER, value.EntityId);
                else
                    this.SetEntityId((UpdateFieldId) CorpseFields.OWNER, EntityId.Zero);
            }
        }

        public uint DisplayId
        {
            get { return this.GetUInt32(CorpseFields.DISPLAY_ID); }
            set { this.SetUInt32((UpdateFieldId) CorpseFields.DISPLAY_ID, value); }
        }

        /// <summary>
        /// Array of 19 uints
        /// TODO: Set the equipment of the player
        /// </summary>
        public uint ItemBase
        {
            get { return this.GetUInt32(CorpseFields.ITEM); }
            set { this.SetUInt32((UpdateFieldId) CorpseFields.ITEM, value); }
        }

        public byte[] Bytes1
        {
            get { return this.GetByteArray((UpdateFieldId) CorpseFields.BYTES_1); }
            set { this.SetByteArray((UpdateFieldId) CorpseFields.BYTES_1, value); }
        }

        public byte Bytes1_0
        {
            get { return this.GetByte((UpdateFieldId) CorpseFields.BYTES_1, 0); }
            set { this.SetByte((UpdateFieldId) CorpseFields.BYTES_1, 0, value); }
        }

        public RaceId Race
        {
            get { return (RaceId) this.GetByte((UpdateFieldId) CorpseFields.BYTES_1, 1); }
            set { this.SetByte((UpdateFieldId) CorpseFields.BYTES_1, 1, (byte) value); }
        }

        public GenderType Gender
        {
            get { return (GenderType) this.GetByte((UpdateFieldId) CorpseFields.BYTES_1, 2); }
            set { this.SetByte((UpdateFieldId) CorpseFields.BYTES_1, 2, (byte) value); }
        }

        public byte Skin
        {
            get { return this.GetByte((UpdateFieldId) CorpseFields.BYTES_1, 3); }
            set { this.SetByte((UpdateFieldId) CorpseFields.BYTES_1, 3, value); }
        }

        public byte[] Bytes2
        {
            get { return this.GetByteArray((UpdateFieldId) CorpseFields.BYTES_2); }
            set { this.SetByteArray((UpdateFieldId) CorpseFields.BYTES_2, value); }
        }

        public byte Face
        {
            get { return this.GetByte((UpdateFieldId) CorpseFields.BYTES_2, 0); }
            set { this.SetByte((UpdateFieldId) CorpseFields.BYTES_2, 0, value); }
        }

        public byte HairStyle
        {
            get { return this.GetByte((UpdateFieldId) CorpseFields.BYTES_2, 1); }
            set { this.SetByte((UpdateFieldId) CorpseFields.BYTES_2, 1, value); }
        }

        public byte HairColor
        {
            get { return this.GetByte((UpdateFieldId) CorpseFields.BYTES_2, 2); }
            set { this.SetByte((UpdateFieldId) CorpseFields.BYTES_2, 2, value); }
        }

        public byte FacialHair
        {
            get { return this.GetByte((UpdateFieldId) CorpseFields.BYTES_2, 3); }
            set { this.SetByte((UpdateFieldId) CorpseFields.BYTES_2, 3, value); }
        }

        public uint GuildId
        {
            get { return this.GetUInt32(CorpseFields.GUILD); }
            set { this.SetUInt32((UpdateFieldId) CorpseFields.GUILD, value); }
        }

        public CorpseFlags Flags
        {
            get { return (CorpseFlags) this.GetUInt32(CorpseFields.FLAGS); }
            set { this.SetUInt32((UpdateFieldId) CorpseFields.FLAGS, (uint) value); }
        }

        public CorpseDynamicFlags DynamicFlags
        {
            get { return (CorpseDynamicFlags) this.GetUInt32(CorpseFields.DYNAMIC_FLAGS); }
            set { this.SetUInt32((UpdateFieldId) CorpseFields.DYNAMIC_FLAGS, (uint) value); }
        }

        public override ObjectTypeCustom CustomType
        {
            get { return ObjectTypeCustom.Object | ObjectTypeCustom.Corpse; }
        }
    }
}