using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.DBC;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
    public class MapConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            MapTemplate val = new MapTemplate();
            val.Id = (MapId) DBCRecordConverter.GetUInt32(rawData, 0);
            int num1 = 2;
            MapTemplate mapTemplate1 = val;
            byte[] data1 = rawData;
            int field1 = num1;
            int num2 = field1 + 1;
            int uint32_1 = (int) DBCRecordConverter.GetUInt32(data1, field1);
            mapTemplate1.Type = (MapType) uint32_1;
            int num3 = num2 + 1;
            MapTemplate mapTemplate2 = val;
            byte[] data2 = rawData;
            int field2 = num3;
            int num4 = field2 + 1;
            int num5 = DBCRecordConverter.GetUInt32(data2, field2) != 0U ? 1 : 0;
            mapTemplate2.HasTwoSides = num5 != 0;
            MapTemplate mapTemplate3 = val;
            byte[] data3 = rawData;
            int stringOffset = num4;
            int num6 = stringOffset + 1;
            string str = this.GetString(data3, stringOffset);
            mapTemplate3.Name = str;
            int num7 = num6 + 16 + 1;
            MapTemplate mapTemplate4 = val;
            byte[] data4 = rawData;
            int field3 = num7;
            int num8 = field3 + 1;
            int uint32_2 = (int) DBCRecordConverter.GetUInt32(data4, field3);
            mapTemplate4.AreaTableId = (uint) uint32_2;
            val.ParentMapId = (MapId) DBCRecordConverter.GetUInt32(rawData, 59);
            val.RepopMapId = val.ParentMapId;
            val.RepopPosition = new Vector3(DBCRecordConverter.GetFloat(rawData, 60),
                DBCRecordConverter.GetFloat(rawData, 61), 500f);
            val.RequiredClientId = (ClientId) DBCRecordConverter.GetUInt32(rawData, 63);
            val.DefaultResetTime = DBCRecordConverter.GetInt32(rawData, 64);
            val.MaxPlayerCount = DBCRecordConverter.GetInt32(rawData, 65);
            ArrayUtil.Set<MapTemplate>(ref WCell.RealmServer.Global.World.s_MapTemplates, (uint) val.Id, val);
        }
    }
}