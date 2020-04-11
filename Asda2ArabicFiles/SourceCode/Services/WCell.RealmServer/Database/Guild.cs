using System;
using Castle.ActiveRecord;
using WCell.RealmServer.Database;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Guilds
{
	[ActiveRecord("Guild", Access = PropertyAccess.Property)]
	public partial class Guild : ActiveRecordBase<Guild>
	{
		private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(Guild), "_id");

		[PrimaryKey(PrimaryKeyType.Assigned, "Id")]
		private long _id
		{
			get;
			set;
		}

		[Field("Name", NotNull = true, Unique = true)]
		private string _name;

		[Field("MOTD", NotNull = true)]
		private string _MOTD;

		[Field("Info", NotNull = true)]
		private string _info;

		[Field("Created", NotNull = true)]
		private DateTime _created;

		[Field("LeaderLowId", NotNull = true)]
		private int _leaderLowId;

	    private byte _level;
	    private uint _points;
        private int _waveLimit;
	    private byte[] _clanCrest;
	    private string _noticeWriter;
	    private DateTime _noticeDateTime;

	    public uint LeaderLowId
		{
			get { return (uint)_leaderLowId; }
		}

        [Property]
        public int WaveLimit
        {
            get { return _waveLimit; }
            set { _waveLimit = value; this.UpdateLater(); }
        }

		[Nested("Tabard")]
		private GuildTabard _tabard
		{
			get;
			set;
		}

		[Property]
		public int PurchasedBankTabCount
		{
			get;
			internal set;
		}

		[Property]
		public long Money
		{
			get;
			set;
		}
        [Property]
	    public byte Level
        {
            get { return _level; }
            set { _level = value; this.UpdateLater(); }
        }

	    [Property]
        public byte MaxMembersCount { get; set; }
         [Property]
	    public UInt32 Points
         {
             get { return _points; }
             set { _points = value; }
         }

	    [Property]
	    public DateTime NoticeDateTime
	    {
	        get { return _noticeDateTime; }
            set { _noticeDateTime = value; this.UpdateLater(); }
	    }

	    [Property (Length = 20)]
	    public string NoticeWriter
         {
             get { return _noticeWriter; }
             set { _noticeWriter = value; this.UpdateLater(); }
         }

	    [Property(Length = 40)]
	    public byte[] ClanCrest
        {
            get { return _clanCrest; }
            set { _clanCrest = value; this.UpdateLater();
                foreach (var character in GetCharacters())
                {
                    GlobalHandler.SendCharactrerInfoClanNameToAllNearbyCharacters(character);
                }
            }
        }
	}
}