using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2Fishing
{
    [Castle.ActiveRecord.ActiveRecord("Asda2FishingBook", Access = PropertyAccess.Property)]
    public class Asda2FishingBook : WCellRecord<Asda2FishingBook>
    {
        private static readonly NHIdGenerator _idGenerator =
            new NHIdGenerator(typeof(Asda2FishingBook), nameof(Guid), 1L);

        public FishingBookTemplate Template { get; set; }

        public Character Owner { get; set; }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }

        [Property] public uint OwnerId { get; set; }

        [Property] public int BookId { get; set; }

        [Property] public byte Num { get; set; }

        public int[] FishIds
        {
            get { return this.Template.RequiredFishes; }
        }

        [Persistent(Length = 30)] [Property] public short[] Amounts { get; set; }

        [Property] [Persistent(Length = 30)] public short[] MinLengths { get; set; }

        [Property] [Persistent(Length = 30)] public short[] MaxLength { get; set; }

        public bool IsComleted
        {
            get
            {
                for (int index = 0; index < 30; ++index)
                {
                    if (this.FishIds[index] != -1 &&
                        (int) this.Amounts[index] < this.Template.RequiredFishesAmounts[index])
                        return false;
                }

                return true;
            }
        }

        public void ResetBook()
        {
            for (int index = 0; index < this.Amounts.Length; ++index)
                this.Amounts[index] = (short) 0;
        }

        public void OnCatchFish(int fishId, short fishLen)
        {
            if (!this.Template.FishIndexes.ContainsKey(fishId))
                return;
            byte fishIndex = this.Template.FishIndexes[fishId];
            if ((int) this.Amounts[(int) fishIndex] < this.Template.RequiredFishesAmounts[(int) fishIndex])
                ++this.Amounts[(int) fishIndex];
            if (this.MinLengths[(int) fishIndex] == (short) 0 || (int) this.MinLengths[(int) fishIndex] > (int) fishLen)
                this.MinLengths[(int) fishIndex] = fishLen;
            if (this.MaxLength[(int) fishIndex] != (short) 0 && (int) this.MaxLength[(int) fishIndex] >= (int) fishLen)
                return;
            this.MaxLength[(int) fishIndex] = fishLen;
        }

        public Asda2FishingBook()
        {
        }

        public Asda2FishingBook(int bookId, Character owner, byte num)
        {
            this.Guid = Asda2FishingBook._idGenerator.Next();
            this.BookId = bookId;
            this.OwnerId = owner.EntityId.Low;
            this.Owner = owner;
            this.Template = Asda2FishingMgr.FishingBookTemplates[bookId];
            this.Num = num;
            this.MaxLength = new short[30];
            this.MinLengths = new short[30];
            this.Amounts = new short[30];
        }

        private void InitAfterLoad()
        {
            this.Template = Asda2FishingMgr.FishingBookTemplates[this.BookId];
            this.Owner = World.GetCharacter(this.OwnerId);
        }

        public static Asda2FishingBook[] LoadAll(Character chr)
        {
            Asda2FishingBook[] allByProperty =
                ActiveRecordBase<Asda2FishingBook>.FindAllByProperty("OwnerId", (object) chr.EntityId.Low);
            foreach (Asda2FishingBook asda2FishingBook in allByProperty)
                asda2FishingBook.InitAfterLoad();
            return allByProperty;
        }

        public void Complete()
        {
            for (int index = 0; index < 30; ++index)
                this.Amounts[index] = (short) this.Template.RequiredFishesAmounts[index];
        }
    }
}