using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.ApplicationContainer;
using VRH.Common;

namespace iSchedulerMonitor
{
    public class MonitorPlugin : PluginAncestor
    {
        #region Privates

        /// <summary>
        /// iSchedulerMonitor példány
        /// </summary>
        private Monitor _monitor;

        #endregion Privates

        #region Properties

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

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        private MonitorPlugin()
        {
            EndLoad();
        }

        #endregion Constructor

        /// <summary>
        /// Factory
        /// </summary>
        /// <param name="instanceDefinition">A példány definiciója</param>
        /// <param name="instanceData">Not used in this plugin</param>
        /// <returns></returns>
        public static MonitorPlugin MonitorPluginFactory(InstanceDefinition instanceDefinition, Object instanceData)
        {
            var instance = new MonitorPlugin();
            instance._myData = instanceDefinition;
            return instance;
        }

        #region Public override methods

        #region Start override method
        /// <summary>
        /// base.BeginStart meghívása, valamint a plugin elindítása.
        /// Vagy példányosítással, vagy egy olyan metódussal, ami elindítja.
        /// Célszerűen ha a példányosítás nem sikerül, az FATAL ERROR.
        /// De ha csak a plugin elindítása nem sikerül, akkor csak HIBA állapotba kerül a plugin.
        /// </summary>
        public override void Start()
        {
            System.Diagnostics.Debug.WriteLine("MonitorPlugin START.");
            BeginStart();
            
            // Implement Plugin logic here
            // Ha netán újra indítják, akkor az előzőt el kell dobni!
            if (_monitor != null) _monitor.Dispose();

            string pluginConfig = String.IsNullOrEmpty(_myData.InstanceConfig) ? _myData.Type.PluginConfig : _myData.InstanceConfig;
            string pluginData = _myData.InstanceData == null ? null : (string)_myData.InstanceData; 
            System.Diagnostics.Debug.WriteLine($"MonitorPlugin pluginConfig={pluginConfig};pluginData={pluginData}");

            _monitor = new Monitor(pluginConfig, pluginData);

            try
            {
                System.Diagnostics.Debug.WriteLine($"MonitorPlugin _monitor.Start");
                _monitor.Start();
                base.Start();
            }
            catch (Exception ex)
            {
                SetErrorState(ex);
            }
        }
        #endregion Start override method

        #region Stop override method
        /// <summary>
        /// base.BeginStop és a Dispose meghívása.
        /// </summary>
        public override void Stop()
        {
            base.BeginStop();
            try
            {
                // Implement stop logic here
                if (_monitor != null)
                {
                    _monitor.Stop();
                    _monitor.Dispose();
                    _monitor = null;
                }
                base.Stop();
            }
            catch (Exception ex)
            {
                SetErrorState(ex);
            }
        }
        #endregion

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
                        _monitor?.Dispose();
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
        #endregion IDisposable Support

        #endregion Public override methods
    }
}
