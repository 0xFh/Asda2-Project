// Decompiled with JetBrains decompiler
// Type: WCell.RealmServerConsole.GetFilesService.CompositeType
// Assembly: WCell.RealmServerConsole, Version=0.5.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1D19BA74-5B0A-4712-9872-8B8B01DD7A49
// Assembly location: C:\Users\NoN\Desktop\Run\Debug\WCell.RealmServerConsole.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace WCell.RealmServerConsole.GetFilesService
{
    [DebuggerStepThrough]
    [DataContract(Name = "CompositeType", Namespace = "http://schemas.datacontract.org/2004/07/ServerDistributer")]
    [GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
    [Serializable]
    public class CompositeType : IExtensibleDataObject, INotifyPropertyChanged
    {
        [NonSerialized] private ExtensionDataObject extensionDataField;
        [OptionalField] private Dictionary<string, byte[]> FilesField;

        [Browsable(false)]
        public ExtensionDataObject ExtensionData
        {
            get { return this.extensionDataField; }
            set { this.extensionDataField = value; }
        }

        [DataMember]
        public Dictionary<string, byte[]> Files
        {
            get { return this.FilesField; }
            set
            {
                if (object.ReferenceEquals((object) this.FilesField, (object) value))
                    return;
                this.FilesField = value;
                this.RaisePropertyChanged(nameof(Files));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
        }
    }
}