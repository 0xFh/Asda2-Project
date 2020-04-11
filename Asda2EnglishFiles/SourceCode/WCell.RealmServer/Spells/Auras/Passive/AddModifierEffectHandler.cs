namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class AddModifierEffectHandler : AuraEffectHandler
    {
        /// <summary>
        /// The amount of remaining charges or 0 if it doesn't need any
        /// </summary>
        public int Charges;
    }
}