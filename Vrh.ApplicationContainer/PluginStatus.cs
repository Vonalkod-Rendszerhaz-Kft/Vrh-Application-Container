using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Vrh.ApplicationContainer
{
    [DataContract]
    public class PluginStatus
    {
        public PluginStatus()
        {
            State = PluginStateEnum.Unknown;
            ErrorInfo = null;
        }

        [DataMember]
        public PluginStateEnum State { get; set; }

        [DataMember]
        public string ErrorInfo { get; set; }

        [DataMember]
        public long CurrentSizeInByte { get; set; }
    }

    /// <summary>
    /// A Plugin státusz állapota
    /// </summary>
    [DataContract]
    public enum PluginStateEnum
    {
        [EnumMember]
        /// <summary>
        /// Ismeretlen
        /// </summary>
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
