using System;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.NPCs;
using WCell.Util.Data;

namespace WCell.RealmServer.RacesClasses
{
    /// <summary>Defines the basics of a race.</summary>
    public class BaseRace
    {
        /// <summary>
        /// The <see cref="T:WCell.Constants.RaceId" /> that this race object represents.
        /// </summary>
        public RaceId Id;

        public string Name;

        /// <summary>The faction to which players of this Race belong</summary>
        public FactionTemplateId FactionTemplateId;

        /// <summary>The faction to which players of this Race belong</summary>
        [NotPersistent] public Faction Faction;

        /// <summary>
        /// The introduction movie (cinematic) for the given race.
        /// </summary>
        public uint IntroductionMovie;

        /// <summary>
        /// 
        /// </summary>
        public uint MaleDisplayId;

        /// <summary>
        /// 
        /// </summary>
        public uint FemaleDisplayId;

        /// <summary>
        /// 
        /// </summary>
        public UnitModelInfo MaleModel;

        /// <summary>
        /// 
        /// </summary>
        public UnitModelInfo FemaleModel;

        /// <summary>
        /// The scale that characters should have with their specific model.
        /// </summary>
        /// <remarks>If a model is normally "this big," then we adjust the Scale property to make
        /// the character's model appear bigger or smaller than normal, with 1f representing the
        /// normal size, reducing it or increasing it to make the character appear smaller or
        /// larger, respectively</remarks>
        public float Scale;

        public ClientId ClientId;

        public BaseRace()
        {
        }

        internal BaseRace(RaceId id)
        {
            this.Id = id;
        }

        public uint GetDisplayId(GenderType gender)
        {
            if (gender != GenderType.Female)
                return this.MaleDisplayId;
            return this.FemaleDisplayId;
        }

        public UnitModelInfo GetModel(GenderType gender)
        {
            if (gender != GenderType.Female)
                return this.MaleModel;
            return this.FemaleModel;
        }

        public void FinalizeAfterLoad()
        {
            this.FemaleModel = UnitMgr.GetModelInfo(this.FemaleDisplayId);
            if (this.FemaleModel == null)
                return;
            this.MaleModel = UnitMgr.GetModelInfo(this.MaleDisplayId);
            if (this.MaleModel == null)
                return;
            if ((double) this.FemaleModel.BoundingRadius < 0.1)
                this.FemaleModel.BoundingRadius = this.MaleModel.BoundingRadius;
            else if ((double) this.MaleModel.BoundingRadius < 0.1)
                this.MaleModel.BoundingRadius = this.FemaleModel.BoundingRadius;
            if ((double) this.FemaleModel.CombatReach < 0.1)
                this.FemaleModel.CombatReach = this.MaleModel.CombatReach;
            else if ((double) this.MaleModel.CombatReach < 0.1)
                this.MaleModel.CombatReach = this.FemaleModel.CombatReach;
            ArchetypeMgr.BaseRaces[(uint) this.Id] = this;
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}