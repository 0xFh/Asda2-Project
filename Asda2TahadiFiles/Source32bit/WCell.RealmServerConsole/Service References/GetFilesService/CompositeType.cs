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
    [NonSerialized]private ExtensionDataObject extensionDataField;
    [OptionalField]private Dictionary<string, byte[]> FilesField;

    [Browsable(false)]
    public ExtensionDataObject ExtensionData
    {
      get { return extensionDataField; }
      set { extensionDataField = value; }
    }

    [DataMember]
    public Dictionary<string, byte[]> Files
    {
      get { return FilesField; }
      set
      {
        if(ReferenceEquals(FilesField, value))
          return;
        FilesField = value;
        RaisePropertyChanged(nameof(Files));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void RaisePropertyChanged(string propertyName)
    {
      PropertyChangedEventHandler propertyChanged = PropertyChanged;
      if(propertyChanged == null)
        return;
      propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}