using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vrh.PrintingService.MessageTypes;
using Vrh.ApplicationContainer;
using Vrh.EventHub.Core;
using Vrh.EventHub.Protocols.RedisPubSub;

namespace Vrh.PrintingService
{
    /// <summary>
    /// PrintingServicePlugin
    /// </summary>
    public class PrintingServicePlugin : PluginAncestor
    {
        List<PrinterControl> printerControls = new List<PrinterControl>();

        /// <summary>
        /// Jelzi, hogy a plugin áll
        /// </summary>
        private bool _stopped = true;

        /// <summary>
        /// Constructor
        /// </summary>
        protected PrintingServicePlugin()
        {
            EndLoad();
        }

        /// <summary>
        /// Factory
        /// </summary>
        /// <param name="instanceDefinition">A példány definiciója</param>
        /// <param name="instanceData">Not used in this plugin</param>
        /// <returns></returns>
        public static PrintingServicePlugin PrintingServicePluginFactory(InstanceDefinition instanceDefinition, Object instanceData)
        {
            var instance = new PrintingServicePlugin();
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
                PrintingConfigXmlProcessor pcxp = new PrintingConfigXmlProcessor(subpass, "PrintingConfig", "hu-HU");

                int messageTimeoutmS = pcxp.GetValue("MessageTimeout", pcxp.GetElement("PrintingService"), 1000, true, true);
                int intervalmS = pcxp.GetValue("Interval", pcxp.GetElement("PrintingService"), 1000, true, true);
                int timeToWaitmS = pcxp.GetValue("TimeToWait", pcxp.GetElement("PrintingService"), 500, true, true);
                int maxTry = pcxp.GetValue("MaxTry", pcxp.GetElement("PrintingService"), 3, true, true);
                string pathOfLabels = pcxp.GetValue("PathOfLabels", pcxp.GetElement("PrintingService"), @"C:\Labels", true, true);

                TimeSpan messageTimeout = new TimeSpan(0, 0, 0, 0, messageTimeoutmS);
                TimeSpan interval = new TimeSpan(0, 0, 0, 0, intervalmS);
                TimeSpan timeToWait = new TimeSpan(0, 0, 0, 0, timeToWaitmS);

                IEnumerable<XNode> printerDefinitions = pcxp.GetElement("PrinterDefinitions").Nodes();

                foreach (XNode printerDefinition in printerDefinitions)
                {
                    Printer printer = new Printer( (printerDefinition as XElement).Attribute("Name").Value,
                                                   (printerDefinition as XElement).Attribute("Type").Value,
                                                   (printerDefinition as XElement).Attribute("ConnectionType").Value,
                                                   (printerDefinition as XElement).Attribute("ConnectionString").Value);

                    IEnumerable<XNode> labelDefinitions = pcxp.GetElement("LabelDefinitions").Nodes();

                    foreach (XNode labelDefinition in labelDefinitions)
                    {
                        if ((labelDefinition as XElement).Attribute("PrinterType").Value == printer.Type)
                        {
                            printer.LabelDefinitions.Add(new LabelDefinition((labelDefinition as XElement).Attribute("Name").Value,
                                                                     (labelDefinition as XElement).Attribute("PrinterType").Value,
                                                                     (labelDefinition as XElement).Attribute("Nameseparator").Value
                                                                    ));
                        }
                    }

                    printerControls.Add(new PrinterControl(printer, messageTimeout, interval, timeToWait, maxTry, pathOfLabels));
                }

                //List<Printer> printers = context.Printers.Take(maxPrinter).ToList();

                //for (int i = 0; i < printers.Count; i++)
                //{
                //    printerControls.Add(new PrinterControl(printers[i], interval, timeToWait, maxTry, labelFormat));
                //}

                foreach (PrinterControl printerControl in printerControls)
                {
                    printerControl.StartTimer();
                }

                base.Start();

                EventHubCore.RegisterHandler<RedisPubSubChannel, PrintMessage, MessageResult>("DATA", Print);

                #region Debug print call
                ////Debug idejére
                ////Callhoz kérés összerakása

                //PrintMessage pm = new PrintMessage("REELCHECK_AUTO",
                //                                   "DATA",
                //                                   new Message("REELCHECK_AUTO.prn",
                //                                               null,
                //                                               new Dictionary<string, string>() {
                //                                                                                    { "MACHINE", "12345678"},
                //                                                                                    { "PROGRAM", "87654321"},
                //                                                                                },
                //                                               Modes.SYNC
                //                                              ));
                //try
                //{
                //    MessageResult mr = EventHubCore.Call<RedisPubSubChannel, PrintMessage, MessageResult>("DATA", pm, null);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"Exception:{ex.Message}");
                //}
                #endregion
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
                EventHubCore.DropChannel<RedisPubSubChannel>("DATA");

                foreach (PrinterControl printerControl in printerControls)
                {
                    printerControl.Dispose();
                }
                printerControls = null;

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

        public Response<MessageResult>Print(Request<PrintMessage, MessageResult> message)
        {
            Response<MessageResult> Rmr = message.GetMyResponse;

            try
            {
                PrinterControl printerControl = printerControls.FirstOrDefault(x => x.printer.Name == message.RequestContent.MSGConnectionName);

                if (printerControl == null)
                {
                    throw new Exception("Printer control not found!");
                }
                message.RequestContent.Semaphore = new AutoResetEvent(false);
                printerControl.messageCache.Enqueue(message.RequestContent);

                message.RequestContent.Semaphore.WaitOne(printerControl.MessageTimeout);

                if (message.RequestContent.Exception != null)
                {
                    throw message.RequestContent.Exception;
                }

                if (!message.RequestContent.SendingState)
                {
                    throw new Exception ("Printing timeout!");
                }

                Rmr.ResponseContent = new MessageResult();                
            }
            catch (Exception ex)
            {
                Rmr.Exception = ex;
            }

            return Rmr;
        }
    }
}
