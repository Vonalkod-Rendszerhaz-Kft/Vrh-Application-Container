using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.ApplicationContainer
{
    /// <summary>
    /// Interface for Build all Plugin Instances
    /// </summary>
    public interface IInstanceFactory : IDisposable
    {
        /// <summary>
        /// Visszadja az ősszes definiált plugint
        /// </summary>
        /// <returns></returns>
        IEnumerable<PluginDefinition> GetAllPlugin();

        /// <summary>
        /// Visszadja az InstanceFactory plugin konfigurációjára vonatkozó információkat, ha vannak ilyenek
        /// </summary>
        string Config { get; }

        /// <summary>
        /// Visszadja az adott plugintípus és verzió alá definiált pluginok listáját, ha nem maga  aplugin típus gondoskodik ennek szolgáltatásáról 
        /// </summary>
        /// <param name="pluginType">Plugin típusa (Type.FullName)</param>
        /// <param name="version">Plugin verziója</param>
        /// <returns></returns>
        IEnumerable<InstanceDefinition> GetAllInstance(string pluginType, string version);

        /// <summary>
        /// Az intsance factory működése során előforduló hiba információk listája
        /// </summary>
        List<MessageStackEntry> Errors { get; }

        /// <summary>
        /// Az intsance factory működése során előforduló információk listája
        /// </summary>
        List<MessageStackEntry> Infos { get; }

        Dictionary<string, IPlugin> BuildAll();

        Dictionary<string, IPlugin> BuildAllFromThis(Type type);

        Dictionary<string, IPlugin> BuildAllFromThisUnderThisVersion(Type type, String version);

        IPlugin BuildThis(Type type, String name, String version);
    }
}
