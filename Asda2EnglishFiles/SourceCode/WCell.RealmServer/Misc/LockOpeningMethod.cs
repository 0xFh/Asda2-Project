using System;
using WCell.Constants;
using WCell.Constants.Skills;

namespace WCell.RealmServer.Misc
{
    /// <summary>Different ways of interacting with a lock</summary>
    public class LockOpeningMethod
    {
        /// <summary>The index within the LockEntry</summary>
        public readonly uint Index;

        /// <summary>What kind of method is this (we don't use a key)</summary>
        public LockInteractionType InteractionType;

        /// <summary>The profession required to open this Lock</summary>
        public SkillId RequiredSkill;

        /// <summary>Required value in the Profession</summary>
        public uint RequiredSkillValue;

        public LockOpeningMethod(uint index)
        {
            this.Index = index;
        }

        public override string ToString()
        {
            return (this.InteractionType + ((this.RequiredSkillValue > 0)
                        ? string.Concat(new object[]
                            {" (Requires: ", this.RequiredSkillValue, " ", this.RequiredSkill, ")"})
                        : ""));
        }
    }
}