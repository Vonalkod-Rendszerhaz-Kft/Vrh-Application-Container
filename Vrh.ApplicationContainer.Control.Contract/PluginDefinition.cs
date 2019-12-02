using System;
using System.Runtime.Serialization;

namespace Vrh.ApplicationContainer.Control.Contract
{
    /// <summary>
    /// A pluginok definicója
    /// </summary>
    [DataContract]
    public class PluginDefinition
    {
        /// <summary>
        /// Plugin leírása
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Típusnév
        /// </summary>
        [DataMember]
        public string TypeName { get; set; }

        /// <summary>
        /// Típus
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Verzió
        /// </summary>
        [DataMember]
        public string Version { get; set; }

        /// <summary>
        /// Automatikusan elindítja-e a konténer
        /// </summary>
        [DataMember]
        public bool AutoStart { get; set; }

        /// <summary>
        /// Singletone mintában töltődik
        /// </summary>
        [DataMember]
        public bool Singletone { get; set; }

        /// <summary>
        /// A plugin konfigurációjának a definiciója
        /// </summary>
        [DataMember]
        public string PluginConfig { get; set; }

        /// <summary>
        /// A plugin könyvtára
        /// </summary>
        [DataMember]
        public string PluginDirectory { get; set; }

        /// <summary>
        /// A .Net szerelvény, amely a plugint tartalmazza
        /// </summary>
        [DataMember]
        public string Assembly { get; set; }

        /// <summary>
        /// Ha a plugin implementál gyártófügvény mintát, akkor ebben van az információ a metódus nevéről
        /// </summary>
        [DataMember]
        public string FactoryMethodName { set; get; }

        /// <summary>
        /// Azt mutatja, hogy mennyi példány van betöltve jelenleg eből a pluginból az ApplicationContainer-be
        /// </summary>
        [DataMember]
        public int LoadedInstanceCount { set; get; }

        /// <summary>
        /// Azt mutatja, hogy mennyi példány van definiálva eből a pluginból az összeállítás konfigurációja szerint
        /// </summary>
        [DataMember]
        public int DefinedInstanceCount { set; get; }

        /// <summary>
        /// Azt mutatja, hogy be van-e betöltve eből  apluginból jelenleg példány 
        /// </summary>
        [DataMember]
        public bool Loaded { get; set; }

        /// <summary>
        /// Megmutatja, hogy a plugin jelenleg definiálva van-e az összeállítás konfigurációjában
        /// </summary>
        [DataMember]
        public bool NotDefined { get; set; }
    }    
}
