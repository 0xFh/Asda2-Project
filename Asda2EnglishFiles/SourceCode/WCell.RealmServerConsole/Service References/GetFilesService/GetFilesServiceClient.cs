// Decompiled with JetBrains decompiler
// Type: WCell.RealmServerConsole.GetFilesService.GetFilesServiceClient
// Assembly: WCell.RealmServerConsole, Version=0.5.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1D19BA74-5B0A-4712-9872-8B8B01DD7A49
// Assembly location: C:\Users\NoN\Desktop\Run\Debug\WCell.RealmServerConsole.exe

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
        private ClientBase<IGetFilesService>.BeginOperationDelegate onBeginGetDataUsingDataContractDelegate;
        private ClientBase<IGetFilesService>.EndOperationDelegate onEndGetDataUsingDataContractDelegate;
        private SendOrPostCallback onGetDataUsingDataContractCompletedDelegate;
        private ClientBase<IGetFilesService>.BeginOperationDelegate onBeginGetContentDataDelegate;
        private ClientBase<IGetFilesService>.EndOperationDelegate onEndGetContentDataDelegate;
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
            return this.Channel.GetDataUsingDataContract();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IAsyncResult BeginGetDataUsingDataContract(AsyncCallback callback, object asyncState)
        {
            return this.Channel.BeginGetDataUsingDataContract(callback, asyncState);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public CompositeType EndGetDataUsingDataContract(IAsyncResult result)
        {
            return this.Channel.EndGetDataUsingDataContract(result);
        }

        private IAsyncResult OnBeginGetDataUsingDataContract(object[] inValues, AsyncCallback callback,
            object asyncState)
        {
            return this.BeginGetDataUsingDataContract(callback, asyncState);
        }

        private object[] OnEndGetDataUsingDataContract(IAsyncResult result)
        {
            return new object[1]
            {
                (object) this.EndGetDataUsingDataContract(result)
            };
        }

        private void OnGetDataUsingDataContractCompleted(object state)
        {
            if (this.GetDataUsingDataContractCompleted == null)
                return;
            ClientBase<IGetFilesService>.InvokeAsyncCompletedEventArgs completedEventArgs =
                (ClientBase<IGetFilesService>.InvokeAsyncCompletedEventArgs) state;
            this.GetDataUsingDataContractCompleted((object) this,
                new GetDataUsingDataContractCompletedEventArgs(completedEventArgs.Results, completedEventArgs.Error,
                    completedEventArgs.Cancelled, completedEventArgs.UserState));
        }

        public void GetDataUsingDataContractAsync()
        {
            this.GetDataUsingDataContractAsync((object) null);
        }

        public void GetDataUsingDataContractAsync(object userState)
        {
            if (this.onBeginGetDataUsingDataContractDelegate == null)
                this.onBeginGetDataUsingDataContractDelegate =
                    new ClientBase<IGetFilesService>.BeginOperationDelegate(this.OnBeginGetDataUsingDataContract);
            if (this.onEndGetDataUsingDataContractDelegate == null)
                this.onEndGetDataUsingDataContractDelegate =
                    new ClientBase<IGetFilesService>.EndOperationDelegate(this.OnEndGetDataUsingDataContract);
            if (this.onGetDataUsingDataContractCompletedDelegate == null)
                this.onGetDataUsingDataContractCompletedDelegate =
                    new SendOrPostCallback(this.OnGetDataUsingDataContractCompleted);
            this.InvokeAsync(this.onBeginGetDataUsingDataContractDelegate, (object[]) null,
                this.onEndGetDataUsingDataContractDelegate, this.onGetDataUsingDataContractCompletedDelegate,
                userState);
        }

        public CompositeType GetContentData()
        {
            return this.Channel.GetContentData();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IAsyncResult BeginGetContentData(AsyncCallback callback, object asyncState)
        {
            return this.Channel.BeginGetContentData(callback, asyncState);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public CompositeType EndGetContentData(IAsyncResult result)
        {
            return this.Channel.EndGetContentData(result);
        }

        private IAsyncResult OnBeginGetContentData(object[] inValues, AsyncCallback callback, object asyncState)
        {
            return this.BeginGetContentData(callback, asyncState);
        }

        private object[] OnEndGetContentData(IAsyncResult result)
        {
            return new object[1]
            {
                (object) this.EndGetContentData(result)
            };
        }

        private void OnGetContentDataCompleted(object state)
        {
            if (this.GetContentDataCompleted == null)
                return;
            ClientBase<IGetFilesService>.InvokeAsyncCompletedEventArgs completedEventArgs =
                (ClientBase<IGetFilesService>.InvokeAsyncCompletedEventArgs) state;
            this.GetContentDataCompleted((object) this,
                new GetContentDataCompletedEventArgs(completedEventArgs.Results, completedEventArgs.Error,
                    completedEventArgs.Cancelled, completedEventArgs.UserState));
        }

        public void GetContentDataAsync()
        {
            this.GetContentDataAsync((object) null);
        }

        public void GetContentDataAsync(object userState)
        {
            if (this.onBeginGetContentDataDelegate == null)
                this.onBeginGetContentDataDelegate =
                    new ClientBase<IGetFilesService>.BeginOperationDelegate(this.OnBeginGetContentData);
            if (this.onEndGetContentDataDelegate == null)
                this.onEndGetContentDataDelegate =
                    new ClientBase<IGetFilesService>.EndOperationDelegate(this.OnEndGetContentData);
            if (this.onGetContentDataCompletedDelegate == null)
                this.onGetContentDataCompletedDelegate = new SendOrPostCallback(this.OnGetContentDataCompleted);
            this.InvokeAsync(this.onBeginGetContentDataDelegate, (object[]) null, this.onEndGetContentDataDelegate,
                this.onGetContentDataCompletedDelegate, userState);
        }
    }
}