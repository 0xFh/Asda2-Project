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
            NPCId miscValue = (NPCId) this.Effect.MiscValue;
            this.entry = NPCMgr.GetEntry(miscValue);
            if (this.entry != null)
                return SpellFailedReason.Ok;
            LogManager.GetCurrentClassLogger()
                .Warn("The NPC for Summon-Spell {0} does not exist: {1} (Are NPCs loaded?)", (object) this.Effect.Spell,
                    (object) miscValue);
            return SpellFailedReason.Error;
        }

        public virtual SummonType SummonType
        {
            get { return (SummonType) this.Effect.MiscValueB; }
        }

        public override void Apply()
        {
            this.Summon(SpellHandler.GetSummonEntry(this.SummonType));
        }

        protected virtual void Summon(SpellSummonEntry summonEntry)
        {
            Vector3 targetLoc = (double) this.m_cast.TargetLoc.X == 0.0
                ? this.m_cast.CasterUnit.Position
                : this.m_cast.TargetLoc;
            int num1 = this.CalcEffectValue();
            int num2 = !summonEntry.DetermineAmountBySpellEffect ? 1 : (num1 > 0 ? num1 : 1);
            for (int index = 0; index < num2; ++index)
            {
                NPC npc = summonEntry.Handler.Summon(this.m_cast, ref targetLoc, this.entry);
                npc.CreationSpellId = this.Effect.Spell.SpellId;
                if (!summonEntry.DetermineAmountBySpellEffect && num1 > 1)
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