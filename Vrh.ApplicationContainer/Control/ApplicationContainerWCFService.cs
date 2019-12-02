using System;
using System.Collections.Generic;
using System.ServiceModel;
using Vrh.ApplicationContainer.Control.Contract;

namespace Vrh.ApplicationContainer.Control
{
    /// <summary>
    /// Application container WCF service
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ApplicationContainerWCFService : IApplicationContainer
    {
        /// <summary>
        /// Alapvető információk az ApplicationContainer keretről
        /// </summary>
        /// <returns></returns>
        public ApplicationContainerInfo GetApplicationContainerInfo()
        {
            return ApplicationContainerReference.MyInfo;
        }

        /// <summary>
        /// Visszadja a core által összegyűjtött hibaeseményeket
        /// </summary>
        /// <returns></returns>
        public List<MessageStackEntry> GetErrors()
        {
            return ApplicationContainerReference.ErrorStack;
        }

        /// <summary>
        /// Visszadja a coreban tárolt működési információkat
        /// </summary>
        /// <returns></returns>
        public List<MessageStackEntry> GetInfos()
        {
            return ApplicationContainerReference.InfoStack;
        }

        /// <summary>
        /// Visszadja a használt InstanceFactory működése kapcsán tárolt hibákat 
        /// </summary>
        /// <returns></returns>
        public List<MessageStackEntry> GetInstanceFactoryErrors()
        {
            return ApplicationContainerReference.InstanceFactoryErrorStack;
        }

        /// <summary>
        /// Visszadja a használt InstanceFactory működésere vonatkozó információkat
        /// </summary>
        /// <returns></returns>
        public List<MessageStackEntry> GetInstanceFactoryInfos()
        {
            return ApplicationContainerReference.InstanceFactoryInfoStack;
        }

        /// <summary>
        /// Visszadja, hogy az összeállítás milyen pluginokat definiál
        /// </summary>
        /// <returns></returns>
        public List<PluginDefinition> GetDefinedPlugins()
        {
            return ApplicationContainerReference.DefinedOrLoadedPlugins;
        }

        /// <summary>
        /// Visszadja az instance-ok listáját az adott típusú és verziójú pluginból
        /// </summary>
        /// <param name="pluginType">plugin típus</param>
        /// <param name="version">verzió</param>
        /// <returns></returns>
        public List<InstanceDefinition> GetInstances(string pluginType, string version)
        {
            return ApplicationContainerReference.GetInstances(pluginType, version);
        }

        /// <summary>
        /// Visszadja a példány státusz információit
        /// </summary>
        /// <param name="internalId">a példány azonosítója</param>
        /// <returns></returns>
        public PluginStatus GetInstanceSatus(Guid internalId)
        {
            return ApplicationContainerReference.GetPluginStatus(internalId);
        }

        /// <summary>
        /// Visszadja a példány hiba információit
        /// </summary>
        /// <param name="internalId">a példány azonosítója</param>
        /// <returns></returns>
        public List<MessageStackEntry> GetPluginInstanceErrors(Guid internalId)
        {
            return ApplicationContainerReference.GetPluginInstanceErrors(internalId);
        }

        /// <summary>
        /// Visszadja a példány működésével kapcsolatban tárolt információkat
        /// </summary>
        /// <param name="internalId">a példány azonosítója</param>
        /// <returns></returns>
        public List<MessageStackEntry> GetPluginInstanceInfos(Guid internalId)
        {
            return ApplicationContainerReference.GetPluginInstanceInfos(internalId);
        }

        /// <summary>
        /// Elindítja a plugint
        /// </summary>
        /// <param name="internalId">a példány azonosítója</param>
        /// <returns></returns>
        public bool StartPlugin(Guid internalId)
        {
            return ApplicationContainerReference.StartPlugin(internalId);
        }

        /// <summary>
        /// Leállítja a plugint
        /// </summary>
        /// <param name="internalId">a példány azonosítója</param>
        /// <returns></returns>
        public bool StopPlugin(Guid internalId)
        {
            return ApplicationContainerReference.StopPlugin(internalId);
        }

        /// <summary>
        /// újratölti a plugint
        /// </summary>
        /// <param name="internalId">a példány azonosítója</param>
        /// <returns></returns>
        public bool ReloadPluginInstance(Guid internalId)
        {
            return ApplicationContainerReference.ReloadPluginInstance(internalId);
        }

        /// <summary>
        /// Leköveti az adott típusú és verziójú plugin alatt bekövetkezett instancokra vonatkozó definiciós változásokat
        /// </summary>
        /// <param name="pluginType">plugin típus</param>
        /// <param name="version">plugin verzió</param>
        /// <returns></returns>
        public bool FollowPluginDefinitionChanges(string pluginType, string version)
        {
            return ApplicationContainerReference.FollowPluginDefinitionChange(pluginType, version);
        }

        /// <summary>
        /// referencia az application container példányra
        /// </summary>
        internal Core.ApplicationContainer ApplicationContainerReference { private get; set; }
    }
}
