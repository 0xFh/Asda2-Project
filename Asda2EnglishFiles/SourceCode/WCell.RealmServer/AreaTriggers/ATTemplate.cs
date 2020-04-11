using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.World;
using WCell.RealmServer.Content;
using WCell.RealmServer.Global;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AreaTriggers
{
    /// <summary>All information associated with AreaTriggers</summary>
    public class ATTemplate : IDataHolder
    {
        public uint Id;
        public string Name;
        public MapId TargetMap;
        public Vector3 TargetPos;
        public AreaTriggerType Type;
        public float TargetOrientation;
        public uint TargetScreen;

        /// <summary>Item, required for triggering</summary>
        public Asda2ItemId RequiredItemId;

        /// <summary>Item, required for triggering</summary>
        public Asda2ItemId RequiredItem2Id;

        /// <summary>Required heoric key</summary>
        public Asda2ItemId RequiredHeroicKeyId;

        /// <summary>Required heoric key</summary>
        public Asda2ItemId RequiredHeroicKey2Id;

        /// <summary>Required quest to be finished</summary>
        public uint RequiredQuestId;

        /// <summary>Quest to be triggered</summary>
        public uint TriggerQuestId;

        /// <summary>Unused</summary>
        public string RequiredFailedText;

        public uint RequiredLevel;
        [NotPersistent] public AreaTriggerHandler Handler;

        public uint GetId()
        {
            return this.Id;
        }

        public DataHolderState DataHolderState { get; set; }

        public void FinalizeDataHolder()
        {
            AreaTrigger areaTrigger = AreaTriggerMgr.AreaTriggers.Get<AreaTrigger>(this.Id);
            if (areaTrigger == null)
            {
                ContentMgr.OnInvalidDBData("AreaTriggerEntry {0} (#{1}, Type: {2}) had invalid AreaTrigger-id.",
                    (object) this.Name, (object) this.Id, (object) this.Type);
            }
            else
            {
                areaTrigger.Template = this;
                if (this.TargetPos.IsSet)
                {
                    MapTemplate mapTemplate = WCell.RealmServer.Global.World.GetMapTemplate(this.TargetMap);
                    if (mapTemplate != null)
                    {
                        this.Type = AreaTriggerType.Teleport;
                        int num = (int) ArrayUtil.AddOnlyOne<Vector3>(ref mapTemplate.EntrancePositions,
                            this.TargetPos);
                    }
                }

                this.Handler = AreaTriggerMgr.GetHandler(this.Type);
            }
        }

        public override string ToString()
        {
            return this.Name + string.Format(" (in {0}, Lvl {1})", (object) this.TargetMap,
                       (object) this.RequiredLevel);
        }

        public void Write(IndentTextWriter writer)
        {
            writer.WriteLine("Type: " + (object) this.Type);
            writer.WriteLineNotDefault<Asda2ItemId>(this.RequiredItemId,
                "RequiredItemId: " + (object) this.RequiredItemId);
            writer.WriteLineNotDefault<Asda2ItemId>(this.RequiredItem2Id,
                "RequiredItem2Id: " + (object) this.RequiredItem2Id);
            writer.WriteLineNotDefault<Asda2ItemId>(this.RequiredHeroicKeyId,
                "RequiredHeroicKeyId: " + (object) this.RequiredHeroicKeyId);
            writer.WriteLineNotDefault<Asda2ItemId>(this.RequiredHeroicKey2Id,
                "RequiredHeroicKey2Id: " + (object) this.RequiredHeroicKey2Id);
            writer.WriteLineNotDefault<uint>(this.RequiredQuestId, "RequiredQuestId: " + (object) this.RequiredQuestId);
            writer.WriteLineNotDefault<uint>(this.RequiredLevel, "RequiredLevel: " + (object) this.RequiredLevel);
            writer.WriteLineNotDefault<Asda2ItemId>(this.RequiredHeroicKeyId,
                "RequiredHeroicKeyId: " + (object) this.RequiredHeroicKeyId);
            writer.WriteLineNotDefault<Asda2ItemId>(this.RequiredHeroicKeyId,
                "RequiredHeroicKeyId: " + (object) this.RequiredHeroicKeyId);
        }
    }
}