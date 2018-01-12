using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vrh.LinqXMLProcessor.Base;

namespace IVConnector.Plugin
{
    internal class MessageDefinitionsParameterFileProcessor : LinqXMLProcessorBaseClass
    {
        #region Public Members

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="parameterFile">paraméter fájl és fájlon belüli útvonal a root TAG-ig</param>
        public MessageDefinitionsParameterFileProcessor(string parameterFile)
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
        /// Visszadja az üzenet definiciót
        /// </summary>
        /// <param name="messageId">üzenet azonosítója</param>
        /// <returns>üzenet definició</returns>
        public DefinedMessage GetMessage(string messageId)
        {
            DefinedMessage message = new DefinedMessage();
            foreach (var msg in GetAllXElements(MESSAGE_ELEMENT_NAME))
            {
                string msgIvId = GetAttribute<string>(msg, IVID_ATTRIBUTE_IN_MESSGAE_ELEMENT, String.Empty);
                string intervention = GetAttribute<string>(msg, INTERVENTION_ATTRIBUTE_IN_MESSGAE_ELEMENT, String.Empty);
                if (msgIvId.ToLower() == messageId.ToLower())
                {
                    message.IvId = msgIvId;
                    message.Intervention = !String.IsNullOrEmpty(intervention) ? intervention : msgIvId;
                    message.Fields = new List<DefinedField>();
                    
                    foreach (var field in msg.Elements(XName.Get(MSGFIELD_ELEMENT_NAME, _xmlNameSpace)))
                    {
                        string regExp = GetElementValue<string>(field, String.Empty);
                        string fieldName = GetAttribute<string>(field, NAME_ATTRIBUTE_IN_MSGFIELD_ELEMENT, String.Empty);
                        string intervemntionParameterName = GetAttribute<string>(field, INTERVENTIONPARAMETER_ATTRIBUTE_IN_MSGFIELD_ELEMENT, String.Empty);
                        if (!String.IsNullOrEmpty(fieldName))
                        {
                            var fieldDefinition = 
                                new DefinedField()
                                {
                                    CheckerRegExp = regExp,
                                    Name = fieldName,
                                    InterventionParameter = !String.IsNullOrEmpty(intervemntionParameterName) 
                                                                ? intervemntionParameterName 
                                                                : fieldName,
                                };
                            message.Fields.Add(fieldDefinition);
                        }
                    }
                    return message;
                }
            }
            return null;
        }

        #endregion Public Members

        #region Private Members

        private const string MESSAGE_ELEMENT_NAME = "Message";
        private const string IVID_ATTRIBUTE_IN_MESSGAE_ELEMENT = "IVID";
        private const string INTERVENTION_ATTRIBUTE_IN_MESSGAE_ELEMENT = "Intervention";
        private const string MSGFIELD_ELEMENT_NAME = "MsgField";
        private const string NAME_ATTRIBUTE_IN_MSGFIELD_ELEMENT = "Name";
        private const string INTERVENTIONPARAMETER_ATTRIBUTE_IN_MSGFIELD_ELEMENT = "InterventionParameter";
            
        #endregion Private Members
    }

    /// <summary>
    /// Definiált üzenetek, melyeket feldolgoz az IV connector
    /// </summary>
    internal class DefinedMessage
    {
        /// <summary>
        /// Üzenet azonosító
        /// </summary>
        public string IvId { get; set; }

        /// <summary>
        /// A hívandó beavatkozás
        /// </summary>
        public string Intervention { get; set; }

        /// <summary>
        /// Üzenet mezői
        /// </summary>
        public List<DefinedField> Fields { get; set; }
    }

    /// <summary>
    /// Definiált mező adatai
    /// </summary>
    internal class DefinedField
    {
        /// <summary>
        /// Mező neve
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ebbe a bevatkozás paraméterbe kerül ez az adat
        /// </summary>
        public string InterventionParameter { get; set; }

        /// <summary>
        /// Ellenőrző kifejezés
        /// </summary>
        public string CheckerRegExp { get; set; }
    }
}
