using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using VRH.Common;
using Vrh.ApplicationContainer;
using IVConnector.Plugin.InterventionService;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.Xml.Linq;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.IO;
using Vrh.LinqXMLProcessor.Base;
using Vrh.Logger;
using System.Messaging;

namespace IVConnector.Plugin
{
    internal class IVConnector : IDisposable
    {
        /// <summary>
        /// Creates the comm. class instance, reads the configuration. Rethrows any exceptions after logging the errors.
        /// </summary>
        public IVConnector(string parameterFile, IVConnectorPlugin pluginReference)
        {
            _pluginReference = pluginReference;
            _configuration = new IVConnectorParameterFileProcessor(parameterFile);
            _configuration.ConfigProcessorEvent += _configuration_ConfigProcessorEvent;
            _msgPrefix = _configuration.MessagePrefix;
            _msgSuffix = _configuration.MessageSuffix;
            _idSeparator = _configuration.IDSeparator;
            _paramSeparator = _configuration.ParameterSeparator;
        }

        /// <summary>
        /// Starts the listening process and returns immediately. Watch the log for errors.
        /// </summary>
        public void Start()
        {
            try
            {
                if (_process == null)
                {
                    if (_configuration.ConnectorType == IVConnectorType.TCP)
                    {
                        IPAddress ip;
                        if (!IPAddress.TryParse(_configuration.IP, out ip))
                        {
                            throw new FatalException("Configuration Error: invalid IP address.", null, null);
                        }
                        try
                        {
                            if (_listener != null)
                            {
                                _listener.Stop();
                                _listener = null;
                            }
                            _listener = new TcpListener(IPAddress.Parse(_configuration.IP), _configuration.Port);
                            _listener.Start();
                        }
                        catch (Exception ex)
                        {
                            _listener.Stop();
                            _listener = null;
                            throw new FatalException("TCP Listener not started!", ex,
                                                                        new KeyValuePair<string, string>("IVConnector plugin instance", _pluginReference.InstanceName),
                                                                        new KeyValuePair<string, string>("IVConnector plugin version", _pluginReference.PluginVersion),
                                                                        new KeyValuePair<string, string>("Configuration file", _configuration.ConfigurationFileDefinition),
                                                                        new KeyValuePair<string, string>("IP value", _configuration.IP),
                                                                        new KeyValuePair<string, string>("Port value", _configuration.Port.ToString())
                                                                        );
                        }
                    }
                    ThreadStart ts = new ThreadStart(ListenerProcess);
                    _process = new Thread(ts);
                    _process.Name = String.Format("IVConnector Process Thread ({0})", _pluginReference.InstanceName);
                    _process.Start();
                    _stopped = false;
                    byte[] ba;
                    string hexString;
                    ba = Encoding.Default.GetBytes(_msgSuffix);
                    hexString = BitConverter.ToString(ba);
                    string msgSuffix = !String.IsNullOrEmpty(_msgSuffix) ? String.Format("{0} ({1})", _msgSuffix, hexString.Replace("-", "")) : "";
                    ba = Encoding.Default.GetBytes(_msgPrefix);
                    hexString = BitConverter.ToString(ba);
                    string msgPrefix = !String.IsNullOrEmpty(_msgPrefix) ? String.Format("{0} ({1})", _msgPrefix, hexString.Replace("-", "")) : "";
                    ba = Encoding.Default.GetBytes(_idSeparator);
                    hexString = BitConverter.ToString(ba);
                    string idSeparator = !String.IsNullOrEmpty(_idSeparator) ? String.Format("{0} ({1})", _idSeparator, hexString.Replace("-", "")) : "";
                    ba = Encoding.Default.GetBytes(_paramSeparator);
                    hexString = BitConverter.ToString(ba);
                    string paramSeparator = !String.IsNullOrEmpty(_paramSeparator) ? String.Format("{0} ({1})", _paramSeparator, hexString.Replace("-", "")) : "";
                    var data = new Dictionary<string, string>()
                    {
                        { "IVConnector plugin instance", _pluginReference.InstanceName },
                        { "IVConnector plugin version", _pluginReference.PluginVersion },
                        { "Used configuration file", _configuration.ConfigurationFileDefinition },
                        { "Connector type", _configuration.ConnectorType.ToString() },
                    };
                    if (_configuration.ConnectorType == IVConnectorType.TCP)
                    {
                        data.Add("Watcher IP", _configuration.IP);
                        data.Add("Watcher Port", _configuration.Port.ToString());
                        data.Add("Message prefix", msgPrefix);
                        data.Add("Message suffix", msgSuffix);
                        data.Add("Intervention Id separator", idSeparator);
                        data.Add("Intervention Parameter separator", paramSeparator);
                    }
                    else
                    {
                        data.Add("Input queue", _configuration.InQueue);
                        if (!String.IsNullOrEmpty(_configuration.ResponseQueue))
                        {
                            data.Add("Response handling", true.ToString());
                            data.Add("Response queue", _configuration.ResponseQueue);
                            data.Add("Response label", _configuration.ResponseLabel);
                            data.Add("Message id handling type", _configuration.IdHandling.ToString());
                        }
                        data.Add("Input message label filtering", _configuration.LabelFilters);
                        data.Add("Intervention Id separator", _configuration.IDSeparator);
                        data.Add("Intervention parameter separator", _configuration.ParameterSeparator);
                    }
                    _pluginReference.LogThis("IVConnector instance seccessfull started and watching!", data, null, Vrh.Logger.LogLevel.Information, this.GetType());
                }
            }
            catch (Exception ex)
            {
                throw new FatalException("IVConnector not started!", ex,
                                    new KeyValuePair<string, string>("IVConnector plugin instance", _pluginReference.InstanceName),
                                    new KeyValuePair<string, string>("IVConnector plugin version", _pluginReference.PluginVersion),
                                    new KeyValuePair<string, string>("Configuration file", _configuration.ConfigurationFileDefinition),
                                    new KeyValuePair<string, string>("IP value", _configuration.IP),
                                    new KeyValuePair<string, string>("Port value", _configuration.Port.ToString())
                                  );
            }
        }

