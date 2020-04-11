using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using WCell.Constants;
using WCell.RealmServer.Instances;
using WCell.Util;

namespace WCell.RealmServer.Battlegrounds
{
    [XmlRoot("Battlegrounds")]
    public class BattlegroundConfig : InstanceConfigBase<BattlegroundConfig, BattlegroundId>
    {
        public static BattlegroundConfig Instance;

        public override IEnumerable<BattlegroundId> SortedIds
        {
            get
            {
                return (IEnumerable<BattlegroundId>) ((IEnumerable<BattlegroundTemplate>) BattlegroundMgr.Templates)
                    .Where<BattlegroundTemplate>((Func<BattlegroundTemplate, bool>) (info =>
                    {
                        if (info != null && info.Id > BattlegroundId.None && info.Id < BattlegroundId.End)
                            return info.Id != BattlegroundId.AllArenas;
                        return false;
                    })).TransformList<BattlegroundTemplate, BattlegroundId>(
                        (Func<BattlegroundTemplate, BattlegroundId>) (info => info.Id));
            }
        }

        public static void LoadSettings()
        {
            BattlegroundConfig.Instance =
                InstanceConfigBase<BattlegroundConfig, BattlegroundId>.LoadSettings("Battlegrounds.xml");
        }

        protected override void InitSetting(InstanceConfigEntry<BattlegroundId> configEntry)
        {
            BattlegroundMgr.SetCreator(configEntry.Name, configEntry.TypeName.Trim());
        }
    }
}