using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells
{
    /// <summary>A set of Runes that Death Knights use</summary>
    public class RuneSet
    {
        public static float DefaultRuneCooldownPerSecond = 0.1f;
        public readonly RuneType[] ActiveRunes = new RuneType[6];

        public RuneSet(Character owner)
        {
            this.Owner = owner;
        }

        public Character Owner { get; internal set; }

        public float[] Cooldowns
        {
            get { return this.Owner.Record.RuneCooldowns; }
        }

        internal void InitRunes(Character owner)
        {
            this.Owner = owner;
            this.UnpackRuneSetMask(owner.Record.RuneSetMask);
            float[] cooldowns = this.Cooldowns;
            if (cooldowns == null || cooldowns.Length != 6)
                owner.Record.RuneCooldowns = new float[6];
            for (RuneType type = RuneType.Blood; type < RuneType.End; ++type)
                this.SetCooldown(type, RuneSet.DefaultRuneCooldownPerSecond);
        }

        internal void Dispose()
        {
            this.Owner = (Character) null;
        }

        public int GetIndexOfFirstRuneOfType(RuneType type, bool onlyIfNotOnCooldown = false)
        {
            for (int index = 0; index < 6; ++index)
            {
                if (this.ActiveRunes[index] == type && (!onlyIfNotOnCooldown || (double) this.Cooldowns[index] <= 0.0))
                    return index;
            }

            return -1;
        }

        public bool Convert(RuneType from, RuneType to, bool onlyIfNotOnCooldown = true)
        {
            for (uint index = 0; index < 6U; ++index)
            {
                if (this.ActiveRunes[index] == from && (!onlyIfNotOnCooldown || (double) this.Cooldowns[index] <= 0.0))
                {
                    this.Convert(index, to);
                    return true;
                }
            }

            return false;
        }

        public void ConvertToDefault(uint index)
        {
            this.Convert(index, SpellConstants.DefaultRuneSet[index]);
        }

        public void Convert(uint index, RuneType to)
        {
            this.ActiveRunes[index] = to;
            SpellHandler.SendConvertRune(this.Owner.Client, index, to);
        }

        /// <summary>Returns how many runes of the given type are ready</summary>
        public int GetReadyRunes(RuneType type)
        {
            int num = 0;
            for (int index = 0; index < 6; ++index)
            {
                if (this.ActiveRunes[index] == type && (double) this.Cooldowns[index] <= 0.0)
                    ++num;
            }

            return num;
        }

        /// <summary>
        /// Whether there are enough runes in this set to satisfy the given cost requirements
        /// </summary>
        public bool HasEnoughRunes(Spell spell)
        {
            RuneCostEntry runeCostEntry = spell.RuneCostEntry;
            if (runeCostEntry == null || !runeCostEntry.CostsRunes ||
                this.Owner.Auras.GetModifiedInt(SpellModifierType.PowerCost, spell, 1) != 1)
                return true;
            for (RuneType runeType = RuneType.Blood; runeType < (RuneType) runeCostEntry.CostPerType.Length; ++runeType)
            {
                int num = runeCostEntry.CostPerType[(int) runeType];
                if (num > 0)
                {
                    for (int index = 0; index < 6; ++index)
                    {
                        if ((this.ActiveRunes[index] == runeType || this.ActiveRunes[index] == RuneType.Death) &&
                            (double) this.Cooldowns[index] <= 0.0)
                            --num;
                    }

                    if (num > 0)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Method is internal because we don't have a packet yet to signal the client spontaneous cooldown updates
        /// </summary>
        internal void ConsumeRunes(Spell spell)
        {
            RuneCostEntry runeCostEntry = spell.RuneCostEntry;
            if (runeCostEntry == null || !runeCostEntry.CostsRunes ||
                this.Owner.Auras.GetModifiedInt(SpellModifierType.PowerCost, spell, 1) != 1)
                return;
            for (RuneType runeType = RuneType.Blood; runeType < (RuneType) runeCostEntry.CostPerType.Length; ++runeType)
            {
                int num = runeCostEntry.CostPerType[(int) runeType];
                if (num > 0)
                {
                    for (uint index = 0; index < 6U; ++index)
                    {
                        if (this.ActiveRunes[index] == runeType && (double) this.Cooldowns[index] <= 0.0)
                        {
                            this.StartCooldown(index);
                            --num;
                            if (num == 0)
                                return;
                        }
                    }

                    for (uint index = 0; index < 6U; ++index)
                    {
                        if (this.ActiveRunes[index] == RuneType.Death && (double) this.Cooldowns[index] <= 0.0)
                        {
                            this.ConvertToDefault(index);
                            this.StartCooldown(index);
                            --num;
                            if (num == 0)
                                return;
                        }
                    }
                }
            }
        }

        /// <summary>TODO: Send update to client, if necessary</summary>
        internal void StartCooldown(uint index)
        {
            this.Cooldowns[index] = 1f;
        }

        /// <summary>TODO: Send update to client, if necessary</summary>
        internal void UnsetCooldown(uint index)
        {
            this.Cooldowns[index] = 0.0f;
        }

        internal void UpdateCooldown(int dtMillis)
        {
            float[] cooldowns = this.Cooldowns;
            for (uint index = 0; index < 6U; ++index)
            {
                float num = cooldowns[index] -
                            (float) (((double) dtMillis * (double) this.GetCooldown(this.ActiveRunes[index]) + 500.0) /
                                     1000.0);
                cooldowns[index] = (double) num <= 0.0 ? 0.0f : num;
            }
        }

        /// <summary>
        /// Gets the cooldown of the given RuneType in rune refreshment per second.
        /// For example:
        /// 1 = a rune refreshes in one second;
        /// 0.1 = a rune refrehes in 10 seconds.
        /// </summary>
        public float GetCooldown(RuneType type)
        {
            return this.Owner.GetFloat((UpdateFieldId) ((PlayerFields) ((byte) 25 + type)));
        }

        public void SetCooldown(RuneType type, float cdPerSecond)
        {
            this.Owner.SetFloat((UpdateFieldId) ((PlayerFields) ((byte) 25 + type)), cdPerSecond);
        }

        public void ModCooldown(RuneType type, float delta)
        {
            this.SetCooldown(type, this.GetCooldown(type) + delta);
        }

        /// <summary>Modifies all cooldowns by the given percentage</summary>
        /// <param name="percentDelta">If this value is 100, runes will cooldown in half the time</param>
        /// <returns>The delta of all rune types</returns>
        public float[] ModAllCooldownsPercent(int percentDelta)
        {
            float[] numArray = new float[4];
            for (RuneType type = RuneType.Blood; type < RuneType.End; ++type)
            {
                float cooldown = this.GetCooldown(type);
                float cdPerSecond = cooldown + (float) ((double) cooldown * (double) percentDelta / 100.0);
                this.SetCooldown(type, cdPerSecond);
                numArray[(int) type] = cdPerSecond - cooldown;
            }

            return numArray;
        }

        public int PackRuneSetMask()
        {
            int num = 0;
            for (int index = 0; index < 6; ++index)
                num |= (int) (this.ActiveRunes[index] + (byte) 1) << SpellConstants.BitsPerRune * index;
            return num;
        }

        public void UnpackRuneSetMask(int runeSetMask)
        {
            if (runeSetMask == 0)
            {
                SpellConstants.DefaultRuneSet.CopyTo((Array) this.ActiveRunes, 0);
            }
            else
            {
                for (int index = 0; index < 6; ++index)
                {
                    RuneType runeType = (RuneType) ((runeSetMask & SpellConstants.SingleRuneFullBitMask) - 1);
                    this.ActiveRunes[index] = runeType >= RuneType.End || runeType < RuneType.Blood
                        ? SpellConstants.DefaultRuneSet[index]
                        : runeType;
                    runeSetMask >>= SpellConstants.BitsPerRune;
                }
            }
        }

        /// <summary>Used for packets</summary>
        internal byte GetActiveRuneMask()
        {
            int num = 0;
            float[] cooldowns = this.Cooldowns;
            for (int index = 0; index < 6; ++index)
            {
                if ((double) cooldowns[index] == 0.0)
                    num |= 1 << index;
            }

            return (byte) num;
        }
    }
}