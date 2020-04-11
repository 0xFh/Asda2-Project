using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Util.Data;

namespace WCell.RealmServer.Quests
{
    /// <summary>
    /// Consists of a Type of objects, an id of the object's Template and
    /// amount of objects to be searched for in order to complete a <see cref="T:WCell.RealmServer.Quests.Quest" />.
    /// </summary>
    public class QuestInteractionTemplate
    {
        [NotPersistent] public uint[] TemplateId = new uint[1];

        /// <summary>
        /// Either <see cref="F:WCell.Constants.Updates.ObjectTypeId.Unit" /> or <see cref="F:WCell.Constants.Updates.ObjectTypeId.GameObject" />
        /// </summary>
        [NotPersistent] public ObjectTypeId ObjectType = ObjectTypeId.None;

        public int Amount;

        /// <summary>
        /// Spell to be casted.
        /// If not set, the objective is to kill or use the target.
        /// </summary>
        public SpellId RequiredSpellId;

        [NotPersistent] public uint Index;

        /// <summary>
        /// The RawId is used in certain Packets.
        /// It encodes TemplateId and Type
        /// The setter should only ever be used
        /// when loading info from the database!
        /// </summary>
        public uint RawId
        {
            get
            {
                if (this.ObjectType != ObjectTypeId.GameObject)
                    return this.TemplateId[0];
                return (uint) (-1 - (int) this.TemplateId[0] + 1);
            }
            set
            {
                if (value > 2147483648U)
                {
                    this.TemplateId[0] = (uint) (-1 - (int) value + 1);
                    this.ObjectType = ObjectTypeId.GameObject;
                }
                else
                {
                    if (value == 0U)
                        return;
                    this.TemplateId[0] = value;
                    this.ObjectType = ObjectTypeId.Unit;
                }
            }
        }

        public bool IsValid
        {
            get
            {
                if (this.TemplateId[0] == 0U)
                    return this.RequiredSpellId != SpellId.None;
                return true;
            }
        }

        public override string ToString()
        {
            ((IEnumerable<uint>) this.TemplateId).Where<uint>((Func<uint, bool>) (templ => templ != 0U));
            return (this.Amount != 1 ? (object) (this.Amount.ToString() + "x ") : (object) "").ToString() +
                   (object) this.ObjectType + " " +
                   this.ObjectType.ToString((IEnumerable<uint>) this.TemplateId, ", ") +
                   (this.RequiredSpellId != SpellId.None
                       ? (object) (" - Spell: " + (object) this.RequiredSpellId)
                       : (object) "");
        }
    }
}