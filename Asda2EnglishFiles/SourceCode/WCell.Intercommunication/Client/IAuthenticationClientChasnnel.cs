using System;
using System.CodeDom.Compiler;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace WCell.Intercommunication.Client
{
    [GeneratedCode("System.ServiceModel", "3.0.0.0")]
    public interface IAuthenticationClientChasnnel : IWCellIntercomService, IClientChannel, IContextChannel, IChannel,
        ICommunicationObject, IExtensibleObject<IContextChannel>, IDisposable
    {
    }
}