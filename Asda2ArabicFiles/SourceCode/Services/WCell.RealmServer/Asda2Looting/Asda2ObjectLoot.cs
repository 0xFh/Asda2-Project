using System;
using NLog;
using WCell.Constants.Factions;
using WCell.Constants.Looting;
using WCell.Constants.Updates;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Factions;
using WCell.RealmServer.UpdateFields;

namespace WCell.RealmServer.Looting
{
	/// <summary>
	/// TODO: Implement seperated loot for everyone when looting Quest-objects
	/// </summary>
	public class Asda2ObjectLoot : Asda2Loot
	{
		private static Logger log = LogManager.GetCurrentClassLogger();
		internal Action OnLootFinish;

		public Asda2ObjectLoot()
		{
		}

        public Asda2ObjectLoot(IAsda2Lootable looted, uint money, Asda2LootItem[] items)
			: base(looted, money, items)
		{
		}

		public override LootResponseType ResponseType
		{
			get { return LootResponseType.Profession; }
		}

		protected override void OnDispose()
		{
			if (OnLootFinish != null)
			{
				OnLootFinish();
				OnLootFinish = null;
			}
			base.OnDispose();
		}

	    #region Overrides of ObjectBase

	    public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
	    {
            get { return null; }
	    }

	    protected override UpdateFieldCollection _UpdateFieldInfos
	    {
            get { return null; }
	    }

	    public override UpdateFlags UpdateFlags
	    {
            get { return 0; }
	    }

	    public override ObjectTypeId ObjectTypeId
	    {
            get { return 0; }
	    }

	    #endregion

	    #region Overrides of WorldObject

	    public override string Name { get; set; }
	    public override Faction Faction { get; set; }
	    public override FactionId FactionId { get; set; }

	    #endregion
	}
}