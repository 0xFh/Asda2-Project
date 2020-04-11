using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Quests;

namespace WCell.RealmServer.Spells.Effects
{
    public class ClearQuestEffectHandler : SpellEffectHandler
    {
        public ClearQuestEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Player; }
        }

        public override void Apply()
        {
            uint miscValue = (uint) this.Effect.MiscValue;
            Character casterChar = this.Cast.CasterChar;
            if (casterChar == null)
                return;
            Quest activeQuest = casterChar.QuestLog.GetActiveQuest(miscValue);
            if (activeQuest == null)
                return;
            activeQuest.Cancel(false);
        }
    }
}