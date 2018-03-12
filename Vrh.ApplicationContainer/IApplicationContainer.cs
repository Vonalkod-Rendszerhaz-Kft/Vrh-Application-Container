using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Vrh.ApplicationContainer
{
    /// <summary>
    /// Az ApplicationContainer WCF interfésze, main át elérhetőek a működésével kapcsolatos szolgáltatások
    /// </summary>
    [ServiceContract]    
    public interface IApplicationContainer
    {
        /// <summary>
        /// Alapvető információk az ApplicationContainer keretről
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        ApplicationContainerInfo GetApplicationContainerInfo();

        /// <summary>
        /// Visszadja, hogy az összeállítás milyen pluginokat definiál
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<PluginDefinition> GetDefinedPlugins();

        /// <summary>
        /// Visszadja a core által összegyűjtött hibaeseményeket
        /// </summary>
        /// <returns></returns>
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

    /// <summary>
    /// Az alkalmazás container információ
    /// </summary>
    [DataContract]
    public class ApplicationContainerInfo
    {
        /// <summary>
        /// Munkakönyvtár
        /// </summary>
        [DataMember]
        public string RunningDirectory { get; set; }

        /// <summary>
        /// Megjegyzés (az apllicationcontainer core dll descriptionje)
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// ApplicationContainer Core verzió
        /// </summary>
        [DataMember]
        public string Version { get; set; }

        /// <summary>
        /// Copyright info
        /// </summary>
        [DataMember]
        public string CopyRight { get; set; }

        /// <summary>
        /// Az összeállításban használt instancefactory plugin
        /// </summary>
        [DataMember]
        public string InstanceFactoryPlugin { get; set; }

        /// <summary>
        /// Az összeállításban használt instancefactory plugin verziója
        /// </summary>
        [DataMember]
        public string InstanceFactoryVersion { get; set; }

        /// <summary>
        /// Az instancefactory plugin beállításaira vonatkozó információ
        /// </summary>
        [DataMember]
        public string InstanceFactorySettings { get; set; }

        /// <summary>
        /// Ez az Assembly tartalmazza az összeállításban használt InstanceFactory-t
        /// </summary>
        [DataMember]
        public string InstanceFactoryAssembly { get; set; }

        /// <summary>
        /// Ekkor indult az ApplicationContainer keret
        /// </summary>
        [DataMember]
        public DateTime StartTimeStamp { get; set; }

        /// <summary>
        /// Eddig tartott a keret teljes elindulása
        /// </summary>
        [DataMember]
        public double LastStartupFullTime { get; set; }
    }
}
