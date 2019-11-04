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

        /// <summary>
        /// Betölti (létrehozza) az összes plugin definiált instancát
        /// </summary>
        /// <returns>betöltött pluginek listája</returns>
        Dictionary<string, IPlugin> BuildAll();

        /// <summary>
        /// A megadott típusú plugin összes instancának betöltése
        /// </summary>
        /// <param name="type">Plugion típus</param>
        /// <returns>betöltött pluginek listája</returns>
        Dictionary<string, IPlugin> BuildAllFromThis(Type type);

        /// <summary>
        /// A megadott típusú és verziójú plugin összes instancának betöltése
        /// </summary>
        /// <param name="type">Plugin típusa</param>
        /// <param name="version">Verzió</param>
        /// <returns>betöltött pluginek listája</returns>
        Dictionary<string, IPlugin> BuildAllFromThisUnderThisVersion(Type type, string version);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        IPlugin BuildThis(Type type, string name, string version);
    }
}
