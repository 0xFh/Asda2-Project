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
    [NotPersistent]public Faction Faction;

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
      Id = id;
    }

    public uint GetDisplayId(GenderType gender)
    {
      if(gender != GenderType.Female)
        return MaleDisplayId;
      return FemaleDisplayId;
    }

    public UnitModelInfo GetModel(GenderType gender)
    {
      if(gender != GenderType.Female)
        return MaleModel;
      return FemaleModel;
    }

    public void FinalizeAfterLoad()
    {
      FemaleModel = UnitMgr.GetModelInfo(FemaleDisplayId);
      if(FemaleModel == null)
        return;
      MaleModel = UnitMgr.GetModelInfo(MaleDisplayId);
      if(MaleModel == null)
        return;
      if(FemaleModel.BoundingRadius < 0.1)
        FemaleModel.BoundingRadius = MaleModel.BoundingRadius;
      else if(MaleModel.BoundingRadius < 0.1)
        MaleModel.BoundingRadius = FemaleModel.BoundingRadius;
      if(FemaleModel.CombatReach < 0.1)
        FemaleModel.CombatReach = MaleModel.CombatReach;
      else if(MaleModel.CombatReach < 0.1)
        MaleModel.CombatReach = FemaleModel.CombatReach;
      ArchetypeMgr.BaseRaces[(uint) Id] = this;
    }

    public override string ToString()
    {
      return Id.ToString();
    }
  }
}