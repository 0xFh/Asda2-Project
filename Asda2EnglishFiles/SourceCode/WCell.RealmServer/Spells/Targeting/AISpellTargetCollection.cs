namespace WCell.RealmServer.Spells.Targeting
{
    public class AISpellTargetCollection : SpellTargetCollection
    {
        public static AISpellTargetCollection ObtainAICollection()
        {
            return new AISpellTargetCollection();
        }
    }
}