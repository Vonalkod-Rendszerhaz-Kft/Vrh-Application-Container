using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using Vrh.ApplicationContainer;
using Vrh.Logger;
using Vrh.DataToRedisCore;


namespace Vrh.DataToRedis
{
    public class DataToRedisPlugin : PluginAncestor
    {
        SQLToRedisControl sqlToRedisControl;

        /// <summary>
        /// Constructor
        /// </summary>
        private DataToRedisPlugin()
        {
            EndLoad();
        }

        /// <summary>
        /// Static Factory (Ha nincs megadva, akkor egy egy paraméteres konstruktort kell implementálni, amely konstruktor paraméterben fogja megkapni a )
        /// </summary>
        /// <param name="instanceDefinition">A példány definiciója</param>
        /// <param name="instanceData">A példánynak átadott adat(ok)</param>
        /// <returns></returns>
        public static DataToRedisPlugin DataToRedisPluginFactory(InstanceDefinition instanceDefinition, Object instanceData)
        {
            var instance = new DataToRedisPlugin();
            instance._myData = instanceDefinition;
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
                // Implement Start logic here 
                string puginConfig = _myData.InstanceConfig;
                if (String.IsNullOrEmpty(puginConfig)) {   puginConfig = _myData.Type.PluginConfig;   }
                int separatorIndex = puginConfig.IndexOf(":") > -1 ? puginConfig.IndexOf(":") : puginConfig.Length;
                string configFile = puginConfig;
                string xmlfilepath = string.Empty;
                if (configFile.Substring(0, 1) == @"\")
                {
                    string configParameterFile = configFile;
                    string assemblyFolder = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    xmlfilepath = assemblyFolder + configParameterFile;
                }
                else {    xmlfilepath = configFile;    }
                var dtrcElement = XElement.Load(xmlfilepath);

                sqlToRedisControl = new SQLToRedisControl(dtrcElement, this.Stop, this);
                var logData = new Dictionary<string, string>();
                logData.Add("Config file", configFile);
                LogThis("DataToRedisPlugin started.", logData, null, LogLevel.Debug, this.GetType());

                base.Start();
            }
            catch (Exception ex) {   SetErrorState(ex);   }
        }

        /// <summary>
        /// IPlugin.Stop
        /// </summary>
        public override void Stop()
        {
            if (MyStatus == PluginStateEnum.Stopping || MyStatus == PluginStateEnum.Loaded) {    return;    }
            BeginStop();
            try
            {
                if (sqlToRedisControl != null) {     sqlToRedisControl.Dispose();    }
                // Implement stop logic here
                LogThis("DataToRedisPlugin stopped.", null, null, LogLevel.Information, this.GetType());
                base.Stop();
            }
            catch (Exception ex) {    SetErrorState(ex);    }
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

