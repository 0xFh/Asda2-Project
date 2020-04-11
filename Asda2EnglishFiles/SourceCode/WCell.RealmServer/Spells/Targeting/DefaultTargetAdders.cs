using NLog;
using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Groups;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells.Targeting
{
    /// <summary>
    /// Contains default spell targeting Adders.
    /// For more info, refer to: http://wiki.wcell.org/index.php?title=API:Spells#Targeting
    /// </summary>
    public static class DefaultTargetAdders
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static void AddSelf(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason)
        {
            WorldObject casterObject = targets.Cast.CasterObject;
            if (casterObject == null)
            {
                DefaultTargetAdders.log.Warn("Invalid SpellCast tried to target self, but no Caster given: {0}",
                    (object) targets.Cast);
                failReason = SpellFailedReason.Error;
            }
            else
            {
                if ((sbyte) (failReason = targets.ValidateTargetForHandlers(casterObject)) != (sbyte) -1)
                    return;
                targets.Add(casterObject);
            }
        }

        /// <summary>Your current summon or self</summary>
        public static void AddSummon(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason)
        {
            targets.AddSelf(filter, ref failReason);
        }

        public static void AddPet(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason)
        {
            WorldObject casterObject = targets.Cast.CasterObject;
            if (!(casterObject is Character))
            {
                DefaultTargetAdders.log.Warn("Non-Player {0} tried to cast Pet - spell {1}", (object) casterObject,
                    (object) targets.Cast.Spell);
                failReason = SpellFailedReason.TargetNotPlayer;
            }
            else
            {
                NPC activePet = ((Character) casterObject).ActivePet;
                if (activePet == null)
                    failReason = SpellFailedReason.NoPet;
                else
                    targets.Add((WorldObject) activePet);
            }
        }

        /// <summary>
        /// Adds the object or unit that has been chosen by a player when the spell was casted
        /// </summary>
        /// <param name="targets"></param>
        public static void AddSelection(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason)
        {
            targets.AddSelection(filter, ref failReason, false);
        }

        /// <summary>Adds the selected targets and chain-targets (if any)</summary>
        public static void AddSelection(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason, bool harmful)
        {
            SpellCast cast = targets.Cast;
            if (cast == null)
                return;
            Unit casterObject = cast.CasterObject as Unit;
            WorldObject target = cast.SelectedTarget;
            if (target == null)
            {
                if (casterObject == null)
                {
                    DefaultTargetAdders.log.Warn(
                        "Invalid SpellCast, tried to add Selection but nothing selected and no Caster present: {0}",
                        (object) targets.Cast);
                    failReason = SpellFailedReason.Error;
                    return;
                }

                target = (WorldObject) casterObject.Target;
                if (target == null)
                {
                    failReason = SpellFailedReason.BadTargets;
                    return;
                }
            }

            SpellEffect effect = targets.FirstHandler.Effect;
            Spell spell = effect.Spell;
            if (target != casterObject && casterObject != null)
            {
                if (!casterObject.IsInMaxRange(spell, target))
                    failReason = SpellFailedReason.OutOfRange;
                else if (casterObject.IsPlayer && !target.IsInFrontOf((WorldObject) casterObject))
                    failReason = SpellFailedReason.UnitNotInfront;
            }

            if (failReason != SpellFailedReason.Ok)
                return;
            failReason = targets.ValidateTarget(target, filter);
            if (failReason != SpellFailedReason.Ok)
                return;
            targets.Add(target);
            int limit = effect.ChainTargets;
            if (casterObject != null)
                limit = casterObject.Auras.GetModifiedInt(SpellModifierType.ChainTargets, spell, limit);
            if (limit <= 1 || !(target is Unit))
                return;
            targets.FindChain((Unit) target, filter, true, limit);
        }

        public static void AddChannelObject(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason)
        {
            Unit casterUnit = targets.Cast.CasterUnit;
            if (casterUnit == null)
                return;
            if (casterUnit.ChannelObject != null)
            {
                if ((sbyte) (failReason = targets.ValidateTarget(casterUnit.ChannelObject, filter)) != (sbyte) -1)
                    return;
                targets.Add(casterUnit.ChannelObject);
            }
            else
                failReason = SpellFailedReason.BadTargets;
        }

        /// <summary>Adds targets around the caster</summary>
        public static void AddAreaSource(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason)
        {
            targets.AddAreaSource(filter, ref failReason, targets.FirstHandler.GetRadius());
        }

        public static void AddAreaSource(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason, float radius)
        {
            targets.AddTargetsInArea(targets.Cast.SourceLoc, filter, radius);
        }

        /// <summary>Adds targets around the target area</summary>
        public static void AddAreaDest(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason)
        {
            targets.AddAreaDest(filter, ref failReason, targets.FirstHandler.GetRadius());
        }

        public static void AddAreaDest(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason, float radius)
        {
            targets.AddTargetsInArea(targets.Cast.TargetLoc, filter, radius);
        }

        public static void AddChain(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason)
        {
            targets.AddSelection(filter, ref failReason, false);
        }

        public static void AddTargetsInArea(this SpellTargetCollection targets, Vector3 pos, TargetFilter targetFilter,
            float radius)
        {
            SpellEffectHandler firstHandler = targets.FirstHandler;
            Spell spell = firstHandler.Effect.Spell;
            SpellCast cast = firstHandler.Cast;
            int limit = spell.MaxTargetEffect == null
                ? (!(targetFilter == new TargetFilter(DefaultTargetFilters.IsAllied)) ? (int) spell.MaxTargets : 40)
                : spell.MaxTargetEffect.CalcEffectValue(cast.CasterReference);
            if (limit < 1)
                limit = int.MaxValue;
            cast.Map.IterateObjects(pos, (double) radius > 0.0 ? radius : 5f, cast.Phase,
                (Func<WorldObject, bool>) (obj =>
                {
                    if (obj is Unit && targets.ValidateTarget(obj, targetFilter) == SpellFailedReason.Ok)
                        return targets.AddOrReplace(obj, limit);
                    return true;
                }));
        }

        public static void AddAllParty(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason)
        {
            SpellCast cast = targets.Cast;
            if (cast.CasterChar != null)
            {
                if (targets.Cast.CasterChar.Group != null)
                {
                    foreach (GroupMember groupMember in cast.CasterChar.Group)
                    {
                        Character character = groupMember.Character;
                        if (character != null)
                            targets.Add((WorldObject) character);
                    }
                }
                else
                    failReason = SpellFailedReason.TargetNotInParty;
            }
            else
            {
                float radius = targets.FirstHandler.GetRadius();
                if ((double) radius == 0.0)
                    radius = 30f;
                targets.AddAreaSource(
                    cast.Spell.HasHarmfulEffects
                        ? new TargetFilter(DefaultTargetFilters.IsFriendly)
                        : new TargetFilter(DefaultTargetFilters.IsHostile), ref failReason, radius);
            }
        }

        /// <summary>
        /// Used for Lock picking and opening (with or without keys)
        /// </summary>
        public static void AddItemOrObject(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason)
        {
            if (targets.Cast.TargetItem != null || targets.Cast.SelectedTarget is GameObject)
                return;
            failReason = SpellFailedReason.BadTargets;
        }

        public static void AddObject(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason)
        {
            if (!(targets.Cast.SelectedTarget is GameObject))
                failReason = SpellFailedReason.BadTargets;
            else
                targets.Add(targets.Cast.SelectedTarget);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void AddSecondHighestThreatTarget(this SpellTargetCollection targets, TargetFilter filter,
            ref SpellFailedReason failReason)
        {
            NPC casterUnit = targets.Cast.CasterUnit as NPC;
            if (casterUnit == null)
            {
                failReason = SpellFailedReason.NoValidTargets;
            }
            else
            {
                Unit aggressorByThreatRank = casterUnit.ThreatCollection.GetAggressorByThreatRank(2);
                if (aggressorByThreatRank != null)
                    targets.Add((WorldObject) aggressorByThreatRank);
                else
                    failReason = SpellFailedReason.NoValidTargets;
            }
        }
    }
}