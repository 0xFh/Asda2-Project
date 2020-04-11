using System;
using System.CodeDom.Compiler;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace WCell.RealmServerConsole.GetFilesService
{
  [GeneratedCode("System.ServiceModel", "4.0.0.0")]
  public interface IGetFilesServiceChannel : IGetFilesService, IClientChannel, IContextChannel, IChannel,
    ICommunicationObject, IExtensibleObject<IContextChannel>, IDisposable
  {
  }
}