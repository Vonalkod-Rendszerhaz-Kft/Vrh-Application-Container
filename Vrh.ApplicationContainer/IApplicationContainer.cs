using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Vrh.ApplicationContainer
{
    [ServiceContract]
    public interface IApplicationContainer
    {
        [OperationContract]
        ApplicationContainerInfo GetApplicationContainerInfo();

        [OperationContract]
        List<PluginDefinition> GetDefinedPlugins();

        [OperationContract]
        List<MessageStackEntry> GetErrors();

        [OperationContract]
        List<MessageStackEntry> GetInfos();

        [OperationContract]
        List<MessageStackEntry> GetInstanceFactoryErrors();

        [OperationContract]
        List<MessageStackEntry> GetInstanceFactoryInfos();

        [OperationContract]
        List<InstanceDefinition> GetInstances(string pluginType, string version);

        [OperationContract]
        PluginStatus GetInstanceSatus(Guid internalId);

        [OperationContract]
        List<MessageStackEntry> GetPluginInstanceErrors(Guid internalId);

        [OperationContract]
        List<MessageStackEntry> GetPluginInstanceInfos(Guid internalId);

        [OperationContract]
        bool StartPlugin(Guid internalId);

        [OperationContract]
        bool StopPlugin(Guid internalId);

        [OperationContract]
        bool ReloadPluginInstance(Guid internalId);

        [OperationContract]
        bool FollowPluginDefinitionChanges(string pluginType, string version);
    }

    [DataContract]
    public class ApplicationContainerInfo
    {
        [DataMember]
        public string RunningDirectory { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public string CopyRight { get; set; }

        [DataMember]
        public string InstanceFactoryPlugin { get; set; }

        [DataMember]
        public string InstanceFactoryVersion { get; set; }

        [DataMember]
        public string InstanceFactorySettings { get; set; }

        [DataMember]
        public string InstanceFactoryAssembly { get; set; }

        [DataMember]
        public DateTime StartTimeStamp { get; set; }

        [DataMember]
        public double LastStartupFullTime { get; set; }
    }

    [DataContract]
    public enum Level
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        FatalError = 1,
        [EnumMember]
        Error = 2,
        [EnumMember]
        Warning = 3,
        [EnumMember]
        Info = 4,
    }
}
