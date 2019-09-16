using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net;
using System.Messaging;

using Vrh.LinqXMLProcessor.Base;
using VRH.Common;
using Vrh.Logger;
using VRH.ConnectionStringStore;
using Vrh.ApplicationContainer;

namespace IVConnector.Plugin
{
    /// <summary>
    /// IV Connector configuration parameters
    /// </summary>
    internal class IVConnectorParameterFileProcessor : LinqXMLProcessorBaseClass
    {
        #region Public Members

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="parameterFile">paraméter fájl és fájlon belüli útvonal a root TAG-ig</param>
        public IVConnectorParameterFileProcessor(string parameterFile)
        {
            _xmlFileDefinition = parameterFile;
        }

        /// <summary>
        /// A használt konfiguráció
        /// </summary>
        public string ConfigurationFileDefinition
        {
            get
            {
                return _xmlFileDefinition;
            }
        }

        /// <summary>
        /// A connector típusa 
        /// </summary>
        public IVConnectorType ConnectorType
        {
            get
            {
                return GetEnumAttributeValue<IVConnectorType>(GetRootElement(), TYPE_ATTRIBUTE_IN_CONFIGURATION_ELEMENT_NAME, IVConnectorType.TCP);
            }
        }

        private IPAddress ip;
        private int port;
        private string socket = null;
        /// <summary>
        /// IV Connecrtor Listener IP
        /// </summary>
        public string ListenTCPIPSocket
        {
            get
            {
                string cs = null, _csname = null;
                try
                {
                    _csname = GetElementValue<string>(GetXElement(LISTENTCPIPSOCKET_NAME), "");
                    cs = VRHConnectionStringStore.GetTCPIPsocketConnectionString(string.IsNullOrEmpty(_csname) ? "VRH.ivConnector:ListenTCPIPSocket" : _csname);
                }
                catch { cs = null; }
                string socket = !string.IsNullOrEmpty(cs) ? cs
                        : !string.IsNullOrEmpty(_csname) ? _csname
                        : "127.0.0.1:1981";
                try
                {
                    ip = IPAddress.Parse(socket.Split(':')[0]);
                    port = int.Parse(socket.Split(':')[1]);
                    return socket;
                }
                catch (Exception ex)
                {
                    ip = null; port = 0;
                    var data = new Dictionary<string, string>() { { "ListenTCPIPSocket name", _csname }, { "ListenTCPIPSocket", socket } };
                    throw new FatalException("Configuration Error: invalid TCPIP socket name/address.", ex, null);
                }
            }
        }


        /// <summary>
        /// IV Connecrtor Listener IP
        /// </summary>
        public IPAddress IP
        {
            get
            {
                if (ip == null) socket = ListenTCPIPSocket;
                return ip;
            }
        }

        /// <summary>
        /// IC Connector listener Port
        /// </summary>
        public int Port
        {
            get
            {
                if (ip == null) socket = ListenTCPIPSocket;
                return port;
            }
        }

        /// <summary>
        /// Az üzenetek definiált prefixe
        /// </summary>
        public string MessagePrefix
        {
            get
            {
                return GetAttribute<string>(GetXElement(MESSAGESTRUCTURE_ELEMENT_NAME), 
                    PREFIX_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT, 
                    String.Empty).FromHexOrThis();
            }
        }

        /// <summary>
        /// Az üzenetek definiált záró karakterlánca
        /// </summary>
        public string MessageSuffix
        {
            get
            {
                return GetAttribute<string>(GetXElement(MESSAGESTRUCTURE_ELEMENT_NAME), 
                    SUFFIX_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT, 
                    ConnectorType == IVConnectorType.TCP ? @"\x0D0A" : String.Empty).FromHexOrThis();
            }
        }

        /// <summary>
        /// Üzenet azonosító separátor
        /// </summary>
        public string IDSeparator
        {
            get
            {
                return GetAttribute<string>(GetXElement(MESSAGESTRUCTURE_ELEMENT_NAME), 
                    IVIDSEPARATOR_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT, 
                    String.Empty).FromHexOrThis();
            }
        }

        /// <summary>
        /// A mezők azonosítóit keretező karakterek
        /// </summary>
        public string FieldNameFrame
        {
            get
            {
                return GetAttribute<string>(GetXElement(MESSAGESTRUCTURE_ELEMENT_NAME), 
                    FIELDNAMEFRAME_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT, 
                    String.Empty).FromHexOrThis();
            }
        }

