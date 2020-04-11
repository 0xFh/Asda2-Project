using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;

namespace WCell.RealmServerConsole.GetFilesService
{
  [DebuggerStepThrough]
  [GeneratedCode("System.ServiceModel", "4.0.0.0")]
  public class GetContentDataCompletedEventArgs : AsyncCompletedEventArgs
  {
    private object[] results;

    public GetContentDataCompletedEventArgs(object[] results, Exception exception, bool cancelled, object userState)
      : base(exception, cancelled, userState)
    {
      this.results = results;
    }

    public CompositeType Result
    {
      get
      {
        RaiseExceptionIfNecessary();
        return (CompositeType) results[0];
      }
    }
  }
}