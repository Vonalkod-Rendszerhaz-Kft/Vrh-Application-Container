using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vrh.CameraService.MessageTypes;
using Vrh.ApplicationContainer;

namespace Vrh.CameraService
{
    /// <summary>
    /// CameraServicePlugin
    /// </summary>
    public class CameraServicePlugin : PluginAncestor
    {
        List<CameraControl> cameraControls = new List<CameraControl>();

        /// <summary>
        /// Jelzi, hogy a plugin áll
        /// </summary>
        private bool _stopped = true;

        /// <summary>
        /// Constructor
        /// </summary>
        protected CameraServicePlugin()
        {
            EndLoad();
        }

        /// <summary>
        /// Factory
        /// </summary>
        /// <param name="instanceDefinition">A példány definiciója</param>
        /// <param name="instanceData">Not used in this plugin</param>
        /// <returns></returns>
        public static CameraServicePlugin CameraServicePluginFactory(InstanceDefinition instanceDefinition, Object instanceData)
        {
            var instance = new CameraServicePlugin();
            instance._myData = instanceDefinition;
            return instance;
        }

        /// <summary>
        /// IPlugin.Start
        /// </summary>
        public override void Start()
        {
            if (MyStatus == PluginStateEnum.Starting || MyStatus == PluginStateEnum.Running)
            {
                return;
            }
            BeginStart();
            try
            {
                // Implement Start logic here 
                string configParameterFile = _myData.InstanceConfig;
                if (String.IsNullOrEmpty(configParameterFile))
                {
                    configParameterFile = _myData.Type.PluginConfig;
                }

                string assemblyFolder = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string subpass = assemblyFolder + configParameterFile;
                CameraConfigXmlProcessor ccxp = new CameraConfigXmlProcessor(subpass, "CameraConfig", "hu-HU");

                int messageTimeoutmS = ccxp.GetValue("MessageTimeout", ccxp.GetElement("CameraService"), 1000, true, true);
                int intervalmS = ccxp.GetValue("Interval", ccxp.GetElement("CameraService"), 100, true, true);
                int timeToWaitmS = ccxp.GetValue("TimeToWait", ccxp.GetElement("CameraService"), 500, true, true);
                int maxTry = ccxp.GetValue("MaxTry", ccxp.GetElement("CameraService"), 3, true, true);

                TimeSpan messageTimeout = new TimeSpan(0, 0, 0, 0, messageTimeoutmS);
                TimeSpan interval = new TimeSpan(0, 0, 0, 0, intervalmS);
                TimeSpan timeToWait = new TimeSpan(0, 0, 0, 0, timeToWaitmS);

                IEnumerable<XNode> cameraDefinitions = ccxp.GetElement("CameraDefinitions").Nodes();

                foreach (XNode cameraDefinition in cameraDefinitions)
                {
                    Camera camera = new Camera( (cameraDefinition as XElement).Attribute("Name").Value,
                                                   (cameraDefinition as XElement).Attribute("Type").Value,
                                                   (cameraDefinition as XElement).Attribute("Protocol").Value,
                                                   bool.Parse((cameraDefinition as XElement).Attribute("EnableInterventions").Value),
                                                   bool.Parse((cameraDefinition as XElement).Attribute("EnableIOEXT").Value),
                                                   (cameraDefinition as XElement).Attribute("ConnectionType").Value,
                                                   (cameraDefinition as XElement).Attribute("ConnectionString").Value);

                    cameraControls.Add(new CameraControl(camera, messageTimeout, interval, timeToWait, maxTry));
                }

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
            if (MyStatus == PluginStateEnum.Stopping || MyStatus == PluginStateEnum.Loaded)
            {
                return;
            }
            BeginStop();
            try
            {
                // Implement stop logic here

                foreach (CameraControl cameraControl in cameraControls)
                {
                    cameraControl.Dispose();
                }
                cameraControls = null;

                base.Stop();
            }
            catch (Exception ex)
            {
                SetErrorState(ex);
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
