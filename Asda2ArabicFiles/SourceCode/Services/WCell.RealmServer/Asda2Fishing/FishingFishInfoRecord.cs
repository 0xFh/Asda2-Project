using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using WCell.Constants.World;
using WCell.RealmServer.Items;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Asda2Fishing
{
    [DataHolder]
    public class FishingFishInfoRecord : IDataHolder
    {
        public int Id { get; set; }
        public int FishId { get; set; }
        public int FishingTime { get; set; }
        [Property]
        [Persistent (Length = 6)]
        public int[] BaitIds { get; set; }

        #region Implementation of IDataHolder

        public void FinalizeDataHolder()
        {
             Asda2FishingMgr.FishRecords.Add(FishId,this);
        }

        #endregion
    }
     [DataHolder]
    public class FishingBaseInfoRecord : IDataHolder
    {
        public int Id { get; set; }
         [Persistent(Length = 20)]
        public int[] ItemIds { get; set; }
        [Persistent(Length = 20)]
        public int[] MaxFishLenghts { get; set; }
         [Persistent(Length = 20)]
        public int[] MinFishLengths { get; set; }
         [Persistent(Length = 20)]
        public int[] Chances { get; set; }

         public int Key { get; set; }
         public int IsPremium { get; set; }
        #region Implementation of IDataHolder

        public void FinalizeDataHolder()
        {
            if(IsPremium == 1)
                Asda2FishingMgr.PremiumFishingBaseInfos.Add(Key,this);
            else
                Asda2FishingMgr.FishingBaseInfos.Add(Key, this);
        }

        #endregion
    }
     [DataHolder]
     public class FishingSpotInfoRecord : IDataHolder
     {
         public int Id { get; set; }
         [Persistent(Length = 10)]
         public int[] RequiredFishingLvls { get; set; }
         [Persistent(Length = 20)]
         public int[] Points { get; set; }
         [Persistent(Length = 10)]
         public int[] Radius { get; set; }
         [Persistent(Length = 10)]
         public int[] DataKey { get; set; }
         #region Implementation of IDataHolder

         public void FinalizeDataHolder()
         {
             var list = new List<FishingSpot>();
             Asda2FishingMgr.FishingSpotsByMaps.Add(Id,list);
             for (int i = 0; i < 10; i++)
             {
                 if(RequiredFishingLvls[i]==-1)
                     continue;
                 var nfs = new FishingSpot();
                 nfs.RequiredFishingLevel = RequiredFishingLvls[i];
                 nfs.Map = (MapId) Id;
                 nfs.Position = new Vector3(Points[i],Points[i+10],0);
                 nfs.Radius = (byte) Radius[i];
                 nfs.Fishes = new Dictionary<int, Fish>();
                 var bi = Asda2FishingMgr.FishingBaseInfos[DataKey[i]];
                 var chance = 0;
                 for (int j = 0; j < 20; j++)
                 {
                     if(bi.Chances[j] ==0)
                         continue;
                     chance += bi.Chances[j];
                     var nf = new Fish();
                     nf.ItemTemplate = Asda2ItemMgr.GetTemplate(bi.ItemIds[j]) ?? Asda2ItemMgr.GetTemplate(31725);
                     nf.BaitIds = new List<int>();
                     var ft = Asda2FishingMgr.FishRecords[bi.ItemIds[j]];
                     nf.FishingTime = ft.FishingTime;
                     for (int k = 0; k < 6; k++)
                     {
                         if(ft.BaitIds[k]==-1)
                             continue;
                         nf.BaitIds.Add(ft.BaitIds[k]);
                     }
                     nf.MinLength = (byte) bi.MinFishLengths[j];
                     nf.MaxLength = (byte)bi.MaxFishLenghts[j];
                     nfs.Fishes.Add(chance,nf);
                 }

                 nfs.PremiumFishes = new Dictionary<int, Fish>();
                  bi = Asda2FishingMgr.PremiumFishingBaseInfos[DataKey[i]];
                  chance = 0;
                 for (int j = 0; j < 20; j++)
                 {
                     if (bi.Chances[j] == 0)
                         continue;
                     chance += bi.Chances[j];
                     var nf = new Fish();
                     nf.ItemTemplate = Asda2ItemMgr.GetTemplate(bi.ItemIds[j]) ?? Asda2ItemMgr.GetTemplate(31725);
                     nf.BaitIds = new List<int>();
                     var ft = Asda2FishingMgr.FishRecords[bi.ItemIds[j]];
                     nf.FishingTime = ft.FishingTime;
                     for (int k = 0; k < 6; k++)
                     {
                         if (ft.BaitIds[k] == -1)
                             continue;
                         nf.BaitIds.Add(ft.BaitIds[k]);
                     }
                     nf.MinLength = (byte)bi.MinFishLengths[j];
                     nf.MaxLength = (byte)bi.MaxFishLenghts[j];
                     nfs.PremiumFishes.Add(chance, nf);
                 }
                 list.Add(nfs);
             }
         }

         #endregion
     }
     [DataHolder]
     public class FishingBookTemplate : IDataHolder
     {
         public Dictionary<int,byte> FishIndexes = new Dictionary<int, byte>();
        public int Id { get; set; }
        public int BookId { get; set; }
         [Persistent (Length = 30)]
        public int[] RequiredFishes { get; set; }
          [Persistent(Length = 30)]
        public int[] RequiredFishesAmounts { get; set; }
          [Persistent(Length = 4)]
        public int[] Rewards { get; set; }
          [Persistent(Length = 4)]
        public int[] RewardAmounts { get; set; }
         #region Implementation of IDataHolder

         public void FinalizeDataHolder()
         {
             if (Asda2FishingMgr.FishingBookTemplates.ContainsKey(BookId))
                 return;
             Asda2FishingMgr.FishingBookTemplates.Add(BookId,this);
             for (byte i = 0; i < RequiredFishes.Length; i++)
             {
                 if(RequiredFishes[i]!=-1)
                     FishIndexes.Add(RequiredFishes[i],i);
             }
         }

         #endregion
     }
}
