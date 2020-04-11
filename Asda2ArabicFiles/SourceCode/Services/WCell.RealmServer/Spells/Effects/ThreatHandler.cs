using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Generates Threat
	/// </summary>
	public class ThreatHandler : SpellEffectHandler
	{
		public ThreatHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason InitializeTarget(WorldObject target)
		{
			if (!(target is Unit))
			{
				return SpellFailedReason.DontReport;
			}
			return SpellFailedReason.Ok;
		}

		protected override void Apply(WorldObject target, ref DamageAction[] actions)
		{
			var npc = target as NPC;
		    var chr =  target as Character;

			var caster = m_cast.CasterUnit;
			if (caster != null)
			{
                if (Effect.Spell.Id == 206)
                {
                    foreach (var unit in caster.GetObjectsInRadius(25, ObjectTypes.Unit, false))
                    {
                        chr = unit as Character;
                        npc = unit as NPC;
                        if (chr != null && chr!=caster && chr.IsHostileWith(caster))
                        {
                            OnCharacterProvoked(chr, caster);
                        }
                        else if (npc != null)
                        {
                            npc.ThreatCollection[caster] +=
                                caster.GetGeneratedThreat((int)Math.Max(caster.RandomMagicDamage, caster.RandomDamage) * 50, Effect.Spell.Schools[0], Effect);
                        }
                    }
                }
                else
                {
                    if (chr != null)
                    {
                        OnCharacterProvoked(chr, caster);
                    }
                    else if (npc != null)
                    {
                        npc.ThreatCollection[caster] +=
                            caster.GetGeneratedThreat((int)Math.Max(caster.RandomMagicDamage, caster.RandomDamage) * 50, Effect.Spell.Schools[0], Effect);
                    }
                }
			}
	}	

	    private static void OnCharacterProvoked(Character chr, Unit caster)
	    {
	        chr.IsAggred = true;
	        chr.ArggredDateTime = DateTime.Now.AddMilliseconds(2500);
	        Asda2MovmentHandler.OnMoveRequest(chr.Client, caster.Asda2Y, caster.Asda2X);
	    }

	    public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}