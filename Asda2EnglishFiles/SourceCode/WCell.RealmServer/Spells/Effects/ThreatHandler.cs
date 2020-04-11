using System;
using System.Collections.Generic;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Generates Threat</summary>
    public class ThreatHandler : SpellEffectHandler
    {
        public ThreatHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            return !(target is Unit) ? SpellFailedReason.DontReport : SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
            NPC npc1 = target as NPC;
            Character chr1 = target as Character;
            Unit casterUnit = this.m_cast.CasterUnit;
            if (casterUnit == null)
                return;
            if (this.Effect.Spell.Id == 206U)
            {
                foreach (WorldObject objectsInRadiu in (IEnumerable<WorldObject>) casterUnit.GetObjectsInRadius<Unit>(
                    25f, ObjectTypes.Unit, false, int.MaxValue))
                {
                    Character chr2 = objectsInRadiu as Character;
                    NPC npc2 = objectsInRadiu as NPC;
                    if (chr2 != null && chr2 != casterUnit && chr2.IsHostileWith((IFactionMember) casterUnit))
                        ThreatHandler.OnCharacterProvoked(chr2, casterUnit);
                    else if (npc2 != null)
                        npc2.ThreatCollection[casterUnit] += casterUnit.GetGeneratedThreat(
                            (int) Math.Max(casterUnit.RandomMagicDamage, casterUnit.RandomDamage) * 50,
                            this.Effect.Spell.Schools[0], this.Effect);
                }
            }
            else if (chr1 != null)
            {
                ThreatHandler.OnCharacterProvoked(chr1, casterUnit);
            }
            else
            {
                if (npc1 == null)
                    return;
                npc1.ThreatCollection[casterUnit] += casterUnit.GetGeneratedThreat(
                    (int) Math.Max(casterUnit.RandomMagicDamage, casterUnit.RandomDamage) * 50,
                    this.Effect.Spell.Schools[0], this.Effect);
            }
        }

        private static void OnCharacterProvoked(Character chr, Unit caster)
        {
            chr.IsAggred = true;
            chr.ArggredDateTime = DateTime.Now.AddMilliseconds(2500.0);
            Asda2MovmentHandler.OnMoveRequest(chr.Client, caster.Asda2Y, caster.Asda2X);
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}