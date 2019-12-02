using System.Runtime.Serialization;

namespace Vrh.ApplicationContainer.Control.Contract
{
    /// <summary>
    /// Plugin státuszát leíró típus
    /// </summary>
    [DataContract]
    public class PluginStatus
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PluginStatus()
        {
            State = PluginState.Unknown;
            ErrorInfo = null;
        }

        /// <summary>
        /// Státusz állapot
        /// </summary>
        [DataMember]
        public PluginState State { get; set; }

        /// <summary>
        /// Hiba állapot információ
        /// </summary>
        [DataMember]
        public string ErrorInfo { get; set; }

        /// <summary>
        /// Plugin instance jelenelegi memoria foglalása (TODO: jelenleg még nincs mögötte implementáció!!!)
        /// </summary>
        [DataMember]
        public long CurrentSizeInByte { get; set; }
    }
}
