﻿using System;
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
        /// <summary>
        /// Constructor
        /// </summary>
        private MonitorPlugin()
        {
            EndLoad();
        }

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

        /// <summary>
        /// IPlugin.Start
        /// </summary>
        public override void Start()
        {
            BeginStart();
            // Implement Plugin logic here
            if (_monitor!= null)
            {
                _monitor.Dispose();
            }
            string pluginConfig = String.IsNullOrEmpty(_myData.InstanceConfig) ? _myData.Type.PluginConfig : _myData.InstanceConfig;
            _monitor = new Monitor();

            try
            {
                //_monitor.Start();
                base.Start();
            }
            catch (Exception ex)
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
                if (_monitor != null)
                {
                    //_monitor.Stop();
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
        /// IScheduler példány
        /// </summary>
        private Monitor _monitor;

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
        #endregion

    }
}