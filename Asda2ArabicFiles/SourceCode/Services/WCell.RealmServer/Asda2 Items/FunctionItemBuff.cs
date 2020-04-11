using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.RealmServer.Asda2Fishing;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2_Items
{
    [ActiveRecord("FunctionItemBuff", Access = PropertyAccess.Property)]
    public class FunctionItemBuff : WCellRecord<FunctionItemBuff>
    {
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(FunctionItemBuff), "Guid");
        public Asda2ItemTemplate Template { get; set; }
        public Character Owner { get; set; }
        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }
        [Property]
        public uint OwnerId { get; set; }
        [Property]
        public int ItemId { get; set; }
         [Property]
        public byte Stacks { get; set; }
        [Property]
         public bool IsLongTime { get; set; }
         [Property]
         public long Duration { get; set; }
        [Property]
        public DateTime EndsDate { get; set; }

        public FunctionItemBuff()
        {
        }
        public FunctionItemBuff(int itemId, Character owner)
        {
            Guid = _idGenerator.Next();
            ItemId = itemId;
            OwnerId = owner.EntityId.Low;
            Owner = owner;
            Template =Asda2ItemMgr.GetTemplate(itemId);
            Stacks = 1;
        }
        void InitAfterLoad()
        {
            Template = Asda2ItemMgr.GetTemplate(ItemId);
            Owner = World.GetCharacter(OwnerId);
        }
        public static FunctionItemBuff[] LoadAll(Character chr)
        {
            var r = FindAllByProperty("OwnerId", chr.EntityId.Low);
            foreach (var asda2FishingBook in r)
            {
                asda2FishingBook.InitAfterLoad();
            }
            return r;
        }

    }
}
