using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells
{
    /// <summary>
    /// Contains a list of all SpellProcEventEntries and some helper functions
    /// </summary>
    public static class ProcEventHelper
    {
        public static readonly Dictionary<SpellId, SpellProcEventEntry> Entries =
            new Dictionary<SpellId, SpellProcEventEntry>();

        /// <summary>
        /// Apply custom proc settings from SpellProcEventEntry to all spells
        /// </summary>
        public static void PatchSpells(Spell[] spells)
        {
            foreach (SpellId key in ProcEventHelper.Entries.Keys)
            {
                Spell spell = spells[(int) key];
                if (spell != null)
                {
                    SpellProcEventEntry entry = ProcEventHelper.Entries[spell.SpellId];
                    if (spell.Line == null)
                        ProcEventHelper.PatchSpell(spell, entry);
                    else
                        spell.Line.LineId.Apply((Action<Spell>) (spellToPatch =>
                            ProcEventHelper.PatchSpell(spellToPatch, entry)));
                }
            }
        }

        /// <summary>
        /// Apply custom proc settings from SpellProcEventEntry to a given spell
        /// </summary>
        private static void PatchSpell(Spell spell, SpellProcEventEntry procEntry)
        {
            if (procEntry.SchoolMask != DamageSchoolMask.None)
                spell.SchoolMask = procEntry.SchoolMask;
            if (procEntry.SpellClassSet != SpellClassSet.Generic)
                spell.SpellClassSet = procEntry.SpellClassSet;
            if (procEntry.ProcFlags != ProcTriggerFlags.None)
                spell.ProcTriggerFlagsProp = procEntry.ProcFlags;
            if (procEntry.ProcFlagsEx != ProcFlagsExLegacy.None)
                spell.ProcHitFlags = (ProcHitFlags) (procEntry.ProcFlagsEx &
                                                     (ProcFlagsExLegacy.NormalHit | ProcFlagsExLegacy.CriticalHit |
                                                      ProcFlagsExLegacy.Miss | ProcFlagsExLegacy.Resist |
                                                      ProcFlagsExLegacy.Dodge | ProcFlagsExLegacy.Parry |
                                                      ProcFlagsExLegacy.Block | ProcFlagsExLegacy.Evade |
                                                      ProcFlagsExLegacy.Immune | ProcFlagsExLegacy.Deflect |
                                                      ProcFlagsExLegacy.Absorb | ProcFlagsExLegacy.Reflect |
                                                      ProcFlagsExLegacy.Interrupt | ProcFlagsExLegacy.FullBlock));
            if ((double) procEntry.CustomChance != 0.0)
                spell.ProcChance = (uint) procEntry.CustomChance;
            foreach (SpellEffect spellEffect in ((IEnumerable<SpellEffect>) spell.Effects).Where<SpellEffect>(
                (Func<SpellEffect, bool>) (effect =>
                {
                    if (effect.AuraType == AuraType.ProcTriggerSpell)
                        return effect.EffectIndex != EffectIndex.Custom;
                    return false;
                })))
            {
                uint[] spellFamilyMask = procEntry.GetSpellFamilyMask(spellEffect.EffectIndex);
                if (spellFamilyMask != null)
                    spellEffect.AffectMask = ((IEnumerable<uint>) spellFamilyMask).ToArray<uint>();
            }
        }
    }
}