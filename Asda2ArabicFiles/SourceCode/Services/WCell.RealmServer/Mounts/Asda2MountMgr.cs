using System.Collections.Generic;
using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.Util.Data;
using WCell.Util.Variables;

namespace WCell.RealmServer.Mounts
{
    public static class Asda2MountMgr
    {
        [NotVariable]
        public static Dictionary<int, MountTemplate> TemplatesByItemIDs = new Dictionary<int, MountTemplate>();
        [NotVariable]
        public static Dictionary<int, MountTemplate> TemplatesById = new Dictionary<int, MountTemplate>();

        [Initialization(InitializationPass.Last, "Mount system")]
        public static void Init()
        {
            ContentMgr.Load<MountTemplate>();
        }
    }
    [ActiveRecord("Asda2MountRecord", Access = PropertyAccess.Property)]
    public class Asda2MountRecord : WCellRecord<Asda2MountRecord>
    {
        private static readonly NHIdGenerator IdGenerator = new NHIdGenerator(typeof(Asda2MountRecord), "Guid");
        [PrimaryKey]
        public int Guid { get; set; }
        public Asda2MountRecord(MountTemplate mount, Character owner)
        {
            Guid = (int)IdGenerator.Next();

            Id = mount.Id;
            OwnerId = owner.EntityId.Low;
        }
        [Property]
        protected uint OwnerId { get; set; }

        [Property]
        public int Id { get; set; }

        public Asda2MountRecord()
        {

        }
        public static Asda2MountRecord[] GetAllRecordsOfCharacter(uint charId)
        {
           return FindAllByProperty("OwnerId", charId);
        }
    }
    [DataHolder]
    public class MountTemplate:IDataHolder
    {
        public void FinalizeDataHolder()
        {
            if (!Asda2MountMgr.TemplatesByItemIDs.ContainsKey(ItemId))
                Asda2MountMgr.TemplatesByItemIDs.Add(ItemId, this);
            if (!Asda2MountMgr.TemplatesById.ContainsKey(Id))
                Asda2MountMgr.TemplatesById.Add(Id, this);
        }

        public int Id { get; set; }
        public int ItemId { get; set; }
        public int Unk { get; set; }
        public int Time { get; set; }
        public int Unk2 { get; set; }
        public string Name { get; set; }
        public string ImageName { get; set; }
        public int Unk1 { get; set; }
    }
}
