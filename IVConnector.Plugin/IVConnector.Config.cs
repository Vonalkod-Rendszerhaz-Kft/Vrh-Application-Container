using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vrh.LinqXMLProcessor.Base;
using VRH.Common;

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
                return GetEnumAttributeValue<IVConnectorType>(GetXElement(CONFIGURATION_ELEMENT_NAME), TYPE_ATTRIBUTE_IN_CONFIGURATION_ELEMENT, IVConnectorType.TCP);
            }
        }

        /// <summary>
        /// IV Connecrtor Listener IP
        /// </summary>
        public string IP
        {
            get
            {
                return GetElementValue<string>(GetXElement(CONFIGURATION_ELEMENT_NAME, IP_ELEMENT_NAME), "127.0.0.1");
            }
        }

        /// <summary>
        /// IC Connector listener Port
        /// </summary>
        public int Port
        {
            get
            {
                return GetElementValue<int>(GetXElement(CONFIGURATION_ELEMENT_NAME, PORT_ELEMENT_NAME), 1981);
            }
        }

        /// <summary>
        /// Az üzenetek definiált prefixe
        /// </summary>
        public string MessagePrefix
        {
            get
            {
                return GetElementValue<string>(GetXElement(CONFIGURATION_ELEMENT_NAME, MESSAGEPREFIX_ELEMENT_NAME), String.Empty).FromHexOrThis();
            }
        }

        /// <summary>
        /// Az üzenetek definiált záró karakterlánca
        /// </summary>
        public string MessageSuffix
        {
            get
            {
                return GetElementValue<string>(GetXElement(CONFIGURATION_ELEMENT_NAME, MESSAGESUFFIX_ELEMENT_NAME), @"\x0D0A").FromHexOrThis();
            }
        }

        /// <summary>
        /// Üzenet azonosító separátor
        /// </summary>
        public string IDSeparator
        {
            get
            {
                return GetElementValue<string>(GetXElement(CONFIGURATION_ELEMENT_NAME, IVIDSEPARATOR_ELEMENT_NAME), "#$").FromHexOrThis();
            }
        }

        /// <summary>
        /// Paraméterek separátora
        /// </summary>
        public string ParameterSeparator
        {
            get
            {
                return GetElementValue<string>(GetXElement(CONFIGURATION_ELEMENT_NAME, PARAMETERSEPARATOR_ELEMENT_NAME), "@").FromHexOrThis();
            }
        }

        /// <summary>
        /// Definiált üzenetnyugta
        /// </summary>
        public string Ack
        {
            get
            {
                return GetElementValue<string>(GetXElement(CONFIGURATION_ELEMENT_NAME, ACK_ELEMENT_NAME), "\x06").FromHexOrThis();
            }
        }

        /// <summary>
        /// Bejövő MSMQ címe
        /// </summary>
        public string InQueue
        {
            get
            {
                return GetElementValue<string>(GetXElement(CONFIGURATION_ELEMENT_NAME, INQUEUE_ELEMENT_NAME), "");
            }
        }

        /// <summary>
        /// Válasz MSMQ címe
        /// </summary>
        public string ResponseQueue
        {
            get
            {
                return GetElementValue<string>(GetXElement(CONFIGURATION_ELEMENT_NAME, RESPONSEQUEUE_ELEMENT_NAME), "");
            }
        }

        /// <summary>
        /// Csak a megadott Filtereknek megfelelő label-lel rendelkező üzeneteket dolgozza fel 
        /// </summary>
        public string LabelFilters
        {
            get
            {
                return GetElementValue<string>(GetXElement(CONFIGURATION_ELEMENT_NAME, LABELFILTER_ELEMENT_NAME), "");
            }
        }

        /// <summary>
        /// Megmondja, milyen Id kezelés van konfigurálva
        /// </summary>
        public MSMQIdHandling IdHandling
        {
            get
            {
                return GetEnumValue<MSMQIdHandling>(GetXElement(CONFIGURATION_ELEMENT_NAME, IDHANDLING_ELEMENT_NAME), MSMQIdHandling.None);
            }
        }

        /// <summary>
        /// Válasz MSMQ üzenet címkéje
        /// </summary>
        public string ResponseLabel
        {
            get
            {
                return GetElementValue<string>(GetXElement(CONFIGURATION_ELEMENT_NAME, RESPONSELABEL_ELEMENT_NAME), "");
            }
        }

        /// <summary>
        /// Vissazdja a konfiguráció szerint az adott üzenet azonosítóhoz tartozó beavatkozás elnevezést (vagy String.Empty-t, ha nincs hozzá)
        /// </summary>
        /// <param name="Id">Üzenet azonosító</param>
        /// <returns></returns>
        public string GetIterventionFromID(string Id)
        {
            try
            {
                IEnumerable<XElement> messages = GetAllXElements(HANDLEDMESSAGES_ELEMENT_NAME, MESSAGE_ELEMENT_NAME);
                XElement message = messages.FirstOrDefault(x => x.Attribute(MESSAGEID_ATTRIBUTE_IN_MESSAGE_ELEMENT).Value.ToUpper() == Id.ToUpper());
                return GetAttribute<string>(message, INTERVENTION_ATTRIBUTE_IN_MESSAGE_ELEMENT, String.Empty);
            }
            catch
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// A felhasználó azonosítója, amivel megszemélyesíti a beavatkozásokat
        /// </summary>
        public Guid UserGuid
        {
            get
            {
                string strValue = GetAttribute<string>(GetXElement(HANDLEDMESSAGES_ELEMENT_NAME), USERGUID_ATTRIBUTE_IN_HANDLEDMESSAGES_ELEMENT, String.Empty);
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

        #endregion Public Members

        #region Private Members

        private const string IVCONNECTORCONFIG_ELEMENT_NAME = "IVConnectorConfig";
        private const string CONFIGURATION_ELEMENT_NAME = "Configuration";
        private const string TYPE_ATTRIBUTE_IN_CONFIGURATION_ELEMENT = "Type";
        private const string IP_ELEMENT_NAME = "IP";
        private const string PORT_ELEMENT_NAME = "Port";
        private const string MESSAGEPREFIX_ELEMENT_NAME = "MessagePrefix";
        private const string MESSAGESUFFIX_ELEMENT_NAME = "MessageSuffix";
        private const string IVIDSEPARATOR_ELEMENT_NAME = "IVIDSeparator";
        private const string PARAMETERSEPARATOR_ELEMENT_NAME = "ParameterSeparator";
        private const string ACK_ELEMENT_NAME = "Ack";
        private const string INQUEUE_ELEMENT_NAME = "InQueue";
        private const string RESPONSEQUEUE_ELEMENT_NAME = "ResponseQueue";
        private const string LABELFILTER_ELEMENT_NAME = "LabelFilter";
        private const string IDHANDLING_ELEMENT_NAME = "IdHandling";
        private const string RESPONSELABEL_ELEMENT_NAME = "ResponseLabel";

        private const string HANDLEDMESSAGES_ELEMENT_NAME = "HandledMessages";
        private const string USERGUID_ATTRIBUTE_IN_HANDLEDMESSAGES_ELEMENT = "UserGuid";        
        private const string MESSAGE_ELEMENT_NAME = "Message";
        private const string MESSAGEID_ATTRIBUTE_IN_MESSAGE_ELEMENT = "MessageId";
        private const string INTERVENTION_ATTRIBUTE_IN_MESSAGE_ELEMENT = "Intervention";

        #endregion Private Members
    }
}
