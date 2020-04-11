using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2_Items
{
    [DataHolder]
    public class Asda2RecipeTemplate : IDataHolder
    {
        [Property] public string Name { get; set; }

        [Property] public int Id { get; set; }

        [Property] public int CraftingLevel { get; set; }

        [Property] [Persistent(6)] public int[] RequredItemIds { get; set; }

        [Property] [Persistent(6)] public short[] ReqiredItemAmounts { get; set; }

        [Persistent(7)] [Property] public int[] ResultItemIds { get; set; }

        [Property] [Persistent(7)] public short[] ResultItemAmounts { get; set; }

        public byte MaximumPosibleRarity { get; set; }

        public void FinalizeDataHolder()
        {
            if (this.Id == 0)
                return;
            ArrayUtil.SetValue((Array) Asda2CraftMgr.RecipeTemplates, this.Id, (object) this);
            this.MaximumPosibleRarity =
                (byte) ((IEnumerable<int>) this.ResultItemIds).Count<int>((Func<int, bool>) (i => i != -1));
        }
    }
}