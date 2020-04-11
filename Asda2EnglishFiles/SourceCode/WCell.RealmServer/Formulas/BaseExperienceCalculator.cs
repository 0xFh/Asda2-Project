namespace WCell.RealmServer.Formulas
{
    /// <summary>
    /// Takes Target-level and receiver-level and returns the amount of base-experience to be gained
    /// </summary>
    public delegate int BaseExperienceCalculator(int targetLvl, int receiverLvl);
}