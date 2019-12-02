using System.Runtime.Serialization;

namespace Vrh.ApplicationContainer.Control.Contract
{
    /// <summary>
    /// A Plugin státusz állapota
    /// </summary>
    [DataContract]
    public enum PluginState
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
