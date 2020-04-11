namespace WCell.RealmServer.Misc
{
    public interface IAttackEventHandler
    {
        /// <summary>
        /// Called before hit chance, damage etc is determined.
        /// This is not used for Spell attacks, since those only have a single "stage".
        /// NOT CURRENTLY IMPLEMENTED
        /// </summary>
        void OnBeforeAttack(DamageAction action);

        /// <summary>
        /// Called on the attacker, right before resistance is subtracted and final damage is evaluated
        /// </summary>
        void OnAttack(DamageAction action);

        /// <summary>
        /// Called on the defender, right before resistance is subtracted and final damage is evaluated
        /// </summary>
        void OnDefend(DamageAction action);
    }
}