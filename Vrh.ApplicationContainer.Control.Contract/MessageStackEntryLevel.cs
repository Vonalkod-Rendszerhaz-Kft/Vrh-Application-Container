using System.Runtime.Serialization;

namespace Vrh.ApplicationContainer.Control.Contract
{
    /// <summary>
    /// MessageStackEntry kategória
    /// </summary>
    [DataContract]
    public enum MessageStackEntryLevel
    {
        /// <summary>
        /// Ismeretlen / nem besorolt
        /// </summary>
        [EnumMember]
        Unknown = 0,
        /// <summary>
        /// Végzetes (működést gátoló) hiba
        /// </summary>
        [EnumMember]
        FatalError = 1,
        /// <summary>
        /// Hiba
        /// </summary>
        [EnumMember]
        Error = 2,
        /// <summary>
        /// Figyelmeztetés
        /// </summary>
        [EnumMember]
        Warning = 3,
        /// <summary>
        /// Információ
        /// </summary>
        [EnumMember]
        Info = 4,
    }
}