        /// <summary>
        /// Stops the listening process.
        /// </summary>
        public void Stop()
        {
            try
            {
                if (_process != null)
                {
                    _stopped = true;
                    if (_listener != null)
                    {
                        _listener.Stop();
                        _listener = null;
                    }
                    _process = null;
                    var data = new Dictionary<string, string>()
                    {
                        { "IVConnector plugin instance", _pluginReference.InstanceName },
                        { "IVConnector plugin version", _pluginReference.PluginVersion },
                    };
                    _pluginReference.LogThis("IVConnector instance seccessfull stoped!", data, null, Vrh.Logger.LogLevel.Information, this.GetType());
                }
            }
            catch (Exception ex)
            {
                throw new FatalException("IVConnector stop error!", ex, null);
            }
        }

        /// <summary>
        /// Listener process. In an ifinite loop, receives sockets and sends to threadpool for processing.
        /// </summary>
        private void ListenerProcess()
        {
            while (!disposed && !_stopped)
            {
                if (_configuration.ConnectorType == IVConnectorType.TCP)
                {
                    // TCP
                    if (_listener != null)
                    {
                        try
                        {
                            Socket s = _listener.AcceptSocket();
                            ThreadPool.QueueUserWorkItem(TCPProcessMessage, s);
                        }
                        catch { }
                    }
                }
                else
                {
                    // MSMQ
                    ProcessMSMQ();
                }
            }
        }

