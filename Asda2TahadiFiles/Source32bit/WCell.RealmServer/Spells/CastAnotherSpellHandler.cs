using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
  internal class CastAnotherSpellHandler : SpellEffectHandler
  {
    public CastAnotherSpellHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      if(Effect.MiscValue != 0)
        RunSpell((Unit) target, (uint) Effect.MiscValue);
      if(Effect.MiscValueB != 0)
        RunSpell((Unit) target, (uint) Effect.MiscValueB);
      if(Effect.MiscValueC == 0)
        return;
      RunSpell((Unit) target, (uint) Effect.MiscValueC);
    }

    private void RunSpell(Unit target, uint spellId)
    {
      if(Utility.Random(0, 101) > Cast.Spell.ProcChance)
        return;
      Spell spell = SpellHandler.Get(spellId);
      if(spell == null)
        return;
      Vector3 position = target.Position;
      SpellCast.Trigger(m_cast.CasterUnit, spell, ref position, target);
    }
  }
}