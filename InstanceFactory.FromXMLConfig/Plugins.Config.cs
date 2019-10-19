using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vrh.XmlProcessing;
using Vrh.ApplicationContainer;
using Vrh.Logger;

namespace InstanceFactory.FromXML
{
    /// <summary>
    /// Az XML-ben tárolt konfiguráció elérésére szopolgáló osztály
    /// </summary>
    class PluginsConfig : LinqXMLProcessorBase
    {
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="parameterFile">XML fájl aminek  afeldolgozásárta az osztály készül</param>
        public PluginsConfig(string parameterFile):base(parameterFile)
        {
            _xmlFileDefinition = parameterFile;
        }

        #region Retrive all information from XML

        //  Alapelvek:
        //      - Mindig csak olvasható property-ket használj getter megvalósítással, ha az adat visszanyeréséhez nem szükéges paraméter átadása!
        //      - Csak akkor használj függvényeket, ha a param,éterek átadására van szükség az informácoó visszanyeréséhez!
        //      - Mindig légy tipusos! Az alaposztály jelenlegi implemnetációja (V1.0.0) az alaposztálynak az alábbi típusokat kezeli: int, string, bool, Enumerátor (generikusan)! Ha típusbővítésere lenne szükséged, kérj fejlesztést rá (change request)! 
        //      - Bonyolultabb típusokat elemi feldolgozással építs! Soha ne használj XML alapú serializációt, amit depcreatednek tekintünk  akörnyezeteinkben!
        //      - A bonyolultabb típusok kódját ne helyezd el ebben a fájlban, hanem külső definiciókat használj!
        //      - Ismétlödő információk visszanyerésére (listák, felsorolások), generikus kollekciókat használj (Lis<T>, Dictonary<Tkey, TValue>, IENUMERABLE<T>, stb...) 

        /// <summary>
        /// A definiált pluginek listája
        /// </summary>
        public IEnumerable<PluginDefinition> Plugins
        {
            get
            {
                List<PluginDefinition> plugins = new List<PluginDefinition>();
                foreach (var item in GetAllXElements(PLUGINS_ELEMENT_NAME, PLUGIN_ELEMENT_NAME))
                {
                    PluginDefinition plugin = GetPluginInfo(item);
                    if (String.IsNullOrEmpty(plugin.TypeName))
                    {
                        Dictionary<string, string> data = new Dictionary<string, string>()
                        {
                            { "ConfigFile", this._xmlFileDefinition },
                        };
                        VrhLogger.Log<string>("Config Error! Plugin Type not defined!", data, null, LogLevel.Warning, this.GetType());
                        continue;
                    }
                    plugins.Add(plugin);
                }
                return plugins;
            }
        }

        /// <summary>
        /// Visszaadja a plugin példányokatokat
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<InstanceDefinition> GetInstances(string pluginType, string version)
        {
            List<InstanceDefinition> instances = new List<InstanceDefinition>();
            XElement pluginElement = GetAllXElements(PLUGINS_ELEMENT_NAME, PLUGIN_ELEMENT_NAME).FirstOrDefault(x =>
                                        x.Attribute(XName.Get(TYPE_ATTRIBUTE_IN_PLUGIN_ELEMENT, _xmlNameSpace)) != null
                                        && x.Attribute(XName.Get(TYPE_ATTRIBUTE_IN_PLUGIN_ELEMENT, _xmlNameSpace)).Value == pluginType
                                        && x.Attribute(XName.Get(VERSION_ATTRIBUTE_IN_PLUGIN_ELEMENT, _xmlNameSpace)) != null
                                        && x.Attribute(XName.Get(VERSION_ATTRIBUTE_IN_PLUGIN_ELEMENT, _xmlNameSpace)).Value == version
                                        );
            if (pluginElement != null)
            {
                PluginDefinition plugin = GetPluginInfo(pluginElement);
                foreach (var item in pluginElement.Descendants())
                {
                    InstanceDefinition instance = new InstanceDefinition();
                    instance.Id = GetAttribute(item, ID_ATTRIBUTE_IN_INSTANCE_ELEMENT, String.Empty);
                    instance.Name = GetAttribute(item, NAME_ATTRIBUTE_IN_INSTANCE_ELEMENT, String.Empty);
                    instance.Description = GetAttribute(item, DESCRIPTION_ATTRIBUTE_IN_INSTANCE_ELEMENT, String.Empty);
                    instance.InuseBy= GetAttribute(item, INUSE_ATTRIBUTE_IN_INSTANCE_ELEMENT, String.Empty);
                    instance.InstanceConfig = GetAttribute(item, INSTANCECONFIG_ATTRIBUTE_IN_INSTANCE_ELEMENT, String.Empty);
                    instance.InstanceData = GetAttribute(item, INSTANCEDATA_ATTRIBUTE_IN_INSTANCE_ELEMENT, String.Empty);
                    instance.Type = plugin;
                    if (String.IsNullOrEmpty(instance.Id))
                    {
                        Dictionary<string, string> data = new Dictionary<string, string>()
                        {
                            { "ConfigFile", this._xmlFileDefinition },
                        };
                        VrhLogger.Log<string>("Config Error! Instance Id not defined!", data, null, LogLevel.Warning, this.GetType());
                        continue;
                    }
                    instances.Add(instance);
                }
            }
            return instances;
        }

