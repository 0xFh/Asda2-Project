using System;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.GOEntries;

namespace WCell.RealmServer.GameObjects
{
  /// <summary>Walk-in portal</summary>
  public class GOPortalEntry : GOCustomEntry
  {
    public static int PortalUpdateDelayMillis = 2000;
    public const GOEntryId PortalId = GOEntryId.Portal;

    public GOPortalEntry()
    {
      Id = 337U;
      DisplayId = 4396U;
      UseHandler = OnUse;
      DefaultName = "Portal";
      FactionId = FactionTemplateId.Friendly;
      Type = GameObjectType.SpellCaster;
      GOCreator = () => (GameObject) new Portal();
    }

    private static void Teleport(GameObject go, Character chr)
    {
      if(!go.Handler.CanBeUsedBy(chr))
        return;
      Portal portal = (Portal) go;
      chr.AddMessage(() => chr.TeleportTo(portal.Target));
    }

    private static bool OnUse(GameObject go, Character chr)
    {
      Portal portal = (Portal) go;
      chr.AddMessage(() => chr.TeleportTo(portal.Target));
      return true;
    }
  }
}