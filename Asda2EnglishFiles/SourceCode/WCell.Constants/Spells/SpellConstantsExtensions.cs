namespace WCell.Constants.Spells
{
    public static class SpellConstantsExtensions
    {
        public static bool HasAnyFlag(this SpellTargetFlags flags, SpellTargetFlags otherFlags)
        {
            return (flags & otherFlags) != SpellTargetFlags.Self;
        }

        public static bool HasAnyFlag(this SpellAttributes flags, SpellAttributes otherFlags)
        {
            return (flags & otherFlags) != SpellAttributes.None;
        }

        public static bool HasAnyFlag(this SpellAttributesEx flags, SpellAttributesEx otherFlags)
        {
            return (flags & otherFlags) != SpellAttributesEx.None;
        }

        public static bool HasAnyFlag(this ProcTriggerFlags flags, ProcTriggerFlags otherFlags)
        {
            return (flags & otherFlags) != ProcTriggerFlags.None;
        }

        public static bool HasAnyFlag(this ProcHitFlags flags, ProcHitFlags otherFlags)
        {
            return (flags & otherFlags) != ProcHitFlags.None;
        }

        public static bool RequireHitFlags(this ProcTriggerFlags flags)
        {
            return flags.HasAnyFlag(ProcTriggerFlags.RequiringHitFlags);
        }

        public static bool HasAnyFlag(this AuraStateMask mask, AuraState state)
        {
            return (mask & (AuraStateMask) (1 << (int) (state - 1U &
                                                        (AuraState.RejuvenationOrRegrowth | AuraState.DeadlyPoison)))
                   ) != AuraStateMask.None;
        }

        public static bool HasAnyFlag(this AuraStateMask mask, AuraStateMask mask2)
        {
            return (mask & mask2) != AuraStateMask.None;
        }

        public static bool HasAnyFlag(this DamageSchoolMask flags, DamageSchoolMask otherFlags)
        {
            return (flags & otherFlags) != DamageSchoolMask.None;
        }

        public static bool HasAnyFlag(this DamageSchoolMask flags, DamageSchool school)
        {
            return (flags & (DamageSchoolMask) (1 << (int) (school & (DamageSchool) 31))) != DamageSchoolMask.None;
        }

        public static RuneMask ToMask(this RuneType type)
        {
            return (RuneMask) (1U << (int) (type & (RuneType) 31));
        }

        public static bool HasAnyFlag(this RuneMask mask, RuneMask mask2)
        {
            return (mask & mask2) != (RuneMask) 0;
        }

        public static bool HasAnyFlag(this RuneMask mask, RuneType type)
        {
            return (mask & type.ToMask()) != (RuneMask) 0;
        }
    }
}