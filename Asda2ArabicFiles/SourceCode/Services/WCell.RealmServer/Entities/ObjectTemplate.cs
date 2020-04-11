using System.Collections.Generic;
using WCell.RealmServer.Asda2Looting;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Quests;
using WCell.Util.Data;

namespace WCell.RealmServer.Entities
{
    [System.Serializable]
	public abstract class ObjectTemplate : IQuestHolderEntry
	{
		/// <summary>
		/// Entry Id
		/// </summary>
		public uint Id
		{
			get;
			set;
		}

	    private float _scale =1;

	    public float Scale
	    {
	        get { return _scale; }
	        set { _scale = value; }
	    }

	    public abstract List<Asda2LootItemEntry> GetLootEntries();

		#region Implementation of IQuestHolderEntry
        [System.NonSerialized]
		private QuestHolderInfo m_QuestHolderInfo;
        [System.NonSerialized]
        private GossipMenu _defaultGossip;

        /// <summary>
		/// The QuestHolderEntry of this template, if this is a QuestGiver
		/// </summary>
		[NotPersistent]
		public QuestHolderInfo QuestHolderInfo
		{
			get { return m_QuestHolderInfo; }
			set
			{
				m_QuestHolderInfo = value;
			}
		}

		[NotPersistent]
		public GossipMenu DefaultGossip
		{
		    get { return _defaultGossip; }
		    set { _defaultGossip = value; }
		}


        public abstract IWorldLocation[] GetInWorldTemplates();

		#endregion
	}
}
