using WCell.Constants;
using WCell.Util.Data;

namespace WCell.RealmServer.RacesClasses
{
    public class LevelStatInfo : IDataHolder
    {
        [NotPersistent] public int[] Stats = new int[6];
        public RaceId Race;
        public ClassId Class;
        public int Level;

        public int Strength
        {
            get { return this.Stats[0]; }
            set { this.Stats[0] = value; }
        }

        public int Agility
        {
            get { return this.Stats[1]; }
            set { this.Stats[1] = value; }
        }

        public int Stamina
        {
            get { return this.Stats[2]; }
            set { this.Stats[2] = value; }
        }

        public int Intellect
        {
            get { return this.Stats[3]; }
            set { this.Stats[3] = value; }
        }

        public int Spirit
        {
            get { return this.Stats[4]; }
            set { this.Stats[4] = value; }
        }

        public void FinalizeDataHolder()
        {
            int num = this.Level > 0 ? this.Level : 1;
            if (num > RealmServerConfiguration.MaxCharacterLevel)
                return;
            Archetype archetype = ArchetypeMgr.GetArchetype(this.Race, this.Class);
            if (archetype == null)
                return;
            if (this.Level == 1)
                archetype.FirstLevelStats = this;
            archetype.LevelStats[num - 1] = this;
        }
    }
}