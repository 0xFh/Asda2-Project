using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Is used to feed the currently ActivePet of the Caster</summary>
    public class FeedPetEffectHandler : SpellEffectHandler
    {
        public FeedPetEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Player; }
        }

        public override SpellFailedReason Initialize()
        {
            NPC activePet = this.Cast.CasterChar.ActivePet;
            if (activePet == null)
                return SpellFailedReason.BadImplicitTargets;
            if (this.Cast.TargetItem == null)
                return SpellFailedReason.ItemNotFound;
            ItemTemplate template = this.Cast.TargetItem.Template;
            if (!activePet.CanEat(template.m_PetFood))
                return SpellFailedReason.WrongPetFood;
            long num = (long) activePet.Level - (long) template.Level;
            if (num > 35L)
                return SpellFailedReason.FoodLowlevel;
            return num < -15L ? SpellFailedReason.Highlevel : SpellFailedReason.Ok;
        }

        public override void Apply()
        {
            NPC activePet = this.Cast.CasterChar.ActivePet;
            ItemTemplate template = this.Cast.TargetItem.Template;
            if (activePet == null || template == null)
                return;
            this.Cast.Trigger(this.Effect.TriggerSpell, new WorldObject[1]
            {
                (WorldObject) activePet
            });
            Aura aura = activePet.Auras[this.Effect.TriggerSpellId];
            if (aura == null)
                return;
            AuraEffectHandler handler = aura.GetHandler(AuraType.PeriodicEnergize);
            if (handler == null)
                return;
            handler.BaseEffectValue = activePet.GetHappinessGain(template);
        }
    }
}