using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.GOEntries
{
  /// <summary>
  /// Can be used to create custom GameObjects that will apply the given
  /// Spell to everyone in Radius.
  /// </summary>
  public class GOCustomAreaEffectEntry : GOCustomEntry
  {
    private GOInteractionHandler m_AreaEffectHandler;
    protected float m_Radius;
    protected float m_RadiusSq;

    public GOCustomAreaEffectEntry()
    {
      Radius = 5f;
      UpdateDelayMillis = 500;
    }

    public float Radius
    {
      get { return m_Radius; }
      set
      {
        m_Radius = value;
        m_RadiusSq = m_Radius * m_Radius;
      }
    }

    public int UpdateDelayMillis { get; set; }

    /// <summary>
    /// The EffectHandler that will be applied to every Unit that comes into the Radius.
    /// When moving, removing or adding anything in this Method, enqueue a Message!
    /// </summary>
    public GOInteractionHandler AreaEffectHandler
    {
      get { return m_AreaEffectHandler; }
      set { m_AreaEffectHandler = value; }
    }

    protected internal override void InitGO(GameObject go)
    {
      go.SetUpdatePriority(UpdatePriority.VeryLowPriority);
      if(m_AreaEffectHandler == null)
        return;
      go.CallPeriodically(UpdateDelayMillis, ApplyEffectsToArea);
    }

    protected void ApplyEffectsToArea(WorldObject goObj)
    {
      GameObject go = (GameObject) goObj;
      goObj.IterateEnvironment(Radius, obj =>
      {
        if(obj is Character)
          AreaEffectHandler(go, (Character) obj);
        return true;
      });
    }

    public delegate void GOInteractionHandler(GameObject go, Character chr);
  }
}