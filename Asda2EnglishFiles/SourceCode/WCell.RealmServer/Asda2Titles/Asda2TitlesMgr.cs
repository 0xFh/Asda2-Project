using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using WCell.Core.Initialization;
using WCell.Core.Timers;
using WCell.RealmServer.Database;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Asda2Titles
{
    internal class Asda2TitlesMgr : IUpdatable
    {
        [WCell.Core.Initialization.Initialization(InitializationPass.Tenth, "Asda2 title system.")]
        public static void InitTitles()
        {
            Asda2TitlesMgr.TopRating();
            World.TaskQueue.RegisterUpdatableLater((IUpdatable) new Asda2TitlesMgr());
        }

        public static void TopRating()
        {
            List<CharacterRecord> characterRecordList =
                new List<CharacterRecord>((IEnumerable<CharacterRecord>) ActiveRecordBase<CharacterRecord>.FindAll());
            characterRecordList.Sort((Comparison<CharacterRecord>) ((a, b) => b.TitlePoints.CompareTo(a.TitlePoints)));
            for (int index = 0; index < characterRecordList.Count; ++index)
            {
                characterRecordList[index].Rank = index + 1;
                characterRecordList[index].SaveAndFlush();
            }
        }

        public void Update(int dt)
        {
            if (DateTime.Now.Hour != 3)
                return;
            Asda2TitlesMgr.TopRating();
        }
    }
}