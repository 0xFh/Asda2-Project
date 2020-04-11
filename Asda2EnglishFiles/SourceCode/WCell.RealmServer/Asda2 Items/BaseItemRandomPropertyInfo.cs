using WCell.Util.Data;

namespace WCell.RealmServer.Items
{
    public abstract class BaseItemRandomPropertyInfo : IDataHolder
    {
        public uint EnchantId;
        public uint PropertiesId;

        /// <summary>
        /// 
        /// </summary>
        public float ChancePercent;

        public abstract void FinalizeDataHolder();
    }
}