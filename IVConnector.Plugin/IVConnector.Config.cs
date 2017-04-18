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
        /// IV Connecrtor Listener IP
        /// </summary>
        public string IP
        {
            get
            {
                return GetAttribute<string>(GetXElement(CONFIGURATION_ELEMENT_NAME), IP_ATTRIBUTE_IN_CONFIGURATION_ELEMENT, "127.0.0.1");
            }
        }

        /// <summary>
        /// IC Connector listener Port
        /// </summary>
        public int Port
        {
            get
            {
                return GetAttribute<int>(GetXElement(CONFIGURATION_ELEMENT_NAME), PORT_ATTRIBUTE_IN_CONFIGURATION_ELEMENT, 1981);
            }
        }

        /// <summary>
        /// Az üzenetek definiált prefixe
        /// </summary>
        public string MessagePrefix
        {
            get
            {
                return GetAttribute<string>(GetXElement(CONFIGURATION_ELEMENT_NAME), MESSAGEPREFIX_ATTRIBUTE_IN_CONFIGURATION_ELEMENT, String.Empty).FromHexOrThis();
            }
        }

        /// <summary>
        /// Az üzenetek definiált záró karakterlánca
        /// </summary>
        public string MessageSuffix
        {
            get
            {
                return GetAttribute<string>(GetXElement(CONFIGURATION_ELEMENT_NAME), MESSAGESUFFIX_ATTRIBUTE_IN_CONFIGURATION_ELEMENT, "\x0D0A").FromHexOrThis();
            }
        }

        /// <summary>
        /// Üzenet azonosító separátor
        /// </summary>
        public string IDSeparator
        {
            get
            {
                return GetAttribute<string>(GetXElement(CONFIGURATION_ELEMENT_NAME), IVIDSEPARATOR_ATTRIBUTE_IN_CONFIGURATION_ELEMENT, "#$").FromHexOrThis();
            }
        }

        /// <summary>
        /// Paraméterek separátora
        /// </summary>
        public string ParameterSeparator
        {
            get
            {
                return GetAttribute<string>(GetXElement(CONFIGURATION_ELEMENT_NAME), PARAMETERSEPARATOR_ATTRIBUTE_IN_CONFIGURATION_ELEMENT, "@").FromHexOrThis();
            }
        }

        /// <summary>
        /// Definiált üzenetnyugta
        /// </summary>
        public string Ack
        {
            get
            {
                return GetAttribute<string>(GetXElement(CONFIGURATION_ELEMENT_NAME), ACK_ATTRIBUTE_IN_CONFIGURATION_ELEMENT, "\x06").FromHexOrThis();
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
        private const string IP_ATTRIBUTE_IN_CONFIGURATION_ELEMENT = "IP";
        private const string PORT_ATTRIBUTE_IN_CONFIGURATION_ELEMENT = "Port";
        private const string MESSAGEPREFIX_ATTRIBUTE_IN_CONFIGURATION_ELEMENT = "MessagePrefix";
        private const string MESSAGESUFFIX_ATTRIBUTE_IN_CONFIGURATION_ELEMENT = "MessageSuffix";
        private const string IVIDSEPARATOR_ATTRIBUTE_IN_CONFIGURATION_ELEMENT = "IVIDSeparator";
        private const string PARAMETERSEPARATOR_ATTRIBUTE_IN_CONFIGURATION_ELEMENT = "ParameterSeparator";
        private const string ACK_ATTRIBUTE_IN_CONFIGURATION_ELEMENT = "Ack";
        private const string HANDLEDMESSAGES_ELEMENT_NAME = "HandledMessages";
        private const string USERGUID_ATTRIBUTE_IN_HANDLEDMESSAGES_ELEMENT = "UserGuid";        
        private const string MESSAGE_ELEMENT_NAME = "Message";
        private const string MESSAGEID_ATTRIBUTE_IN_MESSAGE_ELEMENT = "MessageId";
        private const string INTERVENTION_ATTRIBUTE_IN_MESSAGE_ELEMENT = "Intervention";

        #endregion Private Members
    }
}
