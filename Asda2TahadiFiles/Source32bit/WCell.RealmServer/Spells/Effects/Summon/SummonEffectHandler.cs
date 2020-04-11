using NLog;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells.Effects
{
  /// <summary>
  /// Summons a friendly companion, Pets, Guardians or Totems
  /// TODO: Handle Totems
  /// </summary>
  public class SummonEffectHandler : SpellEffectHandler
  {
    protected NPCEntry entry;

    public SummonEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override SpellFailedReason Initialize()
    {
      NPCId miscValue = (NPCId) Effect.MiscValue;
      entry = NPCMgr.GetEntry(miscValue);
      if(entry != null)
        return SpellFailedReason.Ok;
      LogManager.GetCurrentClassLogger()
        .Warn("The NPC for Summon-Spell {0} does not exist: {1} (Are NPCs loaded?)", Effect.Spell,
          miscValue);
      return SpellFailedReason.Error;
    }

    public virtual SummonType SummonType
    {
      get { return (SummonType) Effect.MiscValueB; }
    }

    public override void Apply()
    {
      Summon(SpellHandler.GetSummonEntry(SummonType));
    }

    protected virtual void Summon(SpellSummonEntry summonEntry)
    {
      Vector3 targetLoc = (double) m_cast.TargetLoc.X == 0.0
        ? m_cast.CasterUnit.Position
        : m_cast.TargetLoc;
      int num1 = CalcEffectValue();
      int num2 = !summonEntry.DetermineAmountBySpellEffect ? 1 : (num1 > 0 ? num1 : 1);
      for(int index = 0; index < num2; ++index)
      {
        NPC npc = summonEntry.Handler.Summon(m_cast, ref targetLoc, entry);
        npc.CreationSpellId = Effect.Spell.SpellId;
        if(!summonEntry.DetermineAmountBySpellEffect && num1 > 1)
          npc.Health = npc.BaseHealth = num1;
      }
    }

    public override ObjectTypes CasterType
    {
      get { return ObjectTypes.Unit; }
    }

    public override bool HasOwnTargets
    {
      get { return false; }
    }
  }
}