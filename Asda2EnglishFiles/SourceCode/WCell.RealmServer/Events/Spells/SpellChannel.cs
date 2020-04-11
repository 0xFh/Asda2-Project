using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;
using WCell.Util.ObjectPools;

namespace WCell.RealmServer.Spells
{
    /// <summary>
    /// Represents a SpellChannel during a SpellCast (basically any Spell or Action that is being performed over time).
    /// </summary>
    public class SpellChannel : IUpdatable, ITickTimer
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static readonly ObjectPool<SpellChannel> SpellChannelPool =
            ObjectPoolMgr.CreatePool<SpellChannel>((Func<SpellChannel>) (() => new SpellChannel()));

        protected int m_duration;
        protected int m_amplitude;
        protected int m_until;
        protected int m_maxTicks;
        protected int m_ticks;
        private bool m_channeling;
        internal SpellCast m_cast;
        private List<SpellEffectHandler> m_channelHandlers;
        internal TimerEntry m_timer;
        private List<IAura> m_auras;

        /// <summary>Is called on every SpellChannel tick.</summary>
        public static event Action<SpellChannel> Ticked;

        /// <summary>Can only work with unit casters</summary>
        private SpellChannel()
        {
            this.m_timer = new TimerEntry(new Action<int>(this.Tick));
        }

        public SpellCast Cast
        {
            get { return this.m_cast; }
        }

        /// <summary>The total duration of this Channel</summary>
        public int Duration
        {
            get { return this.m_duration; }
        }

        /// <summary>Whether this SpellChannel is currently being used</summary>
        public bool IsChanneling
        {
            get { return this.m_channeling; }
        }

        /// <summary>The amount of milliseconds between 2 channel ticks</summary>
        public int Amplitude
        {
            get { return this.m_amplitude; }
        }

        public int Until
        {
            get { return this.m_until; }
            set
            {
                this.m_until = value;
                int num = this.TimeLeft;
                if (num < 0)
                    num = 1;
                this.m_ticks = MathUtil.CeilingInt((float) num / (float) this.m_amplitude);
                this.m_timer.Start(0, num % this.m_amplitude);
                SpellHandler.SendChannelUpdate(this.m_cast, (uint) num);
            }
        }

        public int Ticks
        {
            get { return this.m_ticks; }
        }

        public int MaxTicks
        {
            get { return this.m_maxTicks; }
        }

        /// <summary>The time in milliseconds until this channel closes</summary>
        public int TimeLeft
        {
            get { return this.m_until - Environment.TickCount; }
        }

        /// <summary>Reduces the channel by one tick</summary>
        public void Pushback(int millis)
        {
            if (!this.m_channeling || this.m_cast == null)
                return;
            this.Until += millis;
        }

        /// <summary>
        /// Opens this SpellChannel.
        /// Will be called by SpellCast class.
        /// Requires an active Caster.
        /// </summary>
        internal void Open(List<SpellEffectHandler> channelHandlers, List<IAura> auras)
        {
            if (!this.m_channeling && this.m_cast != null)
            {
                this.m_channeling = true;
                this.m_auras = auras;
                Spell spell = this.m_cast.Spell;
                Unit casterUnit = this.m_cast.CasterUnit;
                this.m_duration = spell.Durations.Max;
                this.m_amplitude = spell.ChannelAmplitude;
                if (this.m_amplitude < 1)
                    this.m_amplitude = this.m_duration;
                casterUnit.ChannelSpell = spell.SpellId;
                int tickCount = Environment.TickCount;
                this.m_ticks = 0;
                this.m_maxTicks = this.m_duration / this.m_amplitude;
                this.m_channelHandlers = channelHandlers;
                this.m_duration = spell.GetDuration(casterUnit.SharedReference);
                if (this.m_amplitude < 1)
                    this.m_amplitude = this.m_duration;
                this.m_until = tickCount + this.m_duration;
                SpellHandler.SendChannelStart(this.m_cast, spell.SpellId, this.m_duration);
                if (!this.m_channeling)
                    return;
                this.m_timer.Start(this.m_amplitude, this.m_amplitude);
            }
            else
                SpellChannel.log.Warn(this.ToString() + " was opened more than once or after disposal!");
        }

