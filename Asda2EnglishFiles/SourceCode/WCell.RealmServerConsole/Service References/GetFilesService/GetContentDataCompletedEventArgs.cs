// Decompiled with JetBrains decompiler
// Type: WCell.RealmServerConsole.GetFilesService.GetContentDataCompletedEventArgs
// Assembly: WCell.RealmServerConsole, Version=0.5.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1D19BA74-5B0A-4712-9872-8B8B01DD7A49
// Assembly location: C:\Users\NoN\Desktop\Run\Debug\WCell.RealmServerConsole.exe

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
                this.RaiseExceptionIfNecessary();
                return (CompositeType) this.results[0];
            }
        }
    }
}