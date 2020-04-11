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
      Message = msg;
      Exception = exception;
    }

    public override string ToString()
    {
      return Exception.Message + " triggered at " + Time + " - " + Message;
    }
  }
}