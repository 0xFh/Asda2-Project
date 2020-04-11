using Castle.ActiveRecord;
using System;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Asda2_Items
{
    [Castle.ActiveRecord.ActiveRecord("FunctionItemBuff", Access = PropertyAccess.Property)]
    public class FunctionItemBuff : WCellRecord<FunctionItemBuff>
    {
        private static readonly NHIdGenerator _idGenerator =
            new NHIdGenerator(typeof(FunctionItemBuff), nameof(Guid), 1L);

        public Asda2ItemTemplate Template { get; set; }

        public Character Owner { get; set; }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }

        [Property] public uint OwnerId { get; set; }

        [Property] public int ItemId { get; set; }

        [Property] public byte Stacks { get; set; }

        [Property] public bool IsLongTime { get; set; }

        [Property] public long Duration { get; set; }

        [Property] public DateTime EndsDate { get; set; }

        public FunctionItemBuff()
        {
        }

        public FunctionItemBuff(int itemId, Character owner)
        {
            this.Guid = FunctionItemBuff._idGenerator.Next();
            this.ItemId = itemId;
            this.OwnerId = owner.EntityId.Low;
            this.Owner = owner;
            this.Template = Asda2ItemMgr.GetTemplate(itemId);
            this.Stacks = (byte) 1;
        }

        private void InitAfterLoad()
        {
            this.Template = Asda2ItemMgr.GetTemplate(this.ItemId);
            this.Owner = World.GetCharacter(this.OwnerId);
        }

        public static FunctionItemBuff[] LoadAll(Character chr)
        {
            FunctionItemBuff[] allByProperty =
                ActiveRecordBase<FunctionItemBuff>.FindAllByProperty("OwnerId", (object) chr.EntityId.Low);
            foreach (FunctionItemBuff functionItemBuff in allByProperty)
                functionItemBuff.InitAfterLoad();
            return allByProperty;
        }
    }
}