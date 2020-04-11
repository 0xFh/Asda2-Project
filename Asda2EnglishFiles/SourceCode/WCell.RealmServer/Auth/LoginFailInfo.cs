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
            this.Count = 1;
            this.LastAttempt = lastAttempt;
            this.Handle = (WaitHandle) new EventWaitHandle(false, EventResetMode.ManualReset);
        }
    }
}