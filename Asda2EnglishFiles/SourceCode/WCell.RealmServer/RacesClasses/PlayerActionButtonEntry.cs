using System.Collections.Generic;
using WCell.Constants;
using WCell.RealmServer.Content;
using WCell.RealmServer.Misc;
using WCell.Util.Data;

namespace WCell.RealmServer.RacesClasses
{
    /// <summary>
    /// 
    /// </summary>
    public class PlayerActionButtonEntry : IDataHolder
    {
        public RaceId Race;
        public ClassId Class;
        public uint Index;
        public ushort Action;
        public byte Type;
        public byte Info;

        public void FinalizeDataHolder()
        {
            List<Archetype> archetypes = ArchetypeMgr.GetArchetypes(this.Race, this.Class);
            if (archetypes == null)
            {
                ContentMgr.OnInvalidDBData(this.GetType().Name + " \"{0}\" refers to invalid Archetype: {1} {2}.",
                    (object) this, (object) this.Race, (object) this.Class);
            }
            else
            {
                foreach (Archetype archetype in archetypes)
                    ActionButton.Set(archetype.ActionButtons, this.Index, this.Action, this.Type, this.Info);
            }
        }

        public override string ToString()
        {
            return string.Format("Action {0} (Index: {1})", (object) this.Action, (object) this.Index);
        }
    }
}