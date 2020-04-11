// Decompiled with JetBrains decompiler
// Type: WCell.RealmServerConsole.GetFilesService.IGetFilesServiceChannel
// Assembly: WCell.RealmServerConsole, Version=0.5.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1D19BA74-5B0A-4712-9872-8B8B01DD7A49
// Assembly location: C:\Users\NoN\Desktop\Run\Debug\WCell.RealmServerConsole.exe

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