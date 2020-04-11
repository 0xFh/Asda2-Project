using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;

namespace WCell.RealmServerConsole.GetFilesService
{
  [GeneratedCode("System.ServiceModel", "4.0.0.0")]
  [DebuggerStepThrough]
  public class GetDataUsingDataContractCompletedEventArgs : AsyncCompletedEventArgs
  {
    private object[] results;

    public GetDataUsingDataContractCompletedEventArgs(object[] results, Exception exception, bool cancelled,
      object userState)
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