        /// <summary>
        /// Paraméterek separátora MMQ feldolgozáskor
        /// </summary>
        public string FieldSeparator
        {
            get
            {
                return GetAttribute<string>(GetXElement(MESSAGESTRUCTURE_ELEMENT_NAME),
                    FIELDSEPARATOR_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT, 
                    ";").FromHexOrThis();
            }
        }

        /// <summary>
        /// Listaelem separátor karakter
        /// </summary>
        public string ListSeparator
        {
            get
            {
                return GetAttribute<string>(GetXElement(VALUESSEPARATOR_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT),
                    VALUESSEPARATOR_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT, 
                    ",").FromHexOrThis();
            }
        }

        /// <summary>
        /// Definiált üzenetnyugta
        /// </summary>
        public string Ack
        {
            get
            {
                return GetElementValue<string>(GetXElement(ACK_ELEMENT_NAME), "\x06").FromHexOrThis();
            }
        }

        /// <summary>
        /// Bejövő MSMQ címe
        /// </summary>
        public string InQueue
        {
            get
            {
                string cs = null, _csname = null;
                try
                {
                    _csname = GetElementValue<string>(GetXElement(INQUEUE_ELEMENT_NAME), "");
                    cs = VRHConnectionStringStore.GetMSMQConnectionString(string.IsNullOrEmpty(_csname) ? "VRH.ivConnector:InMSMQ" : _csname);
                }
                catch { cs = null; }
                string msmqcs = !string.IsNullOrEmpty(cs) ? cs
                        : !string.IsNullOrEmpty(_csname) ? _csname
                        : @".\private$\inmsmq";
                return msmqcs;
            }
        }

        /// <summary>
        /// Válasz MSMQ címe
        /// </summary>
        public string ResponseQueue
        {
            get
            {
                return GetElementValue<string>(GetXElement(RESPONSEQUEUE_ELEMENT_NAME), String.Empty);
            }
        }

        /// <summary>
        /// Csak a megadott Filtereknek megfelelő label-lel rendelkező üzeneteket dolgozza fel 
        /// </summary>
        public string LabelFilters
        {
            get
            {
                return GetElementValue<string>(GetXElement(LABELFILTER_ELEMENT_NAME), String.Empty);
            }
        }

        /// <summary>
        /// Megmondja, milyen Id kezelés van konfigurálva
        /// </summary>
        public MSMQIdHandling IdHandling
        {
            get
            {
                return GetEnumValue<MSMQIdHandling>(GetXElement(IDHANDLING_ELEMENT_NAME), MSMQIdHandling.None);
            }
        }

        /// <summary>
        /// Válasz MSMQ üzenet címkéje
        /// </summary>
        public string ResponseLabel
        {
            get
            {
                return GetElementValue<string>(GetXElement(RESPONSELABEL_ELEMENT_NAME), String.Empty);
            }
        }

        /// <summary>
        /// A felhasználó azonosítója, amivel megszemélyesíti a beavatkozásokat
        /// </summary>
        public Guid UserGuid
        {
            get
            {
                string strValue = GetElementValue<string>(GetXElement(USERGUID_ELEMENT_NAME), String.Empty);
                Guid value;
                if (Guid.TryParse(strValue, out value))
                {
                    return value;
                }
                else
                {
                    return Guid.Empty;
                }
            }
        }

        /// <summary>
        /// A feldolgozott üzenetek formátum típusa
        /// </summary>
        public MessageFormat MessageFormat
        {
            get
            {
                string strValue = GetAttribute<string>(GetXElement(MESSAGESTRUCTURE_ELEMENT_NAME), 
                    FORMAT_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT, 
                    String.Empty);
                MessageFormat value;
                if (Enum.TryParse<MessageFormat>(strValue, true, out value))
                {
                    return value;
                }
                else
                {
                    return MessageFormat.Positional;
                }
            }
        }

        /// <summary>
        /// Kell e az üzenetfeldolgozás során felépő hibákkal hívni a WCF service-t
        /// </summary>
        public bool CallWCFWithProcessingErrors
        {
            get
            {
                return GetExtendedBoolElementValue(GetXElement(CALLWCFWITHPROCESSINGERRORS_ELEMENT_NAME), false, "1", "yes", "true");
            }
        }

        /// <summary>
        /// Az MSMQ üzenetekhez ghasznált formatter
        /// </summary>
        public MSMQFormatter MsmqFormatter
        {
            get
            {
                return GetEnumValue<MSMQFormatter>(GetXElement(MESSAGEFORMATTER_ELEMENT_NAME), MSMQFormatter.ActiveXMessageFormatter);
            }
        }

