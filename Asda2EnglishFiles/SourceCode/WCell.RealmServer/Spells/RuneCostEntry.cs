using System;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells
{
    [Serializable]
    public class RuneCostEntry
    {
        public int[] CostPerType = new int[3];
        public uint Id;
        public int RunicPowerGain;
        public int RequiredRuneAmount;

        public int GetCost(RuneType type)
        {
            return this.CostPerType[(int) type];
        }

        public bool CostsRunes
        {
            get { return this.RequiredRuneAmount > 0; }
        }

        public override string ToString()
        {
            return string.Format("{0}", (object) this.Id);
        }
    }
}