        /// <summary>Triggers a new tick</summary>
        protected void Tick(int timeElapsed)
        {
            ++this.m_ticks;
            SpellCast cast = this.m_cast;
            if (cast == null || !cast.IsCasting)
                return;
            Spell spell = cast.Spell;
            List<SpellEffectHandler> channelHandlers = this.m_channelHandlers;
            if (spell.PowerPerSecond > 0)
            {
                int amount = spell.PowerPerSecond;
                if (this.m_amplitude != 1000 && this.m_amplitude != 0)
                    amount = (int) ((double) amount * ((double) this.m_amplitude / 1000.0));
                SpellFailedReason reason = cast.ConsumePower(amount);
                if (reason != SpellFailedReason.Ok)
                {
                    this.m_cast.Cancel(reason);
                    return;
                }
            }

            foreach (SpellEffectHandler spellEffectHandler in channelHandlers)
            {
                spellEffectHandler.OnChannelTick();
                if (!this.m_channeling)
                    return;
            }

            if (this.m_auras != null)
                this.m_auras.RemoveAll((Predicate<IAura>) (aura =>
                {
                    if (!aura.IsAdded)
                        return true;
                    aura.Apply();
                    return !aura.IsAdded;
                }));
            if (!this.m_channeling)
                return;
            Action<SpellChannel> ticked = SpellChannel.Ticked;
            if (ticked != null)
                ticked(this);
            if (this.m_maxTicks > 0 && this.m_ticks < this.m_maxTicks)
                return;
            this.Close(false);
            if (!cast.IsCasting)
                return;
            cast.Cleanup();
        }

        public void OnRemove(Unit owner, Aura aura)
        {
            if (!this.m_channeling)
                return;
            if (this.m_cast.CasterUnit.ChannelObject == owner)
                this.m_cast.Cancel(SpellFailedReason.DontReport);
            else
                this.m_auras.Remove((IAura) aura);
        }

        public void Cancel()
        {
            this.m_cast.Cancel(SpellFailedReason.Interrupted);
        }

        /// <summary>
        /// Will be called internally to close this Channel.
        /// Call SpellCast.Cancel to cancel channeling.
        /// </summary>
        internal void Close(bool cancelled)
        {
            if (!this.m_channeling)
                return;
            this.m_channeling = false;
            Unit casterUnit = this.m_cast.CasterUnit;
            foreach (SpellEffectHandler channelHandler in this.m_channelHandlers)
                channelHandler.OnChannelClose(cancelled);
            List<IAura> auras = this.m_auras;
            if (auras != null)
            {
                foreach (IAura aura in auras)
                    aura.Remove(false);
                auras.Clear();
                SpellCast.AuraListPool.Recycle(auras);
                this.m_auras = (List<IAura>) null;
            }

            this.m_channelHandlers.Clear();
            SpellCast.SpellEffectHandlerListPool.Recycle(this.m_channelHandlers);
            this.m_channelHandlers = (List<SpellEffectHandler>) null;
            this.m_timer.Stop();
            if (cancelled)
                SpellHandler.SendChannelUpdate(this.m_cast, 0U);
            WorldObject channelObject = casterUnit.ChannelObject;
            if (channelObject is DynamicObject)
                channelObject.Delete();
            casterUnit.ChannelObject = (WorldObject) null;
            casterUnit.ChannelSpell = SpellId.None;
        }

        public void Update(int dt)
        {
            if (this.m_timer == null)
                SpellChannel.log.Warn("SpellChannel is updated after disposal: {0}", (object) this);
            else
                this.m_timer.Update(dt);
        }

        /// <summary>Get rid of circular references</summary>
        internal void Dispose()
        {
            this.m_cast = (SpellCast) null;
            SpellChannel.SpellChannelPool.Recycle(this);
        }

        public override string ToString()
        {
            if (this.m_cast == null)
                return "SpellChannel (Inactive)";
            return "SpellChannel (Caster: " + (object) this.m_cast.CasterObject + ", " + (object) this.m_cast.Spell +
                   ")";
        }
    }
}