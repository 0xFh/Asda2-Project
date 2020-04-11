using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core.DBC;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.RacesClasses
{
    public class DBCStartOutfitConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            uint uint32 = DBCRecordConverter.GetUInt32(rawData, 1);
            RaceId race = (RaceId) ((int) uint32 & (int) byte.MaxValue);
            ClassId clssId = (ClassId) ((uint32 & 65280U) >> 8);
            GenderType gender = (GenderType) ((uint32 & 16711680U) >> 16);
            Archetype archetype = ArchetypeMgr.GetArchetype(race, clssId);
            if (archetype == null)
                return;
            List<ItemStack> initialItems = archetype.GetInitialItems(gender);
            for (int field = 2; field <= 25; ++field)
            {
                int itemId = DBCRecordConverter.GetInt32(rawData, field);
                if (itemId > 0)
                {
                    ItemTemplate template = ItemMgr.GetTemplate((Asda2ItemId) itemId);
                    if (template == null)
                        LogManager.GetCurrentClassLogger()
                            .Warn("Missing initial Item in DB: " + (object) itemId + " (" + (object) (uint) itemId +
                                  ")");
                    else if (template.IsStackable)
                    {
                        int index = initialItems.FindIndex((Predicate<ItemStack>) (stack =>
                            (long) stack.Template.Id == (long) itemId));
                        if (index > -1)
                            initialItems[index] = new ItemStack()
                            {
                                Template = template,
                                Amount = initialItems[index].Amount + 1
                            };
                        else
                            initialItems.Add(new ItemStack()
                            {
                                Template = template,
                                Amount = template.IsAmmo ? template.MaxAmount : 1
                            });
                    }
                    else
                        initialItems.Add(new ItemStack()
                        {
                            Template = template,
                            Amount = 1
                        });
                }
            }
        }
    }
}