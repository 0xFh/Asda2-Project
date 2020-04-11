using System;
using System.CodeDom.Compiler;
using System.ServiceModel;

namespace WCell.RealmServerConsole.GetFilesService
{
  [ServiceContract(ConfigurationName = "GetFilesService.IGetFilesService")]
  [GeneratedCode("System.ServiceModel", "4.0.0.0")]
  public interface IGetFilesService
  {
    [OperationContract(Action = "http://tempuri.org/IGetFilesService/GetDataUsingDataContract",
      ReplyAction = "http://tempuri.org/IGetFilesService/GetDataUsingDataContractResponse")]
    CompositeType GetDataUsingDataContract();

    [OperationContract(Action = "http://tempuri.org/IGetFilesService/GetDataUsingDataContract", AsyncPattern = true,
      ReplyAction = "http://tempuri.org/IGetFilesService/GetDataUsingDataContractResponse")]
    IAsyncResult BeginGetDataUsingDataContract(AsyncCallback callback, object asyncState);

    CompositeType EndGetDataUsingDataContract(IAsyncResult result);

    [OperationContract(Action = "http://tempuri.org/IGetFilesService/GetContentData",
      ReplyAction = "http://tempuri.org/IGetFilesService/GetContentDataResponse")]
    CompositeType GetContentData();

    [OperationContract(Action = "http://tempuri.org/IGetFilesService/GetContentData", AsyncPattern = true,
      ReplyAction = "http://tempuri.org/IGetFilesService/GetContentDataResponse")]
    IAsyncResult BeginGetContentData(AsyncCallback callback, object asyncState);

    CompositeType EndGetContentData(IAsyncResult result);
  }
}