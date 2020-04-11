using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace WCell.RealmServerConsole.GetFilesService
{
  [GeneratedCode("System.ServiceModel", "4.0.0.0")]
  [DebuggerStepThrough]
  public class GetFilesServiceClient : ClientBase<IGetFilesService>, IGetFilesService
  {
    private BeginOperationDelegate onBeginGetDataUsingDataContractDelegate;
    private EndOperationDelegate onEndGetDataUsingDataContractDelegate;
    private SendOrPostCallback onGetDataUsingDataContractCompletedDelegate;
    private BeginOperationDelegate onBeginGetContentDataDelegate;
    private EndOperationDelegate onEndGetContentDataDelegate;
    private SendOrPostCallback onGetContentDataCompletedDelegate;

    public GetFilesServiceClient()
    {
    }

    public GetFilesServiceClient(string endpointConfigurationName)
      : base(endpointConfigurationName)
    {
    }

    public GetFilesServiceClient(string endpointConfigurationName, string remoteAddress)
      : base(endpointConfigurationName, remoteAddress)
    {
    }

    public GetFilesServiceClient(string endpointConfigurationName, EndpointAddress remoteAddress)
      : base(endpointConfigurationName, remoteAddress)
    {
    }

    public GetFilesServiceClient(Binding binding, EndpointAddress remoteAddress)
      : base(binding, remoteAddress)
    {
    }

    public event EventHandler<GetDataUsingDataContractCompletedEventArgs> GetDataUsingDataContractCompleted;

    public event EventHandler<GetContentDataCompletedEventArgs> GetContentDataCompleted;

    public CompositeType GetDataUsingDataContract()
    {
      return Channel.GetDataUsingDataContract();
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IAsyncResult BeginGetDataUsingDataContract(AsyncCallback callback, object asyncState)
    {
      return Channel.BeginGetDataUsingDataContract(callback, asyncState);
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public CompositeType EndGetDataUsingDataContract(IAsyncResult result)
    {
      return Channel.EndGetDataUsingDataContract(result);
    }

    private IAsyncResult OnBeginGetDataUsingDataContract(object[] inValues, AsyncCallback callback,
      object asyncState)
    {
      return BeginGetDataUsingDataContract(callback, asyncState);
    }

    private object[] OnEndGetDataUsingDataContract(IAsyncResult result)
    {
      return new object[1]
      {
        EndGetDataUsingDataContract(result)
      };
    }

    private void OnGetDataUsingDataContractCompleted(object state)
    {
      if (GetDataUsingDataContractCompleted == null)
        return;
      InvokeAsyncCompletedEventArgs completedEventArgs =
        (InvokeAsyncCompletedEventArgs) state;
      GetDataUsingDataContractCompleted(this,
        new GetDataUsingDataContractCompletedEventArgs(completedEventArgs.Results, completedEventArgs.Error,
          completedEventArgs.Cancelled, completedEventArgs.UserState));
    }

    public void GetDataUsingDataContractAsync()
    {
      GetDataUsingDataContractAsync(null);
    }

    public void GetDataUsingDataContractAsync(object userState)
    {
      if (onBeginGetDataUsingDataContractDelegate == null)
        onBeginGetDataUsingDataContractDelegate =
          OnBeginGetDataUsingDataContract;
      if (onEndGetDataUsingDataContractDelegate == null)
        onEndGetDataUsingDataContractDelegate =
          OnEndGetDataUsingDataContract;
      if (onGetDataUsingDataContractCompletedDelegate == null)
        onGetDataUsingDataContractCompletedDelegate =
          OnGetDataUsingDataContractCompleted;
      InvokeAsync(onBeginGetDataUsingDataContractDelegate, null,
        onEndGetDataUsingDataContractDelegate, onGetDataUsingDataContractCompletedDelegate,
        userState);
    }

    public CompositeType GetContentData()
    {
      return Channel.GetContentData();
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IAsyncResult BeginGetContentData(AsyncCallback callback, object asyncState)
    {
      return Channel.BeginGetContentData(callback, asyncState);
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public CompositeType EndGetContentData(IAsyncResult result)
    {
      return Channel.EndGetContentData(result);
    }

    private IAsyncResult OnBeginGetContentData(object[] inValues, AsyncCallback callback, object asyncState)
    {
      return BeginGetContentData(callback, asyncState);
    }

    private object[] OnEndGetContentData(IAsyncResult result)
    {
      return new object[1]
      {
        EndGetContentData(result)
      };
    }

    private void OnGetContentDataCompleted(object state)
    {
      if (GetContentDataCompleted == null)
        return;
      InvokeAsyncCompletedEventArgs completedEventArgs =
        (InvokeAsyncCompletedEventArgs) state;
      GetContentDataCompleted(this,
        new GetContentDataCompletedEventArgs(completedEventArgs.Results, completedEventArgs.Error,
          completedEventArgs.Cancelled, completedEventArgs.UserState));
    }

    public void GetContentDataAsync()
    {
      GetContentDataAsync(null);
    }

    public void GetContentDataAsync(object userState)
    {
      if (onBeginGetContentDataDelegate == null)
        onBeginGetContentDataDelegate =
          OnBeginGetContentData;
      if (onEndGetContentDataDelegate == null)
        onEndGetContentDataDelegate =
          OnEndGetContentData;
      if (onGetContentDataCompletedDelegate == null)
        onGetContentDataCompletedDelegate = OnGetContentDataCompleted;
      InvokeAsync(onBeginGetContentDataDelegate, null, onEndGetContentDataDelegate,
        onGetContentDataCompletedDelegate, userState);
    }
  }
}