        /// <summary>
        /// MSMQ encoding
        /// </summary>
        public MyEncoding Encoding
        {
            get
            {
                return GetEnumValue<MyEncoding>(GetXElement(ENCODING_ELEMENT_NAME), MyEncoding.Default);
            }
        }

        /// <summary>
        /// A kezelt üzenetek listája
        /// </summary>
        public List<string> HandledMessages
        {
            get
            {
                List<string> handledMessages = new List<string>();
                foreach (var msg in GetAllXElements(MESSAGES_ELEMENT_NAME, MESSAGE_ELEMENT_NAME))
                {
                    string id = GetAttribute(msg, IVID_ATTRIBUTE_IN_MESSAGE_ELEMENT, String.Empty);
                    if (!String.IsNullOrEmpty(id))
                    {
                        handledMessages.Add(id);
                    }
                } 
                return handledMessages;
            }
        }

        /// <summary>
        /// Megmondja, hogy ezt az üzenetet kezeli-e a connector példány (az id nem érzékeny kis és nagybetű különbségekre (ABC==abc))
        /// </summary>
        /// <param name="msgId">üzenet azonosítója</param>
        /// <returns>kezeli/nem kezeli</returns>
        public bool IsHandled(string msgId)
        {
            return HandledMessages.Any(x => x.ToLower() == msgId.ToLower());
        }

        /// <summary>
        /// A TCP fregmentumnak ennyi időn belül meg kell érkeznie az elözőhöz képest
        /// </summary>
        public int ReceiveTimeout
        {
            get
            {
                return GetElementValue<int>(GetXElement(RECEIVETIMEOUT_ELEMENT_NAME), 100);
            }
        }

        #endregion Public Members

        #region Private Members

        //COMMON Config
        private const string TYPE_ATTRIBUTE_IN_CONFIGURATION_ELEMENT_NAME = "Type";
        private const string CALLWCFWITHPROCESSINGERRORS_ELEMENT_NAME = "CallWCFWithProcessingErrors";
        // - MessageStructure
        private const string MESSAGESTRUCTURE_ELEMENT_NAME = "MessageStructure";
        private const string PREFIX_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT = "Prefix";
        private const string SUFFIX_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT = "Suffix";
        private const string IVIDSEPARATOR_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT = "IVIDSeparator";
        private const string FIELDSEPARATOR_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT = "FieldSeparator";
        private const string FIELDNAMEFRAME_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT = "FieldNameFrame";
        private const string VALUESSEPARATOR_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT = "ValuesSeparator";
        private const string FORMAT_ATTRIBUTE_IN_MESSAGESTRUCTURE_ELEMENT = "Format";
        // - UserGuid
        private const string USERGUID_ELEMENT_NAME = "UserGuid";
        // - Messages
        private const string MESSAGES_ELEMENT_NAME = "Messages";
        private const string MESSAGE_ELEMENT_NAME = "Message";
        private const string IVID_ATTRIBUTE_IN_MESSAGE_ELEMENT = "IVID";
        // TCP Config
        private const string LISTENTCPIPSOCKET_NAME = "ListenTCPIPSocket";
        private const string ACK_ELEMENT_NAME = "Ack";
        private const string RECEIVETIMEOUT_ELEMENT_NAME = "ReceiveTimeout";
        // MSMQ Config
        private const string MESSAGEFORMATTER_ELEMENT_NAME = "MessageFormatter";
        private const string INQUEUE_ELEMENT_NAME = "InQueue";
        private const string RESPONSEQUEUE_ELEMENT_NAME = "ResponseQueue";
        private const string LABELFILTER_ELEMENT_NAME = "LabelFilter";
        private const string IDHANDLING_ELEMENT_NAME = "IdHandling";
        private const string RESPONSELABEL_ELEMENT_NAME = "ResponseLabel";
        private const string ENCODING_ELEMENT_NAME = "Encoding";

        #endregion Private Members
    }

    /// <summary>
    /// a MSMQ üzenetkhez hazsnálandó formatter
    /// </summary>
    internal enum MSMQFormatter
    {
        ActiveXMessageFormatter = 0,
        XmlMessageFormatter = 1,
    }

    /// <summary>
    /// MSMQ üzenetekben használt karakterkódolás
    /// </summary>
    internal enum MyEncoding
    {
        Default = 0,
        UTF8 = 1,
        UTF7 = 2,
        UTF32 = 3,
        Unicode = 4,
        BigEndianUnicode = 5,
        ASCII = 6,
    }
}
