using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.ApplicationContainer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ApplicationContainerService : IApplicationContainer
    {
        public ApplicationContainerInfo GetApplicationContainerInfo()
        {
            return ApplicationContainerReference.MyInfo;
        }

        public List<MessageStackEntry> GetErrors()
        {
            return ApplicationContainerReference.ErrorStack;
        }

        public List<MessageStackEntry> GetInfos()
        {
            return ApplicationContainerReference.InfoStack;
        }

        public List<MessageStackEntry> GetInstanceFactoryErrors()
        {
            return ApplicationContainerReference.InstanceFactoryErrorStack;
        }

        public List<MessageStackEntry> GetInstanceFactoryInfos()
        {
            return ApplicationContainerReference.InstanceFactoryInfoStack;
        }

        public List<PluginDefinition> GetDefinedPlugins()
        {
            return ApplicationContainerReference.DefinedOrLoadedPlugins;
        }

        public List<InstanceDefinition> GetInstances(string pluginType, string version)
        {
            return ApplicationContainerReference.GetInstances(pluginType, version);
        }

        public PluginStatus GetInstanceSatus(Guid internalId)
        {
            return ApplicationContainerReference.GetPluginStatus(internalId);
        }

        public List<MessageStackEntry> GetPluginInstanceErrors(Guid internalId)
        {
            return ApplicationContainerReference.GetPluginInstanceErrors(internalId);
        }

        public List<MessageStackEntry> GetPluginInstanceInfos(Guid internalId)
        {
            return ApplicationContainerReference.GetPluginInstanceInfos(internalId);
        }

        public bool StartPlugin(Guid internalId)
        {
            return ApplicationContainerReference.StartPlugin(internalId);
        }

        public bool StopPlugin(Guid internalId)
        {
            return ApplicationContainerReference.StopPlugin(internalId);
        }

        public bool ReloadPluginInstance(Guid internalId)
        {
            return ApplicationContainerReference.ReloadPluginInstance(internalId);
        }

        public bool FollowPluginDefinitionChanges(string pluginType, string version)
        {
            return ApplicationContainerReference.FollowPluginDefinitionChange(pluginType, version);
        }

        public ApplicationContainer ApplicationContainerReference { private get; set; }
    }
}
