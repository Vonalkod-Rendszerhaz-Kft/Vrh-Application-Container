using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Threading;

namespace Vrh.ApplicationContainer
{
    /// <summary>
    /// A plugin példány adatai
    /// </summary>
    [DataContract]
    public class InstanceDefinition
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public InstanceDefinition()
        {
            InternalId = Guid.NewGuid();
        }

        /// <summary>
        /// Belső azonosító (grantáltan egyedi globálisan, a példány létrejöttekor generálódik)
        /// </summary>
        [DataMember]
        public Guid InternalId { private set; get; }
        /// <summary>
        /// Példány azonosító. Típuson belül egyedi!
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Példány informatív neve
        /// </summary>
        [DataMember]
        public string Name { set; get; }

        /// <summary>
        /// Példánnyal kapcsolatos bővebb információ
        /// </summary>
        [DataMember]
        public string Description { set; get; }

        /// <summary>
        /// A példányt betöltő szerviz által megadott Inuse név
        /// </summary>
        [DataMember]
        public string InuseBy { set; get; }


        /// <summary>
        /// Példány konfigurációja
        /// </summary>
        [DataMember]
        public string InstanceConfig { set; get; }

        /// <summary>
        /// A típus (plugin) leírója
        /// </summary>
        [DataMember]
        public PluginDefinition Type { set; get; }

        /// <summary>
        /// Példány adatai ezt lehet felhasználni, hogy típusos paramétert kapjon a plugin gyártófüggvénye
        /// </summary>        
        public Object InstanceData { set; get; }

        /// <summary>
        /// MaunualResetEvent a státusz változás esemény bekövetkeztének a jelzésére
        /// </summary>
        public ManualResetEvent WaitForPluginStateChangeEvent = new ManualResetEvent(false);

        /// <summary>
        /// Az ApplicationContainer keret arra vár, hogy a plugin ebbe az állapotba kerüljön
        /// </summary>
        public PluginStateEnum WaitForThisState = PluginStateEnum.Unknown;

        /// <summary>
        /// Utolsó ismert töltési idő
        /// </summary>
        [DataMember]
        public double LastKnownLoadTimeCost { get; set; }

        /// <summary>
        /// Utolsó ismert indítási idő
        /// </summary>
        [DataMember]
        public double LastKnownStartTimeCost { get; set; }

        /// <summary>
        /// Utolsó ismert leállítási idő
        /// </summary>
        [DataMember]
        public double LastKnownStopTimeCost { get; set; }

        /// <summary>
        /// Utolsó ismert megsemisítési idő
        /// </summary>
        [DataMember]
        public double LastKnownDisposeTimeCost { get; set; }

        /// <summary>
        /// Utolsó ismert töltési idő
        /// </summary>
        [DataMember]
        public DateTime? LastKnownLoadTimeStamp { get; set; }

        /// <summary>
        /// Utolsó ismert indítási idő
        /// </summary>
        [DataMember]
        public DateTime? LastKnownStartTimeStamp { get; set; }

        /// <summary>
        /// Utolsó ismert leállítási idő
        /// </summary>
        [DataMember]
        public DateTime? LastKnownStopTimeStamp { get; set; }

        /// <summary>
        /// Utolsó ismert megsemisítési idő
        /// </summary>
        [DataMember]
        public DateTime? LastKnownDisposeTimeStamp { get; set; }
    }
}
