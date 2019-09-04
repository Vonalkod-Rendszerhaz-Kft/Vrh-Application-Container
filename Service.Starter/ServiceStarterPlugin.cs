using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Vrh.ApplicationContainer;
using Vrh.LinqXMLProcessor.Base;
using Vrh.Logger;

namespace Service.Starter
{
    public class ServiceStarterPlugin : PluginAncestor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        private ServiceStarterPlugin()
        {
            EndLoad();
        }

        /// <summary>
        /// Factory
        /// </summary>
        /// <param name="instanceDefinition">A példány definiciója</param>
        /// <param name="instanceData">Not used in this plugin</param>
        /// <returns></returns>
        public static ServiceStarterPlugin ServiceStarterPluginFactory(InstanceDefinition instanceDefinition, object instanceData)
        {
            var instance = new ServiceStarterPlugin
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
            if (MyStatus == PluginStateEnum.Starting || MyStatus == PluginStateEnum.Running) {    return;    }
            BeginStart();
            try
            {
                string configParameterFile = _myData.InstanceConfig;
                if (string.IsNullOrEmpty(configParameterFile))  {    configParameterFile = _myData.Type.PluginConfig;   }
                Configuration = new ServiceStarterParameterFileProcessor(configParameterFile);
                Configuration.ConfigProcessorEvent += ConfigProcessorEvent;
                _lazyRedisConnection = new Lazy<ConnectionMultiplexer>(RedisConnectionInitializer, LazyThreadSafetyMode.PublicationOnly);
                foreach (var service in Configuration.AllControlledServices) {   _controlledServices.Add(new ControlledService(service, this));    }
                base.Start();
            }
            catch (Exception ex) {   SetErrorState(ex);   }
        }

        /// <summary>
        /// IPlugin.Stop
        /// </summary>
        public override void Stop()
        {
            if (MyStatus == PluginStateEnum.Stopping || MyStatus == PluginStateEnum.Loaded)
            { 
                return;
            }
            BeginStop();
            try
            {
                // Implement stop logic here
                foreach (var service in _controlledServices)
                {
                    service.Dispose();
                }
                _controlledServices.RemoveAll(x => true);
                if (RedisConnection != null)
                {
                    lock (RedisConnection)
                    {
                        _lazyRedisConnection.Value.Close(true);
                        _lazyRedisConnection.Value.Dispose();
                        _lazyRedisConnection = null;
                        _lazyRedisConnection = new Lazy<ConnectionMultiplexer>(() => { return null; });
                    }
                }
                base.Stop();
            }
            catch (Exception ex)
            {
                SetErrorState(ex);
            }
        }

        /// <summary>
        /// A plugin konfigurációja
        /// </summary>
        internal ServiceStarterParameterFileProcessor Configuration { get; private set; }

        /// <summary>
        /// A konfiguráció feldolgozás üzeneteit logoló metódus
        /// </summary>
        /// <param name="e">Eseményargumentum</param>
        private void ConfigProcessorEvent(Vrh.LinqXMLProcessor.Base.ConfigProcessorEventArgs e)
        {
            LogLevel level =
                e.Exception.GetType().Name == typeof(ConfigProcessorWarning).Name
                    ? LogLevel.Warning
                    : LogLevel.Error;
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "ConfigProcessor class", e.ConfigProcessor },
                { "Config file", e.ConfigFile },
            };
            LogThis($"Configuration issue: {e.Message}", data, e.Exception, level);
        }

        /// <summary>
        /// A kontrolált servicek listája
        /// </summary>
        private List<ControlledService> _controlledServices = new List<ControlledService>();

        /// <summary>
        /// Jelzi, hogy a plugin áll
        /// </summary>
        //private bool _stopped = true;

        /// <summary>
        /// Lazy StackExchange.Redis connection multiplexer
        /// </summary>
        private Lazy<ConnectionMultiplexer> _lazyRedisConnection;
        /// <summary>
        /// Redis connection
        /// </summary>
        internal ConnectionMultiplexer RedisConnection
        {
            get
            {
                return _lazyRedisConnection.Value;
            }
        }
        private ConnectionMultiplexer RedisConnectionInitializer()
        {
            if (!string.IsNullOrEmpty(Configuration.RedisConnection))
            {
                for (int i = 1; i < Configuration.RedisconnectRetries; i++)
                {
                    try
                    {
                        var cm = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(Configuration.RedisConnection, true));
                        cm.PreserveAsyncOrder = false;
                        return cm;
                    }
                    catch (Exception ex)
                    {
                        LogThis($"Attempting ({i} of {Configuration.RedisconnectRetries}) to establish connection to Redis server {Configuration.RedisConnection}", null, ex, LogLevel.Warning, this.GetType());
                    }
                }
                string text = $"Error to connect to Redis server {Configuration.RedisConnection}. All {Configuration.RedisconnectRetries} connect attempt failed.";
                LogThis(text, null, null, LogLevel.Fatal, this.GetType());
                throw new Exception(text);
            }
            else
            {
                string text = $"No Redis connection string is specified. No connection to Redis server {Configuration.RedisConnection} is established.";
                LogThis(text, null, null, LogLevel.Fatal, this.GetType());
                throw new Exception(text);
            }
        }

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
                        Stop();
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
