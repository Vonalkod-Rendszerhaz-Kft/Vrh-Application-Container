﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IVConnector.Plugin.InterventionService {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="InterventionDefination", Namespace="http://schemas.datacontract.org/2004/07/DTM.WCF.InterventionService")]
    [System.SerializableAttribute()]
    public partial class InterventionDefination : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DescriptpionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DisplayNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string InterventionGroupNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private ParameterDefinition[] ParameterListField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Descriptpion {
            get {
                return this.DescriptpionField;
            }
            set {
                if ((object.ReferenceEquals(this.DescriptpionField, value) != true)) {
                    this.DescriptpionField = value;
                    this.RaisePropertyChanged("Descriptpion");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string DisplayName {
            get {
                return this.DisplayNameField;
            }
            set {
                if ((object.ReferenceEquals(this.DisplayNameField, value) != true)) {
                    this.DisplayNameField = value;
                    this.RaisePropertyChanged("DisplayName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string InterventionGroupName {
            get {
                return this.InterventionGroupNameField;
            }
            set {
                if ((object.ReferenceEquals(this.InterventionGroupNameField, value) != true)) {
                    this.InterventionGroupNameField = value;
                    this.RaisePropertyChanged("InterventionGroupName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public ParameterDefinition[] ParameterList {
            get {
                return this.ParameterListField;
            }
            set {
                if ((object.ReferenceEquals(this.ParameterListField, value) != true)) {
                    this.ParameterListField = value;
                    this.RaisePropertyChanged("ParameterList");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ParameterDefinition", Namespace="http://schemas.datacontract.org/2004/07/DTM.WCF.InterventionService")]
    [System.SerializableAttribute()]
    public partial class ParameterDefinition : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string AutoCompleteActionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Collections.Generic.Dictionary<string, string> AutoCompleteListField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DisplayNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string GetMessageField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private DataType ParameterTypeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool RequiredField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AutoCompleteAction {
            get {
                return this.AutoCompleteActionField;
            }
            set {
                if ((object.ReferenceEquals(this.AutoCompleteActionField, value) != true)) {
                    this.AutoCompleteActionField = value;
                    this.RaisePropertyChanged("AutoCompleteAction");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Collections.Generic.Dictionary<string, string> AutoCompleteList {
            get {
                return this.AutoCompleteListField;
            }
            set {
                if ((object.ReferenceEquals(this.AutoCompleteListField, value) != true)) {
                    this.AutoCompleteListField = value;
                    this.RaisePropertyChanged("AutoCompleteList");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string DisplayName {
            get {
                return this.DisplayNameField;
            }
            set {
                if ((object.ReferenceEquals(this.DisplayNameField, value) != true)) {
                    this.DisplayNameField = value;
                    this.RaisePropertyChanged("DisplayName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string GetMessage {
            get {
                return this.GetMessageField;
            }
            set {
                if ((object.ReferenceEquals(this.GetMessageField, value) != true)) {
                    this.GetMessageField = value;
                    this.RaisePropertyChanged("GetMessage");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public DataType ParameterType {
            get {
                return this.ParameterTypeField;
            }
            set {
                if ((this.ParameterTypeField.Equals(value) != true)) {
                    this.ParameterTypeField = value;
                    this.RaisePropertyChanged("ParameterType");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool Required {
            get {
                return this.RequiredField;
            }
            set {
                if ((this.RequiredField.Equals(value) != true)) {
                    this.RequiredField = value;
                    this.RaisePropertyChanged("Required");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="DataType", Namespace="http://schemas.datacontract.org/2004/07/DTM.WCF.InterventionService")]
    public enum DataType : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        String = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Boolean = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Int32 = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Double = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        DateTime = 4,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="InterventionedObject", Namespace="http://schemas.datacontract.org/2004/07/DTM.WCF.InterventionService")]
    [System.SerializableAttribute()]
    public partial class InterventionedObject : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ObjectDescriptionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int ObjectIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ObjectLabelField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ObjectDescription {
            get {
                return this.ObjectDescriptionField;
            }
            set {
                if ((object.ReferenceEquals(this.ObjectDescriptionField, value) != true)) {
                    this.ObjectDescriptionField = value;
                    this.RaisePropertyChanged("ObjectDescription");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int ObjectID {
            get {
                return this.ObjectIDField;
            }
            set {
                if ((this.ObjectIDField.Equals(value) != true)) {
                    this.ObjectIDField = value;
                    this.RaisePropertyChanged("ObjectID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ObjectLabel {
            get {
                return this.ObjectLabelField;
            }
            set {
                if ((object.ReferenceEquals(this.ObjectLabelField, value) != true)) {
                    this.ObjectLabelField = value;
                    this.RaisePropertyChanged("ObjectLabel");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Intervention", Namespace="http://schemas.datacontract.org/2004/07/DTM.WCF.InterventionService")]
    [System.SerializableAttribute()]
    public partial class Intervention : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int ObjectIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Collections.Generic.Dictionary<string, object> ParametersField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int ObjectID {
            get {
                return this.ObjectIDField;
            }
            set {
                if ((this.ObjectIDField.Equals(value) != true)) {
                    this.ObjectIDField = value;
                    this.RaisePropertyChanged("ObjectID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Collections.Generic.Dictionary<string, object> Parameters {
            get {
                return this.ParametersField;
            }
            set {
                if ((object.ReferenceEquals(this.ParametersField, value) != true)) {
                    this.ParametersField = value;
                    this.RaisePropertyChanged("Parameters");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="InterventionService.IInterventionService")]
    public interface IInterventionService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInterventionService/GetAllIntervention", ReplyAction="http://tempuri.org/IInterventionService/GetAllInterventionResponse")]
        InterventionDefination[] GetAllIntervention(string languageCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInterventionService/GetAllIntervention", ReplyAction="http://tempuri.org/IInterventionService/GetAllInterventionResponse")]
        System.Threading.Tasks.Task<InterventionDefination[]> GetAllInterventionAsync(string languageCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInterventionService/GetInterventionedObject", ReplyAction="http://tempuri.org/IInterventionService/GetInterventionedObjectResponse")]
        InterventionedObject[] GetInterventionedObject(string languageCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInterventionService/GetInterventionedObject", ReplyAction="http://tempuri.org/IInterventionService/GetInterventionedObjectResponse")]
        System.Threading.Tasks.Task<InterventionedObject[]> GetInterventionedObjectAsync(string languageCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInterventionService/GetCurrentInterventionOnThisObject", ReplyAction="http://tempuri.org/IInterventionService/GetCurrentInterventionOnThisObjectRespons" +
            "e")]
        InterventionDefination[] GetCurrentInterventionOnThisObject(int objectID, System.Guid userGuid, string languageCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInterventionService/GetCurrentInterventionOnThisObject", ReplyAction="http://tempuri.org/IInterventionService/GetCurrentInterventionOnThisObjectRespons" +
            "e")]
        System.Threading.Tasks.Task<InterventionDefination[]> GetCurrentInterventionOnThisObjectAsync(int objectID, System.Guid userGuid, string languageCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInterventionService/DoIntervention", ReplyAction="http://tempuri.org/IInterventionService/DoInterventionResponse")]
        string DoIntervention(Intervention intervention, System.Guid userGuid, string languageCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInterventionService/DoIntervention", ReplyAction="http://tempuri.org/IInterventionService/DoInterventionResponse")]
        System.Threading.Tasks.Task<string> DoInterventionAsync(Intervention intervention, System.Guid userGuid, string languageCode);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IInterventionServiceChannel : IInterventionService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class InterventionServiceClient : System.ServiceModel.ClientBase<IInterventionService>, IInterventionService {
        
        public InterventionServiceClient() {
        }
        
        public InterventionServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public InterventionServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public InterventionServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public InterventionServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public InterventionDefination[] GetAllIntervention(string languageCode) {
            return base.Channel.GetAllIntervention(languageCode);
        }
        
        public System.Threading.Tasks.Task<InterventionDefination[]> GetAllInterventionAsync(string languageCode) {
            return base.Channel.GetAllInterventionAsync(languageCode);
        }
        
        public InterventionedObject[] GetInterventionedObject(string languageCode) {
            return base.Channel.GetInterventionedObject(languageCode);
        }
        
        public System.Threading.Tasks.Task<InterventionedObject[]> GetInterventionedObjectAsync(string languageCode) {
            return base.Channel.GetInterventionedObjectAsync(languageCode);
        }
        
        public InterventionDefination[] GetCurrentInterventionOnThisObject(int objectID, System.Guid userGuid, string languageCode) {
            return base.Channel.GetCurrentInterventionOnThisObject(objectID, userGuid, languageCode);
        }
        
        public System.Threading.Tasks.Task<InterventionDefination[]> GetCurrentInterventionOnThisObjectAsync(int objectID, System.Guid userGuid, string languageCode) {
            return base.Channel.GetCurrentInterventionOnThisObjectAsync(objectID, userGuid, languageCode);
        }
        
        public string DoIntervention(Intervention intervention, System.Guid userGuid, string languageCode) {
            return base.Channel.DoIntervention(intervention, userGuid, languageCode);
        }
        
        public System.Threading.Tasks.Task<string> DoInterventionAsync(Intervention intervention, System.Guid userGuid, string languageCode) {
            return base.Channel.DoInterventionAsync(intervention, userGuid, languageCode);
        }
    }
}
