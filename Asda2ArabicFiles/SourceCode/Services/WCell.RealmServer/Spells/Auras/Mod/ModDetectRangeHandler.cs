using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;


namespace WCell.RealmServer.Spells.Auras.Mod
{
	public class ModDetectRangeHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
            // TODO: add check to Unit.GetAggroRangeSq
            if (SpellEffect.MiscValueB == 437)//from Kings rage
            {
                var targets = Owner.GetObjectsInRadius(8, ObjectTypes.Unit, false);
                foreach (var worldObject in targets)
                {
                    var unit = worldObject as Unit;
                    if (unit == null) continue;
                    if (!unit.IsHostileWith(Owner))
                        continue;
                   if (!unit.IsVisible)
                    {
                        unit.Auras.RemoveWhere(aura => aura.Spell.DispelType == DispelType.Stealth);
                        
                    }
                }
            }
            base.Apply();
        }
       

        protected override void Remove(bool cancelled)
		{

		}
	}
}
