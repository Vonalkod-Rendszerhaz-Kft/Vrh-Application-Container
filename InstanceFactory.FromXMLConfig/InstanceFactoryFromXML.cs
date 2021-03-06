using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System.Configuration;
using VRH.Common;
using System.Runtime.CompilerServices;
using Vrh.Logger;
using Vrh.XmlProcessing;
using Vrh.ApplicationContainer.Control.Contract;
using Vrh.ApplicationContainer.Core;

namespace InstanceFactory.FromXML
{
    /// <summary>
    /// InstanceFactory implementáció, amerlyik egy XML config fájl lapján dolgozik
    /// </summary>
    [Export(typeof(IInstanceFactory))]
    public class InstanceFactoryFromXML : IInstanceFactory
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public InstanceFactoryFromXML()
        {
            string configFile = ConfigurationManager.AppSettings[_configFileSettingKey];
            if (string.IsNullOrEmpty(configFile))
            {
                configFile = @"Plugins.Config.xml";
            }
            _pluginConfig = new PluginsConfig(configFile);
            _pluginConfig.ConfigProcessorEvent += PluginConfig_ConfigProcessorEvent;
            _errors.Capacity = _pluginConfig.StackSize;
            _infos.Capacity = _pluginConfig.StackSize;
            var data = new Dictionary<string, string>()
            {
                { "Type", this.GetType().FullName },
                { "Version", this.GetType().Assembly.Version() },
                { "Assembly",  this.GetType().Assembly.Location },
                { "Config file", configFile },
            };
            LogThis("Instance Factory plugin loaded!", data, null, LogLevel.Information);
        }

        #region IInstanceFactory members

        /// <summary>
        /// Configuráció információ
        /// </summary>
        public string Config
        {
            get
            {
                return _pluginConfig.MyConfig;
            }
        }

        /// <summary>
        /// Visszadja az összes plugindefiniciót, amit a definiált kiépítés hostol
        /// </summary>
        /// <returns>plugin definiciók listája</returns>
        public IEnumerable<PluginDefinition> GetAllPlugin()
        {
            List<PluginDefinition> uniquePluginDefinations = new List<PluginDefinition>();
            foreach (var item in _pluginConfig.Plugins)
            {
                if (uniquePluginDefinations.Any(x => x.TypeName == item.TypeName && x.Version == item.Version))
                {
                    Dictionary<string, string> data = new Dictionary<string, string>()
                    {
                        { "Type", item.TypeName },
                        { "Version", item.Version },
                        { "Description", item.Description },
                    };
                    LogThis("This plugin already defined with this version!!!", data, null, LogLevel.Error);                    
                    continue;
                }
                else
                {
                    uniquePluginDefinations.Add(item);
                }
            }
            return uniquePluginDefinations;       
        }

        /// <summary>
        /// Visszadja az összes instance definiciót ha erről nem a plugin tyípus gondoskodik
        /// </summary>
        /// <param name="pluginType">plugin typusa</param>
        /// <param name="version">plugin verziója</param>
        /// <returns></returns>
        public IEnumerable<InstanceDefinition> GetAllInstance(string pluginType, string version)
        {
            return _pluginConfig.GetInstances(pluginType, version);
        }

        /// <summary>
        /// Visszadja a gyűjtött hibainformációkat
        /// </summary>
        public List<MessageStackEntry> Errors
        {
            get
            {
                return _errors.Items;
            }
        }

        /// <summary>
        /// Visszadja a gyűjtött müködési információkat
        /// </summary>
        public List<MessageStackEntry> Infos
        {
            get
            {
                return _infos.Items;
            }
        }

        #endregion IInstanceFactory members

        /// <summary>
        /// ConfigProcessorEvent eseményre feliratkozó metódus 
        /// </summary>
        /// <param name="e">Esemény argumentumok</param>
        private void PluginConfig_ConfigProcessorEvent(ConfigProcessorEventArgs e)
        {
            Vrh.Logger.LogLevel level =
                e.Exception.GetType().Name == typeof(ConfigProcessorWarning).Name
                    ? Vrh.Logger.LogLevel.Warning
                    : Vrh.Logger.LogLevel.Error;
            var data = new Dictionary<string, string>()
            {
                { "ConfigProcessor class", e.ConfigProcessor },
                { "Config file", e.ConfigFile },
            };
            LogThis($"Configuration issue: {e.Message}", data, e.Exception, level);
        }