        /// <summary>
        /// A használt XML
        /// </summary>
        public string MyConfig
        {
            get
            {
                return _xmlFileDefinition;
            }
        }

        /// <summary>
        /// Visszadja mekkora stack méreteket kell használni a plugin üzeneteinek tárolására
        /// </summary>
        public ushort StackSize
        {
            get
            {
                return GetAttribute<ushort>(GetXElement(PLUGINS_ELEMENT_NAME), STACKSIZE_ATTRIBUTE_IN_PLUGINS_ELEMENT, 50);
            }
        }

        /// <summary>
        /// Visszadaja a paraméterben kapott plugin Node-ban tárolt információkhoz tartozó PluginDefinition objektumot
        /// </summary>
        /// <param name="pluginNode">a plugin XML node-ja</param>
        /// <returns></returns>
        private PluginDefinition GetPluginInfo(XElement pluginNode)
        {
            PluginDefinition plugin = new PluginDefinition();
            plugin.TypeName = GetAttribute(pluginNode, TYPE_ATTRIBUTE_IN_PLUGIN_ELEMENT, String.Empty);
            plugin.Version = GetAttribute(pluginNode, VERSION_ATTRIBUTE_IN_PLUGIN_ELEMENT, String.Empty);
            plugin.Singletone = GetAttribute(pluginNode, SINGLETONE_ATTRIBUTE_IN_PLUGIN_ELEMENT, false);
            plugin.AutoStart = GetAttribute(pluginNode, AUTOSTART_ATTRIBUTE_IN_PLUGIN_ELEMENT, true);
            plugin.PluginConfig = GetAttribute(pluginNode, PLUGINCONFIG_ATTRIBUTE_IN_PLUGIN_ELEMENT, String.Empty);
            plugin.PluginDirectory = GetAttribute(pluginNode, PLUGINDIRECTORY_ATTRIBUTE_IN_PLUGIN_ELEMENT, String.Empty);
            plugin.Assembly = GetAttribute(pluginNode, ASSEMBLY_ATTRIBUTE_IN_PLUGIN_ELEMENT, String.Empty);
            plugin.FactoryMethodName = GetAttribute(pluginNode, FACTORY_ATTRIBUTE_IN_PLUGIN_ELEMENT, String.Empty);
            plugin.Description = GetAttribute(pluginNode, DESCRIPTION_ATTRIBUTE_IN_PLUGIN_ELEMENT, String.Empty);
            if (String.IsNullOrEmpty(plugin.Assembly))
            {
                plugin.Assembly = plugin.TypeName + ".dll";
            }
            return plugin;
        }

        #endregion

        #region Defination of namming rules in XML
        // A szabályok:
        //  - Mindig konstansokat használj, hogy az element és az atribútum neveket azon át hivatkozzd! 
        //  - Az elnevezések feleljenek meg  akonstansokra vonatkozó elnevetési szabályoknak!
        //  - Az Attribútumok neveiben mindig jelöld, mely elem alatt fordul elő.
        //  - Az elemekre ne használj, ilyet, mert az elnevezések a hierarchia mélyén túl összetetté (hosszúvá) válnának! Ez alól kivétel, ha nem egysértelmű az elnevezés enélkül.
        // 
        private const string PLUGINS_ELEMENT_NAME = "Plugins";
        private const string STACKSIZE_ATTRIBUTE_IN_PLUGINS_ELEMENT = "StackSize";
        private const string PLUGIN_ELEMENT_NAME = "Plugin";
        private const string TYPE_ATTRIBUTE_IN_PLUGIN_ELEMENT = "Type";
        private const string VERSION_ATTRIBUTE_IN_PLUGIN_ELEMENT = "Version";
        private const string SINGLETONE_ATTRIBUTE_IN_PLUGIN_ELEMENT = "Singletone";
        private const string AUTOSTART_ATTRIBUTE_IN_PLUGIN_ELEMENT = "AutoStart";
        private const string PLUGINCONFIG_ATTRIBUTE_IN_PLUGIN_ELEMENT = "PluginConfig";
        private const string PLUGINDIRECTORY_ATTRIBUTE_IN_PLUGIN_ELEMENT = "PluginDirectory";
        private const string ASSEMBLY_ATTRIBUTE_IN_PLUGIN_ELEMENT = "Assembly";
        private const string FACTORY_ATTRIBUTE_IN_PLUGIN_ELEMENT = "Factory";
        private const string DESCRIPTION_ATTRIBUTE_IN_PLUGIN_ELEMENT = "Description";
        private const string INSTANCE_ELEMENT_NAME = "Instance";
        private const string ID_ATTRIBUTE_IN_INSTANCE_ELEMENT = "Id";
        private const string NAME_ATTRIBUTE_IN_INSTANCE_ELEMENT = "Name";
        private const string DESCRIPTION_ATTRIBUTE_IN_INSTANCE_ELEMENT = "Description";
        private const string INUSE_ATTRIBUTE_IN_INSTANCE_ELEMENT = "Inuse";
        private const string INSTANCECONFIG_ATTRIBUTE_IN_INSTANCE_ELEMENT = "InstanceConfig";
        private const string INSTANCEDATA_ATTRIBUTE_IN_INSTANCE_ELEMENT = "InstanceData";

        #endregion
    }
}
