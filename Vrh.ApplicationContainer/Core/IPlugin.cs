using System;
using System.Collections.Generic;
using Vrh.ApplicationContainer.Control.Contract;

namespace Vrh.ApplicationContainer.Core
{
    /// <summary>
    /// Interface for Plugins 
    /// </summary>
    public interface IPlugin : IDisposable
    {
        /// <summary>
        /// Esemény a plugin állapotváltozás jelzésére
        /// </summary>
        event PluginStatusChangedEventHandler PluginStatusChanged;

        /// <summary>
        /// Plugin indíttása
        /// </summary>
        void Start();

        /// <summary>
        /// Plugin leállítása
        /// </summary>
        void Stop();

        /// <summary>
        /// Plugin példány státuszinformációi
        /// </summary>
        PluginStatus Status { get; }

        /// <summary>
        /// Plugin példány adatai 
        /// </summary>
        InstanceDefinition WhoAmI { get; set; }

        /// <summary>
        /// Pluginban keletkezett hibák gyűjtőstackje
        /// </summary>
        List<MessageStackEntry> Errors { get; }

        /// <summary>
        /// Plugin működésével kapcsolatos információk gyűjtőstackje
        /// </summary>
        List<MessageStackEntry> Infos { get; }
    }

    /// <summary>
    /// Delegate definició a plugin állapotváltozását jelző eseményhez 
    /// </summary>
    /// <param name="pluginId">plugin azonosító</param>
    /// <param name="condition">plugin állapot</param>
    public delegate void PluginStatusChangedEventHandler(Guid pluginId, PluginState condition);
}