        /// <summary>
        /// Logol egy bejegyzést, tölti a message stackekekt is 
        /// </summary>
        /// <param name="message">Szöveges információ</param>
        /// <param name="data">adatok (kulcs-érték párként)</param>
        /// <param name="ex">Kivétel</param>
        /// <param name="level">Log szint</param>
        /// <param name="caller">Hivási hely (metódus)</param>
        /// <param name="line">Hivási hely (forrássor)</param>
        private void LogThis(string message, Dictionary<string, string> data, Exception ex, LogLevel level, [CallerMemberName]string caller = "", [CallerLineNumber]int line = 0)
        {
            VrhLogger.LogNested<string>(message, data, ex, level, this.GetType(), caller, line,stacklevel:2);
            MessageStackEntry e = new MessageStackEntry()
            {
                Body = message,
                Data = data,
                TimeStamp = DateTime.UtcNow,
            };
            switch (level)
            {
                case Vrh.Logger.LogLevel.Information:
                case Vrh.Logger.LogLevel.Warning:
                    if (ex != null)
                    {
                        if (e.Data == null)
                        {
                            e.Data = new Dictionary<string, string>();
                        }
                        e.Data.Add("Exception", Vrh.Logger.LogHelper.GetExceptionInfo(ex));
                    }
                    e.Level = level == Vrh.Logger.LogLevel.Warning ? MessageStackEntryLevel.Warning : MessageStackEntryLevel.Info;
                    lock (_infos)
                    {
                        _infos.DropItem(e);
                    }
                    break;
                case Vrh.Logger.LogLevel.Error:
                case Vrh.Logger.LogLevel.Fatal:
                    if (e.Data == null)
                    {
                        e.Data = new Dictionary<string, string>();
                    }
                    e.Data.Add("Exception", Vrh.Logger.LogHelper.GetExceptionInfo(ex));
                    e.Level = level == Vrh.Logger.LogLevel.Fatal ? MessageStackEntryLevel.FatalError : MessageStackEntryLevel.Error;
                    lock (_errors)
                    {
                        _errors.DropItem(e);
                    }
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// A configurációs osztály 
        /// </summary>
        private readonly PluginsConfig _pluginConfig;

        /// <summary>
        /// A .config fileban ez alatt az app settzings kulcs alatt kell elhelyezni a configurációra vonatkozó információt
        /// </summary>
        private string _configFileSettingKey => $"{MODULEPREFIX}:ConfigurationFile";

        /// <summary>
        /// FixStack a hiba információk gyűjtésére
        /// </summary>
        private readonly FixStack<MessageStackEntry> _errors = new FixStack<MessageStackEntry>(50);

        /// <summary>
        /// FixStack a működési információk gyűjtésére
        /// </summary>
        private readonly FixStack<MessageStackEntry> _infos = new FixStack<MessageStackEntry>(50);

        /// <summary>
        /// Ez a definiált modul azonosító
        /// </summary>
        internal const string MODULEPREFIX = "Vrh.ApplicationContainer.InstanceFactory.FromXML";

        /// <summary>
        /// ???
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, IPlugin> BuildAll()
        {                                    
            throw new NotImplementedException();
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Dictionary<string, IPlugin> BuildAllFromThis(Type type)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="type"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public Dictionary<string, IPlugin> BuildAllFromThisUnderThisVersion(Type type, string version)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  ???
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public IPlugin BuildThis(Type type, string name, string version)
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// part od dispose poattern
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_pluginConfig != null)
                    {
                        // ez az esemény nem létezik az XmlProcesing alatti LinqXmlProcessorbase alatt
                        //_pluginConfig.ConfigProcessorEvent -= _pluginConfig_ConfigProcessorEvent;
                        _pluginConfig.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~IntsnceFactoryFromXMLConfig() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// part of dispose pattern
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
