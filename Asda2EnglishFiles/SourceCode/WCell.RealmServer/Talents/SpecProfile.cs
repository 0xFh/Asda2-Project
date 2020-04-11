using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using System;
using System.Collections.Generic;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Talents
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class SpecProfile : WCellRecord<SpecProfile>
    {
        private static readonly Order[] order = new Order[1]
        {
            Order.Asc(nameof(SpecIndex))
        };

        public static int MAX_TALENT_GROUPS = 2;
        public static int MAX_GLYPHS_PER_GROUP = 6;

        [Field("CharacterId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _characterGuid;

        internal static SpecProfile[] LoadAllOfCharacter(Character chr)
        {
            ICriterion[] criterionArray = new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("_characterGuid", (object) (int) chr.EntityId.Low)
            };
            SpecProfile[] all = ActiveRecordBase<SpecProfile>.FindAll(SpecProfile.order, criterionArray);
            int num = 0;
            foreach (SpecProfile specProfile in all)
            {
                if (specProfile.SpecIndex != num)
                {
                    LogManager.GetCurrentClassLogger()
                        .Warn("Found SpecProfile for \"{0}\" with invalid SpecIndex {1} (should be {2})",
                            (object) specProfile.SpecIndex, (object) num);
                    specProfile.SpecIndex = num;
                    specProfile.State = RecordState.Dirty;
                }

                if (specProfile.ActionButtons == null)
                    specProfile.ActionButtons = (byte[]) chr.Archetype.ActionButtons.Clone();
                else if (specProfile.ActionButtons.Length != chr.Archetype.ActionButtons.Length)
                {
                    byte[] actionButtons = specProfile.ActionButtons;
                    Array.Resize<byte>(ref actionButtons, chr.Archetype.ActionButtons.Length);
                    specProfile.ActionButtons = actionButtons;
                }

                ++num;
            }

            return all;
        }

        /// <summary>Creates a new SpecProfile and saves it to the db.</summary>
        /// <param name="record">The character or pet that will own the spec profile.</param>
        /// <returns>The newly created SpecProfile.</returns>
        public static SpecProfile NewSpecProfile(Character owner, int specIndex)
        {
            return SpecProfile.NewSpecProfile(owner, specIndex, (byte[]) owner.Archetype.ActionButtons.Clone());
        }

        /// <summary>Creates a new SpecProfile and saves it to the db.</summary>
        /// <param name="record">The character or pet that will own the spec profile.</param>
        /// <returns>The newly created SpecProfile.</returns>
        public static SpecProfile NewSpecProfile(Character owner, int specIndex, byte[] actionbar)
        {
            return new SpecProfile(owner.EntityId.Low, specIndex)
            {
                ActionButtons = actionbar,
                GlyphIds = new uint[6]
            };
        }

        private SpecProfile()
        {
            this.TalentSpells = new List<SpellRecord>();
        }

        private SpecProfile(uint lowId, int specIndex)
            : this()
        {
            this._characterGuid = (int) lowId;
            this.SpecIndex = specIndex;
            this.State = RecordState.New;
        }

        /// <summary>
        /// Primary key. A combination of Character id and TalentGroup.
        /// </summary>
        [PrimaryKey(PrimaryKeyType.Assigned)]
        public long SpecRecordId
        {
            get { return Utility.MakeLong(this._characterGuid, this.SpecIndex); }
            set
            {
                int high = 0;
                Utility.UnpackLong(value, ref this._characterGuid, ref high);
                this.SpecIndex = high;
            }
        }

        public uint CharacterGuid
        {
            get { return (uint) this._characterGuid; }
            set { this._characterGuid = (int) value; }
        }

        /// <summary>The Id of the Talent Group currently in use.</summary>
        [Property]
        public int SpecIndex { get; set; }

        /// <summary>TODO: Move to own table</summary>
        [Property]
        public uint[] GlyphIds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Property]
        public byte[] ActionButtons { get; set; }

        public List<SpellRecord> TalentSpells { get; internal set; }
    }
}