using System;
using System.Collections.Generic;
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
        Asda2PortalMgr.Portals.Add((MapId) FromMap, new List<GOSpawnPoolTemplate>());
      Vector3 pos = new Vector3(FromX + 1000 * FromMap,
        FromY + 1000 * FromMap);
      GOPortalEntry goPortalEntry = new GOPortalEntry();
      goPortalEntry.GOCreator = GoCreator;
      goPortalEntry.GOId = GOEntryId.Portal;
      GOSpawnPoolTemplate spawnPoolTemplate = new GOSpawnPoolTemplate(
        new GOSpawnEntry(goPortalEntry, GameObjectState.Enabled, (MapId) FromMap, ref pos, 0.0f,
          1f, new float[3], 600), 1);
      Asda2PortalMgr.Portals[(MapId) FromMap].Add(spawnPoolTemplate);
    }

    private GameObject GoCreator()
    {
      Portal portal = Portal.Create(
        new WorldLocation((MapId) FromMap,
          new Vector3(FromX + 1000 * FromMap,
            FromY + 1000 * FromMap), 1U),
        new WorldLocation((MapId) ToMap,
          new Vector3(ToX + 1000 * ToMap,
            ToY + 1000 * ToMap), 1U));
      Spell spell = new Spell
      {
        Id = 50000,
        Range = new SimpleRange(0.0f, 10f)
      };
      SpellEffect spellEffect = new SpellEffect(spell, EffectIndex.Zero)
      {
        MiscValue = ToMap,
        MiscValueB = ToX + 1000 * ToMap,
        MiscValueC = ToY + 1000 * ToMap,
        Radius = 3f,
        EffectType = SpellEffectType.PortalTeleport,
        ImplicitTargetA = ImplicitSpellTargetType.AllAroundLocation
      };
      spell.Effects = new SpellEffect[1];
      spell.Duration = int.MaxValue;
      spell.IsAreaAura = true;
      spell.Effects[0] = spellEffect;
      spell.Initialize();
      spell.Init2();
      new AreaAura(portal, spell).Start(null, true);
      return portal;
    }
  }
}