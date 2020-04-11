using System;
using System.Threading;

namespace WCell.RealmServer.Auth
{
	public class LoginFailInfo
	{
		public DateTime LastAttempt;
		public readonly WaitHandle Handle;
		public int Count;

		public LoginFailInfo(DateTime lastAttempt)
		{
			Count = 1;
			LastAttempt = lastAttempt;
			Handle = new EventWaitHandle(false, EventResetMode.ManualReset);
		}
	}
}