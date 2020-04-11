using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WCell.Constants.Items;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Items
{
    [DataHolder]
    public class AvatarDisasembleRecord : IDataHolder
    {
        public int Id { get; set; }

        public int IsRegular { get; set; }

        public int Level { get; set; }

        [Persistent(Length = 10)] public int[] ItemIds { get; set; }

        [Persistent(Length = 10)] public int[] Chances { get; set; }

        public string ChancesAsString
        {
            get
            {
                return ((IEnumerable<int>) this.Chances).Aggregate<int, string>("",
                    (Func<string, int, string>) ((current, i) =>
                        current + i.ToString((IFormatProvider) CultureInfo.InvariantCulture) + ","));
            }
        }

        public void FinalizeDataHolder()
        {
            if (this.IsRegular == 0)
                Asda2ItemMgr.RegularAvatarRecords.SetValue((object) this, this.Id);
            else
                Asda2ItemMgr.PremiumAvatarRecords.SetValue((object) this, this.Id);
        }

        public Asda2ItemId GetRandomItemId()
        {
            int num1 = Utility.Random(0, 100000);
            int num2 = 0;
            for (int index = 0; index < this.ItemIds.Length; ++index)
            {
                num2 += this.Chances[index];
                if (num2 >= num1)
                    return (Asda2ItemId) this.ItemIds[index];
            }

            return Asda2ItemId.BoosterLv90CommonRune31175;
        }
    }
}