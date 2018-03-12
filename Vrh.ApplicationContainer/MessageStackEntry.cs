using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Vrh.ApplicationContainer
{
    /// <summary>
    /// Egy esemény (információ, hiba) bejegyzése 
    /// </summary>
    [DataContract]
    public class MessageStackEntry
    {
        /// <summary>
        /// Esemény időbélyege
        /// </summary>
        [DataMember]
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Emberi olvasására szánt információ az eseményről
        /// </summary>
        [DataMember]
        public string Body { get; set; }

        /// <summary>
        /// Az eseményhez tartozó adatok kulcs-érték párok listájaként
        /// </summary>
        [DataMember]
        public Dictionary<string, string> Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public Level Type { get; set; }
    }

    /// <summary>
    /// MessageStackEntry kategória
    /// </summary>
    [DataContract]
    public enum Level
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        FatalError = 1,
        [EnumMember]
        Error = 2,
        [EnumMember]
        Warning = 3,
        [EnumMember]
        Info = 4,
    }
}
