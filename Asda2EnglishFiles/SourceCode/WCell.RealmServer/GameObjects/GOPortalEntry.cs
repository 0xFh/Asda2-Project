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
            this.Id = 337U;
            this.DisplayId = 4396U;
            this.UseHandler = new GOEntry.GOUseHandler(GOPortalEntry.OnUse);
            this.DefaultName = "Portal";
            this.FactionId = FactionTemplateId.Friendly;
            this.Type = GameObjectType.SpellCaster;
            this.GOCreator = (Func<GameObject>) (() => (GameObject) new Portal());
        }

        private static void Teleport(GameObject go, Character chr)
        {
            if (!go.Handler.CanBeUsedBy(chr))
                return;
            Portal portal = (Portal) go;
            chr.AddMessage((Action) (() => chr.TeleportTo(portal.Target)));
        }

        private static bool OnUse(GameObject go, Character chr)
        {
            Portal portal = (Portal) go;
            chr.AddMessage((Action) (() => chr.TeleportTo(portal.Target)));
            return true;
        }
    }
}