using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
    public static class Asda2PortalMgr
    {
        static readonly List<GOSpawnPoolTemplate> EmptyList = new List<GOSpawnPoolTemplate>();
        public static Dictionary<MapId, List<GOSpawnPoolTemplate>> Portals = new Dictionary<MapId, List<GOSpawnPoolTemplate>>();

        public static List<GOSpawnPoolTemplate> GetSpawnPoolTemplatesByMap(MapId mapId)
        {
            return Portals.ContainsKey(mapId) ? Portals[mapId] : EmptyList;
        }
    }

    [DataHolder]
    public class Asda2Portal : IDataHolder
    {
        public int Id { get; set; }
        public short FromX { get; set; }
        public short FromY { get; set; }
        public byte FromMap { get; set; }
        public short ToX { get; set; }
        public short ToY { get; set; }
        public byte ToMap { get; set; }

        public void FinalizeDataHolder()
        {
            if(!Asda2PortalMgr.Portals.ContainsKey((MapId) FromMap))
                Asda2PortalMgr.Portals.Add((MapId)FromMap,new List<GOSpawnPoolTemplate>());
            var pos = new Vector3(FromX + 1000*FromMap, FromY+ 1000*FromMap);
            var goEntry = new GOPortalEntry {GOCreator = GoCreator,GOId = GOEntryId.Portal};
            var goTempl = new GOSpawnEntry(goEntry, GameObjectState.Enabled, (MapId)FromMap,
                                           ref pos, 0, 1, new float[] {0, 0, 0});
            var templ = new GOSpawnPoolTemplate(goTempl, 1);
            Asda2PortalMgr.Portals[(MapId)FromMap].Add(templ);
        }

        private GameObject GoCreator()
        {
            var portal =
                Portal.Create(
                    new WorldLocation((MapId) FromMap, new Vector3(FromX + 1000*FromMap, FromY + 1000*FromMap)),
                    new WorldLocation((MapId) ToMap, new Vector3(ToX + 1000*ToMap, ToY + 1000*ToMap)));
            var spell = new Spell(){Id = 50000,Range = new SimpleRange(0,10)};
            var effect = new SpellEffect(spell, 0)
                {
                    MiscValue = ToMap,
                    MiscValueB = ToX + 1000*ToMap,
                    MiscValueC = ToY + 1000*ToMap,
                    Radius = 3f,
                    EffectType = SpellEffectType.PortalTeleport,ImplicitTargetA = ImplicitSpellTargetType.AllAroundLocation
                };
            spell.Effects = new SpellEffect[1];
            spell.Duration = int.MaxValue;
            spell.IsAreaAura = true;
            /*spell.AuraAmplitude = 10000;
            spell.CastDelay = 10000;*/
            spell.Effects[0] = effect;
            spell.Initialize();
            spell.Init2();
            var aura = new AreaAura(portal, spell);
            aura.Start(null,true);
            return portal;
        }
    }
}
