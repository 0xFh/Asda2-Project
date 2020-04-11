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
      ObjectPoolMgr.CreatePool(() => new SpellChannel());

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
      m_timer = new TimerEntry(Tick);
    }

    public SpellCast Cast
    {
      get { return m_cast; }
    }

    /// <summary>The total duration of this Channel</summary>
    public int Duration
    {
      get { return m_duration; }
    }

    /// <summary>Whether this SpellChannel is currently being used</summary>
    public bool IsChanneling
    {
      get { return m_channeling; }
    }

    /// <summary>The amount of milliseconds between 2 channel ticks</summary>
    public int Amplitude
    {
      get { return m_amplitude; }
    }

    public int Until
    {
      get { return m_until; }
      set
      {
        m_until = value;
        int num = TimeLeft;
        if(num < 0)
          num = 1;
        m_ticks = MathUtil.CeilingInt(num / (float) m_amplitude);
        m_timer.Start(0, num % m_amplitude);
        SpellHandler.SendChannelUpdate(m_cast, (uint) num);
      }
    }

    public int Ticks
    {
      get { return m_ticks; }
    }

    public int MaxTicks
    {
      get { return m_maxTicks; }
    }

    /// <summary>The time in milliseconds until this channel closes</summary>
    public int TimeLeft
    {
      get { return m_until - Environment.TickCount; }
    }

    /// <summary>Reduces the channel by one tick</summary>
    public void Pushback(int millis)
    {
      if(!m_channeling || m_cast == null)
        return;
      Until += millis;
    }

    /// <summary>
    /// Opens this SpellChannel.
    /// Will be called by SpellCast class.
    /// Requires an active Caster.
    /// </summary>
    internal void Open(List<SpellEffectHandler> channelHandlers, List<IAura> auras)
    {
      if(!m_channeling && m_cast != null)
      {
        m_channeling = true;
        m_auras = auras;
        Spell spell = m_cast.Spell;
        Unit casterUnit = m_cast.CasterUnit;
        m_duration = spell.Durations.Max;
        m_amplitude = spell.ChannelAmplitude;
        if(m_amplitude < 1)
          m_amplitude = m_duration;
        casterUnit.ChannelSpell = spell.SpellId;
        int tickCount = Environment.TickCount;
        m_ticks = 0;
        m_maxTicks = m_duration / m_amplitude;
        m_channelHandlers = channelHandlers;
        m_duration = spell.GetDuration(casterUnit.SharedReference);
        if(m_amplitude < 1)
          m_amplitude = m_duration;
        m_until = tickCount + m_duration;
        SpellHandler.SendChannelStart(m_cast, spell.SpellId, m_duration);
        if(!m_channeling)
          return;
        m_timer.Start(m_amplitude, m_amplitude);
      }
      else
        log.Warn(ToString() + " was opened more than once or after disposal!");
    }

    /// <summary>Triggers a new tick</summary>
    protected void Tick(int timeElapsed)
    {
      ++m_ticks;
      SpellCast cast = m_cast;
      if(cast == null || !cast.IsCasting)
        return;
      Spell spell = cast.Spell;
      List<SpellEffectHandler> channelHandlers = m_channelHandlers;
      if(spell.PowerPerSecond > 0)
      {
        int amount = spell.PowerPerSecond;
        if(m_amplitude != 1000 && m_amplitude != 0)
          amount = (int) (amount * (m_amplitude / 1000.0));
        SpellFailedReason reason = cast.ConsumePower(amount);
        if(reason != SpellFailedReason.Ok)
        {
          m_cast.Cancel(reason);
          return;
        }
      }

      foreach(SpellEffectHandler spellEffectHandler in channelHandlers)
      {
        spellEffectHandler.OnChannelTick();
        if(!m_channeling)
          return;
      }

      if(m_auras != null)
        m_auras.RemoveAll(aura =>
        {
          if(!aura.IsAdded)
            return true;
          aura.Apply();
          return !aura.IsAdded;
        });
      if(!m_channeling)
        return;
      Action<SpellChannel> ticked = Ticked;
      if(ticked != null)
        ticked(this);
      if(m_maxTicks > 0 && m_ticks < m_maxTicks)
        return;
      Close(false);
      if(!cast.IsCasting)
        return;
      cast.Cleanup();
    }

    public void OnRemove(Unit owner, Aura aura)
    {
      if(!m_channeling)
        return;
      if(m_cast.CasterUnit.ChannelObject == owner)
        m_cast.Cancel(SpellFailedReason.DontReport);
      else
        m_auras.Remove(aura);
    }

    public void Cancel()
    {
      m_cast.Cancel(SpellFailedReason.Interrupted);
    }

    /// <summary>
    /// Will be called internally to close this Channel.
    /// Call SpellCast.Cancel to cancel channeling.
    /// </summary>
    internal void Close(bool cancelled)
    {
      if(!m_channeling)
        return;
      m_channeling = false;
      Unit casterUnit = m_cast.CasterUnit;
      foreach(SpellEffectHandler channelHandler in m_channelHandlers)
        channelHandler.OnChannelClose(cancelled);
      List<IAura> auras = m_auras;
      if(auras != null)
      {
        foreach(IAura aura in auras)
          aura.Remove(false);
        auras.Clear();
        SpellCast.AuraListPool.Recycle(auras);
        m_auras = null;
      }

      m_channelHandlers.Clear();
      SpellCast.SpellEffectHandlerListPool.Recycle(m_channelHandlers);
      m_channelHandlers = null;
      m_timer.Stop();
      if(cancelled)
        SpellHandler.SendChannelUpdate(m_cast, 0U);
      WorldObject channelObject = casterUnit.ChannelObject;
      if(channelObject is DynamicObject)
        channelObject.Delete();
      casterUnit.ChannelObject = null;
      casterUnit.ChannelSpell = SpellId.None;
    }

    public void Update(int dt)
    {
      if(m_timer == null)
        log.Warn("SpellChannel is updated after disposal: {0}", this);
      else
        m_timer.Update(dt);
    }

    /// <summary>Get rid of circular references</summary>
    internal void Dispose()
    {
      m_cast = null;
      SpellChannelPool.Recycle(this);
    }

    public override string ToString()
    {
      if(m_cast == null)
        return "SpellChannel (Inactive)";
      return "SpellChannel (Caster: " + m_cast.CasterObject + ", " + m_cast.Spell +
             ")";
    }
  }
}