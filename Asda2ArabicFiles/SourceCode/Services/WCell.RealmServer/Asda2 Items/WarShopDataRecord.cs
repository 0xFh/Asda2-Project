using System;
using WCell.RealmServer.Items;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2_Items
{
    [DataHolder]
    public class WarShopDataRecord : IDataHolder
    {
        public int Location { get; set; }//if = -1 spawn at all locations 0 - silaris; 1 - alpen; 2 - Flamio
        
        public int Id { get; set; }
  
        public int ItemId { get; set; }
       
        public int Amount { get; set; }
        
        public int Money1Type { get; set; }
      
        public int Money2Type { get; set; }
        
        public int Cost1 { get; set; }

        public int Cost2 { get; set; }//If tradeing for honor coins this value shows required rank

        public Int64 Page { get; set; }

        #region Implementation of IDataHolder

        public void FinalizeDataHolder()
        {
            Asda2ItemMgr.WarShopDataRecords[Id] = this;
        }

        #endregion
    }
    [DataHolder]
    public class RegularShopRecord : IDataHolder
    {
        public int Id { get; set; }
  
        public int ItemId { get; set; }
       
        public int NpcId { get; set; }

        #region Implementation of IDataHolder

        public void FinalizeDataHolder()
        {
            if(!Asda2ItemMgr.AvalibleRegularShopItems.ContainsKey(ItemId))
                Asda2ItemMgr.AvalibleRegularShopItems.Add(ItemId,this);
        }

        #endregion
    }
    
}