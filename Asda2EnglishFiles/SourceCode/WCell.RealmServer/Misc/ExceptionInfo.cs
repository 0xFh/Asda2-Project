using System;

namespace WCell.RealmServer.Misc
{
    public class ExceptionInfo
    {
        public readonly DateTime Time = DateTime.Now;
        public readonly Exception Exception;
        public readonly string Message;

        public ExceptionInfo(string msg, Exception exception)
        {
            this.Message = msg;
            this.Exception = exception;
        }

        public override string ToString()
        {
            return this.Exception.Message + " triggered at " + (object) this.Time + " - " + this.Message;
        }
    }
}