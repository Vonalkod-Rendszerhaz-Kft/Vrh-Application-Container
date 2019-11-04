using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Vrh.ApplicationContainer
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
            State = PluginStateEnum.Unknown;
            ErrorInfo = null;
        }

        /// <summary>
        /// Státusz állapot
        /// </summary>
        [DataMember]
        public PluginStateEnum State { get; set; }

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

    /// <summary>
    /// A Plugin státusz állapota
    /// </summary>
    [DataContract]
    public enum PluginStateEnum
    {
        /// <summary>
        /// Ismeretlen
        /// </summary>
        [EnumMember]
        Unknown = 0,
        /// <summary>
        /// Plugin töltődik (példányosítás alatt)
        /// </summary>
        [EnumMember]
        Loading = 1,
        /// <summary>
        /// Betőltve/példányosítva
        /// </summary>
        [EnumMember]
        Loaded = 2,
        /// <summary>
        /// Fut (start lefutott)
        /// </summary>
        [EnumMember]
        Running = 3,
        /// <summary>
        /// Hibaállapot
        /// </summary>
        [EnumMember]
        Error = 4,
        /// <summary>
        /// Megsemítis (unload) alatt
        /// </summary>
        [EnumMember]
        Disposing = 5,
        /// <summary>
        /// Plugin instance megsemísítve (disposed)
        /// </summary>
        [EnumMember]
        Disposed = 6,
        /// <summary>
        /// Plugin indítás alatt
        /// </summary>
        [EnumMember]
        Starting = 7,
        /// <summary>
        /// Plugin leállítás alatt
        /// </summary>
        [EnumMember]
        Stopping = 8,
    }
}
