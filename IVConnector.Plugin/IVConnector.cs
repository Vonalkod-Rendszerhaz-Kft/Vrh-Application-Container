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
        public IVConnector(string configParameterFile, string messagesParameterFile, IVConnectorPlugin pluginReference)
        {
            _pluginReference = pluginReference;
            _configuration = new IVConnectorParameterFileProcessor(configParameterFile);
            _configuration.ConfigProcessorEvent += _configuration_ConfigProcessorEvent;
            _messagesConfiguration = new MessageDefinitionsParameterFileProcessor(messagesParameterFile);
            _messagesConfiguration.ConfigProcessorEvent += _configuration_ConfigProcessorEvent;
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
                    byte[] ba;
                    string hexString;
                    ba = Encoding.Default.GetBytes(_configuration.MessageSuffix);
                    hexString = BitConverter.ToString(ba);
                    string msgSuffix = !String.IsNullOrEmpty(_configuration.MessageSuffix) ? String.Format("{0} ({1})", _configuration.MessageSuffix, hexString.Replace("-", "")) : "";
                    ba = Encoding.Default.GetBytes(_configuration.MessagePrefix);
                    hexString = BitConverter.ToString(ba);
                    string msgPrefix = !String.IsNullOrEmpty(_configuration.MessagePrefix) ? String.Format("{0} ({1})", _configuration.MessagePrefix, hexString.Replace("-", "")) : "";
                    ba = Encoding.Default.GetBytes(_configuration.IDSeparator);
                    hexString = BitConverter.ToString(ba);
                    string idSeparator = !String.IsNullOrEmpty(_configuration.IDSeparator) ? String.Format("{0} ({1})", _configuration.IDSeparator, hexString.Replace("-", "")) : "";
                    ba = Encoding.Default.GetBytes(_configuration.FieldSeparator);
                    hexString = BitConverter.ToString(ba);
                    string paramSeparator = !String.IsNullOrEmpty(_configuration.FieldSeparator) ? String.Format("{0} ({1})", _configuration.FieldSeparator, hexString.Replace("-", "")) : "";
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
                        data.Add("Intervention parameter separator", _configuration.FieldSeparator);
                    }
                    _pluginReference.LogThis($"Instance successfull started and watching!", data, null, Vrh.Logger.LogLevel.Information, this.GetType());
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
                    _pluginReference.LogThis($"Instance successfull stoped!", data, null, Vrh.Logger.LogLevel.Information, this.GetType());
                }
            }
            catch (Exception ex)
            {
                throw new FatalException($"Stop error!", ex, null);
            }
        }

        /// <summary>
        /// Listener process. In an ifinite loop, receives sockets and sends to threadpool for processing.
        /// </summary>
        private void ListenerProcess()
        {
            VrhLogger.Log<string>($"ListenerProcess started: {Thread.CurrentThread.Name}", null, null, LogLevel.Debug, this.GetType());
            _stopped = false;
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
                        catch(Exception ex)
                        {
                            VrhLogger.Log(ex, this.GetType(), LogLevel.Error);
                        }
                    }
                }
                else
                {
                    // MSMQ
                    ProcessMSMQ();
                }
            }
            var logData = new Dictionary<string, string>();
            logData.Add("disposed", disposed.ToString());
            logData.Add("stopped", _stopped.ToString());
            VrhLogger.Log<string>($"ListenerProcess exited: {Thread.CurrentThread.Name}", logData, null, LogLevel.Debug, this.GetType());
        }

        /// <summary>
        /// Feldolgozza a bejövő queban található feldolgozandó üzeneteket
        /// </summary>
        private void ProcessMSMQ()
        {
            var logData = new Dictionary<string, string>()
                    {
                        { "IVConnector plugin instance", _pluginReference.InstanceName },
                        { "IVConnector plugin version", _pluginReference.PluginVersion },
                        { "Used configuration file", _configuration.ConfigurationFileDefinition },
                    };
            try
            {
                if (!MessageQueue.Exists(_configuration.InQueue))
                {
                    throw new FatalException(String.Format("Message queue not exists! {0}", _configuration.InQueue));
                }
            }
            catch (Exception ex)
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
                                switch (_configuration.MsmqFormatter)
                                {
                                    case MSMQFormatter.ActiveXMessageFormatter:
                                        msg.Formatter = new ActiveXMessageFormatter();
                                        break;
                                    case MSMQFormatter.XmlMessageFormatter:
                                        msg.Formatter = new XmlMessageFormatter();
                                        break;
                                    default:
                                        break;
                                }
                                msg.BodyType = 0;
                                StreamReader reader;
                                switch (_configuration.Encoding)
                                {
                                    case MyEncoding.Default:
                                        reader = new StreamReader(msg.BodyStream, Encoding.Default);
                                        break;
                                    case MyEncoding.UTF8:
                                        reader = new StreamReader(msg.BodyStream, Encoding.UTF8);
                                        break;
                                    case MyEncoding.UTF7:
                                        reader = new StreamReader(msg.BodyStream, Encoding.UTF7);
                                        break;
                                    case MyEncoding.UTF32:
                                        reader = new StreamReader(msg.BodyStream, Encoding.UTF32);
                                        break;
                                    case MyEncoding.Unicode:
                                        reader = new StreamReader(msg.BodyStream, Encoding.Unicode);
                                        break;
                                    case MyEncoding.BigEndianUnicode:
                                        reader = new StreamReader(msg.BodyStream, Encoding.BigEndianUnicode);
                                        break;
                                    case MyEncoding.ASCII:
                                        reader = new StreamReader(msg.BodyStream, Encoding.ASCII);
                                        break;
                                    default:
                                        reader = new StreamReader(msg.BodyStream, Encoding.Default);
                                        break;
                                }
                                string msgBody = reader.ReadToEnd();
                                bool harnessLogging = false;
                                string response = String.Empty;
                                try
                                {
                                    response = ProcessMessage(msgBody, logData, out harnessLogging);
                                }
                                catch (EndpointNotFoundException ex)
                                {
                                    response = $"ALM side WCF service currently not available!";
                                    _pluginReference.LogThis(response, logData, null, Vrh.Logger.LogLevel.Error, this.GetType());
                                    //Ennek a hívásnak nincs értelme, mert ez is a WCF-et használja!
                                    //if (harnessLogging) {CallErrorLogging(msgBody, response, 1);}
                                }
                                catch (System.TimeoutException ex)
                                {
                                    response = $"ALM side WCF service response timeout exceeded!";
                                    _pluginReference.LogThis(response, logData, null, Vrh.Logger.LogLevel.Error, this.GetType());
                                    //Ennek a hívásnak nincs értelme, mert ez is a WCF-et használja!
                                    //if (harnessLogging) {CallErrorLogging(msgBody, response, 1);}
                                }
                                catch (Exception ex)
                                {
                                    response = $"Message processing error occured!";
                                    _pluginReference.LogThis(response, logData, ex, LogLevel.Error, this.GetType());
                                    response += " " + ex.Message;
                                    if (harnessLogging){CallErrorLogging(msgBody, ex.Message, 2);}
                                }

                                if (!String.IsNullOrEmpty(_configuration.ResponseQueue))
                                {
                                    using (var responseQueue = new MessageQueue(_configuration.ResponseQueue))
                                    {
                                        using (System.Messaging.Message resmsg = new System.Messaging.Message())
                                        {
                                            switch (_configuration.MsmqFormatter)
                                            {
                                                case MSMQFormatter.ActiveXMessageFormatter:
                                                    resmsg.Formatter = new ActiveXMessageFormatter();
                                                    break;
                                                case MSMQFormatter.XmlMessageFormatter:
                                                    resmsg.Formatter = new XmlMessageFormatter();
                                                    break;
                                                default:
                                                    break;
                                            }
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
                                            try
                                            {
                                                responseQueue.Send(resmsg);
                                            }
                                            catch (Exception ex)
                                            {
                                                Dictionary<string, string> data = new Dictionary<string, string>()
                                                {
                                                    { "Output Queue", _configuration.ResponseQueue },
                                                };
                                                _pluginReference.LogThis($"Error occured when send response in MSMQ!", null, ex, LogLevel.Error, this.GetType());
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
        /// Loggol egy hibát a WCF sevicen
        /// </summary>
        /// <param name="fullMessage">a feldolgozott üzenet</param>
        /// <param name="errorTxt">hibaszöveg</param>
        /// <param name="src">forrás: 0 - LJS, 1 - CP</param>
        private void CallErrorLogging(string fullMessage, string errorTxt, int src)
        {
            var logData = new Dictionary<string, string>();
            try
            {
                if (_configuration.CallWCFWithProcessingErrors)
                {

                    logData.Add("Intervention name", "HarnessErrorLog");
                    logData.Add("Intervention parameter ProcessorSideError", errorTxt);
                    logData.Add("Intervention parameter Message", fullMessage);
                    logData.Add("Message source", src==0?"LJS":"CabPack");

                    InterventionServiceClient interventionService = new InterventionServiceClient();
                    Guid user = _configuration.UserGuid;
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("ProcessorSideError", errorTxt);
                    parameters.Add("FullMessage", fullMessage);
                    Intervention intervention = new Intervention()
                    {
                        Name = "HarnessErrorLog",
                        ObjectID = src,
                        Parameters = parameters,
                    };
                    interventionService.DoIntervention(intervention, user, null);
                }
            }
            catch (Exception ex)
            {
                _pluginReference.LogThis($"Exception in Harness error logging!", logData, ex, Vrh.Logger.LogLevel.Error, this.GetType());
            }
        }

        /// <summary>
        /// Processes a socket with its message.
        /// </summary>
        /// <param name="state">The Accepted socket which must be processed</param>
        private void TCPProcessMessage(object state)
        {
            bool harnessLogging = false;
            string message = String.Empty;
            var logData = new Dictionary<string, string>()
                    {
                        { "IVConnector plugin instance", _pluginReference.InstanceName },
                        { "IVConnector plugin version", _pluginReference.PluginVersion },
                        { "Used configuration file", _configuration.ConfigurationFileDefinition },
                    };
            Socket s = state as Socket;
            byte[] tmpBuffer = new byte[_maxReceiveBufferSize];
            try
            {
                try
                {
                    // Setting up timeouts
                    s.ReceiveTimeout = _socketTimeout;
                    s.SendTimeout = _socketTimeout;
                    _pluginReference?.LogThis($"Receiving msg...", logData, null, Vrh.Logger.LogLevel.Debug, this.GetType());
                    int rcvd = 0;
                    do
                    {
                        rcvd = s.Receive(tmpBuffer);
                        //_buffer.Concat(tmpBuffer);
                        message += Encoding.ASCII.GetString(tmpBuffer, 0, rcvd);
                        if (!String.IsNullOrEmpty(_configuration.MessageSuffix))
                        {
                            if (message.Contains(_configuration.MessageSuffix))
                            {
                                break;
                            }
                        }
                        var tc = new TickCounter();
                        while(s.Available == 0 && !tc.IsTimeout(new TimeSpan(0, 0, 0, 0, _configuration.ReceiveTimeout)))
                        {
                            Thread.Sleep(_configuration.ReceiveTimeout / 10 >= 1 
                                            ? _configuration.ReceiveTimeout / 10 
                                            : _configuration.ReceiveTimeout);
                        }                        
                    } while (s.Available > 0);                        
                    if (rcvd == 0)
                    {
                        s.Send(Encoding.ASCII.GetBytes("/x15".FromHexOrThis() + "Timeout occured. (Client connected, but sended Nothing within timeout!!!)" + _configuration.Ack));
                        return;
                    }
                }
                catch(Exception ex)
                {
                    s.Send(Encoding.ASCII.GetBytes("/x15".FromHexOrThis() + "Timeout or tcp socket error occured." + _configuration.Ack));
                    _pluginReference?.LogThis($"ERROR: No data received on socket.", logData, ex, Vrh.Logger.LogLevel.Warning, this.GetType());
                    return;
                }
                try
                {
                    harnessLogging = message.Contains("IVPPL") || message.Contains("IVPPC");
                    _pluginReference.LogThis($"Received message: {Vrh.Logger.LogHelper.HexControlChars(message)}", logData, null, Vrh.Logger.LogLevel.Information, this.GetType());
                    ProcessMessage(message, logData, out harnessLogging);
                }
                catch (EndpointNotFoundException e)
                {
                    s.Send(Encoding.ASCII.GetBytes("/x15".FromHexOrThis() + "ALM side WCF service currently not available!" + _configuration.Ack));
                    _pluginReference.LogThis($"Message processing error occured!", logData, null, Vrh.Logger.LogLevel.Error, this.GetType());
                    return;
                }
                //catch (System.TimeoutException e)
                catch (System.ServiceModel.CommunicationException e)
                    {
                    s.Send(Encoding.ASCII.GetBytes("/x15".FromHexOrThis() + "ALM side WCF service response timeout exceeded!" + _configuration.Ack));
                    _pluginReference.LogThis($"WCF timeout eception occured!", logData, null, Vrh.Logger.LogLevel.Error, this.GetType());
                }
                catch (Exception e)
                {
                    _pluginReference.LogThis($"Message processing error occured!", logData, e, Vrh.Logger.LogLevel.Error, this.GetType());
                    if (harnessLogging)
                    {
                        CallErrorLogging(message, e.Message, 1);
                    }
                }
                if (String.IsNullOrEmpty(_configuration.MessageSuffix) || message.Contains(_configuration.MessageSuffix))
                {
                    s.Send(Encoding.ASCII.GetBytes(_configuration.Ack));
                }
                else
                {
                    s.Send(Encoding.ASCII.GetBytes("/x15".FromHexOrThis() + "Timeout or tcp socket error occured. Message suffix is missing in recieved message!" + _configuration.Ack));
                }
            }
            catch (Exception e)
            {
                _pluginReference.LogThis($"Message processing error occured!", logData, e, Vrh.Logger.LogLevel.Error, this.GetType());
                if (harnessLogging)
                {
                    CallErrorLogging(message, e.Message, 1);
                }                
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

        /// <summary>
        /// Feldolgozza az üzenetet, mint stringet (akár TCP-n, akár MMQ-n érkezett)
        /// </summary>
        /// <param name="message">üzenet</param>
        /// <param name="logData">adatok a loghoz</param>
        /// <param name="harnessLoging">harness-en kell-e ezt a beavatkozást logolni</param>
        /// <returns></returns>
        private string ProcessMessage(string message, Dictionary<string, string> logData, out bool harnessLoging)
        {
            string tmp = message;
            string result;
            string messagePrefix = _configuration.MessagePrefix;
            harnessLoging = false;
            if (!String.IsNullOrEmpty(messagePrefix))
            {
                // Ha van üzenet prefix
                if (!tmp.StartsWith(messagePrefix))
                {
                    result = $"Processing failed! Invalid Message! Excepted message prefix not found in received message.";
                    logData.Add("Expected message prefix", Vrh.Logger.LogHelper.HexControlChars(messagePrefix));
                    logData.Add("Received message", Vrh.Logger.LogHelper.HexControlChars(message));
                    _pluginReference.LogThis(result, logData, null, Vrh.Logger.LogLevel.Information, this.GetType());
                    return result;
                    ///throw new Exception($"Invalid Message! Excepted message prefix ({Vrh.Logger.LogHelper.HexControlChars(messagePrefix)}) not found in received message: {Vrh.Logger.LogHelper.HexControlChars(message)}");
                }
                tmp = tmp.Remove(0, messagePrefix.Length);
            }
            string messageSuffix = _configuration.MessageSuffix;
            if (!String.IsNullOrEmpty(messageSuffix))
            {
                // Ha van üzenet postfix
                if (!tmp.EndsWith(messageSuffix))
                {
                    result = $"Processing failed! Invalid Message! Excepted message suffix not found in received message.";
                    logData.Add("Excepted message suffix", Vrh.Logger.LogHelper.HexControlChars(messageSuffix));
                    logData.Add("Received message", Vrh.Logger.LogHelper.HexControlChars(message));
                    _pluginReference.LogThis(result, logData, null, Vrh.Logger.LogLevel.Information, this.GetType());
                    return result;
                    ///throw new Exception($"Invalid Message! Excepted message suffix ({Vrh.Logger.LogHelper.HexControlChars(messageSuffix)}) not found in received message: {Vrh.Logger.LogHelper.HexControlChars(message)}");
                }
                tmp = tmp.Remove(tmp.Length - messageSuffix.Length);
            }
            string ivIdSeparator = _configuration.IDSeparator;
            string assemblyLineId = null;
            string ivId = null;
            if (!String.IsNullOrEmpty(ivIdSeparator))
            {
                //string[] parts = tmp.Split(new string[] { ivIdSeparator }, StringSplitOptions.None);
                //if (parts.Length > 2)
                //{
                //    var ex = new Exception($"The message id separator ({Vrh.Logger.LogHelper.HexControlChars(ivIdSeparator)}) can only once occur in message! Processed message {Vrh.Logger.LogHelper.HexControlChars(message)}");
                //    throw ex;
                //}
                // Ha van ivid separátor --> akkor az addig tartó string a sor azonosító, a maradék a további adat
                string[] parts = tmp.Split(new string[] { ivIdSeparator }, StringSplitOptions.RemoveEmptyEntries);
                //if (parts.Length == 0) || !tmp.Contains(_configuration.IDSeparator))
                //{
                    //var ex = new Exception($"MessageID separator ({Vrh.Logger.LogHelper.HexControlChars(ivIdSeparator)}) required! Processed message {Vrh.Logger.LogHelper.HexControlChars(message)}");
                    //throw ex;
                //}
                if (parts.Length == 1)
                {
                    ivId = parts[0];
                    tmp = String.Empty;
                }
                if (parts.Length == 2)
                {
                    ivId = parts[0];
                    tmp = parts[1];
                }
                if (parts.Length == 3)
                {
                    assemblyLineId = parts[0];
                    ivId = parts[1];
                    tmp = parts[2];
                }
            }
            string[] fields = tmp.Split(new string[] { _configuration.FieldSeparator }, StringSplitOptions.RemoveEmptyEntries);
            // fields[0] most az üzenet azonosító
            // ivId = fields[0];                
            harnessLoging = ivId.ToLower() == "ivppl" || ivId.ToLower() == "ivppc";
            if (!_configuration.IsHandled(ivId))
            {
                result = $"Processing failed! The received message IVD is not handled by this connector!";
                logData.Add("Message IVID", Vrh.Logger.LogHelper.HexControlChars(ivId));
                logData.Add("Received message", Vrh.Logger.LogHelper.HexControlChars(message));
                _pluginReference.LogThis(result, logData, null, Vrh.Logger.LogLevel.Information, this.GetType());
                return result;
                ///throw new Exception($"This message with '{Vrh.Logger.LogHelper.HexControlChars(ivId)}' id not handled by this connector! Processed message {Vrh.Logger.LogHelper.HexControlChars(message)}");
            }
            // a maradék a paraméterek
            List<Parameter> inputParameters = new List<Parameter>();
            int no = 0;
            for (int j = 0; j < fields.Length; j++)
            {
                try
                {
                    inputParameters.Add(
                        new Parameter()
                        {
                            No = no,
                            Name = Parameter.GetName(fields[j], _configuration.MessageFormat),
                            Value = Parameter.GetValue(fields[j], _configuration.MessageFormat),
                        }
                    );
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                no++;
            }
            if (_configuration.MessageFormat == MessageFormat.ByName && String.IsNullOrEmpty(assemblyLineId))
            {
                var prline = inputParameters.FirstOrDefault(x => x.Name.ToUpper() == "PRLINE");
                if (prline != null)
                {
                    assemblyLineId = prline.Value;
                }
            }
            DefinedMessage def = _messagesConfiguration.GetMessage(ivId);
            if (def == null)
            {
                result = $"Processing failed! Intervention definition not found for the IVID of the processed message.";
                logData.Add("Message IVID", Vrh.Logger.LogHelper.HexControlChars(ivId));
                logData.Add("Received message", Vrh.Logger.LogHelper.HexControlChars(message));
                _pluginReference.LogThis(result, logData, null, Vrh.Logger.LogLevel.Information, this.GetType());

                return result;
                ///throw new Exception($"Intervention definition not found this message id: {Vrh.Logger.LogHelper.HexControlChars(ivId)}! Processed message {Vrh.Logger.LogHelper.HexControlChars(message)}");
            }
            if (String.IsNullOrEmpty(assemblyLineId))
            {
                result = $"Processing failed! Assembly line id not present in received message.";
                logData.Add("Received message", Vrh.Logger.LogHelper.HexControlChars(message));
                _pluginReference.LogThis(result, logData, null, Vrh.Logger.LogLevel.Information, this.GetType());
                return result;
                ///throw new Exception($"Assembly line id not present in received message: {Vrh.Logger.LogHelper.HexControlChars(message)}");
            }

            Parameter externalSystemId = null;
            if (_configuration.MessageFormat == MessageFormat.ByName)
            {
                externalSystemId = inputParameters.FirstOrDefault(x => x.Name.ToUpper() == "IVSRC");
            }
            InterventionServiceClient interventionService = new InterventionServiceClient();
            Intervention intervention = new Intervention();
            int asId = GetObjectId(assemblyLineId, interventionService);
            if (asId == -1)
            {
                if (externalSystemId != null)
                {
                    intervention.Name = "TranslateId";
                    intervention.Parameters = new Dictionary<string, object>();
                    intervention.Parameters.Add("EntityType", "ASLINE");
                    intervention.Parameters.Add("ExternalSystem", externalSystemId.Value);
                    intervention.Parameters.Add("fId", assemblyLineId);
                    string resultId;
                    try
                    {
                        /// biztosan csak exception-nel lehet megoldani, hogy nincs a fordítótáblában a külső kódhoz fordítás????
                        resultId = interventionService.DoIntervention(intervention, _configuration.UserGuid, null);
                    }
                    catch
                    {
                        /// másrészt ilyen felismert hiba esetekben nem throw exception-nel kellene megoldani a hiba naplózást, mert semmi értelme a 
                        /// teljes exception stack beírásának, elég lenne a VrhLogger.Log használata, majd valami return visszatérő kód előállítása, 
                        /// amire megszakad a feldolgozás.
                        result = $"Processing failed! Unknown Assembly line external identifyer in received message (or some other error in requesting IdTranslation)!";
                        logData.Add("Assembly line external id", assemblyLineId);
                        logData.Add("Received message", Vrh.Logger.LogHelper.HexControlChars(message));
                        _pluginReference.LogThis(result, logData, null, Vrh.Logger.LogLevel.Information, this.GetType());
                        return result;
                        /// throw new Exception($"Unknown Assembly line external identifyer (or some other error in requesting IdTranslation): {assemblyLineId}! Processed message {Vrh.Logger.LogHelper.HexControlChars(message)}");
                    }
                    if (!Int32.TryParse(resultId, out asId)) { asId = -1; }
                }
                if (asId == -1)
                {
                    result = $"Processing failed! Unknown Assembly line referenced in received message.";
                    logData.Add("Assembly line", assemblyLineId);
                    logData.Add("Received message", Vrh.Logger.LogHelper.HexControlChars(message));
                    _pluginReference.LogThis(result, logData, null, Vrh.Logger.LogLevel.Information, this.GetType());
                    return result;
                    ///throw new Exception($"Unknown Assembly line: {assemblyLineId}! Processed message {Vrh.Logger.LogHelper.HexControlChars(message)}");
                }
            }
            InterventionDefination ivDef = null;
            foreach (var item in interventionService.GetAllIntervention(null))
            {
                if (((InterventionDefination)item).Name == def.Intervention)
                {
                    ivDef = (InterventionDefination)item;
                    break;
                }
            }
            if (ivDef == null)
            {
                result = $"Processing failed! Unknown intervention is referenced in received message.";
                logData.Add("Intervention", def.Intervention);
                logData.Add("Received message", Vrh.Logger.LogHelper.HexControlChars(message));
                _pluginReference.LogThis(result, logData, null, Vrh.Logger.LogLevel.Information, this.GetType());
                return result;
                ///throw new Exception($"Unknown intervention: {def.Intervention}! Processed message {Vrh.Logger.LogHelper.HexControlChars(message)}");
            }
            if (harnessLoging)
            {
                inputParameters.Add(
                        new Parameter()
                        {
                            Name = "FullMessage",
                            Value = message,
                            No = inputParameters.Max(x => x.No),
                        }
                    );
            }
            intervention.Name = def.Intervention;
            intervention.ObjectID = asId;
            intervention.Parameters = GetParameters(ivDef, def, inputParameters);
            Guid user = _configuration.UserGuid;
            //data.Add("User guid", user.ToString());
            result = $"Processing received message was successful!";
            logData.Add("Received message", Vrh.Logger.LogHelper.HexControlChars(message));
            logData.Add("Intervention response", interventionService.DoIntervention(intervention, user, null));
            _pluginReference.LogThis(result, logData, null, Vrh.Logger.LogLevel.Information, this.GetType());
            return result;
        }

        /// <summary>
        /// Összeállítja a paramétereket a WCF intervention-höz
        /// </summary>
        /// <param name="ivDef">Intervention definiciója (a szolgáltatásból)</param>
        /// <param name="msgDef">üzenet definiciója az ivconnector xml-je szerint</param>
        /// <param name="inputParameters">üzenetben kaott paraméterek listája</param>
        /// <returns>iv paraméter lista</returns>
        private Dictionary<string, object> GetParameters(InterventionDefination ivDef, DefinedMessage msgDef, List<Parameter> inputParameters)
        {
            var returnParameters = new Dictionary<string, Object>();
            int i = 0;
            foreach (var item in ivDef.ParameterList)
            {
                ParameterDefinition parameter = (ParameterDefinition)item;
                Object parameterValue;
                string inputStr = GetStringParameterValue(parameter, msgDef, inputParameters, i);
                if (!String.IsNullOrEmpty(inputStr))
                {
                    switch (((DataType)parameter.ParameterType))
                    {
                        case DataType.Boolean:
                            bool bValue = false;
                            if (!bool.TryParse(inputStr, out bValue))
                            {
                                throw new Exception(string.Format("Wrong parameter format! Value: {0}, Expected type: {1}", inputStr, "Boolean"));
                            }
                            parameterValue = bValue;
                            break;
                        case DataType.Int32:
                            int iValue = 0;
                            if (!int.TryParse(inputStr, out iValue))
                            {
                                throw new Exception(string.Format("Wrong parameter format! Value: {0}, Expected type: {1}", inputStr, "Int32"));
                            }
                            parameterValue = iValue;
                            break;
                        case DataType.Double:
                            double dValue = 0;
                            if (!double.TryParse(inputStr, out dValue))
                            {
                                throw new Exception(string.Format("Wrong parameter format! Value: {0}, Expected type: {1}", inputStr, "Double"));
                            }
                            parameterValue = dValue;
                            break;
                        case DataType.DateTime:
                            DateTime dtValue;
                            if (!DateTime.TryParse(inputStr, out dtValue))
                            {
                                throw new Exception(string.Format("Wrong parameter format! Value: {0}, Expected type: {1}", inputStr, "DateTime"));
                            }
                            parameterValue = dtValue;
                            break;
                        default:
                            parameterValue = inputStr;
                            break;
                    }
                    returnParameters.Add(parameter.Name, parameterValue);
                }
                else
                {
                    if ((DataType) parameter.ParameterType == DataType.String)
                    {
                        returnParameters.Add(parameter.Name, String.Empty);
                    }
                    else
                    {
                        throw new Exception($"Intervention parameter is missing: {parameter.Name}");
                    }                    
                }
                i++;
            }

            return returnParameters;
        }

        /// <summary>
        /// kiszedi a megadott paraméter üzenettel kapott értékét stringként
        /// </summary>
        /// <param name="ivPar"></param>
        /// <param name="msgDef">üzenet definiciója az ivconnector xml-je szerint</param>
        /// <param name="inputParameters">üzenetben kaott paraméterek listája</param>        
        /// <param name="no">hányadik paraméter?</param>
        /// <returns>paraméter érték stringként</returns>
        private string GetStringParameterValue(ParameterDefinition ivPar, DefinedMessage msgDef, List<Parameter> inputParameters, int no)
        {
            if (_configuration.MessageFormat == MessageFormat.Positional)
            {
                var input = inputParameters.FirstOrDefault(x => x.No == no);
                if (input != null)
                {
                    return input.Value;
                }
            }
            else
            {
                var field = msgDef.Fields.FirstOrDefault(x => x.InterventionParameter.ToLower() == ivPar.Name.ToLower());
                if (field != null)
                {
                    var input = inputParameters.FirstOrDefault(x => x.Name.ToLower() == field.Name.ToLower());
                    if (input != null)
                    {
                        return input.Value;
                    }
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// Visszaadja az object id-t, a szerelősor kulcsából
        /// </summary>
        /// <param name="assemblyLineId">szerelősor kulcsa</param>
        /// <param name="interventionService">referencia a WCF intervention szolgáltatásra, ahonnan az információt megszerzi</param>
        /// <returns>objektum id</returns>
        private int GetObjectId(string assemblyLineId, InterventionServiceClient interventionService)
        {
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
                    if (key == String.Format("{0})", assemblyLineId) || name == assemblyLineId)
                    {
                        asId = item.ObjectID;
                        break;
                    }
                }
            }
            return asId;
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
            _pluginReference.LogThis(String.Format($"Configuration issue: {0}", e.Message), data, e.Exception, level);
        }

        /// <summary>
        /// Log rekordokhoz fejlécet állít elő
        /// </summary>

        /// <summary>
        /// Referencia az iv connector pluginra (a szolgáltatásai közvetlen eléréséhez)
        /// </summary>
        private IVConnectorPlugin _pluginReference = null;

        /// <summary>
        /// TCP socket timeout
        /// </summary>
        private static int _socketTimeout = 5000;

        /// <summary>
        /// bejövő üenet méret
        /// </summary>
        private static int _maxReceiveBufferSize = 200;

        /// <summary>
        /// Folymatot futtató szál
        /// </summary>
        private Thread _process;

        /// <summary>
        /// Konfiguráció
        /// </summary>
        private IVConnectorParameterFileProcessor _configuration;

        /// <summary>
        /// Azon üzenetek definiciója, melyeket a connector feldolgoz 
        /// </summary>
        private MessageDefinitionsParameterFileProcessor _messagesConfiguration;

        /// <summary>
        /// TCP server socket
        /// </summary>
        private TcpListener _listener;

        private byte[] _buffer = new byte[_maxReceiveBufferSize];

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
