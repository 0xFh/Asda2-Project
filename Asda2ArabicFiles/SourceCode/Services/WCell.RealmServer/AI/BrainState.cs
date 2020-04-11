namespace WCell.RealmServer.AI
{
    [System.Serializable]
	public enum BrainState
	{
		/// <summary>
		/// Don't do anything
		/// </summary>
		Idle,

		/// <summary>
		/// Do default actions (move along waypoints, if there are any)
		/// </summary>
		Roam,

		/// <summary>
		/// Attack nearby enemies
		/// </summary>
		Combat,

		/// <summary>
		/// Toughen mob back up, run back Home then fall back to DefaultState
		/// </summary>
		Evade,

		/// <summary>
		/// Follow
		/// </summary>
		Follow,

		/// <summary>
		/// Follow and guard
		/// </summary>
		Guard,

		/// <summary>
		/// Move in formation
		/// </summary>
		FormationMove,
        
	    PatrolCircle,
	    GmMove,
	    DefenceTownEventMove,
	    Fear,
		/// <summary>
		/// Dead.
		/// </summary>
		Dead,
		End,
	}
}