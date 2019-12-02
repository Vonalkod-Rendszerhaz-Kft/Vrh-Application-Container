using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using Vrh.ApplicationContainer.Control.Contract;

namespace Vrh.ApplicationContainer.Control
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

        /// <summary>
        /// Visszadja a coreban tárolt működési információkat
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<MessageStackEntry> GetInfos();

        /// <summary>
        /// Visszadja a használt InstanceFactory működése kapcsán tárolt hibákat 
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<MessageStackEntry> GetInstanceFactoryErrors();

        /// <summary>
        /// Visszadja a használt InstanceFactory működésere vonatkozó információkat
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<MessageStackEntry> GetInstanceFactoryInfos();

        /// <summary>
        /// Visszadja az instance-ok listáját az adott típusú és verziójú pluginból
        /// </summary>
        /// <param name="pluginType">plugin típus</param>
        /// <param name="version">verzió</param>
        /// <returns></returns>
        [OperationContract]
        List<InstanceDefinition> GetInstances(string pluginType, string version);

        /// <summary>
        /// Visszadja a példány státusz információit
        /// </summary>
        /// <param name="internalId">a példány azonosítója</param>
        /// <returns></returns>
        [OperationContract]
        PluginStatus GetInstanceSatus(Guid internalId);

        /// <summary>
        /// Visszadja a példány hiba információit
        /// </summary>
        /// <param name="internalId">a példány azonosítója</param>
        /// <returns></returns>
        [OperationContract]
        List<MessageStackEntry> GetPluginInstanceErrors(Guid internalId);

        /// <summary>
        /// Visszadja a példány működésével kapcsolatban tárolt információkat
        /// </summary>
        /// <param name="internalId">a példány azonosítója</param>
        /// <returns></returns>
        [OperationContract]
        List<MessageStackEntry> GetPluginInstanceInfos(Guid internalId);

        /// <summary>
        /// Elindítja a plugint
        /// </summary>
        /// <param name="internalId">a példány azonosítója</param>
        /// <returns></returns>
        [OperationContract]
        bool StartPlugin(Guid internalId);

        /// <summary>
        /// Leállítja a plugint
        /// </summary>
        /// <param name="internalId">a példány azonosítója</param>
        /// <returns></returns>
        [OperationContract]
        bool StopPlugin(Guid internalId);

        /// <summary>
        /// újratölti a plugint
        /// </summary>
        /// <param name="internalId">a példány azonosítója</param>
        /// <returns></returns>
        [OperationContract]
        bool ReloadPluginInstance(Guid internalId);

        /// <summary>
        /// Leköveti az adott típusú és verziójú plugin alatt bekövetkezett instancokra vonatkozó definiciós változásokat
        /// </summary>
        /// <param name="pluginType">plugin típus</param>
        /// <param name="version">plugin verzió</param>
        /// <returns></returns>
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
