namespace WCell.RealmServer.Items.Enchanting
{
    /// <summary>
    /// See SpellItemEnchantmentCondition.dbc
    /// 
    /// TODO:
    /// </summary>
    public class ItemEnchantmentCondition
    {
        public uint Id;
        public uint[] LTOperandType;
        public uint[] LTOperand;
        public uint[] Operator;
        public uint[] RTOperandType;
        public uint[] RTOperand;
        public uint[] Logic;
    }
}