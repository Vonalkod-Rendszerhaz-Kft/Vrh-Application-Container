using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.ApplicationContainer;
using VRH.Common;

namespace IVConnector.Plugin
{
    /// <summary>
    /// IVConnector ApplicationContainer plugin for Lear ALM system
    /// </summary>
    class IVConnectorPlugin : PluginAncestor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        private IVConnectorPlugin()
        {
            EndLoad();
        }

        /// <summary>
        /// Factory
        /// </summary>
        /// <param name="instanceDefinition">A példány definiciója</param>
        /// <param name="instanceData">Not used in this plugin</param>
        /// <returns></returns>
        public static IVConnectorPlugin IVConnectorPluginFactory(InstanceDefinition instanceDefinition, object instanceData)
        {
            var instance = new IVConnectorPlugin
            {
                _myData = instanceDefinition
            };
            return instance;
        }

        /// <summary>
        /// IPlugin.Start
        /// </summary>
        public override void Start()
        {
            BeginStart();
            // Implement Plugin logic here
            if (_ivConnector != null)
            {
                _ivConnector.Dispose();
            }
            string pluginTmp = _myData.Type.PluginConfig;
            string[] pluginConfigParts = pluginTmp.Split(",".ToCharArray(), StringSplitOptions.None);
            string pluginConfig = string.Empty;
            string pluginMessageDefinitions = string.Empty;
            if (pluginConfigParts.Length > 0)
            {
                pluginConfig = pluginConfigParts[0];
            }
            if (pluginConfigParts.Length > 1)
            {
                pluginMessageDefinitions = pluginConfigParts[1];
            }
            string instanceTmp = _myData.InstanceConfig;
            string[] instanceConfigParts = instanceTmp.Split(",".ToCharArray(), StringSplitOptions.None);
            string instanceConfig = string.Empty;
            string instanceMessageDefinitions = string.Empty;
            if (instanceConfigParts.Length > 0)
            {
                instanceConfig = instanceConfigParts[0];
            }
            if (instanceConfigParts.Length > 1)
            {
                instanceMessageDefinitions = instanceConfigParts[1];
            }
            instanceConfig = !string.IsNullOrEmpty(instanceConfig) 
                                ? instanceConfig 
                                : pluginConfig;
            instanceMessageDefinitions = !string.IsNullOrEmpty(instanceMessageDefinitions) 
                                            ? instanceMessageDefinitions 
                                            : pluginMessageDefinitions;
            instanceConfig = !string.IsNullOrEmpty(instanceConfig) 
                                ? instanceConfig 
                                : "IVConnector.Config.xml/Configuration";
            instanceMessageDefinitions = !string.IsNullOrEmpty(instanceMessageDefinitions) 
                                ? instanceMessageDefinitions 
                                : "IVConnector.Config.xml/MessageDefinitions";                        
            _ivConnector = new IVConnector(instanceConfig, instanceMessageDefinitions, this);
            try
            {
                _ivConnector.Start();
                base.Start();
            }
            catch(Exception ex)
            {
                SetErrorState(ex);
            }            
        }

        /// <summary>
        /// IPlugin.Stop
        /// </summary>
        public override void Stop()
        {
            BeginStop();
            try
            {
                // Implement stop logic here
                if (_ivConnector != null)
                {
                    _ivConnector.Stop();
                    _ivConnector.Dispose();
                    _ivConnector = null;
                }
                base.Stop();
            }
            catch(Exception ex)
            {
                SetErrorState(ex);
            }
            
        }

        /// <summary>
        /// Plugin Instance azonosító
        /// </summary>
        public string InstanceName
        {
            get
            {
                return _myData.Id;
            }
        }

        /// <summary>
        /// Plugin Instance verziója
        /// </summary>
        public string PluginVersion
        {
            get
            {
                return this.GetType().Assembly.Version();
            }
        }

        /// <summary>
        /// IVConnector példány
        /// </summary>
        private IVConnector _ivConnector;

        #region IDisposable Support
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        BeginDispose();
                        // TODO: dispose managed state (managed objects).
                        _ivConnector?.Dispose();
                    }
                    finally
                    {
                        base.Dispose(disposing);
                    }
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TestPlugin() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
