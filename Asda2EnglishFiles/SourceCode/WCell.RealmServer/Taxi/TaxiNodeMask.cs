using NLog;
using System;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Taxi
{
    public class TaxiNodeMask
    {
        private static Logger sLog = LogManager.GetCurrentClassLogger();
        private uint[] fields;

        public uint[] Mask
        {
            get { return this.fields; }
            internal set { this.fields = value; }
        }

        public TaxiNodeMask()
        {
            this.fields = new uint[32];
        }

        public TaxiNodeMask(uint[] mask)
        {
            if (mask.Length < 32)
                Array.Resize<uint>(ref mask, 32);
            this.fields = mask;
        }

        public void Activate(PathNode node)
        {
            this.Activate(node.Id);
        }

        public void Activate(uint nodeId)
        {
            uint num = this.fields[nodeId / 32U] | 1U << (int) (nodeId % 32U);
            this.fields[nodeId / 32U] = num;
        }

        public bool IsActive(PathNode node)
        {
            if (node != null)
                return this.IsActive(node.Id);
            return false;
        }

        public bool IsActive(uint nodeId)
        {
            uint num1 = this.Mask[nodeId / 32U];
            uint num2 = 1U << (int) (nodeId % 32U);
            return ((int) num2 & (int) num1) == (int) num2;
        }
    }
}