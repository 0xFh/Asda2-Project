using NHibernate;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Spells;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.RealmServer.Spells.Auras
{
    /// <summary>Represents the collection of all Auras of a Unit</summary>
    public class AuraCollection : IEnumerable<Aura>, IEnumerable
    {
        /// <summary>
        /// All non-passive Auras.
        /// Through items and racial abilities, one Unit can easily have 100 Auras active at a time -
        /// No need to iterate over all of them when checking for interruption etc.
        /// </summary>
        protected readonly Aura[] m_visibleAuras = new Aura[64];

        public const byte InvalidIndex = 255;
        protected Unit m_owner;
        protected Dictionary<AuraIndexId, Aura> m_auras;

        /// <summary>
        /// An immutable array that contains all Auras and is re-created
        /// whenever an Aura is added or removed (lazily prevents threading and update issues -&gt; Find something better).
        /// TODO: Recycle
        /// </summary>
        protected Aura[] m_AuraArray;

        protected int m_visAuraCount;

        public AuraCollection(Unit owner)
        {
            this.m_auras = new Dictionary<AuraIndexId, Aura>();
            this.m_AuraArray = Aura.EmptyArray;
            this.m_owner = owner;
        }

        public Aura[] VisibleAuras
        {
            get { return this.m_visibleAuras; }
        }

        public int VisibleAuraCount
        {
            get { return this.m_visAuraCount; }
        }

        public Aura[] ActiveAuras
        {
            get { return this.m_AuraArray; }
        }

        public Unit Owner
        {
            get { return this.m_owner; }
            internal set { this.m_owner = value; }
        }

        public Character OwnerChar
        {
            get { return this.m_owner as Character; }
        }

        public int Count
        {
            get { return this.m_auras.Count; }
        }

        public Aura this[SpellId spellId, bool positive]
        {
            get
            {
                Spell index = SpellHandler.Get(spellId);
                if (index != null)
                    return this[index, positive];
                return (Aura) null;
            }
        }

        public Aura this[Spell spell, bool positive]
        {
            get
            {
                if (spell.CanApplyMultipleTimes)
                {
                    foreach (Aura aura in this.m_AuraArray)
                    {
                        if (aura.Spell == spell)
                            return aura;
                    }
                }
                else
                {
                    Aura aura;
                    this.m_auras.TryGetValue(new AuraIndexId(spell.AuraUID, positive), out aura);
                    if (aura != null && (int) aura.Spell.Id == (int) spell.Id)
                        return aura;
                }

                return (Aura) null;
            }
        }

        /// <summary>Returns the first visible Aura with the given SpellId</summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Aura this[SpellId id]
        {
            get
            {
                Spell index = SpellHandler.Get(id);
                if (index != null)
                    return this[index];
                return (Aura) null;
            }
        }

        public Aura this[Spell spell]
        {
            get
            {
                Aura aura = spell.IsBeneficial || spell.IsNeutral ? this[spell, true] : (Aura) null;
                if (aura == null && spell.HasHarmfulEffects)
                    aura = this[spell, false];
                if (aura != null && aura.Spell == spell)
                    return aura;
                return (Aura) null;
            }
        }

        public Aura this[SpellLineId id, bool positive]
        {
            get
            {
                SpellLine line = id.GetLine();
                if (line != null)
                    return this[line, positive];
                return (Aura) null;
            }
        }

        public Aura this[SpellLine line, bool positive]
        {
            get
            {
                Aura aura;
                this.m_auras.TryGetValue(new AuraIndexId(line.AuraUID, positive), out aura);
                if (aura != null && aura.Spell.Line == line)
                    return aura;
                return (Aura) null;
            }
        }

        /// <summary>Returns the first visible Aura with the given SpellId</summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Aura this[SpellLineId id]
        {
            get
            {
                SpellLine line = id.GetLine();
                if (line != null)
                    return this[line];
                return (Aura) null;
            }
        }

        public Aura this[SpellLine line]
        {
            get
            {
                Aura aura;
                if (!this.m_auras.TryGetValue(new AuraIndexId(line.AuraUID, !line.BaseSpell.HasHarmfulEffects),
                        out aura) &&
                    !this.m_auras.TryGetValue(new AuraIndexId(line.AuraUID, line.BaseSpell.HasHarmfulEffects),
                        out aura))
                    return (Aura) null;
                if (aura.Spell.Line != line)
                    return (Aura) null;
                return aura;
            }
        }

        public Aura this[AuraIndexId auraId]
        {
            get
            {
                Aura aura;
                this.m_auras.TryGetValue(auraId, out aura);
                return aura;
            }
        }

        /// <summary>
        /// Returns the first visible (not passive) Aura with the given Type (if any).
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public Aura this[AuraType type]
        {
            get
            {
                foreach (Aura visibleAura in this.m_visibleAuras)
                {
                    if (visibleAura != null &&
                        visibleAura.Spell.HasEffectWith((Predicate<SpellEffect>) (effect => effect.AuraType == type)))
                        return visibleAura;
                }

                return (Aura) null;
            }
        }

        /// <summary>
        /// Returns the first Aura that matches the given Predicate.
        /// Only looks in active Auras.
        /// </summary>
        /// <remarks>Requires map context.</remarks>
        public Aura FindFirst(Predicate<Aura> condition)
        {
            foreach (Aura visibleAura in this.m_visibleAuras)
            {
                if (visibleAura != null && condition(visibleAura))
                    return visibleAura;
            }

            return (Aura) null;
        }

        /// <summary>
        /// Iterates over all Auras and returns the n'th visible one
        /// </summary>
        /// <returns>The nth visible Aura or null if there is none</returns>
        public Aura GetAt(uint n)
        {
            if ((long) n < (long) this.m_visibleAuras.Length)
                return this.m_visibleAuras[n];
            return (Aura) null;
        }

        /// <summary>
        /// Get an Aura that is incompatible with the one represented by the given spell
        /// </summary>
        /// <returns>Whether or not another Aura may be applied</returns>
        public Aura GetAura(ObjectReference caster, AuraIndexId id, Spell spell)
        {
            Aura aura1 = this[id];
            if (aura1 != null)
                return aura1;
            if (spell.AuraCasterGroup != null)
            {
                int num = 0;
                foreach (Aura aura2 in this.m_AuraArray)
                {
                    if (aura2.CasterReference.EntityId == caster.EntityId &&
                        spell.AuraCasterGroup == aura2.Spell.AuraCasterGroup)
                    {
                        ++num;
                        if (num >= spell.AuraCasterGroup.MaxCount)
                            return aura2;
                    }
                }
            }

            return (Aura) null;
        }

        public int GetTotalAuraModifier(AuraType type)
        {
            int num = 0;
            foreach (Aura aura in this)
            {
                foreach (SpellEffect effect in aura.Spell.Effects)
                {
                    if (effect.AuraType == type)
                        num += effect.CalcEffectValue();
                }
            }

            return num;
        }

        /// <summary>
        /// Gets the total modifiers of an AuraType in this AuraCollection.
        /// Takes only auras with a given miscvalue into account.
        /// </summary>
        public int GetTotalAuraModifier(AuraType type, int miscvalue)
        {
            int num = 0;
            foreach (Aura aura in this)
            {
                foreach (SpellEffect effect in aura.Spell.Effects)
                {
                    if (effect.AuraType == type && effect.MiscValue == miscvalue)
                        num += effect.CalcEffectValue();
                }
            }

            return num;
        }

        public bool Contains(AuraIndexId id)
        {
            return this[id] != null;
        }

        public bool Contains(uint auraUID, bool beneficial)
        {
            return this[new AuraIndexId(auraUID, beneficial)] != null;
        }

        public bool Contains(SpellId id)
        {
            return this[id] != null;
        }

        public bool Contains(Spell spell)
        {
            return this[spell] != null;
        }

        public bool ContainsAny(params Spell[] spells)
        {
            foreach (Spell spell in spells)
            {
                if (this[spell] != null)
                    return true;
            }

            return false;
        }

        /// <summary>Returns the first visible Aura with the given SpellId</summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Contains(SpellLineId id)
        {
            SpellLine line = id.GetLine();
            if (line != null)
                return this[line] != null;
            return false;
        }

        public bool Contains(SpellLine line)
        {
            Aura aura;
            this.m_auras.TryGetValue(new AuraIndexId(line.AuraUID, !line.BaseSpell.HasHarmfulEffects), out aura);
            if (aura != null)
                return aura.Spell.Line == line;
            return false;
        }

        /// <summary>
        /// Applies the given spell as an Aura (the owner being the caster) to the owner of this AuraCollection.
        /// Also initializes the new Aura.
        /// </summary>
        /// <returns>null if Spell is not an Aura</returns>
        public Aura CreateSelf(SpellId id, bool noTimeout = false)
        {
            return this.CreateAndStartAura(this.m_owner.SharedReference, SpellHandler.Get(id), noTimeout,
                (WCell.RealmServer.Entities.Item) null);
        }

        /// <summary>
        /// Applies the given spell as an Aura (the owner being the caster) to the owner of this AuraCollection.
        /// Also initializes the new Aura.
        /// </summary>
        /// <returns>null if Spell is not an Aura</returns>
        public Aura CreateSelf(Spell spell, bool noTimeout = false)
        {
            return this.CreateAndStartAura(this.m_owner.SharedReference, spell, noTimeout,
                (WCell.RealmServer.Entities.Item) null);
        }

        /// <summary>
        /// Applies the given spell as a buff or debuff.
        /// Also initializes the new Aura.
        /// </summary>
        /// <returns>null if Spell is not an Aura</returns>
        public Aura CreateAndStartAura(ObjectReference caster, SpellId spell, bool noTimeout,
            WCell.RealmServer.Entities.Item usedItem = null)
        {
            return this.CreateAndStartAura(caster, SpellHandler.Get(spell), noTimeout, usedItem);
        }

        /// <summary>
        /// Applies the given spell as a buff or debuff.
        /// Also initializes the new Aura.
        /// </summary>
        /// <returns>null if Spell is not an Aura or an already existing version of the Aura that was refreshed</returns>
        public Aura CreateAndStartAura(ObjectReference caster, Spell spell, bool noTimeout,
            WCell.RealmServer.Entities.Item usedItem = null)
        {
            try
            {
                Aura aura = this.CreateAura(caster, spell, usedItem);
                if (aura != null)
                {
                    aura.Start((ITickTimer) null, noTimeout);
                    return aura;
                }
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex, "Unable to add new Aura \"{0}\" by \"{1}\" to: {2}", (object) spell,
                    (object) caster, (object) this.m_owner);
            }

            return (Aura) null;
        }

        /// <summary>
        /// Called when an Aura has been dynamically created (not called, when applying via SpellCast)
        /// </summary>
        private void OnCreated(Aura aura)
        {
            if (!aura.Spell.IsAreaAura || !(this.Owner.EntityId == aura.CasterReference.EntityId))
                return;
            AreaAura areaAura = new AreaAura((WorldObject) this.Owner, aura.Spell);
        }

        /// <summary>
        /// Adds a new Aura with the given information to the Owner.
        /// Does not initialize the new Aura.
        /// If you use this method, make sure to call <see cref="M:WCell.RealmServer.Spells.Auras.Aura.Start" /> on the newly created Aura.
        /// Overrides any existing Aura that matches.
        /// </summary>
        /// <returns>null if Spell is not an Aura</returns>
        internal Aura CreateAura(ObjectReference casterReference, Spell spell, List<AuraEffectHandler> handlers,
            WCell.RealmServer.Entities.Item usedItem, bool beneficial)
        {
            byte freeIndex = this.GetFreeIndex(beneficial);
            if (freeIndex == byte.MaxValue)
                return (Aura) null;
            Aura aura = new Aura(this, casterReference, spell, handlers, freeIndex, beneficial);
            aura.UsedItem = usedItem;
            this.AddAura(aura, false);
            return aura;
        }

        /// <summary>
        /// Applies the given spell as a buff or debuff.
        /// Does not necessarily create
        /// Also, initializes new Auras.
        /// </summary>
        /// <returns>null if Spell is not an Aura or an already existing version of the Aura that was refreshed</returns>
        public Aura CreateAura(ObjectReference caster, Spell spell, WCell.RealmServer.Entities.Item usedItem = null)
        {
            try
            {
                bool flag = spell.IsBeneficialFor(caster, (WorldObject) this.m_owner);
                AuraIndexId auraUid = spell.GetAuraUID(flag);
                SpellFailedReason err = SpellFailedReason.Ok;
                Aura aura1 = this.GetAura(caster, auraUid, spell);
                if (aura1 != null && !this.PrepareStackOrOverride(caster, aura1, spell, ref err, (SpellCast) null))
                {
                    if (err == SpellFailedReason.Ok)
                        return aura1;
                    if (caster.Object is Character)
                        SpellHandler.SendCastFailed((IPacketReceiver) caster.Object, spell, err);
                    return (Aura) null;
                }

                List<AuraEffectHandler> auraEffectHandlers = spell.CreateAuraEffectHandlers(caster, this.m_owner, flag);
                if (auraEffectHandlers != null)
                {
                    Aura aura2 = this.CreateAura(caster, spell, auraEffectHandlers, usedItem, flag);
                    if (aura2 != null)
                        this.OnCreated(aura2);
                    return aura2;
                }
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex, "Unable to add new Aura \"{0}\" by \"{1}\" to: {2}", (object) spell,
                    (object) caster, (object) this.m_owner);
            }

            return (Aura) null;
        }

        /// <summary>Adds an already created Aura and optionally starts it</summary>
        public virtual void AddAura(Aura aura, bool start = true)
        {
            AuraIndexId id = aura.Id;
            if (this.m_auras.ContainsKey(aura.Id))
            {
                LogManager.GetCurrentClassLogger()
                    .Warn("Tried to add Aura \"{0}\" by \"{1}\" when it was already added, to: {2}", (object) aura,
                        (object) aura.CasterReference, (object) this.Owner);
            }
            else
            {
                foreach (Aura aura1 in this.m_auras.Values)
                {
                    if ((int) aura1.Spell.RealId == (int) aura.Spell.RealId)
                    {
                        aura1.Remove(true);
                        break;
                    }
                }

                this.m_auras.Add(id, aura);
                if (!aura.Spell.IsPassive)
                {
                    this.m_visibleAuras[(int) aura.Index] = aura;
                    ++this.m_visAuraCount;
                }

                this.InvalidateAurasCopy();
                aura.IsAdded = true;
                if (!start)
                    return;
                aura.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Aura.AuraOverrideStatus GetOverrideStatus(ObjectReference caster, Spell spell)
        {
            Aura aura = this[spell];
            if (aura != null)
                return aura.GetOverrideStatus(caster, spell);
            return Aura.AuraOverrideStatus.NotPresent;
        }

        /// <summary>
        /// Stack or removes the Aura represented by the given spell, if possible.
        /// Returns true if there is no incompatible Aura or if the Aura could be removed.
        /// <param name="err">Ok, if stacked or no incompatible Aura is blocking a new Aura</param>
        /// </summary>
        internal bool PrepareStackOrOverride(ObjectReference caster, AuraIndexId id, Spell spell,
            ref SpellFailedReason err, SpellCast triggeringCast = null)
        {
            Aura aura = this.GetAura(caster, id, spell);
            if (aura != null)
                return this.PrepareStackOrOverride(caster, aura, spell, ref err, triggeringCast);
            return true;
        }

        internal bool PrepareStackOrOverride(ObjectReference caster, Aura oldAura, Spell spell,
            ref SpellFailedReason err, SpellCast triggeringCast = null)
        {
            Aura.AuraOverrideStatus auraOverrideStatus = oldAura.GetOverrideStatus(caster, spell);
            if (auraOverrideStatus == Aura.AuraOverrideStatus.Replace)
                auraOverrideStatus = oldAura.TryRemove(true)
                    ? Aura.AuraOverrideStatus.NotPresent
                    : Aura.AuraOverrideStatus.Bounced;
            switch (auraOverrideStatus)
            {
                case Aura.AuraOverrideStatus.NotPresent:
                    return true;
                case Aura.AuraOverrideStatus.Refresh:
                    oldAura.Refresh(caster);
                    return false;
                default:
                    err = SpellFailedReason.AuraBounced;
                    return false;
            }
        }

        /// <summary>
        /// Removes all visible Auras that match the given predicate
        /// </summary>
        /// <param name="predicate"></param>
        public void RemoveWhere(Predicate<Aura> predicate)
        {
            foreach (Aura visibleAura in this.m_visibleAuras)
            {
                if (visibleAura != null && predicate(visibleAura))
                    visibleAura.Remove(false);
            }
        }

        /// <summary>
        /// Removes up to the given max amount of visible Auras that match the given predicate
        /// </summary>
        /// <param name="predicate"></param>
        public void RemoveWhere(Predicate<Aura> predicate, int max)
        {
            Aura[] visibleAuras = this.m_visibleAuras;
            int num = 0;
            foreach (Aura aura in visibleAuras)
            {
                if (aura != null && predicate(aura))
                {
                    aura.Remove(false);
                    if (++num >= max)
                        break;
                }
            }
        }

        /// <summary>
        /// Removes the first occurance of an Aura that matches the given predicate
        /// </summary>
        /// <param name="predicate"></param>
        public void RemoveFirstVisibleAura(Predicate<Aura> predicate)
        {
            foreach (Aura visibleAura in this.m_visibleAuras)
            {
                if (visibleAura != null && predicate(visibleAura))
                {
                    visibleAura.Remove(false);
                    break;
                }
            }
        }

        /// <summary>Removes auras based on their interrupt flag.</summary>
        /// <param name="interruptFlags">the interrupt flags to remove the auras by</param>
        public void RemoveByFlag(AuraInterruptFlags interruptFlags)
        {
            foreach (Aura visibleAura in this.m_visibleAuras)
            {
                if (visibleAura != null &&
                    (visibleAura.Spell.AuraInterruptFlags & interruptFlags) != AuraInterruptFlags.None)
                    visibleAura.Remove(false);
            }
        }

        public bool Remove(uint auraUID, bool positive)
        {
            return this.Remove(new AuraIndexId()
            {
                AuraUID = auraUID,
                IsPositive = positive
            });
        }

        public bool Remove(AuraIndexId auraId)
        {
            Aura aura;
            if (!this.m_auras.TryGetValue(auraId, out aura))
                return false;
            aura.Remove(true);
            return true;
        }

        public bool Remove(SpellId id)
        {
            Spell spell = SpellHandler.Get(id);
            if (spell != null)
                return this.Remove(spell);
            return false;
        }

        /// <summary>
        /// Removes and cancels the first Aura of the given SpellLine
        /// </summary>
        public bool Remove(SpellLineId spellLine)
        {
            Aura aura = this[spellLine];
            if (aura == null)
                return false;
            aura.Remove(true);
            return true;
        }

        /// <summary>
        /// Removes and cancels the first Aura of the given SpellLine
        /// </summary>
        public bool Remove(SpellLine spellLine)
        {
            Aura aura = this[spellLine];
            if (aura == null)
                return false;
            aura.Remove(true);
            return true;
        }

        /// <summary>Removes and cancels the first Aura of the given Spell</summary>
        public bool Remove(Spell spell)
        {
            Aura aura = this[spell];
            if (aura == null)
                return false;
            aura.Remove(true);
            return true;
        }

        /// <summary>
        /// Removes the given Aura without cancelling it.
        /// Automatically called by <see cref="M:WCell.RealmServer.Spells.Auras.Aura.Remove(System.Boolean)" />.
        /// </summary>
        protected internal virtual void Remove(Aura aura)
        {
            this.m_auras.Remove(aura.Id);
            if (aura.Spell.IsProc)
                this.m_owner.RemoveProcHandler((IProcHandler) aura);
            if (!aura.Spell.IsPassive)
            {
                this.m_visibleAuras[(int) aura.Index] = (Aura) null;
                --this.m_visAuraCount;
            }

            this.InvalidateAurasCopy();
            this.OnAuraChange(aura);
        }

        /// <summary>
        /// Removes all Aura effects, when the Owner is about to leave the world (due to logout / deletion).
        /// </summary>
        public void CleanupAuras()
        {
            for (int index = 0; index < this.m_AuraArray.Length; ++index)
            {
                Aura aura = this.m_AuraArray[index];
                if (aura != null)
                    aura.Cleanup();
            }
        }

        /// <summary>
        /// Removes all auras that are casted by anyone but this unit itself
        /// </summary>
        public void RemoveOthersAuras()
        {
            for (int index = 0; index < this.m_visibleAuras.Length; ++index)
            {
                Aura visibleAura = this.m_visibleAuras[index];
                if (visibleAura != null && visibleAura.CasterUnit != this.m_owner)
                    visibleAura.Remove(true);
            }
        }

        /// <summary>Removes all visible buffs and debuffs</summary>
        public void ClearVisibleAuras()
        {
            for (int index = 0; index < this.m_visibleAuras.Length; ++index)
            {
                Aura visibleAura = this.m_visibleAuras[index];
                if (visibleAura != null)
                    visibleAura.Remove(true);
            }
        }

        /// <summary>
        /// Removes all auras, including passive auras -
        /// Don't use unless you understand the consequences.
        /// </summary>
        public void Clear()
        {
            foreach (Aura aura in this.m_AuraArray)
                aura.Remove(true);
        }

        /// <summary>
        /// Removes all auras, including passive auras, when owner is deleted.
        /// </summary>
        internal void ClearWithoutCleanup()
        {
            foreach (Aura aura in this.m_AuraArray)
                aura.RemoveWithoutCleanup();
        }

        /// <summary>
        /// TODO: Improve by having a container for recyclable ids
        /// </summary>
        /// <returns></returns>
        public byte GetFreePositiveIndex()
        {
            for (byte index = 0; (int) index < this.m_visibleAuras.Length - 16; ++index)
            {
                if (this.m_visibleAuras[(int) index] == null)
                    return index;
            }

            return byte.MaxValue;
        }

        public byte GetFreeNegativeIndex()
        {
            for (byte index = 48; (int) index < this.m_visibleAuras.Length; ++index)
            {
                if (this.m_visibleAuras[(int) index] == null)
                    return index;
            }

            return byte.MaxValue;
        }

        public byte GetFreeIndex(bool beneficial)
        {
            if (!beneficial)
                return this.GetFreeNegativeIndex();
            return this.GetFreePositiveIndex();
        }

        /// <summary>
        /// Always represents your curent ride (or null when not mounted)
        /// </summary>
        public Aura MountAura { get; internal set; }

        /// <summary>Represents the Aura that makes us a Ghost</summary>
        public Aura GhostAura { get; internal set; }

        /// <summary>Create a new Copy of</summary>
        private void InvalidateAurasCopy()
        {
            this.m_AuraArray = this.m_auras.Values.ToArray<Aura>();
        }

        /// <summary>Called when an Aura gets added or removed</summary>
        /// <param name="aura"></param>
        internal void OnAuraChange(Aura aura)
        {
            if (!aura.IsBeneficial || !aura.Spell.HasModifierEffects)
                return;
            this.ReApplyAffectedAuras(aura.Spell);
        }

        /// <summary>
        /// Reapplies all passive permanent Auras that are affected by the given Spell
        /// </summary>
        /// <param name="spell"></param>
        public void ReApplyAffectedAuras(Spell spell)
        {
            foreach (Aura aura in this.m_AuraArray)
            {
                if (aura.Spell.IsPassive && !aura.HasTimeout && (aura.Spell != spell && aura.Spell.IsAffectedBy(spell)))
                    aura.ReApplyNonPeriodicEffects();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReapplyAllAuras()
        {
            foreach (Aura aura in this.m_AuraArray)
                aura.ReApplyNonPeriodicEffects();
        }

        /// <summary>
        /// Returns the modified value (modified by certain talent bonusses) of the given type for the given spell (as int)
        /// </summary>
        public virtual int GetModifiedInt(SpellModifierType type, Spell spell, int value)
        {
            if (this.Owner.Master is Character)
                return ((Character) this.Owner.Master).PlayerAuras.GetModifiedInt(type, spell, value);
            return value;
        }

        /// <summary>
        /// Returns the given value minus bonuses through certain talents, of the given type for the given spell (as int)
        /// </summary>
        public virtual int GetModifiedIntNegative(SpellModifierType type, Spell spell, int value)
        {
            if (this.Owner.Master is Character)
                return ((Character) this.Owner.Master).PlayerAuras.GetModifiedIntNegative(type, spell, value);
            return value;
        }

        /// <summary>
        /// Returns the modified value (modified by certain talents) of the given type for the given spell (as float)
        /// </summary>
        public virtual float GetModifiedFloat(SpellModifierType type, Spell spell, float value)
        {
            if (this.Owner.Master is Character)
                return ((Character) this.Owner.Master).PlayerAuras.GetModifiedFloat(type, spell, value);
            return value;
        }

        public virtual void OnCasted(SpellCast cast)
        {
        }

        /// <summary>
        /// Returns whether there are any harmful Auras on the Unit.
        /// Unit cannot leave combat mode while under the influence of harmful Auras.
        /// </summary>
        /// <returns></returns>
        public bool HasHarmfulAura()
        {
            return this.FindFirst((Predicate<Aura>) (aura => !aura.IsBeneficial)) != null;
        }

        /// <summary>
        /// Returns whether the given spell was modified to be casted
        /// in any shapeshift form, (even if it usually requires a specific one).
        /// </summary>
        public bool IsShapeshiftRequirementIgnored(Spell spell)
        {
            foreach (Aura aura in this.m_AuraArray)
            {
                if (aura.Spell.SpellClassSet == spell.SpellClassSet)
                {
                    foreach (AuraEffectHandler handler in aura.Handlers)
                    {
                        if (handler.SpellEffect.AuraType == AuraType.IgnoreShapeshiftRequirement &&
                            handler.SpellEffect.MatchesSpell(spell))
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>Extra damage to be applied against a bleeding target</summary>
        public int GetBleedBonusPercent()
        {
            int num = 0;
            foreach (Aura aura in this.m_AuraArray)
            {
                foreach (AuraEffectHandler handler in aura.Handlers)
                {
                    if (handler.SpellEffect.AuraType == AuraType.IncreaseBleedEffectPct)
                        num += handler.SpellEffect.MiscValue;
                }
            }

            return num;
        }

        /// <summary>
        /// Returns the amount of visible Auras that are casted by the given caster
        /// and have the given DispelType.
        /// </summary>
        public int GetVisibleAuraCount(ObjectReference caster, DispelType type)
        {
            int num = 0;
            foreach (Aura visibleAura in this.m_visibleAuras)
            {
                if (visibleAura != null && visibleAura.CasterReference == caster &&
                    visibleAura.Spell.DispelType == type)
                    ++num;
            }

            return num;
        }

        /// <summary>
        /// Called after Character entered world to load all it's active Auras
        /// </summary>
        internal void InitializeAuras(AuraRecord[] records)
        {
            foreach (AuraRecord record in records)
            {
                byte freeIndex = this.GetFreeIndex(record.IsBeneficial);
                if (freeIndex == byte.MaxValue)
                {
                    record.DeleteLater();
                }
                else
                {
                    ObjectReference casterInfo = record.GetCasterInfo(this.m_owner.Map);
                    List<AuraEffectHandler> auraEffectHandlers =
                        record.Spell.CreateAuraEffectHandlers(casterInfo, this.m_owner, record.IsBeneficial);
                    if (auraEffectHandlers == null)
                    {
                        record.DeleteLater();
                    }
                    else
                    {
                        Aura aura = new Aura(this, casterInfo, record, auraEffectHandlers, freeIndex);
                        this.OnCreated(aura);
                        this.AddAura(aura, true);
                    }
                }
            }
        }

        /// <summary>Save all savable auras</summary>
        internal void SaveAurasNow()
        {
            foreach (Aura visibleAura in this.m_visibleAuras)
            {
                if (visibleAura != null && visibleAura.CanBeSaved)
                {
                    if (visibleAura.HasTimeout)
                    {
                        if (visibleAura.TimeLeft <= 5000)
                            continue;
                    }

                    try
                    {
                        visibleAura.SaveNow();
                    }
                    catch (StaleStateException ex)
                    {
                        LogUtil.WarnException((Exception) ex,
                            string.Format("failed to save aura, character {0} acc {1}[{2}]",
                                (object) this.OwnerChar.Name, (object) this.OwnerChar.Account.Name,
                                (object) this.OwnerChar.AccId), new object[0]);
                    }
                }
            }
        }

        /// <summary>Dumps all currently applied auras to the given chr</summary>
        /// <param name="receiver"></param>
        /// <param name="includePassive">Whether to also include invisible effects (eg through items etc)</param>
        public void DumpTo(IChatTarget receiver, bool includePassive)
        {
            if (this.m_auras.Count > 0)
            {
                receiver.SendMessage("{0}'s Auras:", (object) this.m_owner.Name);
                foreach (Aura aura in this.m_auras.Values)
                {
                    if (includePassive || !aura.Spell.IsPassive)
                        receiver.SendMessage("\t{0}{1}", (object) aura.Spell,
                            aura.HasTimeout
                                ? (object) (" [" + TimeSpan.FromMilliseconds((double) aura.TimeLeft).Format() + "]")
                                : (object) "");
                }
            }
            else
                receiver.SendMessage("{0} has no active Auras.", (object) this.m_owner.Name);
        }

        /// <summary>
        /// We need a second method because yield return and return statements cannot
        /// co-exist in one method.
        /// </summary>
        /// <returns></returns>
        private IEnumerator<Aura> _GetEnumerator()
        {
            for (int i = 0; i < this.m_AuraArray.Length; ++i)
                yield return this.m_AuraArray[i];
        }

        public IEnumerator<Aura> GetEnumerator()
        {
            if (this.m_auras.Count == 0)
                return Aura.EmptyEnumerator;
            return this._GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }

        /// <summary>
        /// Checks whether the given Aura may be activated (see PlayerAuraCollection)
        /// </summary>
        protected internal virtual bool MayActivate(Aura aura)
        {
            return true;
        }

        /// <summary>
        /// Checks whether the given Handler may be activated (see PlayerAuraCollection)
        /// </summary>
        protected internal virtual bool MayActivate(AuraEffectHandler handler)
        {
            return true;
        }
    }
}