        /// <summary>
        /// Feldolgozza a bejövő queban található feldolgozandó üzeneteket
        /// </summary>
        private void ProcessMSMQ()
        {
            try
            {
                if (!MessageQueue.Exists(_configuration.InQueue))
                {
                    throw new FatalException(String.Format("Message queue not exists! {0}", _configuration.InQueue));
                }
            }
            catch(Exception ex)
            {
                Stop();
                _pluginReference.SetErrorState(new FatalException(String.Format("Error access input Message queue: {0}", _configuration.InQueue), ex));
                return;
            }
            string[] filters = _configuration.LabelFilters.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
            using (var receiveQueue = new MessageQueue(_configuration.InQueue))
            {
                if (_configuration.IdHandling == MSMQIdHandling.CorrelationId)
                {
                    receiveQueue.MessageReadPropertyFilter.CorrelationId = true;
                }
                using (var e = receiveQueue.GetMessageEnumerator2())
                {
                    while (!disposed && !_stopped && e.MoveNext(new TimeSpan(0, 0, 10)))
                    {
                        System.Messaging.Message msg = e.Current;
                        if (msg != null)
                        {
                            bool processThis = true;
                            foreach (var filter in filters)
                            {
                                if (!String.IsNullOrEmpty(filter))
                                {
                                    if (filter.StartsWith("!"))
                                    {
                                        if (msg.Label.Contains(filter))
                                        {
                                            processThis = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (!msg.Label.Contains(filter))
                                        {
                                            processThis = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (processThis)
                            {
                                msg.Formatter = new ActiveXMessageFormatter();
                                msg.BodyType = 0;
                                StreamReader reader = new StreamReader(msg.BodyStream, Encoding.Default);
                                string msgBody = reader.ReadToEnd();
                                string response = ProcessMSMQMessage(msgBody);
                                if (!String.IsNullOrEmpty(_configuration.ResponseQueue))
                                {
                                    using (var responseQueue = new MessageQueue(_configuration.ResponseQueue))
                                    {
                                        using (System.Messaging.Message resmsg = new System.Messaging.Message())
                                        {
                                            resmsg.Formatter = new ActiveXMessageFormatter();
                                            string label = _configuration.ResponseLabel;
                                            if (_configuration.IdHandling == MSMQIdHandling.Label)
                                            {
                                                label = String.Format(label, msg.Label);
                                            }
                                            if (_configuration.IdHandling == MSMQIdHandling.CorrelationId)
                                            {
                                                resmsg.CorrelationId = msg.CorrelationId;
                                            }
                                            resmsg.Label = label;
                                            resmsg.BodyStream = new MemoryStream(Encoding.UTF8.GetBytes(response));
                                            //resmsg.UseDeadLetterQueue = true;
                                            //resmsg.TimeToBeReceived = TimeSpan.FromMinutes(settings.MSMQReceiveTimout);                                            
                                            try
                                            {
                                                responseQueue.Send(resmsg);
                                            }
                                            catch(Exception ex)
                                            {
                                                Dictionary<string, string> data = new Dictionary<string, string>()
                                                {
                                                    { "Output Queue", _configuration.ResponseQueue },
                                                };
                                                _pluginReference.LogThis(String.Format("Error occured when send response in MSMQ!"), null, ex, LogLevel.Error, this.GetType());
                                            }
                                        }
                                    }
                                }
                                e.RemoveCurrent();
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Feldolgozz egy MSMQ-ból kivett üzenetet
        /// </summary>
        /// <param name="messageBody">Az üzenet tartalma</param>
        /// <returns></returns>
        private string ProcessMSMQMessage(string messageBody)
        {
            var data = new Dictionary<string, string>()
                    {
                        { "IVConnector plugin instance", _pluginReference.InstanceName },
                        { "IVConnector plugin version", _pluginReference.PluginVersion },
                        { "Used configuration file", _configuration.ConfigurationFileDefinition },
                    };
            try
            {
                return ProcessData(data, messageBody);
            }
            catch (Exception e)
            {
                _pluginReference.LogThis("IVConnector message processing error occured!", data, e, Vrh.Logger.LogLevel.Error, this.GetType());
                return e.Message;
            }
        }

        /// <summary>
        /// Processes a socket with its message.
        /// </summary>
        /// <param name="state">The Accepted socket which must be processed</param>
        private void TCPProcessMessage(object state)
        {
            var data = new Dictionary<string, string>()
                    {
                        { "IVConnector plugin instance", _pluginReference.InstanceName },
                        { "IVConnector plugin version", _pluginReference.PluginVersion },
                        { "Used configuration file", _configuration.ConfigurationFileDefinition },
                    };
            Socket s = state as Socket;
            byte[] buffer = new byte[_maxReceiveBufferSize];
            try
            {
                // Setting up timeouts
                s.ReceiveTimeout = _socketTimeout;
                s.SendTimeout = _socketTimeout;
                _pluginReference?.LogThis("Receiving msg...", data, null, Vrh.Logger.LogLevel.Debug, this.GetType());
                int rcvd = 0;
                try
                {
                    rcvd = s.Receive(buffer);
                }
                catch
                {
                    _pluginReference?.LogThis("ERROR: No data received on socket.", data, null, Vrh.Logger.LogLevel.Warning, this.GetType());
                    return;
                }
                string message = Encoding.ASCII.GetString(buffer, 0, rcvd);
                _pluginReference.LogThis(String.Format("Receive message: {0}", message), data, null, Vrh.Logger.LogLevel.Information, this.GetType());
                if (!message.StartsWith(_msgPrefix) && !message.EndsWith(_msgSuffix))
                {
                    throw new Exception("ERROR: Bad prefix or suffix in message!");
                }
                // Throwing out prefix and suffix
                message = message.Substring(_msgPrefix.Length, message.Length - _msgSuffix.Length);
                if (message.Length == 0)
                {
                    throw new Exception("ERROR: Empty message received!");
                }
                string result = ProcessData(data, message);
                result = string.Empty;
                if (String.IsNullOrEmpty(result))
                {
                    s.Send(Encoding.ASCII.GetBytes(_configuration.Ack));
                }
                else
                {
                    s.Send(Encoding.ASCII.GetBytes("/x15".FromHexOrThis() + result + _configuration.Ack));
                }
            }
            catch (Exception e)
            {
                _pluginReference.LogThis("IVConnector message processing error occured!", data, e, Vrh.Logger.LogLevel.Error, this.GetType());
                s.Send(Encoding.ASCII.GetBytes("/x15".FromHexOrThis() + e.Message + _configuration.Ack));
            }
            finally
            {
                try
                {
                    s.Close();
                }
                catch { }
            }
        }

        private string ProcessData(Dictionary<string, string> data, string message)
        {
            string[] parts = message.Split(new string[] { _idSeparator }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new Exception("ERROR: Cannot find or more than one ID.");
                //_pluginReference.LogThis("ERROR: Cannot find or more than one ID.", data, null, Vrh.Logger.LogLevel.Warning, this.GetType());
                //return;
            }
            string assemblyLineID = parts[0];
            _pluginReference.LogThis(string.Format("Assembly Line ID : {0}", assemblyLineID), data, null, Vrh.Logger.LogLevel.Debug, this.GetType());
            string[] pars = parts[1].Split(_paramSeparator.ToCharArray());
            string interventionName = _configuration.GetIterventionFromID(pars[0]);
            if (String.IsNullOrEmpty(interventionName))
            {
                throw new Exception(string.Format("ERROR: Invalid message name: {0}.", pars[0]));
                //_pluginReference.LogThis(string.Format("ERROR: Invalid message name: {0}.", pars[0]), data, null, Vrh.Logger.LogLevel.Warning, this.GetType());
                //return;
            }
            InterventionServiceClient interventionService = new InterventionServiceClient();
            Intervention intervention = new Intervention();
            intervention.Name = interventionName;
            int asId = -1;
            foreach (var item in interventionService.GetInterventionedObject(null))
            {
                string[] splitedName = item.ObjectLabel.Split(new string[] { " (" }, StringSplitOptions.RemoveEmptyEntries);
                string name = null;
                if (splitedName.Count() > 0)
                {
                    name = splitedName[0];
                }
                if (splitedName.Count() > 1)
                {
                    string key = splitedName[1];
                    if (key == String.Format("{0})", assemblyLineID) || name == assemblyLineID)
                    {
                        asId = item.ObjectID;
                        break;
                    }
                }
            }
            if (asId == -1)
            {
                throw new Exception(string.Format("ERROR: Unknown Assembly line: {0}!", assemblyLineID));
                //_pluginReference.LogThis("ERROR: Unknown Assembly line: {0}!", data, null, Vrh.Logger.LogLevel.Warning, this.GetType());
                //return;
            }

            InterventionDefination iv = null;
            foreach (var item in interventionService.GetAllIntervention(null))
            {
                if (((InterventionDefination)item).Name == interventionName)
                {
                    iv = (InterventionDefination)item;
                    break;
                }
            }
            if (iv == null)
            {
                throw new Exception(string.Format("ERROR: Unknown intervention: {0}!", interventionName));
            }
            _pluginReference.LogThis(pars[0], data, null, Vrh.Logger.LogLevel.Information);
            if (pars.Count() - 1 != iv.ParameterList.Count())
            {
                throw new Exception(string.Format("Wrong parameter count! Received: {0}, Expected: {1}", pars.Count() - 1, iv.ParameterList.Count()));
            }
            intervention.ObjectID = asId;
            intervention.Parameters = new Dictionary<string, object>();
            int i = 1;
            foreach (var item in iv.ParameterList)
            {
                ParameterDefinition parameter = (ParameterDefinition)item;
                Object parameterValue;
                switch (((DataType)parameter.ParameterType))
                {
                    case DataType.Boolean:
                        bool bValue = false;
                        if (!bool.TryParse(pars[i], out bValue))
                        {
                            throw new Exception(string.Format("Wrong parameter format! Value: {0}, Expected type: {1}", pars[i], "Boolean"));
                        }
                        parameterValue = bValue;
                        break;
                    case DataType.Int32:
                        int iValue = 0;
                        if (!int.TryParse(pars[i], out iValue))
                        {
                            throw new Exception(string.Format("Wrong parameter format! Value: {0}, Expected type: {1}", pars[i], "Int32"));
                        }
                        parameterValue = iValue;
                        break;
                    case DataType.Double:
                        double dValue = 0;
                        if (!double.TryParse(pars[i], out dValue))
                        {
                            throw new Exception(string.Format("Wrong parameter format! Value: {0}, Expected type: {1}", pars[i], "Double"));
                        }
                        parameterValue = dValue;
                        break;
                    case DataType.DateTime:
                        DateTime dtValue;
                        if (!DateTime.TryParse(pars[i], out dtValue))
                        {
                            throw new Exception(string.Format("Wrong parameter format! Value: {0}, Expected type: {1}", pars[i], "DateTime"));
                        }
                        parameterValue = dtValue;
                        break;
                    default:
                        parameterValue = pars[i];
                        break;
                }
                intervention.Parameters.Add(parameter.Name, parameterValue);
                i++;
            }
            Guid user = _configuration.UserGuid;
            data.Add("User guid", user.ToString());
            string result = interventionService.DoIntervention(intervention, user, null);
            _pluginReference.LogThis(string.Format("IVConnector: Intervention result: {0}", result), data, null, Vrh.Logger.LogLevel.Debug);
            return result;
        }

        /// <summary>
        /// Config processor események logolása
        /// </summary>
        /// <param name="e">eseményargumentum</param>
        private void _configuration_ConfigProcessorEvent(ConfigProcessorEventArgs e)
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
            _pluginReference.LogThis(String.Format("Configuration issue: {0}", e.Message), data, e.Exception, level);
        }

        private IVConnectorPlugin _pluginReference = null;

        private static int _socketTimeout = 5000;
        private static int _maxReceiveBufferSize = 200;

        private Thread _process;
        private IVConnectorParameterFileProcessor _configuration;

        private string _msgPrefix;
        private string _msgSuffix;
        private string _idSeparator;
        private string _paramSeparator;

        private TcpListener _listener;
        //private InterventionService _referencToInterventionService;

        /// <summary>
        /// Jelzi, hogy a connector nincs elindított állapotban
        /// </summary>
        private bool _stopped = true;

        #region IDisposable Members

        public bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~IVConnector()
        {
            Dispose(false);
        }

        // hibavédett kell, hogy legyen
        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // események nullázása                    
                    // Dispose managed resources.
                    try
                    {
                        disposed = true;
                        Stop();
                        _configuration.ConfigProcessorEvent -= _configuration_ConfigProcessorEvent;
                        _configuration.Dispose();
                    }
                    catch { }
                }
            }
        }

        #endregion
    }
}
